using PppLib.Checksums;
using System;
using System.Linq;

namespace PppLib.Protocols
{
    public class IpPacket : IPppDataPacket
    {
        public byte[] PacketBytes { get; private set; }

        public byte Version => (byte)(PacketBytes[0] >> 4);
        public byte HeaderLength => (byte)(PacketBytes[0] & 0x0f);
        public byte ServiceType => PacketBytes[1];
        public ushort TotalLength => (ushort)(PacketBytes[2] * 256 + PacketBytes[3]);
        public ushort Id => (ushort)(PacketBytes[4] * 256 + PacketBytes[5]);
        public byte Ttl => PacketBytes[8];
        public byte Protocol => PacketBytes[9];
        public ushort HeaderChecksum => (ushort)(PacketBytes[10] * 256 + PacketBytes[11]);
        public byte[] SourceAddress => PacketBytes.Skip(12).Take(4).ToArray();
        public byte[] DestinationAddress => PacketBytes.Skip(16).Take(4).ToArray();
        public byte[] Data => PacketBytes.Skip(20).ToArray();

        // Technically this is TCP down here
        public ushort SourcePort => (ushort)(PacketBytes[20] * 256 + PacketBytes[21]);
        public ushort DestinationPort => (ushort)(PacketBytes[22] * 256 + PacketBytes[23]);
        public uint SequenceNumber => (uint)(PacketBytes[24] * 0x1000000 + PacketBytes[25] * 0x10000 + PacketBytes[26] * 0x100 + PacketBytes[27]);
        public uint AcknowledgementNumber => (uint)(PacketBytes[28] * 0x1000000 + PacketBytes[29] * 0x10000 + PacketBytes[30] * 0x100 + PacketBytes[31]);
        public uint DataOffset => (byte)(PacketBytes[32] >> 4);
        public TcpFlags Flags => (TcpFlags)((PacketBytes[32] * 256 + PacketBytes[33]) * 0x1ff);
        public ushort WindowSize => (ushort)(PacketBytes[34] * 256 + PacketBytes[35]);
        public ushort TcpChecksum => (ushort)(PacketBytes[36] * 256 + PacketBytes[37]);
        public ushort UrgentPointer => (ushort)(PacketBytes[38] * 256 + PacketBytes[39]);
        public byte[] TcpData => PacketBytes.Skip(20 + (int)DataOffset * 4).ToArray();

        [Flags]
        public enum TcpFlags : ushort
        {
            NS = 0x100,
            CWR = 0x80,
            ECE = 0x40,
            URG = 0x20,
            ACK = 0x10,
            PSH = 0x08,
            RST = 0x04,
            SYN = 0x02,
            FIN = 0x01,
        }

        public IpPacket(
            ushort id,
            byte protocol,
            byte[] sourceAddress,
            byte[] destinationAddress,
            ushort sourcePort,
            ushort destinationPort,
            uint sequenceNumber,
            uint acknowledgementNumber, byte[] data)
        {
            if (sourceAddress.Length != 4 || destinationAddress.Length != 4)
            {
                throw new ArgumentException("Invalid address length");
            }

            byte[] ipHeader = new byte[20];
            ipHeader[0] = 0x45;
            ipHeader[1] = 0x00;
            int totalLength = 20 + 20 + data.Length;
            ipHeader[2] = (byte)(totalLength >> 8);
            ipHeader[3] = (byte)(totalLength);
            ipHeader[4] = (byte)(id >> 8);
            ipHeader[5] = (byte)(id);
            ipHeader[8] = 0x40;
            ipHeader[9] = 6; // TCP
            Array.Copy(sourceAddress, 0, ipHeader, 12, 4);
            Array.Copy(destinationAddress, 0, ipHeader, 16, 4);

            byte[] tcpHeader = new byte[20];
            tcpHeader[0] = (byte)(sourcePort >> 8);
            tcpHeader[1] = (byte)(sourcePort);
            tcpHeader[2] = (byte)(destinationPort >> 8);
            tcpHeader[3] = (byte)(destinationPort);
            tcpHeader[4] = (byte)(sequenceNumber >> 24);
            tcpHeader[5] = (byte)(sequenceNumber >> 16);
            tcpHeader[6] = (byte)(sequenceNumber >> 8);
            tcpHeader[7] = (byte)(sequenceNumber);
            tcpHeader[8] = (byte)(acknowledgementNumber >> 24);
            tcpHeader[9] = (byte)(acknowledgementNumber >> 16);
            tcpHeader[10] = (byte)(acknowledgementNumber >> 8);
            tcpHeader[11] = (byte)(acknowledgementNumber);
            tcpHeader[12] = 5 << 4;
            tcpHeader[13] = 0x10;
            tcpHeader[14] = 0x16;
            tcpHeader[15] = 0xd0;

            PacketBytes = new byte[ipHeader.Length + tcpHeader.Length + data.Length];
            Array.Copy(ipHeader, 0, PacketBytes, 0, ipHeader.Length);
            Array.Copy(tcpHeader, 0, PacketBytes, ipHeader.Length, tcpHeader.Length);
            Array.Copy(data, 0, PacketBytes, ipHeader.Length + tcpHeader.Length, data.Length);

            ushort ipChecksum = IpTcpChecksumGenerator.GenerateIp(this);
            ushort tcpChecksum = IpTcpChecksumGenerator.GenerateTcp(this);
            PacketBytes[10] = (byte)(ipChecksum >> 8);
            PacketBytes[11] = (byte)(ipChecksum);
            PacketBytes[36] = (byte)(tcpChecksum >> 8);
            PacketBytes[37] = (byte)(tcpChecksum);

            // VJ compressed frame overrides protocol using slot ID
            if (protocol <= 5)
            {
                PacketBytes[9] = protocol;
            }
        }

        public IpPacket(byte[] packet)
        {
            PacketBytes = packet;
        }

        public static IpPacket FromPppFrame(PppFrame frame)
        {
            if (frame.Protocol != PppFrame.PppProtocol.Ip)
            {
                throw new ArgumentException("Incorrect protocol");
            }

            return new IpPacket(frame.Data);
        }

        public IpPacket WithZeroChecksums()
        {
            byte[] bytes = (byte[])PacketBytes.Clone();

            // IP header checksum
            bytes[10] = 0;
            bytes[11] = 0;

            // IP header checksum
            bytes[36] = 0;
            bytes[37] = 0;

            return new IpPacket(bytes);
        }
    }
}

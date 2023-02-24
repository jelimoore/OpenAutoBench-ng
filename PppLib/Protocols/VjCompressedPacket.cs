using PppLib.Checksums;
using System;
using System.Linq;

namespace PppLib.Protocols
{
    public class VjCompressedPacket : IPppDataPacket
    {
        public byte[] PacketBytes { get; set; }

        [Flags]
        public enum VjCompressedHeaderOptions : byte
        {
            None = 0x00,
            UrgentPointer = 0x01,
            DeltaWindow = 0x02,
            DeltaAck = 0x04,
            DeltaSequence = 0x08,
            DeltaPush = 0x10,
            DeltaIp = 0x20,
            ConnectionNumber = 0x40,
        }

        public VjCompressedPacket(
            VjUncompressedPacket basePacket,
            VjCompressedHeaderOptions headerOptions,
            byte[] data)
        {
            byte[] uncompressedBytes = new byte[basePacket.PacketBytes.Length + data.Length];
            Array.Copy(basePacket.PacketBytes, 0, uncompressedBytes, 0, basePacket.PacketBytes.Length);
            Array.Copy(data, 0, uncompressedBytes, basePacket.PacketBytes.Length, data.Length);
            uncompressedBytes[9] = 6; // Reset protocol to TCP

            IpPacket packet = new IpPacket(uncompressedBytes);
            ushort tcpChecksum = IpTcpChecksumGenerator.GenerateTcp(packet);

            PacketBytes = new byte[3 + data.Length];
            PacketBytes[0] = (byte)headerOptions;
            PacketBytes[1] = (byte)(tcpChecksum >> 8);
            PacketBytes[2] = (byte)(tcpChecksum);
            Array.Copy(data, 0, PacketBytes, 3, data.Length);
        }

        private VjCompressedPacket(byte[] packet)
        {
            PacketBytes = packet;
        }

        public static VjCompressedPacket FromPppFrame(PppFrame frame)
        {
            if (frame.Protocol != PppFrame.PppProtocol.VanJacobsonCompressedTcpIp)
            {
                throw new ArgumentException("Incorrect protocol");
            }

            return new VjCompressedPacket(frame.Data);
        }
    }
}

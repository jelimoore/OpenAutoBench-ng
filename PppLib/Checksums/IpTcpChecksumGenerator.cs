using System;
using System.Linq;
using System.Net;

namespace PppLib.Checksums
{
    internal static class IpTcpChecksumGenerator
    {
        public static ushort GenerateIp(Protocols.IpPacket packet)
        {
            packet = packet.WithZeroChecksums();
            if (packet.HeaderChecksum != 0)
            {
                throw new Exception("Packet checksums were not zeroed correctly");
            }

            return Generate(packet.PacketBytes.Take(20).ToArray());
        }

        public static ushort GenerateTcp(Protocols.IpPacket packet)
        {
            packet = packet.WithZeroChecksums();
            if (packet.TcpChecksum != 0)
            {
                throw new Exception("Packet checksums were not zeroed correctly");
            }

            byte[] pseudoHeader = new byte[12];
            Array.Copy(packet.SourceAddress, 0, pseudoHeader, 0, 4);
            Array.Copy(packet.DestinationAddress, 0, pseudoHeader, 4, 4);
            pseudoHeader[8] = 0;
            pseudoHeader[9] = packet.Protocol;
            pseudoHeader[10] = (byte)(packet.Data.Length >> 8);
            pseudoHeader[11] = (byte)(packet.Data.Length);

            byte[] tcpFull = new byte[pseudoHeader.Length + packet.Data.Length];
            Array.Copy(pseudoHeader, 0, tcpFull, 0, pseudoHeader.Length);
            Array.Copy(packet.Data, 0, tcpFull, pseudoHeader.Length, packet.Data.Length);

            return Generate(tcpFull);
        }

        private static ushort Generate(byte[] data)
        {
            uint checksum = 0;

            for (int i = 0; i < data.Length - 1; i += 2)
            {
                checksum += data[i] * 256u + data[i + 1];
            }

            if (data.Length % 2 == 1)
            {
                checksum += data[data.Length - 1] * 256u;
            }

            while (checksum >> 16 != 0)
            {
                checksum = (checksum & 0xffff) + (checksum >> 16);
            }

            return (ushort)(checksum ^ 0xffff);
        }
    }
}

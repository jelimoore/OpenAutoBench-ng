using System;

namespace PppLib.Protocols
{
    public class VjUncompressedPacket : IpPacket
    {
        public VjUncompressedPacket(
            ushort id,
            byte protocol,
            byte[] sourceAddress,
            byte[] destinationAddress,
            ushort sourcePort,
            ushort destinationPort,
            uint sequenceNumber,
            uint acknowledgementNumber,
            byte[] data)
            : base(id, protocol, sourceAddress, destinationAddress, sourcePort, destinationPort, sequenceNumber, acknowledgementNumber, data)
        { }

        private VjUncompressedPacket(byte[] packet) : base(packet)
        { }

        public static new VjUncompressedPacket FromPppFrame(PppFrame frame)
        {
            if (frame.Protocol != PppFrame.PppProtocol.VanJacobsonUncompressedTcpIp)
            {
                throw new ArgumentException("Incorrect protocol");
            }

            return new VjUncompressedPacket(frame.Data);
        }
    }
}

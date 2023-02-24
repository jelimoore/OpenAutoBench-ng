using System;
using System.Collections.Generic;
using System.Linq;
using static PppLib.Protocols.LcpPacket;
using static PppLib.Protocols.LcpPacket.LcpOption;

namespace PppLib.Protocols
{
    public sealed class IpcpPacket : IPppDataPacket
    {
        public byte[] PacketBytes { get; private set; }

        public IpcpCode Code => (IpcpCode)PacketBytes[0];
        public byte Id => PacketBytes[1];
        public ushort Length => (ushort)(PacketBytes[2] * 256 + PacketBytes[3]);
        public byte[] Data => PacketBytes.Skip(4).ToArray();
        public IpcpOption[] Options => IpcpOption.FromIpcpPacket(this);

        public enum IpcpCode : byte
        {
            ConfigureRequest = 0x01,
            ConfigureAck = 0x02,
            ConfigureNack = 0x03,
            ConfigureReject = 0x04,
            TerminateRequest = 0x05,
            TerminateAck = 0x06,
            CodeReject = 0x07,
        }

        public class IpcpOption
        {
            private byte[] Bytes { get; set; }

            public IpcpOptionType Type => (IpcpOptionType)Bytes[0];
            public byte Length => Bytes[1];
            public byte[] Data => Bytes.Skip(2).ToArray();

            public enum IpcpOptionType : byte
            {
                IpAddresses = 0x01,
                IpCompressionProtocol = 0x02,
                IpAddress = 0x03,
            }

            public IpcpOption(IpcpOptionType type, byte[]? data = null)
            {
                List<byte> bytes = new List<byte>();
                bytes.Add((byte)type);
                bytes.Add(0); // Placeholder for length
                if (data != null)
                {
                    bytes.AddRange(data);
                }

                Bytes = bytes.ToArray();
                Bytes[1] = (byte)Bytes.Length;
            }

            private IpcpOption(byte[] option)
            {
                Bytes = option;
            }

            public static IpcpOption[] FromIpcpPacket(IpcpPacket packet)
            {
                List<IpcpOption> results = new List<IpcpOption>();

                int startIndex = 0;
                while (startIndex < packet.Data.Length)
                {
                    byte length = packet.Data[startIndex + 1];
                    if (startIndex + length > packet.Data.Length)
                    {
                        throw new ArgumentException("Overran end of LCP packet data looking for options");
                    }
                    results.Add(new IpcpOption(packet.Data.Skip(startIndex).Take(length).ToArray()));
                    startIndex += length;
                }

                if (startIndex != packet.Data.Length)
                {
                    throw new ArgumentException("LCP options length is malformed");
                }

                return results.ToArray();
            }
        }

        public IpcpPacket(IpcpCode code, byte id, params IpcpOption[] options)
        {
            List<byte> packet = new List<byte>();
            packet.Add((byte)code);
            packet.Add(id);
            packet.AddRange(new byte[2]); // Placeholder for length
            foreach (IpcpOption option in options)
            {
                byte[] optionBytes = new byte[option.Length];

                optionBytes[0] = (byte)option.Type;
                optionBytes[1] = option.Length;
                Array.Copy(option.Data, 0, optionBytes, 2, option.Data.Length);

                packet.AddRange(optionBytes);
            }

            // Fill in length
            packet[2] = (byte)(packet.Count >> 8);
            packet[3] = (byte)(packet.Count);

            PacketBytes = packet.ToArray();
        }

        private IpcpPacket(byte[] packet)
        {
            PacketBytes = packet;
        }

        public static IpcpPacket FromPppFrame(PppFrame frame)
        {
            if (frame.Protocol != PppFrame.PppProtocol.Ipcp)
            {
                throw new ArgumentException("Incorrect protocol");
            }

            return new IpcpPacket(frame.Data);
        }
    }
}

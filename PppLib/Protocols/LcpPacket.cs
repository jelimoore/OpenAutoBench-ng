using System;
using System.Collections.Generic;
using System.Linq;

namespace PppLib.Protocols
{
    public sealed class LcpPacket : IPppDataPacket
    {
        public byte[] PacketBytes { get; private set; }

        public LcpCode Code => (LcpCode)PacketBytes[0];
        public byte Id => PacketBytes[1];
        public ushort Length => (ushort)(PacketBytes[2] * 256 + PacketBytes[3]);
        public byte[] Data => PacketBytes.Skip(4).ToArray();
        public LcpOption[] Options => LcpOption.FromLcpPacket(this);

        public enum LcpCode : byte
        {
            ConfigureRequest = 0x01,
            ConfigureAck = 0x02,
            ConfigureNack = 0x03,
            ConfigureReject = 0x04,
            TerminateRequest = 0x05,
            TerminateAck = 0x06,
            CodeReject = 0x07,
            ProtocolReject = 0x08,
            EchoRequest = 0x09,
            EchoReply = 0x0a,
            DiscardRequest = 0x0b,
        }

        public class LcpOption
        {
            private byte[] Bytes { get; set; }

            public LcpOptionType Type => (LcpOptionType)Bytes[0];
            public byte Length => Bytes[1];
            public byte[] Data => Bytes.Skip(2).ToArray();

            public enum LcpOptionType : byte
            {
                MaximumReceiveUnit = 0x01,
                AsyncControlCharacterMap = 0x02,
                AuthenticationProtocol = 0x03,
                QualityProtocol = 0x04,
                MagicNumber = 0x05,
                ProtocolFieldCompression = 0x07,
                AddressControlFieldCompression = 0x08,
                FcsAlternative = 0x09,
            }

            public LcpOption(LcpOptionType type, byte[]? data = null)
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

            private LcpOption(byte[] option)
            {
                Bytes = option;
            }

            public static LcpOption[] FromLcpPacket(LcpPacket packet)
            {
                List<LcpOption> results = new List<LcpOption>();

                int startIndex = 0;
                while (startIndex < packet.Data.Length)
                {
                    byte length = packet.Data[startIndex + 1];
                    if (startIndex + length > packet.Data.Length)
                    {
                        throw new ArgumentException("Overran end of LCP packet data looking for options");
                    }
                    results.Add(new LcpOption(packet.Data.Skip(startIndex).Take(length).ToArray()));
                    startIndex += length;
                }

                if (startIndex != packet.Data.Length)
                {
                    throw new ArgumentException("LCP options length is malformed");
                }

                return results.ToArray();
            }
        }

        public LcpPacket(LcpCode code, byte id, params LcpOption[] options)
        {
            List<byte> packet = new List<byte>();
            packet.Add((byte)code);
            packet.Add(id);
            packet.AddRange(new byte[2]); // Placeholder for length
            foreach (LcpOption option in options)
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

        private LcpPacket(byte[] packet)
        {
            PacketBytes = packet;
        }

        public static LcpPacket FromPppFrame(PppFrame frame)
        {
            if (frame.Protocol != PppFrame.PppProtocol.Lcp)
            {
                throw new ArgumentException("Incorrect protocol");
            }

            return new LcpPacket(frame.Data);
        }
    }
}

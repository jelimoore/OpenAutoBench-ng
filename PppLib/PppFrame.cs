using System;
using System.Collections.Generic;
using System.Linq;
using PppLib.Checksums;
using PppLib.Protocols;

namespace PppLib
{
    public class PppFrame
    {
        private byte[] FrameBytes { get; set; }
        private int ProtocolIndex => AddressControlCompression ? 1 : 3;
        private int ProtocolLength => ProtocolCompression && (FrameBytes[ProtocolIndex] % 2 == 1) ? 1 : 2;
        private int AddressControlLength => AddressControlCompression ? 0 : 2;

        static public bool ProtocolCompression { get; set; }
        static public bool AddressControlCompression { get; set; }

        public byte Address => AddressControlCompression ? (byte)0xff : FrameBytes[1];
        public byte Control => AddressControlCompression ? (byte)0x03 : FrameBytes[2];
        public PppProtocol Protocol => ProtocolLength == 1 ?
            (PppProtocol)FrameBytes[ProtocolIndex] :
            (PppProtocol)(FrameBytes[ProtocolIndex] * 256 + FrameBytes[ProtocolIndex + 1]);
        public byte[] Data => FrameBytes.Skip(1 + AddressControlLength + ProtocolLength).SkipLast(3).ToArray();
        public IPppDataPacket DataPacket
        {
            get
            {
                switch (Protocol)
                {
                    case PppProtocol.Ip:
                        return IpPacket.FromPppFrame(this);
                    case PppProtocol.Ipcp:
                        return IpcpPacket.FromPppFrame(this);
                    case PppProtocol.Lcp:
                        return LcpPacket.FromPppFrame(this);
                    case PppProtocol.VanJacobsonCompressedTcpIp:
                        return VjCompressedPacket.FromPppFrame(this);
                    case PppProtocol.VanJacobsonUncompressedTcpIp:
                        return VjUncompressedPacket.FromPppFrame(this);
                    default:
                        throw new ArgumentException("Unknown protocol");
                };
            }
        }
        public ushort FrameCheckSequence => (ushort)(FrameBytes[FrameBytes.Length - 3] * 256 + FrameBytes[FrameBytes.Length - 2]);

        public enum PppProtocol : ushort
        {
            Ip = 0x0021,
            VanJacobsonCompressedTcpIp = 0x002d,
            VanJacobsonUncompressedTcpIp = 0x002f,
            CompressedDatagram = 0x00fd,
            Ipcp = 0x8021,
            Ccp = 0x80fd,
            Lcp = 0xc021,
            Pap = 0xc023,
            LinkQualityReport = 0xc025,
            Chap = 0xc223,
        }

        public PppFrame(PppProtocol protocol, IPppDataPacket dataPacket)
        {
            List<byte> bytes = new List<byte>();
            if (!AddressControlCompression)
            {
                bytes.Add(0xff);
                bytes.Add(0x03);
            }
            if (ProtocolCompression && (PppProtocol)((byte)protocol) == protocol)
            {
                bytes.Add((byte)((ushort)protocol));
            }
            else
            {
                bytes.Add((byte)(((ushort)protocol) >> 8));
                bytes.Add((byte)((ushort)protocol));
            }
            bytes.AddRange(dataPacket.PacketBytes);

            ushort fcs = FcsGenerator.Generate(bytes.ToArray());
            bytes.Add((byte)(fcs >> 8));
            bytes.Add((byte)(fcs));

            FrameBytes = new byte[bytes.Count + 2];
            FrameBytes[0] = 0x7e;
            Array.Copy(bytes.ToArray(), 0, FrameBytes, 1, bytes.Count);
            FrameBytes[FrameBytes.Length - 1] = 0x7e;
        }

        private PppFrame(byte[] frameBytes)
        {
            FrameBytes = frameBytes;
        }

        public static PppFrame FromRawBytes(byte[] frameBytes)
        {
            if (frameBytes[0] != 0x7e)
            {
                throw new ArgumentException("Missing start flag");
            }

            int endIndex = -1;
            for (int i = 1; i < frameBytes.Length; i++)
            {
                if (frameBytes[i] == 0x7e)
                {
                    endIndex = i;
                    break;
                }
            }

            if (endIndex == -1)
            {
                throw new ArgumentException("Missing end flag");
            }
            if (endIndex != frameBytes.Length - 1)
            {
                throw new ArgumentException("Multiple frames detected");
            }

            return new PppFrame(OctetStuffer.Unstuff(frameBytes));
        }

        public static PppFrame[] MultipleFromRawBytes(byte[] rawBytes)
        {
            if (rawBytes[0] != 0x7e)
            {
                throw new ArgumentException("Missing start flag");
            }
            if (rawBytes[rawBytes.Length - 1] != 0x7e)
            {
                throw new ArgumentException("Bytes do not end in end flag");
            }

            List<PppFrame> results = new List<PppFrame>();
            int startIndex = 0;
            for (int i = startIndex + 1; i < rawBytes.Length; i++)
            {
                if (rawBytes[i] == 0x7e)
                {
                    results.Add(FromRawBytes(rawBytes.Skip(startIndex).Take(i - startIndex + 1).ToArray()));
                    startIndex = i + 1;
                    i = startIndex + 1;
                }
            }

            return results.ToArray();
        }

        public byte[] ToStuffedBytes()
        {
            return OctetStuffer.Stuff(FrameBytes);
        }

        public byte[] ToUnstuffedBytes()
        {
            return FrameBytes;
        }
    }
}

using PppLib.Checksums;
using PppLib.Protocols;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace PppLib
{
    public class SerialIo : IDisposable
    {
        private SerialPort Port { get; set; }

        private byte[] m_receiveBuffer;
        //private Dictionary<int, VjUncompressedPacket> m_vjConnections;
        //private int m_lastVjConnection = -1;

        private ushort m_nextId;
        private uint m_nextSequence;
        private uint m_nextAck;
        private byte[] m_computerIp;
        private byte[] m_radioIp;

        private byte[] m_lastXcmpResponse;

        public SerialIo(string portName)
        {
            Port = new SerialPort(portName);
            m_receiveBuffer = new byte[0];
            //m_vjConnections = new Dictionary<int, VjUncompressedPacket>();

            m_computerIp = new byte[0];
            m_radioIp = new byte[0];
            m_lastXcmpResponse = new byte[0];
        }

        public void Dispose()
        {
            Port.Dispose();
        }

        public bool OpenSession()
        {
            Port.Open();
            byte[] result;

            result = SendAndReceiveRawBytes(Encoding.ASCII.GetBytes("AT&V\r"));
            if (result.Length < 6 || !result.TakeLast(6).SequenceEqual(Encoding.ASCII.GetBytes("\r\nOK\r\n")))
            {
                Port.Close();
                return false;
            }

            result = SendAndReceiveRawBytes(Encoding.ASCII.GetBytes("AT#DATLCL=1\r"));
            if (result.Length != 6 || !result.SequenceEqual(Encoding.ASCII.GetBytes("\r\nOK\r\n")))
            {
                Port.Close();
                return false;
            }

            result = SendAndReceiveRawBytes(Encoding.ASCII.GetBytes("AT+IPR=115200\r"));
            if (result.Length != 6 || !result.SequenceEqual(Encoding.ASCII.GetBytes("\r\nOK\r\n")))
            {
                Port.Close();
                return false;
            }

            Port.BaudRate = 115200;

            result = SendAndReceiveRawBytes(Encoding.ASCII.GetBytes("CLIENT"));
            if (result.Length != 12 || !result.SequenceEqual(Encoding.ASCII.GetBytes("CLIENTSERVER")))
            {
                Port.Close();
                return false;
            }

            // The start of PPP is setting up the link using LCP

            LcpPacket computerLcpConfigReq = new LcpPacket(LcpPacket.LcpCode.ConfigureRequest, 1,
                new LcpPacket.LcpOption(LcpPacket.LcpOption.LcpOptionType.AsyncControlCharacterMap, new byte[] { 0, 0, 0, 0 }),
                new LcpPacket.LcpOption(LcpPacket.LcpOption.LcpOptionType.MagicNumber, new byte[] { 0x43, 0x8d, 0xe2, 0x22 }),
                new LcpPacket.LcpOption(LcpPacket.LcpOption.LcpOptionType.ProtocolFieldCompression),
                new LcpPacket.LcpOption(LcpPacket.LcpOption.LcpOptionType.AddressControlFieldCompression));
            PppFrame[] frames = SendAndReceive(new PppFrame(PppFrame.PppProtocol.Lcp, computerLcpConfigReq));
            if (frames.Length != 2 ||
                frames[0].Protocol != PppFrame.PppProtocol.Lcp || frames[1].Protocol != PppFrame.PppProtocol.Lcp)
            {
                Port.Close();
                return false;
            }

            LcpPacket radioLcpConfigAck = (LcpPacket)frames[0].DataPacket;
            LcpPacket radioLcpConfigReq = (LcpPacket)frames[1].DataPacket;
            if (radioLcpConfigAck.Code != LcpPacket.LcpCode.ConfigureAck || radioLcpConfigReq.Code != LcpPacket.LcpCode.ConfigureRequest)
            {
                Port.Close();
                return false;
            }

            // Now that we have an ACK from the radio, we can safely disable stuffing (we did ask for an ACCM of all zeros)
            OctetStuffer.DisableStuffing();

            // Okay, we now ACK that and move on to IPCP

            LcpPacket computerLcpConfigAck = new LcpPacket(LcpPacket.LcpCode.ConfigureAck, radioLcpConfigReq.Id, radioLcpConfigReq.Options);
            IpcpPacket computerIpcpZeroIpConfigReq = new IpcpPacket(IpcpPacket.IpcpCode.ConfigureRequest, 1,
                new IpcpPacket.IpcpOption(IpcpPacket.IpcpOption.IpcpOptionType.IpAddress, new byte[] { 0, 0, 0, 0 })/*,
                new IpcpPacket.IpcpOption(IpcpPacket.IpcpOption.IpcpOptionType.IpCompressionProtocol, new byte[] { 0x00, 0x2d, 0x05, 0x01 })*/);
            frames = SendAndReceive(
                new PppFrame(PppFrame.PppProtocol.Lcp, computerLcpConfigAck),
                new PppFrame(PppFrame.PppProtocol.Ipcp, computerIpcpZeroIpConfigReq));
            if (frames.Length != 2 ||
                frames[0].Protocol != PppFrame.PppProtocol.Ipcp || frames[1].Protocol != PppFrame.PppProtocol.Ipcp)
            {
                Port.Close();
                return false;
            }

            IpcpPacket radioIpcpConfigReq = (IpcpPacket)frames[0].DataPacket;
            IpcpPacket computerIpcpZeroIpConfigRej = (IpcpPacket)frames[1].DataPacket;
            if (radioIpcpConfigReq.Code != IpcpPacket.IpcpCode.ConfigureRequest || computerIpcpZeroIpConfigRej.Code != IpcpPacket.IpcpCode.ConfigureNack)
            {
                Port.Close();
                return false;
            }

            foreach (IpcpPacket.IpcpOption option in radioIpcpConfigReq.Options)
            {
                if (option.Type == IpcpPacket.IpcpOption.IpcpOptionType.IpAddress)
                {
                    m_radioIp = option.Data;
                    break;
                }
            }

            foreach (IpcpPacket.IpcpOption option in computerIpcpZeroIpConfigRej.Options)
            {
                if (option.Type == IpcpPacket.IpcpOption.IpcpOptionType.IpAddress)
                {
                    m_computerIp = option.Data;
                    break;
                }
            }

            IpcpPacket radioIpcpConfigAck = new IpcpPacket(IpcpPacket.IpcpCode.ConfigureAck, radioIpcpConfigReq.Id, radioIpcpConfigReq.Options);
            IpcpPacket computerIpcpConfigReq = new IpcpPacket(IpcpPacket.IpcpCode.ConfigureRequest, 2,
                new IpcpPacket.IpcpOption(IpcpPacket.IpcpOption.IpcpOptionType.IpAddress, m_computerIp)/*,
                new IpcpPacket.IpcpOption(IpcpPacket.IpcpOption.IpcpOptionType.IpCompressionProtocol, new byte[] { 0x00, 0x2d, 0x05, 0x01 })*/);
            frames = SendAndReceive(
                new PppFrame(PppFrame.PppProtocol.Ipcp, radioIpcpConfigAck),
                new PppFrame(PppFrame.PppProtocol.Ipcp, computerIpcpConfigReq));
            if (frames.Length != 1 ||
                frames[0].Protocol != PppFrame.PppProtocol.Ipcp)
            {
                Port.Close();
                return false;
            }

            IpcpPacket computerIpcpConfigAck = (IpcpPacket)frames[0].DataPacket;
            if (computerIpcpConfigAck.Code != IpcpPacket.IpcpCode.ConfigureAck)
            {
                Port.Close();
                return false;
            }

            // Now that we have the ACK from the radio, we can enable address/control/protocol compression
            PppFrame.AddressControlCompression = true;
            PppFrame.ProtocolCompression = true;

            PppFrame ip1 = PppFrame.FromRawBytes(new byte[]
                {
                    0x7E, 0x21, 0x45, 0x00, 0x00, 0x2C, 0x02, 0x20, 0x00, 0x00, 0x40, 0x06, 0xF7, 0x57, 0xC0, 0xA8,
                    0x80, 0x02, 0xC0, 0xA8, 0x80, 0x01, 0x1F, 0x41, 0x1F, 0x42, 0x22, 0xE2, 0x8D, 0x78, 0x00, 0x00,
                    0x00, 0x00, 0x60, 0x02, 0x16, 0xD0, 0x11, 0x24, 0x00, 0x00, 0x02, 0x04, 0x05, 0xB4, 0x03, 0x56,
                    0x7E
                });
            m_nextId = 0x0221;

            frames = SendAndReceive(ip1);

            //PppFrame vj1 = new PppFrame(PppFrame.PppProtocol.VanJacobsonUncompressedTcpIp,
            //    new VjUncompressedPacket(
            //        m_nextId++,
            //        0x05,
            //        m_computerIp, m_radioIp,
            //        8001, 8002,
            //        ((IpPacket)frames[0].DataPacket).AcknowledgementNumber, ((IpPacket)frames[0].DataPacket).SequenceNumber + 1,
            //        new byte[0]));

            PppFrame vj1_ip = new PppFrame(PppFrame.PppProtocol.Ip,
                new IpPacket(
                    m_nextId++,
                    0x06,
                    m_computerIp, m_radioIp,
                    8001, 8002,
                    ((IpPacket)frames[0].DataPacket).AcknowledgementNumber, ((IpPacket)frames[0].DataPacket).SequenceNumber + 1,
                    new byte[0]));

            ////PppFrame vj1 = PppFrame.FromRawBytes(new byte[]
            ////    {
            ////        0x7E, 0x2F, 0x45, 0x00, 0x00, 0x28, 0x02, 0x21, 0x00, 0x00, 0x40, 0x05, 0xF7, 0x5A, 0xC0, 0xA8,
            ////        0x80, 0x02, 0xC0, 0xA8, 0x80, 0x01, 0x1F, 0x41, 0x1F, 0x42, 0x22, 0xE2, 0x8D, 0x79, 0x00, 0x00,
            ////        0x2D, 0x8F, 0x50, 0x10, 0x16, 0xD0, 0xFB, 0x41, 0x00, 0x00, 0xC6, 0x08, 0x7E
            ////    });
            //m_vjConnections.Add(5, (VjUncompressedPacket)vj1.DataPacket);
            //m_lastVjConnection = 5;

            Send(vj1_ip);


            //PppFrame vj2 = new PppFrame(PppFrame.PppProtocol.VanJacobsonCompressedTcpIp,
            //    new VjCompressedPacket(
            //        m_vjConnections[m_lastVjConnection],
            //        VjCompressedPacket.VjCompressedHeaderOptions.None,
            //        new byte[] { 0x00, 0x03, 0x00, 0x11, 0x00 }));

            PppFrame vj2_ip = new PppFrame(PppFrame.PppProtocol.Ip,
                new IpPacket(
                    m_nextId++,
                    0x06,
                    m_computerIp, m_radioIp,
                    8001, 8002,
                    ((IpPacket)frames[0].DataPacket).AcknowledgementNumber, ((IpPacket)frames[0].DataPacket).SequenceNumber + 1,
                    new byte[] { 0x00, 0x03, 0x00, 0x11, 0x00 }));

            frames = SendAndReceive(vj2_ip);

            m_nextSequence = ((IpPacket)frames[0].DataPacket).AcknowledgementNumber;
            m_nextAck = ((IpPacket)frames[0].DataPacket).SequenceNumber + 1;

            return true;

            ////PppFrame vj2 = PppFrame.FromRawBytes(new byte[]
            ////{
            ////    0x7E, 0x2D, 0x00, 0xFA, 0x33, 0x00, 0x03, 0x01, 0x06, 0x00, 0x66, 0xD5, 0x7E
            ////});

            //frames = SendAndReceive(vj2_ip);


            //PppFrame vj3_test = new PppFrame(PppFrame.PppProtocol.VanJacobsonCompressedTcpIp,
            //    new VjCompressedPacket(
            //        m_vjConnections[m_lastVjConnection],
            //        VjCompressedPacket.VjCompressedHeaderOptions.DeltaSequence | VjCompressedPacket.VjCompressedHeaderOptions.DeltaAck,
            //        new byte[] { 0x00, 0x02, 0x03, 0x00 }));

            //PppFrame vj3_ip = new PppFrame(PppFrame.PppProtocol.Ip,
            //    new IpPacket(
            //        m_nextId++,
            //        0x06,
            //        m_computerIp, m_radioIp,
            //        8001, 8002,
            //        ((IpPacket)frames[0].DataPacket).AcknowledgementNumber, ((IpPacket)frames[0].DataPacket).SequenceNumber + 1,
            //        new byte[] { 0x00, 0x03, 0x01, 0x06, 0x00 }));

            //frames = SendAndReceive(vj3_ip);

            //PppFrame vj3 = PppFrame.FromRawBytes(new byte[]
            //{
            //    0x7E, 0x2D, 0x00, 0xFA, 0x33, 0x00, 0x03, 0x01, 0x06, 0x00, 0x66, 0xD5, 0x7E
            //});


            //return true;
        }

        public void CloseSession(bool resetRadio = false)
        {
            if (resetRadio)
            {
                SendAndReceiveXcmp(new byte[] { 0x00, 0x0d });
            }
            Port.Close();
        }

        public void SendXcmp(byte[] data)
        {
            //byte[] data = new byte[xcmpBytes.Length + 2];
            //data[0] = (byte)(xcmpBytes.Length >> 8);
            //data[1] = (byte)(xcmpBytes.Length);
            //Array.Copy(xcmpBytes, 0, data, 2, xcmpBytes.Length);

            PppFrame pppFrame = new PppFrame(PppFrame.PppProtocol.Ip,
                new IpPacket(
                    m_nextId++,
                    0x06,
                    m_computerIp, m_radioIp,
                    8001, 8002,
                    m_nextSequence, m_nextAck,
                    data));

            PppFrame[] frames = SendAndReceive(pppFrame).TakeLast(1).ToArray();
            if (frames[0].Protocol != PppFrame.PppProtocol.Ip)
            {
                throw new Exception("Got non-IP response packet");
            }

            IpPacket responsePacket = (IpPacket)frames[0].DataPacket;
            m_nextSequence = responsePacket.AcknowledgementNumber;
            m_nextAck = responsePacket.SequenceNumber + 1;

            //m_lastXcmpResponse = responsePacket.TcpData.Skip(2).ToArray();
            m_lastXcmpResponse = responsePacket.TcpData.ToArray();
        }

        public byte[] ReceiveXcmp()
        {
            return m_lastXcmpResponse;
        }

        public byte[] SendAndReceiveXcmp(byte[] xcmpBytes)
        {
            SendXcmp(xcmpBytes);
            return ReceiveXcmp();
        }

        private byte[] SendAndReceiveRawBytes(byte[] data)
        {
            if (!Port.IsOpen)
            {
                throw new Exception("Serial port is not open");
            }

            Port.Write(data, 0, data.Length);

            return Receive(pppMode: false);
        }

        private PppFrame[] SendAndReceive(params PppFrame[] frames)
        {
            Send(frames);
            return PppFrame.MultipleFromRawBytes(Receive(pppMode: true));
        }

        private void Send(params PppFrame[] frames)
        {
            if (!Port.IsOpen)
            {
                throw new Exception("Serial port is not open");
            }

            foreach (PppFrame frame in frames)
            {
                byte[] data = frame.ToStuffedBytes();
                Port.Write(data, 0, data.Length);
            }
        }

        private byte[] Receive(bool pppMode)
        {
            if (!Port.IsOpen)
            {
                throw new Exception("Serial port is not open");
            }

            List<byte> resultList = new List<byte>();
            while (Port.BytesToRead == 0)
            {
                Thread.Sleep(100);
            }

            int count7e = 0;
            while (Port.BytesToRead > 0 || (pppMode && (count7e == 0 || count7e % 2 != 0)))
            {
                if (Port.BytesToRead > 0)
                {
                    byte[] bytesRead = new byte[m_receiveBuffer.Length + Port.BytesToRead];
                    Port.Read(bytesRead, 0, bytesRead.Length);

                    if (pppMode)
                    {
                        for (int i = 0; i < bytesRead.Length; i++)
                        {
                            count7e += bytesRead[i] == 0x7e ? 1 : 0;
                        }
                    }
                    resultList.AddRange(bytesRead);
                    Thread.Sleep(50);
                }
            }

            return resultList.ToArray();
        }
    }
}
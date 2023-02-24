using System.Net.Sockets;
using PppLib;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class XCMPPPPConnection : IXCMPRadioConnection
    {
        private SerialIo IoPort;
        public XCMPPPPConnection(string serialPort)
        {
            IoPort = new SerialIo(serialPort);
        }

        public void Dispose()
        {
            Disconnect();
            IoPort.Dispose();
        }

        public void Connect()
        {
            if (!IoPort.OpenSession())
            {
                throw new Exception("PppLib reported error when connecting");
            }
        }

        public void Disconnect()
        {
            IoPort.CloseSession(resetRadio: false);
        }

        public byte[] Receive()
        {
            Console.WriteLine();
            byte[] result = IoPort.ReceiveXcmp();
            Console.WriteLine(result.Length);

            foreach (byte b in result) { 
                Console.WriteLine($"{b}");
            }

            if (result.Length == 0)
            {
                Thread.Sleep(50);
                Receive();
            }

            return result;
        }

        public void Send(byte[] data)
        {
            IoPort.SendXcmp(data);
            Thread.Sleep(50);
        }
    }
}

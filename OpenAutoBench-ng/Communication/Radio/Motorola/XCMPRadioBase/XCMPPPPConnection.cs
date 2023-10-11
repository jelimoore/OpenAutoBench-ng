namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class XCMPPPPConnection : IXCMPRadioConnection
    {
        public XCMPPPPConnection(string serialPort)
        {
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void Connect()
        {
            //
        }

        public void Disconnect()
        {
            //
        }

        public byte[] Receive()
        {
            return null;
        }

        public void Send(byte[] data)
        {
            //
        }
    }
}

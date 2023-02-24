namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public interface IXCMPRadioConnection : IDisposable
    {

        public void Connect();

        public void Disconnect();
        public void Send(byte[] data);

        public byte[] Receive();
    }
}

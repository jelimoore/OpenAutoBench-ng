using OpenAutoBench_ng.Communication.Instrument.Connection;

namespace OpenAutoBench_ng.Communication.Instrument.HP_8900
{
    public class HP_8900Instrument : IBaseInstrument
    {
        private IInstrumentConnection Connection;

        public bool Connected { get; private set; }

        public bool SupportsP25 { get { return false; } }

        public bool SupportsDMR { get { return false; } }

        private int GPIBAddr;
        public HP_8900Instrument(IInstrumentConnection conn, int addr)
        {
            Connected = false;
            Connection = conn;
            GPIBAddr = addr;
        }

        private async Task<string> Send(string command)
        {
            return await Connection.Send(command);
        }

        private async Task Transmit(string command)
        {
            await Connection.Transmit(command);
        }

        public async Task Connect()
        {
            Connection.Connect();
            await Transmit("++mode 1");
            await Transmit("++addr " + GPIBAddr.ToString());
            await Transmit("++auto 2");
            await Transmit("++llo");
        }

        public async Task Disconnect()
        {
            await Transmit("++loc");
            await Transmit("++loc");
            Connection.Disconnect();
        }

        public void GenerateSignal(float power)
        {
            Send("RFG:AMPL " + power.ToString());
        }

        public void GenerateFMSignal(float power, float afFreq)
        {
            GenerateSignal(power);
            //todo: add FM frequency
        }

        public void SetRxFrequency(int frequency)
        {
            Transmit(string.Format("RFAN:FREQ {0}", frequency.ToString()));
        }

        public void SetTxFrequency(int frequency)
        {
            Send("RFG:FREQ " + frequency.ToString());
        }

        public async Task<float> MeasurePower()
        {
            return float.Parse(await Send("MEAS:RFR:POW?"));
        }

        public async Task<float> MeasureFrequencyError()
        {
            return float.Parse(await Send("MEAS:RFR:FREQ:ERR?"));
        }

        public async Task<float> MeasureFMDeviation()
        {
            return float.Parse(await Send("MEAS:AFR:FM?"));
        }

        public async Task<string> GetInfo()
        {
            return await Send("*IDN?");
        }

        public async Task Reset()
        {
            await Send("*RST");
        }

        public async Task SetDisplay(string displayName)
        {
            await Transmit("DISP " + displayName);
        }

        public async Task SetupFiltersForDeviation()
        {
            await Transmit("AFAN:FILT1 '<20Hz HPF'");
            await Transmit("AFAN:FILT2 '15kHz LPF'");
        }

        public Task<float> MeasureP25RxBer()
        {
            throw new NotImplementedException("HP 8900 does not support digital tests.");
        }

        public Task<float> MeasurDMR5RxBer()
        {
            throw new NotImplementedException("HP 8900 does not support digital tests.");
        }

        public Task<float> MeasureDMRRxBer()
        {
            throw new NotImplementedException();
        }

        public Task ResetBERErrors()
        {
            throw new NotImplementedException();
        }
    }
}

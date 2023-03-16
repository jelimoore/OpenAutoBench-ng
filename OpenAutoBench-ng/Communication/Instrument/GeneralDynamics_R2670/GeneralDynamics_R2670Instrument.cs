using OpenAutoBench_ng.Communication.Instrument.Connection;

namespace OpenAutoBench_ng.Communication.Instrument.GeneralDynamics_R2670
{
    public class GeneralDynamics_R2670Instrument: IBaseInstrument
    {
        private IInstrumentConnection Connection;

        public bool Connected { get; private set; }

        public bool SupportsP25 { get { return true; } }

        public bool SupportsDMR { get { return false; } }

        private int GPIBAddr;
        public GeneralDynamics_R2670Instrument(IInstrumentConnection conn, int addr)
        {
            Connected = false;
            Connection = conn;
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
        }

        public async Task Disconnect()
        {
            Connection.Disconnect();
        }

        public void GenerateSignal(float power)
        {
            throw new NotImplementedException();
        }

        public void GenerateFMSignal(float power, float afFreq)
        {
            GenerateSignal(power);
            //todo: add FM frequency
        }

        public void SetRxFrequency(int frequency)
        {
            // RM command
            throw new NotImplementedException();
        }

        public void SetTxFrequency(int frequency)
        {
            throw new NotImplementedException();
        }

        public async Task<float> MeasurePower()
        {
            // MR command, pg. 106
            throw new NotImplementedException();
        }

        public async Task<float> MeasureFrequencyError()
        {
            // MR command, pg. 106
            throw new NotImplementedException();
        }

        public async Task<float> MeasureFMDeviation()
        {
            // MR command, pg. 106
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public async Task SetupFiltersForDeviation()
        {
            throw new NotImplementedException();
        }

        public Task<float> MeasureP25RxBer()
        {
            throw new NotImplementedException();
        }

        public Task<float> MeasurDMRRxBer()
        {
            throw new NotImplementedException("R2670 does not support DMR.");
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

using OpenAutoBench_ng.Communication.Instrument.Connection;
using OpenAutoBench_ng.Communication.Instrument.IFR_2975;

namespace OpenAutoBench_ng.Communication.Instrument.Viavi_8800SX
{
    public class Viavi_8800SXInstrument: IBaseInstrument
    {
        private IInstrumentConnection Connection;

        public bool Connected { get; private set; }

        //TODO: get features and see if we are licensed for P25 or DMR
        public bool SupportsP25 { get { return true; } }

        public bool SupportsDMR { get { return true; } }

        public Viavi_8800SXInstrument(IInstrumentConnection conn)
        {
            Connected = false;
            Connection = conn;
            Connection.SetDelimeter("");
        }

        private async Task<string> Send(string command)
        {
            return await Connection.Send(command);
            //return await Connection.Send("\r\n");
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

        public async Task GenerateSignal(float power)
        {
            throw new NotImplementedException();
        }

        public async Task GenerateFMSignal(float power, float afFreq)
        {
            throw new NotImplementedException();
        }

        public Task StopGenerating()
        {
            throw new NotImplementedException();
        }

        public async Task SetGenPort(InstrumentOutputPort outputPort)
        {
            throw new NotImplementedException();
        }

        public async Task SetRxFrequency(int frequency)
        {
            await Send($":rec:freq {frequency} MHz");
        }

        public async Task SetTxFrequency(int frequency)
        {
            await Send($":gen:freq {frequency} MHz");
        }

        public async Task<float> MeasurePower()
        {
            return float.Parse(await Send(":rfpow:reading:avg?"));
        }

        public async Task<float> MeasureFrequencyError()
        {
            return float.Parse(await Send(":rferr:reading:avg?"));
        }

        public async Task<float> MeasureFMDeviation()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetInfo()
        {
            string mfr = await Send(":options:man?");
            string model = await Send(":options:model?");
            string serial = await Send(":options:serial?");
            return $"{mfr},{model},{serial}";
        }

        public async Task Reset()
        {
            await Send("*RST");
        }

        public async Task SetDisplay(string displayName)
        {
            //await Transmit("DISP " + displayName);
        }

        public async Task SetupFiltersForDeviation()
        {
            //await Transmit("AFAN:FILT1 '<20Hz HPF'");
            //await Transmit("AFAN:FILT2 '15kHz LPF'");
        }

        public async Task<float> MeasureP25RxBer()
        {
            throw new NotImplementedException();
            //string resp = await Send("Ber READING");
            // reading is percentage as decimal
            //return float.Parse(resp.Split(" ")[0]) * 100;
        }

        public Task<float> MeasureDMRRxBer()
        {
            throw new NotImplementedException();
        }

        public async Task ResetBERErrors()
        {
            throw new NotImplementedException();
            //await Send("Ber RESETERRors");
        }
    }
}

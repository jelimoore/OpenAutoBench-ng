using OpenAutoBench_ng.Communication.Instrument.Connection;

namespace OpenAutoBench_ng.Communication.Instrument.IFR_2975
{
    public class IFR_2975Instrument : IBaseInstrument
    {
        private IInstrumentConnection Connection;

        public bool Connected { get; private set; }

        public bool SupportsP25 { get { return true; } }

        public bool SupportsDMR { get { return false; } }

        public IFR_2975Instrument(IInstrumentConnection conn)
        {
            Connected = false;
            Connection = new IFR_2975Connection(conn);
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
            await Send("LOCKOUT ON");

        }

        public async Task Disconnect()
        {
            await Send("LOCKOUT OFF");
            Connection.Disconnect();
        }

        public async Task GenerateSignal(float power)
        {
            await Send($"Generator RFLEVel {power.ToString()}");
        }

        public async Task GenerateFMSignal(float power, float afFreq)
        {
            await GenerateSignal(power);
            await Send("Generator MODulation 1");
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
            await Send($"Receiver FREQuency {frequency.ToString()} Hz");
        }

        public async Task SetTxFrequency(int frequency)
        {
            await Send($"Generator FREQuency {frequency.ToString()} Hz");
        }

        public async Task<float> MeasurePower()
        {
            return float.Parse(await Send("Power VALue"));
        }

        public async Task<float> MeasureFrequencyError()
        {
            return float.Parse(await Send("RFError VALue"));
        }

        public async Task<float> MeasureFMDeviation()
        {
            return float.Parse(await Send("FMDev VALue"));
        }

        public async Task<string> GetInfo()
        {
            return await Send("*IDN?");
        }

        public async Task Reset()
        {
            await Send("*RST");
        }

        public async Task SetDisplay(InstrumentScreen screen)
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
            string resp = await Send("Ber READING");
            // reading is percentage as decimal
            return float.Parse(resp.Split(" ")[0]) * 100;
        }

        public Task<float> MeasureDMRRxBer()
        {
            throw new NotImplementedException();
        }

        public async Task ResetBERErrors()
        {
            await Send("Ber RESETERRors");
        }
    }
}

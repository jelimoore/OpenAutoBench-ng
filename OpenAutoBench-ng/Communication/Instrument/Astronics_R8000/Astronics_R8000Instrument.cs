using OpenAutoBench_ng.Communication.Instrument.Connection;

namespace OpenAutoBench_ng.Communication.Instrument.Astronics_R8000
{
    public class Astronics_R8000Instrument : IBaseInstrument
    {
        private IInstrumentConnection Connection;

        public bool Connected { get; private set; }

        public bool SupportsP25 { get { return true; } }

        public bool SupportsDMR { get { return false; } }

        public Astronics_R8000Instrument(IInstrumentConnection conn)
        {
            Connected = false;
            Connection = conn;
            conn.SetDelimeter("\r\n");
        }

        private async Task<string> Send(string command)
        {
            string result = await Connection.Send(command);
            string[] resultTemp = result.Split(":", 2);
            if (resultTemp[0] != "0")
            {
                throw new Exception($"Instrument returned error {resultTemp[0]}");
            }
            return resultTemp[1].Replace("\r", "").Replace("\n", "");

        }

        private async Task Transmit(string command)
        {
            await Connection.Transmit(command);
        }

        public async Task Connect()
        {
            Connection.Connect();
            await Task.Delay(100);
            await Send("SET RF:Frequency Error Units=Hertz");
            await Task.Delay(200);
            await Send("SET METER: PWR Meter Range=150W");
            await Task.Delay(200);
            await Send("SET METER:Subzone=Power Meter");

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

        public async Task StopGenerating()
        {
            throw new NotImplementedException();
        }

        public async Task SetGenPort(InstrumentOutputPort outputPort)
        {
            throw new NotImplementedException();
        }

        public async Task SetRxFrequency(int frequency)
        {
            await Send($"SET RF:Monitor Frequency={frequency} Hz");
            // necessary to wait a little while or else it will return busy
            await Task.Delay(5000);
        }

        public async Task SetTxFrequency(int frequency)
        {
            await Send($"SET RF:Generator Frequency={frequency} Hz");
            // necessary to wait a little while or else it will return busy
            await Task.Delay(5000);
        }

        public async Task<float> MeasurePower()
        {
            float dbm = float.Parse(await Send("GET METER:Measured Power"));
            float watts = (float) Math.Pow(10, (double) dbm / 10D) / 1000;
            return watts;
        }

        public async Task<float> MeasureFrequencyError()
        {
            return float.Parse(await Send("GET MONITOR:Frequency Error"));
        }

        public async Task<float> MeasureFMDeviation()
        {
            return float.Parse(await Send("GET MONITOR:Deviation+"));
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
            //
            //await Transmit("DISP " + displayName);
        }

        public async Task SetupFiltersForDeviation()
        {
            //await Transmit("AFAN:FILT1 '<20Hz HPF'");
            //await Transmit("AFAN:FILT2 '15kHz LPF'");
        }

        public async Task<float> MeasureP25RxBer()
        {
            await Send("SET P25:BER Test=Stop");
            await Task.Delay(1000);     // the times in the manual are a lie
            return float.Parse(await Send("GET P25:BER Result"));
        }

        public Task<float> MeasureDMRRxBer()
        {
            throw new NotImplementedException();
        }

        public async Task ResetBERErrors()
        {
            await Task.Delay(5000);
            await Send("SET P25:BER Test=Start");
        }
    }
}

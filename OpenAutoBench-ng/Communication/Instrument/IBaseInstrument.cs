namespace OpenAutoBench_ng.Communication.Instrument
{
    public interface IBaseInstrument
    {
        public bool Connected { get; }

        public bool SupportsP25 { get; }

        public bool SupportsDMR { get; }
        public Task Connect();
        public Task Disconnect();
        public void GenerateSignal(float power);
        public void GenerateFMSignal(float power, float afFreq);

        public void SetRxFrequency(int frequency);

        public void SetTxFrequency(int frequency);

        public Task<float> MeasurePower();

        public Task<float> MeasureFrequencyError();

        public Task<float> MeasureFMDeviation();

        public Task<string> GetInfo();

        public Task Reset();

        public Task SetDisplay(string displayName);

        public Task SetupFiltersForDeviation();

        public Task<float> MeasureP25RxBer();

        public Task<float> MeasureDMRRxBer();

        public Task ResetBERErrors();
    }
}

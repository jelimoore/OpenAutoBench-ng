using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase
{
    public class MotorolaRSSRepeater_TestTX_Power : IBaseTest
    {
        public string name { get
            {
                return "TX: Power";
            } }

        public bool pass { get; private set; }

        public bool testCompleted { get; private set; }

        private IBaseInstrument Instrument;

        private Action<string> LogCallback;

        protected MotorolaRSSRepeaterBase Repeater;

        // private vars specific to test

        private int TXFrequency;

        public MotorolaRSSRepeater_TestTX_Power(MotorolaRSSRepeaterBaseTestParams testParams)
        {
            LogCallback = testParams.callback;
            Repeater = testParams.radio;
            Instrument = testParams.instrument;
        }

        public bool isRadioEligible()
        {
            return true;
        }

        public async Task setup()
        {
            await Instrument.SetDisplay("RFAN");
            TXFrequency = await Repeater.GetTxFrequency();
            await Repeater.Transmit("AL STNPWR RESET");
        }

        public async Task performTest()
        {
            await Repeater.Send("SET TX PWR 100");
            Instrument.SetRxFrequency(TXFrequency);
            Repeater.Keyup();
            await Task.Delay(5000);
            float measPower = await Instrument.MeasurePower();
            Repeater.Dekey();
            measPower = (float) Math.Round(measPower, 2);
            LogCallback(String.Format("Measured power at {0}MHz: {1}w (expected 100W)", (TXFrequency / 1000000D), measPower));
        }

        public async Task performAlignment()
        {
            throw new NotImplementedException();
        }

        public async Task teardown()
        {
            //
        }
    }
}

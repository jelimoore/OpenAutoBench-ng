using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase
{
    public class MotorolaRSSRepeater_TestTX_ReferenceOscillator : IBaseTest
    {
        public string name
        {
            get
            {
                return "TX: Reference Oscillator";
            }
        }

        public bool pass { get; private set; }

        public bool testCompleted { get; private set; }

        private IBaseInstrument Instrument;

        private Action<string> LogCallback;

        private RSSRepeaterBase Repeater;

        // private vars specific to test

        private int TXFrequency;

        public MotorolaRSSRepeater_TestTX_ReferenceOscillator(RSSRepeaterBaseTestParams testParams)
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
            LogCallback(String.Format("Setting up for {0}", name));
            await Instrument.SetDisplay("RFAN");
            await Repeater.SetShell(RSSRepeaterBase.Shell.RSS);
            TXFrequency = await Repeater.GetTxFrequency();
        }

        public async Task performTest()
        {
            try
            {
                Instrument.SetRxFrequency(TXFrequency);
                Repeater.Keyup();
                await Task.Delay(5000);
                float measErr = await Instrument.MeasureFrequencyError();
                measErr = (float)Math.Round(measErr, 2);
                LogCallback(String.Format("Measured frequency error at {0}MHz: {1}hz", (TXFrequency / 1000000F), measErr));
            }
            catch (Exception ex)
            {
                LogCallback(String.Format("Test failed: {0}", ex.ToString()));
                throw new Exception("Test failed.", ex);
            }
            finally
            {
                Repeater.Dekey();
            }
            
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

using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.Quantar
{
    public class MotorolaQuantar_TestRX_RSSI : IBaseTest
    {
        public string name
        {
            get
            {
                return "RX: RSSI";
            }
        }

        public bool pass { get; private set; }

        public bool testCompleted { get; private set; }

        private IBaseInstrument Instrument;

        private Action<string> LogCallback;

        protected MotorolaQuantar Repeater;

        private int GenLevel = -90;

        // private vars specific to test

        private int RXFrequency;

        public MotorolaQuantar_TestRX_RSSI(MotorolaRSSRepeaterBaseTestParams testParams)
        {
            LogCallback = testParams.callback;
            Repeater = (MotorolaQuantar)testParams.radio;
            Instrument = testParams.instrument;
        }

        public bool isRadioEligible()
        {
            return true;
        }

        public async Task setup()
        {
            await Instrument.SetDisplay("RFG");
            await Repeater.Transmit("SET FREQ TX 0");
            RXFrequency = await Repeater.GetRxFrequency();
        }

        public async Task performTest()
        {
            await Instrument.StopGenerating();
            await Instrument.SetTxFrequency(RXFrequency);
            await Instrument.GenerateSignal(GenLevel);
            float measRssi = await Repeater.ReadRSSI();
            LogCallback(String.Format("Measured RSSI at {0}MHz: {1}db (expected {2}db)", (RXFrequency / 1000000D), measRssi, GenLevel));
            await Instrument.StopGenerating();
        }

        public async Task performAlignment()
        {
            throw new NotSupportedException();
        }

        public async Task teardown()
        {
            //
        }
    }
}

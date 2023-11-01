using Microsoft.Extensions.ObjectPool;
using OpenAutoBench_ng.Communication.Instrument;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class XCMPRadioTestParams
    {
        public bool doRefoscTest = true;
        public bool doPowerTest = true;
        public bool doDeviationTest = true;
        public bool doRssiTest = true;
        public bool doTxBer = true;
        public bool doRxBer = true;

        // extended tests

        public bool doRxExtendedTest = true;
        public bool doTxExtendedTest = true;

        public int ExtendedTestStart = 0;
        public int ExtendedTestEnd = 0;
        public int ExtendedTestStep = 0;

        public IBaseInstrument? instrument;
        public Action<string>? callback;
        public MotorolaXCMPRadioBase? radio;

        public XCMPRadioTestParams()
        {
        }


    }
}

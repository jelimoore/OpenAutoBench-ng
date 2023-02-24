using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_TestTX_DeviationBalance : MotorolaXCMPRadio_TestTX_DeviationBalance
    {
        public MotorolaAPX_TestTX_DeviationBalance(XCMPRadioTestParams testParams): base(testParams)
        {
            //
        }

        public async Task setup()
        {
            await base.setup();
            TXFrequencies = MotorolaAPX_Frequencies.TxFrequencies(Radio);
        }
    }
}

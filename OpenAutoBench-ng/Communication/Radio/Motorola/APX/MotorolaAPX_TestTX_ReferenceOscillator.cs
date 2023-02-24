using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_TestTX_ReferenceOscillator : MotorolaXCMPRadio_TestTX_ReferenceOscillator
    {
        public MotorolaAPX_TestTX_ReferenceOscillator(XCMPRadioTestParams testParams): base(testParams)
        {
            //
        }

        public async Task setup()
        {
            await base.setup();
            int[] TXFrequencies = MotorolaAPX_Frequencies.TxFrequencies(Radio);
            TXFrequency = TXFrequencies[TXFrequencies.Length - 1];
        }
    }
}

using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.Astro25
{
    public class MotorolaAstro25_TestTX_ReferenceOscillator : MotorolaXCMPRadio_TestTX_ReferenceOscillator
    {
        public MotorolaAstro25_TestTX_ReferenceOscillator(XCMPRadioTestParams testParams): base(testParams)
        {
            //
        }

        public async Task setup()
        {
            await base.setup();
            int[] TXFrequencies = MotorolaAstro25_Frequencies.TxFrequencies(Radio);
            TXFrequency = TXFrequencies[TXFrequencies.Length - 1];
        }
    }
}

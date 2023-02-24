using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.Astro25
{
    public class MotorolaAstro25_TestTX_P25_BER : MotorolaXCMPRadio_TestTX_P25_BER
    {
        public MotorolaAstro25_TestTX_P25_BER(XCMPRadioTestParams testParams) : base(testParams)
        {
            //
        }

        public async Task setup()
        {
            await base.setup();
            TXFrequencies = MotorolaAstro25_Frequencies.TxFrequencies(Radio);
        }
    }
}

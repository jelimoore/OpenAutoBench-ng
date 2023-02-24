using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_TestTX_P25_BER : MotorolaXCMPRadio_TestTX_P25_BER
    {
        public MotorolaAPX_TestTX_P25_BER(XCMPRadioTestParams testParams) : base(testParams)
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

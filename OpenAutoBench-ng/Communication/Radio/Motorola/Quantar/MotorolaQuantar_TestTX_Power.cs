using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.Quantar
{
    public class MotorolaQuantar_TestTX_Power : MotorolaRSSRepeater_TestTX_Power
    {
        public MotorolaQuantar_TestTX_Power(MotorolaRSSRepeaterBaseTestParams testParams) :
            base(testParams)
        {
        }

        public async Task setup()
        {
            await base.setup();
            await Repeater.Send("ALN STNPWR RESET");
            await Repeater.Send("SET TX PWR 100");
        }
    }
}

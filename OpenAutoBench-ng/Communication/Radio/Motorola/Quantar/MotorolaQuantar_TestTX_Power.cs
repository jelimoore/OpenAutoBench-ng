using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;
using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

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

        public async Task performAlignment()
        {
            // run 5 times
            for (int i=0; i<5; i++)
            {
                float measPower = await performTestWithReturn();
                LogCallback($"Round {i}: {Math.Round(measPower, 2)}");
                measPower = (float)Math.Round(measPower * 100);
                double radioPower = Math.Round((double)(PA_PWR * 100));
                LogCallback("Writing new power level to radio");
                await Repeater.Send($"AL STNPWR WR {radioPower} {measPower}");
                await Task.Delay(3000);
            }

            await Repeater.Send("AL STNPWR SAVE");
            await Task.Delay(6000);

            float finalMeasPower = await performTestWithReturn();
            LogCallback($"Final measured power: {Math.Round(finalMeasPower, 2)}w");



        }
    }
}

using System.Runtime.CompilerServices;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.Quantar
{
    public class MotorolaQuantar : RSSRepeaterBase.MotorolaRSSRepeaterBase
    {
        public MotorolaQuantar(string portName, int baud = 9600) : base(portName, baud)
        {
            //
        }

        public async Task<float> ReadRSSI()
        {
            // no clue what these options do just that it gives you RSSI
            return float.Parse(await Get("GET DSP RSSI 1 1 SHORT"));
        }
    }
}

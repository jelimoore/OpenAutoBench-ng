using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.Astro25
{
    public class MotorolaAstro25_Frequencies
    {
        public static int[] TxFrequencies(MotorolaXCMPRadioBase radio)
        {
            List<int> TxFrequencies = new List<int>();
            /*
             * PORTABLES
             */

            // VHF XTS5k
            if (radio.ModelNumber.Contains("H18K"))
            {
                TxFrequencies.Add(136025000);
                TxFrequencies.Add(142125000);
                TxFrequencies.Add(154225000);
                TxFrequencies.Add(160125000);
                TxFrequencies.Add(168075000);
                TxFrequencies.Add(173975000);
            }

            // all else, error out
            else
            {
                throw new Exception($"I don't know frequencies for your model {radio.ModelNumber}");
            }

            return TxFrequencies.ToArray();
        }
    }
}

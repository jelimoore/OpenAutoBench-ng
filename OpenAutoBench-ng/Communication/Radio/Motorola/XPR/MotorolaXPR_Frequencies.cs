using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XPR
{
    public class MotorolaXPR_Frequencies
    {
        public static int TxRefOscFrequencies(MotorolaXCMPRadioBase radio)
        {
            int TxFrequency = 0;
            /*
             * PORTABLES
             */

            /*
             * MOBILES
             */

            if (radio.ModelNumber.Contains("M27Q"))
            {
                TxFrequency = 469975000;
            }

            /*
             * BASE
             */

            else if (radio.ModelNumber.Contains("M27T") && radio.ModelNumber.Contains("R9JA7AN"))
            {
                TxFrequency = 527000000;
            }

            // all else, error out
            else
            {
                throw new Exception($"I don't know frequencies for your XPR model {radio.ModelNumber}");
            }

            return TxFrequency;
        }
        public static int[] TxDeviationFrequencies(MotorolaXCMPRadioBase radio)
        {
            List<int> TxFrequencies = new List<int>();
            /*
             * PORTABLES
             */

            /*
             * MOBILES
             */

            if (radio.ModelNumber.Contains("M27Q"))
            {
                TxFrequencies.Add(403000000);
                TxFrequencies.Add(412000000);
                TxFrequencies.Add(426000000);
                TxFrequencies.Add(436500000);
                TxFrequencies.Add(437000000);
                TxFrequencies.Add(449000000);
                TxFrequencies.Add(458000000);
                TxFrequencies.Add(470000000);
            }

            /*
             * BASE
             */

            else if (radio.ModelNumber.Contains("M27T") && radio.ModelNumber.Contains("R9JA7AN"))
            {
                TxFrequencies.Add(450000000);
                TxFrequencies.Add(463000000);
                TxFrequencies.Add(476000000);
                TxFrequencies.Add(488500000);
                TxFrequencies.Add(488500050);
                TxFrequencies.Add(501000000);
                TxFrequencies.Add(514500000);
                TxFrequencies.Add(527000000);
            }

            // all else, error out
            else
            {
                throw new Exception($"I don't know frequencies for your XPR model {radio.ModelNumber}");
            }

            return TxFrequencies.ToArray();
        }
        public static int[] TxPowerFrequencies(MotorolaXCMPRadioBase radio)
        {
            List<int> TxFrequencies = new List<int>();
            /*
             * PORTABLES
             */

            /*
             * MOBILES
             */
            if (radio.ModelNumber.Contains("M27Q"))
            {
                TxFrequencies.Add(403100000);
                TxFrequencies.Add(414250000);
                TxFrequencies.Add(425450000);
                TxFrequencies.Add(436600000);
                TxFrequencies.Add(447775000);
                TxFrequencies.Add(458950000);
                TxFrequencies.Add(469975000);
            }
            /*
             * BASE
             */

            else if (radio.ModelNumber.Contains("M27T") && radio.ModelNumber.Contains("R9JA7AN"))
            {
                TxFrequencies.Add(450000000);
                TxFrequencies.Add(462400000);
                TxFrequencies.Add(474800000);
                TxFrequencies.Add(487200000);
                TxFrequencies.Add(499600000);
                TxFrequencies.Add(512000050);
                TxFrequencies.Add(519500000);
                TxFrequencies.Add(527000000);
            }

            // all else, error out
            else
            {
                throw new Exception($"I don't know frequencies for your XPR model {radio.ModelNumber}");
            }

            return TxFrequencies.ToArray();
        }

        public static int RxRefOscFrequencies(MotorolaXCMPRadioBase radio)
        {
            int RxFrequency = 0;
            /*
             * PORTABLES
             */

            /*
             * MOBILES
             */


            /*
             * BASE
             */
            if (radio.ModelNumber.Contains("M27T") && radio.ModelNumber.Contains("R9JA7AN"))
            {
                RxFrequency = 526925000;
            }

            // all else, error out
            else
            {
                throw new Exception($"I don't know frequencies for your XPR model {radio.ModelNumber}");
            }

            return RxFrequency;
        }
    }
}

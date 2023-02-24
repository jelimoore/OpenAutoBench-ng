namespace OpenAutoBench_ng.OpenAutoBench
{
    public class Settings
    {
        public int Version { get; set; }

        public enum InstrumentTypeEnum
        {
            //Generic = 0,    // will just issue SCPI and hope for the best; analog only
            HP_8900 = 1,            // HP 8935, 8920, possibly others
            //Aeroflex_3920 = 2,      // Aeroflex 3920
            IFR_2975 = 3,           // Aeroflex/IFR 2975
            R2670 = 4,    // General Dynamics/Motorola R2600
            //Anritsu = 5,            // Anritsu LMR Master
        }

        public InstrumentTypeEnum InstrumentType { get; set; }

        public enum InstrumentConnectionTypeEnum
        {
            Serial = 0,
            //USB = 1,
            IP = 2,
        }

        public InstrumentConnectionTypeEnum InstrumentConnectionType { get; set; }

        /// <summary>
        /// Bool to store if the instrument in question is on a GPIB bus.
        /// Pretty much just adds some specific commands for selecting the address
        /// </summary>
        public bool IsGPIB { get; set; }

        /// <summary>
        /// The address of the instrument on the GPIB bus.
        /// </summary>
        public int InstrumentGPIBAddress { get; set;}

        /// <summary>
        /// The serial port the instrument (or interface) is connected at.
        /// </summary>
        public string InstrumentSerialPort { get; set; }
        
        public int InstrumentBaudrate { get; set; }

        public string InstrumentIPAddress { get; set; }

        public int InstrumentIPPort { get; set; }

        public int[] MotoTrboKeys { get; set; }

        public int MotoTrboDelta { get; set; }

        /// <summary>
        /// Danger mode! Enables settings that could be dangerous or are for development purposes.
        /// </summary>
        public bool DangerMode { get; set; }

        /// <summary>
        /// Disables model number checking in tests, will force all tests to be run regardless if the radio supports it.
        /// </summary>
        public bool DisableModelChecking { get; set; }

        public Settings()
        {
            Version = 1;
            InstrumentType = InstrumentTypeEnum.HP_8900;
            InstrumentConnectionType = InstrumentConnectionTypeEnum.Serial;
            InstrumentSerialPort = "";
            InstrumentBaudrate = 115200;
            IsGPIB = false;
            InstrumentGPIBAddress = 0;
            InstrumentIPAddress = "";
            InstrumentIPPort = 0;
            MotoTrboKeys = new int[] { 0, 0, 0, 0};
            MotoTrboDelta = 0;
            DangerMode = false;
            DisableModelChecking = false;
        }
    }
}

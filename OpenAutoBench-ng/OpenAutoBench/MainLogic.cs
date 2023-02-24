using OpenAutoBench_ng.Communication.Instrument;
using System.Net.NetworkInformation;
using OpenAutoBench_ng.Communication.Instrument.Connection;
using OpenAutoBench_ng.Communication.Instrument.HP_8900;
using OpenAutoBench_ng.Communication.Instrument.IFR_2975;
using System;
using System.IO.Ports;
using System.ComponentModel;

namespace OpenAutoBench_ng.OpenAutoBench
{
    public class MainLogic
    {

        public MainLogic()
        {
            //
        }
        public static string[] GetSerialPorts()
        {
            return SerialPort.GetPortNames();
        }

        public static void OnStart()
        {
            //preferences = new Preferences();
            //preferences.Load();
            //Console.WriteLine("Loaded settings");
            //Console.WriteLine("Serial port for instrument: {0}", preferences.settings.InstrumentSerialPort);
        }

        public static async Task<IBaseInstrument> CreateInstrument()
        {
            Preferences prefs = new Preferences();
            Settings settings = prefs.Load();
            IBaseInstrument instrument = null;
            IInstrumentConnection connection = null;

            switch (settings.InstrumentConnectionType)
            {
                case Settings.InstrumentConnectionTypeEnum.Serial:
                    connection = new SerialConnection(settings.InstrumentSerialPort, settings.InstrumentBaudrate);
                    break;
                case Settings.InstrumentConnectionTypeEnum.IP:
                    connection = new IPConnection(settings.InstrumentIPAddress, settings.InstrumentIPPort);
                    break;
                default:
                    throw new Exception("Unsupported connection type. Dying.");
            }

            switch (settings.InstrumentType)
            {
                case Settings.InstrumentTypeEnum.HP_8900:
                    if (!settings.IsGPIB)
                    {
                        throw new Exception("GPIB disabled and HP 8900 selected. This is an impossible combination.");
                    }
                     
                    instrument = new HP_8900Instrument(connection, settings.InstrumentGPIBAddress);
                    await instrument.Connect();
                    await Task.Delay(500);

                    try
                    {
                        string instInfo = await instrument.GetInfo();
                        if (!(instInfo.Length > 0))
                        {
                            throw new Exception("Get info succeeded but returned a zero length");
                        }
                    }
                    catch (Exception e)
                    {
                        await instrument.Disconnect();
                        throw new Exception("Connection to instrument failed: " + e.ToString());
                    }
                    break;

                case Settings.InstrumentTypeEnum.IFR_2975:
                    if (settings.IsGPIB)
                    {
                        throw new Exception("GPIB enabled and IFR 2975 selected. GPIB is not supported on this instrument.");
                    }
                    instrument = new IFR_2975Instrument(connection);
                    await instrument.Connect();
                    await Task.Delay(500);

                    try
                    {
                        string instInfo = await instrument.GetInfo();
                        if (!(instInfo.Length > 0))
                        {
                            throw new Exception("Get info succeeded but returned a zero length");
                        }
                    }
                    catch (Exception e)
                    {
                        await instrument.Disconnect();
                        throw new Exception("Connection to instrument failed: " + e.ToString());
                    }
                    break;
                default:
                    // this shouldn't happen!
                    throw new Exception("Unsupported instrument somehow selected. Dying.");
            }
            return instrument;
        }
    }
}

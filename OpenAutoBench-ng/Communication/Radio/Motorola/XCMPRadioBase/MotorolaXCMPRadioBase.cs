using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;
using System.IO.Ports;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase
{
    public class MotorolaXCMPRadioBase : IBaseRadio
    {
        public string Name { get; private set; }

        public string SerialNumber { get; private set; }

        public string FirmwareVersion { get; private set; }

        public string InfoHeader { get; private set; }

        public string ModelNumber { get; private set; }

        protected IXCMPRadioConnection _connection;

        public enum VersionOperation : byte
        {
            HostSoftware = 0x00,
            DSPSoftware = 0x10,
            UCMSoftware = 0x20,
            MACESoftware = 0x23,
            BootloaderVersion = 0x30,
            TuningVersion = 0x40,
            CPVersion = 0x42,
            RFBand = 0x63,
            RFPowerLevel = 0x65
        }

        public enum StatusOperation : byte
        {
            RSSI = 0x02,
            BatteryLevel = 0x03,
            LowBattery = 0x04,
            ModelNumber = 0x07,
            SerialNumber = 0x08,
            ESN = 0x09,
            RadioID = 0x0E,
            RFPATemp = 0x1D,
            
        }

        public enum SoftpotOperation : byte
        {
            Read = 0x00,
            Write = 0x01,
            Update = 0x02,
            ReadAll = 0x03,
            WriteAll = 0x04,
            Autotune = 0x05,
            ReadMin = 0x06,
            ReadMax = 0x07,
            ReadFrequency = 0x08,
        }

        public MotorolaXCMPRadioBase(IXCMPRadioConnection conn)
        {
            Name = "";
            SerialNumber = "";
            FirmwareVersion = "";
            InfoHeader = "";
            _connection = conn;

        }
        public void Connect(bool underTest = false)
        {
            _connection.Connect();
            if (!underTest)
            {
                SerialNumber = System.Text.Encoding.UTF8.GetString(GetStatus(StatusOperation.SerialNumber)).TrimEnd('\0');
                ModelNumber = System.Text.Encoding.UTF8.GetString(GetStatus(StatusOperation.ModelNumber)).TrimEnd('\0');
            }
        }

        public void Disconnect()
        {
            _connection.Disconnect();
        }

        public byte[] Send(byte[] data)
        {
            int opcodeOut = 0;
            opcodeOut |= (data[0] << 8);
            opcodeOut |= (data[1] & 0xFF);

            // expects to get an XCMP opcode and some data in, length is auto calculated
            byte[] toSend = new byte[data.Length + 2];

            int dataLen = data.Length;

            // length high and low bytes
            toSend[0] = (byte)((dataLen >> 8) & 0xFF);
            toSend[1] = (byte)(dataLen & 0xFF);

            Array.Copy(data, 0, toSend, 2, dataLen);

            //Console.WriteLine("Sending " + Convert.ToHexString(toSend));

            _connection.Send(toSend);

            // start a timer so we don't hold infinitely
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(5))
            {
                byte[] fromRadio = _connection.Receive();
                int len = 0;

                len |= (fromRadio[0] << 8) & 0xFF;
                len |= fromRadio[1];

                byte[] retval = new byte[len];

                Array.Copy(fromRadio, 2, retval, 0, len);

                int opcodeIn = 0;
                opcodeIn |= (retval[0] << 8);
                opcodeIn |= (retval[1] & 0xFF);

                if (opcodeIn - 0x8000 == opcodeOut)
                {
                    return retval;
                }
            }
            throw new TimeoutException("Radio did not reply in a timely manner.");
        }

        public byte[] GetVersion(VersionOperation oper)
        {
            byte[] cmd = new byte[3];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0f;

            // the power index
            cmd[2] = (byte)oper;

            return Send(cmd);
        }

        public byte[] GetStatus(StatusOperation oper)
        {
            byte[] cmd = new byte[3];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0e;

            // the status byte
            cmd[2] = (byte)oper;

            byte[] result = Send(cmd);

            byte[] returnVal = new byte[result.Length - 4];

            //Console.WriteLine("Length is " + returnVal.Length);

            Array.Copy(result, 4, returnVal, 0, result.Length - 4);

            return returnVal;
        }

        public void SetPowerLevel(int powerIndex)
        {
            byte[] cmd = new byte[3];
            
            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x06;
            
            // the power index
            cmd[2] = (byte)powerIndex;

            Send(cmd);
        }

        public void EnterServiceMode()
        {
            byte[] cmd = new byte[2];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0c;

            Send(cmd);
        }

        public void ResetRadio()
        {
            byte[] cmd = new byte[2];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0d;

            Send(cmd);
        }

        public void SetTXFrequency(int frequency, bool modulated)
        {
            // divide by 5 to fit in XCMP opcode
            frequency = frequency / 5;
            byte[] cmd = new byte[8];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0b;

            // frequency
            cmd[2] = (byte) ((frequency >> 24) & 0xFF);
            cmd[3] = (byte) ((frequency >> 16) & 0xFF);
            cmd[4] = (byte) ((frequency >> 8) & 0xFF);
            cmd[5] = (byte) (frequency & 0xFF);

            // bw
            cmd[6] = 0x64;

            // modulated yes/no
            cmd[7] = Convert.ToByte(modulated);

            Send(cmd);
        }

        public void SetRXFrequency(int frequency, bool modulated)
        {
            // divide by 5 to fit in XCMP opcode
            frequency = frequency / 5;
            byte[] cmd = new byte[8];

            // XCMP opcode
            cmd[0] = 0x00;
            cmd[1] = 0x0a;

            // frequency
            cmd[2] = (byte)((frequency >> 24) & 0xFF);
            cmd[3] = (byte)((frequency >> 16) & 0xFF);
            cmd[4] = (byte)((frequency >> 8) & 0xFF);
            cmd[5] = (byte)(frequency & 0xFF);

            // bw
            cmd[6] = 0x64;

            // modulated yes/no
            cmd[7] = Convert.ToByte(modulated);

            Send(cmd);
        }

        public void Keyup()
        {
            byte[] cmd = new byte[3];

            // transmit opcode
            cmd[0] = 0x00;
            cmd[1] = 0x04;

            cmd[2] = 0x03;

            Send(cmd);
        }

        public void Dekey()
        {
            byte[] cmd = new byte[3];

            // receive opcode
            cmd[0] = 0x00;
            cmd[1] = 0x05;

            cmd[2] = 0x11;

            Send(cmd);
        }

        public void SoftpotRead(int id)
        {
            byte[] cmd = new byte[4];

            // receive opcode
            cmd[0] = 0x00;
            cmd[1] = 0x01;

            cmd[2] = 0x00;

            cmd[3] = (byte) id;

            Send(cmd);
        }
        public void SoftpotWrite(int id, int val)
        {
            throw new NotImplementedException();
        }

        public void SoftpotUpdate(int id, int val)
        {
            byte[] cmd = new byte[6];

            // receive opcode
            cmd[0] = 0x00;
            cmd[1] = 0x01;

            cmd[2] = 0x02;

            cmd[3] = (byte)id;

            cmd[4] = (byte)((byte) (val >> 8) & 0xFF);
            cmd[5] = (byte) (val & 0xFF);

            Send(cmd);
        }

        public virtual int[] GetTXPowerPoints()
        {
            throw new NotImplementedException();
        }

        public void SetTransmitConfig(XCMPRadioTransmitOption option)
        {
            byte[] cmd = new byte[3];

            // transmit config opcode
            cmd[0] = 0x00;
            cmd[1] = 0x02;

            cmd[2] = (byte)option;

            Send(cmd);
        }

        public void SetReceiveConfig(XCMPRadioReceiveOption option)
        {
            byte[] cmd = new byte[3];

            // receive config opcode
            cmd[0] = 0x00;
            cmd[1] = 0x03;

            cmd[2] = (byte)option;

            Send(cmd);

            cmd = new byte[3];

            // receive opcode
            cmd[0] = 0x00;
            cmd[1] = 0x05;
            cmd[2] = 0x01;

            Send(cmd);
        }

        public virtual MotorolaBand[] GetBands()
        {
            throw new NotImplementedException();
        }

    }
}

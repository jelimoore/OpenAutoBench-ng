using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;
using System.Net.Sockets;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.XPR
{
    public class MotorolaXNLConnection : IXCMPRadioConnection
    {
        private IXCMPRadioConnection _conn;

        private int[] AuthKey;
        private int AuthDelta;
        private int AuthIndex;

        private int _flag;
        private int _transId;
        private byte[] _xnlAddr;
        public MotorolaXNLConnection(IXCMPRadioConnection lower, int[] authKey, int authDelta, int authIndex)
        {
            _conn = lower;
            _xnlAddr = new byte[2];

            AuthKey = new int[4];
            Array.Copy(authKey, AuthKey, 4);
            AuthDelta = authDelta;
            AuthIndex = authIndex;
        }

        public void Dispose()
        {
            _conn.Dispose();
        }
        public void Connect()
        {
            _conn.Connect();
            // this is all horrible, and i intend to redo it at some point
            // mainly just ported this 1:1 from my old python libs

            /*
             * MASTER STATUS BROADCAST
             */
            byte[] resp = _conn.Receive();

            //TODO: verify the opcode is correct

            /*
             * AUTH KEY REQUEST
             */
            byte[] cmd = new byte[14];

            // length
            cmd[0] = 0x00;
            cmd[1] = 0x0c;

            // opcode
            cmd[2] = 0x00;
            cmd[3] = 0x04;

            // ??????
            cmd[4] = 0x00;
            cmd[5] = 0x00;
            cmd[6] = 0x00;
            cmd[7] = 0x06;
            cmd[8] = 0x00;
            cmd[9] = 0x00;
            cmd[10] = 0x00;
            cmd[11] = 0x00;
            cmd[12] = 0x00;
            cmd[13] = 0x00;

            _conn.Send(cmd);

            /*
             * AUTH KEY REPLY
             */

            resp = _conn.Receive();

            //TODO: check opcode

            // temp address
            byte[] tempAddr = new byte[2];
            Array.Copy(resp, 14, tempAddr, 0, 2);

            // auth challenge
            byte[] authChallenge = new byte[8];
            Array.Copy(resp, 16, authChallenge, 0, 8);
            byte[] authReply = GenerateKey(authChallenge);

            /*
             * CONNECTION REQUEST
             */

            cmd = new byte[26];

            // len
            cmd[0] = 0x00;
            cmd[1] = 0x18;

            // opcode
            cmd[2] = 0x00;
            cmd[3] = 0x06;

            // ??
            cmd[4] = 0x00;
            cmd[5] = 0x00;
            cmd[6] = 0x00;
            cmd[7] = 0x06;

            // temp addr
            cmd[8] = tempAddr[0];
            cmd[9] = tempAddr[1];

            // ??
            cmd[10] = 0x00;
            cmd[11] = 0x00;
            cmd[12] = 0x00;
            cmd[13] = 0x0c;
            cmd[14] = 0x00;
            cmd[15] = 0x00;
            cmd[16] = 0x0a;

            // key ID
            cmd[17] = (byte)AuthIndex;

            // auth response
            Array.Copy(authReply, 0, cmd, 18, 8);

            _conn.Send(cmd);

            /*
             * CONNECTION REPLY
             */

            resp = _conn.Receive();

            Array.Copy(resp, 16, _xnlAddr, 0, 2);

            if (resp[14] != 0x01)
            {
                Dispose();
                throw new Exception("Key was invalid");
            }

            /*
             * SYSMAP BROADCAST
             */
            resp = _conn.Receive();

            /*
             * BROADCAST
             */

            Receive();
        }

        public void Disconnect()
        {
            _conn.Disconnect();
        }

        public byte[] Receive()
        {
            var end = DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(5));
            while (DateTimeOffset.UtcNow < end)
            {
                byte[] resp = _conn.Receive();
                if (resp[3] == 0x0b)
                {
                    byte[] messId = new byte[2];
                    byte flag = resp[5];
                    Array.Copy(resp, 10, messId, 0, 2);

                    Console.WriteLine("Have data");

                    // send ack
                    byte[] cmd = new byte[14];

                    // length
                    cmd[0] = 0x00;
                    cmd[1] = 0x0c;

                    // opcode
                    cmd[2] = 0x00;
                    cmd[3] = 0x0c;

                    // format
                    cmd[4] = 0x01;

                    // flag
                    cmd[5] = flag;

                    // dest addr
                    cmd[6] = 0x00;
                    cmd[7] = 0x06;

                    // src addr
                    cmd[8] = _xnlAddr[0];
                    cmd[9] = _xnlAddr[1];

                    // message ID
                    cmd[10] = messId[0];
                    cmd[11] = messId[1];

                    cmd[12] = 0x00;
                    cmd[13] = 0x00;

                    _conn.Send(cmd);

                    //incrVars();

                    byte[] result = new byte[resp.Length - 14];

                    Array.Copy(resp, 12, result, 0, result.Length);
                    return result;
                }
            }
            throw new Exception("Timeout while receiving");

        }

        public void Send(byte[] data)
        {
            byte[] cmd = new byte[data.Length + 12];

            // len
            int len = data.Length + 10;
            cmd[0] = (byte)((len >> 8) & 0xFF);
            cmd[1] = (byte)(len & 0xFF);

            // opcode
            cmd[2] = 0x00;
            cmd[3] = 0x0b;

            // format
            cmd[4] = 0x01;

            // flag
            cmd[5] = (byte) _flag;

            // dest addr
            cmd[6] = 0x00;
            cmd[7] = 0x06;

            // XNL address
            cmd[8] = _xnlAddr[0];
            cmd[9] = _xnlAddr[1];

            cmd[10] = 0x00;
            cmd[11] = (byte) _transId;

            Array.Copy(data, 0, cmd, 12, data.Length);

            _conn.Send(cmd);
            incrVars();

            // msg ack
            _conn.Receive();
        }

        public byte[] GenerateKey(byte[] challenge)
        {
            UInt32 dword1 = ArrayToInt(challenge, 0);
            UInt32 dword2 = ArrayToInt(challenge, 4);

            UInt32 sum = 0;
            UInt32 _authDelta = (uint)AuthDelta;
            UInt32 num1 = (uint)AuthKey[0];
            UInt32 num2 = (uint)AuthKey[1];
            UInt32 num3 = (uint)AuthKey[2];
            UInt32 num4 = (uint)AuthKey[3];

            for (int index = 0; index < 32; ++index)
            {
                sum += _authDelta;
                dword1 += (uint)(((int)dword2 << 4) + (int)num1 ^ (int)dword2 + (int)sum ^ (int)(dword2 >> 5) + (int)num2);
                dword2 += (uint)(((int)dword1 << 4) + (int)num3 ^ (int)dword1 + (int)sum ^ (int)(dword1 >> 5) + (int)num4);
            }
            byte[] res = new byte[8];
            IntToArray(dword1, res, 0);
            IntToArray(dword2, res, 4);
            return res;
        }

        private UInt32 ArrayToInt(byte[] data, int start)
        {
            UInt32 ret = 0;
            for (int i = 0; i < 4; i++)
            {
                ret = ret << 8 | data[i + start];
            }
            return ret;
        }

        private static void IntToArray(UInt32 i, byte[] data, int start)
        {
            for (int index = 0; index < 4; ++index)
            {
                data[start + 3 - index] = (byte)(i & (uint)byte.MaxValue);
                i >>= 8;
            }
        }

        private void incrVars()
        {
            _flag++;

            if (_flag > 7)
            {
                _flag = 0;
            }

            _transId++;

            if (_transId > 255)
            {
                _transId = 0;
            }
        }
    }
}

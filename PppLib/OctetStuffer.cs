using PppLib.Protocols;
using System;
using System.Collections.Generic;

namespace PppLib
{
    internal static class OctetStuffer
    {
        private static bool[] s_asyncControlCharacterMap = new bool[32] {
            true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true,
        };

        public static byte[] Stuff(byte[] data)
        {
            List<byte> stuffed = new List<byte>();
            stuffed.Add(data[0]);

            for (int i = 1; i < data.Length - 1; i++)
            {
                if ((data[i] < 0x20 && s_asyncControlCharacterMap[data[i]]) || data[i] == 0x7d || data[i] == 0x7e)
                {
                    stuffed.Add(0x7d);
                    stuffed.Add((byte)(data[i] ^ 0x20));
                }
                else
                {
                    stuffed.Add(data[i]);
                }
            }

            stuffed.Add(data[data.Length - 1]);
            return stuffed.ToArray();
        }

        public static byte[] Unstuff(byte[] data)
        {
            List<byte> unstuffed = new List<byte>();
            unstuffed.Add(data[0]);

            for (int i = 1; i < data.Length - 1; i++)
            {
                if (data[i] == 0x7d)
                {
                    i++;
                    unstuffed.Add((byte)(data[i] ^ 0x20));
                }
                else
                {
                    unstuffed.Add(data[i]);
                }
            }

            unstuffed.Add(data[data.Length - 1]);
            return unstuffed.ToArray();
        }

        public static void DisableStuffing()
        {
            for (int i = 0; i < s_asyncControlCharacterMap.Length; i++)
            {
                s_asyncControlCharacterMap[i] = false;
            }
        }
    }
}

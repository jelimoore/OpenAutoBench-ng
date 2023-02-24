using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAutoBench_ng.UnitTests.RadioTests.MotorolaXCMPBase
{
    internal class XCMPRadioTestInterface : IXCMPRadioConnection
    {
        private byte[] inBuffer;
        private byte[] outBuffer;
        public void Connect()
        {
            //
        }

        public void Disconnect()
        {
            //
        }

        public void Dispose()
        {
            //
        }

        public void Send(byte[] toSend)
        {
            outBuffer = toSend;
        }

        public byte[] Receive()
        {
            return inBuffer;
        }
        /// <summary>
        /// From perspective of the OAB tool, set the contents of the buffer for incoming data to the program
        /// </summary>
        /// <param name="buf"></param>
        internal void SetInBuffer(byte[] buf)
        {
            inBuffer = new byte[buf.Length];
            Array.Copy(buf, inBuffer, buf.Length);
        }
        /// <summary>
        /// From perspective of the OAB tool, read the contents of the buffer that holds the data sent by the program.
        /// </summary>
        /// <param name="buf"></param>
        internal byte[] ReadOutBuffer()
        {
            return outBuffer;
        }

        internal void FlushBuffers()
        {
            inBuffer = new byte[0];
            outBuffer = new byte[0];
        }
    }
}

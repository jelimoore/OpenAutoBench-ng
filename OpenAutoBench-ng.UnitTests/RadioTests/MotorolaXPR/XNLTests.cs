
using OpenAutoBench_ng.Communication.Radio.Motorola.XPR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAutoBench_ng.UnitTests.RadioTests.MotorolaXPR
{
    internal class XNLTests
    {
        [SetUp]
        public void Setup()
        {
            //
        }
        /*
        [Test]
        public void TestXNLEncrypt()
        {
            MotorolaXNLConnection conn = new MotorolaXNLConnection(null);
            byte[] challenge = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xab, 0xcd, 0xef };
            byte[] reference = { 0xd1, 0x8d, 0x6b, 0x90, 0xd8, 0x13, 0x1b, 0x5d };
            byte[] test = conn.GenerateKey(challenge);
            Assert.AreEqual(reference, test);

        }
        */
    }
}

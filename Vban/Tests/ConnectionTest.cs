using System.Net;
using System.Threading;
using NUnit.Framework;

namespace Vban.Tests
{
    [TestFixture]
    public class ConnectionTest
    {
        [Test]
        public void MainTest()
        {
            VBANStream<string> vban = VBAN.OpenTextStream(IPAddress.Loopback, null);

            vban.Write("bus(0).mute=1".ToCharArray());
            vban.Flush();
            Thread.Sleep(500);
            vban.SendData("bus(0).mute=0");
            Thread.Sleep(500);
        }
    }
}
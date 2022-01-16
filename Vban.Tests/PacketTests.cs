using System.Collections.Generic;
using NUnit.Framework;
using Vban.Model;

namespace Vban.Tests
{
    // ReSharper disable once InconsistentNaming
    public class VBANPacketTest
    {
        private VBANPacket<string>.Factory<string, string> _factory;

        [SetUp]
        public void Setup()
        {
            var builder = VBANPacketHead<string>.Factory<string, IEnumerable<char>>
                .CreateBuilder(VBAN.Protocol<string>.Text);

            builder.SampleRate = VBAN.BitsPerSecond.Bps150;
            builder.Format = VBAN.CommandFormat.Utf8;
        }
    }
}
using System;
using NUnit.Framework;
using Vban.Model;

namespace Vban.Tests
{
    [TestFixture]
    public class UtilsTest
    {
        private UnfinishedByteArray _array;

        [SetUp]
        public void Setup()
        {
            _array = new UnfinishedByteArray(2);

            _array.Append( 32, 41);
        }

        [Test]
        public void TestAppending()
        {
            Assert.AreEqual(2, _array.Length);
            Assert.AreEqual(2, _array.BufferSize);

            _array.Append(24, 64);

            Assert.AreEqual(4, _array.Length);
            Assert.AreEqual(6, _array.BufferSize);

            Assert.AreEqual(new byte[]{32, 41, 24, 64}, _array.Bytes);
        }

        [Test]
        public void TestNullAppending()
        {
            Assert.Throws<NullReferenceException>(() => _array.Append(null));
        }

        [Test]
        public void TestFixedSize()
        {
            UnfinishedByteArray fixedSize = new UnfinishedByteArray(2, true);

            fixedSize.Append(72, 19);

            Assert.Throws<IndexOutOfRangeException>(() => fixedSize.Append(15));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace GitNet.Tests
{
    [TestFixture]
    public class ByteArrayStreamConvertExtensionsTests
    {
        [Test]
        public void ToByteArrayInvertsToStream()
        {
            byte[] start8192 = new byte[] { 1, 2, 3, 4, 5, 6 };
            byte[] end8192 = start8192.ToStream().ToByteArray();

            Assert.AreEqual(start8192, end8192);

            byte[] start1 = new byte[] { 1, 2, 3, 4, 5, 6 };
            byte[] end1 = start1.ToStream(1).ToByteArray(1);

            Assert.AreEqual(start1, end1);
        }
    }
}

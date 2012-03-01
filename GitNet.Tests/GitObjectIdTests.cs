using System;
using NUnit.Framework;

namespace GitNet.Tests
{
    [TestFixture]
    public class GitObjectIdTests
    {
        [Test]
        public void ZeroIsValid()
        {
            GitObjectId goi = GitObjectId.Zero;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnsuresRawNotNull()
        {
            GitObjectId goi = new GitObjectId((byte[])null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnsuresShaNotNull()
        {
            GitObjectId goi = new GitObjectId((string)null);
        }

        [Test]
        public void EnsuresRawLength()
        {
            try
            {
                GitObjectId goi = new GitObjectId(new byte[20]);
            }
            catch
            {
                Assert.Fail("Should not throw an exception");
            }

            try
            {
                GitObjectId goi = new GitObjectId(new byte[19]);
                Assert.Fail("Should throw an exception");
            }
            catch
            {
            }

            try
            {
                GitObjectId goi = new GitObjectId(new byte[21]);
                Assert.Fail("Should throw an exception");
            }
            catch
            {
            }
        }

        [Test]
        public void EnsuresShaLength()
        {
            try
            {
                GitObjectId goi = new GitObjectId(new string('0', 40));
            }
            catch
            {
                Assert.Fail("Should not throw an exception");
            }

            try
            {
                GitObjectId goi = new GitObjectId(new string('0', 39));
                Assert.Fail("Should throw an exception");
            }
            catch
            {
            }

            try
            {
                GitObjectId goi = new GitObjectId(new string('0', 41));
                Assert.Fail("Should throw an exception");
            }
            catch
            {
            }
        }

        [Test]
        public void EnsuresShaFormat()
        {
            try
            {
                GitObjectId goi = new GitObjectId("0123456789abcdef0123456789abcdef01234567");
            }
            catch (Exception)
            {
                Assert.Fail("Should not throw an exception");
            }

            try
            {
                GitObjectId goi = new GitObjectId("0123456789abcdef0123456789abcdef0123456" + (char)('0' + 1));
                Assert.Fail("Should throw an exception");
            }
            catch (Exception)
            {
            }

            try
            {
                GitObjectId goi = new GitObjectId("0123456789abcdef0123456789abcdef0123456" + (char)('9' + 1));
                Assert.Fail("Should throw an exception");
            }
            catch (Exception)
            {
            }

            try
            {
                GitObjectId goi = new GitObjectId("0123456789abcdef0123456789abcdef0123456" + (char)('a' - 1));
                Assert.Fail("Should throw an exception");
            }
            catch (Exception)
            {
            }

            try
            {
                GitObjectId goi = new GitObjectId("0123456789abcdef0123456789abcdef0123456" + (char)('f' + 1));
                Assert.Fail("Should throw an exception");
            }
            catch (Exception)
            {
            }
        }

        [Test]
        public void ConvertsCorrectlyBetweenRawAndSha()
        {
            GitObjectId goi1 = new GitObjectId(new byte[] { 128, 64, 32, 16, 8, 4, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255 });

            Assert.AreEqual("80402010080402010000000000000000000000ff", goi1.Sha);

            GitObjectId goi2 = new GitObjectId("00008040201008040201000000000000000000ff");

            Assert.AreEqual(new byte[] { 0, 0, 128, 64, 32, 16, 8, 4, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255 }, goi2.Raw);
        }

        [Test]
        public void ComparesCorrectly()
        {
            GitObjectId goi1 = GitObjectId.Zero;
            GitObjectId goi2 = GitObjectId.Zero;

            Assert.AreNotSame(goi1, goi2);
            Assert.AreEqual(goi1, goi2);
            Assert.IsTrue(goi1 == goi2);
            Assert.IsFalse(goi1 != goi2);

            GitObjectId goi3 = new GitObjectId(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 });

            Assert.AreNotSame(goi1, goi3);
            Assert.AreNotEqual(goi1, goi3);
            Assert.IsFalse(goi1 == goi3);
            Assert.IsTrue(goi1 != goi3);
        }
    }
}
using Azure.Mobile.Server.Utils;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Azure.Mobile.Server.Test.Utils
{
    [TestClass]
    public class ETag_Test
    {
        #region ETag.Matches
        [TestMethod]
        public void Matches_AisWeak_False()
        {
            var a = new EntityTagHeaderValue("\"tag-a\"", true);
            var b = new EntityTagHeaderValue("\"tag-a\"", false);
            Assert.IsFalse(ETag.Matches(a, b));
        }

        [TestMethod]
        public void Matches_BisWeak_False()
        {
            var a = new EntityTagHeaderValue("\"tag-a\"", false);
            var b = new EntityTagHeaderValue("\"tag-a\"", true);
            Assert.IsFalse(ETag.Matches(a, b));
        }

        [TestMethod]
        public void Matches_AStar_True()
        {
            var a = new EntityTagHeaderValue("*");
            var b = new EntityTagHeaderValue("\"tag-a\"");
            Assert.IsTrue(ETag.Matches(a, b));
        }

        [TestMethod]
        public void Matches_BStar_True()
        {
            var b = new EntityTagHeaderValue("*");
            var a = new EntityTagHeaderValue("\"tag-a\"");
            Assert.IsTrue(ETag.Matches(a, b));
        }

        [TestMethod]
        public void Matches_AB_True()
        {
            var a = new EntityTagHeaderValue("\"tag-a\"");
            var b = new EntityTagHeaderValue("\"tag-a\"");
            Assert.IsTrue(ETag.Matches(a, b));
        }

        [TestMethod]
        public void Matches_AnotB_True()
        {
            var a = new EntityTagHeaderValue("\"tag-a\"");
            var b = new EntityTagHeaderValue("\"tag-b\"");
            Assert.IsFalse(ETag.Matches(a, b));
        }
        #endregion ETag.Matches

        #region ETag.FromByteArray
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FromByteArray_Null_ThrowsArgumentNullException()
        {
            var actual = ETag.FromByteArray(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public void FromByteArray_Empty_ReturnsEmptyString()
        {
            var actual = ETag.FromByteArray(new byte[] { });
            var expected = "\"\"";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FromByteArray_ValidData_ReturnsExpected()
        {
            var testData = new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F };
            var actual = ETag.FromByteArray(testData);
            var expected = "\"AQBCIkeP\"";
            Assert.AreEqual(expected, actual);
        }
        #endregion
    }
}

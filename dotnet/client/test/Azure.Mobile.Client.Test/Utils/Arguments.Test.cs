using Azure.Mobile.Client.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Mobile.Client.Test.Utils
{
    [TestClass]
    public class Arguments_Tests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsNotNull_NullThrows()
        {
            long? actual = null;

            Arguments.IsNotNull(actual, nameof(actual));
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNotNullOrEmpty_EmptyThrows()
        {
            string actual = "";
            Arguments.IsNotNullOrEmpty(actual, nameof(actual));
            Assert.Fail("ArgumentException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsAbsoluteUri_RelativeUriThrows()
        {
            Uri abs = new Uri("https://localhost");
            Uri rel = new Uri("https://localhost/relative");
            Arguments.IsAbsoluteUri(rel.MakeRelativeUri(abs), nameof(rel));
            Assert.Fail("ArgumentException expected");
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Zumo.MobileData.Internal;
using System;

namespace Microsoft.Zumo.MobileData.Test.Internal
{
    [TestClass]
    public class Arguments_Tests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsNotNull_NullArgument_Throws()
        {
            long? actual = null;

            Arguments.IsNotNull(actual, nameof(actual));
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public void IsNotNull_NonNullArgument_DoesNotThrow()
        {
            long? actual = 4L;

            Arguments.IsNotNull(actual, nameof(actual));
            Assert.AreEqual(4L, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsNotNullOrEmpty_Null_Throws()
        {
            string actual = null;

            Arguments.IsNotNullOrEmpty(actual, nameof(actual));
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNotNullOrEmpty_Empty_Throws()
        {
            string actual = "";
            Arguments.IsNotNullOrEmpty(actual, nameof(actual));
            Assert.Fail("ArgumentException expected");
        }

        [TestMethod]
        public void IsNotNullOrEmpty_Something_DoesNotThrow()
        {
            string actual = "a";
            Arguments.IsNotNullOrEmpty(actual, nameof(actual));
            Assert.AreEqual("a", actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsAbsoluteUri_RelativeUri_Throws()
        {
            Uri abs = new Uri("https://localhost");
            Uri rel = new Uri("https://localhost/relative");
            Arguments.IsAbsoluteUri(rel.MakeRelativeUri(abs), nameof(rel));
            Assert.Fail("ArgumentException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsAbsoluteUri_Null_Throws()
        {
            Uri abs = null;
            Arguments.IsAbsoluteUri(abs, nameof(abs));
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public void IsAbsoluteUri_AbsoluteUri_DoesNotThrow()
        {
            Uri abs = new Uri("https://localhost");
            Arguments.IsAbsoluteUri(abs, nameof(abs));
            Assert.IsTrue(abs.IsAbsoluteUri);
        }
    }
}

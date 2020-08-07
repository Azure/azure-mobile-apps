// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Zumo.MobileData.Internal;
using System;

namespace Microsoft.Zumo.MobileData.Test
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

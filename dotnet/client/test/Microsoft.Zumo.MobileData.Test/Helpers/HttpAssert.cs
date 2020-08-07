// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Zumo.MobileData.Test.Helpers
{
    internal static class HttpAssert
    {
        internal static void HeaderIsEqual(string headerName, string headerValue, Request request)
        {
            string actual;
            var isPresent = request.Headers.TryGetValue(headerName, out actual);
            Assert.IsTrue(isPresent);
            Assert.IsNotNull(actual);
            Assert.AreEqual(headerValue, actual);
        }
    }
}

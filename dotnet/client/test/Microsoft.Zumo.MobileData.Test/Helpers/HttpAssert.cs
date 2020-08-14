// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Microsoft.Zumo.MobileData.Test.Helpers
{
    internal static class HttpAssert
    {
        internal static void HeaderIsEqual(string headerName, string headerValue, Request request)
        {
            var isPresent = request.Headers.TryGetValue(headerName, out string actual);
            Assert.IsTrue(isPresent, $"Expected header {headerName} to be present in response");
            Assert.IsTrue(actual != null, $"Value of header {headerName} is null");
            Assert.IsTrue(actual.Equals(headerValue), $"Expected {headerName} value to be \"{headerValue}\", Actual \"{actual}\"");
        }

        internal static void HeaderIsEqual(string headerName, string headerValue, HttpRequestMessage request)
        {
            var isPresent = request.Headers.TryGetValues(headerName, out IEnumerable<string> values);
            Assert.IsTrue(isPresent, $"Expected header {headerName} to be present in response");
            var actual = values.First();
            Assert.IsTrue(actual != null, $"Value of header {headerName} is null");
            Assert.IsTrue(actual.Equals(headerValue), $"Expected {headerName} value to be \"{headerValue}\", Actual \"{actual}\"");
        }

        internal static void HeaderIsNotPresent(string headerName, Request request)
        {
            var isPresent = request.Headers.Contains(headerName);
            Assert.IsFalse(isPresent, $"Expected header {headerName} is present, but not expected");
        }

        internal static void HeaderIsNotPresent(string headerName, HttpRequestMessage request)
        {
            var isPresent = request.Headers.Contains(headerName);
            Assert.IsFalse(isPresent, $"Expected header {headerName} is present, but not expected");
        }

        internal static void HeaderStartsWith(string expected, string headerName, HttpRequestMessage request)
        {
            var isPresent = request.Headers.TryGetValues(headerName, out IEnumerable<string> values);
            Assert.IsTrue(isPresent, $"Expected header {headerName} to be present in response");
            var actual = values.First();
            Assert.IsTrue(actual != null, $"Value of header {headerName} is null");
            Assert.IsTrue(actual.StartsWith(expected), $"Expected {headerName} value to start with \"{expected}\", Actual \"{actual}\"");

        }
    }
}

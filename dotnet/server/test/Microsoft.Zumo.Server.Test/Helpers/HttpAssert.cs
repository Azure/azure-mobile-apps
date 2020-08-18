// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Microsoft.Zumo.Server.Test.Helpers
{
    /// <summary>
    /// Assertions dealing with a HTTP Request or Response.
    /// </summary>
    public static class HttpAssert
    {
        public static void HasResponseHeader(HttpContext context, string headerName, string headerValue)
        {
            bool hasHeader = context.Response.Headers.TryGetValue(headerName, out StringValues headerValues);
            Assert.IsTrue(hasHeader, $"Header {headerName} is not present in the response");
            Assert.AreEqual(1, headerValues.Count, $"There are {headerValues.Count} {headerName} header(s) in the response (expected: 1)");
            Assert.AreEqual(headerValue, headerValues.First(), $"Expected <{headerValue}> for header {headerName}, Actual <{headerValues.First()}>");
        }
    }
}

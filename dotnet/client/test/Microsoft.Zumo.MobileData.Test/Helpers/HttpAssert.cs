// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using System.Diagnostics;

namespace Microsoft.Zumo.MobileData.Test.Helpers
{
    internal static class HttpAssert
    {
        internal static void HeaderIsEqual(string headerName, string headerValue, Request request)
        {
            var isPresent = request.Headers.TryGetValue(headerName, out string actual);
            Debug.Assert(isPresent, $"Expected header {headerName} to be present in response");
            Debug.Assert(actual != null, $"Value of header {headerName} is null");
            Debug.Assert(actual.Equals(headerValue), $"Expected {headerName} value to be \"{headerValue}\", Actual \"{actual}\"");
        }
    }
}

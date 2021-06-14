// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class AssertEx
    {
        public static void HeaderMatches(string headerName, string headerValue, HttpHeaders headers)
        {
            Assert.True(headers.Contains(headerName), $"Headers does not contain {headerName}");
            var actualHeaderValue = headers.GetValues(headerName);
            Assert.Single(actualHeaderValue, headerValue);
        }

        public static void HasValue(string key, string[] expectedValue, IReadOnlyDictionary<string, string[]> headers)
        {
            Assert.True(headers.TryGetValue(key, out string[] actualValue), $"dictionary does not contain key {key}");
            Assert.Equal(expectedValue, actualValue);
        }
    }
}

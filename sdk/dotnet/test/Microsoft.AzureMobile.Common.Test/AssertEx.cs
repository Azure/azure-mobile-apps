// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace Microsoft.AzureMobile.Common.Test
{
    /// <summary>
    /// A set of additional assertions used within the unit test projects.
    /// </summary>
    public static class AssertEx
    {
        public static void CloseTo(DateTimeOffset expected, DateTimeOffset actual, int interval = 1000)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);
            var ms = Math.Abs((expected.Subtract(actual)).TotalMilliseconds);
            Assert.True(ms < interval, $"Date {expected} and {actual} are {ms}ms apart");
        }

        public static void ResponseHasHeader(HttpResponseMessage response, string headerName, string expected)
        {
            var hasHeader = response.Headers.TryGetValues(headerName, out IEnumerable<string> headerValues);
            Assert.True(hasHeader, $"The response does not contain header {headerName}");
            Assert.NotNull(headerValues);
            Assert.True(headerValues.Count() == 1, $"There are {headerValues.Count()} values for header {headerName}");
            Assert.Equal(expected, headerValues.Single());
        }
    }
}

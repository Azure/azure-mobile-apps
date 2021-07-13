// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Datasync.Common.Test
{
    /// <summary>
    /// A set of additional assertions used within the unit test projects.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public static class AssertEx
    {
        /// <summary>
        /// Asserts if the dictionary contains the provided value.
        /// </summary>
        /// <param name="key">the header name</param>
        /// <param name="expected">the header value</param>
        /// <param name="dict">The dictionary to check</param>
        public static void Contains(string key, string expected, IDictionary<string, string> dict)
        {
            Assert.True(dict.TryGetValue(key, out string value), $"Dictionary does not contain key {key}");
            Assert.Equal(expected, value);
        }

        /// <summary>
        /// Asserts if the header dictionary contains the provided value.
        /// </summary>
        /// <param name="key">the header name</param>
        /// <param name="expected">the header value</param>
        /// <param name="headers">The headers</param>
        public static void Contains(string key, string expected, IReadOnlyDictionary<string, IEnumerable<string>> headers)
        {
            Assert.True(headers.TryGetValue(key, out IEnumerable<string> values), $"Dictionary does not contain key {key}");
            Assert.True(values.Count() == 1, $"Dictionary contains multiple values for {key}");
            Assert.Equal(expected, values.Single());
        }

        /// <summary>
        /// Asserts if the two dates are "close" to one another (enough so to be valid in terms of test speed)
        /// timings)
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="interval"></param>
        public static void CloseTo(DateTimeOffset expected, DateTimeOffset actual, int interval = 2000)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);
            var ms = Math.Abs((expected.Subtract(actual)).TotalMilliseconds);
            Assert.True(ms < interval, $"Date {expected} and {actual} are {ms}ms apart");
        }

        /// <summary>
        /// Asserts if the headers contains the specific header provided
        /// </summary>
        /// <param name="response"></param>
        /// <param name="headerName"></param>
        /// <param name="expected"></param>
        public static void ResponseHasHeader(HttpResponseMessage response, string headerName, string expected)
        {
            if (headerName.Equals("Last-Modified", StringComparison.InvariantCultureIgnoreCase) || headerName.StartsWith("Content-", StringComparison.InvariantCultureIgnoreCase))
            {
                HasHeader(response.Content.Headers, headerName, expected);
            }
            else
            {
                HasHeader(response.Headers, headerName, expected);
            }
        }

        /// <summary>
        /// Asserts if the headers contaisn the specific header provided
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="headerName"></param>
        /// <param name="expected"></param>
        public static void HasHeader(HttpHeaders headers, string headerName, string expected)
        {
            Assert.True(headers.TryGetValues(headerName, out IEnumerable<string> values), $"The header does not contain header {headerName}");
            Assert.True(values.Count() == 1, $"There are {values.Count()} values for header {headerName}");
            Assert.Equal(expected, values.Single());
        }

        public static void SystemPropertiesSet(ITableData entity)
        {
            CloseTo(DateTimeOffset.Now, entity.UpdatedAt);
            Assert.NotEmpty(entity.Version);
        }

        public static void SystemPropertiesChanged(ITableData original, ITableData replacement)
        {
            Assert.NotEqual(original.UpdatedAt, replacement.UpdatedAt);
            Assert.NotEqual(original.Version, replacement.Version);
        }

        /// <summary>
        /// Compares the server-side data to the client-side data.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void SystemPropertiesMatch(ITableData expected, ClientTableData actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.UpdatedAt, actual.UpdatedAt);
            Assert.Equal(expected.Deleted, actual.Deleted);
            Assert.Equal(Convert.ToBase64String(expected.Version), actual.Version);
        }

        public static void ResponseHasConditionalHeaders(ITableData expected, HttpResponseMessage response)
        {
            var lastModified = expected.UpdatedAt.ToString(DateTimeFormatInfo.InvariantInfo.RFC1123Pattern);
            ResponseHasHeader(response, HeaderNames.ETag, expected.GetETag());
            ResponseHasHeader(response, HeaderNames.LastModified, lastModified);
        }
    }
}

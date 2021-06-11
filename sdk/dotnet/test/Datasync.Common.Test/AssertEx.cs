// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http;
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
        public static void CloseTo(DateTimeOffset expected, DateTimeOffset actual, int interval = 2000)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);
            var ms = Math.Abs((expected.Subtract(actual)).TotalMilliseconds);
            Assert.True(ms < interval, $"Date {expected} and {actual} are {ms}ms apart");
        }

        public static void ResponseHasHeader(HttpResponseMessage response, string headerName, string expected)
        {
            var hasHeader = (headerName == "Last-Modified")
                ? response.Content.Headers.TryGetValues(headerName, out IEnumerable<string> headerValues)
                : response.Headers.TryGetValues(headerName, out headerValues);
            Assert.True(hasHeader, $"The response does not contain header {headerName}");
            Assert.NotNull(headerValues);
            Assert.True(headerValues.Count() == 1, $"There are {headerValues.Count()} values for header {headerName}");
            Assert.Equal(expected, headerValues.Single());
        }

        public static void SystemPropertiesSet(ITableData entity)
        {
            AssertEx.CloseTo(DateTimeOffset.Now, entity.UpdatedAt);
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

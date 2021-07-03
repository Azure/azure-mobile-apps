// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    [ExcludeFromCodeCoverage]
    internal static class AssertEx
    {
        /// <summary>
        /// Asserts if the header dictionary contains the provided value.
        /// </summary>
        /// <param name="key">The header name</param>
        /// <param name="expected">The header value</param>
        /// <param name="headers">The headers</param>
        internal static void Contains(string key, string expected, HttpHeaders headers)
        {
            Assert.True(headers.TryGetValues(key, out IEnumerable<string> values));
            Assert.Single(values);
            Assert.Equal(expected, values.First());
        }

        /// <summary>
        /// Asserts if the header dictionary contains the provided value.
        /// </summary>
        /// <param name="key">the header name</param>
        /// <param name="expected">the header value</param>
        /// <param name="headers">The headers</param>
        internal static void Contains(string key, string expected, IReadOnlyDictionary<string, IEnumerable<string>> headers)
        {
            Assert.True(headers.TryGetValue(key, out IEnumerable<string> values));
            Assert.Single(values);
            Assert.Equal(expected, values.First());
        }
    }
}

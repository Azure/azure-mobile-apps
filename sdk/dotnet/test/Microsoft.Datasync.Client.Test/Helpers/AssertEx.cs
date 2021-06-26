// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    /// <summary>
    /// A set of extensions to the XUnit Assert class.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public static class AssertEx
    {
        /// <summary>
        /// Assert that the named header contains the values expected.
        /// </summary>
        /// <param name="name">The named header</param>
        /// <param name="expected">The expected values</param>
        /// <param name="headers">The headers</param>
        public static void HasValue(string name, IEnumerable<string> expected, HttpHeaders headers)
        {
            Assert.True(headers.TryGetValues(name, out IEnumerable<string> actual));
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Assert that the named header contains the values expected.
        /// </summary>
        /// <param name="name">The named header</param>
        /// <param name="expected">The expected values</param>
        /// <param name="headers">The headers</param>
        public static void HasValue(string name, IEnumerable<string> expected, IReadOnlyDictionary<string, IEnumerable<string>> headers)
        {
            var enumerable = headers.Where(hdr => hdr.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            Assert.Single(enumerable);
            Assert.Equal(expected, enumerable.First().Value);
        }

        /// <summary>
        /// Assert that the dictionary contains the values expected
        /// </summary>
        /// <param name="key">The key to test</param>
        /// <param name="expected">The expected value</param>
        /// <param name="dict">The dictionary to check</param>
        public static void HasValue(string key, string expected, IDictionary<string, string> dict)
        {
            Assert.NotNull(dict);
            Assert.True(dict.TryGetValue(key, out string value));
            Assert.Equal(expected, value);
        }
    }
}

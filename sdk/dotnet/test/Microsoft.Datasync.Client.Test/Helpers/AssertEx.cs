// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    /// <summary>
    /// A set of additional assertions.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public static class AssertEx
    {
        /// <summary>
        /// Assert if a header-style dictionary contains a specific header.
        /// </summary>
        public static void HasValue(string name, IEnumerable<string> expected, IReadOnlyDictionary<string, IEnumerable<string>> dictionary)
        {
            Assert.True(dictionary.TryGetValue(name, out IEnumerable<string> actual));
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Assert if a header-style dictionary contains a specific header.
        /// </summary>
        public static void HasValue(string name, IEnumerable<string> expected, HttpHeaders headers)
        {
            Assert.True(headers.TryGetValues(name, out IEnumerable<string> actual));
            Assert.Equal(expected, actual);
        }
    }
}

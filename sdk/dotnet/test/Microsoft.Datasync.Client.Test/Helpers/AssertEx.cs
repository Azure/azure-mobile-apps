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
        /// Asserts if the expected header is not present.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expected"></param>
        /// <param name="headers"></param>
        internal static void Contains(string key, string expected, HttpHeaders headers)
        {
            Assert.True(headers.TryGetValues(key, out IEnumerable<string> values));
            Assert.Single(values);
            Assert.Equal(expected, values.First());
        }
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable
{
    [ExcludeFromCodeCoverage]
    public class CountItemsAsync_Tests : BaseOperationTest
    {
        [Fact]
        [Trait("Method", "CountItemsAsync")]
        public async Task CountItemsAsync_RetrievesCount()
        {
            long count = await table.CountItemsAsync("");
            Assert.Equal(MovieCount, count);
        }
    }
}

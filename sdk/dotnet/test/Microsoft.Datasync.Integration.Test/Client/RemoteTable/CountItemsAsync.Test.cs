// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable;

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

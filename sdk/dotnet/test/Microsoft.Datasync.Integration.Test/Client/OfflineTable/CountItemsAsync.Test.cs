// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTable;

[ExcludeFromCodeCoverage]
public class CountItemsAsync_Tests : BaseOperationTest
{
    public CountItemsAsync_Tests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Method", "CountItemsAsync")]
    public async Task CountItemsAsync_RetrievesCount()
    {
        await InitializeAsync(true);

        // Act
        long count = await table!.CountItemsAsync("");

        // Assert
        Assert.Equal(MovieCount, count);
    }
}

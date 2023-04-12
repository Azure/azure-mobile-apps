// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTableOfT;

[ExcludeFromCodeCoverage]
public class RefreshItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_ThrowsOnNull()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.RefreshItemAsync(null));
    }

    [Fact]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_ReturnsOnNullId()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = new ClientMovie { Id = null };
        await table.RefreshItemAsync(item);
        Assert.Null(item.Id);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_ThrowsOnInvalidId(string id)
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = new ClientMovie { Id = id };
        await Assert.ThrowsAsync<ArgumentException>(() => table.RefreshItemAsync(item));
    }

    [Fact]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_MissingItem()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString() };

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => table.RefreshItemAsync(item));
    }

    [Fact]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_WithItem()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = GetSampleMovie<ClientMovie>();
        item.Id = Guid.NewGuid().ToString();
        item.UpdatedAt = DateTimeOffset.UtcNow;
        var json = (JObject)table.ServiceClient.Serializer.Serialize(item);
        store.Upsert("movies", new[] { json });

        var refresher = new ClientMovie { Id = item.Id };

        // Act
        await table.RefreshItemAsync(refresher);

        // Assert
        Assert.Equal<IMovie>(item, refresher);
    }
}

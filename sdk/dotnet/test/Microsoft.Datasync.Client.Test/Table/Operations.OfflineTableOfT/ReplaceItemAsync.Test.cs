// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTableOfT;

[ExcludeFromCodeCoverage]
public class ReplaceItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsOnNull()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.ReplaceItemAsync(null)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsOnNullId()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = new ClientMovie { Id = null };
        await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(item)).ConfigureAwait(false);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsOnInvalidId(string id)
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = new ClientMovie { Id = id };
        await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(item)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Works()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = GetSampleMovie<ClientMovie>();
        item.Id = Guid.NewGuid().ToString();
        var json = (JObject)table.ServiceClient.Serializer.Serialize(item);
        store.Upsert("movies", new[] { json });

        item.Title = "New Title";

        // Act
        await table.ReplaceItemAsync(item).ConfigureAwait(false);

        // Assert
        Assert.Equal("New Title", store.TableMap["movies"][item.Id].Value<string>("title"));
    }
}

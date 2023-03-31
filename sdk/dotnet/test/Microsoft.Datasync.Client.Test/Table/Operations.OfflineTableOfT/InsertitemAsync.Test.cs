// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Offline;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTableOfT;

[ExcludeFromCodeCoverage]
public class InsertItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_ThrowsOnNull()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.InsertItemAsync(null));
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_Success_NoId()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = GetSampleMovie<ClientMovie>();
        var json = (JObject)table.ServiceClient.Serializer.Serialize(item);

        // Act
        await table.InsertItemAsync(item);

        // Assert
        Assert.Single(store.TableMap["movies"]);
        var storedItem = store.TableMap["movies"].Values.First();
        Assert.True(storedItem.ContainsKey("id"));
        Assert.Equal(storedItem.Value<string>("id"), item.Id);
        json["id"] = item.Id;
        AssertEx.JsonEqual(storedItem, json);
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_Success_WithId()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = GetSampleMovie<ClientMovie>();
        item.Id = Guid.NewGuid().ToString();
        var id = item.Id;   // Store for comparison
        var json = (JObject)table.ServiceClient.Serializer.Serialize(item);

        // Act
        await table.InsertItemAsync(item);

        // Assert
        Assert.Single(store.TableMap["movies"]);
        var storedItem = store.TableMap["movies"].Values.First();
        AssertEx.JsonEqual(storedItem, json);
        Assert.True(storedItem.ContainsKey("id"));
        AssertEx.JsonEqual(storedItem, json);
        Assert.Equal(id, storedItem.Value<string>("id"));
        Assert.Equal(id, item.Id);
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_Conflict()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = GetSampleMovie<ClientMovie>();
        item.Id = Guid.NewGuid().ToString();
        var json = (JObject)table.ServiceClient.Serializer.Serialize(item);
        store.Upsert("movies", new[] { json });

        // Act
        await Assert.ThrowsAsync<OfflineStoreException>(() => table.InsertItemAsync(item));
    }
}

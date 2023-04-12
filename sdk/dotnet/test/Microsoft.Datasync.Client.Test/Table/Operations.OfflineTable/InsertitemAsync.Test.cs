// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Offline;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTable;

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
        var response = await table.InsertItemAsync(json);

        // Assert
        Assert.Single(store.TableMap["movies"]);
        var storedItem = store.TableMap["movies"].Values.First();
        AssertEx.JsonEqual(storedItem, response);
        Assert.True(storedItem.ContainsKey("id"));

        var id = storedItem.Value<string>("id");
        json["id"] = id;
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
        var json = (JObject)table.ServiceClient.Serializer.Serialize(item);

        // Act
        var response = await table.InsertItemAsync(json);

        // Assert
        Assert.Single(store.TableMap["movies"]);
        var storedItem = store.TableMap["movies"].Values.First();
        AssertEx.JsonEqual(storedItem, response);
        Assert.True(storedItem.ContainsKey("id"));
        AssertEx.JsonEqual(storedItem, json);
        Assert.Equal(item.Id, storedItem.Value<string>("id"));
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
        await Assert.ThrowsAsync<OfflineStoreException>(() => table.InsertItemAsync(json));
    }
}

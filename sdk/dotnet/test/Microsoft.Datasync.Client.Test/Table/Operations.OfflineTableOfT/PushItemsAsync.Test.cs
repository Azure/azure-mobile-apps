// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTableOfT;

[ExcludeFromCodeCoverage]
public class PushItemsAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_HandlesEmptyQueue_AllTables()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();

        await table.PushItemsAsync();

        Assert.Empty(store.GetOrCreateTable(SystemTables.OperationsQueue));
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_HandlesEmptyQueue_SingleTable()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var op = new DeleteOperation("test", "abc123");
        store.Upsert(SystemTables.OperationsQueue, new[] { op.Serialize() });

        await table.PushItemsAsync();

        Assert.Single(store.TableMap[SystemTables.OperationsQueue]);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_HandlesDeleteOperation_WithVersion()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "abc123" };
        var instance = (JObject)table.ServiceClient.Serializer.Serialize(item);
        store.Upsert("movies", new[] { instance });
        MockHandler.AddResponse(HttpStatusCode.NoContent);

        await table.DeleteItemAsync(instance);
        await table.PushItemsAsync();

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Single(MockHandler.Requests);

        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"/tables/movies/{item.Id}", request.RequestUri.PathAndQuery);
        Assert.Equal($"\"{item.Version}\"", request.Headers.IfMatch.FirstOrDefault()?.Tag);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesDeleteOperation_WithoutVersion()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString() };
        var instance = (JObject)table.ServiceClient.Serializer.Serialize(item);
        store.Upsert("movies", new[] { instance });
        MockHandler.AddResponse(HttpStatusCode.NoContent);

        await table.DeleteItemAsync(instance);

        await table.PushItemsAsync();

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Single(MockHandler.Requests);

        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"/tables/movies/{item.Id}", request.RequestUri.PathAndQuery);
        Assert.Empty(request.Headers.IfMatch);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesDeleteOperation_Conflict()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        var instance = (JObject)table.ServiceClient.Serializer.Serialize(item);
        store.Upsert("movies", new[] { instance });
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await table.DeleteItemAsync(instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => table.PushItemsAsync());

        Assert.Single(MockHandler.Requests);
        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"/tables/movies/{item.Id}", request.RequestUri.PathAndQuery);

        Assert.Equal(PushStatus.Complete, ex.PushResult.Status);
        Assert.Single(ex.PushResult.Errors);
        Assert.Equal("movies", ex.PushResult.Errors.First().TableName);
        Assert.Equal(item.Id, ex.PushResult.Errors.First().Item.Value<string>("id"));

        Assert.Single(store.TableMap[SystemTables.OperationsQueue]);
        var op = TableOperation.Deserialize(store.TableMap[SystemTables.OperationsQueue].Values.First());
        Assert.Equal(TableOperationState.Failed, op.State);

        Assert.Single(store.TableMap[SystemTables.SyncErrors]);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesInsertOperation()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Title = "The Big Test" };
        var returnedItem = item.Clone();
        returnedItem.UpdatedAt = DateTimeOffset.Now;
        returnedItem.Version = "1";
        var expectedContent = $"{{\"bestPictureWinner\":false,\"duration\":0,\"rating\":null,\"releaseDate\":\"0001-01-01T00:00:00.000Z\",\"title\":\"The Big Test\",\"year\":0,\"id\":\"{item.Id}\"}}";
        var instance = (JObject)table.ServiceClient.Serializer.Serialize(item);
        MockHandler.AddResponse(HttpStatusCode.OK, returnedItem);

        await table.InsertItemAsync(instance);
        await table.PushItemsAsync();

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Single(MockHandler.Requests);

        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/tables/movies", request.RequestUri.PathAndQuery);
        Assert.Equal(expectedContent, await request.Content.ReadAsStringAsync());

        Assert.True(store.TableMap["movies"].ContainsKey(item.Id));
        var storedItem = store.TableMap["movies"][item.Id];
        Assert.Equal(storedItem.Value<DateTime>("updatedAt").Ticks, returnedItem.UpdatedAt.Ticks);
        Assert.Equal(storedItem.Value<string>("version"), returnedItem.Version);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesInsertOperation_Conflict()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        var instance = (JObject)table.ServiceClient.Serializer.Serialize(item);
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await table.InsertItemAsync(instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => table.PushItemsAsync());

        Assert.Single(MockHandler.Requests);
        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/tables/movies", request.RequestUri.PathAndQuery);

        Assert.Equal(PushStatus.Complete, ex.PushResult.Status);
        Assert.Single(ex.PushResult.Errors);
        Assert.Equal("movies", ex.PushResult.Errors.First().TableName);
        Assert.Equal(item.Id, ex.PushResult.Errors.First().Item.Value<string>("id"));

        Assert.Single(store.TableMap[SystemTables.OperationsQueue]);
        var op = TableOperation.Deserialize(store.TableMap[SystemTables.OperationsQueue].Values.First());
        Assert.Equal(TableOperationState.Failed, op.State);

        Assert.Single(store.TableMap[SystemTables.SyncErrors]);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesUpdateOperationWithoutVersion()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var itemToUpdate = new ClientMovie { Id = Guid.NewGuid().ToString(), Title = "The Big Test" };
        var instance = table.ServiceClient.Serializer.Serialize(itemToUpdate) as JObject;
        store.Upsert("movies", new[] { instance });

        var updatedItem = itemToUpdate.Clone();
        updatedItem.Title = "Modified";
        var mInstance = table.ServiceClient.Serializer.Serialize(updatedItem) as JObject;

        var returnedItem = itemToUpdate.Clone();
        returnedItem.Version = "2";
        MockHandler.AddResponse(HttpStatusCode.OK, returnedItem);

        await table.ReplaceItemAsync(mInstance);
        await table.PushItemsAsync();

        // Request was a PUT
        Assert.Single(MockHandler.Requests);
        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"/tables/movies/{itemToUpdate.Id}", request.RequestUri.PathAndQuery);
        Assert.Empty(request.Headers.IfMatch);
        var requestObj = JObject.Parse(await request.Content.ReadAsStringAsync());
        AssertEx.JsonEqual(mInstance, requestObj);

        // Queue is empty
        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);

        // Item in the store has been updated.
        Assert.True(store.TableMap["movies"].ContainsKey(itemToUpdate.Id));
        Assert.Equal("2", store.TableMap["movies"][itemToUpdate.Id].Value<string>("version"));
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesUpdateOperation_WithVersion()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var itemToUpdate = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1", Title = "The Big Test" };
        var instance = table.ServiceClient.Serializer.Serialize(itemToUpdate) as JObject;
        store.Upsert("movies", new[] { instance });

        var updatedItem = itemToUpdate.Clone();
        updatedItem.Title = "Modified";
        var mInstance = table.ServiceClient.Serializer.Serialize(updatedItem) as JObject;

        var returnedItem = itemToUpdate.Clone();
        returnedItem.Version = "2";
        MockHandler.AddResponse(HttpStatusCode.OK, returnedItem);

        await table.ReplaceItemAsync(mInstance);
        await table.PushItemsAsync();

        // Request was a PUT
        Assert.Single(MockHandler.Requests);
        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"/tables/movies/{itemToUpdate.Id}", request.RequestUri.PathAndQuery);
        Assert.Equal("\"1\"", request.Headers.IfMatch.First().Tag);
        var requestObj = JObject.Parse(await request.Content.ReadAsStringAsync());
        AssertEx.JsonEqual(mInstance, requestObj);

        // Queue is empty
        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);

        // Item in the store has been updated.
        Assert.True(store.TableMap["movies"].ContainsKey(itemToUpdate.Id));
        Assert.Equal("2", store.TableMap["movies"][itemToUpdate.Id].Value<string>("version"));
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesUpdateOperation_Conflict()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var itemToUpdate = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1", Title = "The Big Test" };
        var instance = table.ServiceClient.Serializer.Serialize(itemToUpdate) as JObject;
        store.Upsert("movies", new[] { instance });

        var updatedItem = itemToUpdate.Clone();
        updatedItem.Title = "Modified";
        var mInstance = table.ServiceClient.Serializer.Serialize(updatedItem) as JObject;

        var returnedItem = itemToUpdate.Clone();
        returnedItem.Version = "2";
        var expectedInstance = table.ServiceClient.Serializer.Serialize(returnedItem) as JObject;
        MockHandler.AddResponse(HttpStatusCode.Conflict, returnedItem);

        await table.ReplaceItemAsync(mInstance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => table.PushItemsAsync());

        Assert.Single(MockHandler.Requests);
        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"/tables/movies/{itemToUpdate.Id}", request.RequestUri.PathAndQuery);
        Assert.Equal("\"1\"", request.Headers.IfMatch.First().Tag);
        var requestObj = JObject.Parse(await request.Content.ReadAsStringAsync());
        AssertEx.JsonEqual(mInstance, requestObj);

        Assert.Equal(PushStatus.Complete, ex.PushResult.Status);
        Assert.Single(ex.PushResult.Errors);
        Assert.Equal("movies", ex.PushResult.Errors.First().TableName);
        Assert.Equal(itemToUpdate.Id, ex.PushResult.Errors.First().Item.Value<string>("id"));

        Assert.Single(store.TableMap[SystemTables.OperationsQueue]);
        var op = TableOperation.Deserialize(store.TableMap[SystemTables.OperationsQueue].Values.First());
        Assert.Equal(TableOperationState.Failed, op.State);

        Assert.Single(store.TableMap[SystemTables.SyncErrors]);
    }
}

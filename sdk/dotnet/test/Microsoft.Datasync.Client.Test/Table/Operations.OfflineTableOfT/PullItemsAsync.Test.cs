// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTableOfT;

[ExcludeFromCodeCoverage]
public class PullItemsAsync_Tests : BaseOperationTest
{
    private readonly Random rnd = new();
    private readonly IdEntity testObject;
    private readonly JObject jsonObject;

    public PullItemsAsync_Tests() : base()
    {
        testObject = new IdEntity { Id = Guid.NewGuid().ToString("N"), StringValue = "testValue" };
        jsonObject = (JObject)table.ServiceClient.Serializer.Serialize(testObject);
    }

    #region Helpers
    /// <summary>
    /// Creates some random operations in the queue and returns the number of operations created.
    /// </summary>
    /// <param name="tableName">The name of the table holding the operations.</param>
    /// <param name="itemCount">The number of items to create.</param>
    /// <returns>The list of items created.</returns>
    private List<TableOperation> AddRandomOperations(string tableName, int itemCount = 0)
    {
        if (itemCount <= 0)
        {
            itemCount = rnd.Next(1, 20);
        }
        List<TableOperation> operations = new();
        for (var i = 0; i < itemCount; i++)
        {
            int t = rnd.Next(1, 4);
            switch (t)
            {
                case 1:
                    operations.Add(new DeleteOperation(tableName, Guid.NewGuid().ToString()) { Item = jsonObject });
                    break;
                case 2:
                    operations.Add(new InsertOperation(tableName, Guid.NewGuid().ToString()));
                    break;
                default:
                    operations.Add(new UpdateOperation(tableName, Guid.NewGuid().ToString()));
                    break;
            }
        }
        store.Upsert(SystemTables.OperationsQueue, operations.Select(op => op.Serialize()));
        return operations;
    }
    #endregion

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItems_NullQuery()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.PullItemsAsync(null, new PullOptions()));
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItems_NullOptions()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.PullItemsAsync(table.CreateQuery(), null));
    }

    [Fact]
    [Trait("method", "PullItemsAsync")]
    public async Task PullItemsAsync_NoResponse_Works()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<ClientMovie> { Items = new List<ClientMovie>() });
        store.GetOrCreateTable("movies");

        await table.PullItemsAsync(table.CreateQuery(), new PullOptions() { AlwaysPullWithDeltaToken = true });

        // Items were pulled.
        var storedEntities = store.TableMap["movies"]?.Values.ToList() ?? new List<JObject>();
        Assert.Empty(storedEntities);

        // Delta Token was not stored.
        Assert.Empty(store.TableMap[SystemTables.Configuration]);

        // Query was correct
        Assert.Single(MockHandler.Requests);
        Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset))&$orderby=updatedAt&$count=true&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
    }

    [Fact]
    [Trait("method", "PullItemsAsync")]
    public async Task PullItemsAsync_WithFilter_NoResponse_Works()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity> { Items = new List<IdEntity>() });
        store.GetOrCreateTable("movies");

        var query = table.Where(m => m.Rating == "PG-13");
        await table.PullItemsAsync(query, new PullOptions() { AlwaysPullWithDeltaToken = true });

        // Items were pulled.
        var storedEntities = store.TableMap["movies"]?.Values.ToList() ?? new List<JObject>();
        Assert.Empty(storedEntities);

        // Delta Token was not stored.
        Assert.Empty(store.TableMap[SystemTables.Configuration]);

        // Query was correct
        Assert.Single(MockHandler.Requests);
        Assert.Equal("/tables/movies?$filter=((rating eq 'PG-13') and (updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset)))&$orderby=updatedAt&$count=true&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
    }

    [Fact]
    [Trait("method", "PullItemsAsync")]
    public async Task PullItemsAsync_WithFilter_NoResponseORUpdatedAt_Works()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity> { Items = new List<IdEntity>() });
        store.GetOrCreateTable("movies");

        var query = table.Where(m => m.Rating == "PG-13");
        await table.PullItemsAsync(query, new PullOptions());

        // Items were pulled.
        var storedEntities = store.TableMap["movies"]?.Values.ToList() ?? new List<JObject>();
        Assert.Empty(storedEntities);

        // Delta Token was not stored.
        Assert.Empty(store.TableMap[SystemTables.Configuration]);

        // Query was correct
        Assert.Single(MockHandler.Requests);
        Assert.Equal("/tables/movies?$filter=(rating eq 'PG-13')&$orderby=updatedAt&$count=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_ProducesCorrectQuery()
    {
        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var items = CreatePageOfMovies(10, lastUpdatedAt);

        await table.PullItemsAsync(table.CreateQuery(), new PullOptions() { AlwaysPullWithDeltaToken = true });

        // Items were pulled.
        var storedEntities = store.TableMap["movies"].Values.ToList();
        AssertEx.SequenceEqual(items, storedEntities);

        // Delta Token was stored - it's stored as UnixTimeMilliseconds().
        Assert.Single(store.TableMap[SystemTables.Configuration].Values);
        var deltaToken = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(store.TableMap[SystemTables.Configuration].Values.FirstOrDefault()?.Value<string>("value")));
        Assert.Equal(lastUpdatedAt.ToUniversalTime(), deltaToken.ToUniversalTime());

        // Query was correct
        Assert.Single(MockHandler.Requests);
        Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset))&$orderby=updatedAt&$count=true&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_ProducesCorrectQuery_WithoutUpdatedAt()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        _ = CreatePageOfItems(10);

        await table.PullItemsAsync(table.CreateQuery(), new PullOptions());

        // Delta Token was not stored
        Assert.Empty(store.GetOrCreateTable(SystemTables.Configuration));

        // Query was correct
        Assert.Single(MockHandler.Requests);
        Assert.Equal("/tables/movies?$orderby=updatedAt&$count=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_UsesQueryId()
    {
        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var options = new PullOptions { AlwaysPullWithDeltaToken = true, QueryId = "abc123" };
        const string keyId = "dt.movies.abc123";
        var items = CreatePageOfMovies(10, lastUpdatedAt);

        await table.PullItemsAsync(table.CreateQuery(), options);

        // Items were pulled.
        AssertEx.SequenceEqual(items, store.TableMap["movies"].Values.ToList());

        // Delta Token was stored.
        Assert.True(store.TableMap[SystemTables.Configuration].ContainsKey(keyId));
        var deltaToken = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(store.TableMap[SystemTables.Configuration][keyId].Value<string>("value")));
        Assert.Equal(lastUpdatedAt.ToUniversalTime(), deltaToken.ToUniversalTime());

        // Query was correct
        Assert.Single(MockHandler.Requests);
        Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset))&$orderby=updatedAt&$count=true&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_ReadsQueryId()
    {
        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var options = new PullOptions { QueryId = "abc123" };
        const string keyId = "dt.movies.abc123";
        store.GetOrCreateTable(SystemTables.Configuration).Add(keyId, JObject.Parse($"{{\"id\":\"{keyId}\",\"value\":\"{lastUpdatedAt.ToUnixTimeMilliseconds()}\"}}"));
        _ = CreatePageOfMovies(10, lastUpdatedAt);

        await table.PullItemsAsync(table.CreateQuery(), options);

        // Query was correct
        Assert.Single(MockHandler.Requests);
        Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(2021-03-24T12:50:44.000Z,Edm.DateTimeOffset))&$orderby=updatedAt&$count=true&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_Overwrites_Data()
    {
        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var items = CreatePageOfMovies(10, lastUpdatedAt, 3);
        store.Upsert("movies", items); // store the 10 items in the store

        await table.PullItemsAsync(table.CreateQuery(), new PullOptions() { AlwaysPullWithDeltaToken = true });

        // Items were pulled, and the deleted items were in fact deleted.
        var storedEntities = store.TableMap["movies"].Values.ToList();
        AssertEx.SequenceEqual(items.Where(m => !m.Value<bool>("deleted")).ToList(), storedEntities);
        // There are no deleted items.
        Assert.DoesNotContain(storedEntities, m => m.Value<bool>("deleted"));

        // Delta Token was stored - it's stored as UnixTimeMilliseconds().
        Assert.Single(store.TableMap[SystemTables.Configuration].Values);
        var deltaToken = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(store.TableMap[SystemTables.Configuration].Values.FirstOrDefault()?.Value<string>("value")));
        Assert.Equal(lastUpdatedAt.ToUniversalTime(), deltaToken.ToUniversalTime());

        // Query was correct
        Assert.Single(MockHandler.Requests);
        Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset))&$orderby=updatedAt&$count=true&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_CallsPush_WhenDirty()
    {
        AddRandomOperations("movies", 1);

        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var items = CreatePageOfMovies(10, lastUpdatedAt, 3);
        store.Upsert("movies", items); // store the 10 items in the store

        var pushContext = Substitute.For<IPushContext>();
        pushContext.PushItemsAsync(Arg.Any<string[]>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        table.ServiceClient.SyncContext.PushContext = pushContext;

        // We don't "consume" the operation, so PullItemsAsync() will throw an invalid operation because the table is dirty the second time.
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.PullItemsAsync(table.CreateQuery(), new PullOptions()));

        Received.InOrder(async () =>
        {
            await pushContext.Received(1).PushItemsAsync(Arg.Is<string[]>(t => t.Length == 1 && t[0] == "movies"), default);
        });
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_CallsPush_WhenDirty_WithPushOtherTables()
    {
        AddRandomOperations("movies", 1);

        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var items = CreatePageOfMovies(10, lastUpdatedAt, 3);
        store.Upsert("movies", items); // store the 10 items in the store

        var pushContext = Substitute.For<IPushContext>();
        pushContext.PushItemsAsync(Arg.Any<string[]>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        table.ServiceClient.SyncContext.PushContext = pushContext;

        // We don't "consume" the operation, so PullItemsAsync() will throw an invalid operation because the table is dirty the second time.
        var options = new PullOptions { PushOtherTables = true };
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.PullItemsAsync(table.CreateQuery(), options));

        Received.InOrder(async () =>
        {
            await pushContext.Received(1).PushItemsAsync(Arg.Is<string[]>(arg => arg == null), default);
        });
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_CallsPush_WhenDirty_ThenCleaned()
    {
        AddRandomOperations("movies", 1);

        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var options = new PullOptions { AlwaysPullWithDeltaToken = true, QueryId = "abc123" };
        const string keyId = "dt.movies.abc123";
        var items = CreatePageOfMovies(10, lastUpdatedAt);

        var pushContext = Substitute.For<IPushContext>();
        pushContext.PushItemsAsync(Arg.Any<string[]>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask).AndDoes(_ => store.TableMap[SystemTables.OperationsQueue].Clear());
        table.ServiceClient.SyncContext.PushContext = pushContext;

        await table.PullItemsAsync(table.CreateQuery(), options);

        // Items were pulled.
        var storedEntities = store.TableMap["movies"].Values.ToList();
        AssertEx.SequenceEqual(items, storedEntities);

        // Delta Token was stored.
        Assert.True(store.TableMap[SystemTables.Configuration].ContainsKey(keyId));
        var deltaToken = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(store.TableMap[SystemTables.Configuration][keyId].Value<string>("value")));
        Assert.Equal(lastUpdatedAt.ToUniversalTime(), deltaToken.ToUniversalTime());

        // Query was correct
        Assert.Single(MockHandler.Requests);
        Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset))&$orderby=updatedAt&$count=true&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
    }
}

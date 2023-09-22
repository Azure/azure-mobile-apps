// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.DeltaToken;
using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Offline;

[ExcludeFromCodeCoverage]
public class SyncContext_Tests : ClientBaseTest
{
    private readonly DatasyncClient client;
    private readonly MockOfflineStore store;
    private readonly Random rnd = new();

    private readonly IdEntity testObject;
    private readonly JObject jsonObject;
    private readonly List<SynchronizationEventArgs> events = new();
    private readonly object eventLock = new();

    public SyncContext_Tests()
    {
        client = GetMockClient();
        store = new MockOfflineStore();

        client.SynchronizationProgress += (sender, args) =>
        {
            // Some tests are in parallel, so we have to be careful not to modify concurrently
            lock(eventLock) { events.Add(args); }
        };

        testObject = new IdEntity { Id = Guid.NewGuid().ToString("N"), StringValue = "testValue" };
        jsonObject = (JObject)client.Serializer.Serialize(testObject);
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

    /// <summary>
    /// Creates some random records in a table
    /// </summary>
    /// <param name="tableName">The name of the table holding the records.</param>
    /// <param name="itemCount">The number of items to create.</param>
    /// <returns>The list of items created.</returns>
    private List<JObject> AddRandomRecords(string tableName, int itemCount = 0, bool sleep = false)
    {
        if (itemCount <= 0)
        {
            itemCount = rnd.Next(1, 20);
        }
        List<JObject> records = new();
        for (var i = 0; i < itemCount; i++)
        {
            var item = (JObject)jsonObject.DeepClone();
            item["id"] = Guid.NewGuid().ToString();
            item["updatedAt"] = DateTimeOffset.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK");
            if (sleep) Thread.Sleep(50);
            records.Add(item);
        }
        store.Upsert(tableName, records);
        return records;
    }

    /// <summary>
    /// Stores a valid delta token in the offline store.
    /// </summary>
    /// <param name="qid">The query ID.</param>
    private void SetDeltaToken(string tableName, string qid)
    {
        var dt = DateTimeOffset.Parse("2021-03-12T03:30:04.000Z");
        var token = JObject.Parse($"{{\"id\":\"dt.{tableName}.{qid}\",\"value\":{dt.ToUnixTimeMilliseconds()}}}");
        store.Upsert(SystemTables.Configuration, new[] { token });
    }

    /// <summary>
    /// Gets an initialized <see cref="SyncContext"/>
    /// </summary>
    private async Task<SyncContext> GetSyncContext()
    {
        var context = new SyncContext(client, store);
        await context.InitializeAsync();
        return context;
    }

    private void AssertEventsRecorded(int count, int qlen)
    {
        // push started, push finished, plus 2 events (before/after) for each count.
        Assert.Equal((count * 2) + 2, events.Count);
        Assert.Equal(SynchronizationEventType.PushStarted, events.First().EventType);
        Assert.Equal(SynchronizationEventType.PushFinished, events.Last().EventType);
        Assert.Equal(qlen, events.First().QueueLength);
    }

    private static void AssertItemWillBePushed(SynchronizationEventArgs args, string table, string id)
    {
        Assert.Equal(SynchronizationEventType.ItemWillBePushed, args.EventType);
        Assert.Equal(table, args.TableName);
        Assert.Equal(id, args.ItemId);
        Assert.Null(args.IsSuccessful);
    }

    private static void AssertItemWasPushed(SynchronizationEventArgs args, string table, string id, bool success)
    {
        Assert.Equal(SynchronizationEventType.ItemWasPushed, args.EventType);
        Assert.Equal(table, args.TableName);
        Assert.Equal(id, args.ItemId);
        Assert.Equal(success, args.IsSuccessful);
    }

    private void AssertEventsAreProduced(string table, string id, bool success)
    {
        // Find the Synchronization events for table + id - there should be 2
        var foundEvents = events.Where(e => e.TableName == table && e.ItemId == id).ToList();
        Assert.Equal(2, foundEvents.Count);
        AssertItemWillBePushed(foundEvents[0], table, id);
        AssertItemWasPushed(foundEvents[1], table, id, success);
    }
    #endregion

    #region Ctor
    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SyncContext(null, store));
        Assert.Throws<ArgumentNullException>(() => new SyncContext(client, null));
    }

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_SetsUpContext()
    {
        var context = new SyncContext(client, store);

        Assert.NotNull(context);
        Assert.False(context.IsInitialized);
        Assert.Same(store, context.OfflineStore);
        Assert.Same(client, context.ServiceClient);
    }
    #endregion

    #region InitializeAsync
    [Fact]
    [Trait("Method", "InitializeAsync")]
    public async Task InitializeAsync_Works()
    {
        var context = await GetSyncContext();

        Assert.True(context.IsInitialized);
        Assert.NotNull(context.OperationsQueue);
        Assert.NotNull(context.DeltaTokenStore);
    }

    [Fact]
    [Trait("Method", "InitializeAsync")]
    public async Task InitializeAsync_CanBeCalledTwice()
    {
        var context = await GetSyncContext();

        var opQueue = context.OperationsQueue;
        var tokenStore = context.DeltaTokenStore;
        Assert.IsAssignableFrom<DefaultDeltaTokenStore>(tokenStore);

        await context.InitializeAsync();

        Assert.Same(opQueue, context.OperationsQueue);
        Assert.Same(tokenStore, context.DeltaTokenStore);
    }

    [Fact]
    [Trait("Method", "InitializeAsync")]
    public async Task InitializeAsync_UsesStoreProvider_ForDeltaTokenStore()
    {
        var store = new MockOfflineStore_WithDeltaToken();
        var context = new SyncContext(client, store);
        await context.InitializeAsync();

        Assert.True(store.DeltaTokenStoreInitialized);
    }
    #endregion

    #region DeleteItemAsync
    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Throws_WhenNotInitialized()
    {
        var context = new SyncContext(client, store);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.DeleteItemAsync("test", jsonObject));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Throws_WhenNoId()
    {
        var context = await GetSyncContext();
        jsonObject.Remove("id");

        await Assert.ThrowsAsync<ArgumentException>(() => context.DeleteItemAsync("test", jsonObject));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Throws_WhenItemDoesNotExist()
    {
        var context = await GetSyncContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.DeleteItemAsync("test", jsonObject));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_DeletesFromStoreAndEnqueues()
    {
        store.Upsert("test", new[] { jsonObject });
        var context = await GetSyncContext();

        await context.DeleteItemAsync("test", jsonObject);

        Assert.False(store.TableContains("test", jsonObject));
        Assert.NotNull(store.FindInQueue(TableOperationKind.Delete, "test", testObject.Id));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ThrowsOfflineStore_WhenOtherError()
    {
        store.Upsert("test", new[] { jsonObject });
        var context = await GetSyncContext();

        store.ExceptionToThrow = new ApplicationException();

        var ex = await Assert.ThrowsAsync<OfflineStoreException>(() => context.DeleteItemAsync("test", jsonObject));
        Assert.Same(store.ExceptionToThrow, ex.InnerException);
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_WithExistingInsert_Collapses()
    {
        var context = await GetSyncContext();
        await context.InsertItemAsync("test", jsonObject);

        await context.DeleteItemAsync("test", jsonObject);

        var items = store.TableMap[SystemTables.OperationsQueue].Values.ToArray();
        Assert.Empty(items);
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_WithExistingUpdate_Collapses()
    {
        var context = await GetSyncContext();
        await context.InsertItemAsync("test", jsonObject);
        store.TableMap[SystemTables.OperationsQueue].Clear();     // fake the push
        await context.ReplaceItemAsync("test", jsonObject);

        await context.DeleteItemAsync("test", jsonObject);

        var items = store.TableMap[SystemTables.OperationsQueue].Values.ToArray();
        Assert.Single(items);
        Assert.Equal(1, items[0].Value<int>("kind"));
        Assert.Equal(testObject.Id, items[0].Value<string>("itemId"));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_WithExistingDelete_Throws()
    {
        store.Upsert("test", new[] { jsonObject });
        var context = await GetSyncContext();

        await context.DeleteItemAsync("test", jsonObject);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.DeleteItemAsync("test", jsonObject));
    }
    #endregion

    #region GetItemAsync
    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_ReturnsNull_ForMissingItem()
    {
        var context = await GetSyncContext();

        var result = await context.GetItemAsync("test", "1234");
        Assert.Null(result);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_ReturnsItem()
    {
        store.Upsert("test", new[] { jsonObject });
        var context = await GetSyncContext();

        var result = await context.GetItemAsync("test", testObject.Id);
        Assert.Equal(jsonObject, result);
    }
    #endregion

    #region GetNextPageAsync
    [Fact]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetNextPageAsync_AutoInitializes_WhenNotInitialized()
    {
        var context = new SyncContext(client, store);
        _ = await context.GetNextPageAsync("test", "$count=true");
        Assert.True(context.IsInitialized);
    }

    [Fact]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetNextPageAsync_ReturnsPage_WithoutCount()
    {
        JObject[] items = { jsonObject, jsonObject, jsonObject, jsonObject };
        QueryDescription storedQuery = null;
        int callCount = 0;
        store.ReadAsyncFunc = (QueryDescription query) =>
        {
            storedQuery = query;
            callCount++;
            return items;
        };
        var context = await GetSyncContext();
        callCount = 0; // Adjust for any calls made during initialization.

        var result = await context.GetNextPageAsync("test", "");
        Assert.NotNull(result);
        Assert.Null(result.Count);
        Assert.Equal(items, result.Items);

        Assert.Equal(1, callCount);
        Assert.NotNull(storedQuery);
        Assert.False(storedQuery.IncludeTotalCount);
    }

    [Fact]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetNextPageAsync_ReturnsPage_WithCount()
    {
        JObject[] items = { jsonObject, jsonObject, jsonObject, jsonObject };
        QueryDescription storedQuery = null;
        int callCount = 0;
        store.ReadAsyncFunc = (QueryDescription query) =>
        {
            storedQuery = query;
            callCount++;
            return items;
        };
        var context = await GetSyncContext();
        callCount = 0; // Adjust for any calls made during initialization.

        var result = await context.GetNextPageAsync("test", "$count=true");
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        Assert.Equal(items, result.Items);

        Assert.Equal(1, callCount);
        Assert.NotNull(storedQuery);
        Assert.True(storedQuery.IncludeTotalCount);
    }
    #endregion

    #region InsertItemAsync
    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_AutoInitializes_WhenNotInitialized()
    {
        var context = new SyncContext(client, store);
        await context.InsertItemAsync("test", jsonObject);
        Assert.True(context.IsInitialized);
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_Throws_WhenItemExists()
    {
        store.Upsert("test", new[] { jsonObject });
        var context = await GetSyncContext();

        await Assert.ThrowsAsync<OfflineStoreException>(() => context.InsertItemAsync("test", jsonObject));
        Assert.Null(store.FindInQueue(TableOperationKind.Insert, "test", testObject.Id));
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_InsertsToStoreAndEnqueues_WithId()
    {
        var context = await GetSyncContext();

        await context.InsertItemAsync("test", jsonObject);

        Assert.True(store.TableContains("test", jsonObject));
        Assert.NotNull(store.FindInQueue(TableOperationKind.Insert, "test", testObject.Id));
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_InsertsToStoreAndEnqueues_WithNoId()
    {
        jsonObject.Remove("id");
        var context = await GetSyncContext();

        await context.InsertItemAsync("test", jsonObject);

        var itemId = store.TableMap["test"].Values.FirstOrDefault()?.Value<string>("id");
        Assert.NotEmpty(itemId);
        Assert.NotNull(store.FindInQueue(TableOperationKind.Insert, "test", itemId));
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_ThrowsOfflineStore_WhenOtherError()
    {
        var context = await GetSyncContext();

        store.ExceptionToThrow = new ApplicationException();

        var ex = await Assert.ThrowsAsync<OfflineStoreException>(() => context.InsertItemAsync("test", jsonObject));
        Assert.Same(store.ExceptionToThrow, ex.InnerException);
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_WithExistingInsert_Throws()
    {
        var context = await GetSyncContext();

        await context.InsertItemAsync("test", jsonObject);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.InsertItemAsync("test", jsonObject));
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_WithExistingUpdate_Throws()
    {
        var context = await GetSyncContext();

        await context.InsertItemAsync("test", jsonObject);
        store.TableMap[SystemTables.OperationsQueue].Clear();     // fake the push
        await context.ReplaceItemAsync("test", jsonObject);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.InsertItemAsync("test", jsonObject));
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_WithExistingDelete_Throws()
    {
        store.Upsert("test", new[] { jsonObject });
        var context = await GetSyncContext();

        await context.DeleteItemAsync("test", jsonObject);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.InsertItemAsync("test", jsonObject));
    }
    #endregion

    #region PullItemsAsync
    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItems_NullTable()
    {
        var context = await GetSyncContext();
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.PullItemsAsync(null, "", new PullOptions()));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("abcdef gh")]
    [InlineData("!!!")]
    [InlineData("?")]
    [InlineData(";")]
    [InlineData("{EA235ADF-9F38-44EA-8DA4-EF3D24755767}")]
    [InlineData("###")]
    [InlineData("1abcd")]
    [InlineData("true.false")]
    [InlineData("a-b-c-d")]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItems_InvalidTable(string tableName)
    {
        var context = await GetSyncContext();
        await Assert.ThrowsAsync<ArgumentException>(() => context.PullItemsAsync(tableName, "", new PullOptions()));
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItems_NullQuery()
    {
        var context = await GetSyncContext();
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.PullItemsAsync("movies", null, new PullOptions()));
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItems_NullOptions()
    {
        var context = await GetSyncContext();
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.PullItemsAsync("movies", "", null));
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItems_SelectQuery_Throws()
    {
        var context = await GetSyncContext();
        await Assert.ThrowsAsync<ArgumentException>(() => context.PullItemsAsync("movies", "$select=id,updatedAt", new PullOptions()));
    }

    [Fact]
    [Trait("method", "PullItemsAsync")]
    public async Task PullItemsAsync_NoResponse_Works()
    {
        var context = await GetSyncContext();
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity> { Items = new List<IdEntity>() });
        store.GetOrCreateTable("movies");

        await context.PullItemsAsync("movies", "", new PullOptions() { AlwaysPullWithDeltaToken = true });

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
        var context = await GetSyncContext();
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity> { Items = new List<IdEntity>() });
        store.GetOrCreateTable("movies");

        await context.PullItemsAsync("movies", "$filter=(rating eq 'PG-13')", new PullOptions() { AlwaysPullWithDeltaToken = true });

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
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_ProducesCorrectQuery()
    {
        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        var context = await GetSyncContext();
        var items = CreatePageOfMovies(10, lastUpdatedAt);

        await context.PullItemsAsync("movies", "", new PullOptions() { AlwaysPullWithDeltaToken = true });

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
        var context = await GetSyncContext();
        var pageOfItems = CreatePageOfItems(10, 10);

        await context.PullItemsAsync("movies", "", new PullOptions());

        // Delta Token was not stored
        var table = store.GetOrCreateTable(SystemTables.Configuration);
        Assert.Empty(table);

        // Query was correct
        Assert.Single(MockHandler.Requests);
        Assert.Equal("/tables/movies?$orderby=updatedAt&$count=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));

        // Events were sent properly
        Assert.Equal(22, events.Count); // 2 for each event, plus start and finish.
        Assert.Equal(SynchronizationEventType.PullStarted, events.First().EventType);
        Assert.Equal("movies", events.First().TableName);
        Assert.Equal(SynchronizationEventType.PullFinished, events.Last().EventType);
        Assert.Equal("movies", events.Last().TableName);

        // New logic - because we do out of order storing of data now, we can't guarantee the sequencing
        // of the synchronization events.  So we look at the events and say "was the appropriate event
        // produced in the set"
        foreach (var item in pageOfItems.Items)
        {
            var willStoreEvent = events.FindAll(m => m.EventType == SynchronizationEventType.ItemWillBeStored && m.ItemId == item.Id);
            Assert.Single(willStoreEvent);

            var storedEvent = events.FindAll(m => m.EventType == SynchronizationEventType.ItemWasStored && m.ItemId == item.Id);
            Assert.Single(storedEvent);
        }

        Assert.Equal(10, events[2].QueueLength);
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_UsesQueryId()
    {
        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        var context = await GetSyncContext();
        var options = new PullOptions { AlwaysPullWithDeltaToken = true, QueryId = "abc123" };
        const string keyId = "dt.movies.abc123";
        var items = CreatePageOfMovies(10, lastUpdatedAt);

        await context.PullItemsAsync("movies", "", options);

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

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_ReadsQueryId()
    {
        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        var context = await GetSyncContext();
        var options = new PullOptions { QueryId = "abc123" };
        const string keyId = "dt.movies.abc123";
        var table = store.GetOrCreateTable(SystemTables.Configuration);
        table[keyId] = JObject.Parse($"{{\"id\":\"{keyId}\",\"value\":\"{lastUpdatedAt.ToUnixTimeMilliseconds()}\"}}");
        _ = CreatePageOfMovies(10, lastUpdatedAt);

        await context.PullItemsAsync("movies", "", options);

        // Query was correct
        Assert.Single(MockHandler.Requests);
        Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(2021-03-24T12:50:44.000Z,Edm.DateTimeOffset))&$orderby=updatedAt&$count=true&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_Overwrites_Data()
    {
        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        var context = await GetSyncContext();
        var items = CreatePageOfMovies(10, lastUpdatedAt, 3);
        store.Upsert("movies", items); // store the 10 items in the store

        await context.PullItemsAsync("movies", "", new PullOptions() { AlwaysPullWithDeltaToken = true });

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
        var context = await GetSyncContext();
        var items = CreatePageOfMovies(10, lastUpdatedAt, 3);
        store.Upsert("movies", items); // store the 10 items in the store

        var pushContext = Substitute.For<IPushContext>();
        pushContext.PushItemsAsync(Arg.Any<string[]>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        context.PushContext = pushContext;

        // We don't "consume" the operation, so PullItemsAsync() will throw an invalid operation because the table is dirty the second time.
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => context.PullItemsAsync("movies", "", new PullOptions()));

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
        var context = await GetSyncContext();
        var items = CreatePageOfMovies(10, lastUpdatedAt, 3);
        store.Upsert("movies", items); // store the 10 items in the store

        var pushContext = Substitute.For<IPushContext>();
        pushContext.PushItemsAsync(Arg.Any<string[]>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        context.PushContext = pushContext;

        // We don't "consume" the operation, so PullItemsAsync() will throw an invalid operation because the table is dirty the second time.
        var options = new PullOptions { PushOtherTables = true };
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => context.PullItemsAsync("movies", "", options));

        Received.InOrder(async () =>
        {
            await pushContext.Received(1).PushItemsAsync(default, default);
        });
    }

    [Fact]
    [Trait("Method", "PullItemsAsync")]
    public async Task PullItemsAsync_CallsPush_WhenDirty_ThenCleaned()
    {
        AddRandomOperations("movies", 1);

        var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
        var context = await GetSyncContext();
        var options = new PullOptions { AlwaysPullWithDeltaToken = true, QueryId = "abc123" };
        const string keyId = "dt.movies.abc123";
        var items = CreatePageOfMovies(10, lastUpdatedAt);

        var pushContext = Substitute.For<IPushContext>();
        pushContext.PushItemsAsync(Arg.Any<string[]>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask).AndDoes(_ => store.TableMap[SystemTables.OperationsQueue].Clear());
        context.PushContext = pushContext;

        await context.PullItemsAsync("movies", "", options);

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
    #endregion

    #region PurgeItemsAsync
    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_NullTable()
    {
        var context = await GetSyncContext();
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.PurgeItemsAsync(null, "", new PurgeOptions()));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("abcdef gh")]
    [InlineData("!!!")]
    [InlineData("?")]
    [InlineData(";")]
    [InlineData("{EA235ADF-9F38-44EA-8DA4-EF3D24755767}")]
    [InlineData("###")]
    [InlineData("1abcd")]
    [InlineData("true.false")]
    [InlineData("a-b-c-d")]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_InvalidTable(string tableName)
    {
        var context = await GetSyncContext();
        await Assert.ThrowsAsync<ArgumentException>(() => context.PurgeItemsAsync(tableName, "", new PurgeOptions()));
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_NullQuery()
    {
        var context = await GetSyncContext();
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.PurgeItemsAsync("movies", null, new PurgeOptions()));
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_NullOptions()
    {
        var context = await GetSyncContext();
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.PurgeItemsAsync("movies", "", null));
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_NoOptions_NoRecords()
    {
        var context = await GetSyncContext();
        const string tableName = "test";
        const string query = "";
        var options = new PurgeOptions();

        await context.PurgeItemsAsync(tableName, query, options);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Empty(store.TableMap[SystemTables.Configuration]);
        Assert.Empty(store.TableMap[tableName]);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_Discard_NoRecords()
    {
        var context = await GetSyncContext();
        const string tableName = "test";
        const string query = "";
        var options = new PurgeOptions() { DiscardPendingOperations = true };

        await context.PurgeItemsAsync(tableName, query, options);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Empty(store.TableMap[SystemTables.Configuration]);
        Assert.Empty(store.TableMap[tableName]);
        Assert.Equal(0, context.PendingOperations);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_Discard_QID_NoRecords()
    {
        var context = await GetSyncContext();
        const string tableName = "test";
        const string query = "";
        var options = new PurgeOptions() { DiscardPendingOperations = true, QueryId = "abc123" };

        await context.PurgeItemsAsync(tableName, query, options);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Empty(store.TableMap[SystemTables.Configuration]);
        Assert.Empty(store.TableMap[tableName]);
        Assert.Equal(0, context.PendingOperations);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_NoDiscard_NoRecords()
    {
        var context = await GetSyncContext();
        const string tableName = "test";
        const string query = "";
        var options = new PurgeOptions() { DiscardPendingOperations = false };

        await context.PurgeItemsAsync(tableName, query, options);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Empty(store.TableMap[SystemTables.Configuration]);
        Assert.Empty(store.TableMap[tableName]);
        Assert.Equal(0, context.PendingOperations);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_NoDiscard_QID_NoRecords()
    {
        var context = await GetSyncContext();
        const string tableName = "test";
        const string query = "";
        var options = new PurgeOptions() { DiscardPendingOperations = false, QueryId = "abc123" };

        await context.PurgeItemsAsync(tableName, query, options);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Empty(store.TableMap[SystemTables.Configuration]);
        Assert.Empty(store.TableMap[tableName]);
        Assert.Equal(0, context.PendingOperations);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_NoOptions_WithRecords()
    {
        AddRandomOperations("test", 10);
        AddRandomRecords("test", 10);
        SetDeltaToken("test", "abc123");
        var context = await GetSyncContext();
        const string tableName = "test";
        const string query = "";
        var options = new PurgeOptions();

        Assert.Equal(store.TableMap[SystemTables.OperationsQueue].Count, context.PendingOperations);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.PurgeItemsAsync(tableName, query, options));

        Assert.NotEmpty(store.TableMap["test"]);
        Assert.NotEmpty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.NotEmpty(store.TableMap[SystemTables.Configuration]["dt.test.abc123"]);
        Assert.Equal(store.TableMap[SystemTables.OperationsQueue].Count, context.PendingOperations);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_Discard_WithRecords()
    {
        AddRandomOperations("test", 10);
        AddRandomRecords("test", 10);
        SetDeltaToken("test", "abc123");
        var context = await GetSyncContext();
        const string tableName = "test";
        const string query = "";
        var options = new PurgeOptions() { DiscardPendingOperations = true };

        Assert.Equal(store.TableMap[SystemTables.OperationsQueue].Count, context.PendingOperations);

        await context.PurgeItemsAsync(tableName, query, options);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.NotEmpty(store.TableMap[SystemTables.Configuration]["dt.test.abc123"]);
        Assert.Empty(store.TableMap[tableName]);
        Assert.Equal(0, context.PendingOperations);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_Discard_QID_WithRecords()
    {
        AddRandomOperations("test", 10);
        AddRandomRecords("test", 10);
        SetDeltaToken("test", "abc123");
        var context = await GetSyncContext();
        const string tableName = "test";
        const string query = "";
        var options = new PurgeOptions() { DiscardPendingOperations = true, QueryId = "abc123" };

        Assert.Equal(store.TableMap[SystemTables.OperationsQueue].Count, context.PendingOperations);

        await context.PurgeItemsAsync(tableName, query, options);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Empty(store.TableMap[SystemTables.Configuration]);
        Assert.Empty(store.TableMap[tableName]);
        Assert.Equal(0, context.PendingOperations);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_NoDiscard_WithRecords()
    {
        AddRandomOperations("test", 10);
        AddRandomRecords("test", 10);
        SetDeltaToken("test", "abc123");
        var context = await GetSyncContext();
        const string tableName = "test";
        const string query = "";
        var options = new PurgeOptions() { DiscardPendingOperations = false };

        Assert.Equal(store.TableMap[SystemTables.OperationsQueue].Count, context.PendingOperations);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.PurgeItemsAsync(tableName, query, options));

        Assert.NotEmpty(store.TableMap["test"]);
        Assert.NotEmpty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.NotEmpty(store.TableMap[SystemTables.Configuration]["dt.test.abc123"]);
        Assert.Equal(store.TableMap[SystemTables.OperationsQueue].Count, context.PendingOperations);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_NoDiscard_QID_WithRecords()
    {
        AddRandomOperations("test", 10);
        AddRandomRecords("test", 10);
        SetDeltaToken("test", "abc123");
        var context = await GetSyncContext();
        const string tableName = "test";
        const string query = "";
        var options = new PurgeOptions() { DiscardPendingOperations = false, QueryId = "abc123" };

        Assert.Equal(store.TableMap[SystemTables.OperationsQueue].Count, context.PendingOperations);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.PurgeItemsAsync(tableName, query, options));

        Assert.NotEmpty(store.TableMap["test"]);
        Assert.NotEmpty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.NotEmpty(store.TableMap[SystemTables.Configuration]["dt.test.abc123"]);
        Assert.Equal(store.TableMap[SystemTables.OperationsQueue].Count, context.PendingOperations);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_WithSearch_WithRecords()
    {
        AddRandomOperations("test", 10);
        AddRandomRecords("test", 10);
        SetDeltaToken("test", "abc123");
        var context = await GetSyncContext();
        const string tableName = "test";
        const string query = "$filter=deleted eq false";
        var options = new PurgeOptions() { DiscardPendingOperations = true };

        await context.PurgeItemsAsync(tableName, query, options);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.NotEmpty(store.TableMap[SystemTables.Configuration]);
        Assert.Empty(store.TableMap[tableName]);
        Assert.Equal(0, context.PendingOperations);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_TimestampPolicy_NoUpdate()
    {
        const string tableName = "test";
        const string queryId = "abc123";
        const string query = "$filter=deleted eq false";

        AddRandomRecords(tableName, 10);
        SetDeltaToken(tableName, queryId);

        var context = await GetSyncContext();
        var expected = await context.DeltaTokenStore.GetDeltaTokenAsync(tableName, queryId);
        var options = new PurgeOptions() { QueryId = queryId, TimestampUpdatePolicy = TimestampUpdatePolicy.NoUpdate };

        await context.PurgeItemsAsync(tableName, query, options);

        var actual = await context.DeltaTokenStore.GetDeltaTokenAsync(tableName, queryId);
        Assert.Equal(expected, actual);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_TimestampPolicy_UpdateToNow()
    {
        const string tableName = "test";
        const string queryId = "abc123";
        const string query = "$filter=deleted eq false";

        AddRandomRecords(tableName, 10);
        SetDeltaToken(tableName, queryId);

        var context = await GetSyncContext();
        var options = new PurgeOptions() { QueryId = queryId, TimestampUpdatePolicy = TimestampUpdatePolicy.UpdateToNow };
        var expected = DateTimeOffset.UtcNow;

        await context.PurgeItemsAsync(tableName, query, options);

        var actual = await context.DeltaTokenStore.GetDeltaTokenAsync(tableName, queryId);
        // Depending on speed, may need to update the interval.
        AssertEx.CloseTo(expected, actual);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_TimestampPolicy_UpdateToEpoch()
    {
        const string tableName = "test";
        const string queryId = "abc123";
        const string query = "$filter=deleted eq false";

        AddRandomRecords(tableName, 10);
        SetDeltaToken(tableName, queryId);

        var context = await GetSyncContext();
        var options = new PurgeOptions() { QueryId = queryId, TimestampUpdatePolicy = TimestampUpdatePolicy.UpdateToEpoch };
        var epoch = DateTimeOffset.FromUnixTimeMilliseconds(0L);

        await context.PurgeItemsAsync(tableName, query, options);

        var actual = await context.DeltaTokenStore.GetDeltaTokenAsync(tableName, queryId);
        Assert.Equal(epoch, actual);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_TimestampPolicy_UpdateToLastEntity_NoItems()
    {
        const string tableName = "test";
        const string queryId = "abc123";
        const string query = "$filter=deleted eq false";

        AddRandomRecords(tableName, 10);
        SetDeltaToken(tableName, queryId);

        var context = await GetSyncContext();
        var options = new PurgeOptions() { QueryId = queryId, TimestampUpdatePolicy = TimestampUpdatePolicy.UpdateToLastEntity };
        var epoch = DateTimeOffset.FromUnixTimeMilliseconds(0L);

        await context.PurgeItemsAsync(tableName, query, options);

        var actual = await context.DeltaTokenStore.GetDeltaTokenAsync(tableName, queryId);
        Assert.Equal(epoch, actual);
    }

    [Fact]
    [Trait("Method", "PurgeItemsAsync")]
    public async Task PurgeItems_TimestampPolicy_UpdateToLastEntity_SomeItems()
    {
        const string tableName = "test";
        const string queryId = "abc123";
        const string query = "$filter=deleted eq true";

        var insertedObjects = AddRandomRecords(tableName, 10, sleep: true);
        var updatedAt = insertedObjects.Last().Value<string>("updatedAt");
        var expected = DateTimeOffset.Parse(updatedAt);
        SetDeltaToken(tableName, queryId);

        var context = await GetSyncContext();
        var options = new PurgeOptions() { QueryId = queryId, TimestampUpdatePolicy = TimestampUpdatePolicy.UpdateToLastEntity };

        await context.PurgeItemsAsync(tableName, query, options);

        var actual = await context.DeltaTokenStore.GetDeltaTokenAsync(tableName, queryId);
        Assert.Equal(expected, actual);
    }
    #endregion

    #region PushItemsAsync
    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_HandlesEmptyQueue_AllTables()
    {
        var context = await GetSyncContext();
        MockHandler.AddResponse(HttpStatusCode.NotFound);

        await context.PushItemsAsync((string[])null);

        Assert.Empty(MockHandler.Requests);
        AssertEventsRecorded(0, 0);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_HandlesEmptyQueue_SingleTable()
    {
        var context = await GetSyncContext();
        MockHandler.AddResponse(HttpStatusCode.NotFound);
        var op = new DeleteOperation("test", "abc123");
        store.Upsert(SystemTables.OperationsQueue, new[] { op.Serialize() });

        await context.PushItemsAsync("movies");

        Assert.Empty(MockHandler.Requests);
        AssertEventsRecorded(0, 0);
        Assert.Single(store.TableMap[SystemTables.OperationsQueue]);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_AllTables_HandlesDeleteOperation_WithVersion()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "abc123" };
        var instance = (JObject)client.Serializer.Serialize(item);
        store.Upsert("movies", new[] { instance });
        MockHandler.AddResponse(HttpStatusCode.NoContent);

        await context.DeleteItemAsync("movies", instance);
        await context.PushItemsAsync((string[])null);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Single(MockHandler.Requests);
        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", item.Id);
        AssertItemWasPushed(events[2], "movies", item.Id, true);

        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"/tables/movies/{item.Id}", request.RequestUri.PathAndQuery);
        Assert.Equal($"\"{item.Version}\"", request.Headers.IfMatch.FirstOrDefault()?.Tag);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_AllTables_HandlesDeleteOperation_WithVersionAndParallelOperations()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "abc123" };
        var instance = (JObject)client.Serializer.Serialize(item);
        store.Upsert("movies", new[] { instance });
        MockHandler.AddResponse(HttpStatusCode.NoContent);

        await context.DeleteItemAsync("movies", instance);
        await context.PushItemsAsync((string[])null, new PushOptions { ParallelOperations = 2 });

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Single(MockHandler.Requests);
        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", item.Id);
        AssertItemWasPushed(events[2], "movies", item.Id, true);

        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"/tables/movies/{item.Id}", request.RequestUri.PathAndQuery);
        Assert.Equal($"\"{item.Version}\"", request.Headers.IfMatch.FirstOrDefault()?.Tag);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesDeleteOperation_WithVersion()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "abc123" };
        var instance = (JObject)client.Serializer.Serialize(item);
        store.Upsert("movies", new[] { instance });
        MockHandler.AddResponse(HttpStatusCode.NoContent);

        await context.DeleteItemAsync("movies", instance);
        await context.PushItemsAsync("movies");

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Single(MockHandler.Requests);
        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", item.Id);
        AssertItemWasPushed(events[2], "movies", item.Id, true);

        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"/tables/movies/{item.Id}", request.RequestUri.PathAndQuery);
        Assert.Equal($"\"{item.Version}\"", request.Headers.IfMatch.FirstOrDefault()?.Tag);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesDeleteOperation_MultiOperation(int nThreads)
    {
        var context = await GetSyncContext();
        var options = new PushOptions { ParallelOperations = nThreads };
        int nItems = 10;

        // Create a list of ten movies
        List<ClientMovie> movies = new();
        for (int i = 0; i < nItems; i++)
        {
            var movie = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "abc123" };
            movies.Add(movie);

            // Add Movie to the store
            var jMovie = (JObject)client.Serializer.Serialize(movie);
            store.Upsert("movies", new[] { jMovie });

            // Add a mock result
            MockHandler.AddResponse(HttpStatusCode.NoContent);

            // Delete the item from the store
            await context.DeleteItemAsync("movies", jMovie);
        }

        await context.PushItemsAsync("movies", options);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Equal(nItems, MockHandler.Requests.Count);
        // PushStarted, PushFinished, plus nItems delete operations
        Assert.Equal((nItems * 2) + 2, events.Count);
        Assert.Equal(SynchronizationEventType.PushStarted, events.First().EventType);
        Assert.Equal(SynchronizationEventType.PushFinished, events.Last().EventType);
        Assert.Equal(nItems, events.First().QueueLength);

        foreach (var movie in movies)
        {
            var request = MockHandler.Requests.SingleOrDefault(r => r.RequestUri.PathAndQuery == $"/tables/movies/{movie.Id}");
            Assert.NotNull(request);
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal($"\"{movie.Version}\"", request.Headers.IfMatch.FirstOrDefault()?.Tag);
            AssertEventsAreProduced("movies", movie.Id, true);
        }
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_AllTables_HandlesDeleteOperation_WithoutVersion()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString() };
        var instance = (JObject)client.Serializer.Serialize(item);
        store.Upsert("movies", new[] { instance });
        MockHandler.AddResponse(HttpStatusCode.NoContent);

        await context.DeleteItemAsync("movies", instance);

        await context.PushItemsAsync((string[])null);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Single(MockHandler.Requests);
        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", item.Id);
        AssertItemWasPushed(events[2], "movies", item.Id, true);

        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"/tables/movies/{item.Id}", request.RequestUri.PathAndQuery);
        Assert.Empty(request.Headers.IfMatch);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesDeleteOperation_WithoutVersion()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString() };
        var instance = (JObject)client.Serializer.Serialize(item);
        store.Upsert("movies", new[] { instance });
        MockHandler.AddResponse(HttpStatusCode.NoContent);

        await context.DeleteItemAsync("movies", instance);

        await context.PushItemsAsync("movies");

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Single(MockHandler.Requests);
        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", item.Id);
        AssertItemWasPushed(events[2], "movies", item.Id, true);

        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"/tables/movies/{item.Id}", request.RequestUri.PathAndQuery);
        Assert.Empty(request.Headers.IfMatch);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_AllTables_HandlesDeleteOperation_Conflict()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        var instance = (JObject)client.Serializer.Serialize(item);
        store.Upsert("movies", new[] { instance });
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await context.DeleteItemAsync("movies", instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync((string[])null));

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

        // TODO
        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", item.Id);
        AssertItemWasPushed(events[2], "movies", item.Id, false);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesDeleteOperation_Conflict()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        var instance = (JObject)client.Serializer.Serialize(item);
        store.Upsert("movies", new[] { instance });
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await context.DeleteItemAsync("movies", instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync("movies"));

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

        // TODO
        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", item.Id);
        AssertItemWasPushed(events[2], "movies", item.Id, false);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_AllTables_HandlesInsertOperation()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Title = "The Big Test" };
        var returnedItem = item.Clone();
        returnedItem.UpdatedAt = DateTimeOffset.Now;
        returnedItem.Version = "1";
        var expectedContent = $"{{\"bestPictureWinner\":false,\"duration\":0,\"rating\":null,\"releaseDate\":\"0001-01-01T00:00:00.000Z\",\"title\":\"The Big Test\",\"year\":0,\"id\":\"{item.Id}\"}}";
        var instance = (JObject)client.Serializer.Serialize(item);
        MockHandler.AddResponse(HttpStatusCode.OK, returnedItem);

        await context.InsertItemAsync("movies", instance);
        await context.PushItemsAsync((string[])null);

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Single(MockHandler.Requests);
        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", item.Id);
        AssertItemWasPushed(events[2], "movies", item.Id, true);

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
    public async Task PushItemsAsync_SingleTable_HandlesInsertOperation()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Title = "The Big Test" };
        var returnedItem = item.Clone();
        returnedItem.UpdatedAt = DateTimeOffset.Now;
        returnedItem.Version = "1";
        var expectedContent = $"{{\"bestPictureWinner\":false,\"duration\":0,\"rating\":null,\"releaseDate\":\"0001-01-01T00:00:00.000Z\",\"title\":\"The Big Test\",\"year\":0,\"id\":\"{item.Id}\"}}";
        var instance = (JObject)client.Serializer.Serialize(item);
        MockHandler.AddResponse(HttpStatusCode.OK, returnedItem);

        await context.InsertItemAsync("movies", instance);
        await context.PushItemsAsync("movies");

        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Single(MockHandler.Requests);
        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", item.Id);
        AssertItemWasPushed(events[2], "movies", item.Id, true);

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
    public async Task PushItemsAsync_AllTables_HandlesInsertOperation_Conflict()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        var instance = (JObject)client.Serializer.Serialize(item);
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await context.InsertItemAsync("movies", instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync((string[])null));

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

        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", item.Id);
        AssertItemWasPushed(events[2], "movies", item.Id, false);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesInsertOperation_Conflict()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        var instance = (JObject)client.Serializer.Serialize(item);
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await context.InsertItemAsync("movies", instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync("movies"));

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

        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", item.Id);
        AssertItemWasPushed(events[2], "movies", item.Id, false);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_AllTables_HandlesUpdateOperation_WithoutVersion()
    {
        var context = await GetSyncContext();
        var itemToUpdate = new ClientMovie { Id = Guid.NewGuid().ToString(), Title = "The Big Test" };
        var instance = client.Serializer.Serialize(itemToUpdate) as JObject;
        store.Upsert("movies", new[] { instance });

        var updatedItem = itemToUpdate.Clone();
        updatedItem.Title = "Modified";
        var mInstance = client.Serializer.Serialize(updatedItem) as JObject;

        var returnedItem = itemToUpdate.Clone();
        returnedItem.Version = "2";
        MockHandler.AddResponse(HttpStatusCode.OK, returnedItem);

        await context.ReplaceItemAsync("movies", mInstance);
        await context.PushItemsAsync((string[])null);

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

        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", itemToUpdate.Id);
        AssertItemWasPushed(events[2], "movies", itemToUpdate.Id, true);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesUpdateOperationWithoutVersion()
    {
        var context = await GetSyncContext();
        var itemToUpdate = new ClientMovie { Id = Guid.NewGuid().ToString(), Title = "The Big Test" };
        var instance = client.Serializer.Serialize(itemToUpdate) as JObject;
        store.Upsert("movies", new[] { instance });

        var updatedItem = itemToUpdate.Clone();
        updatedItem.Title = "Modified";
        var mInstance = client.Serializer.Serialize(updatedItem) as JObject;

        var returnedItem = itemToUpdate.Clone();
        returnedItem.Version = "2";
        MockHandler.AddResponse(HttpStatusCode.OK, returnedItem);

        await context.ReplaceItemAsync("movies", mInstance);
        await context.PushItemsAsync("movies");

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

        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", itemToUpdate.Id);
        AssertItemWasPushed(events[2], "movies", itemToUpdate.Id, true);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_AllTables_HandlesUpdateOperation_WithVersion()
    {
        var context = await GetSyncContext();
        var itemToUpdate = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1", Title = "The Big Test" };
        var instance = client.Serializer.Serialize(itemToUpdate) as JObject;
        store.Upsert("movies", new[] { instance });

        var updatedItem = itemToUpdate.Clone();
        updatedItem.Title = "Modified";
        var mInstance = client.Serializer.Serialize(updatedItem) as JObject;

        var returnedItem = itemToUpdate.Clone();
        returnedItem.Version = "2";
        MockHandler.AddResponse(HttpStatusCode.OK, returnedItem);

        await context.ReplaceItemAsync("movies", mInstance);
        await context.PushItemsAsync((string[])null);

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

        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", itemToUpdate.Id);
        AssertItemWasPushed(events[2], "movies", itemToUpdate.Id, true);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesUpdateOperation_WithVersion()
    {
        var context = await GetSyncContext();
        var itemToUpdate = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1", Title = "The Big Test" };
        var instance = client.Serializer.Serialize(itemToUpdate) as JObject;
        store.Upsert("movies", new[] { instance });

        var updatedItem = itemToUpdate.Clone();
        updatedItem.Title = "Modified";
        var mInstance = client.Serializer.Serialize(updatedItem) as JObject;

        var returnedItem = itemToUpdate.Clone();
        returnedItem.Version = "2";
        MockHandler.AddResponse(HttpStatusCode.OK, returnedItem);

        await context.ReplaceItemAsync("movies", mInstance);
        await context.PushItemsAsync("movies");

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

        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", itemToUpdate.Id);
        AssertItemWasPushed(events[2], "movies", itemToUpdate.Id, true);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_AllTables_HandlesUpdateOperation_Conflict()
    {
        var context = await GetSyncContext();
        var itemToUpdate = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1", Title = "The Big Test" };
        var instance = client.Serializer.Serialize(itemToUpdate) as JObject;
        store.Upsert("movies", new[] { instance });

        var updatedItem = itemToUpdate.Clone();
        updatedItem.Title = "Modified";
        var mInstance = client.Serializer.Serialize(updatedItem) as JObject;

        var returnedItem = itemToUpdate.Clone();
        returnedItem.Version = "2";
        var expectedInstance = client.Serializer.Serialize(returnedItem) as JObject;
        MockHandler.AddResponse(HttpStatusCode.Conflict, returnedItem);

        await context.ReplaceItemAsync("movies", mInstance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync((string[])null));

        Assert.Single(MockHandler.Requests);
        var request = MockHandler.Requests[0];
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"/tables/movies/{itemToUpdate.Id}", request.RequestUri.PathAndQuery);
        Assert.Equal("\"1\"", request.Headers.IfMatch.First().Tag);
        var content = await request.Content.ReadAsStringAsync();
        var requestObj = JObject.Parse(content);
        AssertEx.JsonEqual(mInstance, requestObj);

        Assert.Equal(PushStatus.Complete, ex.PushResult.Status);
        Assert.Single(ex.PushResult.Errors);
        Assert.Equal("movies", ex.PushResult.Errors.First().TableName);
        Assert.Equal(itemToUpdate.Id, ex.PushResult.Errors.First().Item.Value<string>("id"));

        Assert.Single(store.TableMap[SystemTables.OperationsQueue]);
        var op = TableOperation.Deserialize(store.TableMap[SystemTables.OperationsQueue].Values.First());
        Assert.Equal(TableOperationState.Failed, op.State);

        Assert.Single(store.TableMap[SystemTables.SyncErrors]);

        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", itemToUpdate.Id);
        AssertItemWasPushed(events[2], "movies", itemToUpdate.Id, false);
    }

    [Fact]
    [Trait("Method", "PushItemsAsync")]
    public async Task PushItemsAsync_SingleTable_HandlesUpdateOperation_Conflict()
    {
        var context = await GetSyncContext();
        var itemToUpdate = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1", Title = "The Big Test" };
        var instance = client.Serializer.Serialize(itemToUpdate) as JObject;
        store.Upsert("movies", new[] { instance });

        var updatedItem = itemToUpdate.Clone();
        updatedItem.Title = "Modified";
        var mInstance = client.Serializer.Serialize(updatedItem) as JObject;

        var returnedItem = itemToUpdate.Clone();
        returnedItem.Version = "2";
        var expectedInstance = client.Serializer.Serialize(returnedItem) as JObject;
        MockHandler.AddResponse(HttpStatusCode.Conflict, returnedItem);

        await context.ReplaceItemAsync("movies", mInstance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync("movies"));

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

        AssertEventsRecorded(1, 1);
        AssertItemWillBePushed(events[1], "movies", itemToUpdate.Id);
        AssertItemWasPushed(events[2], "movies", itemToUpdate.Id, false);
    }
    #endregion

    #region ReplaceItemAsync
    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_AutoInitializes_WhenNotInitialized()
    {
        var context = new SyncContext(client, store);
        // We don't actually care about this - it's going to throw because it doesn't exist.
        await Assert.ThrowsAsync<OfflineStoreException>(() => context.ReplaceItemAsync("test", jsonObject));
        Assert.True(context.IsInitialized);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Throws_WhenNoId()
    {
        var context = await GetSyncContext();

        jsonObject.Remove("id");

        await Assert.ThrowsAsync<ArgumentException>(() => context.ReplaceItemAsync("test", jsonObject));
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Throws_WhenItemDoesNotExists()
    {
        var context = await GetSyncContext();

        await Assert.ThrowsAsync<OfflineStoreException>(() => context.ReplaceItemAsync("test", jsonObject));
        Assert.Null(store.FindInQueue(TableOperationKind.Update, "test", testObject.Id));
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ReplacesInStoreAndEnqueues_WithoutVersion()
    {
        store.Upsert("test", new[] { jsonObject });
        var context = await GetSyncContext();

        JObject replacement = (JObject)jsonObject.DeepClone();
        replacement["stringValue"] = "replaced";
        replacement.Remove("version");

        await context.ReplaceItemAsync("test", replacement);

        var item = await store.GetItemAsync("test", testObject.Id);
        Assert.True(JToken.DeepEquals(replacement, item));
        Assert.NotNull(store.FindInQueue(TableOperationKind.Update, "test", testObject.Id));
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsOfflineStore_WhenOtherError()
    {
        store.Upsert("test", new[] { jsonObject });
        var context = await GetSyncContext();

        JObject replacement = (JObject)jsonObject.DeepClone();
        replacement["stringValue"] = "replaced";
        replacement.Remove("version");

        store.ExceptionToThrow = new ApplicationException();

        var ex = await Assert.ThrowsAsync<OfflineStoreException>(() => context.ReplaceItemAsync("test", replacement));
        Assert.Same(store.ExceptionToThrow, ex.InnerException);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ReplacesInStoreAndEnqueues_WithVersion()
    {
        store.Upsert("test", new[] { jsonObject });
        var context = await GetSyncContext();

        JObject replacement = (JObject)jsonObject.DeepClone();
        replacement["stringValue"] = "replaced";
        replacement["version"] = "etag";

        await context.ReplaceItemAsync("test", replacement);

        var item = await store.GetItemAsync("test", testObject.Id);
        Assert.True(JToken.DeepEquals(replacement, item));
        Assert.NotNull(store.FindInQueue(TableOperationKind.Update, "test", testObject.Id));
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_WithExistingInsert_Collapses()
    {
        var context = await GetSyncContext();

        await context.InsertItemAsync("test", jsonObject);

        await context.ReplaceItemAsync("test", jsonObject);

        var items = store.TableMap[SystemTables.OperationsQueue].Values.ToArray();
        Assert.Single(items);
        Assert.Equal(2, items[0].Value<int>("kind"));
        Assert.Equal(testObject.Id, items[0].Value<string>("itemId"));
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_WithExistingUpdate_Collapses()
    {
        var context = await GetSyncContext();

        await context.InsertItemAsync("test", jsonObject);
        store.TableMap[SystemTables.OperationsQueue].Clear();     // fake the push
        await context.ReplaceItemAsync("test", jsonObject);

        await context.ReplaceItemAsync("test", jsonObject);

        var items = store.TableMap[SystemTables.OperationsQueue].Values.ToArray();
        Assert.Single(items);
        Assert.Equal(3, items[0].Value<int>("kind"));
        Assert.Equal(testObject.Id, items[0].Value<string>("itemId"));
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_WithExistingDelete_Throws()
    {
        store.Upsert("test", new[] { jsonObject });
        var context = await GetSyncContext();

        await context.DeleteItemAsync("test", jsonObject);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.ReplaceItemAsync("test", jsonObject));
    }
    #endregion

    #region DiscardTableOperationsAsync
    [Fact]
    [Trait("Method", "DiscardTableOperationsAsync")]
    public async Task DiscardTableOperations_WorksWithNoItems()
    {
        var context = await GetSyncContext();
        await context.DiscardTableOperationsAsync("test", default).ConfigureAwait(false);
        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
    }

    [Fact]
    [Trait("Method", "DiscardTableOperationsAsync")]
    public async Task DiscardTableOperations_WorksWithItems()
    {
        AddRandomOperations("test", 10);
        var context = await GetSyncContext();
        await context.DiscardTableOperationsAsync("test", default).ConfigureAwait(false);
        Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
    }

    [Fact]
    [Trait("Method", "DiscardTableOperationsAsync")]
    public async Task DiscardTableOperations_WorksWithMultipleTables()
    {
        AddRandomOperations("test", 10);
        AddRandomOperations("movies", 10);
        var context = await GetSyncContext();

        await context.DiscardTableOperationsAsync("test", default).ConfigureAwait(false);
        foreach (var kv in store.TableMap[SystemTables.OperationsQueue])
        {
            Assert.Equal("movies", kv.Value.Value<string>("tableName"));
        }
        Assert.Equal(10, store.TableMap[SystemTables.OperationsQueue].Count);
    }
    #endregion

    #region GetQueryIdFromQuery
    [Fact]
    [Trait("Method", "GetQueryIdFromQuery")]
    public void GetQueryId_ProducesQueryId()
    {
        const string tableName = "movies";
        const string query = "$filter=(id eq '1234')";

        Assert.NotEmpty(SyncContext.GetQueryIdFromQuery(tableName, query));
    }

    [Fact]
    [Trait("Method", "GetQueryIdFromQuery")]
    public void GetQueryId_ProducesSameQueryId()
    {
        const string tableName = "movies";
        const string query = "$filter=(id eq '1234')";

        var expected = SyncContext.GetQueryIdFromQuery(tableName, query);
        Assert.NotEmpty(expected);
        var queryId = SyncContext.GetQueryIdFromQuery(tableName, query);
        Assert.Equal(expected, queryId);
    }

    [Fact]
    [Trait("Method", "GetQueryIdFromQuery")]
    public void GetQueryId_DiffersWhenDifferentQueries()
    {
        const string tableName = "movies";
        const string query1 = "$filter=(id eq '1234')";
        const string query2 = "$filter=(id eq '4323')";

        var queryId1 = SyncContext.GetQueryIdFromQuery(tableName, query1);
        var queryId2 = SyncContext.GetQueryIdFromQuery(tableName, query2);
        Assert.NotEqual(queryId1, queryId2);
    }
    #endregion

    #region TableIsDirtyAsync
    [Fact]
    [Trait("Method", "TableIsDirtyAsync")]
    public async Task TableIsDirty_ReturnsFalseWithEmptyTable()
    {
        var context = await GetSyncContext();

        var actual = await context.TableIsDirtyAsync("movies", default);

        Assert.False(actual);
    }

    [Fact]
    [Trait("Method", "TableIsDirtyAsync")]
    public async Task TableIsDirty_ReturnsFalseWithNoTableOperations()
    {
        AddRandomOperations("test", 10);
        var context = await GetSyncContext();

        var actual = await context.TableIsDirtyAsync("movies", default);

        Assert.False(actual);
    }

    [Fact]
    [Trait("Method", "TableIsDirtyAsync")]
    public async Task TableIsDirty_ReturnsTrueWithTableOperations()
    {
        AddRandomOperations("test", 10);
        var context = await GetSyncContext();

        var actual = await context.TableIsDirtyAsync("test", default);

        Assert.True(actual);
    }

    [Fact]
    [Trait("Method", "TableIsDirtyAsync")]
    public async Task TableIsDirty_GetsInfoFromDb()
    {
        AddRandomOperations("test", 10);
        var context = await GetSyncContext();

        var actual = await context.TableIsDirtyAsync("test", default);

        Assert.True(actual);

        store.TableMap[SystemTables.OperationsQueue].Clear();

        actual = await context.TableIsDirtyAsync("test", default);

        Assert.False(actual);
    }
    #endregion

    #region ExecutePushOperationAsync
    [Fact]
    [Trait("Method", "ExecutePushOperationAsync")]
    public async Task ExecutePushOperation_ReturnsFalse_WhenOperationIsCancelled()
    {
        var context = new CSyncContext(client, store);
        await context.InitializeAsync();
        var batch = new OperationBatch(context);
        var op = new DeleteOperation("movies", "abc123");
        op.Cancel();

        Assert.False(await context.CExecutePushOperationAsync(op, batch, false, CancellationToken.None));
    }

    [Fact]
    [Trait("Method", "ExecutePushOperationAsync")]
    public async Task ExecutePushOperation_ReturnsFalse_WhenBatchIsCancelled()
    {
        var context = new CSyncContext(client, store);
        await context.InitializeAsync();
        var batch = new OperationBatch(context);
        var op = new DeleteOperation("movies", "abc123");
        CancellationToken token = new(true);

        Assert.False(await context.CExecutePushOperationAsync(op, batch, false, token));
    }

    [Fact]
    [Trait("Method", "ExecutePushOperationAsync")]
    public async Task ExecutePushOperation_AbortsBatch_WhenStoreError()
    {
        var context = new CSyncContext(client, store);
        await context.InitializeAsync();
        var batch = new OperationBatch(context);
        var op = new DeleteOperation("movies", "abc123");
        store.ReadExceptionToThrow = new ApplicationException();

        var ex = await Assert.ThrowsAsync<OfflineStoreException>(() => context.CExecutePushOperationAsync(op, batch, false, CancellationToken.None));

        Assert.Equal(PushStatus.CancelledByOfflineStoreError, batch.AbortReason);
        Assert.Same(store.ReadExceptionToThrow, ex.InnerException);
    }

    [Fact]
    [Trait("Method", "ExecutePushOperationAsync")]
    public async Task ExecutePushOperation_AddsError_WhenNoItem()
    {
        var context = new CSyncContext(client, store);
        await context.InitializeAsync();
        var batch = new OperationBatch(context);
        var op = new DeleteOperation("movies", "abc123");

        Assert.False(await context.CExecutePushOperationAsync(op, batch, false, CancellationToken.None));
        Assert.Single(store.TableMap[SystemTables.SyncErrors]);
    }

    [Fact]
    [Trait("Method", "ExecutePushOperationAsync")]
    public async Task ExecutePushOperation_AbortsBatch_OnTimeoutException()
    {
        var context = new CSyncContext(client, store);
        await context.InitializeAsync();
        var item = new IdEntity { Id = Guid.NewGuid().ToString() };
        store.Upsert("movies", new[] { (JObject)client.Serializer.Serialize(item) });
        var batch = new OperationBatch(context);
        var op = new CInsertOperation("movies", item.Id) { ExceptionToThrow = new TimeoutException() };

        Assert.False(await context.CExecutePushOperationAsync(op, batch, true, CancellationToken.None));
        Assert.Equal(PushStatus.CancelledByNetworkError, batch.AbortReason);
    }

    [Fact]
    [Trait("Method", "ExecutePushOperationAsync")]
    public async Task ExecutePushOperation_AbortsBatch_OnHttpException()
    {
        var context = new CSyncContext(client, store);
        await context.InitializeAsync();
        var item = new IdEntity { Id = Guid.NewGuid().ToString() };
        store.Upsert("movies", new[] { (JObject)client.Serializer.Serialize(item) });
        var batch = new OperationBatch(context);
        var op = new CInsertOperation("movies", item.Id) { ExceptionToThrow = new HttpRequestException() };

        Assert.False(await context.CExecutePushOperationAsync(op, batch, true, CancellationToken.None));
        Assert.Equal(PushStatus.CancelledByNetworkError, batch.AbortReason);
    }

    [Fact]
    [Trait("Method", "ExecutePushOperationAsync")]
    public async Task ExecutePushOperation_AbortsBatch_OnAuthError()
    {
        var context = new CSyncContext(client, store);
        await context.InitializeAsync();
        var item = new IdEntity { Id = Guid.NewGuid().ToString() };
        store.Upsert("movies", new[] { (JObject)client.Serializer.Serialize(item) });
        var batch = new OperationBatch(context);
        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        var op = new CInsertOperation("movies", item.Id) { ExceptionToThrow = new DatasyncInvalidOperationException("Unauthorized", null, response) };

        Assert.False(await context.CExecutePushOperationAsync(op, batch, true, CancellationToken.None));
        Assert.Equal(PushStatus.CancelledByAuthenticationError, batch.AbortReason);
    }

    [Fact]
    [Trait("Method", "ExecutePushOperationAsync")]
    public async Task ExecutePushOperation_OnInvalidOperationError()
    {
        var context = new CSyncContext(client, store);
        await context.InitializeAsync();
        var item = new IdEntity { Id = Guid.NewGuid().ToString() };
        store.Upsert("movies", new[] { (JObject)client.Serializer.Serialize(item) });
        var batch = new OperationBatch(context);
        var op = new CInsertOperation("movies", item.Id) { ExceptionToThrow = new DatasyncInvalidOperationException("Unauthorized", null, null) };

        Assert.False(await context.CExecutePushOperationAsync(op, batch, true, CancellationToken.None));
        Assert.Single(store.TableMap[SystemTables.OperationsQueue]);
        Assert.Empty(store.TableMap[SystemTables.SyncErrors]);
    }

    [Fact]
    [Trait("Method", "ExecutePushOperationAsync")]
    public async Task ExecutePushOperation_AbortsBatch_OnPushError()
    {
        var context = new CSyncContext(client, store);
        await context.InitializeAsync();
        var item = new IdEntity { Id = Guid.NewGuid().ToString() };
        store.Upsert("movies", new[] { (JObject)client.Serializer.Serialize(item) });
        var batch = new OperationBatch(context);
        var op = new CInsertOperation("movies", item.Id) { ExceptionToThrow = new PushAbortedException() };

        Assert.False(await context.CExecutePushOperationAsync(op, batch, true, CancellationToken.None));
        Assert.Equal(PushStatus.CancelledByOperation, batch.AbortReason);
    }

    [Fact]
    [Trait("Method", "ExecutePushOperationAsync")]
    public async Task ExecutePushOperation_AbortsBatch_WhenUpsertFails()
    {
        var context = new CSyncContext(client, store);
        await context.InitializeAsync();
        var item = new IdEntity { Id = Guid.NewGuid().ToString() };
        store.Upsert("movies", new[] { (JObject)client.Serializer.Serialize(item) });
        var batch = new OperationBatch(context);
        var instance = client.Serializer.Serialize(item);
        var op = new CInsertOperation("movies", item.Id)
        {
            ResponseObjectFunc = () =>
            {
                store.ExceptionToThrow = new ApplicationException();
                return instance;
            }
        };
        store.Upsert(SystemTables.OperationsQueue, new[] { op.Serialize() });

        var ex = await Assert.ThrowsAsync<OfflineStoreException>(() => context.CExecutePushOperationAsync(op, batch, true, CancellationToken.None));
        Assert.Equal(PushStatus.CancelledByOfflineStoreError, batch.AbortReason);
        Assert.Same(store.ExceptionToThrow, ex.InnerException);
    }
    #endregion

    #region CancelAndDiscardItemAsync
    [Fact]
    [Trait("Method", "CancelAndDiscardItemAsync")]
    public async Task CancelAndDiscardItem_Throws_OnNullError()
    {
        var context = await GetSyncContext();
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.CancelAndDiscardItemAsync(null));
    }

    [Fact]
    [Trait("Method", "CancelAndDiscardItemAsync")]
    public async Task CancelAndDiscardItem_Throws_OnNullItem()
    {
        var context = await GetSyncContext();
        var operation = new DeleteOperation("movies", "1234") { Item = null };
        var error = new TableOperationError(operation, context, null, null, null);
        await Assert.ThrowsAsync<ArgumentException>(() => context.CancelAndDiscardItemAsync(error));
    }

    [Fact]
    [Trait("Method", "CancelAndDiscardItemAsync")]
    public async Task CancelAndDiscardItem_Throws_IfOperationHasBeenUpdated()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        var instance = (JObject)client.Serializer.Serialize(item);
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await context.InsertItemAsync("movies", instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync((string[])null));
        var tableError = ex.PushResult.Errors.First();

        // Update the version in the operation
        store.TableMap[SystemTables.OperationsQueue][tableError.Id]["version"] = 3;

        // Now try to cancel and discard.
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.CancelAndDiscardItemAsync(tableError));
    }

    [Fact]
    [Trait("Method", "CancelAndDiscardItemAsync")]
    public async Task CancelAndDiscardItem_Works_Normally()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        var instance = (JObject)client.Serializer.Serialize(item);
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await context.InsertItemAsync("movies", instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync((string[])null));
        var tableError = ex.PushResult.Errors.First();

        await context.CancelAndDiscardItemAsync(tableError);

        // the tableError is not in the __errors table
        Assert.False(store.TableMap[SystemTables.SyncErrors].ContainsKey(tableError.Id));

        // the tableError.Id is not store
        Assert.False(store.TableMap[SystemTables.OperationsQueue].ContainsKey(tableError.Id));

        // the item we added is not in the store
        Assert.False(store.TableMap["movies"].ContainsKey(item.Id));
    }
    #endregion

    #region CancelAndUpdateItemAsync
    [Fact]
    [Trait("Method", "CancelAndUpdateItemAsync")]
    public async Task CancelAndUpdateItem_Throws_OnNullError()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var instance = (JObject)client.Serializer.Serialize(item);

        await Assert.ThrowsAsync<ArgumentNullException>(() => context.CancelAndUpdateItemAsync(null, instance));
    }

    [Fact]
    [Trait("Method", "CancelAndUpdateItemAsync")]
    public async Task CancelAndUpdateItem_Throws_OnNullInstance()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var instance = (JObject)client.Serializer.Serialize(item);
        var operation = new DeleteOperation("movies", "1234") { Item = instance };
        var error = new TableOperationError(operation, context, null, null, null);
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.CancelAndUpdateItemAsync(error, null));
    }

    [Fact]
    [Trait("Method", "CancelAndUpdateItemAsync")]
    public async Task CancelAndUpdateItem_Throws_OnNullItem()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var instance = (JObject)client.Serializer.Serialize(item);
        var operation = new DeleteOperation("movies", "1234") { Item = null };
        var error = new TableOperationError(operation, context, null, null, null);
        await Assert.ThrowsAsync<ArgumentException>(() => context.CancelAndUpdateItemAsync(error, instance));
    }

    [Fact]
    [Trait("Method", "CancelAndUpdateItemAsync")]
    public async Task CancelAndUpdateItem_Throws_IfOperationHasBeenUpdated()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var instance = (JObject)client.Serializer.Serialize(item);
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await context.InsertItemAsync("movies", instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync((string[])null));
        var tableError = ex.PushResult.Errors.First();

        // Update the version in the operation
        store.TableMap[SystemTables.OperationsQueue][tableError.Id]["version"] = 3;

        // Now try to cancel and update.
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.CancelAndUpdateItemAsync(tableError, instance));
    }

    [Fact]
    [Trait("Method", "CancelAndUpdateItemAsync")]
    public async Task CancelAndUpdateItem_Works_Normally()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        var instance = (JObject)client.Serializer.Serialize(item);
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await context.InsertItemAsync("movies", instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync((string[])null));
        var tableError = ex.PushResult.Errors.First();

        var updatedInstance = (JObject)instance.DeepClone();
        updatedInstance["version"] = "3";
        await context.CancelAndUpdateItemAsync(tableError, updatedInstance);

        // the tableError is not in the __errors table
        Assert.False(store.TableMap[SystemTables.SyncErrors].ContainsKey(tableError.Id));

        // the tableError.Id is not store
        Assert.False(store.TableMap[SystemTables.OperationsQueue].ContainsKey(tableError.Id));

        // the item we added is in the store
        Assert.True(store.TableMap["movies"].ContainsKey(item.Id));
        // and is updated
        Assert.Equal("3", store.TableMap["movies"][item.Id].Value<string>("version"));
    }
    #endregion

    #region UpdateOperationAsync
    [Fact]
    [Trait("Method", "UpdateOperationAsync")]
    public async Task UpdateOperationAsync_Throws_OnNullError()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var instance = (JObject)client.Serializer.Serialize(item);

        await Assert.ThrowsAsync<ArgumentNullException>(() => context.UpdateOperationAsync(null, instance));
    }

    [Fact]
    [Trait("Method", "UpdateOperationAsync")]
    public async Task UpdateOperationAsync_Throws_OnNullInstance()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var instance = (JObject)client.Serializer.Serialize(item);
        var operation = new DeleteOperation("movies", "1234") { Item = instance };
        var error = new TableOperationError(operation, context, null, null, null);
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.UpdateOperationAsync(error, null));
    }

    [Fact]
    [Trait("Method", "UpdateOperationAsync")]
    public async Task UpdateOperationAsync_Throws_IfOperationHasBeenUpdated()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var instance = (JObject)client.Serializer.Serialize(item);
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await context.InsertItemAsync("movies", instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync((string[])null));
        var tableError = ex.PushResult.Errors.First();

        // Update the version in the operation
        store.TableMap[SystemTables.OperationsQueue][tableError.Id]["version"] = 3;

        // Now try to cancel and update.
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.UpdateOperationAsync(tableError, instance));
    }

    [Fact]
    [Trait("Method", "UpdateOperationAsync")]
    public async Task UpdateOperationAsync_Works_Normally()
    {
        var context = await GetSyncContext();
        var item = new ClientMovie { Id = Guid.NewGuid().ToString(), Version = "1" };
        var conflictItem = new ClientMovie { Id = item.Id, Version = "2" };
        var instance = (JObject)client.Serializer.Serialize(item);
        MockHandler.AddResponse(HttpStatusCode.Conflict, conflictItem);

        await context.InsertItemAsync("movies", instance);
        var ex = await Assert.ThrowsAsync<PushFailedException>(() => context.PushItemsAsync((string[])null));
        var tableError = ex.PushResult.Errors.First();

        var updatedInstance = (JObject)instance.DeepClone();
        updatedInstance["version"] = "3";
        await context.UpdateOperationAsync(tableError, updatedInstance);

        // the tableError is not in the __errors table
        Assert.False(store.TableMap[SystemTables.SyncErrors].ContainsKey(tableError.Id));

        // the tableError.Id is updated in the store
        Assert.True(store.TableMap[SystemTables.OperationsQueue].ContainsKey(tableError.Id));
        var operation = await context.OperationsQueue.GetOperationByIdAsync(tableError.Id);
        Assert.Equal(2, operation.Version);

        // the item we added is in the store
        Assert.True(store.TableMap["movies"].ContainsKey(item.Id));
        // and is updated
        Assert.Equal("3", store.TableMap["movies"][item.Id].Value<string>("version"));
    }
    #endregion

    #region IsDisposable
    [Fact]
    public async Task SyncContext_CanBeDisposed()
    {
        var context = await GetSyncContext();
        context.Dispose(); // Should not throw.
    }
    #endregion
}

#region Helper Classes
[ExcludeFromCodeCoverage]
internal class CSyncContext : SyncContext
{
    public CSyncContext(DatasyncClient client, IOfflineStore store) : base(client, store) { }

    public Task<bool> CExecutePushOperationAsync(TableOperation operation, OperationBatch batch, bool removeFromQueueOnSuccess, CancellationToken token = default)
        => base.ExecutePushOperationAsync(operation, batch, removeFromQueueOnSuccess, token);
}

/// <summary>
/// A modified version of the InsertOperation that we can set up an
/// exception on.
/// </summary>
/// <seealso cref="Microsoft.Datasync.Client.Offline.Queue.InsertOperation" />
[ExcludeFromCodeCoverage]
internal class CInsertOperation : InsertOperation
{
    [JsonIgnore]
    public Exception ExceptionToThrow { get; set; }

    [JsonIgnore]
    public Func<JToken> ResponseObjectFunc { get; set; }

    public CInsertOperation(string tableName, string itemId) : base(tableName, itemId) { }

    protected override Task<JToken> ExecuteRemoteOperationAsync(IRemoteTable table, CancellationToken cancellationToken)
    {
        if (ExceptionToThrow != null)
        {
            throw ExceptionToThrow;
        }
        if (ResponseObjectFunc != null)
        {
            return Task.FromResult(ResponseObjectFunc.Invoke());
        }
        return base.ExecuteRemoteOperationAsync(table, cancellationToken);
    }
}

[ExcludeFromCodeCoverage]
internal class MockOfflineStore_WithDeltaToken : MockOfflineStore, IDeltaTokenStoreProvider
{
    internal bool DeltaTokenStoreInitialized { get; set; } = false;

    public Task<IDeltaTokenStore> GetDeltaTokenStoreAsync(CancellationToken cancellationToken = default)
    {
        DeltaTokenStoreInitialized = true;
        var store = new DefaultDeltaTokenStore(this);
        return Task.FromResult<IDeltaTokenStore>(store);
    }
}
#endregion

﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Offline
{
    [ExcludeFromCodeCoverage]
    public class SyncContext_Tests : BaseTest
    {
        private readonly DatasyncClient client;
        private readonly MockOfflineStore store;
        private readonly Random rnd = new();

        private readonly IdEntity testObject;
        private readonly JObject jsonObject;

        public SyncContext_Tests()
        {
            client = GetMockClient();
            store = new MockOfflineStore();

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
        private List<JObject> AddRandomRecords(string tableName, int itemCount = 0)
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
            var token = JObject.Parse($"{{\"id\":\"dt.{tableName}.{qid}\",\"value\":\"2021-03-12T03:30:04.000Z\"}}");
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

            await context.InitializeAsync();

            Assert.Same(opQueue, context.OperationsQueue);
            Assert.Same(tokenStore, context.DeltaTokenStore);
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
        public async Task GetItemAsync_Throws_WhenNotInitialized()
        {
            var context = new SyncContext(client, store);
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.GetItemAsync("test", "1234"));
        }

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
        public async Task GetNextPageAsync_Throws_WhenNotInitialized()
        {
            var context = new SyncContext(client, store);
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.GetNextPageAsync("test", "$count=true"));
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
        public async Task InsertItemAsync_Throws_WhenNotInitialized()
        {
            var context = new SyncContext(client, store);

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.InsertItemAsync("test", jsonObject));
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

            await context.PullItemsAsync("movies", "", new PullOptions());

            // Items were pulled.
            var storedEntities = store.TableMap["movies"]?.Values.ToList() ?? new List<JObject>();
            Assert.Empty(storedEntities);

            // Delta Token was not stored.
            Assert.Empty(store.TableMap[SystemTables.Configuration]);

            // Query was correct
            Assert.Single(MockHandler.Requests);
            Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset))&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
        }

        [Fact]
        [Trait("method", "PullItemsAsync")]
        public async Task PullItemsAsync_WithFilter_NoResponse_Works()
        {
            var context = await GetSyncContext();
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity> { Items = new List<IdEntity>() });
            store.GetOrCreateTable("movies");

            await context.PullItemsAsync("movies", "$filter=(rating eq 'PG-13')", new PullOptions());

            // Items were pulled.
            var storedEntities = store.TableMap["movies"]?.Values.ToList() ?? new List<JObject>();
            Assert.Empty(storedEntities);

            // Delta Token was not stored.
            Assert.Empty(store.TableMap[SystemTables.Configuration]);

            // Query was correct
            Assert.Single(MockHandler.Requests);
            Assert.Equal("/tables/movies?$filter=((rating eq 'PG-13') and (updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset)))&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
        }

        [Fact]
        [Trait("Method", "PullItemsAsync")]
        public async Task PullItemsAsync_ProducesCorrectQuery()
        {
            var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
            var context = await GetSyncContext();
            var items = CreatePageOfMovies(10, lastUpdatedAt);

            await context.PullItemsAsync("movies", "", new PullOptions());

            // Items were pulled.
            var storedEntities = store.TableMap["movies"].Values.ToList();
            AssertEx.SequenceEqual(items, storedEntities);

            // Delta Token was stored - it's stored as UnixTimeMilliseconds().
            Assert.Single(store.TableMap[SystemTables.Configuration].Values);
            var deltaToken = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(store.TableMap[SystemTables.Configuration].Values.FirstOrDefault()?.Value<string>("value")));
            Assert.Equal(lastUpdatedAt.ToUniversalTime(), deltaToken.ToUniversalTime());

            // Query was correct
            Assert.Single(MockHandler.Requests);
            Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset))&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
        }

        [Fact]
        [Trait("Method", "PullItemsAsync")]
        public async Task PullItemsAsync_ProducesCorrectQuery_WithoutUpdatedAt()
        {
            var context = await GetSyncContext();
            _ = CreatePageOfItems(10);

            await context.PullItemsAsync("movies", "", new PullOptions());

            // Delta Token was not stored
            var table = store.GetOrCreateTable(SystemTables.Configuration);
            Assert.Empty(table);

            // Query was correct
            Assert.Single(MockHandler.Requests);
            Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset))&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
        }

        [Fact]
        [Trait("Method", "PullItemsAsync")]
        public async Task PullItemsAsync_UsesQueryId()
        {
            var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
            var context = await GetSyncContext();
            var options = new PullOptions { QueryId = "abc123" };
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
            Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset))&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
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
            Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(2021-03-24T12:50:44.000Z,Edm.DateTimeOffset))&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
        }

        [Fact]
        [Trait("Method", "PullItemsAsync")]
        public async Task PullItemsAsync_Overwrites_Data()
        {
            var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
            var context = await GetSyncContext();
            var items = CreatePageOfMovies(10, lastUpdatedAt, 3);
            store.Upsert("movies", items); // store the 10 items in the store

            await context.PullItemsAsync("movies", "", new PullOptions());

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
            Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset))&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
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

            var pushContext = new Mock<IPushContext>();
            pushContext.Setup(x => x.PushItemsAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            context.PushContext = pushContext.Object;

            // We don't "consume" the operation, so PullItemsAsync() will throw an invalid operation because the table is dirty the second time.
            await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => context.PullItemsAsync("movies", "", new PullOptions()));

            // PushItemsAsync should be called once.
            Assert.Single(pushContext.Invocations);

            // Since we didn't alter the movies, then args[0] should be string[] { "movies" }
            var relatedTables = pushContext.Invocations[0].Arguments[0] as string[];
            Assert.Single(relatedTables);
            Assert.Equal("movies", relatedTables[0]);
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

            var pushContext = new Mock<IPushContext>();
            pushContext.Setup(x => x.PushItemsAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            context.PushContext = pushContext.Object;

            // We don't "consume" the operation, so PullItemsAsync() will throw an invalid operation because the table is dirty the second time.
            var options = new PullOptions { PushOtherTables = true };
            await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => context.PullItemsAsync("movies", "",options));

            // PushItemsAsync should be called once.
            Assert.Single(pushContext.Invocations);

            // Since we are pushing other tables, the argument should be null
            Assert.Null(pushContext.Invocations[0].Arguments[0]);
        }

        [Fact]
        [Trait("Method", "PullItemsAsync")]
        public async Task PullItemsAsync_CallsPush_WhenDirty_ThenCleaned()
        {
            AddRandomOperations("movies", 1);

            var lastUpdatedAt = DateTimeOffset.Parse("2021-03-24T12:50:44.000+00:00");
            var context = await GetSyncContext();
            var options = new PullOptions { QueryId = "abc123" };
            const string keyId = "dt.movies.abc123";
            var items = CreatePageOfMovies(10, lastUpdatedAt);

            var pushContext = new Mock<IPushContext>();
            pushContext.Setup(x => x.PushItemsAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>())).Returns(() =>
            {
                store.TableMap[SystemTables.OperationsQueue].Clear();
                return Task.CompletedTask;
            });
            context.PushContext = pushContext.Object;

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
            Assert.Equal("/tables/movies?$filter=(updatedAt gt cast(1970-01-01T00:00:00.000Z,Edm.DateTimeOffset))&__includedeleted=true", Uri.UnescapeDataString(MockHandler.Requests[0].RequestUri.PathAndQuery));
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

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.PurgeItemsAsync(tableName, query, options));

            Assert.NotEmpty(store.TableMap["test"]);
            Assert.NotEmpty(store.TableMap[SystemTables.OperationsQueue]);
            Assert.NotEmpty(store.TableMap[SystemTables.Configuration]["dt.test.abc123"]);
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

            await context.PurgeItemsAsync(tableName, query, options);

            Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
            Assert.NotEmpty(store.TableMap[SystemTables.Configuration]["dt.test.abc123"]);
            Assert.Empty(store.TableMap[tableName]);
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

            await context.PurgeItemsAsync(tableName, query, options);

            Assert.Empty(store.TableMap[SystemTables.OperationsQueue]);
            Assert.Empty(store.TableMap[SystemTables.Configuration]);
            Assert.Empty(store.TableMap[tableName]);
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

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.PurgeItemsAsync(tableName, query, options));

            Assert.NotEmpty(store.TableMap["test"]);
            Assert.NotEmpty(store.TableMap[SystemTables.OperationsQueue]);
            Assert.NotEmpty(store.TableMap[SystemTables.Configuration]["dt.test.abc123"]);
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

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.PurgeItemsAsync(tableName, query, options));

            Assert.NotEmpty(store.TableMap["test"]);
            Assert.NotEmpty(store.TableMap[SystemTables.OperationsQueue]);
            Assert.NotEmpty(store.TableMap[SystemTables.Configuration]["dt.test.abc123"]);
        }
        #endregion

        #region ReplaceItemAsync
        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Throws_WhenNotInitialized()
        {
            var context = new SyncContext(client, store);

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.ReplaceItemAsync("test", jsonObject));
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

        #region IsDisposable
        [Fact]
        public async Task SyncContext_CanBeDisposed()
        {
            var context = await GetSyncContext();
            context.Dispose(); // Should not throw.
        }
        #endregion
    }
}

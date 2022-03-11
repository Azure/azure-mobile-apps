// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Offline
{
    [ExcludeFromCodeCoverage]
    public class SyncContext_Tests : BaseTest
    {
        private readonly DatasyncClient client;
        private readonly MockOfflineStore store;

        private readonly IdEntity testObject;
        private readonly JObject jsonObject;
        private readonly string jsonString;

        public SyncContext_Tests() : base()
        {
            client = GetMockClient();
            store = new MockOfflineStore();

            testObject = new IdEntity { Id = Guid.NewGuid().ToString("N"), StringValue = "testValue" };
            jsonObject = (JObject)client.Serializer.Serialize(testObject);
            jsonString = jsonObject.ToString(Formatting.None);
        }

        [Fact]
        public void Ctor_NullArgs_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SyncContext(null, store));
            Assert.Throws<ArgumentNullException>(() => new SyncContext(client, null));
        }

        [Fact]
        public void Ctor_SetsUpContext()
        {
            var context = new SyncContext(client, store);

            Assert.NotNull(context);
            Assert.False(context.IsInitialized);
            Assert.Same(store, context.OfflineStore);
            Assert.Same(client, context.ServiceClient);
        }

        [Fact]
        public async Task InitializeAsync_Works()
        {
            var context = new SyncContext(client, store);

            await context.InitializeAsync();

            Assert.True(context.IsInitialized);
            Assert.NotNull(context.OperationsQueue);
        }

        [Fact]
        public async Task InitializeAsync_CanBeCalledTwice()
        {
            var context = new SyncContext(client, store);

            await context.InitializeAsync();

            var opQueue = context.OperationsQueue;

            await context.InitializeAsync();

            Assert.Same(opQueue, context.OperationsQueue);
        }

        [Fact]
        public async Task DeleteItemAsync_Throws_WhenNotInitialized()
        {
            var context = new SyncContext(client, store);

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.DeleteItemAsync("test", jsonObject));
        }

        [Fact]
        public async Task DeleteItemAsync_Throws_WhenNoId()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            jsonObject.Remove("id");

            await Assert.ThrowsAsync<ArgumentException>(() => context.DeleteItemAsync("test", jsonObject));
        }

        [Fact]
        public async Task DeleteItemAsync_Throws_WhenItemDoesNotExist()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.DeleteItemAsync("test", jsonObject));
        }

        [Fact]
        public async Task DeleteItemAsync_DeletesFromStoreAndEnqueues()
        {
            store.Upsert("test", new[] { jsonObject });

            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            await context.DeleteItemAsync("test", jsonObject);

            Assert.False(store.TableContains("test", jsonObject));
            Assert.NotNull(store.FindInQueue(TableOperationKind.Delete, "test", testObject.Id));
        }

        [Fact]
        public async Task DeleteItemAsync_ThrowsOfflineStore_WhenOtherError()
        {
            store.Upsert("test", new[] { jsonObject });

            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            store.ExceptionToThrow = new ApplicationException();

            var ex = await Assert.ThrowsAsync<OfflineStoreException>(() => context.DeleteItemAsync("test", jsonObject));
            Assert.Same(store.ExceptionToThrow, ex.InnerException);
        }

        [Fact]
        public async Task DeleteItemAsync_WithExistingInsert_Collapses()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();
            await context.InsertItemAsync("test", jsonObject);

            await context.DeleteItemAsync("test", jsonObject);

            var items = store.TableMap[SystemTables.OperationsQueue].Values.ToArray();
            Assert.Empty(items);
        }

        [Fact]
        public async Task DeleteItemAsync_WithExistingUpdate_Collapses()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();
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
        public async Task DeleteItemAsync_WithExistingDelete_Throws()
        {
            store.Upsert("test", new[] { jsonObject });
            var context = new SyncContext(client, store);
            await context.InitializeAsync();
            await context.DeleteItemAsync("test", jsonObject);

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.DeleteItemAsync("test", jsonObject));
        }

        [Fact]
        public async Task GetItemAsync_Throws_WhenNotInitialized()
        {
            var context = new SyncContext(client, store);
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.GetItemAsync("test", "1234"));
        }

        [Fact]
        public async Task GetItemAsync_ReturnsNull_ForMissingItem()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            var result = await context.GetItemAsync("test", "1234");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetItemAsync_ReturnsItem()
        {
            store.Upsert("test", new[] { jsonObject });

            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            var result = await context.GetItemAsync("test", testObject.Id);
            Assert.Equal(jsonObject, result);
        }

        [Fact]
        public async Task GetNextPageAsync_Throws_WhenNotInitialized()
        {
            var context = new SyncContext(client, store);
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.GetNextPageAsync("test", "$count=true"));
        }

        [Fact]
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
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            var result = await context.GetNextPageAsync("test", "");
            Assert.NotNull(result);
            Assert.Null(result.Count);
            Assert.Equal(items, result.Items);

            Assert.Equal(1, callCount);
            Assert.NotNull(storedQuery);
            Assert.False(storedQuery.IncludeTotalCount);
        }

        [Fact]
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
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            var result = await context.GetNextPageAsync("test", "$count=true");
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Equal(items, result.Items);

            Assert.Equal(1, callCount);
            Assert.NotNull(storedQuery);
            Assert.True(storedQuery.IncludeTotalCount);
        }

        [Fact]
        public async Task InsertItemAsync_Throws_WhenNotInitialized()
        {
            var context = new SyncContext(client, store);

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.InsertItemAsync("test", jsonObject));
        }

        [Fact]
        public async Task InsertItemAsync_Throws_WhenItemExists()
        {
            store.Upsert("test", new[] { jsonObject });
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            await Assert.ThrowsAsync<OfflineStoreException>(() => context.InsertItemAsync("test", jsonObject));
            Assert.Null(store.FindInQueue(TableOperationKind.Insert, "test", testObject.Id));
        }

        [Fact]
        public async Task InsertItemAsync_InsertsToStoreAndEnqueues_WithId()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            await context.InsertItemAsync("test", jsonObject);

            Assert.True(store.TableContains("test", jsonObject));
            Assert.NotNull(store.FindInQueue(TableOperationKind.Insert, "test", testObject.Id));
        }

        [Fact]
        public async Task InsertItemAsync_InsertsToStoreAndEnqueues_WithNoId()
        {
            jsonObject.Remove("id");
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            await context.InsertItemAsync("test", jsonObject);

            var itemId = store.TableMap["test"].Values.FirstOrDefault()?.Value<string>("id");
            Assert.NotEmpty(itemId);
            Assert.NotNull(store.FindInQueue(TableOperationKind.Insert, "test", itemId));
        }

        [Fact]
        public async Task InsertItemAsync_ThrowsOfflineStore_WhenOtherError()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            store.ExceptionToThrow = new ApplicationException();

            var ex = await Assert.ThrowsAsync<OfflineStoreException>(() => context.InsertItemAsync("test", jsonObject));
            Assert.Same(store.ExceptionToThrow, ex.InnerException);
        }

        [Fact]
        public async Task InsertItemAsync_WithExistingInsert_Throws()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();
            await context.InsertItemAsync("test", jsonObject);

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.InsertItemAsync("test", jsonObject));
        }

        [Fact]
        public async Task InsertItemAsync_WithExistingUpdate_Throws()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();
            await context.InsertItemAsync("test", jsonObject);
            store.TableMap[SystemTables.OperationsQueue].Clear();     // fake the push
            await context.ReplaceItemAsync("test", jsonObject);

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.InsertItemAsync("test", jsonObject));
        }

        [Fact]
        public async Task InsertItemAsync_WithExistingDelete_Throws()
        {
            store.Upsert("test", new[] { jsonObject });
            var context = new SyncContext(client, store);
            await context.InitializeAsync();
            await context.DeleteItemAsync("test", jsonObject);

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.InsertItemAsync("test", jsonObject));
        }

        [Fact]
        public async Task ReplaceItemAsync_Throws_WhenNotInitialized()
        {
            var context = new SyncContext(client, store);

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.ReplaceItemAsync("test", jsonObject));
        }

        [Fact]
        public async Task ReplaceItemAsync_Throws_WhenNoId()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            jsonObject.Remove("id");

            await Assert.ThrowsAsync<ArgumentException>(() => context.ReplaceItemAsync("test", jsonObject));
        }

        [Fact]
        public async Task ReplaceItemAsync_Throws_WhenItemDoesNotExists()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            await Assert.ThrowsAsync<OfflineStoreException>(() => context.ReplaceItemAsync("test", jsonObject));
            Assert.Null(store.FindInQueue(TableOperationKind.Update, "test", testObject.Id));
        }

        [Fact]
        public async Task ReplaceItemAsync_ReplacesInStoreAndEnqueues_WithoutVersion()
        {
            store.Upsert("test", new[] { jsonObject });
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            JObject replacement = (JObject)jsonObject.DeepClone();
            replacement["stringValue"] = "replaced";
            replacement.Remove("version");

            await context.ReplaceItemAsync("test", replacement);

            var item = await store.GetItemAsync("test", testObject.Id);
            Assert.True(JToken.DeepEquals(replacement, item));
            Assert.NotNull(store.FindInQueue(TableOperationKind.Update, "test", testObject.Id));
        }

        [Fact]
        public async Task ReplaceItemAsync_ThrowsOfflineStore_WhenOtherError()
        {
            store.Upsert("test", new[] { jsonObject });
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            JObject replacement = (JObject)jsonObject.DeepClone();
            replacement["stringValue"] = "replaced";
            replacement.Remove("version");

            store.ExceptionToThrow = new ApplicationException();

            var ex = await Assert.ThrowsAsync<OfflineStoreException>(() => context.ReplaceItemAsync("test", replacement));
            Assert.Same(store.ExceptionToThrow, ex.InnerException);
        }

        [Fact]
        public async Task ReplaceItemAsync_ReplacesInStoreAndEnqueues_WithVersion()
        {
            store.Upsert("test", new[] { jsonObject });
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            JObject replacement = (JObject)jsonObject.DeepClone();
            replacement["stringValue"] = "replaced";
            replacement["version"] = "etag";

            await context.ReplaceItemAsync("test", replacement);

            var item = await store.GetItemAsync("test", testObject.Id);
            Assert.True(JToken.DeepEquals(replacement, item));
            Assert.NotNull(store.FindInQueue(TableOperationKind.Update, "test", testObject.Id));
        }

        [Fact]
        public async Task ReplaceItemAsync_WithExistingInsert_Collapses()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();
            await context.InsertItemAsync("test", jsonObject);

            await context.ReplaceItemAsync("test", jsonObject);

            var items = store.TableMap[SystemTables.OperationsQueue].Values.ToArray();
            Assert.Single(items);
            Assert.Equal(2, items[0].Value<int>("kind"));
            Assert.Equal(testObject.Id, items[0].Value<string>("itemId"));
        }

        [Fact]
        public async Task ReplaceItemAsync_WithExistingUpdate_Collapses()
        {
            var context = new SyncContext(client, store);
            await context.InitializeAsync();
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
        public async Task ReplaceItemAsync_WithExistingDelete_Throws()
        {
            store.Upsert("test", new[] { jsonObject });
            var context = new SyncContext(client, store);
            await context.InitializeAsync();
            await context.DeleteItemAsync("test", jsonObject);

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.ReplaceItemAsync("test", jsonObject));
        }
    }
}

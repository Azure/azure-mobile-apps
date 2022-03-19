// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Offline.Queue
{
    [ExcludeFromCodeCoverage]
    public class OperationsQueue_Tests : BaseOperationTest
    {
        private readonly MockOfflineStore store = new();

        [Fact]
        [Trait("Method", "TableDefinition")]
        public void TableDefinition_Serializes()
        {
            var actual = TableOperation.TableDefinition.ToString(Formatting.None);
            const string expected = "{\"id\":\"\",\"kind\":0,\"state\":0,\"tableName\":\"\",\"itemId\":\"\",\"item\":\"\",\"sequence\":0,\"version\":0}";

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait("Method", "TableOperation.Deserialize")]
        public void TableOperation_Deserialize_ThrowsOnInvalidKind()
        {
            var json = JObject.Parse("{}");
            Assert.Throws<InvalidOperationException>(() => TableOperation.Deserialize(json));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Queue_NotInitialized_WhenConstructed()
        {
            var sut = new OperationsQueue(store);
            Assert.False(sut.IsInitialized);
            Assert.Equal(0, sut.PendingOperations);
        }

        [Fact]
        [Trait("Method", "InitializeAsync")]
        public async Task Queue_Initialized_WithEmptyStore()
        {
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            Assert.True(sut.IsInitialized);
            Assert.Equal(0, sut.PendingOperations);
        }

        [Fact]
        [Trait("Method", "InitializeAsync")]
        public async Task Queue_Initialized_Twice()
        {
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();
            await sut.InitializeAsync();

            Assert.True(sut.IsInitialized);
            Assert.Equal(0, sut.PendingOperations);
        }

        [Fact]
        [Trait("Method", "InitializeAsync")]
        public async Task Queue_Initialized_WithQueueEntries()
        {
            for (int i = 0; i < 10; i++)
            {
                var operation = new DeleteOperation("test", Guid.NewGuid().ToString()) { Sequence = 100 + i };
                await store.UpsertAsync(SystemTables.OperationsQueue, new[] { operation.Serialize() }, true);
            }

            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            Assert.Equal(10, sut.PendingOperations);
            Assert.Equal(109, sut.SequenceId);
        }

        [Fact]
        [Trait("Method", "CountpendingOperationsAsync")]
        public async Task CountPendingOperationsAsync_Throws_WhenNotInitialized()
        {
            var sut = new OperationsQueue(store);
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CountPendingOperationsAsync("movies"));
        }

        [Theory, CombinatorialData]
        [Trait("Method", "CountPendingOperationsAsync")]
        public async Task CountPendingOperationsAsync_Returns_NumberOfItems([CombinatorialRange(0, 5)] int count)
        {
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var operation = new InsertOperation("test", Guid.NewGuid().ToString());
                    store.Upsert(SystemTables.OperationsQueue, new[] { operation.Serialize() });
                }
            }
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            var actual = await sut.CountPendingOperationsAsync("test");

            Assert.Equal(count, actual);
        }

        [Fact]
        [Trait("Method", "DeleteOperationByIdAsync")]
        public async Task DeleteByIdAsync_Throws_WhenNotInitialized()
        {
            var sut = new OperationsQueue(store);
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.DeleteOperationByIdAsync("1234", 1L));
        }

        [Fact]
        [Trait("Method", "DeleteOperationByIdAsync")]
        public async Task DeleteByIdAsync_Works()
        {
            var operation = new DeleteOperation("test", Guid.NewGuid().ToString());
            store.Upsert(SystemTables.OperationsQueue, new[] { operation.Serialize() });
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            var actual = await sut.DeleteOperationByIdAsync(operation.Id, operation.Version);

            Assert.True(actual);
            Assert.Null(store.FindInQueue(TableOperationKind.Delete, "test", operation.ItemId));
        }

        [Fact]
        [Trait("Method", "DeleteOperationByIdAsync")]
        public async Task DeleteByIdAsync_DoesNotExist()
        {
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            var actual = await sut.DeleteOperationByIdAsync("1234", 1);

            Assert.False(actual);
        }

        [Fact]
        [Trait("Method", "DeleteOperationByIdAsync")]
        public async Task DeleteByIdAsync_VersionMismatch()
        {
            var operation = new DeleteOperation("test", Guid.NewGuid().ToString()) { Version = 23 };
            store.Upsert(SystemTables.OperationsQueue, new[] { operation.Serialize() });
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            var actual = await sut.DeleteOperationByIdAsync(operation.Id, 24);

            Assert.False(actual);
            Assert.Single(store.TableMap[SystemTables.OperationsQueue].Values);
        }

        [Fact]
        [Trait("Method", "DeleteOperationByIdAsync")]
        public async Task DeleteByIdAsync_StoreException()
        {
            var operation = new DeleteOperation("test", Guid.NewGuid().ToString()) { Version = 23 };
            store.Upsert(SystemTables.OperationsQueue, new[] { operation.Serialize() });
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            store.ExceptionToThrow = new ApplicationException();

            await Assert.ThrowsAsync<OfflineStoreException>(() => sut.DeleteOperationByIdAsync(operation.Id, operation.Version));
        }

        [Fact]
        [Trait("Method", "DeleteOperationsAsync")]
        public async Task DeleteOperationsAsync_Throws_WhenNotOperationsQueue()
        {
            var query = new QueryDescription("test");
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.DeleteOperationsAsync(query));
        }

        [Fact]
        [Trait("Method", "GetAsyncOperations")]
        public async Task GetAsyncOperations_Returns_ZeroItems()
        {
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            var items = await sut.GetAsyncOperations("").ToListAsync();
            Assert.Empty(items);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "GetAsyncOperations")]
        public async Task GetAsyncOperations_Returns_Items([CombinatorialValues(5, 20, 50)] int count)
        {
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var operation = new InsertOperation("test", Guid.NewGuid().ToString());
                    store.Upsert(SystemTables.OperationsQueue, new[] { operation.Serialize() });
                }
            }

            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            var items = await sut.GetAsyncOperations("").ToListAsync();
            Assert.Equal(count, items.Count);
        }

        [Fact]
        [Trait("Method", "GetOperationByItemIdAsync")]
        public async Task GetOperationByItemIdAsync_Throws_WhenNotInitialized()
        {
            var sut = new OperationsQueue(store);
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetOperationByItemIdAsync("test", "1234"));
        }

        [Fact]
        [Trait("Method", "GetOperationByItemIdAsync")]
        public async Task GetOperationByItemIdAsync_NullItems()
        {
            var store = new Mock<IOfflineStore>();
            var page = new Page<JObject> { Items = null };
            store.Setup(x => x.GetPageAsync(It.IsAny<QueryDescription>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(page));

            var sut = new OperationsQueue(store.Object);
            await sut.InitializeAsync();
            var actual = await sut.GetOperationByItemIdAsync("test", "1234");
            Assert.Null(actual);
        }

        [Fact]
        [Trait("Method", "GetOperationByItemIdAsync")]
        public async Task GetOperationByItemIdAsync_EmptyItems()
        {
            var store = new Mock<IOfflineStore>();
            var page = new Page<JObject> { Items = Array.Empty<JObject>() };
            store.Setup(x => x.GetPageAsync(It.IsAny<QueryDescription>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(page));

            var sut = new OperationsQueue(store.Object);
            await sut.InitializeAsync();
            var actual = await sut.GetOperationByItemIdAsync("test", "1234");
            Assert.Null(actual);
        }

        [Fact]
        [Trait("Method", "GetOperationByItemIdAsync")]
        public async Task GetOperationByItemIdAsync_JObjectItem()
        {
            var store = new Mock<IOfflineStore>();
            var operation = new InsertOperation("test", "1234");
            var page = new Page<JObject> { Items = new JObject[] { operation.Serialize() } };
            store.Setup(x => x.GetPageAsync(It.IsAny<QueryDescription>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(page));

            var sut = new OperationsQueue(store.Object);
            await sut.InitializeAsync();
            var actual = await sut.GetOperationByItemIdAsync("test", "1234");
            Assert.Equal(operation, actual);
        }

        [Fact]
        [Trait("Method", "UpdateOperationAsync")]
        public async Task UpdateOperationAsync_Throws_WhenNotInitialized()
        {
            var operation = new InsertOperation("test", Guid.NewGuid().ToString());
            var sut = new OperationsQueue(store);
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateOperationAsync(operation));
        }

        [Fact]
        [Trait("Method", "UpdateOperationAsync")]
        public async Task UpdateOperation_StoreOperation()
        {
            var operation = new DeleteOperation("test", Guid.NewGuid().ToString()) { Version = 23 };
            store.Upsert(SystemTables.OperationsQueue, new[] { operation.Serialize() });
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            operation.Sequence = 42;

            await sut.UpdateOperationAsync(operation);

            Assert.True(JToken.DeepEquals(operation.Serialize(), store.TableMap[SystemTables.OperationsQueue].Values.First()));
        }

        [Fact]
        [Trait("Method", "UpdateOperationAsync")]
        public async Task UpdateOperation_StoreException()
        {
            var operation = new DeleteOperation("test", Guid.NewGuid().ToString()) { Version = 23 };
            store.Upsert(SystemTables.OperationsQueue, new[] { operation.Serialize() });
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            store.ExceptionToThrow = new ApplicationException();
            await Assert.ThrowsAsync<OfflineStoreException>(() => sut.UpdateOperationAsync(operation));
        }
    }
}

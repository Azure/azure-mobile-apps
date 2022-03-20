// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.OData;
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
        [Trait("Method", "PeekAsync")]
        public async Task PeekAsync_NegativeSequenceId_Throws()
        {
            var store = new Mock<IOfflineStore>();
            var sut = new OperationsQueue(store.Object);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.PeekAsync(-1, null));
        }

        [Theory]
        [InlineData(0, null, "(sequence gt 0L)")]
        [InlineData(1, null, "(sequence gt 1L)")]
        [InlineData(42, null, "(sequence gt 42L)")]
        [InlineData(0, new string[] { "movies" }, "((sequence gt 0L) and (tableName eq 'movies'))")]
        [InlineData(1, new string[] { "movies" }, "((sequence gt 1L) and (tableName eq 'movies'))")]
        [InlineData(42, new string[] { "movies" }, "((sequence gt 42L) and (tableName eq 'movies'))")]
        [InlineData(500, new string[] { "movies", "test" }, "((sequence gt 500L) and ((tableName eq 'movies') or (tableName eq 'test')))")]
        [Trait("Method", "PeekAsync")]
        public async Task PeekAsync_GeneratesCorrectQuery(long sequenceId, string[] tableNames, string filter)
        {
            var response = new Page<JObject>();
            var store = new Mock<IOfflineStore>();
            store.Setup(x => x.GetPageAsync(It.IsAny<QueryDescription>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
            var sut = new OperationsQueue(store.Object);

            _ = await sut.PeekAsync(sequenceId, tableNames);

            Assert.Single(store.Invocations);
            var invocation = store.Invocations[0];
            var query = invocation.Arguments[0] as QueryDescription;

            Assert.NotNull(query);
            Assert.Equal(SystemTables.OperationsQueue, query.TableName);

            var odata = query.ToODataString();
            var expected = $"$filter={filter}&$orderby=sequence&$top=1";
            Assert.Equal(expected, odata);
        }

        [Fact]
        [Trait("Method", "PeekAsync")]
        public async Task PeekAsync_ReturnsCorrectValue()
        {
            var operation = new DeleteOperation("movies", Guid.NewGuid().ToString());
            var response = new Page<JObject>() { Items = new[] { operation.Serialize() } };
            var store = new Mock<IOfflineStore>();
            store.Setup(x => x.GetPageAsync(It.IsAny<QueryDescription>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
            var sut = new OperationsQueue(store.Object);

            var actual = await sut.PeekAsync(0L, new[] { "movies" });

            Assert.Equal(operation.Id, actual.Id);
        }

        [Fact]
        [Trait("Method", "TryUpdateOperationStateAsync")]
        public async Task TryUpdateOperationState_Throws_NullOperation()
        {
            var store = new Mock<IOfflineStore>();
            var sut = new OperationsQueue(store.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.TryUpdateOperationStateAsync(null, TableOperationState.Failed));
        }

        [Fact]
        [Trait("Method", "TryUpdateOperationStateAsync")]
        public async Task TryUpdateOperationState_SetsNewState_InStore()
        {
            var operation = new DeleteOperation("movies", Guid.NewGuid().ToString());
            var store = new Mock<IOfflineStore>();
            store.Setup(x => x.GetPageAsync(It.IsAny<QueryDescription>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new Page<JObject>()));
            store.Setup(x => x.UpsertAsync(It.IsAny<string>(), It.IsAny<IEnumerable<JObject>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var context = new SyncContext(GetMockClient(), store.Object);
            var sut = new OperationsQueue(store.Object);
            await sut.InitializeAsync();

            await sut.TryUpdateOperationStateAsync(operation, TableOperationState.Completed);

            // Step 1: We should have exactly one invocation of the UpsertAsync method.
            var invocations = store.Invocations.Where(x => x.Method.Name == "UpsertAsync").ToList();
            Assert.Single(invocations);
            Assert.Equal(SystemTables.OperationsQueue, invocations[0].Arguments[0] as string);

            var ops = invocations[0].Arguments[1] as IEnumerable<JObject>;
            Assert.Single(ops);
            var actual = ops.First();
            Assert.Equal(operation.Id, actual.Value<string>("id"));
            Assert.Equal((int)TableOperationState.Completed, actual.Value<int>("state"));
        }

        [Fact]
        [Trait("Method", "TryUpdateOperationStateAsync")]
        public async Task TryUpdateOperationState_SetsBatchState_OnError()
        {
            var operation = new DeleteOperation("movies", Guid.NewGuid().ToString());
            var exception = new ApplicationException();
            var store = new Mock<IOfflineStore>();
            store.Setup(x => x.GetPageAsync(It.IsAny<QueryDescription>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new Page<JObject>()));
            store.Setup(x => x.UpsertAsync(It.IsAny<string>(), It.IsAny<IEnumerable<JObject>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Throws(exception);
            var context = new SyncContext(GetMockClient(), store.Object);
            var sut = new OperationsQueue(store.Object);
            await sut.InitializeAsync();

            var batch = new OperationBatch(context);
            var ex = await Assert.ThrowsAsync<OfflineStoreException>(() => sut.TryUpdateOperationStateAsync(operation, TableOperationState.Completed, batch));

            Assert.Equal(PushStatus.CancelledByOfflineStoreError, batch.AbortReason);
            // Find the root of the exception heirarchy
            Exception inner = ex;
            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }
            Assert.Same(exception, inner);
        }

        [Fact]
        [Trait("Method", "TryUpdateOperationStateAsync")]
        public async Task TryUpdateOperationState_SetsBatchState_OnErrorWithoutBatch()
        {
            var operation = new DeleteOperation("movies", Guid.NewGuid().ToString());
            var exception = new ApplicationException();
            var store = new Mock<IOfflineStore>();
            store.Setup(x => x.GetPageAsync(It.IsAny<QueryDescription>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new Page<JObject>()));
            store.Setup(x => x.UpsertAsync(It.IsAny<string>(), It.IsAny<IEnumerable<JObject>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Throws(exception);
            var context = new SyncContext(GetMockClient(), store.Object);
            var sut = new OperationsQueue(store.Object);
            await sut.InitializeAsync();

            var ex = await Assert.ThrowsAsync<OfflineStoreException>(() => sut.TryUpdateOperationStateAsync(operation, TableOperationState.Completed));

            // Find the root of the exception heirarchy
            Exception inner = ex;
            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }
            Assert.Same(exception, inner);
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

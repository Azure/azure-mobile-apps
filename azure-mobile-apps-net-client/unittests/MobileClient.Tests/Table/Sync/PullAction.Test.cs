// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Sync;
using MobileClient.Tests.Helpers;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileClient.Tests.Table.Sync
{
    public class PullActionTests
    {
        [Fact]
        public async Task DoesNotUpsertAnObject_IfItDoesNotHaveAnId()
        {
            string testName = "test";

            var store = new Mock<IMobileServiceLocalStore>(MockBehavior.Strict);
            var settings = new Mock<MobileServiceSyncSettingsManager>(MockBehavior.Strict);
            var opQueue = new Mock<OperationQueue>(MockBehavior.Strict, store.Object);
            var handler = new Mock<IMobileServiceSyncHandler>(MockBehavior.Strict);
            var client = new Mock<MobileServiceClient>();
            client.Object.Serializer = new MobileServiceSerializer();
            var context = new Mock<MobileServiceSyncContext>(client.Object);
            var table = new Mock<MobileServiceTable<ToDoWithSystemPropertiesType>>(testName, client.Object);

            var query = new MobileServiceTableQueryDescription(testName);
            var action = new PullAction(
                table.Object, MobileServiceTableKind.Table, 
                context.Object, 
                null, query, 
                null, null, 
                opQueue.Object, 
                settings.Object, 
                store.Object,
                 MobileServiceRemoteTableOptions.All, 
                 pullOptions: null, 
                 reader: null, 
                 cancellationToken: CancellationToken.None);

            var itemWithId = new JObject() { { "id", "abc" }, { "text", "has id" } };
            var itemWithoutId = new JObject() { { "text", "no id" } };
            var result = new JArray(new[]{
                itemWithId,
                itemWithoutId
            });
            opQueue
                .Setup(q => q.LockTableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IDisposable>(null));
            opQueue
                .Setup(q => q.CountPending(It.IsAny<string>()))
                .Returns(Task.FromResult(0L));
            opQueue
                .Setup(q => q.GetOperationByItemIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult<MobileServiceTableOperation>(null));
            table
                .SetupSequence(t => t.ReadAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                .Returns(Task.FromResult(QueryResult.Parse(result, null, false)))
                .Returns(Task.FromResult(QueryResult.Parse(new JArray(), null, false)));
            store
                .Setup(s => s.UpsertAsync(testName, It.IsAny<IEnumerable<JObject>>(), true))
                .Returns(Task.FromResult(0))
                .Callback<string, IEnumerable<JObject>, bool>((tableName, items, fromServer) =>
                {
                    Assert.Single(items);
                    Assert.Equal(itemWithId, items.First());
                });

            await action.ExecuteAsync();

            store.VerifyAll();
            opQueue.VerifyAll();
            table.VerifyAll();

            store.Verify(s => s.DeleteAsync("test", It.IsAny<IEnumerable<string>>()), Times.Never(), "There shouldn't be any call to delete");
        }

        [Fact]
        public async Task SavesTheMaxUpdatedAt_IfQueryIdIsSpecified_WithoutFilter()
        {
            string testName = "test";
            string queryName = "latestItems";

            var store = new Mock<IMobileServiceLocalStore>(MockBehavior.Strict);
            var settings = new Mock<MobileServiceSyncSettingsManager>(MockBehavior.Strict);
            var opQueue = new Mock<OperationQueue>(MockBehavior.Strict, store.Object);
            var handler = new Mock<IMobileServiceSyncHandler>(MockBehavior.Strict);
            var client = new Mock<MobileServiceClient>();
            client.Object.Serializer = new MobileServiceSerializer();
            var context = new Mock<MobileServiceSyncContext>(client.Object);
            var table = new Mock<MobileServiceTable<ToDoWithSystemPropertiesType>>(testName, client.Object);

            var query = new MobileServiceTableQueryDescription(testName);
            var maxUpdatedAt = new DateTime(2014, 07, 09);
            var result = new JArray(new[]
            {
                new JObject() { { "id", "abc" }, { "text", "has id"}, { "updatedAt", "1985-07-17" } },
                new JObject() { { "id", "abc" }, { "text", "has id"}, { "updatedAt", "2014-07-09" } }
            });
            string firstQuery = "$filter=(updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00')&$orderby=updatedAt&$skip=0&$top=50";
            string secondQuery = "$filter=(updatedAt ge datetimeoffset'2014-07-09T07%3A00%3A00.0000000%2B00%3A00')&$orderby=updatedAt&$skip=0&$top=50";

            var action = new PullAction(
                table.Object, MobileServiceTableKind.Table, 
                context.Object,
                queryName, query, 
                null, null, 
                opQueue.Object, 
                settings.Object, 
                store.Object,
                MobileServiceRemoteTableOptions.All, 
                pullOptions: null, 
                reader: null, 
                cancellationToken: CancellationToken.None);

            opQueue
                .Setup(q => q.LockTableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IDisposable>(null));
            opQueue
                .Setup(q => q.CountPending(It.IsAny<string>()))
                .Returns(Task.FromResult(0L));
            table
                .Setup(t => t.ReadAsync(firstQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                .Returns(Task.FromResult(QueryResult.Parse(result, null, false)));
            opQueue
                .Setup(q => q.GetOperationByItemIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult<MobileServiceTableOperation>(null));
            table
                .Setup(t => t.ReadAsync(secondQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                .Returns(Task.FromResult(QueryResult.Parse(new JArray(), null, false)));
            store
                .Setup(s => s.UpsertAsync(testName, It.IsAny<IEnumerable<JObject>>(), true))
                .Returns(Task.FromResult(0));
            settings
                .Setup(s => s.GetDeltaTokenAsync(testName, queryName))
                .Returns(Task.FromResult(new DateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.Zero)));
            settings
                .Setup(s => s.SetDeltaTokenAsync(testName, queryName, maxUpdatedAt))
                .Returns(Task.FromResult(0));

            await action.ExecuteAsync();

            store.VerifyAll();
            opQueue.VerifyAll();
            table.Verify(t => t.ReadAsync(firstQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()), Times.AtLeastOnce());
            settings.VerifyAll();
            store.Verify(s => s.DeleteAsync(testName, It.IsAny<IEnumerable<string>>()), Times.Never(), "There shouldn't be any call to delete");
        }

        [Fact]
        public async Task SavesTheMaxUpdatedAt_IfQueryIdIsSpecified()
        {
            string testName = "test";
            string queryName = "latestItems";

            var store = new Mock<IMobileServiceLocalStore>(MockBehavior.Strict);
            var settings = new Mock<MobileServiceSyncSettingsManager>(MockBehavior.Strict);
            var opQueue = new Mock<OperationQueue>(MockBehavior.Strict, store.Object);
            var handler = new Mock<IMobileServiceSyncHandler>(MockBehavior.Strict);
            var client = new Mock<MobileServiceClient>();
            client.Object.Serializer = new MobileServiceSerializer();
            var context = new Mock<MobileServiceSyncContext>(client.Object);
            var table = new Mock<MobileServiceTable<ToDoWithSystemPropertiesType>>(testName, client.Object);

            var query = new MobileServiceTableQueryDescription(testName);
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.Equal, new ConstantNode(4), new ConstantNode(3));
            query.Ordering.Add(new OrderByNode(new MemberAccessNode(null, "text"), OrderByDirection.Descending));
            var maxUpdatedAt = new DateTime(2014, 07, 09);
            var result = new JArray(new[]
            {
                new JObject() { { "id", "abc" }, { "text", "has id"}, { "updatedAt", "1985-07-17" } },
                new JObject() { { "id", "abc" }, { "text", "has id"}, { "updatedAt", "2014-07-09" } }
            });
            string firstQuery = "$filter=((4 eq 3) and (updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00'))&$orderby=updatedAt&$skip=0&$top=50";
            string secondQuery = "$filter=((4 eq 3) and (updatedAt ge datetimeoffset'2014-07-09T07%3A00%3A00.0000000%2B00%3A00'))&$orderby=updatedAt&$skip=0&$top=50";

            var action = new PullAction(
                table.Object, MobileServiceTableKind.Table, 
                context.Object,
                queryName, query, 
                null, null, 
                opQueue.Object, 
                settings.Object, 
                store.Object,
                MobileServiceRemoteTableOptions.All, 
                pullOptions: null, 
                reader: null, 
                cancellationToken: CancellationToken.None);          

            opQueue
                .Setup(q => q.LockTableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IDisposable>(null));
            opQueue
                .Setup(q => q.CountPending(It.IsAny<string>()))
                .Returns(Task.FromResult(0L));
            table
                .Setup(t => t.ReadAsync(firstQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                .Returns(Task.FromResult(QueryResult.Parse(result, null, false)));
            opQueue
                .Setup(q => q.GetOperationByItemIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult<MobileServiceTableOperation>(null));
            table
                .Setup(t => t.ReadAsync(secondQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                .Returns(Task.FromResult(QueryResult.Parse(new JArray(), null, false)));
            store
                .Setup(s => s.UpsertAsync(testName, It.IsAny<IEnumerable<JObject>>(), true))
                .Returns(Task.FromResult(0));
            settings
                .Setup(s => s.GetDeltaTokenAsync(testName, queryName))
                .Returns(Task.FromResult(new DateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.Zero)));
            settings
                .Setup(s => s.SetDeltaTokenAsync(testName, queryName, maxUpdatedAt))
                .Returns(Task.FromResult(0));

            await action.ExecuteAsync();

            store.VerifyAll();
            opQueue.VerifyAll();
            table.Verify(t => t.ReadAsync(firstQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()), Times.AtLeastOnce());
            settings.VerifyAll();
            store.Verify(s => s.DeleteAsync("test", It.IsAny<IEnumerable<string>>()), Times.Never(), "There shouldn't be any call to delete");
        }

        [Fact]
        public async Task DoesNotSaveTheMaxUpdatedAt_IfThereAreNoResults()
        {
            string testName = "test";
            string queryName = "latestItems";

            var store = new Mock<IMobileServiceLocalStore>(MockBehavior.Strict);
            var settings = new Mock<MobileServiceSyncSettingsManager>(MockBehavior.Strict);
            var opQueue = new Mock<OperationQueue>(MockBehavior.Strict, store.Object);
            var handler = new Mock<IMobileServiceSyncHandler>(MockBehavior.Strict);
            var client = new Mock<MobileServiceClient>();
            client.Object.Serializer = new MobileServiceSerializer();
            var context = new Mock<MobileServiceSyncContext>(client.Object);
            var table = new Mock<MobileServiceTable<ToDoWithSystemPropertiesType>>(testName, client.Object);

            var query = new MobileServiceTableQueryDescription(testName);
            var result = new JArray();
            string firstQuery = "$filter=(updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00')&$orderby=updatedAt&$skip=0&$top=50";

            var action = new PullAction(
                table.Object, MobileServiceTableKind.Table, 
                context.Object,
                queryName, query, 
                null, null, 
                opQueue.Object, 
                settings.Object, 
                store.Object,
                MobileServiceRemoteTableOptions.All, 
                pullOptions: null, 
                reader: null, 
                cancellationToken: CancellationToken.None); 

            opQueue
                .Setup(q => q.LockTableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IDisposable>(null));
            opQueue
                .Setup(q => q.CountPending(It.IsAny<string>()))
                .Returns(Task.FromResult(0L));
            table
                .Setup(t => t.ReadAsync(firstQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                .Returns(Task.FromResult(QueryResult.Parse(result, null, false)));
            settings
                .Setup(s => s.GetDeltaTokenAsync(testName, queryName))
                .Returns(Task.FromResult(new DateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.Zero)));

            await action.ExecuteAsync();

            store.VerifyAll();
            opQueue.VerifyAll();
            table.VerifyAll();
            settings.VerifyAll();
        }

        [Fact]
        public async Task DoesNotSaveTheMaxUpdatedAt_IfResultsHaveOlderUpdatedAt()
        {
            string testName = "test";
            string queryName = "latestItems";

            var store = new Mock<IMobileServiceLocalStore>(MockBehavior.Strict);
            var settings = new Mock<MobileServiceSyncSettingsManager>(MockBehavior.Strict);
            var opQueue = new Mock<OperationQueue>(MockBehavior.Strict, store.Object);
            var handler = new Mock<IMobileServiceSyncHandler>(MockBehavior.Strict);
            var client = new Mock<MobileServiceClient>();
            client.Object.Serializer = new MobileServiceSerializer();
            var context = new Mock<MobileServiceSyncContext>(client.Object);
            var table = new Mock<MobileServiceTable<ToDoWithSystemPropertiesType>>(testName, client.Object);

            var query = new MobileServiceTableQueryDescription(testName);
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.Equal, new ConstantNode(4), new ConstantNode(3));
            var result = new JArray(new[]
            {
                new JObject() { { "id", "abc" }, { "text", "has id"}, { "updatedAt", "1985-07-17" } },
            });
            string firstQuery = "$filter=((4 eq 3) and (updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00'))&$orderby=updatedAt&$skip=0&$top=50";
            string secondQuery = "$filter=((4 eq 3) and (updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00'))&$orderby=updatedAt&$skip=1&$top=50";

            var action = new PullAction(
                table.Object, MobileServiceTableKind.Table, 
                context.Object,
                queryName, query, 
                null, null, 
                opQueue.Object, 
                settings.Object, 
                store.Object,
                MobileServiceRemoteTableOptions.All, 
                pullOptions: null, 
                reader: null, 
                cancellationToken: CancellationToken.None); 

            opQueue
                .Setup(q => q.LockTableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IDisposable>(null));
            opQueue
                .Setup(q => q.CountPending(It.IsAny<string>()))
                .Returns(Task.FromResult(0L));
            table
                .Setup(t => t.ReadAsync(firstQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                .Returns(Task.FromResult(QueryResult.Parse(result, null, false)));

            opQueue
                .Setup(q => q.GetOperationByItemIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult<MobileServiceTableOperation>(null));
            table
                .Setup(t => t.ReadAsync(secondQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                .Returns(Task.FromResult(QueryResult.Parse(new JArray(), null, false)));
            store
                .Setup(s => s.UpsertAsync(testName, It.IsAny<IEnumerable<JObject>>(), true))
                .Returns(Task.FromResult(0));
            settings
                .Setup(s => s.GetDeltaTokenAsync(testName, queryName))
                .Returns(Task.FromResult(new DateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.Zero)));

            await action.ExecuteAsync();

            store.VerifyAll();
            opQueue.VerifyAll();
            table.VerifyAll();
            settings.VerifyAll();

            store.Verify(s => s.DeleteAsync("test", It.IsAny<IEnumerable<string>>()), Times.Never(), "There shouldn't be any call to delete");
        }

        [Fact]
        public async Task DoesNotSaveTheMaxUpdatedAt_IfResultsDoNotHaveUpdatedAt()
        {
            string testName = "test";
            string queryName = "latestItems";

            var store = new Mock<IMobileServiceLocalStore>(MockBehavior.Strict);
            var settings = new Mock<MobileServiceSyncSettingsManager>(MockBehavior.Strict);
            var opQueue = new Mock<OperationQueue>(MockBehavior.Strict, store.Object);
            var handler = new Mock<IMobileServiceSyncHandler>(MockBehavior.Strict);
            var client = new Mock<MobileServiceClient>();
            client.Object.Serializer = new MobileServiceSerializer();
            var context = new Mock<MobileServiceSyncContext>(client.Object);
            var table = new Mock<MobileServiceTable<ToDoWithSystemPropertiesType>>(testName, client.Object);

            var query = new MobileServiceTableQueryDescription(testName);
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.Equal, new ConstantNode(4), new ConstantNode(3));
            var result = new JArray(new[]
            {
                new JObject() { { "id", "abc" }, { "text", "has id"} },
                new JObject() { { "id", "abc" }, { "text", "has id"} }
            });
            string firstQuery = "$filter=((4 eq 3) and (updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00'))&$orderby=updatedAt&$skip=0&$top=50";
            string secondQuery = "$filter=((4 eq 3) and (updatedAt ge datetimeoffset'2013-01-01T00%3A00%3A00.0000000%2B00%3A00'))&$orderby=updatedAt&$skip=2&$top=50";

            var action = new PullAction(
                table.Object, MobileServiceTableKind.Table, 
                context.Object,
                queryName, query, 
                null, null, 
                opQueue.Object, 
                settings.Object, 
                store.Object,
                MobileServiceRemoteTableOptions.All, 
                pullOptions: null, 
                reader: null, 
                cancellationToken: CancellationToken.None);

            opQueue
                .Setup(q => q.LockTableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IDisposable>(null));
            opQueue
                .Setup(q => q.CountPending(It.IsAny<string>()))
                .Returns(Task.FromResult(0L));
            table
                .Setup(t => t.ReadAsync(firstQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                .Returns(Task.FromResult(QueryResult.Parse(result, null, false)));
            opQueue
                    .Setup(q => q.GetOperationByItemIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.FromResult<MobileServiceTableOperation>(null));
            table
                .Setup(t => t.ReadAsync(secondQuery, It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                .Returns(Task.FromResult(QueryResult.Parse(new JArray(), null, false)));
            store
                .Setup(s => s.UpsertAsync(testName, It.IsAny<IEnumerable<JObject>>(), true))
                .Returns(Task.FromResult(0));
            settings
                .Setup(s => s.GetDeltaTokenAsync(testName, queryName))
                .Returns(Task.FromResult(new DateTimeOffset(2013, 1, 1, 0, 0, 0, TimeSpan.Zero)));

            await action.ExecuteAsync();

            store.VerifyAll();
            opQueue.VerifyAll();
            table.VerifyAll();
            settings.VerifyAll();

            store.Verify(s => s.DeleteAsync(testName, It.IsAny<IEnumerable<string>>()), Times.Never(), "There shouldn't be any call to delete");
        }

        [Fact]
        public void NegativePageSize_Throws()
        {
            Assert.Throws<ArgumentException>(() => new PullOptions { MaxPageSize = -1 });
        }

        [Fact]
        public void ZeroPageSize_Throws()
        {
            Assert.Throws<ArgumentException>(() => new PullOptions { MaxPageSize = 0 });
        }

        [Fact]
        public async Task DoesNotUpsertAnObject_IfRecordIsPresentInOperationQueue()
        {
            string testName = "test";

            var store = new Mock<IMobileServiceLocalStore>(MockBehavior.Strict);
            var settings = new Mock<MobileServiceSyncSettingsManager>(MockBehavior.Strict);
            var opQueue = new Mock<OperationQueue>(MockBehavior.Strict, store.Object);
            var handler = new Mock<IMobileServiceSyncHandler>(MockBehavior.Strict);
            var client = new Mock<MobileServiceClient>();
            client.Object.Serializer = new MobileServiceSerializer();
            var context = new Mock<MobileServiceSyncContext>(client.Object);
            var table = new Mock<MobileServiceTable<ToDoWithSystemPropertiesType>>(testName, client.Object);

            var query = new MobileServiceTableQueryDescription(testName);
            var action = new PullAction(
                table.Object, MobileServiceTableKind.Table, 
                context.Object, null, query, null, null, 
                opQueue.Object, settings.Object, store.Object, 
                MobileServiceRemoteTableOptions.All, 
                pullOptions: null, reader: null, cancellationToken: CancellationToken.None);

            //// item with insert operation from server
            var insertItemWithPendingOperation = new JObject() { { "id", "abc" }, { "text", "has pending operation" } };

            //// item with update operation from server
            var updateItemWithPendingOperation = new JObject() { { "id", "abc2" }, { "text", "has pending operation" } };

            //// item with delete operation from server
            var deleteItemWithPendingOperation = new JObject() { { "id", "abc3" }, { "text", "has pending operation" } };

            //// item with no pending operation from server
            var itemWithNoPendingOperation = new JObject() { { "id", "abc4" }, { "text", "has no pending operation" } };

            var result = new JArray(new[]{
                insertItemWithPendingOperation,
                updateItemWithPendingOperation,
                deleteItemWithPendingOperation,
                itemWithNoPendingOperation
            });

            opQueue
                .Setup(q => q.LockTableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IDisposable>(null));
            opQueue
                .Setup(q => q.CountPending(It.IsAny<string>()))
                .Returns(Task.FromResult(0L));
            opQueue
                .Setup(q => q.GetOperationByItemIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string tableName, string id) =>
                {
                    if (id.Equals("abc") || id.Equals("abc2") || id.Equals("abc3"))
                        return Task.FromResult<MobileServiceTableOperation>(new InsertOperation(tableName, MobileServiceTableKind.Table, id));
                    else
                        return Task.FromResult<MobileServiceTableOperation>(null);
                });

            //// below two reads correspond to first and second page from the server
            table
                .SetupSequence(t => t.ReadAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<MobileServiceFeatures>()))
                .Returns(Task.FromResult(QueryResult.Parse(result, null, false)))
                .Returns(Task.FromResult(QueryResult.Parse(new JArray(), null, false)));

            store
                .Setup(s => s.UpsertAsync(testName, It.IsAny<IEnumerable<JObject>>(), true))
                .Returns(Task.FromResult(0))
                .Callback<string, IEnumerable<JObject>, bool>((tableName, items, fromServer) =>
                {
                    Assert.Single(items);
                    Assert.Equal(itemWithNoPendingOperation, items.First());
                });

            await action.ExecuteAsync();

            store.VerifyAll();
            opQueue.VerifyAll();
            table.VerifyAll();

            store.Verify(s => s.DeleteAsync(testName, It.IsAny<IEnumerable<string>>()), Times.Never(), "There shouldn't be any call to delete");
        }
    }
}
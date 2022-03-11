// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Offline.Queue
{
    [ExcludeFromCodeCoverage]
    public class OperationsQueue_Tests : BaseOperationTest
    {
        private readonly IOfflineStore store = new MockOfflineStore();

        [Fact]
        public void TableDefinition_Serializes()
        {
            var actual = TableOperation.TableDefinition.ToString(Formatting.None);
            const string expected = "{\"id\":\"\",\"kind\":0,\"state\":0,\"tableName\":\"\",\"itemId\":\"\",\"item\":\"\",\"sequence\":0,\"version\":0}";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TableOperation_Deserialize_ThrowsOnInvalidKind()
        {
            var json = JObject.Parse("{}");
            Assert.Throws<InvalidOperationException>(() => TableOperation.Deserialize(json));
        }

        [Fact]
        public void Queue_NotInitialized_WhenConstructed()
        {
            var sut = new OperationsQueue(store);
            Assert.False(sut.IsInitialized);
            Assert.Equal(0, sut.PendingOperations);
        }

        [Fact]
        public async Task Queue_Initialized_WithEmptyStore()
        {
            var sut = new OperationsQueue(store);
            await sut.InitializeAsync();

            Assert.True(sut.IsInitialized);
            Assert.Equal(0, sut.PendingOperations);
        }

        [Fact]
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
    }
}

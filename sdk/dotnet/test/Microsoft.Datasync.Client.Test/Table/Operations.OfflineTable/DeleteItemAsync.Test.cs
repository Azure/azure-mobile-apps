﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTable
{
    [ExcludeFromCodeCoverage]
    public class DeleteItemAsync_Tests : BaseOperationTest
    {
        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ThrowsOnNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.DeleteItemAsync(null)).ConfigureAwait(false);
        }


        [Theory]
        [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ThrowsOnInvalidId(string id)
        {
            var json = CreateJsonDocument(new IdOnly { Id = id });
            await Assert.ThrowsAsync<ArgumentException>(() => table.DeleteItemAsync(json)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Throws_WhenItemDoesNotExist()
        {
            var item = GetSampleMovie<ClientMovie>();
            var instance = StoreInTable("movies", item);

            var toDelete = (JObject)instance.DeepClone();
            toDelete["id"] = Guid.NewGuid().ToString();

            await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.DeleteItemAsync(toDelete));

            var opQueue = store.GetOrCreateTable(SystemTables.OperationsQueue);
            Assert.Empty(opQueue);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_DeletesItem_AndAddsToQueue_WhenItemExists()
        {
            var item = GetSampleMovie<ClientMovie>();
            var instance = StoreInTable("movies", item);

            await table.DeleteItemAsync(instance);

            Assert.Empty(store.TableMap["movies"]);
            Assert.Single(store.TableMap[SystemTables.OperationsQueue]);
            var op = TableOperation.Deserialize(store.TableMap[SystemTables.OperationsQueue].Values.First());
            Assert.Equal(TableOperationKind.Delete, op.Kind);
            Assert.Equal(item.Id, op.ItemId);
            AssertEx.JsonEqual(instance, op.Item);
        }
    }
}

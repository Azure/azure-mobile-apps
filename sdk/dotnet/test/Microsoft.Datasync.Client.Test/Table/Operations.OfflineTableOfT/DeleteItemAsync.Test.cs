// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTableOfT;

[ExcludeFromCodeCoverage]
public class DeleteItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ThrowsOnNull()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.DeleteItemAsync(null)).ConfigureAwait(false);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ThrowsOnInvalidId(string id)
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = GetSampleMovie<ClientMovie>();
        item.Id = id;
        await Assert.ThrowsAsync<ArgumentException>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Throws_WhenItemDoesNotExist()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = GetSampleMovie<ClientMovie>();
        item.Id = Guid.NewGuid().ToString();
        _ = StoreInTable("movies", item);

        var toDelete = GetSampleMovie<ClientMovie>();
        toDelete.Id = Guid.NewGuid().ToString();

        await Assert.ThrowsAsync<InvalidOperationException>(() => table.DeleteItemAsync(toDelete));

        var opQueue = store.GetOrCreateTable(SystemTables.OperationsQueue);
        Assert.Empty(opQueue);
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_DeletesItem_AndAddsToQueue_WhenItemExists()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = GetSampleMovie<ClientMovie>();
        item.Id = Guid.NewGuid().ToString();
        var instance = StoreInTable("movies", item);

        await table.DeleteItemAsync(item);

        Assert.Empty(store.TableMap["movies"]);
        Assert.Single(store.TableMap[SystemTables.OperationsQueue]);
        var op = TableOperation.Deserialize(store.TableMap[SystemTables.OperationsQueue].Values.First());
        Assert.Equal(TableOperationKind.Delete, op.Kind);
        Assert.Equal(item.Id, op.ItemId);
        AssertEx.JsonEqual(instance, op.Item);
    }
}

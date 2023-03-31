// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTableOfT;

[ExcludeFromCodeCoverage]
public class GetAsyncItems_Tests : BaseOperationTest
{
    #region Helpers
    /// <summary>
    /// Injects a set of items into the store.
    /// </summary>
    /// <param name="nItems"></param>
    /// <returns></returns>
    private List<ClientMovie> InjectRandomItems(int nItems = 5)
    {
        List<ClientMovie> items = new();

        for (int i = 0; i < nItems; i++)
        {
            var item = new ClientMovie { Id = $"id-{i}", Title = "true" };
            var instance = (JObject)table.ServiceClient.Serializer.Serialize(item);
            items.Add(item);
            store.Upsert("movies", new[] { instance });
        }
        return items;
    }

    /// <summary>
    /// Gets all the items from the list of items returned.
    /// </summary>
    /// <returns></returns>
    private async Task<List<ClientMovie>> GetAllItems(ITableQuery<ClientMovie> query = null, IOfflineTable<ClientMovie> table = null, long? nItems = null)
    {
        table ??= base.table;
        query ??= table.CreateQuery();
        List<ClientMovie> items = new();
        var pageable = table.GetAsyncItems(query) as AsyncPageable<ClientMovie>;
        await foreach (var item in pageable)
        {
            if (nItems != null)
            {
                Assert.Equal(nItems!, pageable.Count);
            }
            items.Add(item);
        }
        return items;
    }
    #endregion

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_NoItems()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();

        // Act
        var items = await GetAllItems();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_WithItems()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var expectedItems = InjectRandomItems(5);

        // Act
        var items = await GetAllItems();

        // Assert
        Assert.Equal(expectedItems.Count, items.Count);
        for (int i = 0; i < expectedItems.Count; i++)
        {
            Assert.Equal(expectedItems[i].Id, items[i].Id);
        }
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_WithItems_NoArg()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var expectedItems = InjectRandomItems(5);

        // Act
        List<ClientMovie> items = await table.GetAsyncItems().ToListAsync();

        // Assert
        Assert.Equal(expectedItems.Count, items.Count);
        for (int i = 0; i < expectedItems.Count; i++)
        {
            Assert.Equal(expectedItems[i].Id, items[i].Id);
        }
    }

    [Fact]
    [Trait("Method", "ToAsyncEnumerable")]
    public async Task ToAsyncEnumerable_WithItems()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var expectedItems = InjectRandomItems(5);

        // Act
        List<ClientMovie> items = await table.ToAsyncEnumerable().ToListAsync();

        // Assert
        Assert.Equal(expectedItems.Count, items.Count);
        for (int i = 0; i < expectedItems.Count; i++)
        {
            Assert.Equal(expectedItems[i].Id, items[i].Id);
        }
    }
}

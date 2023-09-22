// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTable;

[ExcludeFromCodeCoverage]
public class GetAsyncItems_Tests : BaseOperationTest
{
    #region Helpers
    /// <summary>
    /// Injects a set of items into the store.
    /// </summary>
    /// <param name="nItems"></param>
    /// <returns></returns>
    private List<JObject> InjectRandomItems(int nItems = 5)
    {
        List<JObject> items = new();
        for (int i = 0; i < nItems; i++)
        {
            var item = new IdEntity { Id = $"id-{i}", StringValue = "true" };
            var instance = (JObject)table.ServiceClient.Serializer.Serialize(item);
            items.Add(instance);
        }
        store.Upsert("movies", items);
        return items;
    }

    /// <summary>
    /// Gets all the items from the list of items returned.
    /// </summary>
    /// <returns></returns>
    private async Task<List<JObject>> GetAllItems(string query = "", IOfflineTable table = null, long? nItems = null)
    {
        table ??= base.table;
        List<JObject> items = new();
        var pageable = table.GetAsyncItems(query) as AsyncPageable<JObject>;
        await foreach (var item in pageable)
        {
            if (nItems != null)
            {
                Assert.Equal(nItems!, pageable.Count);
            }
            items.Add(item as JObject);
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
        AssertEx.SequenceEqual(expectedItems, items);
    }
}

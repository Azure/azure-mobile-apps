// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTable;

[ExcludeFromCodeCoverage]
public class CountItemsAsync_Tests : BaseOperationTest
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
    #endregion

    [Fact]
    [Trait("Method", "CountItemsAsync")]
    public async Task CountItemsAsync_Count()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();
        _ = InjectRandomItems(42);

        // Act
        var count = await table.CountItemsAsync("");

        // Assert
        Assert.Equal(42, count);
    }
}

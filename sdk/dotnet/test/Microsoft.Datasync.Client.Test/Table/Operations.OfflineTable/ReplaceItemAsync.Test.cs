// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTable;

[ExcludeFromCodeCoverage]
public class ReplaceItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsOnNull()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.ReplaceItemAsync(null)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsOnNullId()
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var json = CreateJsonDocument(new IdEntity { Id = null });
        await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsOnInvalidId(string id)
    {
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var json = CreateJsonDocument(new IdEntity { Id = id });
        await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Works()
    {
        // Arrange
        await table.ServiceClient.InitializeOfflineStoreAsync();
        var item = GetSampleMovie<ClientMovie>();
        item.Id = Guid.NewGuid().ToString();
        var json = (JObject)table.ServiceClient.Serializer.Serialize(item);
        store.Upsert("movies", new[] { json });

        var updatedItem = (JObject)json.DeepClone();
        updatedItem["title"] = "Updated Title";

        // Act
        await table.ReplaceItemAsync(updatedItem).ConfigureAwait(false);

        // Assert
        AssertEx.JsonEqual(updatedItem, store.TableMap["movies"][item.Id]);
    }
}

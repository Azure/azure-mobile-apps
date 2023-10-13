// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTableOfT;

[ExcludeFromCodeCoverage]
public class CountItemsAsync_Tests : BaseOperationTest
{
    private const string CountArgs = "$top=1&$count=true";

    #region Helpers
    /// <summary>
    /// Creates a paging response with JObjects.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="totalCount"></param>
    /// <param name="nextLink"></param>
    /// <returns></returns>
    private Page<JObject> CreatePageOfJsonItems(int count, long? totalCount = null, Uri nextLink = null)
    {
        List<JObject> items = new();
        List<IdEntity> entities = new();
        for (int i = 0; i < count; i++)
        {
            var entity = new IdEntity { Id = Guid.NewGuid().ToString("N"), StringValue = $"Item #{i}" };
            items.Add(CreateJsonDocument(entity));
            entities.Add(entity);
        }
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity> { Items = entities, Count = totalCount, NextLink = nextLink });
        return new Page<JObject> { Items = items, Count = totalCount, NextLink = nextLink };
    }
    #endregion

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.UnavailableForLegalReasons)]
    [InlineData(HttpStatusCode.ExpectationFailed)]
    [Trait("Method", "CountItemsAsync")]
    public async Task CountItemsAsync_Throws_OnBadRequest(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);

        // Assert
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(async () => await table.CountItemsAsync());
        Assert.Equal(statusCode, ex.Response.StatusCode);
    }

    [Fact]
    [Trait("Method", "CountItemsAsync")]
    public async Task CountItemsAsync_NoItems_WithAuth()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

        // Act
        var count = await authTable.CountItemsAsync();

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, tableEndpoint + $"?{CountArgs}");
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.Equal(-1, count);
    }

    [Fact]
    [Trait("Method", "CountItemsAsync")]
    public async Task CountItemsAsync_NoItems_WithFilter()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

        // Act
        var query = table.CreateQuery().Where(m => m.StringField == "id");
        var count = await table.CountItemsAsync(query);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, tableEndpoint + $"?$filter=%28stringField eq %27id%27%29&{CountArgs}");
    }

    [Fact]
    [Trait("Method", "CountItemsAsync")]
    public async Task CountItemsAsync_NoCount()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

        // Act
        var count = await table.CountItemsAsync();

        // Assert
        Assert.Equal(-1, count);
    }

    [Fact]
    [Trait("Method", "CountItemsAsync")]
    public async Task CountItemsAsync_Count()
    {
        // Arrange
        CreatePageOfJsonItems(1, 42);

        // Act
        var count = await table.CountItemsAsync();

        // Assert
        _ = AssertSingleRequest(HttpMethod.Get, tableEndpoint + $"?{CountArgs}");
        Assert.Equal(42, count);
    }

    [Fact]
    [Trait("Method", "LongCountAsync")]
    public async Task LongCountAsync_Count()
    {
        // Arrange
        CreatePageOfJsonItems(1, 42);

        // Act
        var count = await table.LongCountAsync();

        // Assert
        _ = AssertSingleRequest(HttpMethod.Get, tableEndpoint + $"?{CountArgs}");
        Assert.Equal(42, count);
    }
}


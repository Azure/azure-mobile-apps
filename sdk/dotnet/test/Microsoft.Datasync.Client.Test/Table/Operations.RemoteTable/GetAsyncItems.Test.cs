// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTable;

[ExcludeFromCodeCoverage]
public class GetAsyncItems_Tests : BaseOperationTest
{
    #region Helpers
    /// <summary>
    /// Adds a good response message with formatted JSON.
    /// </summary>
    /// <param name="content"></param>
    private void AddStringResponse(string content)
    {
        MockHandler.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content, Encoding.UTF8, "application/json") });
    }

    /// <summary>
    /// Gets all the items from the list of items returned.
    /// </summary>
    /// <returns></returns>
    private async Task<List<JObject>> GetAllItems(IRemoteTable table = null, long? nItems = null)
    {
        table ??= base.table;
        List<JObject> items = new();
        var pageable = table.GetAsyncItems("") as AsyncPageable<JToken>;
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
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_Throws_OnBadRequest(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);

        // Act
        var enumerator = table.GetAsyncItems("").GetAsyncEnumerator();

        // Assert
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(async () => await enumerator.MoveNextAsync().ConfigureAwait(false)).ConfigureAwait(false);
        Assert.Equal(statusCode, ex.Response.StatusCode);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_NoItems()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

        // Act
        var enumerator = table.GetAsyncItems("").GetAsyncEnumerator();
        var hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);

        // Assert
        _ = AssertSingleRequest(HttpMethod.Get, tableEndpoint);
        Assert.False(hasMore);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_NoItems_WithAuth()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

        // Act
        var enumerator = authTable.GetAsyncItems("$filter=foo").GetAsyncEnumerator();
        var hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, tableEndpoint + "?$filter=foo");
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.False(hasMore);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_NoItems_WhenNullResponse()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK);

        // Act
        var pageable = table.GetAsyncItems("") as AsyncPageable<JToken>;
        var enumerator = pageable.AsPages().GetAsyncEnumerator();

        // Assert
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(async () => await enumerator.MoveNextAsync());
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_OnePageOfItems_WhenItemsReturned()
    {
        // Arrange
        var page = CreatePageOfJsonItems(5);

        // Act
        var items = await GetAllItems();

        // Assert
        _ = AssertSingleRequest(HttpMethod.Get, tableEndpoint);
       AssertEx.SequenceEqual(page.Items.ToList(), items);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_OnePageOfItems_WithAuth_WhenItemsReturned()
    {
        // Arrange
        var page = CreatePageOfJsonItems(5);

        // Act
        var items = await GetAllItems(authTable);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, tableEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        AssertEx.SequenceEqual(page.Items.ToList(), items);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_TwoPagesOfItems_WhenItemsReturned()
    {
        // Arrange
        var page1 = CreatePageOfJsonItems(5, null, new Uri($"{tableEndpoint}?page=2"));
        var page2 = CreatePageOfJsonItems(5);

        // Act
        var items = await GetAllItems();

        // Assert
        Assert.Equal(2, MockHandler.Requests.Count);
        AssertRequest(MockHandler.Requests[0], HttpMethod.Get, tableEndpoint);
        AssertRequest(MockHandler.Requests[1], HttpMethod.Get, $"{tableEndpoint}?page=2");
        Assert.Equal(10, items.Count);
        AssertEx.SequenceEqual(page1.Items.ToList(), items.Take(5).ToList());
        AssertEx.SequenceEqual(page2.Items.ToList(), items.Skip(5).Take(5).ToList());
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_TwoPagesOfItems_WithAuth_WhenItemsReturned()
    {
        // Arrange
        var page1 = CreatePageOfJsonItems(5, null, new Uri($"{tableEndpoint}?page=2"));
        var page2 = CreatePageOfJsonItems(5);

        // Act
        var items = await GetAllItems(authTable);

        // Assert
        Assert.Equal(2, MockHandler.Requests.Count);
        AssertRequest(MockHandler.Requests[0], HttpMethod.Get, tableEndpoint);
        AssertEx.HasHeader(MockHandler.Requests[0].Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        AssertRequest(MockHandler.Requests[1], HttpMethod.Get, $"{tableEndpoint}?page=2");
        AssertEx.HasHeader(MockHandler.Requests[1].Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.Equal(10, items.Count);
        AssertEx.SequenceEqual(page1.Items.ToList(), items.Take(5).ToList());
        AssertEx.SequenceEqual(page2.Items.ToList(), items.Skip(5).Take(5).ToList());
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_ThreePagesOfItems_WhenItemsReturned()
    {
        // Arrange
        var page1 = CreatePageOfJsonItems(5, 10, new Uri($"{tableEndpoint}?page=2"));
        var page2 = CreatePageOfJsonItems(5, 10, new Uri($"{tableEndpoint}?page=3"));
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<JObject>());

        // Act
        var items = await GetAllItems(table, 10);

        // Assert
        Assert.Equal(3, MockHandler.Requests.Count);
        AssertRequest(MockHandler.Requests[0], HttpMethod.Get, tableEndpoint);
        AssertRequest(MockHandler.Requests[1], HttpMethod.Get, $"{tableEndpoint}?page=2");
        AssertRequest(MockHandler.Requests[2], HttpMethod.Get, $"{tableEndpoint}?page=3");
        Assert.Equal(10, items.Count);
        AssertEx.SequenceEqual(page1.Items.ToList(), items.Take(5).ToList());
        AssertEx.SequenceEqual(page2.Items.ToList(), items.Skip(5).Take(5).ToList());
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_ThreePagesOfItems_WithAuth_WhenItemsReturned()
    {
        // Arrange
        var page1 = CreatePageOfJsonItems(5, 10, new Uri($"{tableEndpoint}?page=2"));
        var page2 = CreatePageOfJsonItems(5, 10, new Uri($"{tableEndpoint}?page=3"));
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<JObject>());

        // Act
        var items = await GetAllItems(authTable, 10);

        // Assert
        Assert.Equal(3, MockHandler.Requests.Count);
        AssertRequest(MockHandler.Requests[0], HttpMethod.Get, tableEndpoint);
        AssertEx.HasHeader(MockHandler.Requests[0].Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        AssertRequest(MockHandler.Requests[1], HttpMethod.Get, $"{tableEndpoint}?page=2");
        AssertEx.HasHeader(MockHandler.Requests[1].Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        AssertRequest(MockHandler.Requests[2], HttpMethod.Get, $"{tableEndpoint}?page=3");
        AssertEx.HasHeader(MockHandler.Requests[2].Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.Equal(10, items.Count);
        AssertEx.SequenceEqual(page1.Items.ToList(), items.Take(5).ToList());
        AssertEx.SequenceEqual(page2.Items.ToList(), items.Skip(5).Take(5).ToList());
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_InvalidItems_Skipped()
    {
        // Arrange
        AddStringResponse("{\"items\":\"foo\"}");
        var wrappedTable = new WrappedRemoteTable("movies", GetMockClient());

        // Act
        var page = await wrappedTable.P_GetNextPage();

        // Assert
        Assert.Null(page.Items);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_InvalidCount_Skipped()
    {
        // Arrange
        AddStringResponse("{\"count\":\"foo\"}");
        var wrappedTable = new WrappedRemoteTable("movies", GetMockClient());

        // Act
        var page = await wrappedTable.P_GetNextPage();

        // Assert
        Assert.Null(page.Count);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_InvalidNextLink_Skipped()
    {
        // Arrange
        AddStringResponse("{\"nextLink\":[]}");
        var wrappedTable = new WrappedRemoteTable("movies", GetMockClient());

        // Act
        var page = await wrappedTable.P_GetNextPage();

        // Assert
        Assert.Null(page.NextLink);
    }

    private class WrappedRemoteTable : Client.Table.RemoteTable
    {
        public WrappedRemoteTable(string path, DatasyncClient client) : base(path, client) { }
        public Task<Page<JToken>> P_GetNextPage(string query = "", string nextLink = null)
            => base.GetNextPageAsync(query, nextLink);
    }
}

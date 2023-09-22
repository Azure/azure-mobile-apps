// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTableOfT;

[ExcludeFromCodeCoverage]
public class GetAsyncItems_Tests : BaseOperationTest
{
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
        var pageable = table.GetAsyncItems<IdEntity>("");
        var enumerator = pageable.GetAsyncEnumerator();

        // Assert
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(async () => await enumerator.MoveNextAsync().ConfigureAwait(false)).ConfigureAwait(false);
        Assert.Equal(statusCode, ex.Response?.StatusCode);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_NoItems()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

        // Act
        var enumerator = table.GetAsyncItems<IdEntity>("").GetAsyncEnumerator();
        var hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);

        // Assert
        AssertSingleRequest(HttpMethod.Get, tableEndpoint);
        Assert.False(hasMore);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_NoItems_WithAuth()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

        // Act
        var enumerator = authTable.GetAsyncItems<IdEntity>("").GetAsyncEnumerator();
        var hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, tableEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.False(hasMore);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetItemsAsync_OnePageOfItems_WhenItemsReturned()
    {
        // Arrange
        var page = CreatePageOfItems(5);

        // Act
        List<IdEntity> items = await table.GetAsyncItems<IdEntity>("").ToListAsync();

        // Assert
        AssertSingleRequest(HttpMethod.Get, tableEndpoint);
        Assert.Equal(page.Items, items);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetItemsAsync_OnePageOfItems_WithAuth_WhenItemsReturned()
    {
        // Arrange
        var page = CreatePageOfItems(5);

        // Act
        List<IdEntity> items = await authTable.GetAsyncItems<IdEntity>("").ToListAsync();

        // Assert - request
        var request = AssertSingleRequest(HttpMethod.Get, tableEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.Equal(page.Items, items);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetItemsAsync_TwoPagesOfItems_WhenItemsReturned()
    {
        // Arrange
        var page1 = CreatePageOfItems(5, null, new Uri($"{tableEndpoint}?page=2"));
        var page2 = CreatePageOfItems(5);

        // Act
        List<IdEntity> items = await table.GetAsyncItems<IdEntity>("").ToListAsync();

        // Assert
        Assert.Equal(2, MockHandler.Requests.Count);
        AssertRequest(MockHandler.Requests[0], HttpMethod.Get, tableEndpoint);
        AssertRequest(MockHandler.Requests[1], HttpMethod.Get, page1.NextLink.ToString());
        Assert.Equal(10, items.Count);
        Assert.Equal(page1.Items, items.Take(5));
        Assert.Equal(page2.Items, items.Skip(5).Take(5));
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetItemsAsync_TwoPagesOfItems_WithAuth_WhenItemsReturned()
    {
        // Arrange
        var page1 = CreatePageOfItems(5, null, new Uri($"{expectedEndpoint}?page=2"));
        var page2 = CreatePageOfItems(5);

        // Act
        List<IdEntity> items = await authTable.GetAsyncItems<IdEntity>("").ToListAsync();

        // Assert
        Assert.Equal(2, MockHandler.Requests.Count);
        var request = AssertRequest(MockHandler.Requests[0], HttpMethod.Get, tableEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        request = AssertRequest(MockHandler.Requests[1], HttpMethod.Get, page1.NextLink.ToString());
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.Equal(10, items.Count);
        Assert.Equal(page1.Items, items.Take(5));
        Assert.Equal(page2.Items, items.Skip(5).Take(5));
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetItemsAsync_ThreePagesOfItems_WhenItemsReturned()
    {
        // Arrange
        var page1 = CreatePageOfItems(5, null, new Uri($"{expectedEndpoint}?page=2"));
        var page2 = CreatePageOfItems(5, null, new Uri($"{expectedEndpoint}?page=3"));
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

        // Act
        List<IdEntity> items = await table.GetAsyncItems<IdEntity>("").ToListAsync();

        // Assert - request
        Assert.Equal(3, MockHandler.Requests.Count);
        AssertRequest(MockHandler.Requests[0], HttpMethod.Get, tableEndpoint);
        AssertRequest(MockHandler.Requests[1], HttpMethod.Get, page1.NextLink.ToString());
        AssertRequest(MockHandler.Requests[2], HttpMethod.Get, page2.NextLink.ToString());
        Assert.Equal(10, items.Count);
        Assert.Equal(page1.Items, items.Take(5));
        Assert.Equal(page2.Items, items.Skip(5).Take(5));
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetItemsAsync_SetsCount()
    {
        // Arrange
        _ = CreatePageOfItems(5, 5);
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");

        // Act
        var pageable = table.GetAsyncItems<IdEntity>("") as AsyncPageable<IdEntity>;
        var enumerator = pageable.GetAsyncEnumerator();
        _ = await enumerator.MoveNextAsync().ConfigureAwait(false);

        // Assert
        Assert.Equal(5, pageable.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("$filter=Year eq 1900&$count=true")]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetPageOfItems_ConstructsRequest_WithQuery(string query)
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
        var expectedUri = string.IsNullOrEmpty(query) ? tableEndpoint : $"{tableEndpoint}?{query}";

        // Act
        await (table as RemoteTable<IdEntity>)!.GetNextPageAsync<IdEntity>(query, null).ConfigureAwait(false);

        // Assert
        AssertSingleRequest(HttpMethod.Get, expectedUri);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("$filter=Year eq 1900&$count=true")]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetPageOfItems_ConstructsRequest_WithQueryAndAuth(string query)
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
        var expectedUri = string.IsNullOrEmpty(query) ? tableEndpoint : $"{tableEndpoint}?{query}";

        // Act
        await (authTable as RemoteTable<IdEntity>)!.GetNextPageAsync<IdEntity>(query, null).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, expectedUri);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
    }

    [Theory]
    [InlineData("https://localhost/tables/foo/?$count=true")]
    [InlineData("https://localhost/tables/foo/?$count=true&$skip=5&$top=10")]
    [InlineData("https://localhost/tables/foo/?$count=true&$skip=5&$top=10&__includedeleted=true")]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetPageOfItems_ConstructsRequest_WithRequestUri(string requestUri)
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

        // Act
        await (table as RemoteTable<IdEntity>)!.GetNextPageAsync<IdEntity>("", requestUri).ConfigureAwait(false);

        // Assert
        AssertSingleRequest(HttpMethod.Get, requestUri);
    }

    [Theory]
    [InlineData("https://localhost/tables/foo/?$count=true")]
    [InlineData("https://localhost/tables/foo/?$count=true&$skip=5&$top=10")]
    [InlineData("https://localhost/tables/foo/?$count=true&$skip=5&$top=10&__includedeleted=true")]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetPageOfItems_ConstructsRequest_WithRequestUriAndAuth(string requestUri)
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

        // Act
        await (authTable as RemoteTable<IdEntity>)!.GetNextPageAsync<IdEntity>("", requestUri).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, requestUri);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("$filter=Year eq 1900&$count=true")]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetPageOfItems_ConstructsRequest_PrefersRequestUri(string query)
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
        const string requestUri = "https://localhost/tables/foo?$count=true&$skip=5&$top=10&__includedeleted=true";

        // Act
        _ = await (table as RemoteTable<IdEntity>)!.GetNextPageAsync<IdEntity>(query, requestUri).ConfigureAwait(false);

        // Assert
        AssertSingleRequest(HttpMethod.Get, requestUri);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("$filter=Year eq 1900&$count=true")]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetPageOfItems_ConstructsRequest_PrefersRequestUri_WithAuth(string query)
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
        const string requestUri = "https://localhost/tables/foo?$count=true&$skip=5&$top=10&__includedeleted=true";

        // Act
        _ = await (authTable as RemoteTable<IdEntity>)!.GetNextPageAsync<IdEntity>(query, requestUri).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, requestUri);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
    }

    [Fact]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetPageOfItems_ReturnsItems()
    {
        // Arrange
        Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } } };
        MockHandler.AddResponse(HttpStatusCode.OK, result);

        // Act
        var response = await (table as RemoteTable<IdEntity>)!.GetNextPageAsync<IdEntity>("", null).ConfigureAwait(false);

        // Assert
        Assert.Single(response.Items);
        Assert.Equal("1234", response.Items.First().Id);
        Assert.Null(response.Count);
        Assert.Null(response.NextLink);
    }

    [Fact]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetPageOfItems_ReturnsCount()
    {
        // Arrange
        Page<IdEntity> result = new() { Count = 42 };
        MockHandler.AddResponse(HttpStatusCode.OK, result);

        // Act
        var response = await (table as RemoteTable<IdEntity>)!.GetNextPageAsync<IdEntity>("", null).ConfigureAwait(false);

        // Assert
        Assert.Equal(42, response.Count);
        Assert.Null(response.Items);
        Assert.Null(response.NextLink);
    }

    [Fact]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetPageOfItems_ReturnsNextLink()
    {
        // Arrange
        Page<IdEntity> result = new() { NextLink = new Uri(Endpoint.ToString() + "?$top=5&$skip=5") };
        MockHandler.AddResponse(HttpStatusCode.OK, result);

        // Act
        var response = await (table as RemoteTable<IdEntity>)!.GetNextPageAsync<IdEntity>("", null).ConfigureAwait(false);

        // Assert
        Assert.Null(response.Items);
        Assert.Null(response.Count);
        Assert.Equal(result.NextLink.ToString(), response.NextLink.ToString());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.UnavailableForLegalReasons)]
    [InlineData(HttpStatusCode.ExpectationFailed)]
    [Trait("Method", "GetNextPageAsync")]
    public async Task GetPageOfItemAsync_BadResponse_ThrowsRequestFailed(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);
        var t = table as RemoteTable<IdEntity>;

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => t.GetNextPageAsync<IdEntity>("", null)).ConfigureAwait(false);

        // Assert
        Assert.Equal(statusCode, ex.Response?.StatusCode);
    }
}

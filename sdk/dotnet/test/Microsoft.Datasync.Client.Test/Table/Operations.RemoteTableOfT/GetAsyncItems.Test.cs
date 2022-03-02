// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Table;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTableOfT
{
    [ExcludeFromCodeCoverage]
    public class GetAsyncItems_Tests : BaseOperationTest
    {
        // TODO: Add GetAsyncItems tests

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
}

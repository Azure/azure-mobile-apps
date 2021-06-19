// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class DatasyncTable_Tests : BaseTest
    {
        private const string sEndpoint = "https://foo.azurewebsites.net/tables/movies/";
        private readonly IdEntity payload = new() { Id = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f", StringValue = "test" };
        private readonly Dictionary<string, object> changes = new() { { "stringValue", "test" } };
        private const string sJsonPayload = "{\"id\":\"db0ec08d-46a9-465d-9f5e-0066a3ee5b5f\",\"stringValue\":\"test\"}";
        private const string sBadJson = "{this-is-bad-json";
        private const string sId = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f";

        #region Ctor
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_SetsInternals()
        {
            Assert.Equal("https://foo.azurewebsites.net/tables/movies/", Table.Endpoint.ToString());
            Assert.Same(ClientOptions, Table.ClientOptions);
            Assert.Same(HttpClient, Table.HttpClient);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullEndpoint_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DatasyncTable<MockObject>(null, HttpClient, ClientOptions));
        }

        [Theory]
        [InlineData("file://localhost/foo")]
        [InlineData("http://foo.azurewebsites.net")]
        [Trait("Method", "Ctor")]
        public void Ctor_InvalidEndpoint_Throws(string endpoint)
        {
            Assert.Throws<UriFormatException>(() => new DatasyncTable<MockObject>(new Uri(endpoint), HttpClient, ClientOptions));
        }

        [Theory]
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://localhost?__filter=foo", "http://localhost/")]
        [InlineData("http://localhost#fragment", "http://localhost/")]
        [InlineData("http://localhost/", "http://localhost/")]
        [InlineData("http://localhost/?__filter=foo", "http://localhost/")]
        [InlineData("http://localhost/#fragment", "http://localhost/")]
        [InlineData("https://foo.azurewebsites.net", "https://foo.azurewebsites.net/")]
        [InlineData("https://foo.azurewebsites.net?__filter=foo", "https://foo.azurewebsites.net/")]
        [InlineData("https://foo.azurewebsites.net#fragment", "https://foo.azurewebsites.net/")]
        [InlineData("https://foo.azurewebsites.net/", "https://foo.azurewebsites.net/")]
        [InlineData("https://foo.azurewebsites.net/?__filter=foo", "https://foo.azurewebsites.net/")]
        [InlineData("https://foo.azurewebsites.net/#fragment", "https://foo.azurewebsites.net/")]
        [InlineData("https://foo.azurewebsites.net/tables/movies", "https://foo.azurewebsites.net/tables/movies/")]
        [InlineData("https://foo.azurewebsites.net/tables/movies?__filter=foo", "https://foo.azurewebsites.net/tables/movies/")]
        [InlineData("https://foo.azurewebsites.net/tables/movies#fragment", "https://foo.azurewebsites.net/tables/movies/")]
        [InlineData("https://foo.azurewebsites.net/tables/movies/", "https://foo.azurewebsites.net/tables/movies/")]
        [InlineData("https://foo.azurewebsites.net/tables/movies/?__filter=foo", "https://foo.azurewebsites.net/tables/movies/")]
        [InlineData("https://foo.azurewebsites.net/tables/movies/#fragment", "https://foo.azurewebsites.net/tables/movies/")]
        [Trait("Method", "Ctor")]
        public void Ctor_ValidEndpoint_StoresNormalizedEndpoint(string endpoint, string expected)
        {
            var table = new DatasyncTable<MockObject>(new Uri(endpoint), HttpClient, ClientOptions);
            Assert.Equal(expected, table.Endpoint.ToString());
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullClient_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DatasyncTable<MockObject>(Endpoint, null, ClientOptions));
        }
        #endregion

        #region CreateItemAsync
        [Fact]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_NullItem_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.CreateItemAsync(null)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_Success_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode, payload);

            var response = await Table.CreateItemAsync(payload).ConfigureAwait(false);

            // Check Request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
            Assert.Equal(sJsonPayload, request.Content.ReadAsStringAsync().Result);
            Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);

            // Check Response
            Assert.Equal((int)statusCode, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            Assert.True(response.HasValue);
            Assert.Equal("test", response.Value.StringValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_SuccessNoContent_Throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.CreateItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_SuccessWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            ClientHandler.Responses.Add(response);

            await Assert.ThrowsAsync<JsonException>(() => Table.CreateItemAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode, payload);

            var ex = await Assert.ThrowsAsync<ConflictException<IdEntity>>(() => Table.CreateItemAsync(payload)).ConfigureAwait(false);

            // Check Request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
            Assert.Equal(sJsonPayload, request.Content.ReadAsStringAsync().Result);
            AssertEx.Equals("application/json", request.Content.Headers.ContentType.MediaType);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.True(ex.Response.IsConflictStatusCode);
            Assert.True(ex.Response.HasContent);
            Assert.Equal("test", ex.ServerItem.StringValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_ConflictNoContent_throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.CreateItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            ClientHandler.Responses.Add(response);

            await Assert.ThrowsAsync<JsonException>(() => Table.CreateItemAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);

            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.CreateItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }
        #endregion

        #region DeleteItemAsync
        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_NullId_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.DeleteItemAsync(null)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("abcdef gh")]
        [InlineData("!!!")]
        [InlineData("?")]
        [InlineData(";")]
        [InlineData("{EA235ADF-9F38-44EA-8DA4-EF3D24755767}")]
        [InlineData("###")]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemsync_InvalidId_Throws(string id)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => Table.DeleteItemAsync(id)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Success_FormulatesCorrectResponse(bool hasPrecondition)
        {
            ClientHandler.AddResponse(HttpStatusCode.NoContent);

            var response = hasPrecondition
                ? await Table.DeleteItemAsync(sId, HttpCondition.IfMatch("etag")).ConfigureAwait(false)
                : await Table.DeleteItemAsync(sId).ConfigureAwait(false);

            // Check Request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
            if (hasPrecondition)
            {
                AssertEx.HasValue("If-Match", new[] { "\"etag\"" }, request.Headers);
            }
            else
            {
                Assert.False(request.Headers.Contains("If-Match"));
            }

            // Check Response
            Assert.Equal(204, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.False(response.HasContent);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode, payload);

            var ex = await Assert.ThrowsAsync<ConflictException<IdEntity>>(() => Table.DeleteItemAsync(sId)).ConfigureAwait(false);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.True(ex.Response.IsConflictStatusCode);
            Assert.True(ex.Response.HasContent);
            Assert.Equal("test", ex.ServerItem.StringValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConflictNoContent_throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.DeleteItemAsync(sId)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            ClientHandler.Responses.Add(response);

            await Assert.ThrowsAsync<JsonException>(() => Table.DeleteItemAsync(sId)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);

            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.DeleteItemAsync(sId)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }
        #endregion

        #region GetAsyncItems
        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.UnavailableForLegalReasons)]
        [InlineData(HttpStatusCode.ExpectationFailed)]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_Throws_OnBadRequest(HttpStatusCode statusCode)
        {
            // Arrange
            ClientHandler.AddResponse(statusCode);

            // Act
            var pageable = Table.GetAsyncItems<IdEntity>();
            var enumerator = pageable.GetAsyncEnumerator();

            // Assert
            await Assert.ThrowsAsync<RequestFailedException>(async () => _ = await enumerator.MoveNextAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_NoItems_WhenNoItemsReturned()
        {
            // Arrange
            ClientHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

            // Act
            var pageable = Table.GetAsyncItems<IdEntity>();
            var enumerator = pageable.GetAsyncEnumerator();
            var hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert - request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);

            // Assert - response
            Assert.False(hasMore);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_NoItems_WhenNullResponseReturned()
        {
            // Technically, the whole system is set up so that we throw instead of having a null response.
            // However, there are branches to ensure bad things don't happen if a null response is generated.
            // This test is explicitly for those branches
            Task<HttpResponse<Page<IdEntity>>> pageFunc(string _)
                => HttpResponse.FromResponseAsync<Page<IdEntity>>(new HttpResponseMessage(HttpStatusCode.OK), ClientOptions.DeserializerOptions);
            FuncAsyncPageable<IdEntity> pageable = new(pageFunc);

            // Now we get the AsPages() which is an IAsyncEnumerable<Page<IdEntity>>
            var enumerator = pageable.AsPages().GetAsyncEnumerator();
            _ = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Current should be a blank entity
            Assert.Null(enumerator.Current.Items);
            Assert.Null(enumerator.Current.Count);
            Assert.Null(enumerator.Current.NextLink);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_OnePageOfItems_WhenItemsReturned()
        {
            // Arrange
            var page = CreatePageOfItems(5);
            List<IdEntity> items = new();

            // Act
            await foreach (var item in Table.GetAsyncItems<IdEntity>())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);

            // Assert - response
            Assert.Equal(page.Items, items);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_TwoPagesOfItems_WhenItemsReturned()
        {
            // Arrange
            var page1 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5);
            List<IdEntity> items = new();

            // Act
            await foreach (var item in Table.GetAsyncItems<IdEntity>())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(2, ClientHandler.Requests.Count);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);

            request = ClientHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);

            // Assert - response
            Assert.Equal(10, items.Count);
            Assert.Equal(page1.Items, items.Take(5));
            Assert.Equal(page2.Items, items.Skip(5).Take(5));
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_ThreePagesOfItems_WhenItemsReturned()
        {
            // Arrange
            var page1 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=3"));
            ClientHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            List<IdEntity> items = new();

            // Act
            await foreach (var item in Table.GetAsyncItems<IdEntity>())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(3, ClientHandler.Requests.Count);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);

            request = ClientHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);

            request = ClientHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=3", request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);

            // Assert - response
            Assert.Equal(10, items.Count);
            Assert.Equal(page1.Items, items.Take(5));
            Assert.Equal(page2.Items, items.Skip(5).Take(5));
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_SetsCountAndResponse()
        {
            // Arrange
            _ = CreatePageOfItems(5, 5);

            // Act
            var pageable = Table.GetAsyncItems<IdEntity>();
            var enumerator = pageable.GetAsyncEnumerator();
            _ = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert - request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);

            // Assert - response
            Assert.Equal(5, pageable.Count);
            Assert.NotNull(pageable.CurrentResponse);
            Assert.Equal(200, pageable.CurrentResponse.StatusCode);
            Assert.True(pageable.CurrentResponse.HasContent);
            Assert.NotEmpty(pageable.CurrentResponse.Content);
        }
        #endregion

        #region GetItemAsync
        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_NullId_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.GetItemAsync(null)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("abcdef gh")]
        [InlineData("!!!")]
        [InlineData("?")]
        [InlineData(";")]
        [InlineData("{EA235ADF-9F38-44EA-8DA4-EF3D24755767}")]
        [InlineData("###")]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_InvalidId_Throws(string id)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => Table.GetItemAsync(id)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_Success_FormulatesCorrectResponse(bool hasPrecondition)
        {
            ClientHandler.AddResponse(HttpStatusCode.OK, payload);

            var response = hasPrecondition
                ? await Table.GetItemAsync(sId, HttpCondition.IfNotMatch("etag")).ConfigureAwait(false)
                : await Table.GetItemAsync(sId).ConfigureAwait(false);

            // Check Request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
            if (hasPrecondition)
            {
                AssertEx.HasValue("If-None-Match", new[] { "\"etag\"" }, request.Headers);
            }
            else
            {
                Assert.False(request.Headers.Contains("If-None-Match"));
            }

            // Check Response
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            Assert.Equal(sJsonPayload, Encoding.UTF8.GetString(response.Content));
            Assert.True(response.HasValue);
            Assert.Equal("test", response.Value.StringValue);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_SuccessNoContent_Throws()
        {
            ClientHandler.AddResponse(HttpStatusCode.OK);
            await Assert.ThrowsAsync<RequestFailedException>(() => Table.GetItemAsync(sId)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);

            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.GetItemAsync(sId)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_NotModified_Throws()
        {
            ClientHandler.AddResponse(HttpStatusCode.NotModified);

            var ex = await Assert.ThrowsAsync<NotModifiedException>(() => Table.GetItemAsync(sId)).ConfigureAwait(false);
            Assert.Equal(304, ex.StatusCode);
        }
        #endregion

        #region GetPageOfItemsAsync
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("$filter=Year eq 1900&$count=true")]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ConstructsRequest_WithQuery(string query)
        {
            Page<IdEntity> result = new();
            ClientHandler.AddResponse(HttpStatusCode.OK, result);
            var expectedUri = string.IsNullOrEmpty(query) ? sEndpoint : $"{sEndpoint}?{query}";

            _ = await Table.GetPageOfItemsAsync<IdEntity>(query).ConfigureAwait(false);

            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedUri, request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
        }

        [Theory]
        [InlineData("https://localhost/tables/foo/?$count=true")]
        [InlineData("https://localhost/tables/foo/?$count=true&$skip=5&$top=10")]
        [InlineData("https://localhost/tables/foo/?$count=true&$skip=5&$top=10&__includedeleted=true")]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ConstructsRequest_WithRequestUri(string requestUri)
        {
            // Arrange
            Page<IdEntity> result = new();
            ClientHandler.AddResponse(HttpStatusCode.OK, result);
            var table = new DatasyncTable<IdEntity>(Endpoint, HttpClient, ClientOptions);

            // Act
            _ = await table.GetPageOfItemsAsync<IdEntity>("", requestUri).ConfigureAwait(false);

            // Assert
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(requestUri, request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("$filter=Year eq 1900&$count=true")]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ConstructsRequest_PrefersRequestUri(string query)
        {
            // Arrange
            Page<IdEntity> result = new();
            ClientHandler.AddResponse(HttpStatusCode.OK, result);
            var table = new DatasyncTable<IdEntity>(Endpoint, HttpClient, ClientOptions);
            const string requestUri = "https://localhost/tables/foo?$count=true&$skip=5&$top=10&__includedeleted=true";

            // Act
            _ = await table.GetPageOfItemsAsync<IdEntity>(query, requestUri).ConfigureAwait(false);

            // Assert
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(requestUri, request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsItems()
        {
            // Arrange
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } } };
            ClientHandler.AddResponse(HttpStatusCode.OK, result);
            var table = new DatasyncTable<IdEntity>(Endpoint, HttpClient, ClientOptions);

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<HttpResponse<Page<IdEntity>>>(response);
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasContent);
            Assert.Equal("{\"items\":[{\"id\":\"1234\"}]}", Encoding.UTF8.GetString(response.Content));
            Assert.True(response.HasValue);
            Assert.IsAssignableFrom<Page<IdEntity>>(response.Value);
            Assert.Single(response.Value.Items);
            Assert.Equal("1234", response.Value.Items.First().Id);
            Assert.Null(response.Value.Count);
            Assert.Null(response.Value.NextLink);
        }

        [Fact]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsCount()
        {
            // Arrange
            Page<IdEntity> result = new() { Count = 42 };
            ClientHandler.AddResponse(HttpStatusCode.OK, result);
            var table = new DatasyncTable<IdEntity>(Endpoint, HttpClient, ClientOptions);

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<HttpResponse<Page<IdEntity>>>(response);
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasContent);
            Assert.Equal("{\"count\":42}", Encoding.UTF8.GetString(response.Content));
            Assert.True(response.HasValue);
            Assert.IsAssignableFrom<Page<IdEntity>>(response.Value);
            Assert.Null(response.Value.Items);
            Assert.Equal(42, response.Value.Count);
            Assert.Null(response.Value.NextLink);
        }

        [Fact]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsNextLink()
        {
            // Arrange
            const string nextLink = sEndpoint + "?$top=5&$skip=5";
            Page<IdEntity> result = new() { NextLink = new Uri(nextLink) };
            ClientHandler.AddResponse(HttpStatusCode.OK, result);
            var table = new DatasyncTable<IdEntity>(Endpoint, HttpClient, ClientOptions);

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<HttpResponse<Page<IdEntity>>>(response);
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasContent);
            Assert.Equal($"{{\"nextLink\":\"{nextLink}\"}}", Encoding.UTF8.GetString(response.Content));
            Assert.True(response.HasValue);
            Assert.IsAssignableFrom<Page<IdEntity>>(response.Value);
            Assert.Null(response.Value.Items);
            Assert.Null(response.Value.Count);
            Assert.Equal(new Uri(nextLink), response.Value.NextLink);
        }

        [Fact]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsItemsAndCount()
        {
            // Arrange
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } }, Count = 42 };
            ClientHandler.AddResponse(HttpStatusCode.OK, result);
            var table = new DatasyncTable<IdEntity>(Endpoint, HttpClient, ClientOptions);

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<HttpResponse<Page<IdEntity>>>(response);
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasContent);
            Assert.Equal("{\"items\":[{\"id\":\"1234\"}],\"count\":42}", Encoding.UTF8.GetString(response.Content));
            Assert.True(response.HasValue);
            Assert.IsAssignableFrom<Page<IdEntity>>(response.Value);
            Assert.Single(response.Value.Items);
            Assert.Equal("1234", response.Value.Items.First().Id);
            Assert.Equal(42, response.Value.Count);
            Assert.Null(response.Value.NextLink);
        }

        [Fact]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsItemsAndNextLink()
        {
            // Arrange
            const string nextLink = sEndpoint + "?$top=5&$skip=5";
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } }, NextLink = new Uri(nextLink) };
            ClientHandler.AddResponse(HttpStatusCode.OK, result);
            var table = new DatasyncTable<IdEntity>(Endpoint, HttpClient, ClientOptions);

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<HttpResponse<Page<IdEntity>>>(response);
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasContent);
            Assert.Equal($"{{\"items\":[{{\"id\":\"1234\"}}],\"nextLink\":\"{nextLink}\"}}", Encoding.UTF8.GetString(response.Content));
            Assert.True(response.HasValue);
            Assert.IsAssignableFrom<Page<IdEntity>>(response.Value);
            Assert.Single(response.Value.Items);
            Assert.Equal("1234", response.Value.Items.First().Id);
            Assert.Null(response.Value.Count);
            Assert.Equal(new Uri(nextLink), response.Value.NextLink);
        }

        [Fact]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsItem_Count_NextLink()
        {
            // Arrange
            const string nextLink = sEndpoint + "?$top=5&$skip=5";
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } }, Count = 42, NextLink = new Uri(nextLink) };
            ClientHandler.AddResponse(HttpStatusCode.OK, result);
            var table = new DatasyncTable<IdEntity>(Endpoint, HttpClient, ClientOptions);

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<HttpResponse<Page<IdEntity>>>(response);
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasContent);
            Assert.Equal($"{{\"items\":[{{\"id\":\"1234\"}}],\"count\":42,\"nextLink\":\"{nextLink}\"}}", Encoding.UTF8.GetString(response.Content));
            Assert.True(response.HasValue);
            Assert.IsAssignableFrom<Page<IdEntity>>(response.Value);
            Assert.Single(response.Value.Items);
            Assert.Equal("1234", response.Value.Items.First().Id);
            Assert.Equal(42, response.Value.Count);
            Assert.Equal(new Uri(nextLink), response.Value.NextLink);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.UnavailableForLegalReasons)]
        [InlineData(HttpStatusCode.ExpectationFailed)]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItemAsync_BadResponse_ThrowsRequestFailed(HttpStatusCode statusCode)
        {
            // Arrange
            ClientHandler.AddResponse(statusCode);
            var table = new DatasyncTable<IdEntity>(Endpoint, HttpClient, ClientOptions);

            // Act
            var exception = await Assert.ThrowsAsync<RequestFailedException>(() => table.GetPageOfItemsAsync<IdEntity>(null)).ConfigureAwait(false);

            // Assert
            Assert.Equal((int)statusCode, exception.StatusCode);
            Assert.False(exception.Response.HasContent);
        }
        #endregion

        #region ReplaceItemAsync
        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_NullId_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.ReplaceItemAsync(null)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("abcdef gh")]
        [InlineData("!!!")]
        [InlineData("?")]
        [InlineData(";")]
        [InlineData("{EA235ADF-9F38-44EA-8DA4-EF3D24755767}")]
        [InlineData("###")]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_InvalidId_Throws(string id)
        {
            IdEntity entity = new() { Id = id };
            await Assert.ThrowsAsync<ArgumentException>(() => Table.ReplaceItemAsync(entity)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Success_FormulatesCorrectResponse(bool hasPrecondition)
        {
            ClientHandler.AddResponse(HttpStatusCode.OK, payload);

            var response = hasPrecondition
                ? await Table.ReplaceItemAsync(payload, HttpCondition.IfMatch("etag")).ConfigureAwait(false)
                : await Table.ReplaceItemAsync(payload).ConfigureAwait(false);

            // Check Request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal($"{sEndpoint}{payload.Id}", request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
            if (hasPrecondition)
            {
                AssertEx.HasValue("If-Match", new[] { "\"etag\"" }, request.Headers);
            }
            else
            {
                Assert.False(request.Headers.Contains("If-Match"));
            }

            // Check Response
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            Assert.Equal(sJsonPayload, Encoding.UTF8.GetString(response.Content));
            Assert.True(response.HasValue);
            Assert.Equal("test", response.Value.StringValue);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_SuccessNoContent_Throws()
        {
            ClientHandler.AddResponse(HttpStatusCode.OK);

            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.ReplaceItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal(200, ex.StatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode, payload);

            var ex = await Assert.ThrowsAsync<ConflictException<IdEntity>>(() => Table.ReplaceItemAsync(payload)).ConfigureAwait(false);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.True(ex.Response.IsConflictStatusCode);
            Assert.True(ex.Response.HasContent);
            Assert.Equal("test", ex.ServerItem.StringValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ConflictNoContent_throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.ReplaceItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            ClientHandler.Responses.Add(response);

            await Assert.ThrowsAsync<JsonException>(() => Table.ReplaceItemAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);

            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.ReplaceItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }
        #endregion

        #region UpdateItemAsync
        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_NullId_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.UpdateItemAsync(null, changes)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_NullChanges_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.UpdateItemAsync(sId, null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_EmptyChanges_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => Table.UpdateItemAsync(sId, new Dictionary<string, object>())).ConfigureAwait(false);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("abcdef gh")]
        [InlineData("!!!")]
        [InlineData("?")]
        [InlineData(";")]
        [InlineData("{EA235ADF-9F38-44EA-8DA4-EF3D24755767}")]
        [InlineData("###")]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_InvalidId_Throws(string id)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => Table.UpdateItemAsync(id, changes)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_Success_FormulatesCorrectResponse(bool hasPrecondition)
        {
            ClientHandler.AddResponse(HttpStatusCode.OK, payload);

            var response = hasPrecondition
                ? await Table.UpdateItemAsync(sId, changes, HttpCondition.IfMatch("etag")).ConfigureAwait(false)
                : await Table.UpdateItemAsync(sId, changes).ConfigureAwait(false);

            // Check Request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal("patch", request.Method.ToString().ToLower());
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
            if (hasPrecondition)
            {
                AssertEx.HasValue("If-Match", new[] { "\"etag\"" }, request.Headers);
            }
            else
            {
                Assert.False(request.Headers.Contains("If-Match"));
            }

            // Check Response
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            Assert.Equal(sJsonPayload, Encoding.UTF8.GetString(response.Content));
            Assert.True(response.HasValue);
            Assert.Equal("test", response.Value.StringValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode, payload);

            var ex = await Assert.ThrowsAsync<ConflictException<IdEntity>>(() => Table.UpdateItemAsync(sId, changes)).ConfigureAwait(false);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.True(ex.Response.IsConflictStatusCode);
            Assert.True(ex.Response.HasContent);
            Assert.Equal("test", ex.ServerItem.StringValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ConflictNoContent_throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.UpdateItemAsync(sId, changes)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            ClientHandler.Responses.Add(response);

            await Assert.ThrowsAsync<JsonException>(() => Table.UpdateItemAsync(sId, changes)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);

            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.UpdateItemAsync(sId, changes)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_SuccessNoContent_Throws()
        {
            ClientHandler.AddResponse(HttpStatusCode.OK);

            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.UpdateItemAsync(sId, changes)).ConfigureAwait(false);
            Assert.Equal(200, ex.StatusCode);
        }
        #endregion
    }
}

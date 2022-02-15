// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table
{
    [ExcludeFromCodeCoverage]
    public class RemoteTableOfT_Tests : BaseTest
    {
        private readonly IdEntity payload = new() { Id = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f", StringValue = "test" };
        private const string sJsonPayload = "{\"id\":\"db0ec08d-46a9-465d-9f5e-0066a3ee5b5f\",\"stringValue\":\"test\"}";
        private readonly Dictionary<string, object> changes = new() { { "stringValue", "test" } };
        private const string sBadJson = "{this-is-bad-json";

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_SetsInternals()
        {
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            var client = GetMockClient();
            var sut = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            Assert.Equal(sEndpoint, sut.Endpoint.ToString());
            Assert.Same(client.ClientOptions, sut.ClientOptions);
            Assert.Same(client.HttpClient, sut.HttpClient);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullEndpoint_Throws()
        {
            var client = GetMockClient();
            const string relativeUri = null;
            Assert.Throws<ArgumentNullException>(() => new RemoteTable<IdEntity>(relativeUri, client.HttpClient, client.ClientOptions));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullClient_Throws()
        {
            var client = GetMockClient();
            Assert.Throws<ArgumentNullException>(() => new RemoteTable<IdEntity>("tables/movies", null, client.ClientOptions));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullOptions_Throws()
        {
            var client = GetMockClient();
            Assert.Throws<ArgumentNullException>(() => new RemoteTable<IdEntity>("tables/movies", client.HttpClient, null));
        }

        [Fact]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_ThrowsOnNull()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.InsertItemAsync(null)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_Success_FormulatesCorrectRequest(HttpStatusCode statusCode)
        {
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            MockHandler.AddResponse(statusCode, payload);
            var client = GetMockClient();
            var sut = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var response = await sut.InsertItemAsync(payload).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            Assert.Equal(sJsonPayload, await request.Content.ReadAsStringAsync().ConfigureAwait(false));
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
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_Success_FormulatesCorrectRequest_WithAuth(HttpStatusCode statusCode)
        {
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            MockHandler.AddResponse(statusCode, payload);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var sut = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var response = await sut.InsertItemAsync(payload).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            Assert.Equal(sJsonPayload, await request.Content.ReadAsStringAsync().ConfigureAwait(false));
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
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_SuccessNoContent(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var sut = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var response = await sut.InsertItemAsync(payload).ConfigureAwait(false);
            Assert.Equal((int)statusCode, response.StatusCode);
            Assert.False(response.HasContent);
            Assert.Empty(response.Content);
            Assert.False(response.HasValue);
            Assert.Null(response.Value);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_SuccessWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var payload = new IdEntity() { Id = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f", StringValue = "test" };

            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            MockHandler.Responses.Add(response);
            var client = GetMockClient();
            var sut = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            await Assert.ThrowsAsync<JsonException>(() => sut.InsertItemAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            MockHandler.AddResponse(statusCode, payload);
            var client = GetMockClient();
            var sut = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => sut.InsertItemAsync(payload)).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            Assert.Equal(sJsonPayload, request.Content.ReadAsStringAsync().Result);
            AssertEx.Equals("application/json", request.Content.Headers.ContentType.MediaType);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.Equal("test", ex.ServerItem.StringValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_ConflictNoContent_Throws(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var sut = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);
            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => sut.InsertItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.Empty(ex.Content);
            Assert.Null(ex.ServerItem);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            MockHandler.Responses.Add(response);
            var client = GetMockClient();
            var sut = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            await Assert.ThrowsAsync<JsonException>(() => sut.InsertItemAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var sut = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => sut.InsertItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ThrowsOnNull()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.DeleteItemAsync(null)).ConfigureAwait(false);
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
        public async Task DeleteItemAsync_ThrowsOnInvalidId(string id)
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            var item = new ClientMovie { Id = id };
            await Assert.ThrowsAsync<ArgumentException>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_FormulatesCorrectResponse(bool hasPrecondition)
        {
            var sId = Guid.NewGuid().ToString("N");
            var expectedEndpoint = new Uri(Endpoint, $"/tables/movies/{sId}").ToString();
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            var client = GetMockClient();
            var table = new RemoteTable<ClientMovie>("tables/movies", client.HttpClient, client.ClientOptions);
            var item = new ClientMovie { Id = sId, Version = hasPrecondition ? "etag" : null };

            var response = await table.DeleteItemAsync(item).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            if (hasPrecondition)
            {
                AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
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

        [Theory, CombinatorialData]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_FormulatesCorrectResponse_WithAuth(bool hasPrecondition)
        {
            var sId = Guid.NewGuid().ToString("N");
            var expectedEndpoint = new Uri(Endpoint, $"/tables/movies/{sId}").ToString();
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = new RemoteTable<ClientMovie>("tables/movies", client.HttpClient, client.ClientOptions);
            var item = new ClientMovie { Id = sId, Version = hasPrecondition ? "etag" : null };

            var response = await table.DeleteItemAsync(item).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            if (hasPrecondition)
            {
                AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
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
            var sId = Guid.NewGuid().ToString("N");
            MockHandler.AddResponse(statusCode, payload);
            var client = GetMockClient();
            var table = new RemoteTable<ClientMovie>("tables/movies", client.HttpClient, client.ClientOptions);
            var item = new ClientMovie { Id = sId };

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.Equal(Encoding.UTF8.GetBytes(sJsonPayload), ex.Content);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConflictNoContent_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            var sId = Guid.NewGuid().ToString("N");

            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);
            var item = new IdEntity { Id = sId };

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.Empty(ex.Content);
            Assert.Null(ex.ServerItem);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var sId = Guid.NewGuid().ToString("N");
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            MockHandler.Responses.Add(response);
            var client = GetMockClient();
            var table = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);
            var item = new IdEntity { Id = sId };

            await Assert.ThrowsAsync<JsonException>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            var sId = Guid.NewGuid().ToString("N");
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = new RemoteTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);
            var item = new IdEntity { Id = sId };

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

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
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");

            // Act
            var pageable = table.GetAsyncItems<ClientMovie>();
            var enumerator = pageable.GetAsyncEnumerator();

            // Assert
            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(async () => await enumerator.MoveNextAsync().ConfigureAwait(false)).ConfigureAwait(false);
            Assert.Equal(statusCode, ex.Response.StatusCode);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_NoItems()
        {
            // Arrange
            var expectedEndpoint = new Uri(Endpoint, "tables/movies/").ToString();
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            // Act
            var enumerator = table.GetAsyncItems<IdEntity>().GetAsyncEnumerator();
            var hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            Assert.False(hasMore);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_NoItems_WithAuth()
        {
            // Arrange
            var expectedEndpoint = new Uri(Endpoint, "tables/movies/").ToString();
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetRemoteTable<IdEntity>("movies");

            // Act
            var enumerator = table.GetAsyncItems<IdEntity>().GetAsyncEnumerator();
            var hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            Assert.False(hasMore);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_NoItems_WhenNullResponse()
        {
            var options = new DatasyncClientOptions();
            Task<ServiceResponse<Page<IdEntity>>> pageFunc(string _)
                => ServiceResponse.FromResponseAsync<Page<IdEntity>>(new HttpResponseMessage(HttpStatusCode.OK), options.DeserializerOptions);
            FuncAsyncPageable<IdEntity> pageable = new(pageFunc);

            var enumerator = pageable.AsPages().GetAsyncEnumerator();
            _ = await enumerator.MoveNextAsync().ConfigureAwait(false);

            Assert.Null(enumerator.Current.Items);
            Assert.Null(enumerator.Current.Count);
            Assert.Null(enumerator.Current.NextLink);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_OnePageOfItems_WhenItemsReturned()
        {
            // Arrange
            var expectedEndpoint = new Uri(Endpoint, "tables/movies/").ToString();
            var page = CreatePageOfItems(5);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");
            List<IdEntity> items = new();

            // Act
            await foreach (var item in table.GetAsyncItems<IdEntity>())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            // Assert - response
            Assert.Equal(page.Items, items);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_OnePageOfItems_WithAuth_WhenItemsReturned()
        {
            // Arrange
            var expectedEndpoint = new Uri(Endpoint, "tables/movies/").ToString();
            var page = CreatePageOfItems(5);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetRemoteTable<IdEntity>("movies");
            List<IdEntity> items = new();

            // Act
            await foreach (var item in table.GetAsyncItems<IdEntity>())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            // Assert - response
            Assert.Equal(page.Items, items);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_TwoPagesOfItems_WhenItemsReturned()
        {
            // Arrange
            var expectedEndpoint = new Uri(Endpoint, "tables/movies/").ToString();
            var page1 = CreatePageOfItems(5, null, new Uri($"{expectedEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");
            List<IdEntity> items = new();

            // Act
            await foreach (var item in table.GetAsyncItems<IdEntity>())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(2, MockHandler.Requests.Count);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{expectedEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            // Assert - response
            Assert.Equal(10, items.Count);
            Assert.Equal(page1.Items, items.Take(5));
            Assert.Equal(page2.Items, items.Skip(5).Take(5));
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_TwoPagesOfItems_WithAuth_WhenItemsReturned()
        {
            // Arrange
            var expectedEndpoint = new Uri(Endpoint, "tables/movies/").ToString();
            var page1 = CreatePageOfItems(5, null, new Uri($"{expectedEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetRemoteTable<IdEntity>("movies");
            List<IdEntity> items = new();

            // Act
            await foreach (var item in table.GetAsyncItems<IdEntity>())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(2, MockHandler.Requests.Count);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{expectedEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

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
            var expectedEndpoint = new Uri(Endpoint, "tables/movies/").ToString();
            var page1 = CreatePageOfItems(5, null, new Uri($"{expectedEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, null, new Uri($"{expectedEndpoint}?page=3"));
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");
            List<IdEntity> items = new();

            // Act
            await foreach (var item in table.GetAsyncItems<IdEntity>())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(3, MockHandler.Requests.Count);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{expectedEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            request = MockHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{expectedEndpoint}?page=3", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            // Assert - response
            Assert.Equal(10, items.Count);
            Assert.Equal(page1.Items, items.Take(5));
            Assert.Equal(page2.Items, items.Skip(5).Take(5));
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_ThreePagesOfItems_WithAuth_WhenItemsReturned()
        {
            // Arrange
            var expectedEndpoint = new Uri(Endpoint, "tables/movies/").ToString();
            var page1 = CreatePageOfItems(5, null, new Uri($"{expectedEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, null, new Uri($"{expectedEndpoint}?page=3"));
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetRemoteTable<IdEntity>("movies");
            List<IdEntity> items = new();

            // Act
            await foreach (var item in table.GetAsyncItems<IdEntity>())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(3, MockHandler.Requests.Count);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{expectedEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            request = MockHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{expectedEndpoint}?page=3", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

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
            var expectedEndpoint = new Uri(Endpoint, "tables/movies/").ToString();
            _ = CreatePageOfItems(5, 5);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            // Act
            var pageable = table.GetAsyncItems<IdEntity>();
            var enumerator = pageable.GetAsyncEnumerator();
            _ = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert - request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            // Assert - response
            Assert.Equal(5, pageable.Count);
            Assert.NotNull(pageable.CurrentResponse);
            Assert.Equal(200, pageable.CurrentResponse.StatusCode);
            Assert.True(pageable.CurrentResponse.HasContent);
            Assert.NotEmpty(pageable.CurrentResponse.Content);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_SetsCountAndResponse_WithAuth()
        {
            // Arrange
            _ = CreatePageOfItems(5, 5);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetRemoteTable<IdEntity>("movies");

            // Act
            var pageable = table.GetAsyncItems<IdEntity>();
            var enumerator = pageable.GetAsyncEnumerator();
            _ = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert - request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            // Assert - response
            Assert.Equal(5, pageable.Count);
            Assert.NotNull(pageable.CurrentResponse);
            Assert.Equal(200, pageable.CurrentResponse.StatusCode);
            Assert.True(pageable.CurrentResponse.HasContent);
            Assert.NotEmpty(pageable.CurrentResponse.Content);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_ThrowsOnNull()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.GetItemAsync(null)).ConfigureAwait(false);
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
        public async Task GetItemAsync_ThrowsOnInvalidId(string id)
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentException>(() => table.GetItemAsync(id)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_FormulatesCorrectRequest()
        {
            // Arrange
            var sId = Guid.NewGuid().ToString("N");
            var expectedEndpoint = new Uri(Endpoint, $"tables/movies/{sId}").ToString();

            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            // Act
            var response = await table.GetItemAsync(sId).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

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
        public async Task GetItemAsync_FormulatesCorrectRequest_WithAuth()
        {
            // Arrange
            var sId = Guid.NewGuid().ToString("N");
            var expectedEndpoint = new Uri(Endpoint, $"tables/movies/{sId}").ToString();

            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetRemoteTable<IdEntity>("movies");

            // Act
            var response =  await table.GetItemAsync(sId).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

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
        public async Task GetItemAsync_SuccessNoContent()
        {
            // Arrange
            var sId = Guid.NewGuid().ToString("N");
            var expectedEndpoint = new Uri(Endpoint, $"tables/movies/{sId}").ToString();

            MockHandler.AddResponse(HttpStatusCode.OK);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            // Act
            var response = await table.GetItemAsync(sId).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            Assert.False(request.Headers.Contains("If-None-Match"));

            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.False(response.HasContent);
            Assert.Empty(response.Content);
            Assert.False(response.HasValue);
            Assert.Null(response.Value);
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
            var sId = Guid.NewGuid().ToString("N");

            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetItemAsync(sId)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_NotModified_Throws()
        {
            var sId = Guid.NewGuid().ToString("N");

            MockHandler.AddResponse(HttpStatusCode.NotModified);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<EntityNotModifiedException>(() => table.GetItemAsync(sId)).ConfigureAwait(false);
            Assert.Equal(304, ex.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("$filter=Year eq 1900&$count=true")]
        [Trait("Method", "GetNextPageAsync")]
        public async Task GetPageOfItems_ConstructsRequest_WithQuery(string query)
        {
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            Page<IdEntity> result = new();
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
            var expectedUri = string.IsNullOrEmpty(query) ? sEndpoint : $"{sEndpoint}?{query}";

            _ = await table.GetNextPageAsync<IdEntity>(query).ConfigureAwait(false);

            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedUri, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("$filter=Year eq 1900&$count=true")]
        [Trait("Method", "GetNextPageAsync")]
        public async Task GetPageOfItems_ConstructsRequest_WithQueryAndAuth(string query)
        {
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            Page<IdEntity> result = new();
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
            var expectedUri = string.IsNullOrEmpty(query) ? sEndpoint : $"{sEndpoint}?{query}";

            _ = await table.GetNextPageAsync<IdEntity>(query).ConfigureAwait(false);

            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedUri, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
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
            Page<IdEntity> result = new();
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;

            // Act
            _ = await table.GetNextPageAsync<IdEntity>("", requestUri).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(requestUri, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
        }

        [Theory]
        [InlineData("https://localhost/tables/foo/?$count=true")]
        [InlineData("https://localhost/tables/foo/?$count=true&$skip=5&$top=10")]
        [InlineData("https://localhost/tables/foo/?$count=true&$skip=5&$top=10&__includedeleted=true")]
        [Trait("Method", "GetNextPageAsync")]
        public async Task GetPageOfItems_ConstructsRequest_WithRequestUriAndAuth(string requestUri)
        {
            // Arrange
            Page<IdEntity> result = new();
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;

            // Act
            _ = await table.GetNextPageAsync<IdEntity>("", requestUri).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(requestUri, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
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
            Page<IdEntity> result = new();
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
            const string requestUri = "https://localhost/tables/foo?$count=true&$skip=5&$top=10&__includedeleted=true";

            // Act
            _ = await table.GetNextPageAsync<IdEntity>(query, requestUri).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(requestUri, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("$filter=Year eq 1900&$count=true")]
        [Trait("Method", "GetNextPageAsync")]
        public async Task GetPageOfItems_ConstructsRequest_PrefersRequestUri_WithAuth(string query)
        {
            // Arrange
            Page<IdEntity> result = new();
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
            const string requestUri = "https://localhost/tables/foo?$count=true&$skip=5&$top=10&__includedeleted=true";

            // Act
            _ = await table.GetNextPageAsync<IdEntity>(query, requestUri).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(requestUri, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        }

        [Fact]
        [Trait("Method", "GetNextPageAsync")]
        public async Task GetPageOfItems_ReturnsItems()
        {
            // Arrange
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } } };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;

            // Act
            var response = await table.GetNextPageAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<ServiceResponse<Page<IdEntity>>>(response);
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
        [Trait("Method", "GetNextPageAsync")]
        public async Task GetPageOfItems_ReturnsCount()
        {
            // Arrange
            Page<IdEntity> result = new() { Count = 42 };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;

            // Act
            var response = await table.GetNextPageAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<ServiceResponse<Page<IdEntity>>>(response);
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
        [Trait("Method", "GetNextPageAsync")]
        public async Task GetPageOfItems_ReturnsNextLink()
        {
            // Arrange
            var nextLink = Endpoint.ToString() + "?$top=5&$skip=5";
            Page<IdEntity> result = new() { NextLink = new Uri(nextLink) };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;

            // Act
            var response = await table.GetNextPageAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<ServiceResponse<Page<IdEntity>>>(response);
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
        [Trait("Method", "GetNextPageAsync")]
        public async Task GetPageOfItems_ReturnsItemsAndCount()
        {
            // Arrange
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } }, Count = 42 };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;

            // Act
            var response = await table.GetNextPageAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<ServiceResponse<Page<IdEntity>>>(response);
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
        [Trait("Method", "GetNextPageAsync")]
        public async Task GetPageOfItems_ReturnsItemsAndNextLink()
        {
            // Arrange
            var nextLink = Endpoint.ToString() + "?$top=5&$skip=5";
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } }, NextLink = new Uri(nextLink) };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;

            // Act
            var response = await table.GetNextPageAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<ServiceResponse<Page<IdEntity>>>(response);
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
        [Trait("Method", "GetNextPageAsync")]
        public async Task GetPageOfItems_ReturnsItem_Count_NextLink()
        {
            // Arrange
            var nextLink = Endpoint.ToString() + "?$top=5&$skip=5";
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } }, Count = 42, NextLink = new Uri(nextLink) };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;

            // Act
            var response = await table.GetNextPageAsync<IdEntity>(null).ConfigureAwait(false);

            // Assert
            Assert.IsAssignableFrom<ServiceResponse<Page<IdEntity>>>(response);
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
        [Trait("Method", "GetNextPageAsync")]
        public async Task GetPageOfItemAsync_BadResponse_ThrowsRequestFailed(HttpStatusCode statusCode)
        {
            // Arrange
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetNextPageAsync<IdEntity>(null)).ConfigureAwait(false);

            // Assert
            Assert.Equal((int)statusCode, exception.StatusCode);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ThrowsOnNull()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.ReplaceItemAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ThrowsOnNullId()
        {
            var obj = new ClientMovie { Id = null };
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(obj)).ConfigureAwait(false);
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
        public async Task ReplaceItemAsync_ThrowsOnInvalidId(string id)
        {
            var obj = new ClientMovie { Id = id };
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(obj)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Success_FormulatesCorrectResponse(bool hasPrecondition)
        {
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");
            payload.Version = hasPrecondition ? "etag" : null;

            var response = await table.ReplaceItemAsync(payload).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal($"{sEndpoint}{payload.Id}", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            if (hasPrecondition)
            {
                AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
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

        [Theory, CombinatorialData]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Success_FormulatesCorrectResponse_WithAuth(bool hasPrecondition)
        {
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetRemoteTable<IdEntity>("movies");
            payload.Version = hasPrecondition ? "etag" : null;

            var response = await table.ReplaceItemAsync(payload).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal($"{sEndpoint}{payload.Id}", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            if (hasPrecondition)
            {
                AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
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
        public async Task ReplaceItemAsync_SuccessNoContent()
        {
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            MockHandler.AddResponse(HttpStatusCode.OK);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            var response = await table.ReplaceItemAsync(payload).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal($"{sEndpoint}{payload.Id}", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            // Check Response
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.False(response.HasContent);
            Assert.Empty(response.Content);
            Assert.False(response.HasValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode, payload);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.ReplaceItemAsync(payload)).ConfigureAwait(false);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.NotEmpty(ex.Content);
            Assert.Equal("test", ex.ServerItem.StringValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ConflictNoContent(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.ReplaceItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.Empty(ex.Content);
            Assert.Null(ex.ServerItem);
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
            MockHandler.Responses.Add(response);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            await Assert.ThrowsAsync<JsonException>(() => table.ReplaceItemAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ThrowsOnNullId()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            var updates = new Dictionary<string, object>()
            {
                { "Title", "Replacement Title" },
                { "Rating", "PG-13" }
            };
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.UpdateItemAsync(null, updates)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ThrowsOnNullUpdates()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.UpdateItemAsync("id-107", null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ThrowsOnEmptyUpdates()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            var updates = new Dictionary<string, object>();
            await Assert.ThrowsAsync<ArgumentException>(() => table.UpdateItemAsync("id-107", updates)).ConfigureAwait(false);
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
        public async Task UpdateItemAsync_ThrowsOnInvalidId(string id)
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>("movies");
            var updates = new Dictionary<string, object>()
            {
                { "Title", "Replacement Title" },
                { "Rating", "PG-13" }
            };
            await Assert.ThrowsAsync<ArgumentException>(() => table.UpdateItemAsync(id, updates)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_Success_FormulatesCorrectResponse(bool hasPrecondition)
        {
            var sId = Guid.NewGuid().ToString("N");
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            var response = hasPrecondition
                ? await table.UpdateItemAsync(sId, changes, IfMatch.Version("etag")).ConfigureAwait(false)
                : await table.UpdateItemAsync(sId, changes).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal("patch", request.Method.ToString().ToLower());
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION","3.0.0");
            if (hasPrecondition)
            {
                AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
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

        [Theory, CombinatorialData]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_Success_FormulatesCorrectResponse_WithAuth(bool hasPrecondition)
        {
            var sId = Guid.NewGuid().ToString("N");
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetRemoteTable<IdEntity>("movies");

            var response = hasPrecondition
                ? await table.UpdateItemAsync(sId, changes, IfMatch.Version("etag")).ConfigureAwait(false)
                : await table.UpdateItemAsync(sId, changes).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal("patch", request.Method.ToString().ToLower());
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            if (hasPrecondition)
            {
                AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
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
            var sId = Guid.NewGuid().ToString("N");
            MockHandler.AddResponse(statusCode, payload);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.UpdateItemAsync(sId, changes)).ConfigureAwait(false);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.NotEmpty(ex.Content);
            Assert.Equal("test", ex.ServerItem.StringValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ConflictNoContent_throws(HttpStatusCode statusCode)
        {
            var sId = Guid.NewGuid().ToString("N");
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.UpdateItemAsync(sId, changes)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.Empty(ex.Content);
            Assert.Null(ex.ServerItem);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var sId = Guid.NewGuid().ToString("N");
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            MockHandler.Responses.Add(response);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            await Assert.ThrowsAsync<JsonException>(() => table.UpdateItemAsync(sId, changes)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            var sId = Guid.NewGuid().ToString("N");
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.UpdateItemAsync(sId, changes)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_SuccessNoContent_Throws()
        {
            var sId = Guid.NewGuid().ToString("N");
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            MockHandler.AddResponse(HttpStatusCode.OK);
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>("movies");

            var response = await table.UpdateItemAsync(sId, changes).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal("patch", request.Method.ToString().ToLower());
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            // Check Response
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.False(response.HasContent);
            Assert.Empty(response.Content);
            Assert.False(response.HasValue);
            Assert.Null(response.Value);
        }

        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Enabled_AddsKey()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.IncludeDeletedItems(true) as TableQuery<IdEntity>;

            AssertEx.Contains("__includedeleted", "true", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Disabled_Empty()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.IncludeDeletedItems(false) as TableQuery<IdEntity>;
            Assert.Empty(query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "IncludeDeletedItems")]
        public void ToQueryString_IncludeDeletedItems_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.IncludeDeletedItems() as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("__includedeleted=true", odata);
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Enabled_AddsKey()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.IncludeTotalCount(true) as TableQuery<IdEntity>;
            AssertEx.Contains("$count", "true", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Disabled_WorksWithEmptyParameters()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.IncludeTotalCount(false) as TableQuery<IdEntity>;
            Assert.False(query.QueryParameters.ContainsKey("$count"));
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "IncludeTotalCount")]
        public void ToQueryString_IncludeTotalCount_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.IncludeTotalCount() as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("$count=true", odata);
        }

        [Fact]
        [Trait("Method", "OrderBy")]
        public void OrderBy_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => table.OrderBy(keySelector));
        }

        [Fact]
        [Trait("Method", "OrderBy")]
        public void OrderBy_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.OrderBy(m => m.Id) as TableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("OrderBy", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "OrderBy")]
        public void ToQueryString_OrderBy_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.OrderBy(m => m.Id) as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("$orderby=id", odata);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "OrderBy")]
        public void ToQueryString_OrderBy_ThrowsNotSupported()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.OrderBy(m => m.Id.ToLower()) as TableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToQueryString());
        }

        [Fact]
        [Trait("Method", "OrderByDescending")]
        public void OrderByDescending_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => table.OrderByDescending(keySelector));
        }

        [Fact]
        [Trait("Method", "OrderByDescending")]
        public void OrderByDescending_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.OrderByDescending(m => m.Id) as TableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("OrderByDescending", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "OrderByDescending")]
        public void ToQueryString_OrderByDescending_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.OrderByDescending(m => m.Id) as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("$orderby=id desc", odata);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "OrderByDescending")]
        public void ToQueryString_OrderByDescending_ThrowsNotSupported()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.OrderByDescending(m => m.Id.ToLower()) as TableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToQueryString());
        }

        [Fact]
        [Trait("Method", "Select")]
        public void Select_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            Expression<Func<IdEntity, IdOnly>> selector = null;
            Assert.Throws<ArgumentNullException>(() => table.Select(selector));
        }

        [Fact]
        [Trait("Method", "Select")]
        public void Select_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.Select(m => new IdOnly { Id = m.Id }) as TableQuery<IdOnly>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("Select", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "Select")]
        public void ToQueryString_Select_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.Select(m => new IdOnly { Id = m.Id }) as TableQuery<IdOnly>;
            var odata = query.ToQueryString();
            Assert.Equal("$select=id", odata);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "Skip")]
        public void Skip_Throws_OutOfRange([CombinatorialValues(-10, -1)] int skip)
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            Assert.Throws<ArgumentOutOfRangeException>(() => table.Skip(skip));
        }

        [Fact]
        [Trait("Method", "Skip")]
        public void Skip_Sets_SkipCount()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.Skip(5) as TableQuery<IdEntity>;
            Assert.Equal(5, query.SkipCount);
        }

        [Fact]
        [Trait("Method", "Skip")]
        public void Skip_IsCumulative()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.Skip(5).Skip(20) as TableQuery<IdEntity>;
            Assert.Equal(25, query.SkipCount);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "Skip")]
        public void ToQueryString_Skip_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.Skip(5) as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("$skip=5", odata);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "Take")]
        public void Take_ThrowsOutOfRange([CombinatorialValues(-10, -1, 0)] int take)
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            Assert.Throws<ArgumentOutOfRangeException>(() => table.Take(take));
        }

        [Theory]
        [InlineData(5, 2, 2)]
        [InlineData(2, 5, 2)]
        [InlineData(5, 20, 5)]
        [InlineData(20, 5, 5)]
        [Trait("Method", "Take")]
        public void Take_MinimumWins(int first, int second, int expected)
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.Take(first).Take(second) as TableQuery<IdEntity>;
            Assert.Equal(expected, query.TakeCount);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "Take")]
        public void ToQueryString_Take_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.Take(5) as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("$top=5", odata);
        }

        [Fact]
        [Trait("Method", "ThenBy")]
        public void ThenBy_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => table.ThenBy(keySelector));
        }

        [Fact]
        [Trait("Method", "ThenBy")]
        public void ThenBy_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.ThenBy(m => m.Id) as TableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("ThenBy", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "ThenBy")]
        public void ToQueryString_ThenBy_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.ThenBy(m => m.Id) as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("$orderby=id", odata);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "ThenBy")]
        public void ToQueryString_ThenBy_ThrowsNotSupported()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.ThenBy(m => m.Id.ToLower()) as TableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToQueryString());
        }

        [Fact]
        [Trait("Method", "ThenByDescending")]
        public void ThenByDescending_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => table.ThenByDescending(keySelector));
        }

        [Fact]
        [Trait("Method", "ThenByDescending")]
        public void ThenByDescending_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.ThenByDescending(m => m.Id) as TableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("ThenByDescending", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "ThenByDescending")]
        public void ToQueryString_ThenByDescending_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.ThenByDescending(m => m.Id) as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("$orderby=id desc", odata);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "ThenByDescending")]
        public void ToQueryString_ThenByDescending_ThrowsNotSupported()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.ThenByDescending(m => m.Id.ToLower()) as TableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToQueryString());
        }

        [Fact]
        [Trait("Method", "Where")]
        public void Where_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();
            Expression<Func<IdEntity, bool>> predicate = null;
            Assert.Throws<ArgumentNullException>(() => table.Where(predicate));
        }

        [Fact]
        [Trait("Method", "Where")]
        public void Where_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.Where(m => m.Id.Contains("foo")) as TableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("Where", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "Where")]
        public void ToQueryString_Where_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.Where(m => m.Id == "foo") as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("$filter=(id%20eq%20'foo')", odata);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "Where")]
        public void ToQueryString_Where_ThrowsNotSupported()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.ThenByDescending(m => m.Id.Normalize() == "foo") as TableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToQueryString());
        }

        [Theory]
        [InlineData(null, "test")]
        [InlineData("test", null)]
        [Trait("Method", "WithParameter")]
        public void WithParameter_Null_Throws(string key, string value)
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            Assert.Throws<ArgumentNullException>(() => table.WithParameter(key, value));
        }

        [Theory]
        [InlineData("testkey", "")]
        [InlineData("testkey", " ")]
        [InlineData("testkey", "   ")]
        [InlineData("testkey", "\t")]
        [InlineData("", "testvalue")]
        [InlineData(" ", "testvalue")]
        [InlineData("   ", "testvalue")]
        [InlineData("\t", "testvalue")]
        [InlineData("$count", "true")]
        [InlineData("__includedeleted", "true")]
        [Trait("Method", "WithParameter")]
        public void WithParameter_Illegal_Throws(string key, string value)
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            Assert.Throws<ArgumentException>(() => table.WithParameter(key, value));
        }

        [Fact]
        [Trait("Method", "WithParameter")]
        public void WithParameter_SetsParameter()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.WithParameter("testkey", "testvalue") as TableQuery<IdEntity>;
            AssertEx.Contains("testkey", "testvalue", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "WithParameter")]
        public void WithParameter_Overwrites()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.WithParameter("testkey", "testvalue");
            var actual = query.WithParameter("testkey", "replacement") as TableQuery<IdEntity>;
            AssertEx.Contains("testkey", "replacement", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "WithParameter")]
        public void ToQueryString_WithParameter_isWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.WithParameter("testkey", "testvalue") as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("testkey=testvalue", odata);
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "WithParameter")]
        public void ToQueryString_WithParameter_EncodesValue()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.WithParameter("testkey", "test value") as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("testkey=test%20value", odata);
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            Assert.Throws<ArgumentNullException>(() => table.WithParameters(null));
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_Empty_Throws()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();
            var sut = new Dictionary<string, string>();
            Assert.Throws<ArgumentException>(() => table.WithParameters(sut));
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_CopiesParams()
        {
            var sut = new Dictionary<string, string>()
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.WithParameters(sut) as TableQuery<IdEntity>;
            AssertEx.Contains("key1", "value1", query.QueryParameters);
            AssertEx.Contains("key2", "value2", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_MergesParams()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.WithParameter("key1", "value1");
            var sut = new Dictionary<string, string>()
            {
                { "key1", "replacement" },
                { "key2", "value2" }
            };

            var actual = query.WithParameters(sut) as TableQuery<IdEntity>;
            AssertEx.Contains("key1", "replacement", actual.QueryParameters);
            AssertEx.Contains("key2", "value2", actual.QueryParameters);
        }

        [Theory]
        [InlineData("$count")]
        [InlineData("__includedeleted")]
        [Trait("Method", "WithParameters")]
        public void WithParameters_CannotSetIllegalParams(string key)
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var sut = new Dictionary<string, string>()
            {
                { key, "true" },
                { "key2", "value2" }
            };

            Assert.Throws<ArgumentException>(() => table.WithParameters(sut));
        }

        [Fact]
        [Trait("Method", "ToQueryString")]
        [Trait("Method", "WithParameters")]
        public void ToQueryString_WithParameters_isWellFormed()
        {
            var pairs = new Dictionary<string, string>()
            {
                {  "key1", "value1" },
                {  "key2", "value 2" }
            };
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();

            var query = table.WithParameters(pairs) as TableQuery<IdEntity>;
            var odata = query.ToQueryString();
            Assert.Equal("key1=value1&key2=value%202", odata);
        }
    }
}

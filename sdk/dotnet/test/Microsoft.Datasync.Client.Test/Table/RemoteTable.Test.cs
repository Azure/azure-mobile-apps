// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Table;
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
    [ExcludeFromCodeCoverage]
    public class RemoteTable_Tests : BaseTest
    {
        private readonly IdEntity payload = new() { Id = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f", StringValue = "test" };
        private const string sJsonPayload = "{\"id\":\"db0ec08d-46a9-465d-9f5e-0066a3ee5b5f\",\"stringValue\":\"test\"}";
        private const string sBadJson = "{this-is-bad-json";

        private readonly IdOnly idOnly;
        private readonly IdEntity idEntity;
        private readonly IRemoteTable table, authTable;
        private readonly string sId, expectedEndpoint, tableEndpoint;

        // Default updates for the UpdateItemAsync methods
        private readonly IReadOnlyDictionary<string, object> updates = new Dictionary<string, object>()
        {
            { "stringValue", "test" }
        };
        private readonly string sJsonUpdates = "{\"stringValue\":\"test\"}";

        public RemoteTable_Tests()
        {
            sId = Guid.NewGuid().ToString("N");
            idOnly = new IdOnly { Id = sId };
            idEntity = new IdEntity { Id = sId, Version = "etag" };
            table = GetMockClient().GetRemoteTable("movies");
            authTable = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken)).GetRemoteTable("movies");
            expectedEndpoint = new Uri(Endpoint, $"/tables/movies/{sId}").ToString();
            tableEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_SetsInternals()
        {
            var client = GetMockClient();
            var sut = new RemoteTable("tables/movies", client.HttpClient, client.ClientOptions);

            Assert.Equal(tableEndpoint, sut.Endpoint.ToString());
            Assert.Same(client.ClientOptions, sut.ClientOptions);
            Assert.Same(client.HttpClient, sut.HttpClient);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Client_SetsInternals()
        {
            var client = GetMockClient();
            var sut = client.GetRemoteTable("/tables/movies") as RemoteTable;

            Assert.Equal(tableEndpoint, sut.Endpoint.ToString());
            Assert.Same(client.ClientOptions, sut.ClientOptions);
            Assert.Same(client.HttpClient, sut.HttpClient);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullEndpoint_Throws()
        {
            var client = GetMockClient();
            const string relativeUri = null;
            Assert.Throws<ArgumentNullException>(() => new RemoteTable(relativeUri, client.HttpClient, client.ClientOptions));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullClient_Throws()
        {
            var client = GetMockClient();
            Assert.Throws<ArgumentNullException>(() => new RemoteTable("tables/movies", null, client.ClientOptions));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullOptions_Throws()
        {
            var client = GetMockClient();
            Assert.Throws<ArgumentNullException>(() => new RemoteTable("tables/movies", client.HttpClient, null));
        }

        #region DeleteItemAsync
        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ThrowsOnNull()
        {
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
            var json = CreateJsonDocument(new IdOnly { Id = id });
            await Assert.ThrowsAsync<ArgumentException>(() => table.DeleteItemAsync(json)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_FormsCorrectResponse_NoPrecondition()
        {
            var json = CreateJsonDocument(idOnly);
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            var response = await table.DeleteItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            Assert.False(request.Headers.Contains("If-Match"));

            // Check Response
            Assert.Equal(204, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.False(response.HasContent);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_FormsCorrectResponse_WithPrecondition()
        {
            var json = CreateJsonDocument(idEntity);
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            var response = await table.DeleteItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");

            // Check Response
            Assert.Equal(204, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.False(response.HasContent);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Auth_NoPrecondition()
        {
            var json = CreateJsonDocument(idOnly);
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            var response = await authTable.DeleteItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            Assert.False(request.Headers.Contains("If-Match"));

            // Check Response
            Assert.Equal(204, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.False(response.HasContent);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Auth_WithPrecondition()
        {
            var json = CreateJsonDocument(idEntity);
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            var response = await authTable.DeleteItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");

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
            var json = CreateJsonDocument(idEntity);
            MockHandler.AddResponse(statusCode, payload);
            var ex = await Assert.ThrowsAsync<DatasyncConflictException<JsonDocument>>(() => table.DeleteItemAsync(json)).ConfigureAwait(false);

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
            var json = CreateJsonDocument(idEntity);
            MockHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<DatasyncConflictException<JsonDocument>>(() => table.DeleteItemAsync(json)).ConfigureAwait(false);

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
            var json = CreateJsonDocument(idOnly);
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            MockHandler.Responses.Add(response);
            await Assert.ThrowsAsync<JsonException>(() => table.DeleteItemAsync(json)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            var json = CreateJsonDocument(idOnly);
            MockHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(json)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_TriggersEventHandler()
        {
            var json = CreateJsonDocument(idOnly);
            var events = new List<TableModifiedEventArgs>();
            table.TableModified += (sender, e) => events.Add(e);
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            var response = await table.DeleteItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(events);
            Assert.Equal(sId, events[0].Id);
            Assert.Equal(tableEndpoint, events[0].TableEndpoint.ToString());
            Assert.Equal(TableModifiedEventArgs.TableOperation.Delete, events[0].Operation);
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
        public async Task GetAsyncItems_Throws_OnBadRequest(HttpStatusCode statusCode)
        {
            // Arrange
            MockHandler.AddResponse(statusCode);

            // Act
            var enumerator = table.GetAsyncItems().GetAsyncEnumerator();

            // Assert
            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(async () => await enumerator.MoveNextAsync().ConfigureAwait(false)).ConfigureAwait(false);
            Assert.Equal(statusCode, ex.Response.StatusCode);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_NoItems()
        {
            // Arrange
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

            // Act
            var enumerator = table.GetAsyncItems().GetAsyncEnumerator();
            var hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            Assert.False(hasMore);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_NoItems_WithAuth()
        {
            // Arrange
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

            // Act
            var enumerator = authTable.GetAsyncItems().GetAsyncEnumerator();
            var hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            Assert.False(hasMore);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_NoItems_WhenNullResponse()
        {
            var options = new DatasyncClientOptions();
            Task<ServiceResponse<Page<JsonDocument>>> pageFunc(string _)
                => ServiceResponse.FromResponseAsync<Page<JsonDocument>>(new HttpResponseMessage(HttpStatusCode.OK), options.DeserializerOptions);
            FuncAsyncPageable<JsonDocument> pageable = new(pageFunc);

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
            var page = CreatePageOfJsonItems(5);


            // Act
            List<JsonDocument> items = new();
            await foreach (var item in table.GetAsyncItems())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            // Assert - response
            AssertEx.JsonEqual(page.Items.ToList(), items);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_OnePageOfItems_WithAuth_WhenItemsReturned()
        {
            // Arrange
            var page = CreatePageOfJsonItems(5);

            // Act
            List<JsonDocument> items = new();
            await foreach (var item in authTable.GetAsyncItems())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            // Assert - response
            AssertEx.JsonEqual(page.Items.ToList(), items);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_TwoPagesOfItems_WhenItemsReturned()
        {
            // Arrange
            var page1 = CreatePageOfJsonItems(5, null, new Uri($"{tableEndpoint}?page=2"));
            var page2 = CreatePageOfJsonItems(5);

            // Act
            List<JsonDocument> items = new();
            await foreach (var item in table.GetAsyncItems())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(2, MockHandler.Requests.Count);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{tableEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            // Assert - response
            Assert.Equal(10, items.Count);
            AssertEx.JsonEqual(page1.Items.ToList(), items.Take(5).ToList());
            AssertEx.JsonEqual(page2.Items.ToList(), items.Skip(5).Take(5).ToList());
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_TwoPagesOfItems_WithAuth_WhenItemsReturned()
        {
            // Arrange
            var page1 = CreatePageOfJsonItems(5, null, new Uri($"{tableEndpoint}?page=2"));
            var page2 = CreatePageOfJsonItems(5);

            // Act
            List<JsonDocument> items = new();
            await foreach (var item in authTable.GetAsyncItems())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(2, MockHandler.Requests.Count);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{tableEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            // Assert - response
            Assert.Equal(10, items.Count);
            AssertEx.JsonEqual(page1.Items.ToList(), items.Take(5).ToList());
            AssertEx.JsonEqual(page2.Items.ToList(), items.Skip(5).Take(5).ToList());
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_ThreePagesOfItems_WhenItemsReturned()
        {
            // Arrange
            var page1 = CreatePageOfJsonItems(5, null, new Uri($"{tableEndpoint}?page=2"));
            var page2 = CreatePageOfJsonItems(5, null, new Uri($"{tableEndpoint}?page=3"));
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<JsonDocument>());

            // Act
            List<JsonDocument> items = new();
            await foreach (var item in table.GetAsyncItems())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(3, MockHandler.Requests.Count);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{tableEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            request = MockHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{tableEndpoint}?page=3", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            // Assert - response
            Assert.Equal(10, items.Count);
            AssertEx.JsonEqual(page1.Items.ToList(), items.Take(5).ToList());
            AssertEx.JsonEqual(page2.Items.ToList(), items.Skip(5).Take(5).ToList());
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_ThreePagesOfItems_WithAuth_WhenItemsReturned()
        {
            // Arrange
            var page1 = CreatePageOfJsonItems(5, null, new Uri($"{tableEndpoint}?page=2"));
            var page2 = CreatePageOfJsonItems(5, null, new Uri($"{tableEndpoint}?page=3"));
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<JsonDocument>());

            // Act
            List<JsonDocument> items = new();
            await foreach (var item in authTable.GetAsyncItems())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(3, MockHandler.Requests.Count);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{tableEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            request = MockHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{tableEndpoint}?page=3", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            // Assert - response
            Assert.Equal(10, items.Count);
            AssertEx.JsonEqual(page1.Items.ToList(), items.Take(5).ToList());
            AssertEx.JsonEqual(page2.Items.ToList(), items.Skip(5).Take(5).ToList());
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_SetsCountAndResponse()
        {
            // Arrange
            _ = CreatePageOfJsonItems(5, 5);

            // Act
            var pageable = table.GetAsyncItems();
            var enumerator = pageable.GetAsyncEnumerator();
            _ = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert - request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
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

            // Act
            var pageable = authTable.GetAsyncItems();
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
        #endregion

        #region GetItemAsync
        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_ThrowsOnNull()
        {
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
            await Assert.ThrowsAsync<ArgumentException>(() => table.GetItemAsync(id)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_FormulatesCorrectRequest()
        {
            // Arrange
            MockHandler.AddResponse(HttpStatusCode.OK, payload);

            // Act
            var response =  await table.GetItemAsync(sId).ConfigureAwait(false);

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
            Assert.True(response.HasContent);
            Assert.Equal(sJsonPayload, Encoding.UTF8.GetString(response.Content));
            Assert.True(response.HasValue);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_FormulatesCorrectRequest_WithAuth()
        {
            // Arrange
            MockHandler.AddResponse(HttpStatusCode.OK, payload);

            // Act
            var response = await authTable.GetItemAsync(sId).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            Assert.False(request.Headers.Contains("If-None-Match"));

            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            Assert.Equal(sJsonPayload, Encoding.UTF8.GetString(response.Content));
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_SuccessNoContent()
        {
            // Arrange
            MockHandler.AddResponse(HttpStatusCode.OK);

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
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetItemAsync(sId)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }
        #endregion

        #region InsertItemAsync
        [Fact]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_ThrowsOnNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.InsertItemAsync(null)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_Success_FormulatesCorrectRequest(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode, payload);
            var jsonDocument = CreateJsonDocument(idEntity);
            var json = JsonSerializer.Serialize(jsonDocument, new DatasyncClientOptions().SerializerOptions);

            var response = await table.InsertItemAsync(jsonDocument).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            Assert.Equal(json, await request.Content.ReadAsStringAsync().ConfigureAwait(false));
            Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);

            // Check Response
            Assert.Equal((int)statusCode, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            AssertEx.JsonEqual(sJsonPayload, response.Content);
            Assert.True(response.HasValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_Success_FormulatesCorrectRequest_WithAuth(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode, payload);
            var jsonDocument = CreateJsonDocument(idEntity);
            var json = JsonSerializer.Serialize(jsonDocument, new DatasyncClientOptions().SerializerOptions);
            var response = await authTable.InsertItemAsync(jsonDocument).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            Assert.Equal(json, await request.Content.ReadAsStringAsync().ConfigureAwait(false));
            Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);

            // Check Response
            Assert.Equal((int)statusCode, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            AssertEx.JsonEqual(sJsonPayload, response.Content);
            Assert.True(response.HasValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_SuccessNoContent(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var json = CreateJsonDocument(idEntity);
            var response = await table.InsertItemAsync(json).ConfigureAwait(false);

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
            var json = CreateJsonDocument(idEntity);
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            MockHandler.Responses.Add(response);

            await Assert.ThrowsAsync<JsonException>(() => table.InsertItemAsync(json)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            var jsonDocument = CreateJsonDocument(idEntity);
            var json = JsonSerializer.Serialize(jsonDocument, new DatasyncClientOptions().SerializerOptions);
            MockHandler.AddResponse(statusCode, payload);

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<JsonDocument>>(() => table.InsertItemAsync(jsonDocument)).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(tableEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            Assert.Equal(json, request.Content.ReadAsStringAsync().Result);
            Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_ConflictNoContent_Throws(HttpStatusCode statusCode)
        {
            var json = CreateJsonDocument(idEntity);
            MockHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<DatasyncConflictException<JsonDocument>>(() => table.InsertItemAsync(json)).ConfigureAwait(false);
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
            var json = CreateJsonDocument(idEntity);
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            MockHandler.Responses.Add(response);

            await Assert.ThrowsAsync<JsonException>(() => table.InsertItemAsync(json)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            var json = CreateJsonDocument(idEntity);
            MockHandler.AddResponse(statusCode);

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.InsertItemAsync(json)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_TriggersEventHandler()
        {
            MockHandler.AddResponse(HttpStatusCode.Created, payload);
            var events = new List<TableModifiedEventArgs>();
            table.TableModified += (sender, e) => events.Add(e);
            var json = CreateJsonDocument(idEntity);

            _ = await table.InsertItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(events);
            Assert.Equal(payload.Id, events[0].Id);
            Assert.NotNull(events[0].Entity);
            Assert.Equal(tableEndpoint, events[0].TableEndpoint.ToString());
            Assert.Equal(TableModifiedEventArgs.TableOperation.Create, events[0].Operation);
        }
        #endregion

        #region ReplaceItemAsync
        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ThrowsOnNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.ReplaceItemAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ThrowsOnNullId()
        {
            var json = CreateJsonDocument(new IdEntity { Id = null });
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);
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
        public async Task ReplaceItemAsync_ThrowsOnInvalidId(string id)
        {
            var json = CreateJsonDocument(new IdEntity { Id = id });
            await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_FormulatesCorrectResponse_NoPrecondition()
        {
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var json = CreateJsonDocument(idOnly);
            var response = await table.ReplaceItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            Assert.False(request.Headers.Contains("If-Match"));

            // Check Response
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            AssertEx.JsonEqual(sJsonPayload, response.Content);
            Assert.True(response.HasValue);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_FormulatesCorrectResponse_WithPrecondition()
        {
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var json = CreateJsonDocument(idEntity);

            var response = await table.ReplaceItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");

            // Check Response
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            AssertEx.JsonEqual(sJsonPayload, response.Content);
            Assert.True(response.HasValue);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Auth_FormulatesCorrectResponse_NoPrecondition()
        {
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var json = CreateJsonDocument(idOnly);
            var response = await authTable.ReplaceItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            Assert.False(request.Headers.Contains("If-Match"));

            // Check Response
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            AssertEx.JsonEqual(sJsonPayload, response.Content);
            Assert.True(response.HasValue);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Auth_FormulatesCorrectResponse_WithPrecondition()
        {
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var json = CreateJsonDocument(idEntity);

            var response = await authTable.ReplaceItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");

            // Check Response
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            AssertEx.JsonEqual(sJsonPayload, response.Content);
            Assert.True(response.HasValue);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_SuccessNoContent()
        {
            MockHandler.AddResponse(HttpStatusCode.OK);
            var json = CreateJsonDocument(idEntity);
            var response = await table.ReplaceItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
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
            var json = CreateJsonDocument(idEntity);
            var ex = await Assert.ThrowsAsync<DatasyncConflictException<JsonDocument>>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.NotEmpty(ex.Content);
            AssertEx.JsonEqual(sJsonPayload, ex.Content);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ConflictNoContent(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var json = CreateJsonDocument(idEntity);
            var ex = await Assert.ThrowsAsync<DatasyncConflictException<JsonDocument>>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);

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
            var json = CreateJsonDocument(idEntity);

            await Assert.ThrowsAsync<JsonException>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);
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
            var json = CreateJsonDocument(idEntity);

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_TriggersEventHandler()
        {
            MockHandler.AddResponse(HttpStatusCode.Created, payload);
            var events = new List<TableModifiedEventArgs>();
            table.TableModified += (sender, e) => events.Add(e);
            var json = CreateJsonDocument(idEntity);
            _ = await table.ReplaceItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(events);
            Assert.Equal(payload.Id, events[0].Id);
            Assert.NotNull(events[0].Entity);
            Assert.Equal(tableEndpoint, events[0].TableEndpoint.ToString());
            Assert.Equal(TableModifiedEventArgs.TableOperation.Replace, events[0].Operation);
        }
        #endregion

        #region UpdateItemAsync
        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ThrowsOnNullId()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.UpdateItemAsync(null, updates)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ThrowsOnNullUpdates()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.UpdateItemAsync(sId, null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ThrowsOnEmptyUpdates()
        {
            var client = GetMockClient();
            var table = client.GetTable<ClientMovie>("movies");
            var updates = new Dictionary<string, object>();
            await Assert.ThrowsAsync<ArgumentException>(() => table.UpdateItemAsync(sId, updates)).ConfigureAwait(false);
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
            await Assert.ThrowsAsync<ArgumentException>(() => table.UpdateItemAsync(id, updates)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_Success_FormulatesCorrectResponse(bool hasPrecondition)
        {
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var response = hasPrecondition
                ? await table.UpdateItemAsync(sId, updates, IfMatch.Version("etag")).ConfigureAwait(false)
                : await table.UpdateItemAsync(sId, updates).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal("patch", request.Method.ToString().ToLower());
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            Assert.Equal(sJsonUpdates, await request.Content.ReadAsStringAsync());
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
        }

        [Theory, CombinatorialData]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_Success_FormulatesCorrectResponse_WithAuth(bool hasPrecondition)
        {
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var response = hasPrecondition
                ? await authTable.UpdateItemAsync(sId, updates, IfMatch.Version("etag")).ConfigureAwait(false)
                : await authTable.UpdateItemAsync(sId, updates).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal("patch", request.Method.ToString().ToLower());
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            Assert.Equal(sJsonUpdates, await request.Content.ReadAsStringAsync());
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
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode, payload);

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<JsonDocument>>(() => table.UpdateItemAsync(sId, updates)).ConfigureAwait(false);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.NotEmpty(ex.Content);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ConflictNoContent_throws(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<JsonDocument>>(() => table.UpdateItemAsync(sId, updates)).ConfigureAwait(false);
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
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            MockHandler.Responses.Add(response);

            await Assert.ThrowsAsync<JsonException>(() => table.UpdateItemAsync(sId, updates)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.UpdateItemAsync(sId, updates)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_SuccessNoContent_Throws()
        {
            MockHandler.AddResponse(HttpStatusCode.OK);

            var response = await table.UpdateItemAsync(sId, updates).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal("patch", request.Method.ToString().ToLower());
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
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
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_TriggersEventHandler()
        {
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var events = new List<TableModifiedEventArgs>();
            table.TableModified += (sender, e) => events.Add(e);
            var response = await table.UpdateItemAsync(sId, updates).ConfigureAwait(false);

            // Check Request
            Assert.Single(events);
            Assert.Equal(payload.Id, events[0].Id);
            Assert.NotNull(events[0].Entity);
            Assert.Equal(tableEndpoint, events[0].TableEndpoint.ToString());
            Assert.Equal(TableModifiedEventArgs.TableOperation.Replace, events[0].Operation);
        }
        #endregion
    }
}

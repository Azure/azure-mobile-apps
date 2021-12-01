// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Datasync.Common.Test.TestData;
using Microsoft.AspNetCore.Datasync;
using Microsoft.Datasync.Client.Commands;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
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

using TestData = Datasync.Common.Test.TestData;

namespace Microsoft.Datasync.Client.Test.Table
{
    [ExcludeFromCodeCoverage]
    public class DatasyncTable_Tests : BaseTest
    {
        private const string sTablePath = "tables/movies";
        private readonly IdEntity payload = new() { Id = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f", StringValue = "test" };
        private const string sJsonPayload = "{\"id\":\"db0ec08d-46a9-465d-9f5e-0066a3ee5b5f\",\"stringValue\":\"test\"}";
        private const string sBadJson = "{this-is-bad-json";

        /// <summary>
        /// The common changes to the entity when we go against the test service.
        /// </summary>
        private readonly IReadOnlyDictionary<string, object> EntityUpdates = new Dictionary<string, object>()
        {
            { "Title", "Replacement Title" },
            { "Rating", "PG-13" }
        };

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_SetsInternals()
        {
            var client = GetMockClient();
            var sut = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            Assert.Equal("http://localhost/tables/movies/", sut.Endpoint.ToString());
            Assert.Same(client.ClientOptions, sut.ClientOptions);
            Assert.Same(client.HttpClient, sut.HttpClient);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullEndpoint_Throws()
        {
            var client = GetMockClient();
            const string relativeUri = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncTable<IdEntity>(relativeUri, client.HttpClient, client.ClientOptions));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullClient_Throws()
        {
            var client = GetMockClient();
            Assert.Throws<ArgumentNullException>(() => new DatasyncTable<IdEntity>("tables/movies", null, client.ClientOptions));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullOptions_Throws()
        {
            var client = GetMockClient();
            Assert.Throws<ArgumentNullException>(() => new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, null));
        }

        #region CreateItemAsync
        [Fact]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_ThrowsOnNull()
        {
            var client = GetMockClient();
            var table = client.GetTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.CreateItemAsync(null)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_Success_FormulatesCorrectRequest(HttpStatusCode statusCode)
        {
            var sEndpoint = new Uri(Endpoint, sTablePath).ToString();
            MockHandler.AddResponse(statusCode, payload);
            var client = GetMockClient();
            var sut = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var response = await sut.CreateItemAsync(payload).ConfigureAwait(false);

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
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_Success_FormulatesCorrectRequest_WithAuth(HttpStatusCode statusCode)
        {
            var sEndpoint = new Uri(Endpoint, sTablePath).ToString();
            MockHandler.AddResponse(statusCode, payload);
            var client = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken));
            var sut = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var response = await sut.CreateItemAsync(payload).ConfigureAwait(false);

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
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_SuccessNoContent(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var sut = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var response = await sut.CreateItemAsync(payload).ConfigureAwait(false);
            Assert.Equal((int)statusCode, response.StatusCode);
            Assert.False(response.HasContent);
            Assert.Empty(response.Content);
            Assert.False(response.HasValue);
            Assert.Null(response.Value);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_SuccessWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var payload = new IdEntity() { Id = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f", StringValue = "test" };

            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            MockHandler.Responses.Add(response);
            var client = GetMockClient();
            var sut = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            await Assert.ThrowsAsync<JsonException>(() => sut.CreateItemAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            var sEndpoint = new Uri(Endpoint, sTablePath).ToString();
            MockHandler.AddResponse(statusCode, payload);
            var client = GetMockClient();
            var sut = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => sut.CreateItemAsync(payload)).ConfigureAwait(false);

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
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_ConflictNoContent_Throws(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var sut = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);
            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => sut.CreateItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.Empty(ex.Content);
            Assert.Null(ex.ServerItem);
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
            MockHandler.Responses.Add(response);
            var client = GetMockClient();
            var sut = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            await Assert.ThrowsAsync<JsonException>(() => sut.CreateItemAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var sut = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => sut.CreateItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        #region DeleteItemAsync
        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ThrowsOnNull()
        {
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
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
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentException>(() => table.DeleteItemAsync(id)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_FormulatesCorrectResponse(bool hasPrecondition)
        {
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            var client = GetMockClient();
            var table = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var response = await table.DeleteItemAsync(sId, hasPrecondition ? IfMatch.Version("etag") : null).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
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
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var response = await table.DeleteItemAsync(sId, hasPrecondition ? IfMatch.Version("etag") : null).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
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
            MockHandler.AddResponse(statusCode, payload);
            var client = GetMockClient();
            var table = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.DeleteItemAsync(sId)).ConfigureAwait(false);

            // Check Response
            Assert.Equal((int)statusCode, ex.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.Equal(Encoding.UTF8.GetBytes(sJsonPayload), ex.Content);
            Assert.Equal("test", ex.ServerItem.StringValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConflictNoContent_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.DeleteItemAsync(sId)).ConfigureAwait(false);

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
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            MockHandler.Responses.Add(response);
            var client = GetMockClient();
            var table = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            await Assert.ThrowsAsync<JsonException>(() => table.DeleteItemAsync(sId)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = new DatasyncTable<IdEntity>("tables/movies", client.HttpClient, client.ClientOptions);

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(sId)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Basic()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.DeleteItemAsync(id).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(TestData.Movies.Count - 1, Server.GetMovieCount());
            Assert.Null(Server.GetMovieById(id));

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(TableOperation.Delete, modArg.Operation);
            Assert.Equal(id, modArg.Id);
            Assert.Null(modArg.Entity);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_NotFound()
        {
            // Arrange
            var client = CreateClientForTestServer();
            const string id = "not-found";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(id)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(404, exception.StatusCode);
            Assert.Equal(TestData.Movies.Count, Server.GetMovieCount());
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConditionalSuccess()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var expected = Server.GetMovieById(id);
            var etag = Convert.ToBase64String(expected.Version);

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.DeleteItemAsync(id, IfMatch.Version(etag)).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(TestData.Movies.Count - 1, Server.GetMovieCount());
            Assert.Null(Server.GetMovieById(id));

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(TableOperation.Delete, modArg.Operation);
            Assert.Equal(id, modArg.Id);
            Assert.Null(modArg.Entity);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConditionalFailure()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var expected = Server.GetMovieById(id).Clone();
            const string etag = "dGVzdA==";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.DeleteItemAsync(id, IfMatch.Version(etag))).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(412, exception.StatusCode);
            Assert.Equal(TestData.Movies.Count, Server.GetMovieCount());
            var entity = Server.GetMovieById(id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, exception.ServerItem);
            Assert.Equal<IMovie>(expected, exception.ServerItem);
            Assert.Equal<IMovie>(expected, entity);
            Assert.Equal<ITableData>(expected, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_SoftDelete()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();

            // Set up event handler
            var table = client.GetTable<ClientMovie>("soft");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.DeleteItemAsync(id).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(TestData.Movies.Count, Server.GetMovieCount());
            var entity = Server.GetMovieById(id)!;
            Assert.True(entity.Deleted);

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(TableOperation.Delete, modArg.Operation);
            Assert.Equal(id, modArg.Id);
            Assert.Null(modArg.Entity);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_GoneWhenDeleted()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            await Server.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);

            // Set up event handler
            var table = client.GetTable<ClientMovie>("soft");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(id)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(410, exception.StatusCode);
            Assert.Equal(TestData.Movies.Count, Server.GetMovieCount());
            var entity = Server.GetMovieById(id);
            Assert.True(entity.Deleted);
            Assert.Empty(modifications);
        }
        #endregion

        #region GetIdFromItem
        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => DatasyncTable<IdEntity>.GetIdFromItem(null));
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_IdEntity_ReturnsId()
        {
            IdEntity entity = new() { Id = "test" };
            var id = DatasyncTable<IdEntity>.GetIdFromItem(entity);
            Assert.Equal("test", id);
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_IdEntity_Null_Throws()
        {
            IdEntity entity = new() { Id = null };
            Assert.Throws<ArgumentException>(() => DatasyncTable<IdEntity>.GetIdFromItem(entity));
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_KeyAttribute_ReturnsId()
        {
            KeyEntity entity = new() { KeyId = "test" };
            var id = DatasyncTable<KeyEntity>.GetIdFromItem(entity);
            Assert.Equal("test", id);
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_KeyAttribute_Null_Throws()
        {
            KeyEntity entity = new() { KeyId = null };
            Assert.Throws<ArgumentException>(() => DatasyncTable<KeyEntity>.GetIdFromItem(entity));
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_NoId_Throws()
        {
            NoIdEntity entity = new() { Test = "test" };
            Assert.Throws<MissingMemberException>(() => DatasyncTable<NoIdEntity>.GetIdFromItem(entity));
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_NonStringId_Throws()
        {
            NonStringIdEntity entity = new() { Id = true };
            Assert.Throws<MemberAccessException>(() => DatasyncTable<NonStringIdEntity>.GetIdFromItem(entity));
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
            var client = GetMockClient();
            var table = client.GetTable<ClientMovie>("movies");

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
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

            // Act
            var enumerator = table.GetAsyncItems<IdEntity>().GetAsyncEnumerator();
            var hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            Assert.False(hasMore);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_NoItems_WithAuth()
        {
            // Arrange
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetTable<IdEntity>("movies");

            // Act
            var enumerator = table.GetAsyncItems<IdEntity>().GetAsyncEnumerator();
            var hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            Assert.False(hasMore);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_NoItems_WhenNullResponse()
        {
            Task<ServiceResponse<Page<IdEntity>>> pageFunc(string _)
                => ServiceResponse.FromResponseAsync<Page<IdEntity>>(new HttpResponseMessage(HttpStatusCode.OK), ClientOptions.DeserializerOptions);
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
            var page = CreatePageOfItems(5);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");
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
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            // Assert - response
            Assert.Equal(page.Items, items);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetItemsAsync_OnePageOfItems_WithAuth_WhenItemsReturned()
        {
            // Arrange
            var page = CreatePageOfItems(5);
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetTable<IdEntity>("movies");
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
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
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
            var page1 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");
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
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());
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
            var page1 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5);
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetTable<IdEntity>("movies");
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
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());
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
            var page1 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=3"));
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");
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
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");

            request = MockHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=3", request.RequestUri.ToString());
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
            var page1 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=3"));
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetTable<IdEntity>("movies");
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
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);

            request = MockHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=3", request.RequestUri.ToString());
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
            _ = CreatePageOfItems(5, 5);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

            // Act
            var pageable = table.GetAsyncItems<IdEntity>();
            var enumerator = pageable.GetAsyncEnumerator();
            _ = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert - request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
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
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetTable<IdEntity>("movies");

            // Act
            var pageable = table.GetAsyncItems<IdEntity>();
            var enumerator = pageable.GetAsyncEnumerator();
            _ = await enumerator.MoveNextAsync().ConfigureAwait(false);

            // Assert - request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());
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
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_RetrievesItems()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            int count = 0;

            var pageable = table.GetAsyncItems<ClientMovie>("$count=true");
            Assert.NotNull(pageable);

            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                count++;
                var item = enumerator.Current;

                Assert.NotNull(item);
                Assert.NotNull(item.Id);

                var expected = Server.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);

                Assert.Equal(TestData.Movies.Count, pageable.Count);
                Assert.NotNull(pageable.CurrentResponse);
            }

            Assert.Equal(TestData.Movies.Count, count);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_AsPages_RetrievesItems()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            int itemCount = 0, pageCount = 0;

            var pageable = table.GetAsyncItems<ClientMovie>().AsPages();
            Assert.NotNull(pageable);

            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                pageCount++;

                var page = enumerator.Current;
                Assert.NotNull(page);
                foreach (var item in page.Items)
                {
                    itemCount++;
                    Assert.NotNull(item.Id);
                    var expected = Server.GetMovieById(item.Id);
                    Assert.Equal<IMovie>(expected, item);
                }
            }

            Assert.Equal(Movies.Count, itemCount);
            Assert.Equal(3, pageCount);
        }
        #endregion

        #region GetItemAsync
        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_ThrowsOnNull()
        {
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
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
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentException>(() => table.GetItemAsync(id)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_FormulatesCorrectRequest(bool hasPrecondition)
        {
            // Arrange
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

            // Act
            var response = hasPrecondition
                ? await table.GetItemAsync(sId, IfNoneMatch.Version("etag")).ConfigureAwait(false)
                : await table.GetItemAsync(sId).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            if (hasPrecondition)
            {
                AssertEx.HasHeader(request.Headers, "If-None-Match", "\"etag\"");
            }
            else
            {
                Assert.False(request.Headers.Contains("If-None-Match"));
            }

            Assert.Equal(200, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.True(response.HasContent);
            Assert.Equal(sJsonPayload, Encoding.UTF8.GetString(response.Content));
            Assert.True(response.HasValue);
            Assert.Equal("test", response.Value.StringValue);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_FormulatesCorrectRequest_WithAuth(bool hasPrecondition)
        {
            // Arrange
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetTable<IdEntity>("movies");

            // Act
            var response = hasPrecondition
                ? await table.GetItemAsync(sId, IfNoneMatch.Version("etag")).ConfigureAwait(false)
                : await table.GetItemAsync(sId).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            if (hasPrecondition)
            {
                AssertEx.HasHeader(request.Headers, "If-None-Match", "\"etag\"");
            }
            else
            {
                Assert.False(request.Headers.Contains("If-None-Match"));
            }

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
            MockHandler.AddResponse(HttpStatusCode.OK);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

            // Act
            var response = await table.GetItemAsync(sId).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
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
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetItemAsync(sId)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_NotModified_Throws()
        {
            MockHandler.AddResponse(HttpStatusCode.NotModified);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<EntityNotModifiedException>(() => table.GetItemAsync(sId)).ConfigureAwait(false);
            Assert.Equal(304, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_Basic()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var expected = Server.GetMovieById(id)!;

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var response = await table.GetItemAsync(id).ConfigureAwait(false);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, response.Value);
            AssertEx.SystemPropertiesMatch(expected, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_NotFound()
        {
            // Arrange
            var client = CreateClientForTestServer();
            const string id = "not-found";

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetItemAsync(id)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_GetIfChanged()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var expected = Server.GetMovieById(id)!;

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var response = await table.GetItemAsync(id, IfNoneMatch.Version("dGVzdA==")).ConfigureAwait(false);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, response.Value);
            AssertEx.SystemPropertiesMatch(expected, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_FailIfSame()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var expected = Server.GetMovieById(id)!;
            var etag = Convert.ToBase64String(expected.Version);

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var exception = await Assert.ThrowsAsync<EntityNotModifiedException>(() => table.GetItemAsync(id, IfNoneMatch.Version(etag))).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(304, exception.StatusCode);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_GetIfNotSoftDeleted()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var expected = Server.GetMovieById(id)!;

            // Act
            var table = client.GetTable<ClientMovie>("soft");
            var response = await table.GetItemAsync(id).ConfigureAwait(false);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, response.Value);
            AssertEx.SystemPropertiesMatch(expected, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_GoneIfSoftDeleted()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            await Server.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);
            var expected = Server.GetMovieById(id)!;
            var etag = Convert.ToBase64String(expected.Version);

            // Act
            var table = client.GetTable<ClientMovie>("soft");
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetItemAsync(id, IfNoneMatch.Version(etag))).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(410, exception.StatusCode);
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
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var expectedUri = string.IsNullOrEmpty(query) ? sEndpoint : $"{sEndpoint}?{query}";

            _ = await table.GetPageOfItemsAsync<IdEntity>(query).ConfigureAwait(false);

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
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ConstructsRequest_WithQueryAndAuth(string query)
        {
            Page<IdEntity> result = new();
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var expectedUri = string.IsNullOrEmpty(query) ? sEndpoint : $"{sEndpoint}?{query}";

            _ = await table.GetPageOfItemsAsync<IdEntity>(query).ConfigureAwait(false);

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
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ConstructsRequest_WithRequestUri(string requestUri)
        {
            // Arrange
            Page<IdEntity> result = new();
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;

            // Act
            _ = await table.GetPageOfItemsAsync<IdEntity>("", requestUri).ConfigureAwait(false);

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
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ConstructsRequest_WithRequestUriAndAuth(string requestUri)
        {
            // Arrange
            Page<IdEntity> result = new();
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;

            // Act
            _ = await table.GetPageOfItemsAsync<IdEntity>("", requestUri).ConfigureAwait(false);

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
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ConstructsRequest_PrefersRequestUri(string query)
        {
            // Arrange
            Page<IdEntity> result = new();
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            const string requestUri = "https://localhost/tables/foo?$count=true&$skip=5&$top=10&__includedeleted=true";

            // Act
            _ = await table.GetPageOfItemsAsync<IdEntity>(query, requestUri).ConfigureAwait(false);

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
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ConstructsRequest_PrefersRequestUri_WithAuth(string query)
        {
            // Arrange
            Page<IdEntity> result = new();
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            const string requestUri = "https://localhost/tables/foo?$count=true&$skip=5&$top=10&__includedeleted=true";

            // Act
            _ = await table.GetPageOfItemsAsync<IdEntity>(query, requestUri).ConfigureAwait(false);

            // Assert
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(requestUri, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        }

        [Fact]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsItems()
        {
            // Arrange
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } } };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

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
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsCount()
        {
            // Arrange
            Page<IdEntity> result = new() { Count = 42 };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

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
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsNextLink()
        {
            // Arrange
            const string nextLink = sEndpoint + "?$top=5&$skip=5";
            Page<IdEntity> result = new() { NextLink = new Uri(nextLink) };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

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
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsItemsAndCount()
        {
            // Arrange
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } }, Count = 42 };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

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
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsItemsAndNextLink()
        {
            // Arrange
            const string nextLink = sEndpoint + "?$top=5&$skip=5";
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } }, NextLink = new Uri(nextLink) };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

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
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItems_ReturnsItem_Count_NextLink()
        {
            // Arrange
            const string nextLink = sEndpoint + "?$top=5&$skip=5";
            Page<IdEntity> result = new() { Items = new IdEntity[] { new() { Id = "1234" } }, Count = 42, NextLink = new Uri(nextLink) };
            MockHandler.AddResponse(HttpStatusCode.OK, result);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;

            // Act
            var response = await table.GetPageOfItemsAsync<IdEntity>(null).ConfigureAwait(false);

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
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItemAsync_BadResponse_ThrowsRequestFailed(HttpStatusCode statusCode)
        {
            // Arrange
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetPageOfItemsAsync<IdEntity>(null)).ConfigureAwait(false);

            // Assert
            Assert.Equal((int)statusCode, exception.StatusCode);
        }

        /// <summary>
        /// Basic query tests - these will do tests against tables/movies in various modes to ensure that the OData
        /// query items pass.
        /// </summary>
        /// <param name="query">The query to send</param>
        /// <param name="expectedItemCount">Response: the number of items</param>
        /// <param name="expectedNextLinkQuery">Response: the NextLink entity</param>
        /// <param name="expectedTotalCount">Response: The Count entity</param>
        /// <param name="firstExpectedItems">Response: The IDs of the first elements in the Items entity</param>
        [Theory]
        [InlineData("tables/movies", 100, "tables/movies?$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true", 100, "tables/movies?$count=true&$skip=100", Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 2, new[] { "id-061", "id-173" })]
        [InlineData("tables/movies?$count=true&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 13, null, 13, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" })]
        [InlineData("tables/movies?$count=true&$filter=(year div 1000.5) eq 2", 6, null, 6, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" })]
        [InlineData("tables/movies?$count=true&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 46, null, 46, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" })]
        [InlineData("tables/movies?$count=true&$filter=(year sub 1900) ge 80", 100, "tables/movies?$count=true&$filter=(year sub 1900) ge 80&$skip=100", 138, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner eq false", 100, "tables/movies?$count=true&$filter=bestPictureWinner eq false&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 11, null, 11, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 21, null, 21, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 24, null, 24, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner eq true", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner ne false", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner ne true", 100, "tables/movies?$count=true&$filter=bestPictureWinner ne true&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2&$skip=100", 124, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" })]
        [InlineData("tables/movies?$count=true&$filter=day(releaseDate) eq 1", 7, null, 7, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" })]
        [InlineData("tables/movies?$count=true&$filter=duration ge 60", 100, "tables/movies?$count=true&$filter=duration ge 60&$skip=100", Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=endswith(title, 'er')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$filter=endswith(tolower(title), 'er')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$filter=endswith(toupper(title), 'ER')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2&$skip=100", 120, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=month(releaseDate) eq 11", 14, null, 14, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" })]
        [InlineData("tables/movies?$count=true&$filter=not(bestPictureWinner eq false)", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$filter=not(bestPictureWinner eq true)", 100, "tables/movies?$count=true&$filter=not(bestPictureWinner eq true)&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=not(bestPictureWinner ne false)", 100, "tables/movies?$count=true&$filter=not(bestPictureWinner ne false)&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=not(bestPictureWinner ne true)", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$filter=rating eq 'R'", 94, null, 94, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/movies?$count=true&$filter=rating ne 'PG-13'", 100, "tables/movies?$count=true&$filter=rating ne 'PG-13'&$skip=100", 220, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=rating eq null", 74, null, 74, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$count=true&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 2, new[] { "id-000", "id-003" })]
        [InlineData("tables/movies?$count=true&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=round(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=round(duration div 60.0) eq 2&$skip=100", 186, new[] { "id-000", "id-005", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$count=true&$filter=startswith(rating, 'PG')", 64, null, 64, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" })]
        [InlineData("tables/movies?$count=true&$filter=startswith(tolower(title), 'the')", 63, null, 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=startswith(toupper(title), 'THE')", 63, null, 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=year eq 1994", 5, null, 5, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$count=true&$filter=year ge 2000 and year le 2009", 55, null, 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$count=true&$filter=year ge 2000", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$filter=year gt 1999 and year lt 2010", 55, null, 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$count=true&$filter=year gt 1999", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$filter=year le 2000", 100, "tables/movies?$count=true&$filter=year le 2000&$skip=100", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=year lt 2001", 100, "tables/movies?$count=true&$filter=year lt 2001&$skip=100", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=year(releaseDate) eq 1994", 6, null, 6, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$count=true&$orderby=bestPictureWinner asc", 100, "tables/movies?$count=true&$orderby=bestPictureWinner asc&$skip=100", Movies.Count, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$orderby=bestPictureWinner desc", 100, "tables/movies?$count=true&$orderby=bestPictureWinner desc&$skip=100", Movies.Count, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$orderby=duration asc", 100, "tables/movies?$count=true&$orderby=duration asc&$skip=100", Movies.Count, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" })]
        [InlineData("tables/movies?$count=true&$orderby=duration desc", 100, "tables/movies?$count=true&$orderby=duration desc&$skip=100", Movies.Count, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" })]
        [InlineData("tables/movies?$count=true&$orderby=rating asc", 100, "tables/movies?$count=true&$orderby=rating asc&$skip=100", Movies.Count, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$count=true&$orderby=rating desc", 100, "tables/movies?$count=true&$orderby=rating desc&$skip=100", Movies.Count, new[] { "id-197", "id-107", "id-115", "id-028", "id-039" })]
        [InlineData("tables/movies?$count=true&$orderby=releaseDate asc", 100, "tables/movies?$count=true&$orderby=releaseDate asc&$skip=100", Movies.Count, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" })]
        [InlineData("tables/movies?$count=true&$orderby=releaseDate desc", 100, "tables/movies?$count=true&$orderby=releaseDate desc&$skip=100", Movies.Count, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" })]
        [InlineData("tables/movies?$count=true&$orderby=title asc", 100, "tables/movies?$count=true&$orderby=title asc&$skip=100", Movies.Count, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" })]
        [InlineData("tables/movies?$count=true&$orderby=title desc", 100, "tables/movies?$count=true&$orderby=title desc&$skip=100", Movies.Count, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" })]
        [InlineData("tables/movies?$count=true&$orderby=year asc", 100, "tables/movies?$count=true&$orderby=year asc&$skip=100", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$count=true&$orderby=year asc,title asc", 100, "tables/movies?$count=true&$orderby=year asc,title asc&$skip=100", Movies.Count, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" })]
        [InlineData("tables/movies?$count=true&$orderby=year asc,title desc", 100, "tables/movies?$count=true&$orderby=year asc,title desc&$skip=100", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$count=true&$orderby=year desc", 100, "tables/movies?$count=true&$orderby=year desc&$skip=100", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" })]
        [InlineData("tables/movies?$count=true&$orderby=year desc,title asc", 100, "tables/movies?$count=true&$orderby=year desc,title asc&$skip=100", Movies.Count, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" })]
        [InlineData("tables/movies?$count=true&$orderby=year desc,title desc", 100, "tables/movies?$count=true&$orderby=year desc,title desc&$skip=100", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 2, new[] { "id-061", "id-173" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 13, null, 13, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=(year div 1000.5) eq 2", 6, null, 6, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 46, null, 46, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=(year sub 1900) ge 80", 100, "tables/movies?$count=true&$filter=(year sub 1900) ge 80&$skip=100&$top=25", 138, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq false", 100, "tables/movies?$count=true&$filter=bestPictureWinner eq false&$skip=100&$top=25", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 11, null, 11, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 21, null, 21, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 24, null, 24, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=bestPictureWinner ne false", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=bestPictureWinner ne true", 100, "tables/movies?$count=true&$filter=bestPictureWinner ne true&$skip=100&$top=25", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=ceiling(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2&$skip=100&$top=25", 124, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=day(releaseDate) eq 1", 7, null, 7, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=duration ge 60", 100, "tables/movies?$count=true&$filter=duration ge 60&$skip=100&$top=25", Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=endswith(title, 'er')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=endswith(tolower(title), 'er')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=endswith(toupper(title), 'ER')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=floor(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2&$skip=100&$top=25", 120, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=month(releaseDate) eq 11", 14, null, 14, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner eq false)", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner eq true)", 100, "tables/movies?$count=true&$filter=not(bestPictureWinner eq true)&$skip=100&$top=25", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner ne false)", 100, "tables/movies?$count=true&$filter=not(bestPictureWinner ne false)&$skip=100&$top=25", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner ne true)", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=rating eq 'R'", 94, null, 94, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=rating ne 'PG-13'", 100, "tables/movies?$count=true&$filter=rating ne 'PG-13'&$skip=100&$top=25", 220, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=rating eq null", 74, null, 74, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 2, new[] { "id-000", "id-003" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100&$top=25", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100&$top=25", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=round(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=round(duration div 60.0) eq 2&$skip=100&$top=25", 186, new[] { "id-000", "id-005", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=startswith(rating, 'PG')", 64, null, 64, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=startswith(tolower(title), 'the')", 63, null, 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=startswith(toupper(title), 'THE')", 63, null, 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=year eq 1994", 5, null, 5, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=year ge 2000 and year le 2009", 55, null, 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=year ge 2000", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=year gt 1999 and year lt 2010", 55, null, 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=year gt 1999", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=year le 2000", 100, "tables/movies?$count=true&$filter=year le 2000&$skip=100&$top=25", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=year lt 2001", 100, "tables/movies?$count=true&$filter=year lt 2001&$skip=100&$top=25", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=125&$filter=year(releaseDate) eq 1994", 6, null, 6, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=bestPictureWinner asc", 100, "tables/movies?$count=true&$orderby=bestPictureWinner asc&$skip=100&$top=25", Movies.Count, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=bestPictureWinner desc", 100, "tables/movies?$count=true&$orderby=bestPictureWinner desc&$skip=100&$top=25", Movies.Count, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=duration asc", 100, "tables/movies?$count=true&$orderby=duration asc&$skip=100&$top=25", Movies.Count, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=duration desc", 100, "tables/movies?$count=true&$orderby=duration desc&$skip=100&$top=25", Movies.Count, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=rating asc", 100, "tables/movies?$count=true&$orderby=rating asc&$skip=100&$top=25", Movies.Count, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=rating desc", 100, "tables/movies?$count=true&$orderby=rating desc&$skip=100&$top=25", Movies.Count, new[] { "id-197", "id-107", "id-115", "id-028", "id-039" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=releaseDate asc", 100, "tables/movies?$count=true&$orderby=releaseDate asc&$skip=100&$top=25", Movies.Count, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=releaseDate desc", 100, "tables/movies?$count=true&$orderby=releaseDate desc&$skip=100&$top=25", Movies.Count, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=title asc", 100, "tables/movies?$count=true&$orderby=title asc&$skip=100&$top=25", Movies.Count, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=title desc", 100, "tables/movies?$count=true&$orderby=title desc&$skip=100&$top=25", Movies.Count, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=year asc", 100, "tables/movies?$count=true&$orderby=year asc&$skip=100&$top=25", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=year asc,title asc", 100, "tables/movies?$count=true&$orderby=year asc,title asc&$skip=100&$top=25", Movies.Count, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=year asc,title desc", 100, "tables/movies?$count=true&$orderby=year asc,title desc&$skip=100&$top=25", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=year desc", 100, "tables/movies?$count=true&$orderby=year desc&$skip=100&$top=25", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=year desc,title asc", 100, "tables/movies?$count=true&$orderby=year desc,title asc&$skip=100&$top=25", Movies.Count, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" })]
        [InlineData("tables/movies?$count=true&$top=125&$orderby=year desc,title desc", 100, "tables/movies?$count=true&$orderby=year desc,title desc&$skip=100&$top=25", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" })]
        [InlineData("tables/movies?$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 0, new[] { "id-061", "id-173" })]
        [InlineData("tables/movies?$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 13, null, 0, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" })]
        [InlineData("tables/movies?$filter=(year div 1000.5) eq 2", 6, null, 0, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" })]
        [InlineData("tables/movies?$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 46, null, 0, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" })]
        [InlineData("tables/movies?$filter=(year sub 1900) ge 80", 100, "tables/movies?$filter=(year sub 1900) ge 80&$skip=100", 0, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq false", 100, "tables/movies?$filter=bestPictureWinner eq false&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 11, null, 0, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 21, null, 0, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 24, null, 0, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$filter=bestPictureWinner ne false", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$filter=bestPictureWinner ne true", 100, "tables/movies?$filter=bestPictureWinner ne true&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$filter=ceiling(duration div 60.0) eq 2", 100, "tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=100", 0, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" })]
        [InlineData("tables/movies?$filter=day(releaseDate) eq 1", 7, null, 0, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" })]
        [InlineData("tables/movies?$filter=duration ge 60", 100, "tables/movies?$filter=duration ge 60&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=endswith(title, 'er')", 12, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$filter=endswith(tolower(title), 'er')", 12, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$filter=endswith(toupper(title), 'ER')", 12, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$filter=floor(duration div 60.0) eq 2", 100, "tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=100", 0, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" })]
        [InlineData("tables/movies?$filter=month(releaseDate) eq 11", 14, null, 0, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner eq false)", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner eq true)", 100, "tables/movies?$filter=not(bestPictureWinner eq true)&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner ne false)", 100, "tables/movies?$filter=not(bestPictureWinner ne false)&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner ne true)", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$filter=rating eq 'R'", 94, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/movies?$filter=rating ne 'PG-13'", 100, "tables/movies?$filter=rating ne 'PG-13'&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=rating eq null", 74, null, 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 0, new[] { "id-000", "id-003" })]
        [InlineData("tables/movies?$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=round(duration div 60.0) eq 2", 100, "tables/movies?$filter=round(duration div 60.0) eq 2&$skip=100", 0, new[] { "id-000", "id-005", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=startswith(rating, 'PG')", 64, null, 0, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" })]
        [InlineData("tables/movies?$filter=startswith(tolower(title), 'the')", 63, null, 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$filter=startswith(toupper(title), 'THE')", 63, null, 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$filter=year eq 1994", 5, null, 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$filter=year ge 2000 and year le 2009", 55, null, 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$filter=year ge 2000", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$filter=year gt 1999 and year lt 2010", 55, null, 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$filter=year gt 1999", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$filter=year le 2000", 100, "tables/movies?$filter=year le 2000&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=year lt 2001", 100, "tables/movies?$filter=year lt 2001&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=year(releaseDate) eq 1994", 6, null, 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$orderby=bestPictureWinner asc", 100, "tables/movies?$orderby=bestPictureWinner asc&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$orderby=bestPictureWinner desc", 100, "tables/movies?$orderby=bestPictureWinner desc&$skip=100", 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$orderby=duration asc", 100, "tables/movies?$orderby=duration asc&$skip=100", 0, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" })]
        [InlineData("tables/movies?$orderby=duration desc", 100, "tables/movies?$orderby=duration desc&$skip=100", 0, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" })]
        [InlineData("tables/movies?$orderby=rating asc", 100, "tables/movies?$orderby=rating asc&$skip=100", 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$orderby=rating desc", 100, "tables/movies?$orderby=rating desc&$skip=100", 0, new[] { "id-197", "id-107", "id-115", "id-028", "id-039" })]
        [InlineData("tables/movies?$orderby=releaseDate asc", 100, "tables/movies?$orderby=releaseDate asc&$skip=100", 0, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" })]
        [InlineData("tables/movies?$orderby=releaseDate desc", 100, "tables/movies?$orderby=releaseDate desc&$skip=100", 0, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" })]
        [InlineData("tables/movies?$orderby=title asc", 100, "tables/movies?$orderby=title asc&$skip=100", 0, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" })]
        [InlineData("tables/movies?$orderby=title desc", 100, "tables/movies?$orderby=title desc&$skip=100", 0, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" })]
        [InlineData("tables/movies?$orderby=year asc", 100, "tables/movies?$orderby=year asc&$skip=100", 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$orderby=year asc,title asc", 100, "tables/movies?$orderby=year asc,title asc&$skip=100", 0, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" })]
        [InlineData("tables/movies?$orderby=year asc,title desc", 100, "tables/movies?$orderby=year asc,title desc&$skip=100", 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$orderby=year desc", 100, "tables/movies?$orderby=year desc&$skip=100", 0, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" })]
        [InlineData("tables/movies?$orderby=year desc,title asc", 100, "tables/movies?$orderby=year desc,title asc&$skip=100", 0, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" })]
        [InlineData("tables/movies?$orderby=year desc,title desc", 100, "tables/movies?$orderby=year desc,title desc&$skip=100", 0, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" })]
        [InlineData("tables/movies?$filter=((year div 1000.5) eq 2) and (rating eq 'R')&$skip=5", 0, null, 0, new string[] { })]
        [InlineData("tables/movies?$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)&$skip=5", 8, null, 0, new[] { "id-142", "id-143", "id-162", "id-166", "id-172" })]
        [InlineData("tables/movies?$filter=(year div 1000.5) eq 2&$skip=5", 1, null, 0, new[] { "id-216" })]
        [InlineData("tables/movies?$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)&$skip=5", 41, null, 0, new[] { "id-040", "id-041", "id-044", "id-046", "id-049" })]
        [InlineData("tables/movies?$filter=(year sub 1900) ge 80&$skip=5", 100, "tables/movies?$filter=(year sub 1900) ge 80&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq false&$skip=5", 100, "tables/movies?$filter=bestPictureWinner eq false&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2&$skip=5", 6, null, 0, new[] { "id-150", "id-155", "id-186", "id-189", "id-196" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2&$skip=5", 16, null, 0, new[] { "id-062", "id-083", "id-087", "id-092", "id-093" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2&$skip=5", 19, null, 0, new[] { "id-092", "id-093", "id-094", "id-096", "id-112" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" })]
        [InlineData("tables/movies?$filter=bestPictureWinner ne false&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" })]
        [InlineData("tables/movies?$filter=bestPictureWinner ne true&$skip=5", 100, "tables/movies?$filter=bestPictureWinner ne true&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=5", 100, "tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=105", 0, new[] { "id-027", "id-028", "id-030", "id-031", "id-032" })]
        [InlineData("tables/movies?$filter=day(releaseDate) eq 1&$skip=5", 2, null, 0, new[] { "id-197", "id-215" })]
        [InlineData("tables/movies?$filter=duration ge 60&$skip=5", 100, "tables/movies?$filter=duration ge 60&$skip=105", 0, new[] { "id-005", "id-006", "id-007", "id-008", "id-009" })]
        [InlineData("tables/movies?$filter=endswith(title, 'er')&$skip=5", 7, null, 0, new[] { "id-170", "id-193", "id-197", "id-205", "id-217" })]
        [InlineData("tables/movies?$filter=endswith(tolower(title), 'er')&$skip=5", 7, null, 0, new[] { "id-170", "id-193", "id-197", "id-205", "id-217" })]
        [InlineData("tables/movies?$filter=endswith(toupper(title), 'ER')&$skip=5", 7, null, 0, new[] { "id-170", "id-193", "id-197", "id-205", "id-217" })]
        [InlineData("tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=5", 100, "tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=105", 0, new[] { "id-009", "id-010", "id-011", "id-012", "id-013" })]
        [InlineData("tables/movies?$filter=month(releaseDate) eq 11&$skip=5", 9, null, 0, new[] { "id-115", "id-131", "id-136", "id-146", "id-167" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner eq false)&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner eq true)&$skip=5", 100, "tables/movies?$filter=not(bestPictureWinner eq true)&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner ne false)&$skip=5", 100, "tables/movies?$filter=not(bestPictureWinner ne false)&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner ne true)&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" })]
        [InlineData("tables/movies?$filter=rating eq null&$skip=5", 69, null, 0, new[] { "id-040", "id-041", "id-044", "id-046", "id-049" })]
        [InlineData("tables/movies?$filter=rating eq 'R'&$skip=5", 89, null, 0, new[] { "id-009", "id-014", "id-017", "id-019", "id-022" })]
        [InlineData("tables/movies?$filter=rating ne 'PG-13'&$skip=5", 100, "tables/movies?$filter=rating ne 'PG-13'&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 0, null, 0, new string[] { })]
        [InlineData("tables/movies?$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
        [InlineData("tables/movies?$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
        [InlineData("tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 100, "tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 100, "tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=round(duration div 60.0) eq 2&$skip=5", 100, "tables/movies?$filter=round(duration div 60.0) eq 2&$skip=105", 0, new[] { "id-013", "id-014", "id-015", "id-016", "id-017" })]
        [InlineData("tables/movies?$filter=startswith(rating, 'PG')&$skip=5", 59, null, 0, new[] { "id-015", "id-018", "id-020", "id-021", "id-024" })]
        [InlineData("tables/movies?$filter=startswith(tolower(title), 'the')&$skip=5", 58, null, 0, new[] { "id-008", "id-012", "id-017", "id-020", "id-023" })]
        [InlineData("tables/movies?$filter=startswith(toupper(title), 'THE')&$skip=5", 58, null, 0, new[] { "id-008", "id-012", "id-017", "id-020", "id-023" })]
        [InlineData("tables/movies?$filter=year eq 1994&$skip=5", 0, null, 0, new string[] { })]
        [InlineData("tables/movies?$filter=year ge 2000 and year le 2009&$skip=5", 50, null, 0, new[] { "id-032", "id-042", "id-050", "id-051", "id-058" })]
        [InlineData("tables/movies?$filter=year ge 2000&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
        [InlineData("tables/movies?$filter=year gt 1999 and year lt 2010&$skip=5", 50, null, 0, new[] { "id-032", "id-042", "id-050", "id-051", "id-058" })]
        [InlineData("tables/movies?$filter=year gt 1999&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
        [InlineData("tables/movies?$filter=year le 2000&$skip=5", 100, "tables/movies?$filter=year le 2000&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=year lt 2001&$skip=5", 100, "tables/movies?$filter=year lt 2001&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=year(releaseDate) eq 1994&$skip=5", 1, null, 0, new[] { "id-217" })]
        [InlineData("tables/movies?$orderby=bestPictureWinner asc&$skip=5", 100, "tables/movies?$orderby=bestPictureWinner asc&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$orderby=bestPictureWinner desc&$skip=5", 100, "tables/movies?$orderby=bestPictureWinner desc&$skip=105", 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" })]
        [InlineData("tables/movies?$orderby=duration asc&$skip=5", 100, "tables/movies?$orderby=duration asc&$skip=105", 0, new[] { "id-238", "id-201", "id-115", "id-229", "id-181" })]
        [InlineData("tables/movies?$orderby=duration desc&$skip=5", 100, "tables/movies?$orderby=duration desc&$skip=105", 0, new[] { "id-007", "id-183", "id-063", "id-202", "id-130" })]
        [InlineData("tables/movies?$orderby=rating asc&$skip=5", 100, "tables/movies?$orderby=rating asc&$skip=105", 0, new[] { "id-040", "id-041", "id-044", "id-046", "id-049" })]
        [InlineData("tables/movies?$orderby=rating desc&$skip=5", 100, "tables/movies?$orderby=rating desc&$skip=105", 0, new[] { "id-148", "id-000", "id-001", "id-002", "id-003" })]
        [InlineData("tables/movies?$orderby=releaseDate asc&$skip=5", 100, "tables/movies?$orderby=releaseDate asc&$skip=105", 0, new[] { "id-229", "id-224", "id-041", "id-049", "id-135" })]
        [InlineData("tables/movies?$orderby=releaseDate desc&$skip=5", 100, "tables/movies?$orderby=releaseDate desc&$skip=105", 0, new[] { "id-149", "id-213", "id-102", "id-155", "id-169" })]
        [InlineData("tables/movies?$orderby=title asc&$skip=5", 100, "tables/movies?$orderby=title asc&$skip=105", 0, new[] { "id-214", "id-102", "id-215", "id-039", "id-057" })]
        [InlineData("tables/movies?$orderby=title desc&$skip=5", 100, "tables/movies?$orderby=title desc&$skip=105", 0, new[] { "id-058", "id-046", "id-160", "id-092", "id-176" })]
        [InlineData("tables/movies?$orderby=year asc&$skip=5", 100, "tables/movies?$orderby=year asc&$skip=105", 0, new[] { "id-088", "id-224", "id-041", "id-049", "id-135" })]
        [InlineData("tables/movies?$orderby=year asc,title asc&$skip=5", 100, "tables/movies?$orderby=year asc,title asc&$skip=105", 0, new[] { "id-088", "id-224", "id-041", "id-049", "id-135" })]
        [InlineData("tables/movies?$orderby=year asc,title desc&$skip=5", 100, "tables/movies?$orderby=year asc,title desc&$skip=105", 0, new[] { "id-088", "id-224", "id-049", "id-041", "id-135" })]
        [InlineData("tables/movies?$orderby=year desc&$skip=5", 100, "tables/movies?$orderby=year desc&$skip=105", 0, new[] { "id-149", "id-186", "id-213", "id-013", "id-053" })]
        [InlineData("tables/movies?$orderby=year desc,title asc&$skip=5", 100, "tables/movies?$orderby=year desc,title asc&$skip=105", 0, new[] { "id-186", "id-064", "id-149", "id-169", "id-161" })]
        [InlineData("tables/movies?$orderby=year desc,title desc&$skip=5", 100, "tables/movies?$orderby=year desc,title desc&$skip=105", 0, new[] { "id-186", "id-213", "id-102", "id-053", "id-155" })]
        [InlineData("tables/movies?$skip=0", 100, "tables/movies?$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$skip=100", 100, "tables/movies?$skip=200", 0, new[] { "id-100", "id-101", "id-102", "id-103", "id-104" })]
        [InlineData("tables/movies?$skip=200", 48, null, 0, new[] { "id-200", "id-201", "id-202", "id-203", "id-204" })]
        [InlineData("tables/movies?$skip=300", 0, null, 0, new string[] { })]
        [InlineData("tables/movies?$top=5&$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 0, new[] { "id-061", "id-173" })]
        [InlineData("tables/movies?$top=5&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 5, null, 0, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" })]
        [InlineData("tables/movies?$top=5&$filter=(year div 1000.5) eq 2", 5, null, 0, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" })]
        [InlineData("tables/movies?$top=5&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 5, null, 0, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" })]
        [InlineData("tables/movies?$top=5&$filter=(year sub 1900) ge 80", 5, null, 0, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner eq false", 5, null, 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 5, null, 0, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 5, null, 0, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 5, null, 0, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner eq true", 5, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner ne false", 5, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner ne true", 5, null, 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=ceiling(duration div 60.0) eq 2", 5, null, 0, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" })]
        [InlineData("tables/movies?$top=5&$filter=day(releaseDate) eq 1", 5, null, 0, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" })]
        [InlineData("tables/movies?$top=5&$filter=duration ge 60", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=endswith(title, 'er')", 5, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$top=5&$filter=endswith(tolower(title), 'er')", 5, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$top=5&$filter=endswith(toupper(title), 'ER')", 5, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$top=5&$filter=floor(duration div 60.0) eq 2", 5, null, 0, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=month(releaseDate) eq 11", 5, null, 0, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" })]
        [InlineData("tables/movies?$top=5&$filter=not(bestPictureWinner eq false)", 5, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$top=5&$filter=not(bestPictureWinner eq true)", 5, null, 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=not(bestPictureWinner ne false)", 5, null, 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=not(bestPictureWinner ne true)", 5, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$top=5&$filter=rating eq 'R'", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/movies?$top=5&$filter=rating ne 'PG-13'", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=rating eq null", 5, null, 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$top=5&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 0, new[] { "id-000", "id-003" })]
        [InlineData("tables/movies?$top=5&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$top=5&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$top=5&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=round(duration div 60.0) eq 2", 5, null, 0, new[] { "id-000", "id-005", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$top=5&$filter=startswith(rating, 'PG')", 5, null, 0, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" })]
        [InlineData("tables/movies?$top=5&$filter=startswith(tolower(title), 'the')", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=startswith(toupper(title), 'THE')", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=year eq 1994", 5, null, 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$top=5&$filter=year ge 2000 and year le 2009", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$top=5&$filter=year ge 2000", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$top=5&$filter=year gt 1999 and year lt 2010", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$top=5&$filter=year gt 1999", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$top=5&$filter=year le 2000", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=year lt 2001", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=year(releaseDate) eq 1994", 5, null, 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$top=5&$orderby=bestPictureWinner asc", 5, null, 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$top=5&$orderby=bestPictureWinner desc", 5, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$top=5&$orderby=duration asc", 5, null, 0, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" })]
        [InlineData("tables/movies?$top=5&$orderby=duration desc", 5, null, 0, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" })]
        [InlineData("tables/movies?$top=5&$orderby=rating asc", 5, null, 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$top=5&$orderby=rating desc", 5, null, 0, new[] { "id-197", "id-107", "id-115", "id-028", "id-039" })]
        [InlineData("tables/movies?$top=5&$orderby=releaseDate asc", 5, null, 0, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" })]
        [InlineData("tables/movies?$top=5&$orderby=releaseDate desc", 5, null, 0, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" })]
        [InlineData("tables/movies?$top=5&$orderby=title asc", 5, null, 0, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" })]
        [InlineData("tables/movies?$top=5&$orderby=title desc", 5, null, 0, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" })]
        [InlineData("tables/movies?$top=5&$orderby=year asc", 5, null, 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$top=5&$orderby=year asc,title asc", 5, null, 0, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" })]
        [InlineData("tables/movies?$top=5&$orderby=year asc,title desc", 5, null, 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$top=5&$orderby=year desc", 5, null, 0, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" })]
        [InlineData("tables/movies?$top=5&$orderby=year desc,title asc", 5, null, 0, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" })]
        [InlineData("tables/movies?$top=5&$orderby=year desc,title desc", 5, null, 0, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" })]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItemsAsync_Basic(string pathAndQuery, int expectedItemCount, string expectedNextLinkQuery, long expectedTotalCount, string[] firstExpectedItems)
        {
            // Arrange
            var segments = pathAndQuery.Split('?');
            var query = segments.Length > 1 ? segments[1] : string.Empty;
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies") as DatasyncTable<ClientMovie>;

            // Act
            var response = await table.GetPageOfItemsAsync<ClientMovie>(query).ConfigureAwait(false);
            var result = response.Value;
            var items = result.Items.ToArray();

            // Assert
            Assert.Equal(expectedItemCount, items.Length);
            Assert.Equal(expectedTotalCount, result.Count ?? 0);

            if (expectedNextLinkQuery == null)
            {
                Assert.Null(result.NextLink);
            }
            else
            {
                var nextlinkSegments = result.NextLink.PathAndQuery.Split('?');
                var expectedSegments = expectedNextLinkQuery.Split('?');
                Assert.Equal(expectedSegments[0].Trim('/'), nextlinkSegments[0].Trim('/'));
                Assert.Equal(expectedSegments[1], Uri.UnescapeDataString(nextlinkSegments[1]));
            }

            // The first n items must match what is expected
            Assert.True(items.Length >= firstExpectedItems.Length);
            Assert.Equal(firstExpectedItems, result.Items.Take(firstExpectedItems.Length).Select(m => m.Id).ToArray());
            for (int idx = 0; idx < firstExpectedItems.Length; idx++)
            {
                var expected = Server.GetMovieById(firstExpectedItems[idx]);
                var actual = items[idx];
                Assert.Equal<IMovie>(expected, actual);
                AssertEx.SystemPropertiesMatch(expected, actual);
            }
        }

        [Theory, PairwiseData]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItemsAsync_Select(bool sId, bool sUpdatedAt, bool sVersion, bool sDeleted, bool sBPW, bool sduration, bool srating, bool sreleaseDate, bool stitle, bool syear)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies") as DatasyncTable<ClientMovie>;

            List<string> selection = new();
            if (sId) selection.Add("id");
            if (sUpdatedAt) selection.Add("updatedAt");
            if (sVersion) selection.Add("version");
            if (sDeleted) selection.Add("deleted");
            if (sBPW) selection.Add("bestPictureWinner");
            if (sduration) selection.Add("duration");
            if (srating) selection.Add("rating");
            if (sreleaseDate) selection.Add("releaseDate");
            if (stitle) selection.Add("title");
            if (syear) selection.Add("year");
            if (selection.Count == 0) return;
            var query = $"$top=5&$skip=5&$select={string.Join(',', selection)}";

            // Act
            var response = await table.GetPageOfItemsAsync<Dictionary<string, object>>(query).ConfigureAwait(false);
            var result = response.Value;
            var items = result.Items.ToArray();

            // Assert
            foreach (var item in items)
            {
                foreach (var property in selection)
                {
                    Assert.True(item.ContainsKey(property));
                }
            }
        }

        [Theory]
        [InlineData("tables/notfound", HttpStatusCode.NotFound)]
        [InlineData("tables/movies?$filter=(year - 1900) ge 100", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$filter=missing eq 20", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$orderby=duration fizz", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$orderby=missing asc", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$select=foo", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$select=year rating", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$skip=-1", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$skip=NaN", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$top=-1", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$top=1000000", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$top=NaN", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$filter=(year - 1900) ge 100", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$filter=missing eq 20", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$orderby=duration fizz", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$orderby=missing asc", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$select=foo", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$select=year rating", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$skip=-1", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$skip=NaN", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$top=-1", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$top=1000000", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$top=NaN", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_rated", HttpStatusCode.Unauthorized)]
        [InlineData("tables/movies_legal", HttpStatusCode.UnavailableForLegalReasons)]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItemsAsync_FailedQuery(string pathAndQuery, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var segments = pathAndQuery.Split('?');
            var tableName = segments[0].Split('/')[1];
            var query = segments.Length > 1 ? segments[1] : string.Empty;
            var table = client.GetTable<ClientMovie>(tableName) as DatasyncTable<ClientMovie>;

            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetPageOfItemsAsync<ClientMovie>(query)).ConfigureAwait(false);

            Assert.Equal(expectedStatusCode, exception.Response.StatusCode);
        }

        [Theory]
        [InlineData("tables/soft?$count=true", 100, "tables/soft?$count=true&$skip=100", 154, new[] { "id-004", "id-005", "id-006", "id-008", "id-010" })]
        [InlineData("tables/soft?$filter=deleted eq false&__includedeleted=true", 100, "tables/soft?$filter=deleted eq false&__includedeleted=true&$skip=100", 0, new[] { "id-004", "id-005", "id-006", "id-008", "id-010" })]
        [InlineData("tables/soft?$filter=deleted eq true&__includedeleted=true", 94, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/soft?__includedeleted=true", 100, "tables/soft?__includedeleted=true&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004", "id-005" })]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItemsAsync_SoftDeleteQuery(string pathAndQuery, int expectedItemCount, string expectedNextLinkQuery, long expectedTotalCount, string[] firstExpectedItems)
        {
            // Arrange
            string query = pathAndQuery.Split('?')[1];
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("soft") as DatasyncTable<ClientMovie>;

            // Act
            var response = await table.GetPageOfItemsAsync<ClientMovie>(query).ConfigureAwait(false);
            var result = response.Value;
            var items = result.Items.ToArray();

            // Assert
            Assert.Equal(expectedItemCount, items.Length);
            Assert.Equal(expectedTotalCount, result.Count ?? 0);

            if (expectedNextLinkQuery == null)
            {
                Assert.Null(result.NextLink);
            }
            else
            {
                var nextlinkSegments = result.NextLink.PathAndQuery.Split('?');
                var expectedSegments = expectedNextLinkQuery.Split('?');
                Assert.Equal(expectedSegments[0].Trim('/'), nextlinkSegments[0].Trim('/'));
                Assert.Equal(expectedSegments[1], Uri.UnescapeDataString(nextlinkSegments[1]));
            }

            // The first n items must match what is expected
            Assert.True(items.Length >= firstExpectedItems.Length);
            Assert.Equal(firstExpectedItems, result.Items.Take(firstExpectedItems.Length).Select(m => m.Id).ToArray());
            for (int idx = 0; idx < firstExpectedItems.Length; idx++)
            {
                var expected = Server.GetMovieById(firstExpectedItems[idx]);
                var actual = items[idx];
                Assert.Equal<IMovie>(expected, actual);
                AssertEx.SystemPropertiesMatch(expected, actual);
            }
        }
        #endregion

        #region ReplaceItemAsync
        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ThrowsOnNull()
        {
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.ReplaceItemAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ThrowsOnNullId()
        {
            var obj = new ClientMovie { Id = null };
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
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
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(obj)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Success_FormulatesCorrectResponse(bool hasPrecondition)
        {
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

            var response = hasPrecondition
                ? await table.ReplaceItemAsync(payload, IfMatch.Version("etag")).ConfigureAwait(false)
                : await table.ReplaceItemAsync(payload).ConfigureAwait(false);

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
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetTable<IdEntity>("movies");

            var response = hasPrecondition
                ? await table.ReplaceItemAsync(payload, IfMatch.Version("etag")).ConfigureAwait(false)
                : await table.ReplaceItemAsync(payload).ConfigureAwait(false);

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
            MockHandler.AddResponse(HttpStatusCode.OK);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

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
            var table = client.GetTable<IdEntity>("movies");

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
            var table = client.GetTable<IdEntity>("movies");

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
            var table = client.GetTable<IdEntity>("movies");

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
            var table = client.GetTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Basic()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var original = Server.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.ReplaceItemAsync(expected).ConfigureAwait(false);
            var stored = Server.GetMovieById(id);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.SystemPropertiesMatch(stored, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(TableOperation.Update, modArg.Operation);
            Assert.Equal(response.Value.Id, modArg.Id);
            Assert.Equal<IMovie>(response.Value, modArg.Entity as IMovie);
        }

        [Theory]
        [InlineData("duration", 50)]
        [InlineData("duration", 370)]
        [InlineData("rating", "M")]
        [InlineData("rating", "PG-13 but not always")]
        [InlineData("title", "a")]
        [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
        [InlineData("year", 1900)]
        [InlineData("year", 2035)]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Validation(string propName, object propValue)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var original = Server.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            var entity = expected.Clone();

            switch (propName)
            {
                case "duration": entity.Duration = (int)propValue; break;
                case "rating": entity.Rating = (string)propValue; break;
                case "title": entity.Title = (string)propValue; break;
                case "year": entity.Year = (int)propValue; break;
            }

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(entity)).ConfigureAwait(false);
            var stored = Server.GetMovieById(id).Clone();

            // Assert
            Assert.Equal(400, exception.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            Assert.Equal<ITableData>(original, stored);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ThrowsWhenNotFound()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var obj = blackPantherMovie.Clone();
            obj.Id = "not-found";
            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(obj)).ConfigureAwait(false);

            // Assert
            Assert.Equal(404, exception.StatusCode);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ConditionalSuccess()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var original = Server.GetMovieById(id).Clone();
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.ReplaceItemAsync(expected, IfMatch.Version(expected.Version)).ConfigureAwait(false);
            var stored = Server.GetMovieById(id);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.SystemPropertiesMatch(stored, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(TableOperation.Update, modArg.Operation);
            Assert.Equal(response.Value.Id, modArg.Id);
            Assert.Equal<IMovie>(response.Value, modArg.Entity as IMovie);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ConditionalFailure()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var entityCount = Server.GetMovieCount();
            var id = TestData.Movies.GetRandomId();
            var original = Server.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";
            const string etag = "dGVzdA==";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.ReplaceItemAsync(expected, IfMatch.Version(etag))).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(412, exception.StatusCode);
            Assert.Equal(entityCount, Server.GetMovieCount());
            var entity = Server.GetMovieById(id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, exception.ServerItem);
            Assert.Equal<IMovie>(original, exception.ServerItem);
            Assert.Equal<IMovie>(original, entity);
            Assert.Equal<ITableData>(original, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_SoftNotDeleted()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var original = Server.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("soft");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.ReplaceItemAsync(expected).ConfigureAwait(false);
            var stored = Server.GetMovieById(id).Clone();

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.SystemPropertiesMatch(stored, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(TableOperation.Update, modArg.Operation);
            Assert.Equal(response.Value.Id, modArg.Id);
            Assert.Equal<IMovie>(response.Value, modArg.Entity as IMovie);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_SoftDeleted_ReturnsGone()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var original = Server.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("soft");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(expected)).ConfigureAwait(false);
            var stored = Server.GetMovieById(id).Clone();

            // Assert
            Assert.Equal(410, exception.StatusCode);
            Assert.Equal<IMovie>(original, stored);
            Assert.Equal<ITableData>(original, stored);
            Assert.Empty(modifications);
        }
        #endregion

        #region ToAsyncEnumerable
        [Fact]
        [Trait("Method", "ToAsyncEnumerable")]
        public async Task ToAsyncEnumerable_RetrievesItems()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            int count = 0;

            var result = table.ToAsyncEnumerable();

            var enumerator = result.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                count++;
                var item = enumerator.Current;

                Assert.NotNull(item);
                Assert.NotNull(item.Id);

                var expected = Server.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);
            }

            Assert.Equal(Movies.Count, count);
        }
        #endregion

        #region ToAsyncPageable
        [Fact]
        [Trait("Method", "ToAsyncPageable")]
        public async Task ToAsyncPageable_RetrievesItems()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            int count = 0;

            var pageable = table.ToAsyncPageable();
            Assert.NotNull(pageable);

            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                count++;
                var item = enumerator.Current;

                Assert.NotNull(item);
                Assert.NotNull(item.Id);

                var expected = Server.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);

                Assert.Equal(Movies.Count, pageable.Count);
                Assert.NotNull(pageable.CurrentResponse);
            }

            Assert.Equal(Movies.Count, count);
        }

        [Fact]
        [Trait("Method", "ToAsyncPageable")]
        public async Task ToAsyncPageable_AsPages_RetrievesItems()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            int itemCount = 0, pageCount = 0;

            var pageable = table.ToAsyncPageable().AsPages();
            Assert.NotNull(pageable);

            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                pageCount++;

                var page = enumerator.Current;
                Assert.NotNull(page);
                foreach (var item in page.Items)
                {
                    itemCount++;
                    Assert.NotNull(item.Id);
                    var expected = Server.GetMovieById(item.Id);
                    Assert.Equal<IMovie>(expected, item);
                }
            }

            Assert.Equal(Movies.Count, itemCount);
            Assert.Equal(3, pageCount);
        }
        #endregion

        #region ToLazyObservableCollection
        [Fact]
        [Trait("Method", "ToLazyObservableCollection")]
        public async Task ToLazyObservableCollection_LoadsData()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            int loops = 0;
            const int maxLoops = (Movies.Count / 20) + 1;

            // Act
            var sut = table.ToLazyObservableCollection() as InternalLazyObservableCollection<ClientMovie>;
            var loadMore = sut.LoadMoreCommand as IAsyncCommand;
            await WaitUntil(() => !sut.IsBusy).ConfigureAwait(false);
            while (loops < maxLoops && sut.HasMoreItems)
            {
                loops++;
                await loadMore.ExecuteAsync().ConfigureAwait(false);
            }

            Assert.False(sut.HasMoreItems);
            Assert.Equal(Movies.Count, sut.Count);
        }

        [Fact]
        [Trait("Method", "ToLazyObservableCollection")]
        public async Task ToLazyObservableCollection_WtithPageCount_LoadsData()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            int loops = 0;
            const int maxLoops = (Movies.Count / 50) + 1;

            // Act
            var sut = table.ToLazyObservableCollection(50) as InternalLazyObservableCollection<ClientMovie>;
            var loadMore = sut.LoadMoreCommand as IAsyncCommand;
            await WaitUntil(() => !sut.IsBusy).ConfigureAwait(false);
            while (loops < maxLoops && sut.HasMoreItems)
            {
                loops++;
                await loadMore.ExecuteAsync().ConfigureAwait(false);
            }

            Assert.False(sut.HasMoreItems);
            Assert.Equal(Movies.Count, sut.Count);
        }
        #endregion

        #region UpdateItemAsync
        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ThrowsOnNullId()
        {
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.UpdateItemAsync(null, EntityUpdates)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ThrowsOnNullUpdates()
        {
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.UpdateItemAsync("id-107", null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ThrowsOnEmptyUpdates()
        {
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
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
            var client = CreateClientForTestServer();
            var table = client.GetTable<ClientMovie>("movies");
            await Assert.ThrowsAsync<ArgumentException>(() => table.UpdateItemAsync(id, EntityUpdates)).ConfigureAwait(false);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_Success_FormulatesCorrectResponse(bool hasPrecondition)
        {
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

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
            MockHandler.AddResponse(HttpStatusCode.OK, payload);
            var client = GetMockClient(new TestAuthenticationProvider(ValidAuthenticationToken));
            var table = client.GetTable<IdEntity>("movies");

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
            MockHandler.AddResponse(statusCode, payload);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

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
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

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
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            MockHandler.Responses.Add(response);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

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
            MockHandler.AddResponse(statusCode);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

            var ex = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.UpdateItemAsync(sId, changes)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_SuccessNoContent_Throws()
        {
            MockHandler.AddResponse(HttpStatusCode.OK);
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>("movies");

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
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_Basic()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var original = Server.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.UpdateItemAsync(id, EntityUpdates).ConfigureAwait(false);
            var stored = Server.GetMovieById(id);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.SystemPropertiesMatch(stored, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(TableOperation.Update, modArg.Operation);
            Assert.Equal(response.Value.Id, modArg.Id);
            Assert.Equal<IMovie>(response.Value, modArg.Entity as IMovie);
        }

        [Theory]
        [InlineData("duration", 50)]
        [InlineData("duration", 370)]
        [InlineData("rating", "M")]
        [InlineData("rating", "PG-13 but not always")]
        [InlineData("title", "a")]
        [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
        [InlineData("year", 1900)]
        [InlineData("year", 2035)]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_Validation(string propName, object propValue)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var original = Server.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            var entity = expected.Clone();

            switch (propName)
            {
                case "duration": entity.Duration = (int)propValue; break;
                case "rating": entity.Rating = (string)propValue; break;
                case "title": entity.Title = (string)propValue; break;
                case "year": entity.Year = (int)propValue; break;
            }

            IReadOnlyDictionary<string, object> updates = new Dictionary<string, object>()
            {
                { propName, propValue }
            };

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.UpdateItemAsync(id, updates)).ConfigureAwait(false);
            var stored = Server.GetMovieById(id).Clone();

            // Assert
            Assert.Equal(400, exception.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            Assert.Equal<ITableData>(original, stored);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ThrowsWhenNotFound()
        {
            // Arrange
            var client = CreateClientForTestServer();
            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.UpdateItemAsync("not-found", EntityUpdates)).ConfigureAwait(false);

            // Assert
            Assert.Equal(404, exception.StatusCode);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ConditionalSuccess()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var original = Server.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.UpdateItemAsync(id, EntityUpdates, IfMatch.Version(expected.Version)).ConfigureAwait(false);
            var stored = Server.GetMovieById(id);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.SystemPropertiesMatch(stored, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(TableOperation.Update, modArg.Operation);
            Assert.Equal(response.Value.Id, modArg.Id);
            Assert.Equal<IMovie>(response.Value, modArg.Entity as IMovie);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_ConditionalFailure()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var entityCount = Server.GetMovieCount();
            var id = TestData.Movies.GetRandomId();
            var original = Server.GetMovieById(id).Clone();
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";
            const string etag = "dGVzdA==";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.UpdateItemAsync(id, EntityUpdates, IfMatch.Version(etag))).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(412, exception.StatusCode);
            Assert.Equal(entityCount, Server.GetMovieCount());
            var entity = Server.GetMovieById(id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, exception.ServerItem);
            Assert.Equal<IMovie>(original, exception.ServerItem);
            Assert.Equal<IMovie>(original, entity);
            Assert.Equal<ITableData>(original, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_SoftNotDeleted()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            var original = Server.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("soft");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.UpdateItemAsync(id, EntityUpdates).ConfigureAwait(false);
            var stored = Server.GetMovieById(id).Clone();

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.SystemPropertiesMatch(stored, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(TableOperation.Update, modArg.Operation);
            Assert.Equal(response.Value.Id, modArg.Id);
            Assert.Equal<IMovie>(response.Value, modArg.Entity as IMovie);
        }

        [Fact]
        [Trait("Method", "UpdateItemAsync")]
        public async Task UpdateItemAsync_SoftDeleted_ReturnsGone()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var id = TestData.Movies.GetRandomId();
            await Server.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);
            var original = Server.GetMovieById(id).Clone();
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("soft");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.UpdateItemAsync(id, EntityUpdates)).ConfigureAwait(false);
            var stored = Server.GetMovieById(id).Clone();

            // Assert
            Assert.Equal(410, exception.StatusCode);
            Assert.Equal<IMovie>(original, stored);
            Assert.Equal<ITableData>(original, stored);
            Assert.Empty(modifications);
        }
        #endregion

        #region IncludeDeletedItems
        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Enabled_AddsKey()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.IncludeDeletedItems(true) as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("__includedeleted", "true", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Disabled_Empty()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.IncludeDeletedItems(false) as DatasyncTableQuery<IdEntity>;
            Assert.Empty(query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "IncludeDeletedItems")]
        public void ToODataQueryString_IncludeDeletedItems_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.IncludeDeletedItems() as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("__includedeleted=true", odata);
        }
        #endregion

        #region IncludeTotalCount
        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Enabled_AddsKey()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.IncludeTotalCount(true) as DatasyncTableQuery<IdEntity>;
            AssertEx.Contains("$count", "true", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Disabled_WorksWithEmptyParameters()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.IncludeTotalCount(false) as DatasyncTableQuery<IdEntity>;
            Assert.False(query.QueryParameters.ContainsKey("$count"));
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "IncludeTotalCount")]
        public void ToOdataQueryString_IncludeTotalCount_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.IncludeTotalCount() as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$count=true", odata);
        }
        #endregion

        #region OrderBy
        [Fact]
        [Trait("Method", "OrderBy")]
        public void OrderBy_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => table.OrderBy(keySelector));
        }

        [Fact]
        [Trait("Method", "OrderBy")]
        public void OrderBy_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.OrderBy(m => m.Id) as DatasyncTableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("OrderBy", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderBy")]
        public void ToODataQueryString_OrderBy_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.OrderBy(m => m.Id) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$orderby=id", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderBy")]
        public void ToODataQueryString_OrderBy_ThrowsNotSupported()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.OrderBy(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
        #endregion

        #region OrderByDescending
        [Fact]
        [Trait("Method", "OrderByDescending")]
        public void OrderByDescending_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => table.OrderByDescending(keySelector));
        }

        [Fact]
        [Trait("Method", "OrderByDescending")]
        public void OrderByDescending_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.OrderByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("OrderByDescending", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderByDescending")]
        public void ToODataQueryString_OrderByDescending_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.OrderByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$orderby=id desc", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderByDescending")]
        public void ToODataQueryString_OrderByDescending_ThrowsNotSupported()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.OrderByDescending(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
        #endregion

        #region Select
        [Fact]
        [Trait("Method", "Select")]
        public void Select_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            Expression<Func<IdEntity, IdOnly>> selector = null;
            Assert.Throws<ArgumentNullException>(() => table.Select(selector));
        }

        [Fact]
        [Trait("Method", "Select")]
        public void Select_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.Select(m => new IdOnly { Id = m.Id }) as DatasyncTableQuery<IdOnly>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("Select", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Select")]
        public void ToODataQueryString_Select_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.Select(m => new IdOnly { Id = m.Id }) as DatasyncTableQuery<IdOnly>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$select=id", odata);
        }
        #endregion

        #region Skip
        [Theory, CombinatorialData]
        [Trait("Method", "Skip")]
        public void Skip_Throws_OutOfRange([CombinatorialValues(-10, -1)] int skip)
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            Assert.Throws<ArgumentOutOfRangeException>(() => table.Skip(skip));
        }

        [Fact]
        [Trait("Method", "Skip")]
        public void Skip_Sets_SkipCount()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.Skip(5) as DatasyncTableQuery<IdEntity>;
            Assert.Equal(5, query.SkipCount);
        }

        [Fact]
        [Trait("Method", "Skip")]
        public void Skip_IsCumulative()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.Skip(5).Skip(20) as DatasyncTableQuery<IdEntity>;
            Assert.Equal(25, query.SkipCount);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Skip")]
        public void ToODataQueryString_Skip_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.Skip(5) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$skip=5", odata);
        }
        #endregion

        #region Take
        [Theory, CombinatorialData]
        [Trait("Method", "Take")]
        public void Take_ThrowsOutOfRange([CombinatorialValues(-10, -1, 0)] int take)
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

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
            var table = client.GetTable<IdEntity>();

            var query = table.Take(first).Take(second) as DatasyncTableQuery<IdEntity>;
            Assert.Equal(expected, query.TakeCount);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Take")]
        public void ToODataQueryString_Take_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.Take(5) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$top=5", odata);
        }
        #endregion

        #region ThenBy
        [Fact]
        [Trait("Method", "ThenBy")]
        public void ThenBy_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => table.ThenBy(keySelector));
        }

        [Fact]
        [Trait("Method", "ThenBy")]
        public void ThenBy_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.ThenBy(m => m.Id) as DatasyncTableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("ThenBy", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenBy")]
        public void ToODataQueryString_ThenBy_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.ThenBy(m => m.Id) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$orderby=id", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenBy")]
        public void ToODataQueryString_ThenBy_ThrowsNotSupported()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.ThenBy(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
        #endregion

        #region ThenByDescending
        [Fact]
        [Trait("Method", "ThenByDescending")]
        public void ThenByDescending_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => table.ThenByDescending(keySelector));
        }

        [Fact]
        [Trait("Method", "ThenByDescending")]
        public void ThenByDescending_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.ThenByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("ThenByDescending", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenByDescending")]
        public void ToODataQueryString_ThenByDescending_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.ThenByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$orderby=id desc", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenByDescending")]
        public void ToODataQueryString_ThenByDescending_ThrowsNotSupported()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.ThenByDescending(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
        #endregion

        #region Where
        [Fact]
        [Trait("Method", "Where")]
        public void Where_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();
            Expression<Func<IdEntity, bool>> predicate = null;
            Assert.Throws<ArgumentNullException>(() => table.Where(predicate));
        }

        [Fact]
        [Trait("Method", "Where")]
        public void Where_UpdatesQuery()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.Where(m => m.Id.Contains("foo")) as DatasyncTableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(query.Query.Expression);
            var expression = query.Query.Expression as MethodCallExpression;
            Assert.Equal("Where", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Where")]
        public void ToODataQueryString_Where_IsWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.Where(m => m.Id == "foo") as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$filter=(id%20eq%20'foo')", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Where")]
        public void ToODataQueryString_Where_ThrowsNotSupported()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.ThenByDescending(m => m.Id.Normalize() == "foo") as DatasyncTableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
        #endregion

        #region WithParameter
        [Theory]
        [InlineData(null, "test")]
        [InlineData("test", null)]
        [Trait("Method", "WithParameter")]
        public void WithParameter_Null_Throws(string key, string value)
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

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
            var table = client.GetTable<IdEntity>();

            Assert.Throws<ArgumentException>(() => table.WithParameter(key, value));
        }

        [Fact]
        [Trait("Method", "WithParameter")]
        public void WithParameter_SetsParameter()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.WithParameter("testkey", "testvalue") as DatasyncTableQuery<IdEntity>;
            AssertEx.Contains("testkey", "testvalue", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "WithParameter")]
        public void WithParameter_Overwrites()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.WithParameter("testkey", "testvalue");
            var actual = query.WithParameter("testkey", "replacement") as DatasyncTableQuery<IdEntity>;
            AssertEx.Contains("testkey", "replacement", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "WithParameter")]
        public void ToODataQueryString_WithParameter_isWellFormed()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.WithParameter("testkey", "testvalue") as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("testkey=testvalue", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "WithParameter")]
        public void ToODataQueryString_WithParameter_EncodesValue()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.WithParameter("testkey", "test value") as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("testkey=test%20value", odata);
        }
        #endregion

        #region WithParameters
        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            Assert.Throws<ArgumentNullException>(() => table.WithParameters(null));
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_Empty_Throws()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();
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
            var table = client.GetTable<IdEntity>();

            var query = table.WithParameters(sut) as DatasyncTableQuery<IdEntity>;
            AssertEx.Contains("key1", "value1", query.QueryParameters);
            AssertEx.Contains("key2", "value2", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_MergesParams()
        {
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.WithParameter("key1", "value1");
            var sut = new Dictionary<string, string>()
            {
                { "key1", "replacement" },
                { "key2", "value2" }
            };

            var actual = query.WithParameters(sut) as DatasyncTableQuery<IdEntity>;
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
            var table = client.GetTable<IdEntity>();

            var sut = new Dictionary<string, string>()
            {
                { key, "true" },
                { "key2", "value2" }
            };

            Assert.Throws<ArgumentException>(() => table.WithParameters(sut));
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "WithParameters")]
        public void ToODataQueryString_WithParameters_isWellFormed()
        {
            var pairs = new Dictionary<string, string>()
            {
                {  "key1", "value1" },
                {  "key2", "value 2" }
            };
            var client = GetMockClient();
            var table = client.GetTable<IdEntity>();

            var query = table.WithParameters(pairs) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("key1=value1&key2=value%202", odata);
        }
        #endregion
    }
}

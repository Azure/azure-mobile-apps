// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table.Operations
{
    [ExcludeFromCodeCoverage]
    public class DeleteItemAsync_Tests : BaseTest
    {
        private readonly IdEntity payload = new() { Id = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f", StringValue = "test" };
        private const string sBadJson = "{this-is-bad-json";

        private readonly IRemoteTable table, authTable;
        private readonly IdOnly idOnly;
        private readonly IdEntity idEntity;
        private readonly string sId, expectedEndpoint;

        public DeleteItemAsync_Tests() : base()
        {
            sId = Guid.NewGuid().ToString("N");
            idOnly = new IdOnly { Id = sId };
            idEntity = new IdEntity { Id = sId, Version = "etag" };
            expectedEndpoint = new Uri(Endpoint, $"/tables/movies/{sId}").ToString();

            table = GetMockClient().GetRemoteTable("movies");
            authTable = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken)).GetRemoteTable("movies");
        }

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
            _ = await table.DeleteItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            Assert.False(request.Headers.Contains("If-Match"));
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_FormsCorrectResponse_WithPrecondition()
        {
            var json = CreateJsonDocument(idEntity);
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            _ = await table.DeleteItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Auth_NoPrecondition()
        {
            var json = CreateJsonDocument(idOnly);
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            _ = await authTable.DeleteItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            Assert.False(request.Headers.Contains("If-Match"));
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Auth_WithPrecondition()
        {
            var json = CreateJsonDocument(idEntity);
            MockHandler.AddResponse(HttpStatusCode.NoContent);
            _ = await authTable.DeleteItemAsync(json).ConfigureAwait(false);

            // Check Request
            Assert.Single(MockHandler.Requests);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(expectedEndpoint, request.RequestUri.ToString());
            AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            var json = CreateJsonDocument(idEntity);
            MockHandler.AddResponse(statusCode, payload);
            var ex = await Assert.ThrowsAsync<DatasyncConflictException>(() => table.DeleteItemAsync(json)).ConfigureAwait(false);

            // Check Response
            Assert.Equal(statusCode, ex.Response.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
            Assert.NotNull(ex.Value);
            Assert.Equal(payload.Id, ex.Value.Value<string>("id"));
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConflictNoContent_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            var json = CreateJsonDocument(idEntity);
            MockHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.DeleteItemAsync(json)).ConfigureAwait(false);

            // Check Response
            Assert.Equal(statusCode, ex.Response.StatusCode);
            Assert.False(ex.Response.IsSuccessStatusCode);
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
            await Assert.ThrowsAnyAsync<JsonException>(() => table.DeleteItemAsync(json)).ConfigureAwait(false);
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
            var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.DeleteItemAsync(json)).ConfigureAwait(false);
            Assert.Equal(statusCode, ex.Response.StatusCode);
        }
    }
}

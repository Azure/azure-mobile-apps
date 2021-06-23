// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Extensions
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class IDatasyncTableExtensions_Tests : BaseTest
    {
        private const string sEndpoint = "https://foo.azurewebsites.net/tables/movies/";
        private readonly IdEntity payload = new() { Id = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f", Version = "etag", StringValue = "test" };
        private const string sJsonPayload = "{\"id\":\"db0ec08d-46a9-465d-9f5e-0066a3ee5b5f\",\"version\":\"etag\",\"stringValue\":\"test\"}";
        private const string sBadJson = "{this-is-bad-json";
        private const string sId = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f";

        #region DeleteItemIfUnchangedAsync
        [Fact]
        [Trait("Method", "DeleteItemIfUnchangedAsync")]
        public async Task DeleteItemIfUnchangedAsync_NullItem_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.DeleteItemIfUnchangedAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "DeleteItemIfUnchangedAsync")]
        public async Task DeleteItemIfUnchangedAsync_NullVersion_Throws()
        {
            var item = new IdEntity { Id = sId, Version = null };
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.DeleteItemIfUnchangedAsync(item)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "DeleteItemIfUnchangedAsync")]
        public async Task DeleteItemIfUnchangedAsync_NullId_Throws()
        {
            var item = new IdEntity { Id = null, Version = "etag" };
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.DeleteItemIfUnchangedAsync(item)).ConfigureAwait(false);
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
        [Trait("Method", "DeleteItemIfUnchangedAsync")]
        public async Task DeleteItemIfUnchangedAsync_InvalidId_Throws(string id)
        {
            var item = new IdEntity { Id = id, Version = "etag" };
            await Assert.ThrowsAsync<ArgumentException>(() => Table.DeleteItemIfUnchangedAsync(item)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "DeleteItemIfUnchangedAsync")]
        public async Task DeleteItemIfUnchangedAsync_Success_FormulatesCorrectResponse()
        {
            ClientHandler.AddResponse(HttpStatusCode.NoContent);

            var response = await Table.DeleteItemIfUnchangedAsync(payload).ConfigureAwait(false);

            // Check Request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
            AssertEx.HasValue("If-Match", new[] { "\"etag\"" }, request.Headers);

            // Check Response
            Assert.Equal(204, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.IsConflictStatusCode);
            Assert.False(response.HasContent);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "DeleteItemIfUnchangedAsync")]
        public async Task DeleteItemIfUnchangedAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode, payload);

            var ex = await Assert.ThrowsAsync<ConflictException<IdEntity>>(() => Table.DeleteItemIfUnchangedAsync(payload)).ConfigureAwait(false);

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
        [Trait("Method", "DeleteItemIfUnchangedAsync")]
        public async Task DeleteItemIfUnchangedAsync_ConflictNoContent_throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.DeleteItemIfUnchangedAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "DeleteItemIfUnchangedAsync")]
        public async Task DeleteItemIfUnchangedAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            ClientHandler.Responses.Add(response);

            await Assert.ThrowsAsync<JsonException>(() => Table.DeleteItemIfUnchangedAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "DeleteItemIfUnchangedAsync")]
        public async Task DeleteItemIfUnchangedAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);

            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.DeleteItemIfUnchangedAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }
        #endregion

        #region RefreshItemAsync
        [Fact]
        [Trait("Method", "RefreshItemAsync")]
        public async Task RefreshItemAsync_NullItem_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.RefreshItemAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "RefreshItemAsync")]
        public async Task RefreshItemAsync_NullVersion_Throws()
        {
            var item = new IdEntity { Id = sId, Version = null };
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.RefreshItemAsync(item)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "RefreshItemAsync")]
        public async Task RefreshItemAsync_NullId_Throws()
        {
            var item = new IdEntity { Id = null, Version = "etag" };
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.RefreshItemAsync(item)).ConfigureAwait(false);
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
        [Trait("Method", "RefreshItemAsync")]
        public async Task RefreshItemAsync_InvalidId_Throws(string id)
        {
            var item = new IdEntity { Id = id, Version = "etag" };
            await Assert.ThrowsAsync<ArgumentException>(() => Table.RefreshItemAsync(item)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "RefreshItemAsync")]
        public async Task RefreshItemAsync_Success_FormulatesCorrectResponse()
        {
            ClientHandler.AddResponse(HttpStatusCode.OK, payload);
            var item = new IdEntity { Id = sId, Version = "etag" };

            var response = await Table.RefreshItemAsync(item).ConfigureAwait(false);

            // Check Request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}{sId}", request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
            AssertEx.HasValue("If-None-Match", new[] { "\"etag\"" }, request.Headers);

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
        [Trait("Method", "RefreshItemAsync")]
        public async Task RefreshItemAsync_SuccessNoContent_Throws()
        {
            ClientHandler.AddResponse(HttpStatusCode.OK);
            await Assert.ThrowsAsync<RequestFailedException>(() => Table.RefreshItemAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "RefreshItemAsync")]
        public async Task RefreshItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);

            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.RefreshItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Fact]
        [Trait("Method", "GetIRefreshItemAsynctemAsync")]
        public async Task RefreshItemAsync_NotModified_Throws()
        {
            ClientHandler.AddResponse(HttpStatusCode.NotModified);

            var ex = await Assert.ThrowsAsync<NotModifiedException>(() => Table.RefreshItemAsync(payload)).ConfigureAwait(false);
            Assert.Equal(304, ex.StatusCode);
        }
        #endregion

        #region ReplaceItemIfUnchangedAsync
        [Fact]
        [Trait("Method", "ReplaceItemIfUnchangedAsync")]
        public async Task ReplaceItemIfUnchangedAsync_NullItem_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.ReplaceItemIfUnchangedAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "ReplaceItemIfUnchangedAsync")]
        public async Task ReplaceItemIfUnchangedAsync_NullVersion_Throws()
        {
            var item = new IdEntity { Id = sId, Version = null };
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.ReplaceItemIfUnchangedAsync(item)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "ReplaceItemIfUnchangedAsync")]
        public async Task ReplaceItemIfUnchangedAsync_NullId_Throws()
        {
            var item = new IdEntity { Id = null, Version = "etag" };
            await Assert.ThrowsAsync<ArgumentNullException>(() => Table.ReplaceItemIfUnchangedAsync(item)).ConfigureAwait(false);
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
        [Trait("Method", "ReplaceItemIfUnchangedAsync")]
        public async Task ReplaceItemIfUnchangedAsync_InvalidId_Throws(string id)
        {
            var item = new IdEntity { Id = id, Version = "etag" };
            await Assert.ThrowsAsync<ArgumentException>(() => Table.ReplaceItemIfUnchangedAsync(item)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "ReplaceItemIfUnchangedAsync")]
        public async Task ReplaceItemIfUnchangedAsync_Success_FormulatesCorrectResponse()
        {
            ClientHandler.AddResponse(HttpStatusCode.OK, payload);

            var response = await Table.ReplaceItemIfUnchangedAsync(payload).ConfigureAwait(false);

            // Check Request
            Assert.Single(ClientHandler.Requests);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal($"{sEndpoint}{payload.Id}", request.RequestUri.ToString());
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, request.Headers);
            AssertEx.HasValue("If-Match", new[] { "\"etag\"" }, request.Headers);

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
        [Trait("Method", "ReplaceItemIfUnchangedAsync")]
        public async Task ReplaceItemIfUnchangedAsync_SuccessNoContent_Throws()
        {
            ClientHandler.AddResponse(HttpStatusCode.OK);

            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.ReplaceItemIfUnchangedAsync(payload)).ConfigureAwait(false);
            Assert.Equal(200, ex.StatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "ReplaceItemIfUnchangedAsync")]
        public async Task ReplaceItemIfUnchangedAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode, payload);

            var ex = await Assert.ThrowsAsync<ConflictException<IdEntity>>(() => Table.ReplaceItemIfUnchangedAsync(payload)).ConfigureAwait(false);

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
        [Trait("Method", "ReplaceItemIfUnchangedAsync")]
        public async Task ReplaceItemIfUnchangedAsync_ConflictNoContent_throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);
            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.ReplaceItemIfUnchangedAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "ReplaceItemIfUnchangedAsync")]
        public async Task ReplaceItemIfUnchangedAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
            };
            ClientHandler.Responses.Add(response);

            await Assert.ThrowsAsync<JsonException>(() => Table.ReplaceItemIfUnchangedAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "ReplaceItemIfUnchangedAsync")]
        public async Task ReplaceItemIfUnchangedAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            ClientHandler.AddResponse(statusCode);

            var ex = await Assert.ThrowsAsync<RequestFailedException>(() => Table.ReplaceItemIfUnchangedAsync(payload)).ConfigureAwait(false);
            Assert.Equal((int)statusCode, ex.StatusCode);
        }
        #endregion
    }
}

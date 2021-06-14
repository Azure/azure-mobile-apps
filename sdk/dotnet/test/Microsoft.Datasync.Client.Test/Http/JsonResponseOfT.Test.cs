// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class JsonResponseOfT_Tests
    {
        [Fact]
        public void Ctor_Throws_WhenNull()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new JsonResponse<MockObject>(null));
        }

        [Fact]
        public void Ctor_IsCreated_WhenGoodResponse()
        {
            var sut = new JsonResponse<MockObject>(new HttpResponseMessage(HttpStatusCode.NoContent));
            Assert.NotNull(sut);
            Assert.Equal(204, sut.StatusCode);
            Assert.True(sut.IsSuccessStatusCode);
            Assert.Empty(sut.Content);
            Assert.False(sut.HasContent);
            Assert.Empty(sut.Headers);
            Assert.False(sut.HasValue);
            Assert.Null(sut.Value);
        }

        [Fact]
        public void Ctor_IsCreated_WhenGoodResponse_WithHeaders()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { ReasonPhrase = "Test phrase" };
            response.Headers.Add("X-ZUMO-CHECK", "true");
            var sut = new JsonResponse<MockObject>(response);
            Assert.NotNull(sut);
            Assert.Equal(200, sut.StatusCode);
            Assert.True(sut.IsSuccessStatusCode);
            Assert.Equal("Test phrase", sut.ReasonPhrase);
            Assert.Empty(sut.Content);
            Assert.False(sut.HasContent);
            Assert.Single(sut.Headers);
            AssertEx.HasValue("X-ZUMO-CHECK", new string[] { "true" }, sut.Headers);
        }

        [Theory]
        [InlineData(HttpStatusCode.Continue, 100, false)]
        [InlineData(HttpStatusCode.OK, 200, true)]
        [InlineData(HttpStatusCode.Created, 201, true)]
        [InlineData(HttpStatusCode.NoContent, 204, true)]
        [InlineData(HttpStatusCode.NotModified, 304, false)]
        [InlineData(HttpStatusCode.BadRequest, 400, false)]
        [InlineData(HttpStatusCode.Unauthorized, 401, false)]
        [InlineData(HttpStatusCode.NotFound, 404, false)]
        [InlineData(HttpStatusCode.Conflict, 409, false)]
        [InlineData(HttpStatusCode.PreconditionFailed, 412, false)]
        [InlineData(HttpStatusCode.InternalServerError, 500, false)]
        public void IsSuccessStatusCode_IsSet_OnStatusCode(HttpStatusCode actual, int expectedStatusCode, bool expectedSuccess)
        {
            var sut = new JsonResponse<MockObject>(new HttpResponseMessage(actual));
            Assert.Equal(expectedStatusCode, sut.StatusCode);
            Assert.Equal(expectedSuccess, sut.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ReadContentAsync_SetsContent_WithNoContent()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var sut = new JsonResponse<MockObject>(response);
            var actual = await sut.ReadContentAsync(response.Content).ConfigureAwait(false);
            Assert.Same(sut, actual);
            Assert.False(actual.HasContent);
            Assert.Empty(actual.Content);
        }

        [Fact]
        public async Task ReadContentAsync_SetsContent_WithContent()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("abcdef") };
            var sut = new JsonResponse<MockObject>(response);
            var actual = await sut.ReadContentAsync(response.Content).ConfigureAwait(false);
            Assert.Same(sut, actual);
            Assert.True(actual.HasContent);
            Assert.Equal(6, actual.Content.Length);
            Assert.Equal("abcdef", Encoding.UTF8.GetString(actual.Content));
        }

        [Fact]
        public async Task DeserializeContentAsync_SetsValue_WithNoContent()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var options = new DatasyncClientOptions();
            var sut = new JsonResponse<MockObject>(response);
            var actual = await sut.DeserializeContentAsync(response.Content, options.DeserializerOptions).ConfigureAwait(false);
            Assert.Same(sut, actual);
            Assert.False(actual.HasValue);
            Assert.Null(actual.Value);
        }

        [Fact]
        public async Task DeserializeContentAsync_SetsValue_WithContent()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"stringValue\":\"test\"}", Encoding.UTF8, "application/json")
            };
            var options = new DatasyncClientOptions();
            var sut = new JsonResponse<MockObject>(response);
            var actual = await sut.DeserializeContentAsync(response.Content, options.DeserializerOptions).ConfigureAwait(false);
            Assert.Same(sut, actual);
            Assert.True(actual.HasValue);
            Assert.NotNull(actual.Value);
            Assert.Equal("test", actual.Value?.StringValue);
        }

        [Fact]
        public async Task DeserializeContentAsync_Throws_WithInvalidJson()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("\"stringValue\":\"test\"}", Encoding.UTF8, "application/json")
            };
            var options = new DatasyncClientOptions();
            var sut = new JsonResponse<MockObject>(response);
            await Assert.ThrowsAsync<JsonException>(() => sut.DeserializeContentAsync(response.Content, options.DeserializerOptions)).ConfigureAwait(false);
        }

        [Fact]
        public async Task ImplicitValue_ReturnsValue_WhenNull()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var options = new DatasyncClientOptions();
            var sut = new JsonResponse<MockObject>(response);
            MockObject actual = await sut.DeserializeContentAsync(response.Content, options.DeserializerOptions).ConfigureAwait(false);
            Assert.Null(actual);
        }

        [Fact]
        public async Task ImplicitValue_ReturnsValue_WhenNotNull()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"stringValue\":\"test\"}", Encoding.UTF8, "application/json")
            };
            var options = new DatasyncClientOptions();
            var sut = new JsonResponse<MockObject>(response);
            MockObject actual = await sut.DeserializeContentAsync(response.Content, options.DeserializerOptions).ConfigureAwait(false);
            Assert.NotNull(actual);
            Assert.Equal("test", actual.StringValue);
        }
    }
}

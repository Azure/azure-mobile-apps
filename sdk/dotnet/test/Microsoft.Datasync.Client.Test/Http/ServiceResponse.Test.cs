// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage]
    public class ServiceResponse_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "ServiceResponse.FromResponseAsync")]
        public async Task FromResponseAsync_Null_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => ServiceResponse.FromResponseAsync(null)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.Continue, false, false, false)]
        [InlineData(HttpStatusCode.OK, false, true, false)]
        [InlineData(HttpStatusCode.Created, false, true, false)]
        [InlineData(HttpStatusCode.NoContent, false, true, false)]
        [InlineData(HttpStatusCode.BadRequest, false, false, false)]
        [InlineData(HttpStatusCode.Unauthorized, false, false, false)]
        [InlineData(HttpStatusCode.Forbidden, false, false, false)]
        [InlineData(HttpStatusCode.MethodNotAllowed, false, false, false)]
        [InlineData(HttpStatusCode.Conflict, false, false, true)]
        [InlineData(HttpStatusCode.PreconditionFailed, false, false, true)]
        [InlineData(HttpStatusCode.InternalServerError, false, false, false)]
        [InlineData(HttpStatusCode.Continue, true, false, false)]
        [InlineData(HttpStatusCode.OK, true, true, false)]
        [InlineData(HttpStatusCode.Created, true, true, false)]
        [InlineData(HttpStatusCode.NoContent, true, true, false)]
        [InlineData(HttpStatusCode.BadRequest, true, false, false)]
        [InlineData(HttpStatusCode.Unauthorized, true, false, false)]
        [InlineData(HttpStatusCode.Forbidden, true, false, false)]
        [InlineData(HttpStatusCode.MethodNotAllowed, true, false, false)]
        [InlineData(HttpStatusCode.Conflict, true, false, true)]
        [InlineData(HttpStatusCode.PreconditionFailed, true, false, true)]
        [InlineData(HttpStatusCode.InternalServerError, true, false, false)]
        [Trait("Method", "ServiceResponse.FromResponseAsync")]
        public async Task FromResponseAsync_NoHeaders(HttpStatusCode statusCode, bool hasContent, bool isSuccessful, bool isConflict)
        {
            // Arrange
            const string jsonPayload = "{\"stringValue\":\"test\"}";
            var sut = new HttpResponseMessage(statusCode);
            if (hasContent)
            {
                sut.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }

            // Act
            var response = await ServiceResponse.FromResponseAsync(sut).ConfigureAwait(false);

            // Assert
            Assert.Equal(hasContent, response.HasContent);
            if (hasContent)
            {
                Assert.Equal(jsonPayload, Encoding.UTF8.GetString(response.Content));
                AssertEx.Contains("Content-Type", "application/json; charset=utf-8", response.Headers);
            }
            else
            {
                Assert.Empty(response.Content);
            }
            Assert.Equal(isConflict, response.IsConflictStatusCode);
            Assert.Equal(isSuccessful, response.IsSuccessStatusCode);
            Assert.NotNull(response.ReasonPhrase);
            Assert.Equal((int)statusCode, response.StatusCode);
            Assert.NotNull(response.Version);
        }

        [Theory]
        [InlineData(HttpStatusCode.Continue, false, false, false)]
        [InlineData(HttpStatusCode.OK, false, true, false)]
        [InlineData(HttpStatusCode.Created, false, true, false)]
        [InlineData(HttpStatusCode.NoContent, false, true, false)]
        [InlineData(HttpStatusCode.BadRequest, false, false, false)]
        [InlineData(HttpStatusCode.Unauthorized, false, false, false)]
        [InlineData(HttpStatusCode.Forbidden, false, false, false)]
        [InlineData(HttpStatusCode.MethodNotAllowed, false, false, false)]
        [InlineData(HttpStatusCode.Conflict, false, false, true)]
        [InlineData(HttpStatusCode.PreconditionFailed, false, false, true)]
        [InlineData(HttpStatusCode.InternalServerError, false, false, false)]
        [InlineData(HttpStatusCode.Continue, true, false, false)]
        [InlineData(HttpStatusCode.OK, true, true, false)]
        [InlineData(HttpStatusCode.Created, true, true, false)]
        [InlineData(HttpStatusCode.NoContent, true, true, false)]
        [InlineData(HttpStatusCode.BadRequest, true, false, false)]
        [InlineData(HttpStatusCode.Unauthorized, true, false, false)]
        [InlineData(HttpStatusCode.Forbidden, true, false, false)]
        [InlineData(HttpStatusCode.MethodNotAllowed, true, false, false)]
        [InlineData(HttpStatusCode.Conflict, true, false, true)]
        [InlineData(HttpStatusCode.PreconditionFailed, true, false, true)]
        [InlineData(HttpStatusCode.InternalServerError, true, false, false)]
        [Trait("Method", "ServiceResponse.FromResponseAsync")]
        public async Task FromResponseAsync_WithHeaders(HttpStatusCode statusCode, bool hasContent, bool isSuccessful, bool isConflict)
        {
            // Arrange
            const string jsonPayload = "{\"stringValue\":\"test\"}";
            var sut = new HttpResponseMessage(statusCode);
            if (hasContent)
            {
                sut.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }
            sut.Headers.Add("ETag", "\"42\"");
            sut.Headers.Add("ZUMO-API-VERSION", "3.0.0");

            // Act
            var response = await ServiceResponse.FromResponseAsync(sut).ConfigureAwait(false);

            // Assert
            Assert.Equal(hasContent, response.HasContent);
            if (hasContent)
            {
                Assert.Equal(jsonPayload, Encoding.UTF8.GetString(response.Content));
                AssertEx.Contains("Content-Type", "application/json; charset=utf-8", response.Headers);
            }
            else
            {
                Assert.Empty(response.Content);
            }
            Assert.Equal(isConflict, response.IsConflictStatusCode);
            Assert.Equal(isSuccessful, response.IsSuccessStatusCode);
            Assert.NotNull(response.ReasonPhrase);
            Assert.Equal((int)statusCode, response.StatusCode);
            Assert.NotNull(response.Version);
            AssertEx.Contains("ETag", "\"42\"", response.Headers);
            AssertEx.Contains("ZUMO-API-VERSION", "3.0.0", response.Headers);
        }

        [Fact]
        [Trait("Method", "ServiceResponse.FromResponseAsync<T>")]
        public async Task FromResponseAsyncOfT_NullResponse_Throws()
        {
            var options = new DatasyncClientOptions();
            await Assert.ThrowsAsync<ArgumentNullException>(()
                => ServiceResponse.FromResponseAsync<KitchenSinkEntity>(null, options.DeserializerOptions)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "ServiceResponse.FromResponseAsync<T>")]
        public async Task FromResponseAsyncOfT_NullOptions_Throws()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            await Assert.ThrowsAsync<ArgumentNullException>(()
                => ServiceResponse.FromResponseAsync<KitchenSinkEntity>(response, null)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.Continue, false, false, false)]
        [InlineData(HttpStatusCode.OK, false, true, false)]
        [InlineData(HttpStatusCode.Created, false, true, false)]
        [InlineData(HttpStatusCode.NoContent, false, true, false)]
        [InlineData(HttpStatusCode.BadRequest, false, false, false)]
        [InlineData(HttpStatusCode.Unauthorized, false, false, false)]
        [InlineData(HttpStatusCode.Forbidden, false, false, false)]
        [InlineData(HttpStatusCode.MethodNotAllowed, false, false, false)]
        [InlineData(HttpStatusCode.Conflict, false, false, true)]
        [InlineData(HttpStatusCode.PreconditionFailed, false, false, true)]
        [InlineData(HttpStatusCode.InternalServerError, false, false, false)]
        [InlineData(HttpStatusCode.Continue, true, false, false)]
        [InlineData(HttpStatusCode.OK, true, true, false)]
        [InlineData(HttpStatusCode.Created, true, true, false)]
        [InlineData(HttpStatusCode.NoContent, true, true, false)]
        [InlineData(HttpStatusCode.BadRequest, true, false, false)]
        [InlineData(HttpStatusCode.Unauthorized, true, false, false)]
        [InlineData(HttpStatusCode.Forbidden, true, false, false)]
        [InlineData(HttpStatusCode.MethodNotAllowed, true, false, false)]
        [InlineData(HttpStatusCode.Conflict, true, false, true)]
        [InlineData(HttpStatusCode.PreconditionFailed, true, false, true)]
        [InlineData(HttpStatusCode.InternalServerError, true, false, false)]
        [Trait("Method", "ServiceResponse.FromResponseAsync<T>")]
        public async Task FromResponseAsyncOfT_NoHeaders(HttpStatusCode statusCode, bool hasContent, bool isSuccessful, bool isConflict)
        {
            // Arrange
            const string jsonPayload = "{\"stringValue\":\"test\"}";
            var options = new DatasyncClientOptions();
            var sut = new HttpResponseMessage(statusCode);
            if (hasContent)
            {
                sut.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }

            // Act
            var response = await ServiceResponse.FromResponseAsync<KitchenSinkEntity>(sut, options.DeserializerOptions).ConfigureAwait(false);

            // Assert
            Assert.Equal(hasContent, response.HasContent);
            Assert.Equal(hasContent, response.HasValue);
            if (hasContent)
            {
                Assert.Equal(jsonPayload, Encoding.UTF8.GetString(response.Content));
                Assert.Equal("test", response.Value.StringValue);
                AssertEx.Contains("Content-Type", "application/json; charset=utf-8", response.Headers);
            }
            else
            {
                Assert.Empty(response.Content);
                Assert.Null(response.Value);
            }
            Assert.Equal(isConflict, response.IsConflictStatusCode);
            Assert.Equal(isSuccessful, response.IsSuccessStatusCode);
            Assert.NotNull(response.ReasonPhrase);
            Assert.Equal((int)statusCode, response.StatusCode);
            Assert.NotNull(response.Version);
        }

        [Theory]
        [InlineData(HttpStatusCode.Continue, false, false, false)]
        [InlineData(HttpStatusCode.OK, false, true, false)]
        [InlineData(HttpStatusCode.Created, false, true, false)]
        [InlineData(HttpStatusCode.NoContent, false, true, false)]
        [InlineData(HttpStatusCode.BadRequest, false, false, false)]
        [InlineData(HttpStatusCode.Unauthorized, false, false, false)]
        [InlineData(HttpStatusCode.Forbidden, false, false, false)]
        [InlineData(HttpStatusCode.MethodNotAllowed, false, false, false)]
        [InlineData(HttpStatusCode.Conflict, false, false, true)]
        [InlineData(HttpStatusCode.PreconditionFailed, false, false, true)]
        [InlineData(HttpStatusCode.InternalServerError, false, false, false)]
        [InlineData(HttpStatusCode.Continue, true, false, false)]
        [InlineData(HttpStatusCode.OK, true, true, false)]
        [InlineData(HttpStatusCode.Created, true, true, false)]
        [InlineData(HttpStatusCode.NoContent, true, true, false)]
        [InlineData(HttpStatusCode.BadRequest, true, false, false)]
        [InlineData(HttpStatusCode.Unauthorized, true, false, false)]
        [InlineData(HttpStatusCode.Forbidden, true, false, false)]
        [InlineData(HttpStatusCode.MethodNotAllowed, true, false, false)]
        [InlineData(HttpStatusCode.Conflict, true, false, true)]
        [InlineData(HttpStatusCode.PreconditionFailed, true, false, true)]
        [InlineData(HttpStatusCode.InternalServerError, true, false, false)]
        [Trait("Method", "ServiceResponse.FromResponseAsync<T>")]
        public async Task FromResponseAsyncOfT_WithHeaders(HttpStatusCode statusCode, bool hasContent, bool isSuccessful, bool isConflict)
        {
            // Arrange
            const string jsonPayload = "{\"stringValue\":\"test\"}";
            var options = new DatasyncClientOptions();
            var sut = new HttpResponseMessage(statusCode);
            if (hasContent)
            {
                sut.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }
            sut.Headers.Add("ETag", "\"42\"");
            sut.Headers.Add("ZUMO-API-VERSION", "3.0.0");

            // Act
            var response = await ServiceResponse.FromResponseAsync<KitchenSinkEntity>(sut, options.DeserializerOptions).ConfigureAwait(false);

            // Assert
            Assert.Equal(hasContent, response.HasContent);
            Assert.Equal(hasContent, response.HasValue);
            if (hasContent)
            {
                Assert.Equal(jsonPayload, Encoding.UTF8.GetString(response.Content));
                Assert.Equal("test", response.Value.StringValue);
                AssertEx.Contains("Content-Type", "application/json; charset=utf-8", response.Headers);
            }
            else
            {
                Assert.Empty(response.Content);
                Assert.Null(response.Value);
            }
            Assert.Equal(isConflict, response.IsConflictStatusCode);
            Assert.Equal(isSuccessful, response.IsSuccessStatusCode);
            Assert.NotNull(response.ReasonPhrase);
            Assert.Equal((int)statusCode, response.StatusCode);
            Assert.NotNull(response.Version);
            AssertEx.Contains("ETag", "\"42\"", response.Headers);
            AssertEx.Contains("ZUMO-API-VERSION", "3.0.0", response.Headers);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "ServiceResponse.FromResponseAsync<T>")]
        public async Task FromResponseAsyncOfT_ImplicitOperator([CombinatorialValues(200, 201, 409, 412)] int statusCode, [CombinatorialValues(true, false)] bool hasContent)
        {
            // Arrange
            const string jsonPayload = "{\"stringValue\":\"test\"}";
            var options = new DatasyncClientOptions();
            var sut = new HttpResponseMessage((HttpStatusCode)statusCode);
            if (hasContent)
            {
                sut.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }

            // Act
            KitchenSinkEntity response = await ServiceResponse.FromResponseAsync<KitchenSinkEntity>(sut, options.DeserializerOptions).ConfigureAwait(false);

            // Assert
            if (hasContent)
            {
                Assert.NotNull(response);
                Assert.Equal("test", response.StringValue);
            }
            else
            {
                Assert.Null(response);
            }
        }
    }
}

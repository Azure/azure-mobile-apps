// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
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
    public class HttpResponseOfT_Tests
    {
        #region Helpers
        private class MockHttpResponse : HttpResponse<MockObject>
        {
            public MockHttpResponse() : base()
            {
            }

            public MockHttpResponse(HttpResponseMessage message) : base(message)
            {
            }
        }

        private const string jsonContent = "{\"stringValue\":\"test\"}";
        #endregion

        [Fact]
        public void Ctor_NoArgs_SetsDefaults()
        {
            var sut = new MockHttpResponse();

            Assert.Empty(sut.Content);
            Assert.False(sut.HasContent);
            Assert.False(sut.HasValue);
            Assert.Empty(sut.Headers);
            Assert.False(sut.IsConflictStatusCode);
            Assert.True(sut.IsSuccessStatusCode);
            Assert.Null(sut.ReasonPhrase);
            Assert.Equal(HttpStatusCode.NonAuthoritativeInformation, sut.StatusCode);
            Assert.Null(sut.Value);
            Assert.Equal(0, sut.Version.Major);
        }

        [Theory, CombinatorialData]
        public void Ctor_WithResponse_SetsValues(
            [CombinatorialValues(100, 200, 201, 204, 304, 400, 401, 404, 409, 412, 500)] int statusCode,
            [CombinatorialValues(null, "", "test phrase")] string reasonPhrase)
        {
            // Arrange
            var message = new HttpResponseMessage()
            {
                ReasonPhrase = reasonPhrase,
                StatusCode = (HttpStatusCode)statusCode
            };
            bool isSuccess = statusCode >= 200 && statusCode < 300;
            bool isConflict = statusCode == 409 || statusCode == 412;

            // Act
            var sut = new MockHttpResponse(message);

            // Assert
            Assert.Empty(sut.Content);
            Assert.False(sut.HasContent);
            Assert.False(sut.HasValue);
            Assert.Empty(sut.Headers);
            Assert.Equal(isConflict, sut.IsConflictStatusCode);
            Assert.Equal(isSuccess, sut.IsSuccessStatusCode);
            if (reasonPhrase != null)
            {
                Assert.Equal(reasonPhrase, sut.ReasonPhrase);
            }
            else
            {
                Assert.NotNull(sut.ReasonPhrase);
            }
            Assert.Equal(statusCode, (int)sut.StatusCode);
        }

        [Theory, CombinatorialData]
        public void Ctor_WithResponseAndHeaders_SetsValues(
            [CombinatorialValues(100, 200, 201, 204, 304, 400, 401, 404, 409, 412, 500)] int statusCode,
            [CombinatorialValues(null, "", "test phrase")] string reasonPhrase)
        {
            // Arrange
            var message = new HttpResponseMessage()
            {
                ReasonPhrase = reasonPhrase,
                StatusCode = (HttpStatusCode)statusCode
            };
            message.Headers.Add("X-Test", "true");
            bool isSuccess = statusCode >= 200 && statusCode < 300;
            bool isConflict = statusCode == 409 || statusCode == 412;

            // Act
            var sut = new MockHttpResponse(message);

            // Assert
            Assert.Empty(sut.Content);
            Assert.False(sut.HasContent);
            Assert.False(sut.HasValue);
            AssertEx.HasValue("X-Test", new[] { "true" }, sut.Headers);
            Assert.Equal(isConflict, sut.IsConflictStatusCode);
            Assert.Equal(isSuccess, sut.IsSuccessStatusCode);
            if (reasonPhrase != null)
            {
                Assert.Equal(reasonPhrase, sut.ReasonPhrase);
            }
            else
            {
                Assert.NotNull(sut.ReasonPhrase);
            }
            Assert.Equal(statusCode, (int)sut.StatusCode);
        }

        [Theory, CombinatorialData]
        public void Ctor_WithContent_SetsValues(
            [CombinatorialValues(100, 200, 201, 204, 304, 400, 401, 404, 409, 412, 500)] int statusCode,
            [CombinatorialValues(null, "", "test phrase")] string reasonPhrase)
        {
            // Arrange
            var message = new HttpResponseMessage()
            {
                ReasonPhrase = reasonPhrase,
                StatusCode = (HttpStatusCode)statusCode,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };
            bool isSuccess = statusCode >= 200 && statusCode < 300;
            bool isConflict = statusCode == 409 || statusCode == 412;

            // Act
            var sut = new MockHttpResponse(message);

            // Assert
            Assert.Empty(sut.Content);
            Assert.False(sut.HasContent);
            Assert.False(sut.HasValue);
            AssertEx.HasValue("Content-Type", new[] { "application/json; charset=utf-8" }, sut.Headers);
            Assert.Equal(isConflict, sut.IsConflictStatusCode);
            Assert.Equal(isSuccess, sut.IsSuccessStatusCode);
            if (reasonPhrase != null)
            {
                Assert.Equal(reasonPhrase, sut.ReasonPhrase);
            }
            else
            {
                Assert.NotNull(sut.ReasonPhrase);
            }
            Assert.Equal(statusCode, (int)sut.StatusCode);
            Assert.Null(sut.Value);
        }

        [Theory, CombinatorialData]
        public void Ctor_WithContentAndHeaders_SetsValues(
            [CombinatorialValues(100, 200, 201, 204, 304, 400, 401, 404, 409, 412, 500)] int statusCode,
            [CombinatorialValues(null, "", "test phrase")] string reasonPhrase)
        {
            // Arrange
            var message = new HttpResponseMessage()
            {
                ReasonPhrase = reasonPhrase,
                StatusCode = (HttpStatusCode)statusCode,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };
            message.Headers.Add("X-Test", "true");
            bool isSuccess = statusCode >= 200 && statusCode < 300;
            bool isConflict = statusCode == 409 || statusCode == 412;

            // Act
            var sut = new MockHttpResponse(message);

            // Assert
            Assert.Empty(sut.Content);
            Assert.False(sut.HasContent);
            Assert.False(sut.HasValue);
            AssertEx.HasValue("Content-Type", new[] { "application/json; charset=utf-8" }, sut.Headers);
            AssertEx.HasValue("X-Test", new[] { "true" }, sut.Headers);
            Assert.Equal(isConflict, sut.IsConflictStatusCode);
            Assert.Equal(isSuccess, sut.IsSuccessStatusCode);
            if (reasonPhrase != null)
            {
                Assert.Equal(reasonPhrase, sut.ReasonPhrase);
            }
            else
            {
                Assert.NotNull(sut.ReasonPhrase);
            }
            Assert.Equal(statusCode, (int)sut.StatusCode);
            Assert.Null(sut.Value);
        }

        [Theory, CombinatorialData]
        public async Task FromResponseAsync_WithResponse_SetsValues(
            [CombinatorialValues(100, 200, 201, 204, 304, 400, 401, 404, 409, 412, 500)] int statusCode,
            [CombinatorialValues(null, "", "test phrase")] string reasonPhrase)
        {
            // Arrange
            var message = new HttpResponseMessage()
            {
                ReasonPhrase = reasonPhrase,
                StatusCode = (HttpStatusCode)statusCode
            };
            bool isSuccess = statusCode >= 200 && statusCode < 300;
            bool isConflict = statusCode == 409 || statusCode == 412;
            var options = new DatasyncClientOptions().DeserializerOptions;

            // Act
            var sut = await HttpResponse<MockObject>.FromResponseAsync(message, options).ConfigureAwait(false);

            // Assert
            Assert.Empty(sut.Content);
            Assert.False(sut.HasContent);
            Assert.False(sut.HasValue);
            Assert.Empty(sut.Headers);
            Assert.Equal(isConflict, sut.IsConflictStatusCode);
            Assert.Equal(isSuccess, sut.IsSuccessStatusCode);
            if (reasonPhrase != null)
            {
                Assert.Equal(reasonPhrase, sut.ReasonPhrase);
            }
            else
            {
                Assert.NotNull(sut.ReasonPhrase);
            }
            Assert.Equal(statusCode, (int)sut.StatusCode);
            Assert.Null(sut.Value);
        }

        [Theory, CombinatorialData]
        public async Task FromResponseAsync_WithContent_SetsValues(
            [CombinatorialValues(100, 200, 201, 204, 304, 400, 401, 404, 409, 412, 500)] int statusCode,
            [CombinatorialValues(null, "", "test phrase")] string reasonPhrase)
        {
            // Arrange
            var message = new HttpResponseMessage()
            {
                ReasonPhrase = reasonPhrase,
                StatusCode = (HttpStatusCode)statusCode,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };
            bool isSuccess = statusCode >= 200 && statusCode < 300;
            bool isConflict = statusCode == 409 || statusCode == 412;
            var options = new DatasyncClientOptions().DeserializerOptions;

            // Act
            var sut = await HttpResponse<MockObject>.FromResponseAsync(message, options).ConfigureAwait(false);

            // Assert
            Assert.Equal(jsonContent, Encoding.UTF8.GetString(sut.Content));
            Assert.True(sut.HasContent);
            Assert.True(sut.HasValue);
            AssertEx.HasValue("Content-Type", new[] { "application/json; charset=utf-8" }, sut.Headers);
            Assert.Equal(isConflict, sut.IsConflictStatusCode);
            Assert.Equal(isSuccess, sut.IsSuccessStatusCode);
            if (reasonPhrase != null)
            {
                Assert.Equal(reasonPhrase, sut.ReasonPhrase);
            }
            else
            {
                Assert.NotNull(sut.ReasonPhrase);
            }
            Assert.Equal(statusCode, (int)sut.StatusCode);
            Assert.Equal("test", sut.Value.StringValue);
        }

        [Fact]
        public async Task FromResponseAsync_WithBadJson_Throws()
        {
            // Arrange
            var message = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{some-bad-json", Encoding.UTF8, "application/json")
            };
            var options = new DatasyncClientOptions().DeserializerOptions;

            // Act
            await Assert.ThrowsAsync<JsonException>(() => HttpResponse<MockObject>.FromResponseAsync(message, options)).ConfigureAwait(false);
        }

        [Fact]
        public async Task FromResponseAsync_WorksWithImplicitOp()
        {
            // Arrange
            var message = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };
            var options = new DatasyncClientOptions().DeserializerOptions;

            // Act
            MockObject obj = await HttpResponse<MockObject>.FromResponseAsync(message, options).ConfigureAwait(false);

            // Assert
            Assert.NotNull(obj);
            Assert.Equal("test", obj.StringValue);
        }
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Naming violations don't matter in tests")]
    public class InternalHttpClient_Tests : BaseTest
    {
        #region Helpers
        /// <summary>
        /// Provides access to protected InternalHttpClient members
        /// </summary>
        private class __InternalHttpClient : InternalHttpClient
        {
            internal __InternalHttpClient(Uri endpoint, DatasyncClientOptions options) : base(endpoint, options)
            {
            }

            internal HttpMessageHandler __MessageHandler { get => base.MessageHandler; }

            internal HttpClient __HttpClient { get => base.HttpClient; }
        }
        #endregion

        #region Ctor
        [Fact]
        public void Ctor_NullEndpoint_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new InternalHttpClient(null, new DatasyncClientOptions()));
        }

        [Fact]
        public void Ctor_NullOptions_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new InternalHttpClient(new Uri("https://foo.azure-api.net/"), null));
        }

        [Fact]
        public void Ctor_RelativeEndpoint_Throws()
        {
            Assert.Throws<UriFormatException>(() => new InternalHttpClient(new Uri("a/b", UriKind.Relative), new DatasyncClientOptions()));
        }

        [Theory]
        [InlineData("file://localhost/foo")]
        [InlineData("http://foo.azurewebsites.net")]
        public void Ctor_InvalidEndpoint_Throws(string uri)
        {
            Assert.Throws<UriFormatException>(() => new InternalHttpClient(new Uri(uri), new DatasyncClientOptions()));
        }

        [Theory]
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://127.0.0.1", "http://127.0.0.1/")]
        [InlineData("https://foo.azurewebsites.net", "https://foo.azurewebsites.net/")]
        [InlineData("http://localhost/tables", "http://localhost/tables/")]
        [InlineData("http://127.0.0.1/tables", "http://127.0.0.1/tables/")]
        [InlineData("https://foo.azurewebsites.net/tables", "https://foo.azurewebsites.net/tables/")]
        [InlineData("http://localhost/tables/foo", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo/", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo#fragment", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo/#fragment", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo?$count=true", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo/?$count=true", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo#fragment?$count=true", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo/#fragment?$count=true", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo?$count=true#fragment", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo/?$count=true#fragment", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/", "http://localhost/")]
        [InlineData("http://localhost#fragment", "http://localhost/")]
        [InlineData("http://localhost/#fragment", "http://localhost/")]
        [InlineData("http://localhost?$count=true", "http://localhost/")]
        [InlineData("http://localhost/?$count=true", "http://localhost/")]
        [InlineData("http://localhost#fragment?$count=true", "http://localhost/")]
        [InlineData("http://localhost/#fragment?$count=true", "http://localhost/")]
        [InlineData("http://localhost?$count=true#fragment", "http://localhost/")]
        [InlineData("http://localhost/?$count=true#fragment", "http://localhost/")]
        public void Ctor_GoodEndpoint_CreatesClient(string uri, string expected)
        {
            var client = new __InternalHttpClient(new Uri(uri), new DatasyncClientOptions());
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.__MessageHandler);
            Assert.NotNull(client.__HttpClient);
        }
        #endregion

        #region CreatePipeline
        [Fact]
        public void CreatePipeline_NoHandlers_CreatesPipeline()
        {
            var options = new DatasyncClientOptions { HttpPipeline = Array.Empty<HttpMessageHandler>() };

            // Act
            var client = new __InternalHttpClient(new Uri("http://localhost/tables/movies/"), options);

            // Assert
            Assert.IsAssignableFrom<HttpClientHandler>(client.__MessageHandler);
        }

        [Fact]
        public void CreatePipeline_C_CreatesPipeline()
        {
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c } };

            // Act
            var client = new __InternalHttpClient(new Uri("http://localhost/tables/movies/"), options);

            // Assert
            Assert.Same(c, client.__MessageHandler);
        }

        [Fact]
        public void CreatePipeline_B_CreatesPipeline()
        {
            var b = new MockDelegatingHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { b } };

            // Act
            var client = new __InternalHttpClient(new Uri("http://localhost/tables/movies/"), options);

            // Assert
            Assert.Same(b, client.__MessageHandler);
            Assert.IsAssignableFrom<HttpClientHandler>(b.InnerHandler);
        }

        [Fact]
        public void CreatePipeline_BC_CreatesPipeline()
        {
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { b, c } };

            // Act
            var client = new __InternalHttpClient(new Uri("http://localhost/tables/movies/"), options);

            // Assert
            Assert.Same(b, client.__MessageHandler);
            Assert.Same(c, b.InnerHandler);
        }

        [Fact]
        public void CreatePipeline_AB_CreatesPipeline()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b } };

            // Act
            var client = new __InternalHttpClient(new Uri("http://localhost/tables/movies/"), options);

            // Assert
            Assert.Same(a, client.__MessageHandler);
            Assert.Same(b, a.InnerHandler);
            Assert.IsAssignableFrom<HttpClientHandler>(b.InnerHandler);
        }

        [Fact]
        public void CreatePipeline_ABC_CreatesPipeline()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b, c } };

            // Act
            var client = new __InternalHttpClient(new Uri("http://localhost/tables/movies/"), options);

            // Assert
            Assert.Same(a, client.__MessageHandler);
            Assert.Same(b, a.InnerHandler);
            Assert.Same(c, b.InnerHandler);
        }

        [Fact]
        public void CreatePipeline_CB_ThrowsArgumentException()
        {
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new __InternalHttpClient(new Uri("http://localhost/tables/movies/"), options));
        }

        [Fact]
        public void CreatePipeline_CAB_ThrowsArgumentException()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c, a, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new __InternalHttpClient(new Uri("http://localhost/tables/movies/"), options));
        }

        [Fact]
        public void CreatePipeline_ACB_ThrowsArgumentException()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, c, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new __InternalHttpClient(new Uri("http://localhost/tables/movies/"), options));
        }
        #endregion

        #region SendAsync
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
        public async Task SendAsync_Works(HttpStatusCode statusCode, bool hasContent, bool isSuccessful, bool isConflict)
        {
            var clientHandler = new MockDelegatingHandler();
            if (hasContent)
            {
                MockObject payload = new() { StringValue = "test" };
                clientHandler.AddResponse<MockObject>(statusCode, payload);
            }
            else
            {
                clientHandler.AddResponse(statusCode);
            }
            var clientOptions = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { clientHandler } };
            var client = new InternalHttpClient(new Uri("https://foo.azurewebsites.net/tables/movies"), clientOptions);

            var request = new HttpRequestMessage(HttpMethod.Post, client.Endpoint);
            var response = await client.SendAsync(request).ConfigureAwait(false);

            // Assert - Response
            Assert.Equal(statusCode, response.StatusCode);
            Assert.Equal(isSuccessful, response.IsSuccessStatusCode);
            Assert.Equal(isConflict, response.IsConflictStatusCode());
            if (hasContent)
            {
                Assert.Equal("{\"stringValue\":\"test\"}", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }

            // Assert - Request
            Assert.Equal(HttpMethod.Post, clientHandler.Requests[0].Method);
            AssertEx.HasValue("ZUMO-API-VERSION", new[] { "3.0.0" }, clientHandler.Requests[0].Headers);
        }
        #endregion

        #region Dispose
        [Fact]
        public async Task Dispose_Correctly()
        {
            var clientHandler = new MockDelegatingHandler();
            clientHandler.AddResponse(HttpStatusCode.NoContent);
            var clientOptions = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { clientHandler } };
            var client = new InternalHttpClient(new Uri("https://foo.azurewebsites.net/tables/movies"), clientOptions);

            client.Dispose();

            var request = new HttpRequestMessage(HttpMethod.Delete, client.Endpoint);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.SendAsync(request)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Dispose_CanRepeat()
        {
            var clientHandler = new MockDelegatingHandler();
            clientHandler.AddResponse(HttpStatusCode.NoContent);
            var clientOptions = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { clientHandler } };
            var client = new InternalHttpClient(new Uri("https://foo.azurewebsites.net/tables/movies"), clientOptions);

            client.Dispose();
            client.Dispose();

            var request = new HttpRequestMessage(HttpMethod.Delete, client.Endpoint);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.SendAsync(request)).ConfigureAwait(false);
        }
        #endregion
    }
}

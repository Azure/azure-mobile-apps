// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Reflection;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class DatasyncHttpClient_Tests
    {
        #region Ctor
        #region Ctor(String)
        [Fact]
        public void Ctor_String_Throws_NullEndpoint()
        {
            const string endpoint = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncHttpClient(endpoint));
        }

        [Theory]
        [InlineData("")]
        [InlineData("http://azurewebsites.net")]
        [InlineData("file://localhost/mnt/a")]
        [InlineData("mailto:postmaster@microsoft.com")]
        [InlineData("a/b")]
        public void Ctor_String_Throws_InvalidEndpoint(string endpoint)
        {
            Assert.Throws<UriFormatException>(() => new DatasyncHttpClient(endpoint));
        }

        [Theory]
        [InlineData("http://localhost/tables/foo", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo?a=b", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo#fragment", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo?a=b#fragment", "http://localhost/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo?a=b", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo#fragment", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo?a=b#fragment", "https://zumo.azurewebsites.net/tables/foo/")]
        public void Ctor_String_SetsEndpoint(string endpoint, string expected)
        {
            var client = new DatasyncHttpClient(endpoint);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
        }
        #endregion

        #region Ctor(Uri)
        [Fact]
        public void Ctor_Uri_Throws_NullEndpoint()
        {
            Uri endpoint = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncHttpClient(endpoint));
        }

        [Fact]
        public void Ctor_Uri_Throws_RelativeEndpoint()
        {
            Uri endpoint = new("a/b", UriKind.Relative);
            Assert.Throws<UriFormatException>(() => new DatasyncHttpClient(endpoint));
        }

        [Theory]
        [InlineData("http://azurewebsites.net")]
        [InlineData("file://localhost/mnt/a")]
        [InlineData("mailto:postmaster@microsoft.com")]
        public void Ctor_Uri_Throws_InvalidEndpoint(string endpoint)
        {
            Uri uriEndpoint = new(endpoint);
            Assert.Throws<UriFormatException>(() => new DatasyncHttpClient(uriEndpoint));
        }

        [Theory]
        [InlineData("http://localhost/tables/foo", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo?a=b", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo#fragment", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo?a=b#fragment", "http://localhost/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo?a=b", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo#fragment", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo?a=b#fragment", "https://zumo.azurewebsites.net/tables/foo/")]
        public void Ctor_UriSetsEndpoint(string endpoint, string expected)
        {
            var client = new DatasyncHttpClient(new Uri(endpoint));
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
        }
        #endregion

        #region Ctor(String,DatasyncClientOptions)
        [Fact]
        public void Ctor_StringOptions_Throws_NullEndpoint()
        {
            const string endpoint = null;
            var options = new DatasyncClientOptions();
            Assert.Throws<ArgumentNullException>(() => new DatasyncHttpClient(endpoint, options));
        }

        [Fact]
        public void Ctor_StringOptions_Throws_NullOptions()
        {
            const string endpoint = "http://localhost/tables/foo";
            DatasyncClientOptions options = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncHttpClient(endpoint, options));
        }

        [Theory]
        [InlineData("")]
        [InlineData("http://azurewebsites.net")]
        [InlineData("file://localhost/mnt/a")]
        [InlineData("mailto:postmaster@microsoft.com")]
        [InlineData("a/b")]
        public void Ctor_StringOptions_Throws_InvalidEndpoint(string endpoint)
        {
            var options = new DatasyncClientOptions();
            Assert.Throws<UriFormatException>(() => new DatasyncHttpClient(endpoint, options));
        }

        [Theory]
        [InlineData("http://localhost/tables/foo", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo?a=b", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo#fragment", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo?a=b#fragment", "http://localhost/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo?a=b", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo#fragment", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo?a=b#fragment", "https://zumo.azurewebsites.net/tables/foo/")]
        public void Ctor_StringOptions_SetsEndpoint(string endpoint, string expected)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncHttpClient(endpoint, options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
        }
        #endregion

        #region Ctor(Uri,DatasyncClientOptions)
        [Fact]
        public void Ctor_UriOptions_Throws_NullEndpoint()
        {
            Uri endpoint = null;
            var options = new DatasyncClientOptions();
            Assert.Throws<ArgumentNullException>(() => new DatasyncHttpClient(endpoint, options));
        }

        [Fact]
        public void Ctor_UriOptions_Throws_NullOptions()
        {
            Uri endpoint = new("http://localhost/tables/foo");
            DatasyncClientOptions options = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncHttpClient(endpoint, options));
        }

        [Fact]
        public void Ctor_UriOptions_Throws_RelativeEndpoint()
        {
            Uri endpoint = new("a/b", UriKind.Relative);
            var options = new DatasyncClientOptions();
            Assert.Throws<UriFormatException>(() => new DatasyncHttpClient(endpoint, options));
        }

        [Theory]
        [InlineData("http://azurewebsites.net")]
        [InlineData("file://localhost/mnt/a")]
        [InlineData("mailto:postmaster@microsoft.com")]
        public void Ctor_UriOptions_Throws_InvalidEndpoint(string endpoint)
        {
            Uri uriEndpoint = new(endpoint);
            var options = new DatasyncClientOptions();
            Assert.Throws<UriFormatException>(() => new DatasyncHttpClient(uriEndpoint, options));
        }

        [Theory]
        [InlineData("http://localhost/tables/foo", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo?a=b", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo#fragment", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo?a=b#fragment", "http://localhost/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo?a=b", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo#fragment", "https://zumo.azurewebsites.net/tables/foo/")]
        [InlineData("https://zumo.azurewebsites.net/tables/foo?a=b#fragment", "https://zumo.azurewebsites.net/tables/foo/")]
        public void Ctor_UriOptions_SetsEndpoint(string endpoint, string expected)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncHttpClient(new Uri(endpoint), options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
        }
        #endregion
        #endregion

        #region AddDefaultRequestHeaders
        [Fact]
        public void HttpClient_HasProtocolHeader()
        {
            var options = new DatasyncClientOptions();
            var endpoint = new Uri("https://zumo.azure-api.net/tables/movies");
            var client = new DatasyncHttpClient(endpoint, options);

            AssertEx.HeaderMatches("ZUMO-API-VERSION", "3.0.0", client.HttpClient.DefaultRequestHeaders);
        }
        #endregion

        #region CreatePipeline
        [Fact]
        public void CreatePipeline_NoHandlers_CreatesPipeline()
        {
            var options = new DatasyncClientOptions { HttpPipeline = Array.Empty<HttpMessageHandler>() };

            // Act
            var client = new DatasyncHttpClient("http://localhost", options);

            // Assert
            Assert.IsAssignableFrom<HttpClientHandler>(client.HttpMessageHandler);
        }

        [Fact]
        public void CreatePipeline_C_CreatesPipeline()
        {
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c } };

            // Act
            var client = new DatasyncHttpClient("http://localhost", options);

            // Assert
            Assert.Same(c, client.HttpMessageHandler);
        }

        [Fact]
        public void CreatePipeline_B_CreatesPipeline()
        {
            var b = new MockMessageHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { b } };

            // Act
            var client = new DatasyncHttpClient("http://localhost", options);

            // Assert
            Assert.Same(b, client.HttpMessageHandler);
            Assert.IsAssignableFrom<HttpClientHandler>(b.InnerHandler);
        }

        [Fact]
        public void CreatePipeline_BC_CreatesPipeline()
        {
            var b = new MockMessageHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { b, c } };

            // Act
            var client = new DatasyncHttpClient("http://localhost", options);

            // Assert
            Assert.Same(b, client.HttpMessageHandler);
            Assert.Same(c, b.InnerHandler);
        }

        [Fact]
        public void CreatePipeline_AB_CreatesPipeline()
        {
            var a = new MockMessageHandler();
            var b = new MockMessageHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b } };

            // Act
            var client = new DatasyncHttpClient("http://localhost", options);

            // Assert
            Assert.Same(a, client.HttpMessageHandler);
            Assert.Same(b, a.InnerHandler);
            Assert.IsAssignableFrom<HttpClientHandler>(b.InnerHandler);
        }

        [Fact]
        public void CreatePipeline_ABC_CreatesPipeline()
        {
            var a = new MockMessageHandler();
            var b = new MockMessageHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b, c } };

            // Act
            var client = new DatasyncHttpClient("http://localhost", options);

            // Assert
            Assert.Same(a, client.HttpMessageHandler);
            Assert.Same(b, a.InnerHandler);
            Assert.Same(c, b.InnerHandler);
        }

        [Fact]
        public void CreatePipeline_CB_ThrowsArgumentException()
        {
            var b = new MockMessageHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new DatasyncHttpClient("http://localhost", options));
        }

        [Fact]
        public void CreatePipeline_CAB_ThrowsArgumentException()
        {
            var a = new MockMessageHandler();
            var b = new MockMessageHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c, a, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new DatasyncHttpClient("http://localhost", options));
        }

        [Fact]
        public void CreatePipeline_ACB_ThrowsArgumentException()
        {
            var a = new MockMessageHandler();
            var b = new MockMessageHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, c, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new DatasyncHttpClient("http://localhost", options));
        }
        #endregion

        #region CreateRequest
        [Theory]
        [InlineData("GET", "", "https://zumo.azure-api.net/tables/movies/")]
        [InlineData("GET", "1234", "https://zumo.azure-api.net/tables/movies/1234")]
        [InlineData("DELETE", "1234", "https://zumo.azure-api.net/tables/movies/1234")]
        public void CreateRequest_HandlesNormalRequests(string sMethod, string relativeUri, string expectedUri)
        {
            // Arrange
            var client = new DatasyncHttpClient("https://zumo.azure-api.net/tables/movies");

            // Act
            var request = client.CreateRequest(new HttpMethod(sMethod), relativeUri);

            // Assert
            Assert.Equal(sMethod, request.Method.ToString());
            Assert.Equal(expectedUri, request.RequestUri.ToString());
        }

        [Theory]
        [InlineData(null, "1234")]
        [InlineData("GET", null)]
        public void CreateRequest_ThrowsOnNullArgs(string sMethod, string relativeUri)
        {
            // Arrange
            var client = new DatasyncHttpClient("https://zumo.azure-api.net/tables/movies");
            HttpMethod method = sMethod == null ? null : new HttpMethod(sMethod);

            // Assert
            Assert.Throws<ArgumentNullException>(() => client.CreateRequest(method, relativeUri));
        }
        #endregion

        #region CreateRequest<T>
        [Theory]
        [InlineData("POST", "", "https://zumo.azure-api.net/tables/movies/")]
        [InlineData("PUT", "1234", "https://zumo.azure-api.net/tables/movies/1234")]
        [InlineData("PATCH", "1234", "https://zumo.azure-api.net/tables/movies/1234")]
        public void CreateRequestOfT_HandlesNormalRequests(string sMethod, string relativeUri, string expectedUri)
        {
            // Arrange
            var payload = new MockObject { StringValue = "test" };
            var jsonPayload = "{\"stringValue\":\"test\"}";
            var client = new DatasyncHttpClient("https://zumo.azure-api.net/tables/movies");

            // Act
            var request = client.CreateRequest(new HttpMethod(sMethod), relativeUri, payload);

            // Assert
            Assert.Equal(sMethod, request.Method.ToString());
            Assert.Equal(expectedUri, request.RequestUri.ToString());
            Assert.IsAssignableFrom<StringContent>(request.Content);
            Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);
            Assert.Equal(jsonPayload, request.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData("POST", "", "https://zumo.azure-api.net/tables/movies/")]
        [InlineData("PUT", "1234", "https://zumo.azure-api.net/tables/movies/1234")]
        [InlineData("PATCH", "1234", "https://zumo.azure-api.net/tables/movies/1234")]
        public void CreateRequestOfT_HandlesNormalRequests_WithContentType(string sMethod, string relativeUri, string expectedUri)
        {
            // Arrange
            var payload = new MockObject { StringValue = "test" };
            var jsonPayload = "{\"stringValue\":\"test\"}";
            var client = new DatasyncHttpClient("https://zumo.azure-api.net/tables/movies");

            // Act
            var request = client.CreateRequest(new HttpMethod(sMethod), relativeUri, payload, "application/merge-patch+json");

            // Assert
            Assert.Equal(sMethod, request.Method.ToString());
            Assert.Equal(expectedUri, request.RequestUri.ToString());
            Assert.IsAssignableFrom<StringContent>(request.Content);
            Assert.Equal("application/merge-patch+json", request.Content.Headers.ContentType.MediaType);
            Assert.Equal(jsonPayload, request.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData(null, "1234")]
        [InlineData("GET", null)]
        public void CreateRequestOfT_ThrowsOnNullArgs(string sMethod, string relativeUri)
        {
            // Arrange
            var payload = new MockObject() { StringValue = "test" };
            var client = new DatasyncHttpClient("https://zumo.azure-api.net/tables/movies");
            HttpMethod method = sMethod == null ? null : new HttpMethod(sMethod);

            // Assert
            Assert.Throws<ArgumentNullException>(() => client.CreateRequest(method, relativeUri, payload));
        }

        [Fact]
        public void CreateRequestOfT_ThrowsOnNullPayload()
        {
            MockObject payload = null;
            var client = new DatasyncHttpClient("https://zumo.azure-api.net/tables/movies");

            Assert.Throws<ArgumentNullException>(() => client.CreateRequest(HttpMethod.Post, "1234", payload));
        }

        [Fact]
        public void CreateRequestOfT_ThrowsOnNullContentType()
        {
            // Arrange
            var payload = new MockObject() { StringValue = "test" };
            var client = new DatasyncHttpClient("https://zumo.azure-api.net/tables/movies");

            // Assert
            Assert.Throws<ArgumentNullException>(() => client.CreateRequest(HttpMethod.Post, "1234", payload, null));
        }
        #endregion

        #region Dispose
        [Fact]
        public void CanDispose_DisposesHttpClient()
        {
            // Arrange
            var client = new DatasyncHttpClient("https://zumo.azure-api.net/tables.foo");

            // Act
            client.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => client.HttpClient.CancelPendingRequests());
        }

        [Fact]
        public void CanDispose_CanDisposeTwice()
        {
            // Arrange
            var client = new DatasyncHttpClient("https://zumo.azure-api.net/tables.foo");

            // Act
            client.Dispose();
            client.Dispose(); // This won't have any effect, so it's a null up.  If it throws, it's a problem.
        }
        #endregion
    }
}

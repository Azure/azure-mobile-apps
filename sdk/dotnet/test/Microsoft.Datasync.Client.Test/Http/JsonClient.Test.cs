// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class JsonClient_Tests
    {
        [Fact]
        public void Ctor_String_Throws_NullEndpoint()
        {
            const string endpoint = null;
            Assert.Throws<ArgumentNullException>(() => new JsonClient(endpoint));
        }

        [Theory]
        [InlineData("")]
        [InlineData("http://azurewebsites.net")]
        [InlineData("file://localhost/mnt/a")]
        [InlineData("mailto:postmaster@microsoft.com")]
        [InlineData("a/b")]
        public void Ctor_String_Throws_InvalidEndpoint(string endpoint)
        {
            Assert.Throws<UriFormatException>(() => new JsonClient(endpoint));
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
            var client = new JsonClient(endpoint);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
        }

        [Fact]
        public void Ctor_Uri_Throws_NullEndpoint()
        {
            Uri endpoint = null;
            Assert.Throws<ArgumentNullException>(() => new JsonClient(endpoint));
        }

        [Fact]
        public void Ctor_Uri_Throws_RelativeEndpoint()
        {
            Uri endpoint = new("a/b", UriKind.Relative);
            Assert.Throws<UriFormatException>(() => new JsonClient(endpoint));
        }

        [Theory]
        [InlineData("http://azurewebsites.net")]
        [InlineData("file://localhost/mnt/a")]
        [InlineData("mailto:postmaster@microsoft.com")]
        public void Ctor_Uri_Throws_InvalidEndpoint(string endpoint)
        {
            Uri uriEndpoint = new(endpoint);
            Assert.Throws<UriFormatException>(() => new JsonClient(uriEndpoint));
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
            var client = new JsonClient(new Uri(endpoint));
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
        }

        [Fact]
        public void Ctor_StringOptions_Throws_NullEndpoint()
        {
            const string endpoint = null;
            var options = new DatasyncClientOptions();
            Assert.Throws<ArgumentNullException>(() => new JsonClient(endpoint, options));
        }

        [Fact]
        public void Ctor_StringOptions_Throws_NullOptions()
        {
            const string endpoint = "http://localhost/tables/foo";
            DatasyncClientOptions options = null;
            Assert.Throws<ArgumentNullException>(() => new JsonClient(endpoint, options));
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
            Assert.Throws<UriFormatException>(() => new JsonClient(endpoint, options));
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
            var client = new JsonClient(endpoint, options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
        }

        [Fact]
        public void Ctor_UriOptions_Throws_NullEndpoint()
        {
            Uri endpoint = null;
            var options = new DatasyncClientOptions();
            Assert.Throws<ArgumentNullException>(() => new JsonClient(endpoint, options));
        }

        [Fact]
        public void Ctor_UriOptions_Throws_NullOptions()
        {
            Uri endpoint = new("http://localhost/tables/foo");
            DatasyncClientOptions options = null;
            Assert.Throws<ArgumentNullException>(() => new JsonClient(endpoint, options));
        }

        [Fact]
        public void Ctor_UriOptions_Throws_RelativeEndpoint()
        {
            Uri endpoint = new("a/b", UriKind.Relative);
            var options = new DatasyncClientOptions();
            Assert.Throws<UriFormatException>(() => new JsonClient(endpoint, options));
        }

        [Theory]
        [InlineData("http://azurewebsites.net")]
        [InlineData("file://localhost/mnt/a")]
        [InlineData("mailto:postmaster@microsoft.com")]
        public void Ctor_UriOptions_Throws_InvalidEndpoint(string endpoint)
        {
            Uri uriEndpoint = new(endpoint);
            var options = new DatasyncClientOptions();
            Assert.Throws<UriFormatException>(() => new JsonClient(uriEndpoint, options));
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
            var client = new JsonClient(new Uri(endpoint), options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
        }

        [Fact]
        public void HttpClient_HasProtocolHeader()
        {
            var options = new DatasyncClientOptions();
            var endpoint = new Uri("https://zumo.azure-api.net/tables/movies");
            var client = new JsonClient(endpoint, options);

            AssertEx.HeaderMatches("ZUMO-API-VERSION", "3.0.0", client.HttpClient.DefaultRequestHeaders);
        }

        [Fact]
        public void CreatePipeline_NoHandlers_CreatesPipeline()
        {
            var options = new DatasyncClientOptions { HttpPipeline = Array.Empty<HttpMessageHandler>() };

            // Act
            var client = new JsonClient("http://localhost", options);

            // Assert
            Assert.IsAssignableFrom<HttpClientHandler>(client.HttpMessageHandler);
        }

        [Fact]
        public void CreatePipeline_C_CreatesPipeline()
        {
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c } };

            // Act
            var client = new JsonClient("http://localhost", options);

            // Assert
            Assert.Same(c, client.HttpMessageHandler);
        }

        [Fact]
        public void CreatePipeline_B_CreatesPipeline()
        {
            var b = new MockMessageHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { b } };

            // Act
            var client = new JsonClient("http://localhost", options);

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
            var client = new JsonClient("http://localhost", options);

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
            var client = new JsonClient("http://localhost", options);

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
            var client = new JsonClient("http://localhost", options);

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
            Assert.Throws<ArgumentException>(() => _ = new JsonClient("http://localhost", options));
        }

        [Fact]
        public void CreatePipeline_CAB_ThrowsArgumentException()
        {
            var a = new MockMessageHandler();
            var b = new MockMessageHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c, a, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new JsonClient("http://localhost", options));
        }

        [Fact]
        public void CreatePipeline_ACB_ThrowsArgumentException()
        {
            var a = new MockMessageHandler();
            var b = new MockMessageHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, c, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new JsonClient("http://localhost", options));
        }
    }
}

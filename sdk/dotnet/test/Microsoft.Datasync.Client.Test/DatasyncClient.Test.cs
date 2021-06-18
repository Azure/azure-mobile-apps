// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class DatasyncClient_Tests : BaseTest
    {
        private class IdEntity : IEquatable<IdEntity>
        {
            public string Id { get; set; }
            public string StringValue { get; set; }
            public bool Equals(IdEntity other) => Id == other.Id && StringValue == other.StringValue;
            public override bool Equals(object obj) => obj is IdEntity ide && Equals(ide);
            public override int GetHashCode() => Id.GetHashCode() + StringValue.GetHashCode();
        }

        #region Ctor(string)
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_string_ThrowsIfNull()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new DatasyncClient((string)null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("a/b")]
        [InlineData("file://localhost/foo")]
        [InlineData("http://foo.azurewebsites.net")]
        [Trait("Method", "Ctor")]
        public void Ctor_String_ThrowsIfInvalidUri(string endpoint)
        {
            Assert.Throws<UriFormatException>(() => _ = new DatasyncClient(endpoint));
        }

        [Theory]
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://127.0.0.1", "http://127.0.0.1/")]
        [InlineData("https://foo.azurewebsites.net", "https://foo.azurewebsites.net/")]
        [InlineData("http://localhost/tables", "http://localhost/tables/")]
        [InlineData("http://127.0.0.1/tables", "http://127.0.0.1/tables/")]
        [InlineData("https://foo.azurewebsites.net/tables", "https://foo.azurewebsites.net/tables/")]
        [Trait("Method", "Ctor")]
        public void Ctor_String_CreatesValidClient(string endpoint, string expectedUri)
        {
            var client = new DatasyncClient(endpoint);
            Assert.NotNull(client);
            Assert.Equal(expectedUri, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory]
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
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://localhost/", "http://localhost/")]
        [InlineData("http://localhost#fragment", "http://localhost/")]
        [InlineData("http://localhost/#fragment", "http://localhost/")]
        [InlineData("http://localhost?$count=true", "http://localhost/")]
        [InlineData("http://localhost/?$count=true", "http://localhost/")]
        [InlineData("http://localhost#fragment?$count=true", "http://localhost/")]
        [InlineData("http://localhost/#fragment?$count=true", "http://localhost/")]
        [InlineData("http://localhost?$count=true#fragment", "http://localhost/")]
        [InlineData("http://localhost/?$count=true#fragment", "http://localhost/")]
        [Trait("Method", "Ctor")]
        public void Ctor_String_NormalizesEndpoint(string endpoint, string expectedUri)
        {
            var client = new DatasyncClient(endpoint);
            Assert.NotNull(client);
            Assert.Equal(expectedUri, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }
        #endregion

        #region Ctor(Uri)
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_Uri_ThrowsIfNull()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new DatasyncClient((Uri)null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("a/b")]
        [InlineData("file://localhost/foo")]
        [InlineData("http://foo.azurewebsites.net")]
        [Trait("Method", "Ctor")]
        public void Ctor_Uri_ThrowsIfInvalidUri(string endpoint)
        {
            Assert.Throws<UriFormatException>(() => _ = new DatasyncClient(new Uri(endpoint)));
        }

        [Theory]
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://127.0.0.1", "http://127.0.0.1/")]
        [InlineData("https://foo.azurewebsites.net", "https://foo.azurewebsites.net/")]
        [InlineData("http://localhost/tables", "http://localhost/tables/")]
        [InlineData("http://127.0.0.1/tables", "http://127.0.0.1/tables/")]
        [InlineData("https://foo.azurewebsites.net/tables", "https://foo.azurewebsites.net/tables/")]
        [Trait("Method", "Ctor")]
        public void Ctor_Uri_CreatesValidClient(string endpoint, string expectedUri)
        {
            var client = new DatasyncClient(new Uri(endpoint));
            Assert.NotNull(client);
            Assert.Equal(expectedUri, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory]
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
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://localhost/", "http://localhost/")]
        [InlineData("http://localhost#fragment", "http://localhost/")]
        [InlineData("http://localhost/#fragment", "http://localhost/")]
        [InlineData("http://localhost?$count=true", "http://localhost/")]
        [InlineData("http://localhost/?$count=true", "http://localhost/")]
        [InlineData("http://localhost#fragment?$count=true", "http://localhost/")]
        [InlineData("http://localhost/#fragment?$count=true", "http://localhost/")]
        [InlineData("http://localhost?$count=true#fragment", "http://localhost/")]
        [InlineData("http://localhost/?$count=true#fragment", "http://localhost/")]
        [Trait("Method", "Ctor")]
        public void Ctor_Uri_NormalizesEndpoint(string endpoint, string expectedUri)
        {
            var client = new DatasyncClient(new Uri(endpoint));
            Assert.NotNull(client);
            Assert.Equal(expectedUri, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }
        #endregion

        #region Ctor(string, DatasyncClientOptions)
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_StringOptions_ThrowsIfNull()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new DatasyncClient("http://localhost", null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("a/b")]
        [InlineData("file://localhost/foo")]
        [InlineData("http://foo.azurewebsites.net")]
        [Trait("Method", "Ctor")]
        public void Ctor_StringOptions_ThrowsIfInvalidUri(string endpoint)
        {
            var options = new DatasyncClientOptions();
            Assert.Throws<UriFormatException>(() => _ = new DatasyncClient(endpoint, options));
        }

        [Theory]
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://127.0.0.1", "http://127.0.0.1/")]
        [InlineData("https://foo.azurewebsites.net", "https://foo.azurewebsites.net/")]
        [InlineData("http://localhost/tables", "http://localhost/tables/")]
        [InlineData("http://127.0.0.1/tables", "http://127.0.0.1/tables/")]
        [InlineData("https://foo.azurewebsites.net/tables", "https://foo.azurewebsites.net/tables/")]
        [Trait("Method", "Ctor")]
        public void Ctor_StringOptions_CreatesValidClient(string endpoint, string expectedUri)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(endpoint, options);
            Assert.NotNull(client);
            Assert.Equal(expectedUri, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory]
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
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://localhost/", "http://localhost/")]
        [InlineData("http://localhost#fragment", "http://localhost/")]
        [InlineData("http://localhost/#fragment", "http://localhost/")]
        [InlineData("http://localhost?$count=true", "http://localhost/")]
        [InlineData("http://localhost/?$count=true", "http://localhost/")]
        [InlineData("http://localhost#fragment?$count=true", "http://localhost/")]
        [InlineData("http://localhost/#fragment?$count=true", "http://localhost/")]
        [InlineData("http://localhost?$count=true#fragment", "http://localhost/")]
        [InlineData("http://localhost/?$count=true#fragment", "http://localhost/")]
        [Trait("Method", "Ctor")]
        public void Ctor_string_NormalizesEndpoint(string endpoint, string expectedUri)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(endpoint, options);
            Assert.NotNull(client);
            Assert.Equal(expectedUri, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }
        #endregion

        #region Ctor(Uri, DatasyncClientOptions)
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_UriOptions_ThrowsIfNull()
        {
            var uri = new Uri("http://localhost");
            Assert.Throws<ArgumentNullException>(() => _ = new DatasyncClient(uri, null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("a/b")]
        [InlineData("file://localhost/foo")]
        [InlineData("http://foo.azurewebsites.net")]
        [Trait("Method", "Ctor")]
        public void Ctor_UriOptions_ThrowsIfInvalidUri(string endpoint)
        {
            var options = new DatasyncClientOptions();
            Assert.Throws<UriFormatException>(() => _ = new DatasyncClient(new Uri(endpoint), options));
        }

        [Theory]
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://127.0.0.1", "http://127.0.0.1/")]
        [InlineData("https://foo.azurewebsites.net", "https://foo.azurewebsites.net/")]
        [InlineData("http://localhost/tables", "http://localhost/tables/")]
        [InlineData("http://127.0.0.1/tables", "http://127.0.0.1/tables/")]
        [InlineData("https://foo.azurewebsites.net/tables", "https://foo.azurewebsites.net/tables/")]
        [Trait("Method", "Ctor")]
        public void Ctor_UriOptions_CreatesValidClient(string endpoint, string expectedUri)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(new Uri(endpoint), options);
            Assert.NotNull(client);
            Assert.Equal(expectedUri, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory]
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
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://localhost/", "http://localhost/")]
        [InlineData("http://localhost#fragment", "http://localhost/")]
        [InlineData("http://localhost/#fragment", "http://localhost/")]
        [InlineData("http://localhost?$count=true", "http://localhost/")]
        [InlineData("http://localhost/?$count=true", "http://localhost/")]
        [InlineData("http://localhost#fragment?$count=true", "http://localhost/")]
        [InlineData("http://localhost/#fragment?$count=true", "http://localhost/")]
        [InlineData("http://localhost?$count=true#fragment", "http://localhost/")]
        [InlineData("http://localhost/?$count=true#fragment", "http://localhost/")]
        [Trait("Method", "Ctor")]
        public void Ctor_UriOptions_NormalizesEndpoint(string endpoint, string expectedUri)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(new Uri(endpoint), options);
            Assert.NotNull(client);
            Assert.Equal(expectedUri, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }
        #endregion

        #region GetTable<T>()
        [Fact]
        [Trait("Method", "GetTable<T>")]
        public void GetTableT_SetsEndpoint()
        {
            // Arrange
            var client = new DatasyncClient("http://localhost");

            // Act
            var table = client.GetTable<IdEntity>();

            // Assert
            Assert.Equal("http://localhost/tables/identity/", table.Endpoint.ToString());
        }

        [Fact]
        [Trait("Method", "GetTable<T>")]
        public void GetTableT_SetsEndpointWithDefaultTablesUri()
        {
            // Arrange
            var options = new DatasyncClientOptions { TablesUri = "api" };
            var client = new DatasyncClient("http://localhost", options);

            // Act
            var table = client.GetTable<IdEntity>();

            // Assert
            Assert.Equal("http://localhost/api/identity/", table.Endpoint.ToString());
        }
        #endregion

        #region GetTable<T>(string)
        [Fact]
        [Trait("Method", "GetTable<T>")]
        public void GetTableT_String_ThrowsOnNull()
        {
            // Arrange
            var client = new DatasyncClient("http://localhost");

            // Act
            Assert.Throws<ArgumentNullException>(() => _ = client.GetTable<IdEntity>((string)null));
        }

        [Fact]
        [Trait("Method", "GetTable<T>")]
        public void GetTableT_String_ThrowsOnAbsoluteUri()
        {
            // Arrange
            var client = new DatasyncClient("http://localhost");

            // Act
            Assert.Throws<UriFormatException>(() => _ = client.GetTable<IdEntity>("http://localhost/foo?bar"));
        }

        [Fact]
        [Trait("Method", "GetTable<T>")]
        public void GetTableT_String_SetsEndpoint()
        {
            // Arrange
            var client = new DatasyncClient("http://localhost");

            // Act
            var table = client.GetTable<IdEntity>("tables/foo");

            // Assert
            Assert.Equal("http://localhost/tables/foo/", table.Endpoint.ToString());
        }
        #endregion

        #region GetTable<T>(Uri)
        [Fact]
        [Trait("Method", "GetTable<T>")]
        public void GetTableT_Uri_ThrowsOnNull()
        {
            // Arrange
            var client = new DatasyncClient("http://localhost");

            // Act
            Assert.Throws<ArgumentNullException>(() => _ = client.GetTable<IdEntity>((Uri)null));
        }

        [Fact]
        [Trait("Method", "GetTable<T>")]
        public void GetTableT_Uri_ThrowsOnAbsoluteUri()
        {
            // Arrange
            var client = new DatasyncClient("http://localhost");

            // Act
            Assert.Throws<UriFormatException>(() => _ = client.GetTable<IdEntity>(new Uri("http://localhost")));
        }

        [Fact]
        [Trait("Method", "GetTable<T>")]
        public void GetTableT_Uri_SetsEndpoint()
        {
            // Arrange
            var client = new DatasyncClient("http://localhost");

            // Act
            var table = client.GetTable<IdEntity>(new Uri("tables/foo", UriKind.Relative));

            // Assert
            Assert.Equal("http://localhost/tables/foo/", table.Endpoint.ToString());
        }
        #endregion

        #region Dispose
        [Fact]
        [Trait("Method", "Dispose")]
        public async Task Dispose_Correctly()
        {
            var clientHandler = new MockDelegatingHandler();
            clientHandler.AddResponse(HttpStatusCode.NoContent);
            var clientOptions = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { clientHandler } };
            var client = new DatasyncClient(new Uri("https://foo.azurewebsites.net/tables/movies"), clientOptions);

            client.Dispose();

            var request = new HttpRequestMessage(HttpMethod.Delete, client.Endpoint);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.HttpClient.SendAsync(request)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "Dispose")]
        public async Task Dispose_CanRepeat()
        {
            var clientHandler = new MockDelegatingHandler();
            clientHandler.AddResponse(HttpStatusCode.NoContent);
            var clientOptions = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { clientHandler } };
            var client = new DatasyncClient(new Uri("https://foo.azurewebsites.net/tables/movies"), clientOptions);

            client.Dispose();
            client.Dispose();

            var request = new HttpRequestMessage(HttpMethod.Delete, client.Endpoint);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.HttpClient.SendAsync(request)).ConfigureAwait(false);
        }
        #endregion
    }
}

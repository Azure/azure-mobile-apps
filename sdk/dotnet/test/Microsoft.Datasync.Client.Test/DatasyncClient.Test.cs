// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class DatasyncClient_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "Ctor(string)")]
        public void CtorString_Null_Throws()
        {
            const string endpoint = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncClient(endpoint));
        }

        [Theory]
        [InlineData("")]
        [InlineData("http://")]
        [InlineData("file://localhost/foo")]
        [InlineData("http://foo.azurewebsites.net")]
        [InlineData("http://foo.azure-api.net")]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]")]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000")]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000/myapi")]
        [InlineData("http://10.0.0.8")]
        [InlineData("http://10.0.0.8:3000")]
        [InlineData("http://10.0.0.8:3000/myapi")]
        [InlineData("foo/bar")]
        [Trait("Method", "Ctor(string)")]
        public void CtorString_Invalid_Throws(string endpoint)
        {
            Assert.Throws<UriFormatException>(() => new DatasyncClient(endpoint));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(string)")]
        public void CtorString_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var client = new DatasyncClient(endpoint);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(string,AuthenticationProvider)")]
        public void CtorStringAuth_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new DatasyncClient(endpoint, authProvider);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Ctor(Uri)")]
        public void CtorUri_Null_Throws()
        {
            Uri endpoint = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncClient(endpoint));
        }

        [Theory, ClassData(typeof(TestCases.Invalid_Endpoints))]
        [Trait("Method", "Ctor(Uri)")]
        public void CtorUri_Invalid_Throws(string endpoint, bool isRelative)
        {
            Assert.Throws<UriFormatException>(() => new DatasyncClient(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint)));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(Uri)")]
        public void CtorUri_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var client = new DatasyncClient(new Uri(endpoint));
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(Uri,AuthenticationProvider)")]
        public void CtorUriAuth_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new DatasyncClient(new Uri(endpoint), authProvider);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Ctor(string,DatasyncClientOptions)")]
        public void CtorStringOptions_Null_Throws()
        {
            const string endpoint = null;
            DatasyncClientOptions options = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncClient(endpoint, options));
        }

        [Theory]
        [InlineData("")]
        [InlineData("http://")]
        [InlineData("file://localhost/foo")]
        [InlineData("http://foo.azurewebsites.net")]
        [InlineData("http://foo.azure-api.net")]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]")]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000")]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000/myapi")]
        [InlineData("http://10.0.0.8")]
        [InlineData("http://10.0.0.8:3000")]
        [InlineData("http://10.0.0.8:3000/myapi")]
        [InlineData("foo/bar")]
        [Trait("Method", "Ctor(string,DatasyncClientOptions)")]
        public void CtorStringOptions_Invalid_Throws(string endpoint)
        {
            DatasyncClientOptions options = new();
            Assert.Throws<UriFormatException>(() => new DatasyncClient(endpoint, options));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(string,DatasyncClientOptions)")]
        public void CtorStringOptions_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(endpoint, options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(string,AuthenticationProvider,DatasyncClientOptions)")]
        public void CtorStringAuthOptions_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var options = new DatasyncClientOptions();
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new DatasyncClient(endpoint, authProvider, options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Ctor(Uri,DatasyncClientOptions)")]
        public void CtorUriOptions_Null_Throws()
        {
            Uri endpoint = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncClient(endpoint));
        }

        [Theory, ClassData(typeof(TestCases.Invalid_Endpoints))]
        [Trait("Method", "Ctor(Uri,DatasyncClientOptions)")]
        public void CtorUriOptions_Invalid_Throws(string endpoint, bool isRelative)
        {
            Assert.Throws<UriFormatException>(() => new DatasyncClient(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint)));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(Uri,DatasyncClientOptions)")]
        public void CtorUriOptions_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(new Uri(endpoint), options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(Uri,AuthenticationProvider,DatasyncClientOptions)")]
        public void CtorUriAuthOptions_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var options = new DatasyncClientOptions();
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new DatasyncClient(new Uri(endpoint), authProvider, options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_ProducesTable_WithNormalOptions()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetTable<ClientMovie>();
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "tables/clientmovie/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(client.ClientOptions, table.ClientOptions);
        }

        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_ProducesTable_WithTablesPrefix()
        {
            var options = new DatasyncClientOptions { TablesPrefix = "/api" };
            var client = new DatasyncClient(Endpoint, options);
            var table = client.GetTable<ClientMovie>();
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "api/clientmovie/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(options, table.ClientOptions);
        }

        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_stringTable_ProducesTable()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetTable<ClientMovie>("movies");
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "tables/movies/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(client.ClientOptions, table.ClientOptions);
        }

        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_stringTable_ProducesTable_WithTablesPrefix()
        {
            var options = new DatasyncClientOptions { TablesPrefix = "/api" };
            var client = new DatasyncClient(Endpoint, options);
            var table = client.GetTable<ClientMovie>("movies");
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "api/movies/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(options, table.ClientOptions);
        }

        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_stringRelativeUri_ProducesTable()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetTable<ClientMovie>("/api/movies");
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "api/movies/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(client.ClientOptions, table.ClientOptions);
        }

        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_stringRelativeUri_ProducesTable_WithTablesPrefix()
        {
            var options = new DatasyncClientOptions { TablesPrefix = "/api" };
            var client = new DatasyncClient(Endpoint, options);
            var table = client.GetTable<ClientMovie>("/foo/movies");
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "foo/movies/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(options, table.ClientOptions);
        }
    }
}

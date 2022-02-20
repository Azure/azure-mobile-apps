// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Datasync.Common.Test.TestData;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage]
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

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "Ctor(string)")]
        public void CtorString_Valid_SetsEndpoint(EndpointTestCase testcase)
        {
            var client = new DatasyncClient(testcase.BaseEndpoint);
            Assert.Equal(testcase.NormalizedEndpoint, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "Ctor(string,AuthenticationProvider)")]
        public void CtorStringAuth_Valid_SetsEndpoint(EndpointTestCase testcase)
        {
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new DatasyncClient(testcase.BaseEndpoint, authProvider);
            Assert.Equal(testcase.NormalizedEndpoint, client.Endpoint.ToString());
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

        [Theory]
        [InlineData("", false)]
        [InlineData("", true)]
        [InlineData("http://", false)]
        [InlineData("http://", true)]
        [InlineData("file://localhost/foo", false)]
        [InlineData("http://foo.azurewebsites.net", false)]
        [InlineData("http://foo.azure-api.net", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000/myapi", false)]
        [InlineData("http://10.0.0.8", false)]
        [InlineData("http://10.0.0.8:3000", false)]
        [InlineData("http://10.0.0.8:3000/myapi", false)]
        [InlineData("foo/bar", true)]
        [Trait("Method", "Ctor(Uri)")]
        public void CtorUri_Invalid_Throws(string endpoint, bool isRelative)
        {
            Assert.Throws<UriFormatException>(() => new DatasyncClient(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint)));
        }

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "Ctor(Uri)")]
        public void CtorUri_Valid_SetsEndpoint(EndpointTestCase testcase)
        {
            var client = new DatasyncClient(new Uri(testcase.BaseEndpoint));
            Assert.Equal(testcase.NormalizedEndpoint, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "Ctor(Uri,AuthenticationProvider)")]
        public void CtorUriAuth_Valid_SetsEndpoint(EndpointTestCase testcase)
        {
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new DatasyncClient(new Uri(testcase.BaseEndpoint), authProvider);
            Assert.Equal(testcase.NormalizedEndpoint, client.Endpoint.ToString());
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

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "Ctor(string,DatasyncClientOptions)")]
        public void CtorStringOptions_Valid_SetsEndpoint(EndpointTestCase testcase)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(testcase.BaseEndpoint, options);
            Assert.Equal(testcase.NormalizedEndpoint, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "Ctor(string,AuthenticationProvider,DatasyncClientOptions)")]
        public void CtorStringAuthOptions_Valid_SetsEndpoint(EndpointTestCase testcase)
        {
            var options = new DatasyncClientOptions();
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new DatasyncClient(testcase.BaseEndpoint, authProvider, options);
            Assert.Equal(testcase.NormalizedEndpoint, client.Endpoint.ToString());
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

        [Theory]
        [InlineData("", false)]
        [InlineData("", true)]
        [InlineData("http://", false)]
        [InlineData("http://", true)]
        [InlineData("file://localhost/foo", false)]
        [InlineData("http://foo.azurewebsites.net", false)]
        [InlineData("http://foo.azure-api.net", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000/myapi", false)]
        [InlineData("http://10.0.0.8", false)]
        [InlineData("http://10.0.0.8:3000", false)]
        [InlineData("http://10.0.0.8:3000/myapi", false)]
        [InlineData("foo/bar", true)]
        [Trait("Method", "Ctor(Uri,DatasyncClientOptions)")]
        public void CtorUriOptions_Invalid_Throws(string endpoint, bool isRelative)
        {
            Assert.Throws<UriFormatException>(() => new DatasyncClient(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint)));
        }

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "Ctor(Uri,DatasyncClientOptions)")]
        public void CtorUriOptions_Valid_SetsEndpoint(EndpointTestCase testcase)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(new Uri(testcase.BaseEndpoint), options);
            Assert.Equal(testcase.NormalizedEndpoint, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "Ctor(Uri,AuthenticationProvider,DatasyncClientOptions)")]
        public void CtorUriAuthOptions_Valid_SetsEndpoint(EndpointTestCase testcase)
        {
            var options = new DatasyncClientOptions();
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new DatasyncClient(new Uri(testcase.BaseEndpoint), authProvider, options);
            Assert.Equal(testcase.NormalizedEndpoint, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "InstallationId")]
        public void InstallationId_IsValid()
        {
            var client = new DatasyncClient(Endpoint);
            Assert.NotEmpty(client.InstallationId);
        }

        [Fact]
        [Trait("Method", "InstallationId")]
        public void InstallationId_CanBeOverridden()
        {
            var options = new DatasyncClientOptions { InstallationId = "hijack" };
            var client = new DatasyncClient(Endpoint, options);
            Assert.Equal("hijack", client.InstallationId);
        }

        [Fact]
        [Trait("Method", "GetRemoteTable(string)")]
        public void GetRemoteTable_ProducesTable()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetRemoteTable("movies");
            Assert.IsAssignableFrom<IRemoteTable>(table);
            Assert.Same(client, table.ServiceClient);
            Assert.Equal("movies", table.TableName);
        }

        [Fact]
        [Trait("Method", "GetRemoteTable(string)")]
        public void GetRemoteTable_Throws_OnNullTableName()
        {
            var client = new DatasyncClient(Endpoint);
            Assert.Throws<ArgumentNullException>(() => client.GetRemoteTable(null));
        }

        [Fact]
        [Trait("Method", "GetRemoteTable(string)")]
        public void GetRemoteTable_Throws_OnInvalidTableName()
        {
            var client = new DatasyncClient(Endpoint);
            Assert.Throws<ArgumentException>(() => client.GetRemoteTable("    "));
        }

        [Fact]
        [Trait("Method", "GetRemoteTable<T>()")]
        public void GetRemoteTableOfT_ProducesTable()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetRemoteTable<ClientMovie>();
            Assert.IsAssignableFrom<IRemoteTable<ClientMovie>>(table);
            Assert.Same(client, table.ServiceClient);
            Assert.Equal("clientmovie", table.TableName);
        }

        [Fact]
        [Trait("Method", "GetRemoteTable<T>(string)")]
        public void GetRemoteTableOfT_String_ProducesTable()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetRemoteTable<ClientMovie>("movies");
            Assert.IsAssignableFrom<IRemoteTable<ClientMovie>>(table);
            Assert.Same(client, table.ServiceClient);
            Assert.Equal("movies", table.TableName);
        }


        [Fact]
        [Trait("Method", "GetRemoteTable<T>(string)")]
        public void GetRemoteTableOfT_Throws_OnNullTableName()
        {
            var client = new DatasyncClient(Endpoint);
            Assert.Throws<ArgumentNullException>(() => client.GetRemoteTable<ClientMovie>(null));
        }

        [Fact]
        [Trait("Method", "GetRemoteTable<T>(string)")]
        public void GetRemoteTableOfT_Throws_OnInvalidTableName()
        {
            var client = new DatasyncClient(Endpoint);
            Assert.Throws<ArgumentException>(() => client.GetRemoteTable<ClientMovie>("    "));
        }

        [Fact]
        [Trait("Method", "GetOfflineTable(string)")]
        public void GetOfflineTable_ProducesTable()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetOfflineTable("movies");
            Assert.IsAssignableFrom<IOfflineTable>(table);
            Assert.Same(client, table.ServiceClient);
            Assert.Equal("movies", table.TableName);
        }

        [Fact]
        [Trait("Method", "GetOfflineTable(string)")]
        public void GetOfflineTable_Throws_OnNullTableName()
        {
            var client = new DatasyncClient(Endpoint);
            Assert.Throws<ArgumentNullException>(() => client.GetOfflineTable(null));
        }

        [Fact]
        [Trait("Method", "GetOfflineTable(string)")]
        public void GetOfflineTable_Throws_OnInvalidTableName()
        {
            var client = new DatasyncClient(Endpoint);
            Assert.Throws<ArgumentException>(() => client.GetOfflineTable("    "));
        }

        [Fact]
        [Trait("Method", "GetOfflineTable<T>()")]
        public void GetOfflineTableOfT_ProducesTable()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetOfflineTable<ClientMovie>();
            Assert.IsAssignableFrom<IOfflineTable<ClientMovie>>(table);
            Assert.Same(client, table.ServiceClient);
            Assert.Equal("clientmovie", table.TableName);
        }

        [Fact]
        [Trait("Method", "GetOfflineTable<T>(string)")]
        public void GetOfflineTableOfT_String_ProducesTable()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetOfflineTable<ClientMovie>("movies");
            Assert.IsAssignableFrom<IOfflineTable<ClientMovie>>(table);
            Assert.Same(client, table.ServiceClient);
            Assert.Equal("movies", table.TableName);
        }


        [Fact]
        [Trait("Method", "GetOfflineTable<T>(string)")]
        public void GetOfflineTableOfT_Throws_OnNullTableName()
        {
            var client = new DatasyncClient(Endpoint);
            Assert.Throws<ArgumentNullException>(() => client.GetOfflineTable<ClientMovie>(null));
        }

        [Fact]
        [Trait("Method", "GetOfflineTable<T>(string)")]
        public void GetOfflineTableOfT_Throws_OnInvalidTableName()
        {
            var client = new DatasyncClient(Endpoint);
            Assert.Throws<ArgumentException>(() => client.GetOfflineTable<ClientMovie>("    "));
        }
    }
}

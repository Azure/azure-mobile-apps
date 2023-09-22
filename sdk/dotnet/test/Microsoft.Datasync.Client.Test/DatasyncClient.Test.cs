// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Test.Helpers;

namespace Microsoft.Datasync.Client.Test;

[ExcludeFromCodeCoverage]
public class DatasyncClient_Tests : BaseTest
{
    private readonly DatasyncClientOptions clientOptions;

    #region Test Case Helpers
    public DatasyncClient_Tests() : base()
    {
        var store = Substitute.For<IOfflineStore>();
        clientOptions = new DatasyncClientOptions { OfflineStore = store };
    }

    /// <summary>
    /// A set of invalid endpoints for testing
    /// </summary>
    public static IEnumerable<object[]> GetInvalidEndpoints() => new List<object[]>
    {
        new object[] { "" },
        new object[] { "http://" },
        new object[] { "file://localhost/foo" },
        new object[] { "http://foo.azurewebsites.net" },
        new object[] { "http://foo.azure-api.net" },
        new object[] { "http://[2001:db8:0:b:0:0:0:1A]" },
        new object[] { "http://[2001:db8:0:b:0:0:0:1A]:3000" },
        new object[] { "http://[2001:db8:0:b:0:0:0:1A]:3000/myapi" },
        new object[] { "http://10.0.0.8" },
        new object[] { "http://10.0.0.8:3000" },
        new object[] { "http://10.0.0.8:3000/myapi" },
        new object[] { "foo/bar" }
    };

    public static IEnumerable<object[]> GetInvalidEndpointsWithFlag() => new List<object[]>
    {
        new object[] { "", false },
        new object[] { "", true },
        new object[] { "http://", false },
        new object[] { "http://", true },
        new object[] { "file://localhost/foo", false },
        new object[] { "http://foo.azurewebsites.net", false },
        new object[] { "http://foo.azure-api.net", false },
        new object[] { "http://[2001:db8:0:b:0:0:0:1A]", false },
        new object[] { "http://[2001:db8:0:b:0:0:0:1A]:3000", false },
        new object[] { "http://[2001:db8:0:b:0:0:0:1A]:3000/myapi", false },
        new object[] { "http://10.0.0.8", false },
        new object[] { "http://10.0.0.8:3000", false },
        new object[] { "http://10.0.0.8:3000/myapi", false },
        new object[] { "foo/bar", true }
    };
    #endregion

    [Fact]
    [Trait("Method", "Ctor(string)")]
    public void CtorString_Null_Throws()
    {
        const string endpoint = null;
        Assert.Throws<ArgumentNullException>(() => new DatasyncClient(endpoint));
    }

    [Theory]
    [MemberData(nameof(GetInvalidEndpoints))]
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
    [MemberData(nameof(GetInvalidEndpointsWithFlag))]
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
    [MemberData(nameof(GetInvalidEndpoints))]
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
    [MemberData(nameof(GetInvalidEndpointsWithFlag))]
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
    public void InstallationId_CanBeOverridden()
    {
        var options = new DatasyncClientOptions { InstallationId = "hijack" };
        var client = new DatasyncClient(Endpoint, options);
        Assert.Equal("hijack", client.InstallationId);
    }

    [Fact]
    [Trait("Method", "SerializerSettings")]
    public void SerializerSettings_CopiedToSerializer()
    {
        var settings = new DatasyncSerializerSettings { CamelCasePropertyNames = true };
        var options = new DatasyncClientOptions { SerializerSettings = settings };
        var client = new DatasyncClient(Endpoint, options);
        Assert.Same(settings, client.Serializer.SerializerSettings);
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
    public void GetRemoteTableOfT_String_OnNullTableName()
    {
        var client = new DatasyncClient(Endpoint);
        var table = client.GetRemoteTable<ClientMovie>(null);
        Assert.IsAssignableFrom<IRemoteTable<ClientMovie>>(table);
        Assert.Same(client, table.ServiceClient);
        Assert.Equal("clientmovie", table.TableName);
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
        var client = new DatasyncClient(Endpoint, clientOptions);
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
    [Trait("Method", "GetOfflineTable(string)")]
    public void GetOfflineTable_Throws_WhenNoStore()
    {
        var client = new DatasyncClient(Endpoint);
        Assert.Throws<InvalidOperationException>(() => client.GetOfflineTable("movies"));
    }

    [Fact]
    [Trait("Method", "GetOfflineTable<T>()")]
    public void GetOfflineTableOfT_ProducesTable()
    {
        var client = new DatasyncClient(Endpoint, clientOptions);
        var table = client.GetOfflineTable<ClientMovie>();
        Assert.IsAssignableFrom<IOfflineTable<ClientMovie>>(table);
        Assert.Same(client, table.ServiceClient);
        Assert.Equal("clientmovie", table.TableName);
    }

    [Fact]
    [Trait("Method", "GetOfflineTable<T>(string)")]
    public void GetOfflineTableOfT_String_ProducesTable()
    {
        var client = new DatasyncClient(Endpoint, clientOptions);
        var table = client.GetOfflineTable<ClientMovie>("movies");
        Assert.IsAssignableFrom<IOfflineTable<ClientMovie>>(table);
        Assert.Same(client, table.ServiceClient);
        Assert.Equal("movies", table.TableName);
    }

    [Fact]
    [Trait("Method", "GetOfflineTable<T>(string)")]
    public void GetOfflineTableOfT_String_OnNullTableName()
    {
        var client = new DatasyncClient(Endpoint, clientOptions);
        var table = client.GetOfflineTable<ClientMovie>(null);
        Assert.IsAssignableFrom<IOfflineTable<ClientMovie>>(table);
        Assert.Same(client, table.ServiceClient);
        Assert.Equal("clientmovie", table.TableName);
    }

    [Fact]
    [Trait("Method", "GetOfflineTable<T>(string)")]
    public void GetOfflineTableOfT_Throws_OnInvalidTableName()
    {
        var client = new DatasyncClient(Endpoint);
        Assert.Throws<ArgumentException>(() => client.GetOfflineTable<ClientMovie>("    "));
    }

    [Fact]
    [Trait("Method", "GetOfflineTable<T>(string)")]
    public void GetOfflineTableOfT_Throws_WhenNoStore()
    {
        var client = new DatasyncClient(Endpoint);
        Assert.Throws<InvalidOperationException>(() => client.GetOfflineTable<ClientMovie>("movies"));
    }

    [Fact]
    [Trait("Method", "InitializeOfflineStoreAsync")]
    public async Task InitializeOfflineStoreAsync_Throws_WhenNoStore()
    {
        var client = new DatasyncClient(Endpoint);
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.InitializeOfflineStoreAsync());
    }

    [Fact]
    [Trait("Method", "InitializeOfflineStoreAsync")]
    public async Task InitializedOfflineStoreAsync_CallsInit_WhenStore()
    {
        var store = new MockOfflineStore();
        var client = new DatasyncClient(Endpoint, new DatasyncClientOptions { OfflineStore = store });

        await client.InitializeOfflineStoreAsync();

        Assert.True(client.SyncContext.IsInitialized);
    }
}

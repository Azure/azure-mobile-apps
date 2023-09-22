// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;

namespace Microsoft.Datasync.Client.Test.Table;

[ExcludeFromCodeCoverage]
public class OfflineTable_generic_Tests : BaseTest
{
    private readonly MockOfflineStore store;
    private readonly DatasyncClient client;

    public OfflineTable_generic_Tests() : base()
    {
        store = new MockOfflineStore();
        client = GetMockClient(null, store);
    }

    [Fact]
    public void Ctor_Throws_OnNullTableName()
    {
        var client = GetMockClient();
        Assert.Throws<ArgumentNullException>(() => new OfflineTable<ClientMovie>(null, client));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("abcdef gh")]
    [InlineData("!!!")]
    [InlineData("?")]
    [InlineData(";")]
    [InlineData("{EA235ADF-9F38-44EA-8DA4-EF3D24755767}")]
    [InlineData("###")]
    [InlineData("1abcd")]
    [InlineData("true.false")]
    [InlineData("a-b-c-d")]
    public void Ctor_Throws_OnInvalidTable(string tableName)
    {
        var client = GetMockClient();
        Assert.Throws<ArgumentException>(() => new OfflineTable<ClientMovie>(tableName, client));
    }

    [Fact]
    public void Ctor_Throws_OnNullClient()
    {
        Assert.Throws<ArgumentNullException>(() => new OfflineTable<ClientMovie>("movies", null));
    }

    [Fact]
    public void Ctor_Throws_WhenNoOfflineStore()
    {
        var client = GetMockClient();
        Assert.Throws<InvalidOperationException>(() => new OfflineTable<ClientMovie>("movies", client));
    }

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_NoId_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new OfflineTable<BadEntityNoId>("movies", client));
    }

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_IntId_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new RemoteTable<BadEntityIntId>("movies", client));
    }

    [Fact]
    public void Ctor_CreateTable_WhenArgsCorrect()
    {
        var table = new OfflineTable<ClientMovie>("movies", client);

        Assert.Same(client, table.ServiceClient);
        Assert.Equal("movies", table.TableName);
    }

    #region ILinqMethods<T>
    [Fact]
    [Trait("Method", "IncludeDeletedItems")]
    public async Task ToODataString_IncludeDeletedItems_IsWellFormed()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.IncludeDeletedItems() as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("__includedeleted=true", odata);
    }

    [Fact]
    [Trait("Method", "IncludeTotalCount")]
    public async Task ToODataString_IncludeTotalCount_IsWellFormed()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.IncludeTotalCount() as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$count=true", odata);
    }

    [Fact]
    [Trait("Method", "OrderBy")]
    public async Task ToODataString_OrderBy_IsWellFormed()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.OrderBy(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id", odata);
    }

    [Fact]
    [Trait("Method", "OrderByDescending")]
    public async Task ToODataString_OrderByDescending_IsWellFormed()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.OrderByDescending(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id desc", odata);
    }

    [Fact]
    [Trait("Method", "Select")]
    public async Task ToODataString_Select_IsWellFormed()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.Select(m => new IdOnly { Id = m.Id }) as TableQuery<IdOnly>;
        var odata = query.ToODataString();
        Assert.Equal("$select=id", odata);
    }

    [Fact]
    [Trait("Method", "Skip")]
    public async Task ToODataString_Skip_IsWellFormed()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.Skip(5) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$skip=5", odata);
    }

    [Fact]
    [Trait("Method", "Take")]
    public async Task ToODataString_Take_IsWellFormed()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.Take(5) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$top=5", odata);
    }

    [Fact]
    [Trait("Method", "ThenBy")]
    public async Task ToODataString_ThenBy_IsWellFormed()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.ThenBy(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id", odata);
    }

    [Fact]
    [Trait("Method", "ThenByDescending")]
    public async Task ToODataString_ThenByDescending_IsWellFormed()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.ThenByDescending(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id desc", odata);
    }

    [Fact]
    [Trait("Method", "Where")]
    public async Task ToODataString_Where_IsWellFormed()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.Where(m => m.Id == "foo") as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$filter=%28id%20eq%20%27foo%27%29", odata);
    }

    [Fact]
    [Trait("Method", "WithParameter")]
    public async Task ToODataString_WithParameter_isWellFormed()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.WithParameter("testkey", "testvalue") as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("testkey=testvalue", odata);
    }

    [Fact]
    [Trait("Method", "WithParameter")]
    public async Task ToODataString_WithParameter_EncodesValue()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var query = table.WithParameter("testkey", "test value") as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("testkey=test%20value", odata);
    }

    [Fact]
    [Trait("Method", "WithParameters")]
    public async Task ToODataString_WithParameters_EncodesValue()
    {
        await client.InitializeOfflineStoreAsync();
        var table = client.GetOfflineTable<IdEntity>("movies");
        var sut = new Dictionary<string, string>()
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };
        var query = table.WithParameters(sut) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("key1=value1&key2=value2", odata);
    }
    #endregion
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;

namespace Microsoft.Datasync.Client.Test.Table;

[ExcludeFromCodeCoverage]
public class RemoteTable_generic_Tests : BaseTest
{
    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_SetsInternals()
    {
        var client = GetMockClient();
        var sut = new RemoteTable<ClientMovie>("movies", client);

        Assert.Equal("movies", sut.TableName);
        Assert.Same(client, sut.ServiceClient);
    }

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_NullTableName_Throws()
    {
        var client = GetMockClient();
        Assert.Throws<ArgumentNullException>(() => new RemoteTable<ClientMovie>(null, client));
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
    [Trait("Method", "IsValidTableName")]
    public void Ctor_InvalidTableName_Throws(string sut)
    {
        var client = GetMockClient();
        Assert.Throws<ArgumentException>(() => new RemoteTable<ClientMovie>(sut, client));
    }

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_NullClient_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RemoteTable<ClientMovie>("movies", null));
    }

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_NoId_Throws()
    {
        var client = GetMockClient();
        Assert.ThrowsAny<Exception>(() => new RemoteTable<BadEntityNoId>("movies", client));
    }

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_IntId_Throws()
    {
        var client = GetMockClient();
        Assert.ThrowsAny<Exception>(() => new RemoteTable<BadEntityIntId>("movies", client));
    }

    [Fact]
    [Trait("Method", "CreateQuery")]
    public void CreateQuery_ProducesQuery()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<ClientMovie>();
        var query = table.CreateQuery();

        Assert.NotNull(query);
        Assert.Same(table, query.RemoteTable);
    }

    #region ILinqMethods<T>
    [Fact]
    [Trait("Method", "IncludeDeletedItems")]
    public void ToODataString_IncludeDeletedItems_IsWellFormed()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.IncludeDeletedItems() as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("__includedeleted=true", odata);
    }

    [Fact]
    [Trait("Method", "IncludeTotalCount")]
    public void ToODataString_IncludeTotalCount_IsWellFormed()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.IncludeTotalCount() as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$count=true", odata);
    }

    [Fact]
    [Trait("Method", "OrderBy")]
    public void ToODataString_OrderBy_IsWellFormed()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.OrderBy(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id", odata);
    }

    [Fact]
    [Trait("Method", "OrderByDescending")]
    public void ToODataString_OrderByDescending_IsWellFormed()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.OrderByDescending(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id desc", odata);
    }

    [Fact]
    [Trait("Method", "Select")]
    public void ToODataString_Select_IsWellFormed()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.Select(m => new IdOnly { Id = m.Id }) as TableQuery<IdOnly>;
        var odata = query.ToODataString();
        Assert.Equal("$select=id", odata);
    }

    [Fact]
    [Trait("Method", "Skip")]
    public void ToODataString_Skip_IsWellFormed()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.Skip(5) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$skip=5", odata);
    }

    [Fact]
    [Trait("Method", "Take")]
    public void ToODataString_Take_IsWellFormed()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.Take(5) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$top=5", odata);
    }

    [Fact]
    [Trait("Method", "ThenBy")]
    public void ToODataString_ThenBy_IsWellFormed()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.ThenBy(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id", odata);
    }

    [Fact]
    [Trait("Method", "ThenByDescending")]
    public void ToODataString_ThenByDescending_IsWellFormed()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.ThenByDescending(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id desc", odata);
    }

    [Fact]
    [Trait("Method", "Where")]
    public void ToODataString_Where_IsWellFormed()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.Where(m => m.Id == "foo") as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$filter=%28id%20eq%20%27foo%27%29", odata);
    }

    [Fact]
    [Trait("Method", "WithParameter")]
    public void ToODataString_WithParameter_isWellFormed()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.WithParameter("testkey", "testvalue") as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("testkey=testvalue", odata);
    }

    [Fact]
    [Trait("Method", "WithParameter")]
    public void ToODataString_WithParameter_EncodesValue()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
        var query = table.WithParameter("testkey", "test value") as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("testkey=test%20value", odata);
    }

    [Fact]
    [Trait("Method", "WithParameters")]
    public void ToODataString_WithParameters_EncodesValue()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<IdEntity>("movies");
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

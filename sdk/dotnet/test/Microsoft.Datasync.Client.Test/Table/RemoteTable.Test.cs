// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Table;

namespace Microsoft.Datasync.Client.Test.Table;

[ExcludeFromCodeCoverage]
public class RemoteTable_Tests : BaseTest
{
    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_SetsInternals()
    {
        var client = GetMockClient();
        var sut = new RemoteTable("movies", client);

        Assert.Equal("movies", sut.TableName);
        Assert.Same(client, sut.ServiceClient);
    }

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_NullTableName_Throws()
    {
        var client = GetMockClient();
        Assert.Throws<ArgumentNullException>(() => new RemoteTable(null, client));
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
        Assert.Throws<ArgumentException>(() => new RemoteTable(sut, client));
    }

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_NullClient_Throws()
    {
        var client = GetMockClient();
        Assert.Throws<ArgumentNullException>(() => new RemoteTable("movies", null));
    }

    [Fact]
    [Trait("Method", "TableEndpoint")]
    public void TableEndpoint_ReturnsDefaultValue()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable("foo") as RemoteTable;
        Assert.Equal("tables/foo", table.TableEndpoint);
    }

    [Fact]
    [Trait("Method", "TableEndpoint")]
    public void TableEndpoint_UsesTableResolver()
    {
        var client = GetMockClient();
        client.ClientOptions.TableEndpointResolver = (tableName) => $"/api/{tableName}";
        var table = client.GetRemoteTable("foo") as RemoteTable;
        Assert.Equal("/api/foo", table.TableEndpoint);
    }
}

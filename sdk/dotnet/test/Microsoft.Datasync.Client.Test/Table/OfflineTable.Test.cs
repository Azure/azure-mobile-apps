// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Table;

namespace Microsoft.Datasync.Client.Test.Table;

[ExcludeFromCodeCoverage]
public class OfflineTable_Tests : BaseTest
{
    [Fact]
    public void Ctor_Throws_OnNullTableName()
    {
        var client = GetMockClient();
        Assert.Throws<ArgumentNullException>(() => new OfflineTable(null, client));
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
        Assert.Throws<ArgumentException>(() => new OfflineTable(tableName, client));
    }

    [Fact]
    public void Ctor_Throws_OnNullClient()
    {
        Assert.Throws<ArgumentNullException>(() => new OfflineTable("movies", null));
    }

    [Fact]
    public void Ctor_Throws_WhenNoOfflineStore()
    {
        var client = GetMockClient();
        Assert.Throws<InvalidOperationException>(() => new OfflineTable("movies", client));
    }

    [Fact]
    public void Ctor_CreateTable_WhenArgsCorrect()
    {
        var store = new MockOfflineStore();
        var options = new DatasyncClientOptions { OfflineStore = store };
        var client = new DatasyncClient(Endpoint, options);
        var table = new OfflineTable("movies", client);

        Assert.Same(client, table.ServiceClient);
        Assert.Equal("movies", table.TableName);
    }
}

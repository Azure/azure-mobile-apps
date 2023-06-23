// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Test.Query;

[ExcludeFromCodeCoverage]
public class Linq_Tests : ClientBaseTest
{
    private readonly DatasyncClient _client;
    private readonly RemoteTable<KitchenSink> _table;
    private readonly TableQuery<KitchenSink> _query;

    public Linq_Tests()
    {
        _client = GetMockClient();
        _table = _client.GetRemoteTable<KitchenSink>("kitchensink") as RemoteTable<KitchenSink>;
        _query = new TableQuery<KitchenSink>(_table);
    }

    [Fact]
    public void Linq_DerivedTest()
    {
        var client = GetMockClient();
        var table = client.GetRemoteTable<DerivedTestDatasyncEntity>("derived");
        var queryString = GetDerivedQuery(table);
        Assert.Equal("$filter=deleted", queryString);
    }

    public static string GetDerivedQuery<T>(IRemoteTable<T> table) where T : IDatasyncEntity
    {
        return table.Where(x => x.Deleted).ToODataString();
    }

    [Fact]
    public void Linq_EndsWith_NoStringComparison()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.EndsWith("abc"),
            "$filter=endswith%28stringProperty%2C%27abc%27%29"
        );
    }

    [Fact]
    public void Linq_EndsWith_Ordinal()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.EndsWith("abc", StringComparison.Ordinal),
            "$filter=endswith%28stringProperty%2C%27abc%27%29"
        );
    }

    [Fact]
    public void Linq_EndsWith_Invariant()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.EndsWith("abc", StringComparison.InvariantCulture),
            "$filter=endswith%28stringProperty%2C%27abc%27%29"
        );
    }

    [Fact]
    public void Linq_EndsWith_OrdinalIgnoreCase()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.EndsWith("abc", StringComparison.OrdinalIgnoreCase),
            "$filter=endswith%28tolower%28stringProperty%29%2Ctolower%28%27abc%27%29%29"
        );
    }

    [Fact]
    public void Linq_EndsWith_InvariantIgnoreCase()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.EndsWith("abc", StringComparison.InvariantCultureIgnoreCase),
            "$filter=endswith%28tolower%28stringProperty%29%2Ctolower%28%27abc%27%29%29"
        );
    }

    [Fact]
    public void Linq_Equals_NoStringComparison()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.Equals("abc"),
            "$filter=%28stringProperty%20eq%20%27abc%27%29"
        );
    }

    [Fact]
    public void Linq_Equals_Ordinal()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.Equals("abc", StringComparison.Ordinal),
            "$filter=%28stringProperty%20eq%20%27abc%27%29"
        );
    }

    [Fact]
    public void Linq_Equals_Invariant()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.Equals("abc", StringComparison.InvariantCulture),
            "$filter=%28stringProperty%20eq%20%27abc%27%29"
        );
    }

    [Fact]
    public void Linq_Equals_OrdinalIgnoreCase()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.Equals("abc", StringComparison.OrdinalIgnoreCase),
            "$filter=%28tolower%28stringProperty%29%20eq%20tolower%28%27abc%27%29%29"
        );
    }

    [Fact]
    public void Linq_Equals_InvariantIgnoreCase()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.Equals("abc", StringComparison.InvariantCultureIgnoreCase),
            "$filter=%28tolower%28stringProperty%29%20eq%20tolower%28%27abc%27%29%29"
        );
    }

    [Fact]
    public void Linq_StartsWith_NoStringComparison()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.StartsWith("abc"),
            "$filter=startswith%28stringProperty%2C%27abc%27%29"
        );
    }

    [Fact]
    public void Linq_StartsWith_Ordinal()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.StartsWith("abc", StringComparison.Ordinal),
            "$filter=startswith%28stringProperty%2C%27abc%27%29"
        );
    }

    [Fact]
    public void Linq_StartsWith_Invariant()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.StartsWith("abc", StringComparison.InvariantCulture),
            "$filter=startswith%28stringProperty%2C%27abc%27%29"
        );
    }

    [Fact]
    public void Linq_StartsWith_OrdinalIgnoreCase()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.StartsWith("abc", StringComparison.OrdinalIgnoreCase),
            "$filter=startswith%28tolower%28stringProperty%29%2Ctolower%28%27abc%27%29%29"
        );
    }

    [Fact]
    public void Linq_StartsWith_InvariantIgnoreCase()
    {
        ExecuteWhereQuery(
            m => m.StringProperty.StartsWith("abc", StringComparison.InvariantCultureIgnoreCase),
            "$filter=startswith%28tolower%28stringProperty%29%2Ctolower%28%27abc%27%29%29"
        );
    }

    private void ExecuteWhereQuery(Expression<Func<KitchenSink, bool>> predicate, string expected)
    {
        var sut = _query.Where(predicate);
        var actual = ((TableQuery<KitchenSink>)sut).ToODataString(true);
        Assert.Equal(expected, actual);
    }

    public class KitchenSink : DatasyncClientData
    {
        public string StringProperty { get; set; }
    }
}

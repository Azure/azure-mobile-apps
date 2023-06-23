// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Test.Query;

[ExcludeFromCodeCoverage]
public class TableQuery_Tests : ClientBaseTest
{
    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_NullTable_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new TableQuery<IdEntity>(null));
    }

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_BlankSetup()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Assert.Same(table, query.RemoteTable);
        Assert.IsAssignableFrom<IQueryable<IdEntity>>(query.Query);
        Assert.Empty(query.Parameters);
    }

    [Fact]
    [Trait("Method", "IncludeDeletedItems")]
    public void IncludeDeletedItems_Enabled_ChangesKey()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        query.Parameters.Add("__includedeleted", "test");
        var actual = query.IncludeDeletedItems() as TableQuery<IdEntity>;
        AssertEx.Contains("__includedeleted", "true", actual.Parameters);
    }

    [Fact]
    [Trait("Method", "IncludeDeletedItems")]
    public void IncludeDeletedItems_Disabled_RemovesKey()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        query.Parameters.Add("__includedeleted", "true");
        var actual = query.IncludeDeletedItems(false) as TableQuery<IdEntity>;
        Assert.False(actual.Parameters.ContainsKey("__includedeleted"));
    }

    [Fact]
    [Trait("Method", "IncludeDeletedItems")]
    public void IncludeDeletedItems_Disabled_WorksWithEmptyParameters()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        var actual = query.IncludeDeletedItems(false) as TableQuery<IdEntity>;
        Assert.False(actual.Parameters.ContainsKey("__includedeleted"));
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "IncludeDeletedItems")]
    public void ToODataString_IncludeDeletedItems_IsWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).IncludeDeletedItems() as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("__includedeleted=true", odata);
    }

    [Fact]
    [Trait("Method", "IncludeTotalCount")]
    public void IncludeTotalCount_Enabled_AddsKey()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).IncludeTotalCount(true) as TableQuery<IdEntity>;
        Assert.True(query.RequestTotalCount);
    }

    [Fact]
    [Trait("Method", "IncludeTotalCount")]
    public void IncludeTotalCount_Disabled_RemovesKey()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).IncludeTotalCount(false) as TableQuery<IdEntity>;
        Assert.False(query.RequestTotalCount);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "IncludeTotalCount")]
    public void ToODataString_IncludeTotalCount_IsWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).IncludeTotalCount() as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$count=true", odata);
    }

    [Fact]
    [Trait("Method", "OrderBy")]
    public void OrderBy_Null_Throws()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Expression<Func<IdEntity, string>> keySelector = null;
        Assert.Throws<ArgumentNullException>(() => query.OrderBy(keySelector));
    }

    [Fact]
    [Trait("Method", "OrderBy")]
    public void OrderBy_UpdatesQuery()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        var actual = query.OrderBy(m => m.Id) as TableQuery<IdEntity>;
        Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
        var expression = actual.Query.Expression as MethodCallExpression;
        Assert.Equal("OrderBy", expression.Method.Name);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "OrderBy")]
    public void ToODataString_OrderBy_IsWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).OrderBy(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id", odata);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "OrderBy")]
    public void ToODataString_OrderBy_ThrowsNotSupported()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).OrderBy(m => m.Id.ToLower()) as TableQuery<IdEntity>;
        Assert.Throws<NotSupportedException>(() => query.ToODataString());
    }

    [Fact]
    [Trait("Method", "OrderByDescending")]
    public void OrderByDescending_Null_Throws()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Expression<Func<IdEntity, string>> keySelector = null;
        Assert.Throws<ArgumentNullException>(() => query.OrderByDescending(keySelector));
    }

    [Fact]
    [Trait("Method", "OrderByDescending")]
    public void OrderByDescending_UpdatesQuery()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        var actual = query.OrderByDescending(m => m.Id) as TableQuery<IdEntity>;
        Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
        var expression = actual.Query.Expression as MethodCallExpression;
        Assert.Equal("OrderByDescending", expression.Method.Name);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "OrderByDescending")]
    public void ToODataString_OrderByDescending_IsWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).OrderByDescending(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id desc", odata);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "OrderByDescending")]
    public void ToODataString_OrderByDescending_ThrowsNotSupported()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).OrderByDescending(m => m.Id.ToLower()) as TableQuery<IdEntity>;
        Assert.Throws<NotSupportedException>(() => query.ToODataString());
    }

    [Fact]
    [Trait("Method", "Select")]
    public void Select_Null_Throws()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Expression<Func<IdEntity, IdOnly>> selector = null;
        Assert.Throws<ArgumentNullException>(() => query.Select(selector));
    }

    [Fact]
    [Trait("Method", "Select")]
    public void Select_UpdatesQuery()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        var actual = query.Select(m => new IdOnly { Id = m.Id });
        Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
        var expression = actual.Query.Expression as MethodCallExpression;
        Assert.Equal("Select", expression.Method.Name);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "Select")]
    public void ToODataString_Select_IsWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).Select(m => new IdOnly { Id = m.Id });
        var odata = query.ToODataString();
        Assert.Equal("$select=id", odata);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "Select")]
    public void ToODataString_Select_NoId_IsWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<ClientMovie> table = client.GetRemoteTable<ClientMovie>("movies") as RemoteTable<ClientMovie>;
        var query = table.CreateQuery().Select(m => new { m.Title, m.ReleaseDate });
        var odata = query.ToODataString();
        Assert.Equal("$select=releaseDate,title", odata);
    }

    [Theory, CombinatorialData]
    [Trait("Method", "Skip")]
    public void Skip_Throws_OutOfRange([CombinatorialValues(-10, -1)] int skip)
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Assert.Throws<ArgumentOutOfRangeException>(() => query.Skip(skip));
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "Skip")]
    public void ToODataString_Skip_IsWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).Skip(5) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$skip=5", odata);
    }

    [Theory, CombinatorialData]
    [Trait("Method", "Take")]
    public void Take_ThrowsOutOfRange([CombinatorialValues(-10, -1, 0)] int take)
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Assert.Throws<ArgumentOutOfRangeException>(() => query.Take(take));
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "Take")]
    public void ToODataString_Take_IsWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).Take(5) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$top=5", odata);
    }

    [Fact]
    [Trait("Method", "ThenBy")]
    public void ThenBy_Null_Throws()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Expression<Func<IdEntity, string>> keySelector = null;
        Assert.Throws<ArgumentNullException>(() => query.ThenBy(keySelector));
    }

    [Fact]
    [Trait("Method", "ThenBy")]
    public void ThenBy_UpdatesQuery()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        var actual = query.ThenBy(m => m.Id) as TableQuery<IdEntity>;
        Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
        var expression = actual.Query.Expression as MethodCallExpression;
        Assert.Equal("ThenBy", expression.Method.Name);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "ThenBy")]
    public void ToODataString_ThenBy_IsWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).ThenBy(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id", odata);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "ThenBy")]
    public void ToODataString_ThenBy_ThrowsNotSupported()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).ThenBy(m => m.Id.ToLower()) as TableQuery<IdEntity>;
        Assert.Throws<NotSupportedException>(() => query.ToODataString());
    }

    [Fact]
    [Trait("Method", "ThenByDescending")]
    public void ThenByDescending_Null_Throws()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Expression<Func<IdEntity, string>> keySelector = null;
        Assert.Throws<ArgumentNullException>(() => query.ThenByDescending(keySelector));
    }

    [Fact]
    [Trait("Method", "ThenByDescending")]
    public void ThenByDescending_UpdatesQuery()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        var actual = query.ThenByDescending(m => m.Id) as TableQuery<IdEntity>;
        Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
        var expression = actual.Query.Expression as MethodCallExpression;
        Assert.Equal("ThenByDescending", expression.Method.Name);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "ThenByDescending")]
    public void ToODataString_ThenByDescending_IsWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).ThenByDescending(m => m.Id) as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$orderby=id desc", odata);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "ThenByDescending")]
    public void ToODataString_ThenByDescending_ThrowsNotSupported()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).ThenByDescending(m => m.Id.ToLower()) as TableQuery<IdEntity>;
        Assert.Throws<NotSupportedException>(() => query.ToODataString());
    }

    [Fact]
    [Trait("Method", "Where")]
    public void Where_Null_Throws()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Expression<Func<IdEntity, bool>> predicate = null;
        Assert.Throws<ArgumentNullException>(() => query.Where(predicate));
    }

    [Fact]
    [Trait("Method", "Where")]
    public void Where_UpdatesQuery()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        var actual = query.Where(m => m.Id.Contains("foo")) as TableQuery<IdEntity>;
        Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
        var expression = actual.Query.Expression as MethodCallExpression;
        Assert.Equal("Where", expression.Method.Name);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "Where")]
    public void ToODataString_Where_IsWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).Where(m => m.Id == "foo") as TableQuery<IdEntity>;
        var odata = query.ToODataString();
        Assert.Equal("$filter=%28id%20eq%20%27foo%27%29", odata);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "Where")]
    public void ToODataString_Where_ThrowsNotSupported()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).ThenByDescending(m => m.Id.Normalize() == "foo") as TableQuery<IdEntity>;
        Assert.Throws<NotSupportedException>(() => query.ToODataString());
    }

    [Theory]
    [InlineData(null, "test")]
    [InlineData("test", null)]
    [Trait("Method", "WithParameter")]
    public void WithParameter_Null_Throws(string key, string value)
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Assert.Throws<ArgumentNullException>(() => query.WithParameter(key, value));
    }

    [Theory]
    [InlineData("testkey", "")]
    [InlineData("testkey", " ")]
    [InlineData("testkey", "   ")]
    [InlineData("testkey", "\t")]
    [InlineData("", "testvalue")]
    [InlineData(" ", "testvalue")]
    [InlineData("   ", "testvalue")]
    [InlineData("\t", "testvalue")]
    [InlineData("$count", "true")]
    [InlineData("__includedeleted", "true")]
    [Trait("Method", "WithParameter")]
    public void WithParameter_Illegal_Throws(string key, string value)
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Assert.Throws<ArgumentException>(() => query.WithParameter(key, value));
    }

    [Fact]
    [Trait("Method", "WithParameter")]
    public void WithParameter_SetsParameter()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        var actual = query.WithParameter("testkey", "testvalue") as TableQuery<IdEntity>;
        AssertEx.Contains("testkey", "testvalue", actual.Parameters);
    }

    [Fact]
    [Trait("Method", "WithParameter")]
    public void WithParameter_Overwrites()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).WithParameter("testkey", "testvalue");
        var actual = query.WithParameter("testkey", "replacement") as TableQuery<IdEntity>;
        AssertEx.Contains("testkey", "replacement", actual.Parameters);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "WithParameter")]
    public void ToODataString_WithParameter_isWellFormed()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).WithParameter("testkey", "testvalue") as TableQuery<IdEntity>;

        var odata = query.ToODataString();

        Assert.Equal("testkey=testvalue", odata);
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    [Trait("Method", "WithParameter")]
    public void ToODataString_WithParameter_EncodesValue()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).WithParameter("testkey", "test value") as TableQuery<IdEntity>;

        var odata = query.ToODataString();

        Assert.Equal("testkey=test%20value", odata);
    }

    [Fact]
    [Trait("Method", "WithParameters")]
    public void WithParameters_Null_Throws()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        Assert.Throws<ArgumentNullException>(() => query.WithParameters(null));
    }

    [Fact]
    [Trait("Method", "WithParameters")]
    public void WithParameters_CopiesParams()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        var sut = new Dictionary<string, string>()
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        var actual = query.WithParameters(sut) as TableQuery<IdEntity>;
        AssertEx.Contains("key1", "value1", actual.Parameters);
        AssertEx.Contains("key2", "value2", actual.Parameters);
    }

    [Fact]
    [Trait("Method", "WithParameters")]
    public void WithParameters_MergesParams()
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table).WithParameter("key1", "value1");
        var sut = new Dictionary<string, string>()
        {
            { "key1", "replacement" },
            { "key2", "value2" }
        };

        var actual = query.WithParameters(sut) as TableQuery<IdEntity>;
        AssertEx.Contains("key1", "replacement", actual.Parameters);
        AssertEx.Contains("key2", "value2", actual.Parameters);
    }

    [Theory]
    [InlineData("$count")]
    [InlineData("__includedeleted")]
    [Trait("Method", "WithParameters")]
    public void WithParameters_CannotSetIllegalParams(string key)
    {
        var client = GetMockClient();
        RemoteTable<IdEntity> table = client.GetRemoteTable<IdEntity>("movies") as RemoteTable<IdEntity>;
        var query = new TableQuery<IdEntity>(table);
        var sut = new Dictionary<string, string>()
        {
            { key, "true" },
            { "key2", "value2" }
        };

        Assert.Throws<ArgumentException>(() => query.WithParameters(sut));
    }

    [Fact]
    [Trait("Method", "ToODataString")]
    public void LinqODataWithSelectConversions()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table);
        DateTimeOffset dto1 = new(1994, 10, 14, 0, 0, 0, TimeSpan.Zero);
        const string dts1 = "1994-10-14T00:00:00.000Z";

        // Need to make sure the $select statement is added in the right spot.
        var expected = NormalizeQueryString($"__includedeleted=true&$count=true&$filter=(updatedAt gt cast({dts1},Edm.DateTimeOffset))&$orderby=updatedAt&$skip=25&$select=id,title");

        // Act
        TableQuery<SelectResult> tableQuery = query
            .Where(x => x.UpdatedAt > dto1)
            .IncludeDeletedItems()
            .OrderBy(x => x.UpdatedAt)
            .IncludeTotalCount()
            .Skip(25)
            .Select(m => new SelectResult { Id = m.Id, Title = m.Title }) as TableQuery<SelectResult>;
        var actual = NormalizeQueryString(Uri.UnescapeDataString(tableQuery.ToODataString()));

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Linq_NotSupportedProperties()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table);

        // Act
        var actual = query.Where(m => m.ReleaseDate.UtcDateTime > new DateTime(2001, 12, 31)) as TableQuery<ClientMovie>;
        Assert.Throws<NotSupportedException>(() => actual.ToODataString());
    }

    [Fact]
    public void Linq_NotSupportedMethods()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table);

        // Act
        var actual = query.Where(m => m.Title.LastIndexOf("er") > 0) as TableQuery<ClientMovie>;
        Assert.Throws<NotSupportedException>(() => actual.ToODataString());
    }

    [Fact]
    public void Linq_NotSupportedBinaryOperators()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table);

        // Act
        var actual = query.Where(m => (m.Year ^ 1024) == 0) as TableQuery<ClientMovie>;
        Assert.Throws<NotSupportedException>(() => actual.ToODataString());
    }

    [Fact]
    public void Linq_NotSupportedUnaryOperators()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table);

        // Act
        var actual = query.Where(m => (5 * (-m.Duration)) > -180) as TableQuery<ClientMovie>;
        Assert.Throws<NotSupportedException>(() => actual.ToODataString());
    }

    [Fact]
    public void Linq_NotSupportedDistinctLinqStatement()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table);

        // Act - really - you should NOT be doing this!
        query.Query = query.Query.Distinct();

        // Assert
        Assert.Throws<NotSupportedException>(() => query.ToODataString());
    }

    [Fact]
    public void Linq_NegateNotSupported()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table).Where(m => (-m.Year) <= -2000) as TableQuery<ClientMovie>;

        Assert.Throws<NotSupportedException>(() => query.ToODataString());
    }

    [Fact]
    public void Linq_InvalidOrderBy_Lambda()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table).OrderBy(m => m.Id == "foo" ? "yes" : "no") as TableQuery<ClientMovie>;

        Assert.Throws<NotSupportedException>(() => query.ToODataString());
    }

    [Fact]
    public void Linq_InvalidOrderBy_Method()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table).OrderBy(m => m.GetHashCode()) as TableQuery<ClientMovie>;

        Assert.Throws<NotSupportedException>(() => query.ToODataString());
    }

    [Fact]
    public void Linq_InvalidOrderBy_ToString()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table).OrderBy(m => m.ReleaseDate.ToString("o")) as TableQuery<ClientMovie>;

        Assert.Throws<NotSupportedException>(() => query.ToODataString());
    }

    [Fact]
    public void ToODataString_DontIncludeParameters()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table).OrderBy(m => m.ReleaseDate).WithParameter("foo", "bar") as TableQuery<ClientMovie>;

        var actual = query.ToODataString(false);
        Assert.Equal("$orderby=releaseDate", actual);
    }

    [Fact]
    public void ToODataString_IncludeParameters()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table).OrderBy(m => m.ReleaseDate).WithParameter("foo", "bar") as TableQuery<ClientMovie>;

        var actual = query.ToODataString(true);
        Assert.Equal("$orderby=releaseDate&foo=bar", actual);
    }

    [Fact]
    public void ToODataString_DefaultIncludeParameters()
    {
        // Arrange
        var client = GetMockClient();
        var table = new RemoteTable<ClientMovie>("movies", client);
        var query = new TableQuery<ClientMovie>(table).OrderBy(m => m.ReleaseDate).WithParameter("foo", "bar") as TableQuery<ClientMovie>;

        var actual = query.ToODataString();
        Assert.Equal("$orderby=releaseDate&foo=bar", actual);
    }

    [Fact]
    public void ToODataString_NegativeDouble_Works()
    {
        var client = GetMockClient();
        var table = new RemoteTable<KSV>("ksv", client);
        var query = new TableQuery<KSV>(table).Where(x => x.Value <= -0.5) as TableQuery<KSV>;
        var actual = query.ToODataString();
        Assert.Equal("$filter=%28value%20le%20-0.5%29", actual);
    }

    [Fact]
    public void ToODataString_NegativeNullableDouble_Works()
    {
        var client = GetMockClient();
        var table = new RemoteTable<KSV>("ksv", client);
        var query = new TableQuery<KSV>(table).Where(x => x.NullableValue <= -0.5) as TableQuery<KSV>;
        var actual = query.ToODataString();
        Assert.Equal("$filter=%28nullableValue%20le%20-0.5%29", actual);
    }

    #region Models
    public class SelectResult
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }

    public class KSV : DatasyncClientData
    {
        public double Value { get; set; }

        public double? NullableValue { get; set; }
    }
    #endregion
}

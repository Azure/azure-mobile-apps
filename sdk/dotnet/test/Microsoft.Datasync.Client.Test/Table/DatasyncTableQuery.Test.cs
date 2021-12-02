// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using FluentAssertions;
using Microsoft.Datasync.Client.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table
{
    [ExcludeFromCodeCoverage]
    public class DatasyncTableQuery_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullTable_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DatasyncTableQuery<IdEntity>(null));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_BlankSetup()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            Assert.Same(table, query.Table);
            Assert.IsAssignableFrom<IQueryable<IdEntity>>(query.Query);
            Assert.Empty(query.QueryParameters);
            Assert.Equal(0, query.SkipCount);
            Assert.Equal(0, query.TakeCount);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        public void ToODataString_BlankQuery_ReturnsEmptyString()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

            Assert.Empty(query.ToODataQueryString());
        }

        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Enabled_ChangesKey()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            query.QueryParameters.Add("__includedeleted", "test");

            var actual = query.IncludeDeletedItems() as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("__includedeleted", "true", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Disabled_RemovesKey()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            query.QueryParameters.Add("__includedeleted", "true");

            var actual = query.IncludeDeletedItems(false) as DatasyncTableQuery<IdEntity>;

            Assert.False(actual.QueryParameters.ContainsKey("__includedeleted"));
        }

        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Disabled_WorksWithEmptyParameters()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

            var actual = query.IncludeDeletedItems(false) as DatasyncTableQuery<IdEntity>;

            Assert.False(actual.QueryParameters.ContainsKey("__includedeleted"));
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "IncludeDeletedItems")]
        public void ToODataQueryString_IncludeDeletedItems_IsWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).IncludeDeletedItems() as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();
            Assert.Equal("__includedeleted=true", odata);
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Enabled_AddsKey()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).IncludeTotalCount(true) as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("$count", "true", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Enabled_ChangesKey()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            query.QueryParameters.Add("$count", "test");

            var actual = query.IncludeTotalCount() as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("$count", "true", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Disabled_RemovesKey()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            query.QueryParameters.Add("$count", "true");

            var actual = query.IncludeTotalCount(false) as DatasyncTableQuery<IdEntity>;

            Assert.False(actual.QueryParameters.ContainsKey("$count"));
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Disabled_WorksWithEmptyParameters()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

            var actual = query.IncludeTotalCount(false) as DatasyncTableQuery<IdEntity>;

            Assert.False(actual.QueryParameters.ContainsKey("$count"));
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "IncludeTotalCount")]
        public void ToOdataQueryString_IncludeTotalCount_IsWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).IncludeTotalCount() as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();
            Assert.Equal("$count=true", odata);
        }

        [Fact]
        [Trait("Method", "OrderBy")]
        public void OrderBy_Null_Throws()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            Expression<Func<IdEntity, string>> keySelector = null;

            Assert.Throws<ArgumentNullException>(() => query.OrderBy(keySelector));
        }

        [Fact]
        [Trait("Method", "OrderBy")]
        public void OrderBy_UpdatesQuery()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            var actual = query.OrderBy(m => m.Id) as DatasyncTableQuery<IdEntity>;

            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("OrderBy", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderBy")]
        public void ToODataQueryString_OrderBy_IsWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).OrderBy(m => m.Id) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$orderby=id", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderBy")]
        public void ToODataQueryString_OrderBy_ThrowsNotSupported()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).OrderBy(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        [Trait("Method", "OrderByDescending")]
        public void OrderByDescending_Null_Throws()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            Expression<Func<IdEntity, string>> keySelector = null;

            Assert.Throws<ArgumentNullException>(() => query.OrderByDescending(keySelector));
        }

        [Fact]
        [Trait("Method", "OrderByDescending")]
        public void OrderByDescending_UpdatesQuery()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            var actual = query.OrderByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;

            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("OrderByDescending", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderByDescending")]
        public void ToODataQueryString_OrderByDescending_IsWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).OrderByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$orderby=id desc", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderByDescending")]
        public void ToODataQueryString_OrderByDescending_ThrowsNotSupported()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).OrderByDescending(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        [Trait("Method", "Select")]
        public void Select_Null_Throws()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            Expression<Func<IdEntity, IdOnly>> selector = null;

            Assert.Throws<ArgumentNullException>(() => query.Select(selector));
        }

        [Fact]
        [Trait("Method", "Select")]
        public void Select_UpdatesQuery()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            var actual = query.Select(m => new IdOnly { Id = m.Id }) as DatasyncTableQuery<IdOnly>;

            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("Select", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Select")]
        public void ToODataQueryString_Select_IsWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).Select(m => new IdOnly { Id = m.Id }) as DatasyncTableQuery<IdOnly>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$select=id", odata);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "Skip")]
        public void Skip_Throws_OutOfRange([CombinatorialValues(-10, -1)] int skip)
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

            Assert.Throws<ArgumentOutOfRangeException>(() => query.Skip(skip));
        }

        [Fact]
        [Trait("Method", "Skip")]
        public void Skip_Sets_SkipCount()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

            var actual = query.Skip(5) as DatasyncTableQuery<IdEntity>;

            Assert.Equal(5, actual.SkipCount);
        }

        [Fact]
        [Trait("Method", "Skip")]
        public void Skip_IsCumulative()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

            var actual = query.Skip(5).Skip(20) as DatasyncTableQuery<IdEntity>;

            Assert.Equal(25, actual.SkipCount);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Skip")]
        public void ToODataQueryString_Skip_IsWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).Skip(5) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();
            Assert.Equal("$skip=5", odata);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "Take")]
        public void Take_ThrowsOutOfRange([CombinatorialValues(-10, -1, 0)] int take)
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

            Assert.Throws<ArgumentOutOfRangeException>(() => query.Take(take));
        }

        [Theory]
        [InlineData(5, 2, 2)]
        [InlineData(2, 5, 2)]
        [InlineData(5, 20, 5)]
        [InlineData(20, 5, 5)]
        [Trait("Method", "Take")]
        public void Take_MinimumWins(int first, int second, int expected)
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

            var actual = query.Take(first).Take(second) as DatasyncTableQuery<IdEntity>;

            Assert.Equal(expected, actual.TakeCount);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Take")]
        public void ToODataQueryString_Take_IsWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).Take(5) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();
            Assert.Equal("$top=5", odata);
        }

        [Fact]
        [Trait("Method", "ThenBy")]
        public void ThenBy_Null_Throws()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            Expression<Func<IdEntity, string>> keySelector = null;

            Assert.Throws<ArgumentNullException>(() => query.ThenBy(keySelector));
        }

        [Fact]
        [Trait("Method", "ThenBy")]
        public void ThenBy_UpdatesQuery()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            var actual = query.ThenBy(m => m.Id) as DatasyncTableQuery<IdEntity>;

            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("ThenBy", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenBy")]
        public void ToODataQueryString_ThenBy_IsWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).ThenBy(m => m.Id) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$orderby=id", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenBy")]
        public void ToODataQueryString_ThenBy_ThrowsNotSupported()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).ThenBy(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        [Trait("Method", "ThenByDescending")]
        public void ThenByDescending_Null_Throws()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            Expression<Func<IdEntity, string>> keySelector = null;

            Assert.Throws<ArgumentNullException>(() => query.ThenByDescending(keySelector));
        }

        [Fact]
        [Trait("Method", "ThenByDescending")]
        public void ThenByDescending_UpdatesQuery()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            var actual = query.ThenByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;

            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("ThenByDescending", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenByDescending")]
        public void ToODataQueryString_ThenByDescending_IsWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).ThenByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$orderby=id desc", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenByDescending")]
        public void ToODataQueryString_ThenByDescending_ThrowsNotSupported()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).ThenByDescending(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        [Trait("Method", "ToAsyncPageable")]
        public async Task ToAsyncPageable_WithCount_Executes()
        {
            // Arrange
            var client = GetMockClient();
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var page1 = CreatePageOfItems(5, 10, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, 10, new Uri($"{sEndpoint}?page=3"));
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            List<IdEntity> items = new();
            var query = new DatasyncTableQuery<IdEntity>(table).IncludeTotalCount();
            long? count = null;

            // Act
            var pageable = query.ToAsyncPageable();
            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                if (!count.HasValue) count = pageable.Count;
                items.Add(enumerator.Current);
            }

            // Assert - request
            Assert.Equal(3, MockHandler.Requests.Count);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?$count=true", request.RequestUri.ToString());

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());

            request = MockHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=3", request.RequestUri.ToString());

            // Assert - response
            Assert.Equal(10, count);
            Assert.NotNull(pageable.CurrentResponse);
            Assert.Equal(200, pageable.CurrentResponse.StatusCode);
            Assert.True(pageable.CurrentResponse.HasContent);
            Assert.NotEmpty(pageable.CurrentResponse.Content);

            Assert.Equal(10, items.Count);
            Assert.Equal(page1.Items, items.Take(5));
            Assert.Equal(page2.Items, items.Skip(5).Take(5));
        }

        [Fact]
        [Trait("Method", "ToAsyncPageable")]
        public async Task ToAsyncPageable_WithQuery_Executes()
        {
            // Arrange
            var client = GetMockClient();
            var sEndpoint = new Uri(Endpoint, "/tables/movies/").ToString();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var page1 = CreatePageOfItems(5, 10, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, 10, new Uri($"{sEndpoint}?page=3"));
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            List<IdEntity> items = new();
            var query = new DatasyncTableQuery<IdEntity>(table).Where(m => m.StringValue == "foo").IncludeTotalCount();
            long? count = null;

            // Act
            var pageable = query.ToAsyncPageable();
            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                if (!count.HasValue) count = pageable.Count;
                items.Add(enumerator.Current);
            }

            // Assert - request
            Assert.Equal(3, MockHandler.Requests.Count);
            var request = MockHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?$count=true&$filter=(stringValue eq 'foo')", request.RequestUri.ToString());

            request = MockHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());

            request = MockHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=3", request.RequestUri.ToString());

            // Assert - response
            Assert.Equal(10, count);
            Assert.NotNull(pageable.CurrentResponse);
            Assert.Equal(200, pageable.CurrentResponse.StatusCode);
            Assert.True(pageable.CurrentResponse.HasContent);
            Assert.NotEmpty(pageable.CurrentResponse.Content);
        }

        [Fact]
        [Trait("Method", "Where")]
        public void Where_Null_Throws()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            Expression<Func<IdEntity, bool>> predicate = null;

            Assert.Throws<ArgumentNullException>(() => query.Where(predicate));
        }

        [Fact]
        [Trait("Method", "Where")]
        public void Where_UpdatesQuery()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            var actual = query.Where(m => m.Id.Contains("foo")) as DatasyncTableQuery<IdEntity>;

            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("Where", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Where")]
        public void ToODataQueryString_Where_IsWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).Where(m => m.Id == "foo") as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$filter=(id%20eq%20'foo')", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Where")]
        public void ToODataQueryString_Where_ThrowsNotSupported()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).ThenByDescending(m => m.Id.Normalize() == "foo") as DatasyncTableQuery<IdEntity>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Theory]
        [InlineData(null, "test")]
        [InlineData("test", null)]
        [Trait("Method", "WithParameter")]
        public void WithParameter_Null_Throws(string key, string value)
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

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
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

            Assert.Throws<ArgumentException>(() => query.WithParameter(key, value));
        }

        [Fact]
        [Trait("Method", "WithParameter")]
        public void WithParameter_SetsParameter()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

            var actual = query.WithParameter("testkey", "testvalue") as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("testkey", "testvalue", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "WithParameter")]
        public void WithParameter_Overwrites()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).WithParameter("testkey", "testvalue");

            var actual = query.WithParameter("testkey", "replacement") as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("testkey", "replacement", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "WithParameter")]
        public void ToODataQueryString_WithParameter_isWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).WithParameter("testkey", "testvalue") as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("testkey=testvalue", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "WithParameter")]
        public void ToODataQueryString_WithParameter_EncodesValue()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).WithParameter("testkey", "test value") as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("testkey=test%20value", odata);
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_Null_Throws()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);

            Assert.Throws<ArgumentNullException>(() => query.WithParameters(null));
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_Empty_Throws()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            var sut = new Dictionary<string, string>();

            Assert.Throws<ArgumentException>(() => query.WithParameters(sut));
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_CopiesParams()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            var sut = new Dictionary<string, string>()
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            var actual = query.WithParameters(sut) as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("key1", "value1", actual.QueryParameters);
            AssertEx.Contains("key2", "value2", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_MergesParams()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table).WithParameter("key1", "value1");
            var sut = new Dictionary<string, string>()
            {
                { "key1", "replacement" },
                { "key2", "value2" }
            };

            var actual = query.WithParameters(sut) as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("key1", "replacement", actual.QueryParameters);
            AssertEx.Contains("key2", "value2", actual.QueryParameters);
        }

        [Theory]
        [InlineData("$count")]
        [InlineData("__includedeleted")]
        [Trait("Method", "WithParameters")]
        public void WithParameters_CannotSetIllegalParams(string key)
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var query = new DatasyncTableQuery<IdEntity>(table);
            var sut = new Dictionary<string, string>()
            {
                { key, "true" },
                { "key2", "value2" }
            };

            Assert.Throws<ArgumentException>(() => query.WithParameters(sut));
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "WithParameters")]
        public void ToODataQueryString_WithParameters_isWellFormed()
        {
            var client = GetMockClient();
            DatasyncTable<IdEntity> table = client.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
            var pairs = new Dictionary<string, string>()
            {
                {  "key1", "value1" },
                {  "key2", "value 2" }
            };
            var query = new DatasyncTableQuery<IdEntity>(table).WithParameters(pairs) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("key1=value1&key2=value%202", odata);
        }

        [Theory]
        [ClassData(typeof(LinqTestCases))]
        [Trait("Method", "ToODataQueryString")]
        internal void LinqODataConversions(LinqTestCase testcase)
        {
            // Arrange
            var client = GetMockClient();
            var table = new DatasyncTable<ClientMovie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<ClientMovie>(table);

            // Act
            var actual = (testcase.LinqExpression.Invoke(query) as DatasyncTableQuery<ClientMovie>)?.ToODataQueryString();
            var tester = Uri.UnescapeDataString(actual);

            // Assert
            Assert.NotNull(actual);

            var expectedParams = testcase.ODataString.Split('&').ToList();
            var actualParams = tester.Split('&').ToList();
            // actualParams and expectedParams need to be the same, but can be in different order
            actualParams.Should().BeEquivalentTo(expectedParams, $"Test Case {testcase.Name} OData String");
        }

        [Theory]
        [ClassData(typeof(LinqTestCases))]
        [Trait("Method", "ToODataQueryString")]
        internal void LinqODataWithSelectConversions(LinqTestCase testcase)
        {
            // Arrange
            var client = GetMockClient();
            var table = new DatasyncTable<ClientMovie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<ClientMovie>(table);

            // Need to make sure the $select statement is added in the right spot.
            var splitArgs = testcase.ODataString.Split('&').ToList();
            splitArgs.Add("$select=id,title");
            splitArgs.Sort();
            var expectedWithSelect = string.Join('&', splitArgs).TrimStart('&');

            // Act
            var actual = (testcase.LinqExpression.Invoke(query).Select(m => new SelectResult { Id = m.Id, Title = m.Title }) as DatasyncTableQuery<SelectResult>)?.ToODataQueryString();
            var tester = Uri.UnescapeDataString(actual);

            // Assert
            Assert.NotNull(actual);
            Assert.True(tester.Equals(expectedWithSelect), $"Test '{testcase.Name}' did not match (with select)\nExpected: {expectedWithSelect}\nActual  : {tester}");
        }

        [Fact]
        public void Linq_NotSupportedProperties()
        {
            // Arrange
            var client = GetMockClient();
            var table = new DatasyncTable<ClientMovie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<ClientMovie>(table);

            // Act
            var actual = query.Where(m => m.ReleaseDate.UtcDateTime > new DateTime(2001, 12, 31)) as DatasyncTableQuery<ClientMovie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedMethods()
        {
            // Arrange
            var client = GetMockClient();
            var table = new DatasyncTable<ClientMovie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<ClientMovie>(table);

            // Act
            var actual = query.Where(m => m.Title.LastIndexOf("er") > 0) as DatasyncTableQuery<ClientMovie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedBinaryOperators()
        {
            // Arrange
            var client = GetMockClient();
            var table = new DatasyncTable<ClientMovie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<ClientMovie>(table);

            // Act
            var actual = query.Where(m => (m.Year ^ 1024) == 0) as DatasyncTableQuery<ClientMovie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedUnaryOperators()
        {
            // Arrange
            var client = GetMockClient();
            var table = new DatasyncTable<ClientMovie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<ClientMovie>(table);

            // Act
            var actual = query.Where(m => (5 * (-m.Duration)) > -180) as DatasyncTableQuery<ClientMovie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedDistinctLinqStatement()
        {
            // Arrange
            var client = GetMockClient();
            var table = new DatasyncTable<ClientMovie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<ClientMovie>(table);

            // Act - really - you should NOT be doing this!
            query.Query = query.Query.Distinct();

            // Assert
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_NegateNotSupported()
        {
            // Arrange
            var client = GetMockClient();
            var table = new DatasyncTable<ClientMovie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<ClientMovie>(table).Where(m => (-m.Year) <= -2000) as DatasyncTableQuery<ClientMovie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_InvalidOrderBy_Lambda()
        {
            // Arrange
            var client = GetMockClient();
            var table = new DatasyncTable<ClientMovie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<ClientMovie>(table).OrderBy(m => m.Id == "foo" ? "yes" : "no") as DatasyncTableQuery<ClientMovie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_InvalidOrderBy_Method()
        {
            // Arrange
            var client = GetMockClient();
            var table = new DatasyncTable<ClientMovie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<ClientMovie>(table).OrderBy(m => m.GetHashCode()) as DatasyncTableQuery<ClientMovie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_InvalidOrderBy_ToString()
        {
            // Arrange
            var client = GetMockClient();
            var table = new DatasyncTable<ClientMovie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<ClientMovie>(table).OrderBy(m => m.ReleaseDate.ToString("o")) as DatasyncTableQuery<ClientMovie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
    }
}

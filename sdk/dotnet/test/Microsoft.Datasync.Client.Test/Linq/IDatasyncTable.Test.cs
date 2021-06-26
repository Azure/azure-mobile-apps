// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Linq;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Linq
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class IDatasyncTable_Linq_Tests : BaseTest
    {
        private const string sEndpoint = "https://foo.azurewebsites.net/tables/movies/";

        /// <summary>
        /// Testing for Select operations
        /// </summary>
        private class IdOnly
        {
            public string Id { get; set; }
        }

        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Enabled_AddsKey()
        {
            var query = Table.IncludeDeletedItems(true) as DatasyncTableQuery<IdEntity>;
            AssertEx.HasValue("__includedeleted", "true", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Disabled_Empty()
        {
            var query = Table.IncludeDeletedItems(false) as DatasyncTableQuery<IdEntity>;
            Assert.Empty(query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "IncludeDeletedItems")]
        public void ToODataQueryString_IncludeDeletedItems_IsWellFormed()
        {
            var query = Table.IncludeDeletedItems() as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("__includedeleted=true", odata);
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Enabled_AddsKey()
        {
            var query = Table.IncludeTotalCount(true) as DatasyncTableQuery<IdEntity>;
            AssertEx.HasValue("$count", "true", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Disabled_WorksWithEmptyParameters()
        {
            var actual = Table.IncludeTotalCount(false) as DatasyncTableQuery<IdEntity>;
            Assert.False(actual.QueryParameters.ContainsKey("$count"));
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "IncludeTotalCount")]
        public void ToOdataQueryString_IncludeTotalCount_IsWellFormed()
        {
            var query = Table.IncludeTotalCount() as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$count=true", odata);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "Skip")]
        public void Skip_Throws_OutOfRange([CombinatorialValues(-10, -1)] int skip)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Table.Skip(skip));
        }

        [Fact]
        [Trait("Method", "Skip")]
        public void Skip_Sets_SkipCount()
        {
            var actual = Table.Skip(5) as DatasyncTableQuery<IdEntity>;
            Assert.Equal(5, actual.SkipCount);
        }

        [Fact]
        [Trait("Method", "Skip")]
        public void Skip_IsCumulative()
        {
            var actual = Table.Skip(5).Skip(20) as DatasyncTableQuery<IdEntity>;
            Assert.Equal(25, actual.SkipCount);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Skip")]
        public void ToODataQueryString_Skip_IsWellFormed()
        {
            var query = Table.Skip(5) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$skip=5", odata);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "Take")]
        public void Take_ThrowsOutOfRange([CombinatorialValues(-10, -1, 0)] int take)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Table.Take(take));
        }

        [Theory]
        [InlineData(5, 2, 2)]
        [InlineData(2, 5, 2)]
        [InlineData(5, 20, 5)]
        [InlineData(20, 5, 5)]
        [Trait("Method", "Take")]
        public void Take_MinimumWins(int first, int second, int expected)
        {
            var actual = Table.Take(first).Take(second) as DatasyncTableQuery<IdEntity>;
            Assert.Equal(expected, actual.TakeCount);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Take")]
        public void ToODataQueryString_Take_IsWellFormed()
        {
            var query = Table.Take(5) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$top=5", odata);
        }

        [Theory]
        [InlineData(null, "test")]
        [InlineData("test", null)]
        [Trait("Method", "WithParameter")]
        public void WithParameter_Null_Throws(string key, string value)
        {
            Assert.Throws<ArgumentNullException>(() => Table.WithParameter(key, value));
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
            Assert.Throws<ArgumentException>(() => Table.WithParameter(key, value));
        }

        [Fact]
        [Trait("Method", "WithParameter")]
        public void WithParameter_SetsParameter()
        {
            var actual = Table.WithParameter("testkey", "testvalue") as DatasyncTableQuery<IdEntity>;
            AssertEx.HasValue("testkey", "testvalue", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "WithParameter")]
        public void WithParameter_Overwrites()
        {
            var query = Table.WithParameter("testkey", "testvalue");
            var actual = query.WithParameter("testkey", "replacement") as DatasyncTableQuery<IdEntity>;
            AssertEx.HasValue("testkey", "replacement", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "WithParameter")]
        public void ToODataQueryString_WithParameter_isWellFormed()
        {
            var query = Table.WithParameter("testkey", "testvalue") as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("testkey=testvalue", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "WithParameter")]
        public void ToODataQueryString_WithParameter_EncodesValue()
        {
            var query = Table.WithParameter("testkey", "test value") as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("testkey=test%20value", odata);
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Table.WithParameters(null));
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_Empty_Throws()
        {
            var sut = new Dictionary<string, string>();
            Assert.Throws<ArgumentException>(() => Table.WithParameters(sut));
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_CopiesParams()
        {
            var sut = new Dictionary<string, string>()
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            var actual = Table.WithParameters(sut) as DatasyncTableQuery<IdEntity>;
            AssertEx.HasValue("key1", "value1", actual.QueryParameters);
            AssertEx.HasValue("key2", "value2", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_MergesParams()
        {
            var query = Table.WithParameter("key1", "value1");
            var sut = new Dictionary<string, string>()
            {
                { "key1", "replacement" },
                { "key2", "value2" }
            };

            var actual = query.WithParameters(sut) as DatasyncTableQuery<IdEntity>;
            AssertEx.HasValue("key1", "replacement", actual.QueryParameters);
            AssertEx.HasValue("key2", "value2", actual.QueryParameters);
        }

        [Theory]
        [InlineData("$count")]
        [InlineData("__includedeleted")]
        [Trait("Method", "WithParameters")]
        public void WithParameters_CannotSetIllegalParams(string key)
        {
            var sut = new Dictionary<string, string>()
            {
                { key, "true" },
                { "key2", "value2" }
            };

            Assert.Throws<ArgumentException>(() => Table.WithParameters(sut));
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "WithParameters")]
        public void ToODataQueryString_WithParameters_isWellFormed()
        {
            var pairs = new Dictionary<string, string>()
            {
                {  "key1", "value1" },
                {  "key2", "value 2" }
            };
            var query = Table.WithParameters(pairs) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("key1=value1&key2=value%202", odata);
        }

        [Fact]
        [Trait("Method", "OrderBy")]
        public void OrderBy_Null_Throws()
        {
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => Table.OrderBy(keySelector));
        }

        [Fact]
        [Trait("Method", "OrderBy")]
        public void OrderBy_UpdatesQuery()
        {
            var actual = Table.OrderBy(m => m.Id) as DatasyncTableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("OrderBy", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderBy")]
        public void ToODataQueryString_OrderBy_IsWellFormed()
        {
            var query = Table.OrderBy(m => m.Id) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$orderby=id", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderBy")]
        public void ToODataQueryString_OrderBy_ThrowsNotSupported()
        {
            var query = Table.OrderBy(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        [Trait("Method", "OrderByDescending")]
        public void OrderByDescending_Null_Throws()
        {
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => Table.OrderByDescending(keySelector));
        }

        [Fact]
        [Trait("Method", "OrderByDescending")]
        public void OrderByDescending_UpdatesQuery()
        {
            var actual = Table.OrderByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("OrderByDescending", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderByDescending")]
        public void ToODataQueryString_OrderByDescending_IsWellFormed()
        {
            var query = Table.OrderByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$orderby=id desc", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderByDescending")]
        public void ToODataQueryString_OrderByDescending_ThrowsNotSupported()
        {
            var query = Table.OrderByDescending(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        [Trait("Method", "ThenBy")]
        public void ThenBy_Null_Throws()
        {
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => Table.ThenBy(keySelector));
        }

        [Fact]
        [Trait("Method", "ThenBy")]
        public void ThenBy_UpdatesQuery()
        {
            var actual = Table.ThenBy(m => m.Id) as DatasyncTableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("ThenBy", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenBy")]
        public void ToODataQueryString_ThenBy_IsWellFormed()
        {
            var query = Table.ThenBy(m => m.Id) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$orderby=id", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenBy")]
        public void ToODataQueryString_ThenBy_ThrowsNotSupported()
        {
            var query = Table.ThenBy(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        [Trait("Method", "ThenByDescending")]
        public void ThenByDescending_Null_Throws()
        {
            Expression<Func<IdEntity, string>> keySelector = null;
            Assert.Throws<ArgumentNullException>(() => Table.ThenByDescending(keySelector));
        }

        [Fact]
        [Trait("Method", "ThenByDescending")]
        public void ThenByDescending_UpdatesQuery()
        {
            var actual = Table.ThenByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("ThenByDescending", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenByDescending")]
        public void ToODataQueryString_ThenByDescending_IsWellFormed()
        {
            var query = Table.ThenByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$orderby=id desc", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenByDescending")]
        public void ToODataQueryString_ThenByDescending_ThrowsNotSupported()
        {
            var query = Table.ThenByDescending(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        [Trait("Method", "Where")]
        public void Where_Null_Throws()
        {
            Expression<Func<IdEntity, bool>> predicate = null;
            Assert.Throws<ArgumentNullException>(() => Table.Where(predicate));
        }

        [Fact]
        [Trait("Method", "Where")]
        public void Where_UpdatesQuery()
        {
            var actual = Table.Where(m => m.Id.Contains("foo")) as DatasyncTableQuery<IdEntity>;
            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("Where", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Where")]
        public void ToODataQueryString_Where_IsWellFormed()
        {
            var query = Table.Where(m => m.Id == "foo") as DatasyncTableQuery<IdEntity>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$filter=(id%20eq%20'foo')", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Where")]
        public void ToODataQueryString_Where_ThrowsNotSupported()
        {
            var query = Table.ThenByDescending(m => m.Id.Normalize() == "foo") as DatasyncTableQuery<IdEntity>;
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        [Trait("Method", "Select")]
        public void Select_Null_Throws()
        {
            Expression<Func<IdEntity, IdOnly>> selector = null;
            Assert.Throws<ArgumentNullException>(() => Table.Select(selector));
        }

        [Fact]
        [Trait("Method", "Select")]
        public void Select_UpdatesQuery()
        {
            var actual = Table.Select(m => new IdOnly { Id = m.Id }) as DatasyncTableQuery<IdOnly>;
            Assert.IsAssignableFrom<MethodCallExpression>(actual.Query.Expression);
            var expression = actual.Query.Expression as MethodCallExpression;
            Assert.Equal("Select", expression.Method.Name);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Select")]
        public void ToODataQueryString_Select_IsWellFormed()
        {
            var query = Table.Select(m => new IdOnly { Id = m.Id }) as DatasyncTableQuery<IdOnly>;
            var odata = query.ToODataQueryString();
            Assert.Equal("$select=id", odata);
        }

        [Fact]
        [Trait("Method", "ToAsyncEnumerable")]
        public async Task ToAsyncEnumerable_WithoutQuery_Executes()
        {
            // Arrange
            var page1 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=3"));
            ClientHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            List<IdEntity> items = new();

            // Act
            await foreach (var item in Table.ToAsyncEnumerable())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(3, ClientHandler.Requests.Count);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());

            request = ClientHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());

            request = ClientHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=3", request.RequestUri.ToString());

            // Assert - response
            Assert.Equal(10, items.Count);
            Assert.Equal(page1.Items, items.Take(5));
            Assert.Equal(page2.Items, items.Skip(5).Take(5));
        }

        [Fact]
        [Trait("Method", "ToAsyncPageable")]
        public async Task ToAsyncPageable_WithCount_Executes()
        {
            // Arrange
            var page1 = CreatePageOfItems(5, 10, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, 10, new Uri($"{sEndpoint}?page=3"));
            ClientHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            List<IdEntity> items = new();

            // Act
            var pageable = Table.ToAsyncPageable();
            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                items.Add(enumerator.Current);
            }

            // Assert - request
            Assert.Equal(3, ClientHandler.Requests.Count);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());

            request = ClientHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());

            request = ClientHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=3", request.RequestUri.ToString());

            // Assert - response
            Assert.NotNull(pageable.CurrentResponse);
            Assert.Equal(200, pageable.CurrentResponse.StatusCode);
            Assert.True(pageable.CurrentResponse.HasContent);
            Assert.NotEmpty(pageable.CurrentResponse.Content);

            Assert.Equal(10, items.Count);
            Assert.Equal(page1.Items, items.Take(5));
            Assert.Equal(page2.Items, items.Skip(5).Take(5));
        }

        [Fact]
        [Trait("Method", "ToEnumerable")]
        public void ToEnumerable_WithoutQuery_Executes()
        {
            // Arrange
            var page1 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=3"));
            ClientHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            List<IdEntity> items = new();

            // Act
            foreach (var item in Table.ToEnumerable())
            {
                items.Add(item);
            }

            // Assert - request
            Assert.Equal(3, ClientHandler.Requests.Count);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());

            request = ClientHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());

            request = ClientHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=3", request.RequestUri.ToString());

            // Assert - response
            Assert.Equal(10, items.Count);
            Assert.Equal(page1.Items, items.Take(5));
            Assert.Equal(page2.Items, items.Skip(5).Take(5));
        }

        [Fact]
        [Trait("Method", "ToListAsync")]
        public async Task ToListAsync_WithoutQuery_Executes()
        {
            // Arrange
            var page1 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, null, new Uri($"{sEndpoint}?page=3"));
            ClientHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());

            // Act
            var items = await Table.ToListAsync().ConfigureAwait(false);

            // Assert - request
            Assert.Equal(3, ClientHandler.Requests.Count);
            var request = ClientHandler.Requests[0];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(sEndpoint, request.RequestUri.ToString());

            request = ClientHandler.Requests[1];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=2", request.RequestUri.ToString());

            request = ClientHandler.Requests[2];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{sEndpoint}?page=3", request.RequestUri.ToString());

            // Assert - response
            Assert.Equal(10, items.Count);
            Assert.Equal(page1.Items, items.Take(5));
            Assert.Equal(page2.Items, items.Skip(5).Take(5));
        }
    }
}

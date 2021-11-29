// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.TestData;
using FluentAssertions;
using Microsoft.Datasync.Client.Commands;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using Microsoft.Datasync.Client.Utils;
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
        #region Test Setup
        private DatasyncClient MockClient { get; }
        private DatasyncTable<IdEntity> Table { get; }

        public DatasyncTableQuery_Tests()
        {
            MockClient = CreateClientForMocking();
            Table = MockClient.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
        }
        #endregion

        #region Test Artifacts
        /// <summary>
        /// Testing for Select operations
        /// </summary>
        private class IdOnly
        {
            public string Id { get; set; }
        }

        private class SelectResult
        {
            public string Id { get; set; }
            public string Title { get; set; }
        }
        #endregion

        #region Ctor
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
            var query = new DatasyncTableQuery<IdEntity>(Table);
            Assert.Same(Table, query.Table);
            Assert.IsAssignableFrom<IQueryable<IdEntity>>(query.Query);
            Assert.Empty(query.QueryParameters);
            Assert.Equal(0, query.SkipCount);
            Assert.Equal(0, query.TakeCount);
        }
        #endregion

        #region ToODataQueryString
        [Fact]
        [Trait("Method", "ToODataQueryString")]
        public void ToODataString_BlankQuery_ReturnsEmptyString()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);

            Assert.Empty(query.ToODataQueryString());
        }
        #endregion

        #region IncludeDeletedItems
        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Enabled_ChangesKey()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            query.QueryParameters.Add("__includedeleted", "test");

            var actual = query.IncludeDeletedItems() as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("__includedeleted", "true", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Disabled_RemovesKey()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            query.QueryParameters.Add("__includedeleted", "true");

            var actual = query.IncludeDeletedItems(false) as DatasyncTableQuery<IdEntity>;

            Assert.False(actual.QueryParameters.ContainsKey("__includedeleted"));
        }

        [Fact]
        [Trait("Method", "IncludeDeletedItems")]
        public void IncludeDeletedItems_Disabled_WorksWithEmptyParameters()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);

            var actual = query.IncludeDeletedItems(false) as DatasyncTableQuery<IdEntity>;

            Assert.False(actual.QueryParameters.ContainsKey("__includedeleted"));
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "IncludeDeletedItems")]
        public void ToODataQueryString_IncludeDeletedItems_IsWellFormed()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).IncludeDeletedItems() as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();
            Assert.Equal("__includedeleted=true", odata);
        }
        #endregion

        #region IncludeTotalCount
        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Enabled_AddsKey()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).IncludeTotalCount(true) as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("$count", "true", query.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Enabled_ChangesKey()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            query.QueryParameters.Add("$count", "test");

            var actual = query.IncludeTotalCount() as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("$count", "true", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Disabled_RemovesKey()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            query.QueryParameters.Add("$count", "true");

            var actual = query.IncludeTotalCount(false) as DatasyncTableQuery<IdEntity>;

            Assert.False(actual.QueryParameters.ContainsKey("$count"));
        }

        [Fact]
        [Trait("Method", "IncludeTotalCount")]
        public void IncludeTotalCount_Disabled_WorksWithEmptyParameters()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);

            var actual = query.IncludeTotalCount(false) as DatasyncTableQuery<IdEntity>;

            Assert.False(actual.QueryParameters.ContainsKey("$count"));
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "IncludeTotalCount")]
        public void ToOdataQueryString_IncludeTotalCount_IsWellFormed()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).IncludeTotalCount() as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();
            Assert.Equal("$count=true", odata);
        }
        #endregion

        #region OrderBy
        [Fact]
        [Trait("Method", "OrderBy")]
        public void OrderBy_Null_Throws()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            Expression<Func<IdEntity, string>> keySelector = null;

            Assert.Throws<ArgumentNullException>(() => query.OrderBy(keySelector));
        }

        [Fact]
        [Trait("Method", "OrderBy")]
        public void OrderBy_UpdatesQuery()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
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
            var query = new DatasyncTableQuery<IdEntity>(Table).OrderBy(m => m.Id) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$orderby=id", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderBy")]
        public void ToODataQueryString_OrderBy_ThrowsNotSupported()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).OrderBy(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
        #endregion

        #region OrderByDescending
        [Fact]
        [Trait("Method", "OrderByDescending")]
        public void OrderByDescending_Null_Throws()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            Expression<Func<IdEntity, string>> keySelector = null;

            Assert.Throws<ArgumentNullException>(() => query.OrderByDescending(keySelector));
        }

        [Fact]
        [Trait("Method", "OrderByDescending")]
        public void OrderByDescending_UpdatesQuery()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
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
            var query = new DatasyncTableQuery<IdEntity>(Table).OrderByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$orderby=id desc", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "OrderByDescending")]
        public void ToODataQueryString_OrderByDescending_ThrowsNotSupported()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).OrderByDescending(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
        #endregion

        #region Select
        [Fact]
        [Trait("Method", "Select")]
        public void Select_Null_Throws()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            Expression<Func<IdEntity, IdOnly>> selector = null;

            Assert.Throws<ArgumentNullException>(() => query.Select(selector));
        }

        [Fact]
        [Trait("Method", "Select")]
        public void Select_UpdatesQuery()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
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
            var query = new DatasyncTableQuery<IdEntity>(Table).Select(m => new IdOnly { Id = m.Id }) as DatasyncTableQuery<IdOnly>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$select=id", odata);
        }
        #endregion

        #region Skip
        [Theory, CombinatorialData]
        [Trait("Method", "Skip")]
        public void Skip_Throws_OutOfRange([CombinatorialValues(-10, -1)] int skip)
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);

            Assert.Throws<ArgumentOutOfRangeException>(() => query.Skip(skip));
        }

        [Fact]
        [Trait("Method", "Skip")]
        public void Skip_Sets_SkipCount()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);

            var actual = query.Skip(5) as DatasyncTableQuery<IdEntity>;

            Assert.Equal(5, actual.SkipCount);
        }

        [Fact]
        [Trait("Method", "Skip")]
        public void Skip_IsCumulative()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);

            var actual = query.Skip(5).Skip(20) as DatasyncTableQuery<IdEntity>;

            Assert.Equal(25, actual.SkipCount);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Skip")]
        public void ToODataQueryString_Skip_IsWellFormed()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).Skip(5) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();
            Assert.Equal("$skip=5", odata);
        }
        #endregion

        #region Take
        [Theory, CombinatorialData]
        [Trait("Method", "Take")]
        public void Take_ThrowsOutOfRange([CombinatorialValues(-10, -1, 0)] int take)
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);

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
            var query = new DatasyncTableQuery<IdEntity>(Table);

            var actual = query.Take(first).Take(second) as DatasyncTableQuery<IdEntity>;

            Assert.Equal(expected, actual.TakeCount);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Take")]
        public void ToODataQueryString_Take_IsWellFormed()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).Take(5) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();
            Assert.Equal("$top=5", odata);
        }
        #endregion

        #region ThenBy
        [Fact]
        [Trait("Method", "ThenBy")]
        public void ThenBy_Null_Throws()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            Expression<Func<IdEntity, string>> keySelector = null;

            Assert.Throws<ArgumentNullException>(() => query.ThenBy(keySelector));
        }

        [Fact]
        [Trait("Method", "ThenBy")]
        public void ThenBy_UpdatesQuery()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
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
            var query = new DatasyncTableQuery<IdEntity>(Table).ThenBy(m => m.Id) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$orderby=id", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenBy")]
        public void ToODataQueryString_ThenBy_ThrowsNotSupported()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).ThenBy(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
        #endregion

        #region ThenByDescending
        [Fact]
        [Trait("Method", "ThenByDescending")]
        public void ThenByDescending_Null_Throws()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            Expression<Func<IdEntity, string>> keySelector = null;

            Assert.Throws<ArgumentNullException>(() => query.ThenByDescending(keySelector));
        }

        [Fact]
        [Trait("Method", "ThenByDescending")]
        public void ThenByDescending_UpdatesQuery()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
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
            var query = new DatasyncTableQuery<IdEntity>(Table).ThenByDescending(m => m.Id) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$orderby=id desc", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "ThenByDescending")]
        public void ToODataQueryString_ThenByDescending_ThrowsNotSupported()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).ThenByDescending(m => m.Id.ToLower()) as DatasyncTableQuery<IdEntity>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
        #endregion

        #region ToAsyncPageable
        [Fact]
        [Trait("Method", "ToAsyncPageable")]
        public async Task ToAsyncPageable_WithCount_Executes()
        {
            // Arrange
            var page1 = CreatePageOfItems(5, 10, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, 10, new Uri($"{sEndpoint}?page=3"));
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            List<IdEntity> items = new();
            var query = new DatasyncTableQuery<IdEntity>(Table).IncludeTotalCount();
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
            var page1 = CreatePageOfItems(5, 10, new Uri($"{sEndpoint}?page=2"));
            var page2 = CreatePageOfItems(5, 10, new Uri($"{sEndpoint}?page=3"));
            MockHandler.AddResponse(HttpStatusCode.OK, new Page<IdEntity>());
            List<IdEntity> items = new();
            var query = new DatasyncTableQuery<IdEntity>(Table).Where(m => m.StringValue == "foo").IncludeTotalCount();
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
        #endregion

        #region ToLazyObservableCollection


        [Theory]
        [ClassData(typeof(LinqTestCases))]
        [Trait("Method", "ToODataQueryString")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case doesn't use values")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case doesn't use values")]
        internal async Task ToLazyObservableCollection_WithPageCount_WithLinq(string testCase, Func<ITableQuery<Movie>, ITableQuery<Movie>> func, string expected, int expectedCount, string[] expectedIds)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var table = client.GetTable<Movie>("movies");
            int loops = 0;
            const int maxLoops = (Movies.Count / 50) + 2;
            var query = new DatasyncTableQuery<Movie>(table);

            // Act
            var sut = (func.Invoke(query) as DatasyncTableQuery<Movie>)?.ToLazyObservableCollection(50) as InternalLazyObservableCollection<Movie>;
            var loadMore = sut.LoadMoreCommand as IAsyncCommand;
            await WaitUntil(() => !sut.IsBusy).ConfigureAwait(false);
            while (loops < maxLoops && sut.HasMoreItems)
            {
                loops++;
                await loadMore.ExecuteAsync().ConfigureAwait(false);
            }

            // Do one more load to make sure.
            await loadMore.ExecuteAsync().ConfigureAwait(false);

            Assert.Equal(expectedCount, sut.Count);
            Assert.False(sut.HasMoreItems);
        }
        #endregion

        #region Where
        [Fact]
        [Trait("Method", "Where")]
        public void Where_Null_Throws()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            Expression<Func<IdEntity, bool>> predicate = null;

            Assert.Throws<ArgumentNullException>(() => query.Where(predicate));
        }

        [Fact]
        [Trait("Method", "Where")]
        public void Where_UpdatesQuery()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
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
            var query = new DatasyncTableQuery<IdEntity>(Table).Where(m => m.Id == "foo") as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("$filter=(id%20eq%20'foo')", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "Where")]
        public void ToODataQueryString_Where_ThrowsNotSupported()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).ThenByDescending(m => m.Id.Normalize() == "foo") as DatasyncTableQuery<IdEntity>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
        #endregion

        #region WithParameter
        [Theory]
        [InlineData(null, "test")]
        [InlineData("test", null)]
        [Trait("Method", "WithParameter")]
        public void WithParameter_Null_Throws(string key, string value)
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);

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
            var query = new DatasyncTableQuery<IdEntity>(Table);

            Assert.Throws<ArgumentException>(() => query.WithParameter(key, value));
        }

        [Fact]
        [Trait("Method", "WithParameter")]
        public void WithParameter_SetsParameter()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);

            var actual = query.WithParameter("testkey", "testvalue") as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("testkey", "testvalue", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "WithParameter")]
        public void WithParameter_Overwrites()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).WithParameter("testkey", "testvalue");

            var actual = query.WithParameter("testkey", "replacement") as DatasyncTableQuery<IdEntity>;

            AssertEx.Contains("testkey", "replacement", actual.QueryParameters);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "WithParameter")]
        public void ToODataQueryString_WithParameter_isWellFormed()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).WithParameter("testkey", "testvalue") as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("testkey=testvalue", odata);
        }

        [Fact]
        [Trait("Method", "ToODataQueryString")]
        [Trait("Method", "WithParameter")]
        public void ToODataQueryString_WithParameter_EncodesValue()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table).WithParameter("testkey", "test value") as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("testkey=test%20value", odata);
        }
        #endregion

        #region WithParameters
        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_Null_Throws()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);

            Assert.Throws<ArgumentNullException>(() => query.WithParameters(null));
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_Empty_Throws()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            var sut = new Dictionary<string, string>();

            Assert.Throws<ArgumentException>(() => query.WithParameters(sut));
        }

        [Fact]
        [Trait("Method", "WithParameters")]
        public void WithParameters_CopiesParams()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
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
            var query = new DatasyncTableQuery<IdEntity>(Table).WithParameter("key1", "value1");
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
            var query = new DatasyncTableQuery<IdEntity>(Table);
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
            var pairs = new Dictionary<string, string>()
            {
                {  "key1", "value1" },
                {  "key2", "value 2" }
            };
            var query = new DatasyncTableQuery<IdEntity>(Table).WithParameters(pairs) as DatasyncTableQuery<IdEntity>;

            var odata = query.ToODataQueryString();

            Assert.Equal("key1=value1&key2=value%202", odata);
        }
        #endregion

        #region Linq Tests
        [Theory]
        [ClassData(typeof(LinqTestCases))]
        [Trait("Method", "ToODataQueryString")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case doesn't use values")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case doesn't use values")]
        internal void LinqODataConversions(string testCase, Func<ITableQuery<Movie>, ITableQuery<Movie>> func, string expected, int expectedCount, string[] expectedIds)
        {
            // Arrange
            var client = CreateClientForMocking();
            var table = new DatasyncTable<Movie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act
            var actual = (func.Invoke(query) as DatasyncTableQuery<Movie>)?.ToODataQueryString();
            var tester = Uri.UnescapeDataString(actual);

            // Assert
            Assert.NotNull(actual);

            var expectedParams = expected.Split('&').ToList();
            var actualParams = tester.Split('&').ToList();
            // actualParams and expectedParams need to be the same, but can be in different order
            actualParams.Should().BeEquivalentTo(expectedParams, $"Test Case {testCase} OData String");
        }

        [Theory]
        [ClassData(typeof(LinqTestCases))]
        [Trait("Method", "ToODataQueryString")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case doesn't use values")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case doesn't use values")]
        internal void LinqODataWithSelectConversions(string testCase, Func<ITableQuery<Movie>, ITableQuery<Movie>> func, string expected, int expectedCount, string[] expectedIds)
        {
            // Arrange
            var client = CreateClientForMocking();
            var table = new DatasyncTable<Movie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Need to make sure the $select statement is added in the right spot.
            var splitArgs = expected.Split('&').ToList();
            splitArgs.Add("$select=id,title");
            splitArgs.Sort();
            var expectedWithSelect = string.Join('&', splitArgs).TrimStart('&');

            // Act
            var actual = (func.Invoke(query).Select(m => new SelectResult { Id = m.Id, Title = m.Title }) as DatasyncTableQuery<SelectResult>)?.ToODataQueryString();
            var tester = Uri.UnescapeDataString(actual);

            // Assert
            Assert.NotNull(actual);
            Assert.True(tester.Equals(expectedWithSelect), $"Test '{testCase}' did not match (with select)\nExpected: {expectedWithSelect}\nActual  : {tester}");
        }

        [Theory]
        [ClassData(typeof(LinqTestCases))]
        [Trait("Method", "ToAsyncPageable")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case doesn't use values")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case doesn't use values")]

        internal async Task ToAsyncPageable_WithLiveServer(string testCase, Func<ITableQuery<Movie>, ITableQuery<Movie>> func, string expectedOData, int expectedCount, string[] expectedIds)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var table = client.GetTable<Movie>("movies");
            var query = new DatasyncTableQuery<Movie>(table as DatasyncTable<Movie>);

            // Act
            var pageable = func.Invoke(query).ToAsyncPageable();
            var list = await pageable.ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedCount, list.Count);
            var actualItems = list.Take(expectedIds.Length).Select(m => m.Id).ToArray();
            Assert.Equal(expectedIds, actualItems);
        }

        [Theory]
        [ClassData(typeof(LinqTestCases))]
        [Trait("Method", "ToAsyncEnumerable")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case doesn't use values")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case doesn't use values")]

        internal async Task ToAsyncEnumerable_WithLiveServer(string testCase, Func<ITableQuery<Movie>, ITableQuery<Movie>> func, string expectedOData, int expectedCount, string[] expectedIds)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var table = client.GetTable<Movie>("movies");
            var query = new DatasyncTableQuery<Movie>(table as DatasyncTable<Movie>);

            // Act
            var pageable = func.Invoke(query).ToAsyncEnumerable();
            var list = await pageable.ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedCount, list.Count);
            var actualItems = list.Take(expectedIds.Length).Select(m => m.Id).ToArray();
            Assert.Equal(expectedIds, actualItems);
        }

        [Fact]
        public void Linq_NotSupportedProperties()
        {
            // Arrange
            var client = CreateClientForMocking();
            var table = new DatasyncTable<Movie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act
            var actual = query.Where(m => m.ReleaseDate.UtcDateTime > new DateTime(2001, 12, 31)) as DatasyncTableQuery<Movie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedMethods()
        {
            // Arrange
            var client = CreateClientForMocking();
            var table = new DatasyncTable<Movie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act
            var actual = query.Where(m => m.Title.LastIndexOf("er") > 0) as DatasyncTableQuery<Movie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedBinaryOperators()
        {
            // Arrange
            var client = CreateClientForMocking();
            var table = new DatasyncTable<Movie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act
            var actual = query.Where(m => (m.Year ^ 1024) == 0) as DatasyncTableQuery<Movie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedUnaryOperators()
        {
            // Arrange
            var client = CreateClientForMocking();
            var table = new DatasyncTable<Movie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act
            var actual = query.Where(m => (5 * (-m.Duration)) > -180) as DatasyncTableQuery<Movie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedDistinctLinqStatement()
        {
            // Arrange
            var client = CreateClientForMocking();
            var table = new DatasyncTable<Movie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act - really - you should NOT be doing this!
            query.Query = query.Query.Distinct();

            // Assert
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_NegateNotSupported()
        {
            // Arrange
            var client = CreateClientForMocking();
            var table = new DatasyncTable<Movie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table).Where(m => (-m.Year) <= -2000) as DatasyncTableQuery<Movie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_InvalidOrderBy_Lambda()
        {
            // Arrange
            var client = CreateClientForMocking();
            var table = new DatasyncTable<Movie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table).OrderBy(m => m.Id == "foo" ? "yes" : "no") as DatasyncTableQuery<Movie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_InvalidOrderBy_Method()
        {
            // Arrange
            var client = CreateClientForMocking();
            var table = new DatasyncTable<Movie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table).OrderBy(m => m.GetHashCode()) as DatasyncTableQuery<Movie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_InvalidOrderBy_ToString()
        {
            // Arrange
            var client = CreateClientForMocking();
            var table = new DatasyncTable<Movie>(Endpoint, client.HttpClient, client.ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table).OrderBy(m => m.ReleaseDate.ToString("o")) as DatasyncTableQuery<Movie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
        #endregion
    }
}

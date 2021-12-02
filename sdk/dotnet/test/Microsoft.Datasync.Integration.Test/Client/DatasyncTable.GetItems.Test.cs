// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Datasync.Common.Test.TestData;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Commands;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client
{
    public class DatasyncTable_GetItems_Test : BaseTest
    {
        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_RetrievesItems()
        {
            // Arrange
            var client = GetMovieClient();
            var table = client.GetTable<ClientMovie>("movies");
            int count = 0;

            var pageable = table.GetAsyncItems<ClientMovie>("$count=true");
            Assert.NotNull(pageable);

            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                count++;
                var item = enumerator.Current;

                Assert.NotNull(item);
                Assert.NotNull(item.Id);

                var expected = MovieServer.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);

                Assert.Equal(MovieCount, pageable.Count);
                Assert.NotNull(pageable.CurrentResponse);
            }

            Assert.Equal(MovieCount, count);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_AsPages_RetrievesItems()
        {
            // Arrange
            var client = GetMovieClient();
            var table = client.GetTable<ClientMovie>("movies");
            int itemCount = 0, pageCount = 0;

            var pageable = table.GetAsyncItems<ClientMovie>().AsPages();
            Assert.NotNull(pageable);

            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                pageCount++;

                var page = enumerator.Current;
                Assert.NotNull(page);
                foreach (var item in page.Items)
                {
                    itemCount++;
                    Assert.NotNull(item.Id);
                    var expected = MovieServer.GetMovieById(item.Id);
                    Assert.Equal<IMovie>(expected, item);
                }
            }

            Assert.Equal(MovieCount, itemCount);
            Assert.Equal(3, pageCount);
        }

        [Theory, ClassData(typeof(QueryTestCases))]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItemsAsync_Basic(QueryTestCase testcase)
        {
            // Arrange
            var segments = testcase.PathAndQuery.Split('?');
            var tableName = segments[0].Split('/').Last();
            var query = segments.Length > 1 ? segments[1] : string.Empty;
            var client = GetMovieClient();
            var table = (DatasyncTable<ClientMovie>)client.GetTable<ClientMovie>(tableName)!;

            // Act
            var response = await table.GetPageOfItemsAsync<ClientMovie>(query).ConfigureAwait(false);
            var result = response.Value;
            var items = result.Items.ToArray();

            // Assert
            Assert.Equal(testcase.ItemCount, items.Length);
            Assert.Equal(testcase.TotalCount, result.Count ?? 0);

            if (testcase.NextLinkQuery == null)
            {
                Assert.Null(result.NextLink);
            }
            else
            {
                var nextlinkSegments = result.NextLink.PathAndQuery.Split('?');
                var expectedSegments = testcase.NextLinkQuery.Split('?');
                Assert.Equal(expectedSegments[0].Trim('/'), nextlinkSegments[0].Trim('/'));
                Assert.Equal(expectedSegments[1], Uri.UnescapeDataString(nextlinkSegments[1]));
            }

            // The first n items must match what is expected
            Assert.True(items.Length >= testcase.FirstItems.Length);
            Assert.Equal(testcase.FirstItems, result.Items.Take(testcase.FirstItems.Length).Select(m => m.Id).ToArray());
            for (int idx = 0; idx < testcase.FirstItems.Length; idx++)
            {
                var expected = MovieServer.GetMovieById(testcase.FirstItems[idx]);
                var actual = items[idx];
                Assert.Equal<IMovie>(expected, actual);
                AssertEx.SystemPropertiesMatch(expected, actual);
            }
        }

        [Theory, PairwiseData]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItemsAsync_Select(bool sId, bool sUpdatedAt, bool sVersion, bool sDeleted, bool sBPW, bool sduration, bool srating, bool sreleaseDate, bool stitle, bool syear)
        {
            // Arrange
            var client = GetMovieClient();
            var table = (DatasyncTable<ClientMovie>)client.GetTable<ClientMovie>("movies")!;

            List<string> selection = new();
            if (sId) selection.Add("id");
            if (sUpdatedAt) selection.Add("updatedAt");
            if (sVersion) selection.Add("version");
            if (sDeleted) selection.Add("deleted");
            if (sBPW) selection.Add("bestPictureWinner");
            if (sduration) selection.Add("duration");
            if (srating) selection.Add("rating");
            if (sreleaseDate) selection.Add("releaseDate");
            if (stitle) selection.Add("title");
            if (syear) selection.Add("year");
            if (selection.Count == 0) return;
            var query = $"$top=5&$skip=5&$select={string.Join(',', selection)}";

            // Act
            var response = await table.GetPageOfItemsAsync<Dictionary<string, object>>(query).ConfigureAwait(false);
            var result = response.Value;
            var items = result.Items.ToArray();

            // Assert
            foreach (var item in items)
            {
                foreach (var property in selection)
                {
                    Assert.True(item.ContainsKey(property));
                }
            }
        }

        [Theory]
        [InlineData("tables/notfound", HttpStatusCode.NotFound)]
        [InlineData("tables/movies?$filter=(year - 1900) ge 100", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$filter=missing eq 20", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$orderby=duration fizz", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$orderby=missing asc", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$select=foo", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$select=year rating", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$skip=-1", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$skip=NaN", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$top=-1", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$top=1000000", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$top=NaN", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$filter=(year - 1900) ge 100", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$filter=missing eq 20", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$orderby=duration fizz", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$orderby=missing asc", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$select=foo", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$select=year rating", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$skip=-1", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$skip=NaN", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$top=-1", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$top=1000000", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$top=NaN", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_rated", HttpStatusCode.Unauthorized)]
        [InlineData("tables/movies_legal", HttpStatusCode.UnavailableForLegalReasons)]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItemsAsync_FailedQuery(string pathAndQuery, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = GetMovieClient();
            var segments = pathAndQuery.Split('?');
            var tableName = segments[0].Split('/')[1];
            var query = segments.Length > 1 ? segments[1] : string.Empty;
            var table = (DatasyncTable<ClientMovie>)client.GetTable<ClientMovie>(tableName)!;

            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetPageOfItemsAsync<ClientMovie>(query)).ConfigureAwait(false);

            Assert.Equal(expectedStatusCode, exception.Response.StatusCode);
        }

        [Theory]
        [InlineData("tables/soft?$count=true", 100, "tables/soft?$count=true&$skip=100", 154, new[] { "id-004", "id-005", "id-006", "id-008", "id-010" })]
        [InlineData("tables/soft?$filter=deleted eq false&__includedeleted=true", 100, "tables/soft?$filter=deleted eq false&__includedeleted=true&$skip=100", 0, new[] { "id-004", "id-005", "id-006", "id-008", "id-010" })]
        [InlineData("tables/soft?$filter=deleted eq true&__includedeleted=true", 94, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/soft?__includedeleted=true", 100, "tables/soft?__includedeleted=true&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004", "id-005" })]
        [Trait("Method", "GetPageOfItemsAsync")]
        public async Task GetPageOfItemsAsync_SoftDeleteQuery(string pathAndQuery, int expectedItemCount, string expectedNextLinkQuery, long expectedTotalCount, string[] firstExpectedItems)
        {
            // Soft-Delete all movies that are R rated
            await MovieServer.SoftDeleteMoviesAsync(x => x.Rating == "R").ConfigureAwait(false);

            // Arrange
            var segments = pathAndQuery.Split('?');
            var tableName = segments[0].Split('/')[1];
            string query = segments[1];
            var client = GetMovieClient();
            var table = (DatasyncTable<ClientMovie>)client.GetTable<ClientMovie>(tableName)!;

            // Act
            var response = await table.GetPageOfItemsAsync<ClientMovie>(query).ConfigureAwait(false);
            var result = response.Value;
            var items = result.Items.ToArray();

            // Assert
            Assert.Equal(expectedItemCount, items.Length);
            Assert.Equal(expectedTotalCount, result.Count ?? 0);

            if (expectedNextLinkQuery == null)
            {
                Assert.Null(result.NextLink);
            }
            else
            {
                var nextlinkSegments = result.NextLink.PathAndQuery.Split('?');
                var expectedSegments = expectedNextLinkQuery.Split('?');
                Assert.Equal(expectedSegments[0].Trim('/'), nextlinkSegments[0].Trim('/'));
                Assert.Equal(expectedSegments[1], Uri.UnescapeDataString(nextlinkSegments[1]));
            }

            // The first n items must match what is expected
            Assert.True(items.Length >= firstExpectedItems.Length);
            Assert.Equal(firstExpectedItems, result.Items.Take(firstExpectedItems.Length).Select(m => m.Id).ToArray());
            for (int idx = 0; idx < firstExpectedItems.Length; idx++)
            {
                var expected = MovieServer.GetMovieById(firstExpectedItems[idx]);
                var actual = items[idx];
                Assert.Equal<IMovie>(expected, actual);
                AssertEx.SystemPropertiesMatch(expected, actual);
            }
        }

        [Fact]
        [Trait("Method", "ToAsyncEnumerable")]
        public async Task ToAsyncEnumerable_RetrievesItems()
        {
            // Arrange
            var client = GetMovieClient();
            var table = client.GetTable<ClientMovie>("movies");
            int count = 0;

            var result = table.ToAsyncEnumerable();

            var enumerator = result.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                count++;
                var item = enumerator.Current;

                Assert.NotNull(item);
                Assert.NotNull(item.Id);

                var expected = MovieServer.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);
            }

            Assert.Equal(MovieCount, count);
        }

        [Fact]
        [Trait("Method", "ToAsyncPageable")]
        public async Task ToAsyncPageable_RetrievesItems()
        {
            // Arrange
            var client = GetMovieClient();
            var table = client.GetTable<ClientMovie>("movies");
            int count = 0;

            var pageable = table.ToAsyncPageable();
            Assert.NotNull(pageable);

            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                count++;
                var item = enumerator.Current;

                Assert.NotNull(item);
                Assert.NotNull(item.Id);

                var expected = MovieServer.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);

                Assert.Equal(MovieCount, pageable.Count);
                Assert.NotNull(pageable.CurrentResponse);
            }

            Assert.Equal(MovieCount, count);
        }

        [Fact]
        [Trait("Method", "ToAsyncPageable")]
        public async Task ToAsyncPageable_AsPages_RetrievesItems()
        {
            // Arrange
            var client = GetMovieClient();
            var table = client.GetTable<ClientMovie>("movies");
            int itemCount = 0, pageCount = 0;

            var pageable = table.ToAsyncPageable().AsPages();
            Assert.NotNull(pageable);

            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                pageCount++;

                var page = enumerator.Current;
                Assert.NotNull(page);
                foreach (var item in page.Items)
                {
                    itemCount++;
                    Assert.NotNull(item.Id);
                    var expected = MovieServer.GetMovieById(item.Id);
                    Assert.Equal<IMovie>(expected, item);
                }
            }

            Assert.Equal(MovieCount, itemCount);
            Assert.Equal(3, pageCount);
        }

        [Fact]
        [Trait("Method", "ToLazyObservableCollection")]
        public async Task ToLazyObservableCollection_LoadsData()
        {
            // Arrange
            var client = GetMovieClient();
            var table = client.GetTable<ClientMovie>("movies");
            int loops = 0;
            const int maxLoops = (Movies.Count / 20) + 1;

            // Act
            var sut = table.ToLazyObservableCollection() as InternalLazyObservableCollection<ClientMovie>;
            var loadMore = (IAsyncCommand)sut!.LoadMoreCommand;
            await WaitUntil(() => !sut.IsBusy).ConfigureAwait(false);
            while (loops < maxLoops && sut.HasMoreItems)
            {
                loops++;
                await loadMore.ExecuteAsync().ConfigureAwait(false);
            }

            Assert.False(sut.HasMoreItems);
            Assert.Equal(MovieCount, sut.Count);
        }

        [Fact]
        [Trait("Method", "ToLazyObservableCollection")]
        public async Task ToLazyObservableCollection_WtithPageCount_LoadsData()
        {
            // Arrange
            var client = GetMovieClient();
            var table = client.GetTable<ClientMovie>("movies");
            int loops = 0;
            const int maxLoops = (Movies.Count / 50) + 1;

            // Act
            var sut = table.ToLazyObservableCollection(50) as InternalLazyObservableCollection<ClientMovie>;
            var loadMore = sut!.LoadMoreCommand as IAsyncCommand;
            await WaitUntil(() => !sut.IsBusy).ConfigureAwait(false);
            while (loops < maxLoops && sut.HasMoreItems)
            {
                loops++;
                await loadMore!.ExecuteAsync().ConfigureAwait(false);
            }

            Assert.False(sut.HasMoreItems);
            Assert.Equal(MovieCount, sut.Count);
        }
    }
}

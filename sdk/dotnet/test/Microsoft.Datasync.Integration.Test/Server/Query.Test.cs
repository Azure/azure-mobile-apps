// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Datasync.Common.Test.TestData;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Datasync.Integration.Test.Server
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    [Collection("Integration")]
    public class Query_Tests : BaseTest
    {
        public Query_Tests(ITestOutputHelper logger) : base(logger) { }

        [Theory, ClassData(typeof(QueryTestCases))]
        public async Task BasicQueryTest(QueryTestCase testcase)
        {
            Dictionary<string, string> headers = new();
            Utils.AddAuthHeaders(headers, testcase.Username);

            var response = await MovieServer.SendRequest(HttpMethod.Get, testcase.PathAndQuery, headers);

            // Response has the right Status Code
            await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);

            // Response payload can be decoded
            var result = response.DeserializeContent<PageOfItems<ClientMovie>>();
            Assert.NotNull(result);

            // Payload has the right content
            Assert.Equal(testcase.ItemCount, result!.Items!.Length);
            Assert.Equal(testcase.NextLinkQuery, result.NextLink == null ? null : Uri.UnescapeDataString(result.NextLink.PathAndQuery).TrimStart('/'));
            Assert.Equal(testcase.TotalCount, result.Count);

            // The first n items must match what is expected
            Assert.True(result.Items.Length >= testcase.FirstItems.Length);
            Assert.Equal(testcase.FirstItems, result.Items.Take(testcase.FirstItems.Length).Select(m => m.Id).ToArray());
            for (int idx = 0; idx < testcase.FirstItems.Length; idx++)
            {
                var expected = MovieServer.GetMovieById(testcase.FirstItems[idx])!;
                var actual = result.Items[idx];

                Assert.Equal<IMovie>(expected, actual);
                AssertEx.SystemPropertiesMatch(expected, actual);
            }
        }

        /// <summary>
        /// We do a bunch of tests for select by dealing with the overflow properties capabilities within
        /// System.Text.Json - we are not interested in the search (we're doing the same thing over and
        /// over).  Instead, we are ensuring the right selections are made.
        /// </summary>
        [Theory, PairwiseData]
        public async Task SelectQueryTest(bool sId, bool sUpdatedAt, bool sVersion, bool sDeleted, bool sBPW, bool sduration, bool srating, bool sreleaseDate, bool stitle, bool syear)
        {
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
            var query = $"tables/movies?$top=5&$skip=5&$select={string.Join(',', selection)}";

            var response = await MovieServer.SendRequest(HttpMethod.Get, query);

            // Response has the right Status Code
            await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);

            // Response payload can be decoded
            var result = response.DeserializeContent<PageOfItems<ClientObject>>();
            Assert.NotNull(result);

            // There are items in the response payload
            Assert.NotNull(result?.Items);
            foreach (var item in result!.Items!)
            {
                // Each item in the payload has the requested properties.
                foreach (var property in selection)
                {
                    Assert.True(item.Data!.ContainsKey(property));
                }
            }
        }

        [Theory]
        [InlineData("tables/movies", 248, 3)]
        [InlineData("tables/movies_pagesize?$top=100", 100, 4)]
        [InlineData("tables/movies?$count=true", 248, 3)]
        [InlineData("tables/movies?$top=50&$count=true", 50, 1)]
        [InlineData("tables/movies?$filter=releaseDate ge cast(1960-01-01T05:30:00Z,Edm.DateTimeOffset)&orderby=releaseDate asc", 186, 2)]
        [InlineData("tables/movies?$filter=releaseDate ge cast(1960-01-01T05:30:00Z,Edm.DateTimeOffset)&orderby=releaseDate asc&$top=20", 20, 1)]
        public async Task PagingTest(string startQuery, int expectedCount, int expectedLoops)
        {
            var query = startQuery;
            int loops = 0;
            var items = new Dictionary<string, ClientMovie>();

            //Act
            do
            {
                loops++;

                var response = await MovieServer.SendRequest(HttpMethod.Get, query);
                await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
                var result = response.DeserializeContent<StringNextLinkPage<ClientMovie>>();
                Assert.NotNull(result?.Items);
                foreach (var item in result!.Items!)
                {
                    items.Add(item.Id!, item);
                }

                if (result.NextLink != null)
                {
                    Assert.StartsWith("https://localhost/", result.NextLink);
                    query = new Uri(result.NextLink).PathAndQuery;
                }
                else
                {
                    break;
                }
            } while (loops < expectedLoops + 2);

            Assert.Equal(expectedCount, items.Count);
            Assert.Equal(expectedLoops, loops);
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
        [InlineData("tables/movies_rated", HttpStatusCode.Unauthorized, "X-Auth", "failed")]
        [InlineData("tables/movies_legal", HttpStatusCode.UnavailableForLegalReasons)]
        [InlineData("tables/movies_legal", HttpStatusCode.UnavailableForLegalReasons, "X-Auth", "failed")]
        public async Task FailedQueryTest(string relativeUri, HttpStatusCode expectedStatusCode, string? headerName = null, string? headerValue = null)
        {
            Dictionary<string, string> headers = new();
            if (headerName != null && headerValue != null) headers.Add(headerName, headerValue);

            // Act
            var response = await MovieServer.SendRequest(HttpMethod.Get, relativeUri, headers);

            // Assert
            await AssertResponseWithLoggingAsync(expectedStatusCode, response);
        }

        /// <summary>
        /// There are 248 movies, 154 of which are not R-rated (and hence not deleted)
        /// </summary>
        /// <param name="query"></param>
        /// <param name="expectedItemCount"></param>
        /// <param name="expectedNextLinkQuery"></param>
        /// <param name="expectedTotalCount"></param>
        /// <param name="firstExpectedItems"></param>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        [Theory]
        [InlineData("tables/soft", 100, "tables/soft?$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }, "X-ZUMO-Options", "include:deleted")]
        [InlineData("tables/soft?$count=true", 100, "tables/soft?$count=true&$skip=100", 153, new[] { "id-004", "id-005", "id-006", "id-008", "id-010" })]
        [InlineData("tables/soft?$filter=deleted eq false&__includedeleted=true", 100, "tables/soft?$filter=deleted eq false&__includedeleted=true&$skip=100", 0, new[] { "id-004", "id-005", "id-006", "id-008", "id-010" })]
        [InlineData("tables/soft?$filter=deleted eq true&__includedeleted=true", 95, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/soft?__includedeleted=true", 100, "tables/soft?__includedeleted=true&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004", "id-005" })]
        public async Task SoftDeleteQueryTest(string query, int expectedItemCount, string expectedNextLinkQuery, long expectedTotalCount, string[] firstExpectedItems, string? headerName = null, string? headerValue = null)
        {
            await MovieServer.SoftDeleteMoviesAsync(m => m.Rating == "R");
            Dictionary<string, string> headers = new();
            if (headerName != null && headerValue != null) headers.Add(headerName, headerValue);

            var response = await MovieServer.SendRequest(HttpMethod.Get, query, headers);

            // Response has the right Status Code
            await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);

            // Response payload can be decoded
            var result = response.DeserializeContent<PageOfItems<ClientMovie>>();
            Assert.NotNull(result);

            // Payload has the right content
            Assert.Equal(expectedItemCount, result!.Items!.Length);
            Assert.Equal(expectedNextLinkQuery, result.NextLink == null ? null : Uri.UnescapeDataString(result.NextLink.PathAndQuery).TrimStart('/'));
            Assert.Equal(expectedTotalCount, result.Count);

            // The first n items must match what is expected
            Assert.True(result.Items.Length >= firstExpectedItems.Length);
            Assert.Equal(firstExpectedItems, result.Items.Take(firstExpectedItems.Length).Select(m => m.Id).ToArray());
            for (int idx = 0; idx < firstExpectedItems.Length; idx++)
            {
                var expected = MovieServer.GetMovieById(firstExpectedItems[idx])!;
                var actual = result.Items[idx];

                Assert.Equal<IMovie>(expected, actual);
                AssertEx.SystemPropertiesMatch(expected, actual);
            }
        }
    }
}

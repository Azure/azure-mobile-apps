// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Datasync.Integration.Test.Helpers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

using TestData = Datasync.Common.Test.TestData;

namespace Microsoft.Datasync.Integration.Test.Server
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Delete_Tests
    {
        /// <summary>
        /// A connection to the test service.
        /// </summary>
        private readonly TestServer server = MovieApiServer.CreateTestServer();

        [Fact]
        public async Task BasicDeleteTests()
        {
            var id = TestData.Movies.GetRandomId();

            var response = await server.SendRequest(HttpMethod.Delete, $"tables/movies/{id}").ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(TestData.Movies.Count - 1, server.GetMovieCount());
            Assert.Null(server.GetMovieById(id));
        }

        [Theory]
        [InlineData("tables/movies/not-found", HttpStatusCode.NotFound)]
        [InlineData("tables/movies_pagesize/not-found", HttpStatusCode.NotFound)]
        public async Task FailedDeleteTests(string relativeUri, HttpStatusCode expectedStatusCode)
        {
            var response = await server.SendRequest(HttpMethod.Delete, relativeUri).ConfigureAwait(false);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            Assert.Equal(TestData.Movies.Count, server.GetMovieCount());
        }

        [Theory, CombinatorialData]
        public async Task AuthenticatedDeleteTests(
            [CombinatorialValues(0, 1, 2, 3, 7, 14, 25)] int index,
            [CombinatorialValues(null, "failed", "success")] string userId,
            [CombinatorialValues("movies_rated", "movies_legal")] string table)
        {
            string id = Utils.GetMovieId(index);
            Dictionary<string, string> headers = new();
            Utils.AddAuthHeaders(headers, userId);

            var response = await server.SendRequest(HttpMethod.Delete, $"tables/{table}/{id}", headers).ConfigureAwait(false);

            if (userId != "success")
            {
                var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
                Assert.Equal(statusCode, response.StatusCode);
                Assert.Equal(TestData.Movies.Count, server.GetMovieCount());
                Assert.NotNull(server.GetMovieById(id));
            }
            else
            {
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                Assert.Equal(TestData.Movies.Count - 1, server.GetMovieCount());
                Assert.Null(server.GetMovieById(id));
            }
        }

        [Theory]
        [InlineData("If-Match", null, HttpStatusCode.NoContent)]
        [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
        [InlineData("If-None-Match", null, HttpStatusCode.PreconditionFailed)]
        [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.NoContent)]
        public async Task Delete_OnVersion(string headerName, string? headerValue, HttpStatusCode expectedStatusCode)
        {
            const string id = "id-107";
            var expected = server.GetMovieById(id)!;
            Dictionary<string, string> headers = new()
            {
                { headerName, headerValue ?? expected.GetETag() }
            };

            var response = await server.SendRequest(HttpMethod.Delete, $"tables/movies/{id}", headers).ConfigureAwait(false);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            switch (expectedStatusCode)
            {
                case HttpStatusCode.NoContent:
                    Assert.Equal(TestData.Movies.Count - 1, server.GetMovieCount());
                    Assert.Null(server.GetMovieById(id));
                    break;
                case HttpStatusCode.PreconditionFailed:
                    var actual = response.DeserializeContent<ClientMovie>();
                    Assert.NotNull(actual);
                    Assert.Equal<IMovie>(expected, actual!);
                    AssertEx.SystemPropertiesMatch(expected, actual);
                    AssertEx.ResponseHasConditionalHeaders(expected, response);
                    break;
            }
        }

        [Theory]
        [InlineData("If-Modified-Since", -1, HttpStatusCode.NoContent)]
        [InlineData("If-Modified-Since", 1, HttpStatusCode.PreconditionFailed)]
        [InlineData("If-Unmodified-Since", 1, HttpStatusCode.NoContent)]
        [InlineData("If-Unmodified-Since", -1, HttpStatusCode.PreconditionFailed)]
        public async Task Delete_OnModified(string headerName, int offset, HttpStatusCode expectedStatusCode)
        {
            const string id = "id-107";
            var expected = server.GetMovieById(id)!;
            Dictionary<string, string> headers = new()
            {
                { headerName, expected.UpdatedAt.AddHours(offset).ToString("R") }
            };

            var response = await server.SendRequest(HttpMethod.Delete, $"tables/movies/{id}", headers).ConfigureAwait(false);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            switch (expectedStatusCode)
            {
                case HttpStatusCode.NoContent:
                    Assert.Equal(TestData.Movies.Count - 1, server.GetMovieCount());
                    Assert.Null(server.GetMovieById(id));
                    break;
                case HttpStatusCode.PreconditionFailed:
                    var actual = response.DeserializeContent<ClientMovie>();
                    Assert.NotNull(actual);
                    Assert.Equal<IMovie>(expected, actual!);
                    AssertEx.SystemPropertiesMatch(expected, actual);
                    AssertEx.ResponseHasConditionalHeaders(expected, response);
                    break;
            }
        }

        [Theory, CombinatorialData]
        public async Task SoftDeleteItem_SetsDeletedFlag([CombinatorialValues("soft", "soft_logged")] string table)
        {
            var id = TestData.Movies.GetRandomId();

            var response = await server.SendRequest(HttpMethod.Delete, $"tables/{table}/{id}").ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(TestData.Movies.Count, server.GetMovieCount());
            var entity = server.GetMovieById(id)!;
            Assert.True(entity.Deleted);
        }

        [Theory, CombinatorialData]
        public async Task SoftDeleteItem_GoneWhenDeleted([CombinatorialValues("soft", "soft_logged")] string table)
        {
            var id = TestData.Movies.GetRandomId();
            await server.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);

            var response = await server.SendRequest(HttpMethod.Delete, $"tables/{table}/{id}").ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
            Assert.Equal(TestData.Movies.Count, server.GetMovieCount());
            var currentEntity = server.GetMovieById(id)!;
            Assert.True(currentEntity.Deleted);
        }
    }
}

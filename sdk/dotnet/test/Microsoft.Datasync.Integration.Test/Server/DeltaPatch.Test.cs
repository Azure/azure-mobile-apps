// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Datasync.Integration.Test.Helpers;
using System;
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
    public class DeltaPatch_Tests
    {
        /// <summary>
        /// The time that the test started
        /// </summary>
        private readonly DateTimeOffset startTime = DateTimeOffset.Now;

        /// <summary>
        /// A connection to the test service.
        /// </summary>
        private readonly TestServer server = MovieApiServer.CreateTestServer();

        [Theory, CombinatorialData]
        public async Task BasicPatchTests([CombinatorialValues("movies", "movies_pagesize")] string table)
        {
            var id = TestData.Movies.GetRandomId();
            var expected = server.GetMovieById(id)!;
            expected.Title = "Test Movie Title";
            expected.Rating = "PG-13";

            var patchDoc = new Dictionary<string, object>()
            {
                { "title", "Test Movie Title" },
                { "rating", "PG-13" }
            };

            var response = await server.SendRequest(HttpMethod.Patch, $"tables/{table}/{id}", patchDoc, "application/json", null).ConfigureAwait(true);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.DeserializeContent<ClientMovie>();
            Assert.NotNull(result);

            var stored = server.GetMovieById(id);
            Assert.NotNull(stored);

            AssertEx.SystemPropertiesSet(stored, startTime);
            AssertEx.SystemPropertiesChanged(expected, stored);
            AssertEx.SystemPropertiesMatch(stored, result);
            Assert.Equal<IMovie>(expected, result!);
            AssertEx.ResponseHasConditionalHeaders(stored, response);
        }

        [Theory, CombinatorialData]
        public async Task CannotPatchSystemProperties(
            [CombinatorialValues("movies", "movies_pagesize")] string table,
            [CombinatorialValues("Id", "UpdatedAt", "Version")] string propName)
        {
            // Arrange
            Dictionary<string, string> propValues = new()
            {
                { "Id", "test-id" },
                { "UpdatedAt", "2018-12-31T05:00:00.000Z" },
                { "Version", "dGVzdA==" }
            };

            var id = TestData.Movies.GetRandomId();
            var expected = server.GetMovieById(id)!;
            var patchDoc = new Dictionary<string, object>()
            {
                { propName, propValues[propName] }
            };

            var response = await server.SendRequest(HttpMethod.Patch, $"tables/{table}/{id}", patchDoc, "application/json", null).ConfigureAwait(true);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var stored = server.GetMovieById(id);
            Assert.NotNull(stored);
            Assert.Equal<IMovie>(expected, stored!);
            Assert.Equal<ITableData>(expected, stored!);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound, "tables/movies/missing-id")]
        [InlineData(HttpStatusCode.NotFound, "tables/movies_pagesize/missing-id")]
        public async Task PatchFailureTests(HttpStatusCode expectedStatusCode, string relativeUri)
        {
            var patchDoc = new Dictionary<string, object>()
            {
                { "Title", "Test Movie Title" },
                { "Rating", "PG-13" }
            };

            var response = await server.SendRequest(HttpMethod.Patch, relativeUri, patchDoc, "application/json", null).ConfigureAwait(true);
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Fact]
        public async Task PatchFailedWithWrongContentType()
        {
            var id = TestData.Movies.GetRandomId();
            var expected = server.GetMovieById(id)!;
            var patchDoc = new Dictionary<string, object>()
            {
                { "Title", "Test Movie Title" },
                { "Rating", "PG-13" }
            };

            var response = await server.SendRequest(HttpMethod.Patch, $"tables/movies/{id}", patchDoc, "application/json+problem", null).ConfigureAwait(true);

            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
            var stored = server.GetMovieById(id);
            Assert.NotNull(stored);
            Assert.Equal<IMovie>(expected, stored!);
            Assert.Equal<ITableData>(expected, stored!);
        }

        [Fact]
        public async Task PatchFailedWithMalformedJson()
        {
            var id = TestData.Movies.GetRandomId();
            var expected = server.GetMovieById(id)!;
            const string patchDoc = "{ \"some-malformed-json\": null";

            var response = await server.SendRequest(HttpMethod.Patch, $"tables/movies/{id}", patchDoc, "application/json", null).ConfigureAwait(true);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var stored = server.GetMovieById(id);
            Assert.NotNull(stored);
            Assert.Equal<IMovie>(expected, stored!);
            Assert.Equal<ITableData>(expected, stored!);
        }

        [Theory]
        [InlineData("duration", 50)]
        [InlineData("duration", 370)]
        [InlineData("rating", "M")]
        [InlineData("rating", "PG-13 but not always")]
        [InlineData("title", "a")]
        [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
        [InlineData("year", 1900)]
        [InlineData("year", 2035)]
        [InlineData("Duration", 50)]
        [InlineData("Duration", 370)]
        [InlineData("Rating", "M")]
        [InlineData("Rating", "PG-13 but not always")]
        [InlineData("Title", "a")]
        [InlineData("Title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
        [InlineData("Year", 1900)]
        [InlineData("Year", 2035)]
        public async Task PatchValidationFailureTests(string propName, object propValue)
        {
            string id = TestData.Movies.GetRandomId();
            var patchDoc = new Dictionary<string, object>()
            {
                { propName, propValue }
            };

            var response = await server.SendRequest(HttpMethod.Patch, $"tables/movies/{id}", patchDoc, "application/json", null).ConfigureAwait(true);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory, CombinatorialData]
        public async Task AuthenticatedPatchTests(
            [CombinatorialValues(0, 1, 2, 3, 7, 14, 25)] int index,
            [CombinatorialValues(null, "failed", "success")] string userId,
            [CombinatorialValues("movies_rated", "movies_legal")] string table)
        {
            var id = Utils.GetMovieId(index);
            var original = server.GetMovieById(id)!;
            var expected = original.Clone();
            expected.Title = "TEST MOVIE TITLE"; // Upper Cased because of the PreCommitHook
            expected.Rating = "PG-13";

            var patchDoc = new Dictionary<string, object>()
            {
                { "Title", "Test Movie Title" },
                { "Rating", "PG-13" }
            };

            Dictionary<string, string> headers = new();
            Utils.AddAuthHeaders(headers, userId);

            var response = await server.SendRequest(HttpMethod.Patch, $"tables/{table}/{id}", patchDoc, "application/json", headers).ConfigureAwait(true);

            if (userId != "success")
            {
                var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
                Assert.Equal(statusCode, response.StatusCode);
                var stored = server.GetMovieById(id);
                Assert.NotNull(stored);
                Assert.Equal<IMovie>(original, stored!);
                Assert.Equal<ITableData>(original, stored!);
            }
            else
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var result = response.DeserializeContent<ClientMovie>();
                var stored = server.GetMovieById(id);
                Assert.NotNull(stored);
                AssertEx.SystemPropertiesSet(stored, startTime);
                AssertEx.SystemPropertiesChanged(expected, stored);
                AssertEx.SystemPropertiesMatch(stored, result);
                Assert.Equal<IMovie>(expected, result!);
                AssertEx.ResponseHasConditionalHeaders(stored, response);
            }
        }

        [Theory]
        [InlineData("If-Match", null, HttpStatusCode.OK)]
        [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
        [InlineData("If-None-Match", null, HttpStatusCode.PreconditionFailed)]
        [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.OK)]
        public async Task ConditionalVersionPatchTests(string headerName, string? headerValue, HttpStatusCode expectedStatusCode)
        {
            string id = TestData.Movies.GetRandomId();
            var entity = server.GetMovieById(id)!;
            var expected = entity.Clone();
            Dictionary<string, string> headers = new()
            {
                { headerName, headerValue ?? entity.GetETag() }
            };
            var patchDoc = new Dictionary<string, object>()
            {
                { "Title", "Test Movie Title" },
                { "Rating", "PG-13" }
            };

            var response = await server.SendRequest(HttpMethod.Patch, $"tables/movies/{id}", patchDoc, "application/json", headers).ConfigureAwait(true);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            var actual = response.DeserializeContent<ClientMovie>();
            var stored = server.GetMovieById(id);
            Assert.NotNull(stored);

            switch (expectedStatusCode)
            {
                case HttpStatusCode.OK:
                    // Do the replacement in the expected
                    expected.Title = "Test Movie Title";
                    expected.Rating = "PG-13";

                    AssertEx.SystemPropertiesSet(stored, startTime);
                    AssertEx.SystemPropertiesChanged(expected, stored);
                    AssertEx.SystemPropertiesMatch(stored, actual);
                    Assert.Equal<IMovie>(expected, actual!);
                    AssertEx.ResponseHasConditionalHeaders(stored, response);
                    break;
                case HttpStatusCode.PreconditionFailed:
                    Assert.Equal<IMovie>(expected, actual!);
                    AssertEx.SystemPropertiesMatch(expected, actual);
                    AssertEx.ResponseHasConditionalHeaders(expected, response);
                    break;
            }
        }

        [Theory]
        [InlineData("If-Modified-Since", -1, HttpStatusCode.OK)]
        [InlineData("If-Modified-Since", 1, HttpStatusCode.PreconditionFailed)]
        [InlineData("If-Unmodified-Since", 1, HttpStatusCode.OK)]
        [InlineData("If-Unmodified-Since", -1, HttpStatusCode.PreconditionFailed)]
        public async Task ConditionalModifiedPatchTests(string headerName, int offset, HttpStatusCode expectedStatusCode)
        {
            string id = TestData.Movies.GetRandomId();
            var entity = server.GetMovieById(id)!;
            var expected = entity.Clone();
            Dictionary<string, string> headers = new()
            {
                { headerName, entity.UpdatedAt.AddHours(offset).ToString("R") }
            };
            var patchDoc = new Dictionary<string, object>()
            {
                { "Title", "Test Movie Title" },
                { "Rating", "PG-13" }
            };

            var response = await server.SendRequest(HttpMethod.Patch, $"tables/movies/{id}", patchDoc, "application/json", headers).ConfigureAwait(true);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            var actual = response.DeserializeContent<ClientMovie>();
            var stored = server.GetMovieById(id);
            Assert.NotNull(stored);

            switch (expectedStatusCode)
            {
                case HttpStatusCode.OK:
                    // Do the replacement in the expected
                    expected.Title = "Test Movie Title";
                    expected.Rating = "PG-13";

                    AssertEx.SystemPropertiesSet(stored, startTime);
                    AssertEx.SystemPropertiesChanged(expected, stored);
                    AssertEx.SystemPropertiesMatch(stored, actual);
                    Assert.Equal<IMovie>(expected, actual!);
                    AssertEx.ResponseHasConditionalHeaders(stored, response);
                    break;
                case HttpStatusCode.PreconditionFailed:
                    Assert.Equal<IMovie>(expected, actual!);
                    AssertEx.SystemPropertiesMatch(expected, actual);
                    AssertEx.ResponseHasConditionalHeaders(expected, response);
                    break;
            }
        }

        [Theory, CombinatorialData]
        public async Task SoftDeletePatch_PatchDeletedItem_ReturnsGone([CombinatorialValues("soft", "soft_logged")] string table)
        {
            var id = TestData.Movies.GetRandomId();
            await server.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(true);

            var patchDoc = new Dictionary<string, object>()
            {
                { "Title", "Test Movie Title" },
                { "Rating", "PG-13" }
            };

            var response = await server.SendRequest(HttpMethod.Patch, $"tables/{table}/{id}", patchDoc, "application/json", null).ConfigureAwait(true);
            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        }

        [Theory, CombinatorialData]
        public async Task SoftDeletePatch_CanUndeleteDeletedItem([CombinatorialValues("soft", "soft_logged")] string table)
        {
            var id = TestData.Movies.GetRandomId();
            await server.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(true);

            var expected = server.GetMovieById(id)!;
            expected.Deleted = false;

            var patchDoc = new Dictionary<string, object>()
            {
                { "Deleted", false }
            };

            var response = await server.SendRequest(HttpMethod.Patch, $"tables/{table}/{id}", patchDoc, "application/json", null).ConfigureAwait(true);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.DeserializeContent<ClientMovie>();
            var stored = server.GetMovieById(id);
            Assert.NotNull(stored);
            AssertEx.SystemPropertiesSet(stored, startTime);
            AssertEx.SystemPropertiesChanged(expected, stored);
            AssertEx.SystemPropertiesMatch(stored, result);
            Assert.Equal<IMovie>(expected, result!);
            AssertEx.ResponseHasConditionalHeaders(stored, response);
        }

        [Theory, CombinatorialData]
        public async Task SoftDeletePatch_PatchNotDeletedItem([CombinatorialValues("soft", "soft_logged")] string table)
        {
            var id = TestData.Movies.GetRandomId();
            var expected = server.GetMovieById(id)!;
            expected.Title = "Test Movie Title";
            expected.Rating = "PG-13";

            var patchDoc = new Dictionary<string, object>()
            {
                { "Title", "Test Movie Title" },
                { "Rating", "PG-13" }
            };

            var response = await server.SendRequest(HttpMethod.Patch, $"tables/{table}/{id}", patchDoc, "application/json", null).ConfigureAwait(true);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.DeserializeContent<ClientMovie>();
            var stored = server.GetMovieById(id);
            Assert.NotNull(stored);
            AssertEx.SystemPropertiesSet(stored, startTime);
            AssertEx.SystemPropertiesChanged(expected, stored);
            AssertEx.SystemPropertiesMatch(stored, result);
            Assert.Equal<IMovie>(expected, result!);
            AssertEx.ResponseHasConditionalHeaders(stored, response);
        }
    }
}

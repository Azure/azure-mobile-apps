﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
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
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Xunit;

using TestData = Datasync.Common.Test.TestData;

namespace Microsoft.Datasync.Integration.Test.Server
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Patch_Tests
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
            var patchDoc = new PatchOperation[]
            {
                new PatchOperation("replace", "title", "Test Movie Title"),
                new PatchOperation("replace", "rating", "PG-13")
            };

            var response = await server.SendPatch($"tables/{table}/{id}", patchDoc).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.DeserializeContent<ClientMovie>();
            var stored = server.GetMovieById(id);
            AssertEx.SystemPropertiesSet(stored, startTime);
            AssertEx.SystemPropertiesChanged(expected, stored);
            AssertEx.SystemPropertiesMatch(stored, result);
            Assert.Equal<IMovie>(expected, result!);
            AssertEx.ResponseHasConditionalHeaders(stored, response);
        }

        [Theory, CombinatorialData]
        public async Task CannotPatchSystemProperties(
            [CombinatorialValues("movies", "movies_pagesize")] string table,
            [CombinatorialValues("/id", "/updatedAt", "/version")] string propName)
        {
            Dictionary<string, string> propValues = new()
            {
                { "/id", "test-id" },
                { "/updatedAt", "2018-12-31T05:00:00.000Z" },
                { "/version", "dGVzdA==" }
            };
            var id = TestData.Movies.GetRandomId();
            var expected = server.GetMovieById(id)!;
            var patchDoc = new PatchOperation[] { new PatchOperation("replace", propName, propValues[propName]) };

            var response = await server.SendPatch($"tables/{table}/{id}", patchDoc).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var stored = server.GetMovieById(id);
            Assert.Equal<IMovie>(expected, stored!);
            Assert.Equal<ITableData>(expected, stored!);
        }

        [Theory, CombinatorialData]
        public async Task CanPatchNonModifiedSystemProperties(
            [CombinatorialValues("movies", "movies_pagesize")] string table,
            [CombinatorialValues("/id", "/updatedAt", "/version")] string propName)
        {
            var id = TestData.Movies.GetRandomId();
            var expected = server.GetMovieById(id)!;
            Dictionary<string, string> propValues = new()
            {
                { "/id", id },
                { "/updatedAt", expected.UpdatedAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture) },
                { "/version", Convert.ToBase64String(expected.Version) }
            };
            var patchDoc = new PatchOperation[] { new PatchOperation("replace", propName, propValues[propName]) };

            var response = await server.SendPatch($"tables/{table}/{id}", patchDoc).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.DeserializeContent<ClientMovie>();
            var stored = server.GetMovieById(id);
            AssertEx.SystemPropertiesSet(stored, startTime);
            AssertEx.SystemPropertiesChanged(expected, stored);
            AssertEx.SystemPropertiesMatch(stored, result);
            Assert.Equal<IMovie>(expected, result!);
            AssertEx.ResponseHasConditionalHeaders(stored, response);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound, "tables/movies/missing-id")]
        [InlineData(HttpStatusCode.NotFound, "tables/movies_pagesize/missing-id")]
        public async Task PatchFailureTests(HttpStatusCode expectedStatusCode, string relativeUri)
        {
            PatchOperation[] patchDoc = new PatchOperation[]
            {
                new PatchOperation("replace", "title", "Home Video"),
                new PatchOperation("replace", "rating", "PG-13")
            };

            var response = await server.SendPatch(relativeUri, patchDoc).ConfigureAwait(false);
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Fact]
        public async Task PatchFailedWithWrongContentType()
        {
            var id = TestData.Movies.GetRandomId();
            var expected = server.GetMovieById(id)!;
            PatchOperation[] patchDoc = new PatchOperation[]
            {
                new PatchOperation("replace", "title", "Home Video"),
                new PatchOperation("replace", "rating", "PG-13")
            };

            var response = await server.SendPatch($"tables/movies/{id}", patchDoc, "application/json+problem").ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
            var stored = server.GetMovieById(id);
            Assert.Equal<IMovie>(expected, stored!);
            Assert.Equal<ITableData>(expected, stored!);
        }

        [Theory]
        [InlineData("duration", 50)]
        [InlineData("duration", 370)]
        [InlineData("duration", null)]
        [InlineData("rating", "M")]
        [InlineData("rating", "PG-13 but not always")]
        [InlineData("title", "a")]
        [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
        [InlineData("title", null)]
        [InlineData("year", 1900)]
        [InlineData("year", 2035)]
        [InlineData("year", null)]
        public async Task PatchValidationFailureTests(string propName, object propValue)
        {
            // Arrange
            string id = TestData.Movies.GetRandomId();
            var patchDoc = new PatchOperation[]
            {
                propValue == null ? new PatchOperation("remove", $"/{propName}") : new PatchOperation("replace", $"/{propName}", propValue)
            };

            var response = await server.SendPatch($"tables/movies/{id}", patchDoc).ConfigureAwait(false);
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

            var patchDoc = new PatchOperation[]
            {
                new PatchOperation("replace", "title", "Test Movie Title"),
                new PatchOperation("replace", "rating", "PG-13")
            };

            Dictionary<string, string> headers = new();
            Utils.AddAuthHeaders(headers, userId);

            var response = await server.SendPatch($"tables/{table}/{id}", patchDoc, headers).ConfigureAwait(false);
            var stored = server.GetMovieById(id);
            if (userId != "success")
            {
                var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
                Assert.Equal(statusCode, response.StatusCode);
                Assert.Equal<IMovie>(original, stored!);
                Assert.Equal<ITableData>(original, stored!);
            }
            else
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var result = response.DeserializeContent<ClientMovie>();
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
        public async Task ConditionalVersionPatchTests(string headerName, string headerValue, HttpStatusCode expectedStatusCode)
        {
            string id = TestData.Movies.GetRandomId();
            var entity = server.GetMovieById(id)!;
            var expected = entity.Clone();
            Dictionary<string, string> headers = new()
            {
                { headerName, headerValue ?? entity.GetETag() }
            };
            var patchDoc = new PatchOperation[]
            {
                new PatchOperation("replace", "title", "Test Movie Title"),
                new PatchOperation("replace", "rating", "PG-13")
            };

            var response = await server.SendPatch($"tables/movies/{id}", patchDoc, headers).ConfigureAwait(false);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            var actual = response.DeserializeContent<ClientMovie>();
            var stored = server.GetMovieById(id)!;
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
        public async Task ConditionalPatchTests(string headerName, int offset, HttpStatusCode expectedStatusCode)
        {
            string id = TestData.Movies.GetRandomId();
            var entity = server.GetMovieById(id)!;
            Dictionary<string, string> headers = new()
            {
                { headerName, entity.UpdatedAt.AddHours(offset).ToString("R") }
            };
            var patchDoc = new PatchOperation[]
            {
                new PatchOperation("replace", "title", "Test Movie Title"),
                new PatchOperation("replace", "rating", "PG-13")
            };
            var expected = server.GetMovieById(id)!;

            var response = await server.SendPatch($"tables/movies/{id}", patchDoc, headers).ConfigureAwait(false);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            var actual = response.DeserializeContent<ClientMovie>();
            var stored = server.GetMovieById(id)!;
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
            await server.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);

            var patchDoc = new PatchOperation[]
            {
                new PatchOperation("replace", "title", "Test Movie Title"),
                new PatchOperation("replace", "rating", "PG-13")
            };

            var response = await server.SendPatch($"tables/{table}/{id}", patchDoc).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        }

        [Theory, CombinatorialData]
        public async Task SoftDeletePatch_CanUndeleteDeletedItem([CombinatorialValues("soft", "soft_logged")] string table)
        {
            var id = TestData.Movies.GetRandomId();
            await server.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);

            var expected = server.GetMovieById(id)!;
            expected.Deleted = false;
            var patchDoc = new PatchOperation[]
            {
                new PatchOperation("replace", "deleted", false)
            };

            var response = await server.SendPatch($"tables/{table}/{id}", patchDoc).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.DeserializeContent<ClientMovie>();
            var stored = server.GetMovieById(id);
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

            var patchDoc = new PatchOperation[]
            {
                new PatchOperation("replace", "title", "Test Movie Title"),
                new PatchOperation("replace", "rating", "PG-13")
            };

            var response = await server.SendPatch($"tables/{table}/{id}", patchDoc).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.DeserializeContent<ClientMovie>();
            var stored = server.GetMovieById(id);
            AssertEx.SystemPropertiesSet(stored, startTime);
            AssertEx.SystemPropertiesChanged(expected, stored);
            AssertEx.SystemPropertiesMatch(stored, result);
            Assert.Equal<IMovie>(expected, result!);
            AssertEx.ResponseHasConditionalHeaders(stored, response);
        }
    }
}

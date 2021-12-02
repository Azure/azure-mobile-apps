﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

using TestData = Datasync.Common.Test.TestData;

namespace Microsoft.Datasync.Integration.Test.Server
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Replace_Tests : BaseTest
    {
        [Theory, CombinatorialData]
        public async Task BasicReplaceTests([CombinatorialValues("movies", "movies_pagesize")] string table)
        {
            string id = GetRandomId();
            var original = MovieServer.GetMovieById(id)!;
            var expected = original.Clone();
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            var response = await MovieServer.SendRequest(HttpMethod.Put, $"tables/{table}/{id}", expected).ConfigureAwait(true);
            var stored = MovieServer.GetMovieById(id)!;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesSet(stored, StartTime);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.ResponseHasConditionalHeaders(stored, response);
        }

        [Theory]
        [InlineData("id", "test-id")]
        [InlineData("duration", 50)]
        [InlineData("duration", 370)]
        [InlineData("rating", "M")]
        [InlineData("rating", "PG-13 but not always")]
        [InlineData("title", "a")]
        [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
        [InlineData("year", 1900)]
        [InlineData("year", 2035)]
        public async Task ReplacementValidationTests(string propName, object propValue)
        {
            string id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;
            var entity = expected.ToDictionary();
            entity[propName] = propValue;

            var response = await MovieServer.SendRequest(HttpMethod.Put, $"tables/movies/{id}", entity).ConfigureAwait(true);
            var stored = MovieServer.GetMovieById(id)!;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            Assert.Equal<ITableData>(expected, stored);
        }

        [Theory]
        [InlineData("id", "test-id")]
        [InlineData("duration", 50)]
        [InlineData("duration", 370)]
        [InlineData("rating", "M")]
        [InlineData("rating", "PG-13 but not always")]
        [InlineData("title", "a")]
        [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
        [InlineData("year", 1900)]
        [InlineData("year", 2035)]
        public async Task ReplacementValidationTestsWithoutLogging(string propName, object propValue)
        {
            string id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;
            var entity = expected.ToDictionary();
            entity[propName] = propValue;

            // Act
            var response = await MovieServer.SendRequest(HttpMethod.Put, $"tables/movies_pagesize/{id}", entity).ConfigureAwait(true);
            var stored = MovieServer.GetMovieById(id);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored!);
            Assert.Equal<ITableData>(expected, stored!);
        }

        [Theory, CombinatorialData]
        public async Task AuthenticatedPatchTests(
            [CombinatorialValues(0, 1, 2, 3, 7, 14, 25)] int index,
            [CombinatorialValues(null, "failed", "success")] string userId,
            [CombinatorialValues("movies_rated", "movies_legal")] string table)
        {
            var id = Utils.GetMovieId(index);
            var original = MovieServer.GetMovieById(id)!;
            var expected = original.Clone();
            expected.Title = "TEST MOVIE TITLE"; // Upper Cased because of the PreCommitHook
            expected.Rating = "PG-13";
            var replacement = MovieServer.GetMovieById(id)!;
            replacement.Title = "Test Movie Title";
            replacement.Rating = "PG-13";

            Dictionary<string, string> headers = new();
            Utils.AddAuthHeaders(headers, userId);

            var response = await MovieServer.SendRequest(HttpMethod.Put, $"tables/{table}/{id}", replacement, headers).ConfigureAwait(true);
            var stored = MovieServer.GetMovieById(id);

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
                AssertEx.SystemPropertiesSet(stored, StartTime);
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
            string id = GetRandomId();
            var entity = MovieServer.GetMovieById(id)!;
            Dictionary<string, string> headers = new()
            {
                { headerName, headerValue ?? entity.GetETag() }
            };
            var expected = MovieServer.GetMovieById(id)!;
            var replacement = expected.Clone();
            replacement.Title = "Test Movie Title";
            replacement.Rating = "PG-13";

            var response = await MovieServer.SendRequest(HttpMethod.Put, $"tables/movies/{id}", replacement, headers).ConfigureAwait(true);
            var stored = MovieServer.GetMovieById(id);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            var actual = response.DeserializeContent<ClientMovie>();

            switch (expectedStatusCode)
            {
                case HttpStatusCode.OK:
                    AssertEx.SystemPropertiesSet(stored, StartTime);
                    AssertEx.SystemPropertiesChanged(expected, stored);
                    AssertEx.SystemPropertiesMatch(stored, actual);
                    Assert.Equal<IMovie>(replacement, actual!);
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
            string id = GetRandomId();
            var entity = MovieServer.GetMovieById(id)!;
            Dictionary<string, string> headers = new()
            {
                { headerName, entity.UpdatedAt.AddHours(offset).ToString("R") }
            };
            var expected = MovieServer.GetMovieById(id)!;
            var replacement = expected.Clone();
            replacement.Title = "Test Movie Title";
            replacement.Rating = "PG-13";

            var response = await MovieServer.SendRequest(HttpMethod.Put, $"tables/movies/{id}", replacement, headers).ConfigureAwait(true);
            var stored = MovieServer.GetMovieById(id);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            var actual = response.DeserializeContent<ClientMovie>();

            switch (expectedStatusCode)
            {
                case HttpStatusCode.OK:
                    AssertEx.SystemPropertiesSet(stored, StartTime);
                    AssertEx.SystemPropertiesChanged(expected, stored);
                    AssertEx.SystemPropertiesMatch(stored, actual);
                    Assert.Equal<IMovie>(replacement, actual!);
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
        [InlineData("tables/movies/not-found", HttpStatusCode.NotFound)]
        [InlineData("tables/movies_pagesize/not-found", HttpStatusCode.NotFound)]
        [InlineData("tables/movies_rated/id-107", HttpStatusCode.NotFound, "X-Auth", "success")]
        [InlineData("tables/movies_legal/id-107", HttpStatusCode.NotFound, "X-Auth", "success")]
        public async Task FailedReplaceTests(string relativeUri, HttpStatusCode expectedStatusCode, string? headerName = null, string? headerValue = null)
        {
            var id = relativeUri.Split('/').Last();
            var expected = MovieServer.GetMovieById(id)!;
            ClientMovie blackPantherMovie = new()
            {
                Id = id,
                BestPictureWinner = true,
                Duration = 134,
                Rating = "PG-13",
                ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
                Title = "Black Panther",
                Year = 2018
            };
            Dictionary<string, string> headers = new();
            if (headerName != null && headerValue != null)
            {
                headers.Add(headerName, headerValue);
            }

            // Act
            var response = await MovieServer.SendRequest(HttpMethod.Put, relativeUri, blackPantherMovie, headers).ConfigureAwait(true);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
            var entity = MovieServer.GetMovieById(id);

            if (entity != null)
            {
                Assert.Equal<IMovie>(expected, entity);
                Assert.Equal<ITableData>(expected, entity);
            }
        }

        [Theory, CombinatorialData]
        public async Task ReplaceSoftNotDeleted_Works([CombinatorialValues("soft", "soft_logged")] string table)
        {

            var id = GetRandomId();
            var original = MovieServer.GetMovieById(id)!;
            var expected = original.Clone();
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            var response = await MovieServer.SendRequest(HttpMethod.Put, $"tables/{table}/{id}", expected).ConfigureAwait(true);
            var stored = MovieServer.GetMovieById(id);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored!);
            AssertEx.SystemPropertiesSet(stored, StartTime);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.ResponseHasConditionalHeaders(stored, response);
        }

        [Theory, CombinatorialData]
        public async Task ReplaceSoftDeleted_ReturnsGone([CombinatorialValues("soft", "soft_logged")] string table)
        {
            var id = GetRandomId();
            await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(true);

            var original = MovieServer.GetMovieById(id)!;
            var expected = MovieServer.GetMovieById(id)!;
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            var response = await MovieServer.SendRequest(HttpMethod.Put, $"tables/{table}/{id}", expected).ConfigureAwait(true);
            var stored = MovieServer.GetMovieById(id);
            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
            Assert.Equal<IMovie>(original, stored!);
            Assert.Equal<ITableData>(original, stored!);
        }
    }
}

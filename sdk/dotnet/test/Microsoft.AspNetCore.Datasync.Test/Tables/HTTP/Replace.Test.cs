// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Datasync.Common.Test;
using Datasync.Common.Test.Extensions;
using Datasync.Common.Test.Models;
using Datasync.Common.Test.TestData;
using Datasync.Webservice;
using Microsoft.AspNetCore.Datasync.Extensions;
using Xunit;

namespace Microsoft.AspNetCore.Datasync.Test.Tables.HTTP
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Replace_Tests
    {
        private readonly DateTimeOffset startTime = DateTimeOffset.Now;

        [Theory, CombinatorialData]
        public async Task BasicReplaceTests([CombinatorialRange(0, Movies.Count)] int index, [CombinatorialValues("movies", "movies_pagesize")] string table)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            string id = Utils.GetMovieId(index);
            var original = repository.GetEntity(id).Clone();
            var expected = repository.GetEntity(id).Clone();
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Act
            var response = await server.SendRequest(HttpMethod.Put, $"tables/{table}/{id}", expected).ConfigureAwait(false);
            var stored = repository.GetEntity(id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesSet(stored, startTime);
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
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            string id = Utils.GetMovieId(20);
            var expected = repository.GetEntity(id).Clone();
            var entity = expected.ToDictionary();
            entity[propName] = propValue;

            // Act
            var response = await server.SendRequest(HttpMethod.Put, $"tables/movies/{id}", entity).ConfigureAwait(false);
            var stored = repository.GetEntity(id);

            // Assert
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
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            string id = Utils.GetMovieId(20);
            var expected = repository.GetEntity(id).Clone();
            var entity = expected.ToDictionary();
            entity[propName] = propValue;

            // Act
            var response = await server.SendRequest(HttpMethod.Put, $"tables/movies_pagesize/{id}", entity).ConfigureAwait(false);
            var stored = repository.GetEntity(id);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            Assert.Equal<ITableData>(expected, stored);
        }

        [Theory, CombinatorialData]
        public async Task AuthenticatedPatchTests(
            [CombinatorialValues(0, 1, 2, 3, 7, 14, 25)] int index,
            [CombinatorialValues(null, "failed", "success")] string userId,
            [CombinatorialValues("movies_rated", "movies_legal")] string table)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var id = Utils.GetMovieId(index);
            var original = repository.GetEntity(id).Clone();
            var expected = repository.GetEntity(id).Clone();
            expected.Title = "TEST MOVIE TITLE"; // Upper Cased because of the PreCommitHook
            expected.Rating = "PG-13";
            var replacement = repository.GetEntity(id).Clone();
            replacement.Title = "Test Movie Title";
            replacement.Rating = "PG-13";

            Dictionary<string, string> headers = new();
            Utils.AddAuthHeaders(headers, userId);

            // Act
            var response = await server.SendRequest(HttpMethod.Put, $"tables/{table}/{id}", replacement, headers).ConfigureAwait(false);
            var stored = repository.GetEntity(id);

            // Assert
            if (userId != "success")
            {
                var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
                Assert.Equal(statusCode, response.StatusCode);
                Assert.Equal<IMovie>(original, stored);
                Assert.Equal<ITableData>(original, stored);
            }
            else
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var result = response.DeserializeContent<ClientMovie>();
                AssertEx.SystemPropertiesSet(stored, startTime);
                AssertEx.SystemPropertiesChanged(expected, stored);
                AssertEx.SystemPropertiesMatch(stored, result);
                Assert.Equal<IMovie>(expected, result);
                AssertEx.ResponseHasConditionalHeaders(stored, response);
            }
        }

        [Theory]
        [InlineData("If-Match", null, HttpStatusCode.OK)]
        [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
        [InlineData("If-None-Match", null, HttpStatusCode.PreconditionFailed)]
        [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.OK)]
        [InlineData("If-Modified-Since", "Fri, 01 Mar 2019 15:00:00 GMT", HttpStatusCode.OK)]
        [InlineData("If-Modified-Since", "Sun, 03 Mar 2019 15:00:00 GMT", HttpStatusCode.PreconditionFailed)]
        [InlineData("If-Unmodified-Since", "Sun, 03 Mar 2019 15:00:00 GMT", HttpStatusCode.OK)]
        [InlineData("If-Unmodified-Since", "Fri, 01 Mar 2019 15:00:00 GMT", HttpStatusCode.PreconditionFailed)]
        public async Task ConditionalPatchTests(string headerName, string headerValue, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            string id = Utils.GetMovieId(42);
            var entity = repository.GetEntity(id);
            entity.UpdatedAt = DateTimeOffset.Parse("Sat, 02 Mar 2019 15:00:00 GMT");
            Dictionary<string, string> headers = new() { { headerName, headerValue ?? entity.GetETag() } };
            var expected = repository.GetEntity(id).Clone();
            var replacement = expected.Clone();
            replacement.Title = "Test Movie Title";
            replacement.Rating = "PG-13";

            // Act
            var response = await server.SendRequest(HttpMethod.Put, $"tables/movies/{id}", replacement, headers).ConfigureAwait(false);
            var stored = repository.GetEntity(id);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
            var actual = response.DeserializeContent<ClientMovie>();

            switch (expectedStatusCode)
            {
                case HttpStatusCode.OK:
                    AssertEx.SystemPropertiesSet(stored, startTime);
                    AssertEx.SystemPropertiesChanged(expected, stored);
                    AssertEx.SystemPropertiesMatch(stored, actual);
                    Assert.Equal<IMovie>(replacement, actual);
                    AssertEx.ResponseHasConditionalHeaders(stored, response);
                    break;
                case HttpStatusCode.PreconditionFailed:
                    Assert.Equal<IMovie>(expected, actual);
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
        public async Task FailedReplaceTests(string relativeUri, HttpStatusCode expectedStatusCode, string headerName = null, string headerValue = null)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var id = relativeUri.Split('/').Last();
            var expected = repository.GetEntity(id)?.Clone();
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
            if (headerName != null) { headers.Add(headerName, headerValue); }

            // Act
            var response = await server.SendRequest(HttpMethod.Put, relativeUri, blackPantherMovie, headers).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
            var entity = repository.GetEntity(id);

            if (entity != null)
            {
                Assert.Equal<IMovie>(expected, entity);
                Assert.Equal<ITableData>(expected, entity);
            }
        }

        [Theory, CombinatorialData]
        public async Task ReplaceSoftNotDeleted_Works([CombinatorialValues("soft", "soft_logged")] string table)
        {
            // Arrange
            const int index = 24;
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<SoftMovie>();
            string id = Utils.GetMovieId(index);
            var original = repository.GetEntity(id).Clone();
            var expected = repository.GetEntity(id).Clone();
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Act
            var response = await server.SendRequest(HttpMethod.Put, $"tables/{table}/{id}", expected).ConfigureAwait(false);
            var stored = repository.GetEntity(id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesSet(stored, startTime);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.ResponseHasConditionalHeaders(stored, response);
        }

        [Theory, CombinatorialData]
        public async Task ReplaceSoftDeleted_ReturnsGone([CombinatorialValues("soft", "soft_logged")] string table)
        {
            // Arrange
            const int index = 25;
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<SoftMovie>();
            string id = Utils.GetMovieId(index);
            var original = repository.GetEntity(id).Clone();
            var expected = repository.GetEntity(id).Clone();
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Act
            var response = await server.SendRequest(HttpMethod.Put, $"tables/{table}/{id}", expected).ConfigureAwait(false);
            var stored = repository.GetEntity(id);

            // Assert
            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
            Assert.Equal<IMovie>(original, stored);
            Assert.Equal<ITableData>(original, stored);
        }
    }
}

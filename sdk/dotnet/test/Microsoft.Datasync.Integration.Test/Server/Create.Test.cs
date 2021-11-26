// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
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
    public class Create_Tests
    {
        /// <summary>
        /// A connection to the test service.
        /// </summary>
        private readonly TestServer server = MovieApiServer.CreateTestServer();

        /// <summary>
        /// A basic movie without any adornment that does not exist in the movie data. Tests must clone this
        /// object and then adjust.
        /// </summary>
        private readonly ClientMovie blackPantherMovie = new()
        {
            BestPictureWinner = true,
            Duration = 134,
            Rating = "PG-13",
            ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
            Title = "Black Panther",
            Year = 2018
        };

        /// <summary>
        /// The time the test started
        /// </summary>
        private readonly DateTimeOffset startTime = DateTimeOffset.Now;

        [Theory, CombinatorialData]
        public async Task BasicCreateTests([CombinatorialValues("movies", "movies_pagesize")] string table, bool hasId)
        {
            var movieToAdd = blackPantherMovie.Clone();
            if (hasId)
            {
                movieToAdd.Id = Guid.NewGuid().ToString("N");
            }

            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, $"tables/{table}", movieToAdd).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = response.DeserializeContent<ClientMovie>();
            Assert.NotNull(result);
            Assert.True(Guid.TryParse(result!.Id, out _));
            if (hasId)
            {
                Assert.Equal(movieToAdd.Id, result!.Id);
            }
            Assert.Equal(TestData.Movies.Count + 1, server.GetMovieCount());

            var entity = server.GetMovieById(result!.Id);
            Assert.NotNull(entity);

            AssertEx.SystemPropertiesSet(entity, startTime);
            AssertEx.SystemPropertiesMatch(entity, result);
            Assert.Equal<IMovie>(movieToAdd, result!);
            Assert.Equal<IMovie>(movieToAdd, entity!);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
            AssertEx.ResponseHasHeader(response, "Location", $"https://localhost/tables/{table}/{result.Id}");
        }

        [Theory, CombinatorialData]
        public async Task CreateOverwriteSystemProperties(bool useUpdatedAt, bool useVersion)
        {
            var movieToAdd = blackPantherMovie.Clone();
            movieToAdd.Id = Guid.NewGuid().ToString("N");
            if (useUpdatedAt) { movieToAdd.UpdatedAt = DateTimeOffset.Parse("2018-12-31T01:01:01.000Z"); }
            if (useVersion) { movieToAdd.Version = Convert.ToBase64String(Guid.NewGuid().ToByteArray()); }

            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, "tables/movies", movieToAdd).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = response.DeserializeContent<ClientMovie>();
            Assert.NotNull(result);
            Assert.Equal(movieToAdd.Id, result!.Id);
            Assert.Equal(TestData.Movies.Count + 1, server.GetMovieCount());

            var entity = server.GetMovieById(result.Id);
            Assert.NotNull(entity);

            AssertEx.SystemPropertiesSet(entity, startTime);
            AssertEx.SystemPropertiesMatch(entity, result);
            if (useUpdatedAt)
                Assert.NotEqual(movieToAdd.UpdatedAt, result.UpdatedAt);
            if (useVersion)
                Assert.NotEqual(movieToAdd.Version, result.Version);
            Assert.Equal<IMovie>(movieToAdd, result);
            Assert.Equal<IMovie>(movieToAdd, entity!);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
            AssertEx.ResponseHasHeader(response, "Location", $"https://localhost/tables/movies/{movieToAdd.Id}");
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
        public async Task CreateValidationProducesError(string propName, object propValue)
        {
            Dictionary<string, object> movieToAdd = new()
            {
                { "id", "test-id" },
                { "updatedAt", DateTimeOffset.Parse("2018-12-31T01:01:01.000Z") },
                { "version", Convert.ToBase64String(Guid.NewGuid().ToByteArray()) },
                { "bestPictureWinner", false },
                { "duration", 120 },
                { "rating", "G" },
                { "releaseDate", DateTimeOffset.Parse("2018-12-30T05:30:00.000Z") },
                { "title", "Home Video" },
                { "year", 2021 }
            };
            if (propValue == null)
                movieToAdd.Remove(propName);
            else
                movieToAdd[propName] = propValue;

            var response = await server.SendRequest(HttpMethod.Post, "tables/movies", movieToAdd, null).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(TestData.Movies.Count, server.GetMovieCount());
            Assert.Null(server.GetMovieById("test-id"));
        }

        [Fact]
        public async Task CreateExisting_ReturnsConflict()
        {
            var movieToAdd = blackPantherMovie.Clone();
            movieToAdd.Id = TestData.Movies.GetRandomId();
            var expectedMovie = server.GetMovieById(movieToAdd.Id)!;

            // Act
            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, "tables/movies", movieToAdd).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal(TestData.Movies.Count, server.GetMovieCount());

            var result = response.DeserializeContent<ClientMovie>();
            Assert.NotNull(result);
            var entity = server.GetMovieById(movieToAdd.Id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity, startTime);
            AssertEx.SystemPropertiesMatch(entity, result);
            Assert.Equal<IMovie>(expectedMovie, result!);
            Assert.Equal<IMovie>(expectedMovie, entity!);
            Assert.Equal<ITableData>(expectedMovie, entity!);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
        }

        [Theory, CombinatorialData]
        public async Task AuthenticatedCreateTests(
            [CombinatorialValues(null, "failed", "success")] string userId,
            [CombinatorialValues("movies_rated", "movies_legal")] string table)
        {
            var movieToAdd = blackPantherMovie.Clone();
            var expectedMovie = movieToAdd.Clone();
            expectedMovie.Title = movieToAdd.Title.ToUpper();

            Dictionary<string, string> headers = new();
            Utils.AddAuthHeaders(headers, userId);

            // Act
            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, $"tables/{table}", movieToAdd, headers).ConfigureAwait(false);

            // Assert
            if (userId != "success")
            {
                var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
                Assert.Equal(statusCode, response.StatusCode);
                Assert.Equal(TestData.Movies.Count, server.GetMovieCount());
            }
            else
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                var result = response.DeserializeContent<ClientMovie>();
                Assert.NotNull(result);
                Assert.True(Guid.TryParse(result!.Id, out _));
                Assert.Equal(TestData.Movies.Count + 1, server.GetMovieCount());
                AssertEx.ResponseHasHeader(response, "Location", $"https://localhost/tables/{table}/{result.Id}");

                var entity = server.GetMovieById(result.Id);
                Assert.NotNull(entity);

                AssertEx.SystemPropertiesSet(entity, startTime);
                AssertEx.SystemPropertiesMatch(entity, result);
                Assert.Equal<IMovie>(expectedMovie, result);
                Assert.Equal<IMovie>(expectedMovie, entity!);
                AssertEx.ResponseHasConditionalHeaders(entity, response);
            }
        }

        [Theory, CombinatorialData]
        public async Task SoftDeleteCreateTests([CombinatorialValues("soft", "soft_logged")] string table)
        {
            var movieToAdd = blackPantherMovie.Clone();
            movieToAdd.Id = TestData.Movies.GetRandomId();
            await server.SoftDeleteMoviesAsync(x => x.Id == movieToAdd.Id).ConfigureAwait(false);
            var expectedMovie = server.GetMovieById(movieToAdd.Id)!;

            // Act
            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, $"tables/{table}", movieToAdd).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal(TestData.Movies.Count, server.GetMovieCount());

            var result = response.DeserializeContent<ClientMovie>();
            Assert.NotNull(result);
            var entity = server.GetMovieById(movieToAdd.Id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity, startTime);
            AssertEx.SystemPropertiesMatch(entity, result);
            Assert.Equal<IMovie>(expectedMovie, result!);
            Assert.Equal<IMovie>(expectedMovie, entity!);
            Assert.Equal<ITableData>(expectedMovie, entity!);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
        }

        [Theory, CombinatorialData]
        public async Task BasicV2CreateTests(bool hasId)
        {
            var movieToAdd = blackPantherMovie.Clone();
            if (hasId)
            {
                movieToAdd.Id = Guid.NewGuid().ToString("N");
            }
            var headers = new Dictionary<string, string>
            {
                { "ZUMO-API-VERSION", "2.0.0" }
            };

            // Act
            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, "tables/movies", movieToAdd, headers).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = response.DeserializeContent<ClientMovie>();
            Assert.NotNull(result);
            Assert.True(Guid.TryParse(result!.Id, out _));
            if (hasId)
            {
                Assert.Equal(movieToAdd.Id, result.Id);
            }
            Assert.Equal(TestData.Movies.Count + 1, server.GetMovieCount());

            var entity = server.GetMovieById(result.Id);
            Assert.NotNull(entity);

            AssertEx.SystemPropertiesSet(entity, startTime);
            AssertEx.SystemPropertiesMatch(entity, result);
            Assert.Equal<IMovie>(movieToAdd, result!);
            Assert.Equal<IMovie>(movieToAdd, entity!);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
            AssertEx.ResponseHasHeader(response, "Location", $"https://localhost/tables/movies/{result.Id}");
        }
    }
}

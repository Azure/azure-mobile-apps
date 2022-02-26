// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using TestData = Datasync.Common.Test.TestData;

#pragma warning disable RCS1090 // Add call to 'ConfigureAwait' (or vice versa).

namespace Microsoft.Datasync.Integration.Test.Server
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    [Collection("Integration")]
    public class Create_Tests : BaseTest
    {
        public Create_Tests(ITestOutputHelper helper) : base(helper) { }

        [Theory, CombinatorialData]
        public async Task BasicCreateTests([CombinatorialValues("movies", "movies_pagesize")] string table, bool hasId)
        {
            var movieToAdd = GetSampleMovie<ClientMovie>();
            if (hasId)
            {
                movieToAdd.Id = Guid.NewGuid().ToString("N");
            }

            var response = await MovieServer.SendRequest<ClientMovie>(HttpMethod.Post, $"tables/{table}", movieToAdd);
            await AssertResponseWithLoggingAsync(HttpStatusCode.Created, response);

            var result = response.DeserializeContent<ClientMovie>();
            Assert.NotNull(result);
            Assert.True(Guid.TryParse(result!.Id, out _));
            if (hasId)
            {
                Assert.Equal(movieToAdd.Id, result!.Id);
            }
            Assert.Equal(TestData.Movies.Count + 1, MovieServer.GetMovieCount());

            var entity = MovieServer.GetMovieById(result!.Id);
            Assert.NotNull(entity);

            AssertEx.SystemPropertiesSet(entity, StartTime);
            AssertEx.SystemPropertiesMatch(entity, result);
            Assert.Equal<IMovie>(movieToAdd, result!);
            Assert.Equal<IMovie>(movieToAdd, entity!);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
            AssertEx.HasHeader(response, "Location", $"https://localhost/tables/{table}/{result.Id}");
        }

        [Theory, CombinatorialData]
        public async Task CreateOverwriteSystemProperties(bool useUpdatedAt, bool useVersion)
        {
            var movieToAdd = GetSampleMovie<ClientMovie>();
            movieToAdd.Id = Guid.NewGuid().ToString("N");
            if (useUpdatedAt) { movieToAdd.UpdatedAt = DateTimeOffset.Parse("2018-12-31T01:01:01.000Z"); }
            if (useVersion) { movieToAdd.Version = Convert.ToBase64String(Guid.NewGuid().ToByteArray()); }

            var response = await MovieServer.SendRequest<ClientMovie>(HttpMethod.Post, "tables/movies", movieToAdd);
            await AssertResponseWithLoggingAsync(HttpStatusCode.Created, response);

            var result = response.DeserializeContent<ClientMovie>();
            Assert.NotNull(result);
            Assert.Equal(movieToAdd.Id, result!.Id);
            Assert.Equal(TestData.Movies.Count + 1, MovieServer.GetMovieCount());

            var entity = MovieServer.GetMovieById(result.Id);
            Assert.NotNull(entity);

            AssertEx.SystemPropertiesSet(entity, StartTime);
            AssertEx.SystemPropertiesMatch(entity, result);
            if (useUpdatedAt)
                Assert.NotEqual(movieToAdd.UpdatedAt, result.UpdatedAt);
            if (useVersion)
                Assert.NotEqual(movieToAdd.Version, result.Version);
            Assert.Equal<IMovie>(movieToAdd, result);
            Assert.Equal<IMovie>(movieToAdd, entity!);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
            AssertEx.HasHeader(response, "Location", $"https://localhost/tables/movies/{movieToAdd.Id}");
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

            var response = await MovieServer.SendRequest(HttpMethod.Post, "tables/movies", movieToAdd, null);
            await AssertResponseWithLoggingAsync(HttpStatusCode.BadRequest, response);
            Assert.Equal(TestData.Movies.Count, MovieServer.GetMovieCount());
            Assert.Null(MovieServer.GetMovieById("test-id"));
        }

        [Fact]
        public async Task CreateExisting_ReturnsConflict()
        {
            var movieToAdd = GetSampleMovie<ClientMovie>();
            movieToAdd.Id = GetRandomId();
            var expectedMovie = MovieServer.GetMovieById(movieToAdd.Id)!;

            var response = await MovieServer.SendRequest<ClientMovie>(HttpMethod.Post, "tables/movies", movieToAdd);
            await AssertResponseWithLoggingAsync(HttpStatusCode.Conflict, response);
            Assert.Equal(TestData.Movies.Count, MovieServer.GetMovieCount());

            var result = response.DeserializeContent<ClientMovie>()!;
            var entity = MovieServer.GetMovieById(movieToAdd.Id)!;
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
            var movieToAdd = GetSampleMovie<ClientMovie>();
            var expectedMovie = movieToAdd.Clone();
            expectedMovie.Title = movieToAdd.Title.ToUpper();

            Dictionary<string, string> headers = new();
            Utils.AddAuthHeaders(headers, userId);

            // Act
            var response = await MovieServer.SendRequest<ClientMovie>(HttpMethod.Post, $"tables/{table}", movieToAdd, headers);

            // Assert
            if (userId != "success")
            {
                var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
                await AssertResponseWithLoggingAsync(statusCode, response);
                Assert.Equal(TestData.Movies.Count, MovieServer.GetMovieCount());
            }
            else
            {
                await AssertResponseWithLoggingAsync(HttpStatusCode.Created, response);

                var result = response.DeserializeContent<ClientMovie>();
                Assert.NotNull(result);
                Assert.True(Guid.TryParse(result!.Id, out _));
                Assert.Equal(TestData.Movies.Count + 1, MovieServer.GetMovieCount());
                AssertEx.HasHeader(response, "Location", $"https://localhost/tables/{table}/{result.Id}");

                var entity = MovieServer.GetMovieById(result.Id);
                Assert.NotNull(entity);

                AssertEx.SystemPropertiesSet(entity, StartTime);
                AssertEx.SystemPropertiesMatch(entity, result);
                Assert.Equal<IMovie>(expectedMovie, result);
                Assert.Equal<IMovie>(expectedMovie, entity!);
                AssertEx.ResponseHasConditionalHeaders(entity, response);
            }
        }

        [Theory, CombinatorialData]
        public async Task SoftDeleteCreateTests([CombinatorialValues("soft", "soft_logged")] string table)
        {
            var movieToAdd = GetSampleMovie<ClientMovie>();
            movieToAdd.Id = GetRandomId();
            await MovieServer.SoftDeleteMoviesAsync(x => x.Id == movieToAdd.Id);
            var expectedMovie = MovieServer.GetMovieById(movieToAdd.Id)!;

            var response = await MovieServer.SendRequest<ClientMovie>(HttpMethod.Post, $"tables/{table}", movieToAdd);
            await AssertResponseWithLoggingAsync(HttpStatusCode.Conflict, response);
            Assert.Equal(TestData.Movies.Count, MovieServer.GetMovieCount());

            var result = response.DeserializeContent<ClientMovie>()!;
            var entity = MovieServer.GetMovieById(movieToAdd.Id)!;
            AssertEx.SystemPropertiesMatch(entity, result);
            Assert.Equal<IMovie>(expectedMovie, result!);
            Assert.Equal<IMovie>(expectedMovie, entity!);
            Assert.Equal<ITableData>(expectedMovie, entity!);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
        }

        [Theory, CombinatorialData]
        public async Task BasicV2CreateTests(bool hasId)
        {
            var movieToAdd = GetSampleMovie<ClientMovie>();
            if (hasId)
            {
                movieToAdd.Id = Guid.NewGuid().ToString("N");
            }
            var headers = new Dictionary<string, string>
            {
                { "ZUMO-API-VERSION", "2.0.0" }
            };

            // Act
            var response = await MovieServer.SendRequest<ClientMovie>(HttpMethod.Post, "tables/movies", movieToAdd, headers);

            // Assert
            await AssertResponseWithLoggingAsync(HttpStatusCode.Created, response);

            var result = response.DeserializeContent<ClientMovie>();
            Assert.NotNull(result);
            Assert.True(Guid.TryParse(result!.Id, out _));
            if (hasId)
            {
                Assert.Equal(movieToAdd.Id, result.Id);
            }
            Assert.Equal(TestData.Movies.Count + 1, MovieServer.GetMovieCount());

            var entity = MovieServer.GetMovieById(result.Id);
            Assert.NotNull(entity);

            AssertEx.SystemPropertiesSet(entity, StartTime);
            AssertEx.SystemPropertiesMatch(entity, result);
            Assert.Equal<IMovie>(movieToAdd, result!);
            Assert.Equal<IMovie>(movieToAdd, entity!);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
            AssertEx.HasHeader(response, "Location", $"https://localhost/tables/movies/{result.Id}");
        }
    }
}

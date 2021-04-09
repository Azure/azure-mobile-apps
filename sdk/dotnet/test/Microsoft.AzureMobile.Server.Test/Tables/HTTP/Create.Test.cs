// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AzureMobile.Common.Test;
using Microsoft.AzureMobile.Common.Test.Extensions;
using Microsoft.AzureMobile.Common.Test.Models;
using Microsoft.AzureMobile.Common.Test.TestData;
using Microsoft.AzureMobile.WebService.Test;
using Xunit;

namespace Microsoft.AzureMobile.Server.Test.Tables.HTTP
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Create_Tests
    {
        #region Test Artifacts
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
        #endregion

        [Theory, CombinatorialData]
        public async Task BasicCreateTests([CombinatorialValues("movies", "movies_pagesize")] string table, bool hasId)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var expectedCount = repository.Entities.Count;
            var movieToAdd = blackPantherMovie.Clone();
            if (hasId) { movieToAdd.Id = Guid.NewGuid().ToString("N"); }

            // Act
            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, $"tables/{table}", movieToAdd).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = response.DeserializeContent<ClientMovie>();
            Assert.True(Guid.TryParse(result.Id, out _));
            if (hasId)
            {
                Assert.Equal(movieToAdd.Id, result.Id);
            }
            Assert.Equal(expectedCount + 1, repository.Entities.Count);

            var entity = repository.GetEntity(result.Id);
            Assert.NotNull(entity);

            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, result);
            Assert.Equal<IMovie>(movieToAdd, result);
            Assert.Equal<IMovie>(movieToAdd, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
            AssertEx.ResponseHasHeader(response, "Location", $"https://localhost/tables/{table}/{result.Id}");
        }

        [Theory, CombinatorialData]
        public async Task CreateOverwriteSystemProperties(bool useUpdatedAt, bool useVersion)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var expectedCount = repository.Entities.Count;
            var movieToAdd = blackPantherMovie.Clone();
            movieToAdd.Id = Guid.NewGuid().ToString("N");
            if (useUpdatedAt) { movieToAdd.UpdatedAt = DateTimeOffset.Parse("2018-12-31T01:01:01.000Z");  }
            if (useVersion) { movieToAdd.Version = Convert.ToBase64String(Guid.NewGuid().ToByteArray()); }

            // Act
            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, "tables/movies", movieToAdd).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = response.DeserializeContent<ClientMovie>();
            Assert.Equal(movieToAdd.Id, result.Id);
            Assert.Equal(expectedCount + 1, repository.Entities.Count);

            var entity = repository.GetEntity(result.Id);
            Assert.NotNull(entity);

            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, result);
            if (useUpdatedAt)
                Assert.NotEqual(movieToAdd.UpdatedAt, result.UpdatedAt);
            if (useVersion)
                Assert.NotEqual(movieToAdd.Version, result.Version);
            Assert.Equal<IMovie>(movieToAdd, result);
            Assert.Equal<IMovie>(movieToAdd, entity);
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
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var expectedCount = repository.Entities.Count;
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

            // Act
            var response = await server.SendRequest(HttpMethod.Post, "tables/movies", movieToAdd, null).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(expectedCount, repository.Entities.Count);
            Assert.Null(repository.GetEntity("test-id"));
        }

        [Theory, CombinatorialData]
        public async Task CreateExisting_ReturnsConflict([CombinatorialRange(0, Movies.Count)] int index)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var expectedCount = repository.Entities.Count;
            var movieToAdd = blackPantherMovie.Clone();
            movieToAdd.Id = Utils.GetMovieId(index);
            var expectedMovie = repository.GetEntity(movieToAdd.Id).Clone();

            // Act
            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, "tables/movies", movieToAdd).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal(expectedCount, repository.Entities.Count);

            var result = response.DeserializeContent<ClientMovie>();
            var entity = repository.GetEntity(movieToAdd.Id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, result);
            Assert.Equal<IMovie>(expectedMovie, result);
            Assert.Equal<IMovie>(expectedMovie, entity);
            Assert.Equal<ITableData>(expectedMovie, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
        }

        [Theory, CombinatorialData]
        public async Task AuthenticatedCreateTests(
            [CombinatorialValues(null, "failed", "success")] string userId,
            [CombinatorialValues("movies_rated", "movies_legal")] string table)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var expectedCount = repository.Entities.Count;
            var movieToAdd = blackPantherMovie.Clone();

            // When stored, the PreCommitHook upper-cases the title.
            var expectedMovie = movieToAdd.Clone();
            expectedMovie.Title = movieToAdd.Title.ToUpper();

            Dictionary<string, string> headers = new();
            if (userId != null)
            {
                headers.Add("X-Auth", userId);
            }

            // Act
            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, $"tables/{table}", movieToAdd, headers).ConfigureAwait(false);

            // Assert
            if (userId != "success")
            {
                var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
                Assert.Equal(statusCode, response.StatusCode);
                Assert.Equal(expectedCount, repository.Entities.Count);
            }
            else
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                var result = response.DeserializeContent<ClientMovie>();
                Assert.True(Guid.TryParse(result.Id, out _));
                Assert.Equal(expectedCount + 1, repository.Entities.Count);
                AssertEx.ResponseHasHeader(response, "Location", $"https://localhost/tables/{table}/{result.Id}");

                var entity = repository.GetEntity(result.Id);
                Assert.NotNull(entity);

                AssertEx.SystemPropertiesSet(entity);
                AssertEx.SystemPropertiesMatch(entity, result);
                Assert.Equal<IMovie>(expectedMovie, result);
                Assert.Equal<IMovie>(expectedMovie, entity);
                AssertEx.ResponseHasConditionalHeaders(entity, response);
            }
        }

        // Even though half the movies are deleted, they should all still produce conflicts
        [Theory, CombinatorialData]
        public async Task SoftDeleteCreateTests(
            [CombinatorialRange(0, Movies.Count)] int index,
            [CombinatorialValues("soft", "soft_logged")] string table)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<SoftMovie>();
            var expectedCount = repository.Entities.Count;
            var movieToAdd = blackPantherMovie.Clone();
            movieToAdd.Id = Utils.GetMovieId(index);
            var expectedMovie = repository.GetEntity(movieToAdd.Id).Clone();

            // Act
            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, $"tables/{table}", movieToAdd).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal(expectedCount, repository.Entities.Count);

            var result = response.DeserializeContent<ClientMovie>();
            var entity = repository.GetEntity(movieToAdd.Id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, result);
            Assert.Equal<IMovie>(expectedMovie, result);
            Assert.Equal<IMovie>(expectedMovie, entity);
            Assert.Equal<ITableData>(expectedMovie, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
        }

        [Theory, CombinatorialData]
        public async Task BasicV2CreateTests(bool hasId)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var expectedCount = repository.Entities.Count;
            var movieToAdd = blackPantherMovie.Clone();
            if (hasId) { movieToAdd.Id = Guid.NewGuid().ToString("N"); }
            var headers = new Dictionary<string, string> { { "ZUMO-API-VERSION", "2.0.0" } };

            // Act
            var response = await server.SendRequest<ClientMovie>(HttpMethod.Post, "tables/movies", movieToAdd, headers).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = response.DeserializeContent<ClientMovie>();
            Assert.True(Guid.TryParse(result.Id, out _));
            if (hasId)
            {
                Assert.Equal(movieToAdd.Id, result.Id);
            }
            Assert.Equal(expectedCount + 1, repository.Entities.Count);

            var entity = repository.GetEntity(result.Id);
            Assert.NotNull(entity);

            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, result);
            Assert.Equal<IMovie>(movieToAdd, result);
            Assert.Equal<IMovie>(movieToAdd, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, response);
            AssertEx.ResponseHasHeader(response, "Location", $"https://localhost/tables/movies/{result.Id}");
        }
    }
}

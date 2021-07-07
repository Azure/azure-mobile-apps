// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Datasync.Common.Test.TestData;
using Microsoft.AspNetCore.Datasync;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table
{
    [ExcludeFromCodeCoverage]
    public class DatasyncTable_Tests : BaseTest
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

        #region CreateItemAsync
        [Theory, CombinatorialData]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemsAsync_Basic(bool hasId)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var repository = GetRepository<InMemoryMovie>();
            var expectedCount = repository.Entities.Count;
            var movieToAdd = blackPantherMovie.Clone();
            if (hasId) { movieToAdd.Id = Guid.NewGuid().ToString("N"); }

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var response = await table.CreateItemAsync(movieToAdd).ConfigureAwait(false);

            // Assert
            Assert.Equal(201, response.StatusCode);
            Assert.True(response.HasValue);
            var result = response.Value;
            Assert.True(Guid.TryParse(result.Id, out _));
            if (hasId)
            {
                Assert.Equal(movieToAdd.Id, result.Id);
            }
            Assert.Equal<IMovie>(movieToAdd, result);
            Assert.Equal(expectedCount + 1, repository.Entities.Count);

            var entity = repository.GetEntity(result.Id);
            Assert.NotNull(entity);
            Assert.Equal<IMovie>(movieToAdd, entity);
            AssertEx.Contains("Location", new Uri(Endpoint, $"tables/movies/{result.Id}").ToString(), response.Headers);
            AssertEx.Contains("ETag", $"\"{result.Version}\"", response.Headers);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateAsync_OverwriteSystemProperties(bool useUpdatedAt, bool useVersion)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var repository = GetRepository<InMemoryMovie>();
            var expectedCount = repository.Entities.Count;
            var movieToAdd = blackPantherMovie.Clone();
            movieToAdd.Id = Guid.NewGuid().ToString("N");
            if (useUpdatedAt) { movieToAdd.UpdatedAt = DateTimeOffset.Parse("2018-12-31T01:01:01.000Z"); }
            if (useVersion) { movieToAdd.Version = Convert.ToBase64String(Guid.NewGuid().ToByteArray()); }

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var response = await table.CreateItemAsync(movieToAdd).ConfigureAwait(false);

            // Assert
            Assert.Equal(201, response.StatusCode);
            Assert.True(response.HasValue);
            var result = response.Value;
            Assert.Equal(movieToAdd.Id, result.Id);
            Assert.Equal<IMovie>(movieToAdd, result);
            Assert.Equal(expectedCount + 1, repository.Entities.Count);

            var entity = repository.GetEntity(result.Id);
            Assert.NotNull(entity);
            if (useUpdatedAt)
                Assert.NotEqual(movieToAdd.UpdatedAt, result.UpdatedAt);
            if (useVersion)
                Assert.NotEqual(movieToAdd.Version, result.Version);
            Assert.Equal<IMovie>(movieToAdd, entity);
            AssertEx.Contains("Location", new Uri(Endpoint, $"tables/movies/{result.Id}").ToString(), response.Headers);
            AssertEx.Contains("ETag", $"\"{result.Version}\"", response.Headers);
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
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_ValidationTest(string propName, object propValue)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var repository = GetRepository<InMemoryMovie>();
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
            var table = client.GetTable<Dictionary<string, object>>("movies");
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.CreateItemAsync(movieToAdd)).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, exception.Response.StatusCode);
            Assert.Equal(expectedCount, repository.Entities.Count);
            Assert.Null(repository.GetEntity("test-id"));
        }

        [Theory, CombinatorialData]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_Conflict([CombinatorialRange(0, Movies.Count)] int index)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var repository = GetRepository<InMemoryMovie>();
            var expectedCount = repository.Entities.Count;
            var movieToAdd = blackPantherMovie.Clone();
            movieToAdd.Id = GetMovieId(index);
            var expectedMovie = repository.GetEntity(movieToAdd.Id).Clone();

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.CreateItemAsync(movieToAdd)).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, exception.Response.StatusCode);
            Assert.Equal(expectedCount, repository.Entities.Count);
            var entity = repository.GetEntity(movieToAdd.Id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, exception.ServerItem);
            Assert.Equal<IMovie>(expectedMovie, exception.ServerItem);
            Assert.Equal<IMovie>(expectedMovie, entity);
            Assert.Equal<ITableData>(expectedMovie, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
        }
        #endregion

        #region DeleteItemAsync
        [Theory, CombinatorialData]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Basic([CombinatorialRange(0, Movies.Count)] int index)
        {
            // Arrange
            var client = CreateClientForTestServer();
            var repository = GetRepository<InMemoryMovie>();
            var entityCount = repository.Entities.Count;
            var id = GetMovieId(index);

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var response = await table.DeleteItemAsync(id).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(entityCount - 1, repository.Entities.Count);
            Assert.Null(repository.GetEntity(id));
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_NotFound()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var repository = GetRepository<InMemoryMovie>();
            var entityCount = repository.Entities.Count;
            const string id = "not-found";

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(id)).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, exception.Response.StatusCode);
            Assert.Equal(entityCount, repository.Entities.Count);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConditionalSuccess()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var repository = GetRepository<InMemoryMovie>();
            var entityCount = repository.Entities.Count;
            const string id = "id-107";
            var expected = repository.GetEntity(id);
            var etag = Convert.ToBase64String(expected.Version);

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var response = await table.DeleteItemAsync(id, IfMatch.Version(etag)).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(entityCount - 1, repository.Entities.Count);
            Assert.Null(repository.GetEntity(id));
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConditionalFailure()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var repository = GetRepository<InMemoryMovie>();
            var entityCount = repository.Entities.Count;
            const string id = "id-107";
            var expected = repository.GetEntity(id).Clone();
            const string etag = "dGVzdA==";

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.DeleteItemAsync(id, IfMatch.Version(etag))).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.PreconditionFailed, exception.Response.StatusCode);
            Assert.Equal(entityCount, repository.Entities.Count);
            var entity = repository.GetEntity(id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, exception.ServerItem);
            Assert.Equal<IMovie>(expected, exception.ServerItem);
            Assert.Equal<IMovie>(expected, entity);
            Assert.Equal<ITableData>(expected, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_SoftDelete()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var repository = GetRepository<SoftMovie>();
            var entityCount = repository.Entities.Count;
            const string id = "id-024";

            // Act
            var table = client.GetTable<ClientMovie>("soft");
            var response = await table.DeleteItemAsync(id).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(entityCount, repository.Entities.Count);
            var entity = repository.GetEntity(id);
            Assert.True(entity.Deleted);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_GoneWhenDeleted()
        {
            // Arrange
            var client = CreateClientForTestServer();
            var repository = GetRepository<SoftMovie>();
            var entityCount = repository.Entities.Count;
            const string id = "id-025";

            // Act
            var table = client.GetTable<ClientMovie>("soft");
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(id)).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Gone, exception.Response.StatusCode);
            Assert.Equal(entityCount, repository.Entities.Count);
            var entity = repository.GetEntity(id);
            Assert.True(entity.Deleted);
        }
        #endregion
    }
}

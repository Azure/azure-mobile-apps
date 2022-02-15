// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.Datasync.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTableOfT
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    [Collection("Integration")]
    public class RemoteTable_Insert_Tests : BaseTest
    {
        private readonly DatasyncClient client;
        private readonly IRemoteTable<ClientMovie> table;
        private readonly List<TableModifiedEventArgs> modifications = new();

        public RemoteTable_Insert_Tests()
        {
            client = GetMovieClient();
            table = client.GetRemoteTable<ClientMovie>("movies");
            table.TableModified += (_, e) => modifications.Add(e);
        }

        #region Helpers
        /// <summary>
        /// Checks that the TableModified event handler was fired and it has the correct
        /// information in it.
        /// </summary>
        /// <param name="item">The expected insertion.</param>
        private void AssertEventHandlerCalled(ClientMovie item, string relativeUri = "tables/movies/")
        {
            Assert.Single(modifications);
            Assert.Equal(TableModifiedEventArgs.TableOperation.Create, modifications[0].Operation);
            Assert.Equal(new Uri(Endpoint, relativeUri), modifications[0].TableEndpoint);
            Assert.Equal(item.Id, modifications[0].Id);
            Assert.IsAssignableFrom<ClientMovie>(modifications[0].Entity);
            Assert.Equal<IMovie>(item, (ClientMovie)modifications[0].Entity);
        }
        #endregion

        [Theory, CombinatorialData]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_Basic(bool hasId)
        {
            var movieToAdd = GetSampleMovie<ClientMovie>();
            if (hasId)
            {
                movieToAdd.Id = Guid.NewGuid().ToString("N");
            }

            // Act
            var response = await table.InsertItemAsync(movieToAdd).ConfigureAwait(false);

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
            Assert.Equal(MovieCount + 1, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(result.Id)!;
            Assert.Equal<IMovie>(movieToAdd, entity);
            AssertEx.Contains("Location", new Uri(Endpoint, $"tables/movies/{result.Id}").ToString(), response.Headers);
            AssertEx.Contains("ETag", $"\"{result.Version}\"", response.Headers);
            AssertEventHandlerCalled(result);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_OverwriteSystemProperties(bool useUpdatedAt, bool useVersion)
        {
            var movieToAdd = GetSampleMovie<ClientMovie>();
            movieToAdd.Id = Guid.NewGuid().ToString("N");
            if (useUpdatedAt)
            {
                movieToAdd.UpdatedAt = DateTimeOffset.Parse("2018-12-31T01:01:01.000Z");
            }
            if (useVersion)
            {
                movieToAdd.Version = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            }

            // Act
            var response = await table.InsertItemAsync(movieToAdd).ConfigureAwait(false);

            // Assert
            Assert.Equal(201, response.StatusCode);
            Assert.True(response.HasValue);
            var result = response.Value;
            Assert.Equal(movieToAdd.Id, result.Id);
            Assert.Equal<IMovie>(movieToAdd, result);
            Assert.Equal(MovieCount + 1, MovieServer.GetMovieCount());

            var entity = MovieServer.GetMovieById(result.Id)!;
            if (useUpdatedAt)
                Assert.NotEqual(movieToAdd.UpdatedAt, result.UpdatedAt);
            if (useVersion)
                Assert.NotEqual(movieToAdd.Version, result.Version);
            Assert.Equal<IMovie>(movieToAdd, entity);
            AssertEx.Contains("Location", new Uri(Endpoint, $"tables/movies/{result.Id}").ToString(), response.Headers);
            AssertEx.Contains("ETag", $"\"{result.Version}\"", response.Headers);
            AssertEventHandlerCalled(result);
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
            var table = client.GetRemoteTable<Dictionary<string, object>>("movies");
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.InsertItemAsync(movieToAdd)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(400, exception.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            Assert.Null(MovieServer.GetMovieById("test-id"));
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_Conflict()
        {
            var movieToAdd = GetSampleMovie<ClientMovie>();
            movieToAdd.Id = GetRandomId();
            var expectedMovie = MovieServer.GetMovieById(movieToAdd.Id)!;

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.InsertItemAsync(movieToAdd)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(409, exception.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(movieToAdd.Id)!;
            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, exception.ServerItem);
            Assert.Equal<IMovie>(expectedMovie, exception.ServerItem);
            Assert.Equal<IMovie>(expectedMovie, entity);
            Assert.Equal<ITableData>(expectedMovie, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
            Assert.Empty(modifications);
        }
    }
}

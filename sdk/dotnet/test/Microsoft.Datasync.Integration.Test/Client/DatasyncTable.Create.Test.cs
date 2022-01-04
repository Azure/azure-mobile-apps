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

namespace Microsoft.Datasync.Integration.Test.Client
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    [Collection("Integration")]
    public class DatasyncTable_Create_Tests : BaseTest
    {
        [Theory, CombinatorialData]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_Basic(bool hasId)
        {
            var client = GetMovieClient();
            var movieToAdd = GetSampleMovie<ClientMovie>();
            if (hasId)
            {
                movieToAdd.Id = Guid.NewGuid().ToString("N");
            }

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
            Assert.Equal(MovieCount + 1, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(result.Id)!;
            Assert.Equal<IMovie>(movieToAdd, entity);
            AssertEx.Contains("Location", new Uri(Endpoint, $"tables/movies/{result.Id}").ToString(), response.Headers);
            AssertEx.Contains("ETag", $"\"{result.Version}\"", response.Headers);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "CreateItemAsync")]
        public async Task CreateItemAsync_OverwriteSystemProperties(bool useUpdatedAt, bool useVersion)
        {
            var client = GetMovieClient();
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

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.CreateItemAsync(movieToAdd).ConfigureAwait(false);

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

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Same(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(Microsoft.Datasync.Client.TableOperation.Create, modArg.Operation);
            Assert.Equal(result.Id, modArg.Id);
            Assert.Equal<IMovie>(result, (IMovie)modArg.Entity!);
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
            var client = GetMovieClient();

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

            // Set up event handler
            var table = client.GetTable<Dictionary<string, object>>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.CreateItemAsync(movieToAdd)).ConfigureAwait(false);

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
            var client = GetMovieClient();
            var movieToAdd = GetSampleMovie<ClientMovie>();
            movieToAdd.Id = GetRandomId();
            var expectedMovie = MovieServer.GetMovieById(movieToAdd.Id)!;

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.CreateItemAsync(movieToAdd)).ConfigureAwait(false);

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

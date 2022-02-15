// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    [Collection("Integration")]
    public class RemoteTable_Insert_Tests : BaseTest
    {
        private readonly DatasyncClient client;
        private readonly IRemoteTable table;
        private readonly List<TableModifiedEventArgs> modifications = new();

        public RemoteTable_Insert_Tests()
        {
            client = GetMovieClient();
            table = client.GetRemoteTable("movies");
            table.TableModified += (sender, e) => modifications.Add(e);
        }

        #region Helper Methods
        /// <summary>
        /// Assert that the event handler was called with the right args.
        /// </summary>
        /// <param name="id"></param>
        private void AssertEventHandlerCalled(string id)
        {
            Assert.Single(modifications);
            Assert.Equal(TableModifiedEventArgs.TableOperation.Create, modifications[0].Operation);
            Assert.Equal(id, modifications[0].Id);
            Assert.Equal(table.Endpoint, modifications[0].TableEndpoint);
            Assert.NotNull(modifications[0].Entity);
        }
        #endregion

        [Theory, CombinatorialData]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_Basic(bool hasId)
        {
            var movieToAdd = GetSampleMovie<ClientMovie>();
            if (hasId)
            {
                movieToAdd.Id = Guid.NewGuid().ToString("N");
            }
            var jsonDocument = CreateJsonDocument(movieToAdd);

            // Act
            var response = await table.InsertItemAsync(jsonDocument).ConfigureAwait(false);

            // Assert
            Assert.Equal(201, response.StatusCode);
            Assert.Equal(MovieCount + 1, MovieServer.GetMovieCount());
            Assert.True(response.HasValue);
            var insertedDoc = response.Value;

            // Ensure that the ID is present and the same (if specified)
            var insertedId = insertedDoc.GetId();
            Assert.NotNull(insertedId);
            if (hasId)
            {
                Assert.Equal(movieToAdd.Id, insertedId);
            }

            // Ensure that there is a version
            var insertedVersion = insertedDoc.GetVersion();
            Assert.NotNull(insertedVersion);

            // This is the entity that was actually inserted.
            var entity = MovieServer.GetMovieById(insertedId);
            Assert.Equal<IMovie>(movieToAdd, entity);
            AssertEx.JsonDocumentMatches(entity, insertedDoc);
            AssertEventHandlerCalled(insertedId);

            // Make sure the headers match
            AssertEx.Contains("Location", new Uri(Endpoint, $"tables/movies/{insertedId}").ToString(), response.Headers);
            AssertEx.Contains("ETag", $"\"{insertedVersion}\"", response.Headers);
        }

        [Theory, CombinatorialData]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_OverwriteSystemProperties(bool useUpdatedAt, bool useVersion)
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
            var jsonDocument = CreateJsonDocument(movieToAdd);

            // Act
            var response = await table.InsertItemAsync(jsonDocument).ConfigureAwait(false);

            // Assert
            Assert.Equal(201, response.StatusCode);
            Assert.Equal(MovieCount + 1, MovieServer.GetMovieCount());

            Assert.True(response.HasValue);
            var insertedDoc = response.Value;
            var insertedId = insertedDoc.GetId();
            Assert.Equal(movieToAdd.Id, insertedId);

            var entity = MovieServer.GetMovieById(insertedId);
            Assert.Equal<IMovie>(movieToAdd, entity);
            AssertEx.JsonDocumentMatches(entity, insertedDoc);
            AssertEventHandlerCalled(insertedId);

            if (useUpdatedAt)
                Assert.NotEqual("2018-12-31T01:01:01.000Z", insertedDoc.RootElement.GetProperty("updatedAt").GetString());
            if (useVersion)
                Assert.NotEqual(movieToAdd.Version, insertedDoc.RootElement.GetProperty("version").GetString());
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
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_ValidationTest(string propName, object propValue)
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
            var jsonDocument = CreateJsonDocument(movieToAdd);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.InsertItemAsync(jsonDocument)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(400, exception.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            Assert.Null(MovieServer.GetMovieById("test-id"));
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_Conflict()
        {
            var movieToAdd = GetSampleMovie<ClientMovie>();
            movieToAdd.Id = GetRandomId();
            var jsonDocument = CreateJsonDocument(movieToAdd);
            var expectedMovie = MovieServer.GetMovieById(movieToAdd.Id)!;

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<JsonDocument>>(() => table.InsertItemAsync(jsonDocument)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(409, exception.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(movieToAdd.Id)!;
            Assert.Equal<IMovie>(expectedMovie, entity);
            Assert.Equal<ITableData>(expectedMovie, entity);
            AssertEx.JsonDocumentMatches(entity, exception.ServerItem);        
            AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
            Assert.Empty(modifications);
        }
    }
}

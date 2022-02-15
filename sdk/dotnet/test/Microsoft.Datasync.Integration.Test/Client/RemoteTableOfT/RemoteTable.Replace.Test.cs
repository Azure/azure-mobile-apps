﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
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
    public class RemoteTable_Replace_Tests : BaseTest
    {
        private readonly DatasyncClient client;
        private readonly IRemoteTable<ClientMovie> table;
        private readonly List<TableModifiedEventArgs> modifications = new();

        public RemoteTable_Replace_Tests()
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
            Assert.Equal(TableModifiedEventArgs.TableOperation.Replace, modifications[0].Operation);
            Assert.Equal(new Uri(Endpoint, relativeUri), modifications[0].TableEndpoint);
            Assert.Equal(item.Id, modifications[0].Id);
            Assert.IsAssignableFrom<ClientMovie>(modifications[0].Entity);
            Assert.Equal<IMovie>(item, (ClientMovie)modifications[0].Entity);
        }
        #endregion

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Basic()
        {
            // Arrange
            var id = GetRandomId();
            var original = MovieServer.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Act
            var response = await table.ReplaceItemAsync(expected).ConfigureAwait(false);
            var stored = MovieServer.GetMovieById(id);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.SystemPropertiesMatch(stored, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);
            AssertEventHandlerCalled(response.Value);
        }

        [Theory]
        [InlineData("duration", 50)]
        [InlineData("duration", 370)]
        [InlineData("rating", "M")]
        [InlineData("rating", "PG-13 but not always")]
        [InlineData("title", "a")]
        [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
        [InlineData("year", 1900)]
        [InlineData("year", 2035)]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Validation(string propName, object propValue)
        {
            // Arrange
            var id = GetRandomId();
            var original = MovieServer.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            var entity = expected.Clone();

            switch (propName)
            {
                case "duration": entity.Duration = (int)propValue; break;
                case "rating": entity.Rating = (string)propValue; break;
                case "title": entity.Title = (string)propValue; break;
                case "year": entity.Year = (int)propValue; break;
            }

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(entity)).ConfigureAwait(false);
            var stored = MovieServer.GetMovieById(id);

            // Assert
            Assert.Equal(400, exception.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            Assert.Equal<ITableData>(original, stored);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ThrowsWhenNotFound()
        {
            // Arrange
            var obj = GetSampleMovie<ClientMovie>();
            obj.Id = "not-found";

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(obj)).ConfigureAwait(false);

            // Assert
            Assert.Equal(404, exception.StatusCode);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ConditionalFailure()
        {
            // Arrange
            var id = GetRandomId();
            var original = MovieServer.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";
            expected.Version = "dGVzdA==";

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.ReplaceItemAsync(expected)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(412, exception.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, exception.ServerItem);
            Assert.Equal<IMovie>(original, exception.ServerItem);
            Assert.Equal<IMovie>(original, entity);
            Assert.Equal<ITableData>(original, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_SoftNotDeleted()
        {
            // Arrange
            var table = client.GetRemoteTable<ClientMovie>("soft");
            table.TableModified += (_, e) => modifications.Add(e);

            var id = GetRandomId();
            var original = MovieServer.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Act
            var response = await table.ReplaceItemAsync(expected).ConfigureAwait(false);
            var stored = MovieServer.GetMovieById(id).Clone();

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.SystemPropertiesMatch(stored, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);
            AssertEventHandlerCalled(response.Value, "tables/soft/");
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_SoftDeleted_ReturnsGone()
        {
            // Arrange
            var table = client.GetRemoteTable<ClientMovie>("soft");
            table.TableModified += (_, e) => modifications.Add(e);

            var id = GetRandomId();
            await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);
            var original = MovieServer.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(expected)).ConfigureAwait(false);
            var stored = MovieServer.GetMovieById(id).Clone();

            // Assert
            Assert.Equal(410, exception.StatusCode);
            Assert.Equal<IMovie>(original, stored);
            Assert.Equal<ITableData>(original, stored);
            Assert.Empty(modifications);
        }
    }
}

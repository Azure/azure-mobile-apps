// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.Datasync.Client;
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
    public class RemoteTable_Delete_Tests : BaseTest
    {
        private readonly DatasyncClient client;
        private readonly IRemoteTable table;
        private readonly List<TableModifiedEventArgs> modifications = new();

        public RemoteTable_Delete_Tests()
        {
            client = GetMovieClient();
            table = client.GetRemoteTable("movies");
            table.TableModified += (sender, e) => modifications.Add(e);
        }

        #region Helper Methods
        /// <summary>
        /// Assert that the event handler was called and the ID that was deleted is expected.
        /// </summary>
        private void AssertEventHandlerCalled(IRemoteTable table, string id)
        {
            Assert.Single(modifications);
            Assert.Equal(table.Endpoint, modifications[0].TableEndpoint);
            Assert.Equal(TableModifiedEventArgs.TableOperation.Delete, modifications[0].Operation);
            Assert.Equal(id, modifications[0].Id);
        }
        #endregion

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Basic()
        {
            // Arrange
            var idToDelete = GetRandomId();
            var jsonDocument = CreateJsonDocument(new IdOnly { Id = idToDelete });

            // Act
            var response = await table.DeleteItemAsync(jsonDocument).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
            Assert.Null(MovieServer.GetMovieById(idToDelete));
            AssertEventHandlerCalled(table, idToDelete);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_NotFound()
        {
            // Arrange
            const string id = "not-found";
            var jsonDocument = CreateJsonDocument(new IdOnly { Id = id });

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(jsonDocument)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(404, exception.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConditionalSuccess()
        {
            // Arrange
            var idToDelete = GetRandomId();
            var expected = MovieServer.GetMovieById(idToDelete);
            var jsonDocument = CreateJsonDocument(new IdEntity { Id = idToDelete, Version = Convert.ToBase64String(expected.Version) });

            // Act
            var response = await table.DeleteItemAsync(jsonDocument).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
            Assert.Null(MovieServer.GetMovieById(idToDelete));
            AssertEventHandlerCalled(table, idToDelete);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConditionalFailure()
        {
            // Arrange
            var idToDelete = GetRandomId();
            var jsonDocument = CreateJsonDocument(new IdEntity { Id = idToDelete, Version = "dGVzdA==" });
            var expected = MovieServer.GetMovieById(idToDelete);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<JsonDocument>>(() => table.DeleteItemAsync(jsonDocument)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(412, exception.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            Assert.Empty(modifications);

            var entity = MovieServer.GetMovieById(idToDelete);
            Assert.NotNull(entity);
            AssertEx.JsonDocumentMatches(entity, exception.ServerItem);
            Assert.Equal<IMovie>(expected, entity);
            Assert.Equal<ITableData>(expected, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_SoftDelete()
        {
            // Arrange
            var table = client.GetRemoteTable("soft");
            table.TableModified += (sender, e) => modifications.Add(e);

            var idToDelete = GetRandomId();
            var jsonDocument = CreateJsonDocument(new IdOnly { Id = idToDelete });

            // Act
            var response = await table.DeleteItemAsync(jsonDocument).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(idToDelete)!;
            Assert.True(entity.Deleted);
            AssertEventHandlerCalled(table, idToDelete);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_GoneWhenDeleted()
        {
            // Arrange
            var table = client.GetRemoteTable("soft");
            table.TableModified += (sender, e) => modifications.Add(e);

            var idToDelete = GetRandomId();
            var jsonDocument = CreateJsonDocument(new IdOnly { Id = idToDelete });
            await MovieServer.SoftDeleteMoviesAsync(x => x.Id == idToDelete).ConfigureAwait(false);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(jsonDocument)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(410, exception.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(idToDelete);
            Assert.True(entity.Deleted);
            Assert.Empty(modifications);
        }
    }
}

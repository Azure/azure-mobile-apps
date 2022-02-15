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
    public class RemoteTable_Delete_Tests : BaseTest
    {
        private readonly DatasyncClient client;
        private readonly IRemoteTable<ClientMovie> table;
        private readonly List<TableModifiedEventArgs> modifications = new();

        public RemoteTable_Delete_Tests()
        {
            client = GetMovieClient();
            table = client.GetRemoteTable<ClientMovie>("movies");
            table.TableModified += (_, e) => modifications.Add(e);
        }

        #region Helpers
        /// <summary>
        /// Assert that the event handler was called with the correct values.
        /// </summary>
        /// <param name="id"></param>
        private void AssertEventHandlerCalled(string id, string relativeUri = "tables/movies/")
        {
            Assert.Single(modifications);
            Assert.Equal(TableModifiedEventArgs.TableOperation.Delete, modifications[0].Operation);
            Assert.Equal(new Uri(Endpoint, relativeUri), modifications[0].TableEndpoint);
            Assert.Equal(id, modifications[0].Id);
        }
        #endregion

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Basic()
        {
            // Arrange
            var id = GetRandomId();
            var item = new ClientMovie { Id = id };

            // Act
            var response = await table.DeleteItemAsync(item).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
            Assert.Null(MovieServer.GetMovieById(id));
            AssertEventHandlerCalled(id);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_NotFound()
        {
            // Arrange
            const string id = "not-found";
            var item = new ClientMovie { Id = id };

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);

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
            var id = GetRandomId();
            var item = ClientMovie.From(MovieServer.GetMovieById(id));

            // Act
            var response = await table.DeleteItemAsync(item).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
            Assert.Null(MovieServer.GetMovieById(id));
            AssertEventHandlerCalled(id);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConditionalFailure()
        {
            // Arrange
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id);
            var item = new ClientMovie { Id = id, Version = "dGVzdA==" };

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(412, exception.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, exception.ServerItem);
            Assert.Equal<IMovie>(expected, exception.ServerItem);
            Assert.Equal<IMovie>(expected, entity);
            Assert.Equal<ITableData>(expected, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_SoftDelete()
        {
            // Arrange
            var table = client.GetRemoteTable<ClientMovie>("soft");
            table.TableModified += (_, e) => modifications.Add(e);

            var id = GetRandomId();
            var item = ClientMovie.From(MovieServer.GetMovieById(id));

            // Act
            var response = await table.DeleteItemAsync(item).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(id)!;
            Assert.True(entity.Deleted);
            AssertEventHandlerCalled(id, "tables/soft/");
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_GoneWhenDeleted()
        {
            // Arrange
            var table = client.GetRemoteTable<ClientMovie>("soft");
            table.TableModified += (_, e) => modifications.Add(e);

            var id = GetRandomId();
            var item = ClientMovie.From(MovieServer.GetMovieById(id));
            await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(410, exception.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(id);
            Assert.True(entity.Deleted);
            Assert.Empty(modifications);
        }
    }
}

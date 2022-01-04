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
    public class DatasyncTable_Delete_Test : BaseTest
    {
        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_Basic()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.DeleteItemAsync(id).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
            Assert.Null(MovieServer.GetMovieById(id));

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(Microsoft.Datasync.Client.TableOperation.Delete, modArg.Operation);
            Assert.Equal(id, modArg.Id);
            Assert.Null(modArg.Entity);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_NotFound()
        {
            // Arrange
            var client = GetMovieClient();
            const string id = "not-found";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(id)).ConfigureAwait(false);

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
            var client = GetMovieClient();
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id);
            var etag = Convert.ToBase64String(expected.Version);

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.DeleteItemAsync(id, IfMatch.Version(etag)).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
            Assert.Null(MovieServer.GetMovieById(id));

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(Microsoft.Datasync.Client.TableOperation.Delete, modArg.Operation);
            Assert.Equal(id, modArg.Id);
            Assert.Null(modArg.Entity);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_ConditionalFailure()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id);
            const string etag = "dGVzdA==";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.DeleteItemAsync(id, IfMatch.Version(etag))).ConfigureAwait(false);

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
            var client = GetMovieClient();
            var id = GetRandomId();

            // Set up event handler
            var table = client.GetTable<ClientMovie>("soft");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.DeleteItemAsync(id).ConfigureAwait(false);

            // Assert
            Assert.Equal(204, response.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(id)!;
            Assert.True(entity.Deleted);

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(Microsoft.Datasync.Client.TableOperation.Delete, modArg.Operation);
            Assert.Equal(id, modArg.Id);
            Assert.Null(modArg.Entity);
        }

        [Fact]
        [Trait("Method", "DeleteItemAsync")]
        public async Task DeleteItemAsync_GoneWhenDeleted()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);

            // Set up event handler
            var table = client.GetTable<ClientMovie>("soft");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.DeleteItemAsync(id)).ConfigureAwait(false);

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

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    [Collection("Integration")]
    public class RemoteTable_GetItem_Tests : BaseTest
    {
        private readonly DatasyncClient client;
        private readonly IRemoteTable table;
        private readonly List<TableModifiedEventArgs> modifications = new();

        public RemoteTable_GetItem_Tests()
        {
            client = GetMovieClient();
            table = client.GetRemoteTable("movies");
            table.TableModified += (sender, e) => modifications.Add(e);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_Basic()
        {
            // Arrange
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;

            // Act
            var response = await table.GetItemAsync(id).ConfigureAwait(false);

            // Assert
            Assert.Equal(200, response.StatusCode);
            AssertEx.JsonDocumentMatches(expected, response.Value);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_NotFound()
        {
            // Arrange
            const string id = "not-found";

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetItemAsync(id)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(404, exception.StatusCode);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_GetIfNotSoftDeleted()
        {
            // Arrange
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;

            // Act
            var table = client.GetRemoteTable("soft");
            var response = await table.GetItemAsync(id).ConfigureAwait(false);

            // Assert
            Assert.Equal(200, response.StatusCode);
            AssertEx.JsonDocumentMatches(expected, response.Value);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_GoneIfSoftDeleted()
        {
            // Arrange
            // Arrange
            var id = GetRandomId();
            await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);

            // Act
            var table = client.GetRemoteTable("soft");
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetItemAsync(id)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(410, exception.StatusCode);
            Assert.Empty(modifications);
        }
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTableOfT
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    [Collection("Integration")]
    public class RemoteTable_GetItem_Tests : BaseTest
    {
        private readonly DatasyncClient client;
        private readonly IRemoteTable<ClientMovie> table;

        public RemoteTable_GetItem_Tests()
        {
            client = GetMovieClient();
            table = client.GetRemoteTable<ClientMovie>("movies");
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
            Assert.Equal<IMovie>(expected, response.Value);
            AssertEx.SystemPropertiesMatch(expected, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);
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
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_GetIfNotSoftDeleted()
        {
            // Arrange
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;

            // Act
            var table = client.GetRemoteTable<ClientMovie>("soft");
            var response = await table.GetItemAsync(id).ConfigureAwait(false);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, response.Value);
            AssertEx.SystemPropertiesMatch(expected, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_GoneIfSoftDeleted()
        {
            // Arrange
            var id = GetRandomId();
            await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);
            var expected = MovieServer.GetMovieById(id)!;
            var etag = Convert.ToBase64String(expected.Version);

            // Act
            var table = client.GetRemoteTable<ClientMovie>("soft");
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetItemAsync(id)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(410, exception.StatusCode);
        }
    }
}

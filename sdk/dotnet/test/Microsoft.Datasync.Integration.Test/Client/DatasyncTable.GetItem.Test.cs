// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    [Collection("Integration")]
    public class DatasyncTable_GetItem_Test : BaseTest
    {
        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_Basic()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;

            // Act
            var table = client.GetTable<ClientMovie>("movies");
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
            var client = GetMovieClient();
            const string id = "not-found";

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetItemAsync(id)).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_GetIfChanged()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var response = await table.GetItemAsync(id, IfNoneMatch.Version("dGVzdA==")).ConfigureAwait(false);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, response.Value);
            AssertEx.SystemPropertiesMatch(expected, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_FailIfSame()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;
            var etag = Convert.ToBase64String(expected.Version);

            // Act
            var table = client.GetTable<ClientMovie>("movies");
            var exception = await Assert.ThrowsAsync<EntityNotModifiedException>(() => table.GetItemAsync(id, IfNoneMatch.Version(etag))).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(304, exception.StatusCode);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_GetIfNotSoftDeleted()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;

            // Act
            var table = client.GetTable<ClientMovie>("soft");
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
            var client = GetMovieClient();
            var id = GetRandomId();
            await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);
            var expected = MovieServer.GetMovieById(id)!;
            var etag = Convert.ToBase64String(expected.Version);

            // Act
            var table = client.GetTable<ClientMovie>("soft");
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.GetItemAsync(id, IfNoneMatch.Version(etag))).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(410, exception.StatusCode);
        }
    }
}

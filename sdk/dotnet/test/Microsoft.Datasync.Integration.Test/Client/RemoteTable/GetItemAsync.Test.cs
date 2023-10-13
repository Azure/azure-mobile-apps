// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable;

[ExcludeFromCodeCoverage]
public class GetItemAsync_Tests : BaseOperationTest
{
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
        AssertJsonDocumentMatches(expected, response);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_NotFound()
    {
        // Arrange
        const string id = "not-found";

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.GetItemAsync(id)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(HttpStatusCode.NotFound, exception.Response?.StatusCode);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_GetIfNotSoftDeleted()
    {
        // Arrange
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;

        // Act
        var response = await soft.GetItemAsync(id).ConfigureAwait(false);

        // Assert
        AssertJsonDocumentMatches(expected, response);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_GoneIfSoftDeleted()
    {
        // Arrange
        var id = GetRandomId();
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => soft.GetItemAsync(id)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(HttpStatusCode.Gone, exception.Response?.StatusCode);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_GetIfSoftDeleted()
    {
        // Arrange
        var id = GetRandomId();
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);
        var expected = MovieServer.GetMovieById(id)!;

        // Act
        var response = await soft.GetItemAsync(id, true).ConfigureAwait(false);

        // Assert
        AssertJsonDocumentMatches(expected, response);
    }
}

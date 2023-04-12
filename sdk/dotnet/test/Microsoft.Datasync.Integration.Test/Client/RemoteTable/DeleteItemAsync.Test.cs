// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable;

[ExcludeFromCodeCoverage]
public class DeleteItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Basic()
    {
        // Arrange
        var idToDelete = GetRandomId();
        var jsonDocument = CreateJsonDocument(new IdOnly { Id = idToDelete });

        // Act
        await table.DeleteItemAsync(jsonDocument).ConfigureAwait(false);

        // Assert
        Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
        Assert.Null(MovieServer.GetMovieById(idToDelete));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_NotFound()
    {
        // Arrange
        const string id = "not-found";
        var jsonDocument = CreateJsonDocument(new IdOnly { Id = id });

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.DeleteItemAsync(jsonDocument)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(HttpStatusCode.NotFound, exception.Response?.StatusCode);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
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
        await table.DeleteItemAsync(jsonDocument).ConfigureAwait(false);

        // Assert
        Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
        Assert.Null(MovieServer.GetMovieById(idToDelete));
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
        var exception = await Assert.ThrowsAsync<DatasyncConflictException>(() => table.DeleteItemAsync(jsonDocument)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.Response?.StatusCode);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());

        var entity = MovieServer.GetMovieById(idToDelete);
        Assert.NotNull(entity);
        AssertJsonDocumentMatches(entity, exception.Value);
        Assert.Equal<IMovie>(expected, entity);
        Assert.Equal<ITableData>(expected, entity);
        AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_SoftDelete()
    {
        // Arrange
        var idToDelete = GetRandomId();
        var jsonDocument = CreateJsonDocument(new IdOnly { Id = idToDelete });

        // Act
        await soft.DeleteItemAsync(jsonDocument).ConfigureAwait(false);

        // Assert
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        var entity = MovieServer.GetMovieById(idToDelete)!;
        Assert.True(entity.Deleted);
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_GoneWhenDeleted()
    {
        // Arrange
        var idToDelete = GetRandomId();
        var jsonDocument = CreateJsonDocument(new IdOnly { Id = idToDelete });
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == idToDelete).ConfigureAwait(false);
        var original = MovieServer.GetMovieById(idToDelete);

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => soft.DeleteItemAsync(jsonDocument)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(HttpStatusCode.Gone, exception.Response?.StatusCode);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        var entity = MovieServer.GetMovieById(idToDelete);
        Assert.True(entity.Deleted);
    }
}

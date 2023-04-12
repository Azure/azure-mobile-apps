// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTableOfT;

[ExcludeFromCodeCoverage]
public class DeleteItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Basic()
    {
        // Arrange
        var id = GetRandomId();
        var item = new ClientMovie { Id = id };

        // Act
        await table.DeleteItemAsync(item).ConfigureAwait(false);

        // Assert
        Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
        Assert.Null(MovieServer.GetMovieById(id));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_NotFound()
    {
        // Arrange
        const string id = "not-found";
        var item = new ClientMovie { Id = id };

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);

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
        var id = GetRandomId();
        var item = ClientMovie.From(MovieServer.GetMovieById(id));

        // Act
        await table.DeleteItemAsync(item).ConfigureAwait(false);

        // Assert
        Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
        Assert.Null(MovieServer.GetMovieById(id));
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
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        var entity = MovieServer.GetMovieById(id);
        Assert.NotNull(entity);
        AssertEx.SystemPropertiesSet(entity, startTime);
        AssertEx.SystemPropertiesMatch(entity, exception.Item);
        Assert.Equal<IMovie>(expected, exception.Item);
        Assert.Equal<IMovie>(expected, entity);
        Assert.Equal<ITableData>(expected, entity);
        AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_SoftDelete()
    {
        // Arrange
        var id = GetRandomId();
        var item = ClientMovie.From(MovieServer.GetMovieById(id));

        // Act
        await soft.DeleteItemAsync(item).ConfigureAwait(false);

        // Assert
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        var entity = MovieServer.GetMovieById(id)!;
        Assert.True(entity.Deleted);
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_GoneWhenDeleted()
    {
        // Arrange
        var id = GetRandomId();
        var item = ClientMovie.From(MovieServer.GetMovieById(id));
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => soft.DeleteItemAsync(item)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(HttpStatusCode.Gone, exception.Response?.StatusCode);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        var entity = MovieServer.GetMovieById(id);
        Assert.True(entity.Deleted);
    }
}

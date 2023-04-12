// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTableOfT;

[ExcludeFromCodeCoverage]
public class UndeleteItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_Basic()
    {
        // Arrange
        var id = GetRandomId();
        var item = new ClientMovie { Id = id };

        // Act
        await table.UndeleteItemAsync(item).ConfigureAwait(false);
        var stored = MovieServer.GetMovieById(id)!;

        // Assert
        Assert.False(stored.Deleted);
        Assert.Equal<IMovie>(stored, item);
        AssertEx.SystemPropertiesMatch(stored, item);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_NotFound()
    {
        // Arrange
        const string id = "not-found";
        var item = new ClientMovie { Id = id };

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.UndeleteItemAsync(item)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(HttpStatusCode.NotFound, exception.Response?.StatusCode);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_ConditionalFailure()
    {
        // Arrange
        var id = GetRandomId();
        var item = new ClientMovie { Id = id, Version = "dGVzdA==" };
        var expected = MovieServer.GetMovieById(id);

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.UndeleteItemAsync(item)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.Response?.StatusCode);

        var entity = MovieServer.GetMovieById(id);
        Assert.NotNull(entity);
        Assert.Equal<IMovie>(entity, exception.Item);
        Assert.Equal<IMovie>(expected, entity);
        Assert.Equal<ITableData>(expected, entity);
        AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_SoftDelete()
    {
        // Arrange
        var id = GetRandomId();
        var item = new ClientMovie { Id = id };

        // Act
        await soft.UndeleteItemAsync(item).ConfigureAwait(false);
        var stored = MovieServer.GetMovieById(id)!;

        // Assert
        Assert.False(stored.Deleted);
        Assert.Equal<IMovie>(stored, item);
        AssertEx.SystemPropertiesMatch(stored, item);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_WhenDeleted()
    {
        // Arrange
        var id = GetRandomId();
        var item = new ClientMovie { Id = id };
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);
        var original = MovieServer.GetMovieById(id);

        // Act
        await soft.UndeleteItemAsync(item).ConfigureAwait(false);
        var stored = MovieServer.GetMovieById(id)!;

        // Assert
        Assert.False(stored.Deleted);
        Assert.Equal<IMovie>(stored, item);
        AssertEx.SystemPropertiesMatch(stored, item);
    }
}

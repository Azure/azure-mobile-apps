// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable;

[ExcludeFromCodeCoverage]
public class UndeleteItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_Basic()
    {
        // Arrange
        var id = GetRandomId();
        var jsonDocument = CreateJsonDocument(new IdOnly { Id = id });

        // Act
        var response = await table.UndeleteItemAsync(jsonDocument).ConfigureAwait(false);
        var stored = MovieServer.GetMovieById(id)!;

        // Assert
        Assert.False(stored.Deleted);
        AssertJsonDocumentMatches(stored, response);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_NotFound()
    {
        // Arrange
        const string id = "not-found";
        var jsonDocument = CreateJsonDocument(new IdOnly { Id = id });

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.UndeleteItemAsync(jsonDocument)).ConfigureAwait(false);

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
        var jsonDocument = CreateJsonDocument(new IdEntity { Id = id, Version = "dGVzdA==" });
        var expected = MovieServer.GetMovieById(id);

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncConflictException>(() => table.UndeleteItemAsync(jsonDocument)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.Response?.StatusCode);

        var entity = MovieServer.GetMovieById(id);
        Assert.NotNull(entity);
        AssertJsonDocumentMatches(entity, exception.Value);
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
        var jsonDocument = CreateJsonDocument(new IdOnly { Id = id });

        // Act
        var response = await soft.UndeleteItemAsync(jsonDocument).ConfigureAwait(false);
        var stored = MovieServer.GetMovieById(id)!;

        // Assert
        Assert.False(stored.Deleted);
        AssertJsonDocumentMatches(stored, response);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_WhenDeleted()
    {
        // Arrange
        var id = GetRandomId();
        var jsonDocument = CreateJsonDocument(new IdOnly { Id = id });
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);
        var original = MovieServer.GetMovieById(id);

        // Act
        var response = await soft.UndeleteItemAsync(jsonDocument).ConfigureAwait(false);
        var stored = MovieServer.GetMovieById(id)!;

        // Assert
        Assert.False(stored.Deleted);
        AssertJsonDocumentMatches(stored, response);
    }
}


// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable;

[ExcludeFromCodeCoverage]
public class ReplaceItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Basic()
    {
        // Arrange
        var id = GetRandomId();
        var original = MovieServer.GetMovieById(id)!;
        var expected = ClientMovie.From(original);
        expected.Title = "Replacement Title";
        expected.Rating = "PG-13";
        var json = CreateJsonDocument(expected);

        // Act
        var response = await table.ReplaceItemAsync(json).ConfigureAwait(false);
        var stored = MovieServer.GetMovieById(id);

        // Assert
        Assert.Equal<IMovie>(expected, stored);
        AssertEx.SystemPropertiesChanged(original, stored);
        AssertJsonDocumentMatches(stored, response);
    }

    [Theory]
    [InlineData("duration", 50)]
    [InlineData("duration", 370)]
    [InlineData("rating", "M")]
    [InlineData("rating", "PG-13 but not always")]
    [InlineData("title", "a")]
    [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
    [InlineData("year", 1900)]
    [InlineData("year", 2035)]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Validation(string propName, object propValue)
    {
        // Arrange
        var id = GetRandomId();
        var original = MovieServer.GetMovieById(id)!;
        var expected = ClientMovie.From(original);
        var entity = expected.Clone();

        switch (propName)
        {
            case "duration": entity.Duration = (int)propValue; break;
            case "rating": entity.Rating = (string)propValue; break;
            case "title": entity.Title = (string)propValue; break;
            case "year": entity.Year = (int)propValue; break;
        }
        var json = CreateJsonDocument(entity);

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);
        var stored = MovieServer.GetMovieById(id);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, exception.Response?.StatusCode);
        Assert.Equal<IMovie>(expected, stored);
        Assert.Equal<ITableData>(original, stored);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsWhenNotFound()
    {
        // Arrange
        var obj = GetSampleMovie<ClientMovie>();
        obj.Id = "not-found";
        var json = CreateJsonDocument(obj);

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, exception.Response?.StatusCode);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ConditionalFailure()
    {
        // Arrange
        var id = GetRandomId();
        var original = MovieServer.GetMovieById(id)!;
        var expected = ClientMovie.From(original);
        expected.Title = "Replacement Title";
        expected.Rating = "PG-13";
        expected.Version = "dGVzdA==";
        var json = CreateJsonDocument(expected);

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncConflictException>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        var entity = MovieServer.GetMovieById(id);
        Assert.NotNull(entity);
        AssertEx.SystemPropertiesSet(entity, startTime);
        AssertJsonDocumentMatches(entity, exception.Value);
        Assert.Equal<IMovie>(original, entity);
        Assert.Equal<ITableData>(original, entity);
        AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_SoftNotDeleted()
    {
        // Arrange
        var id = GetRandomId();
        var original = MovieServer.GetMovieById(id)!;
        var expected = ClientMovie.From(original);
        expected.Title = "Replacement Title";
        expected.Rating = "PG-13";
        var json = CreateJsonDocument(expected);

        // Act
        var response = await soft.ReplaceItemAsync(json).ConfigureAwait(false);
        var stored = MovieServer.GetMovieById(id).Clone();

        // Assert
        Assert.Equal<IMovie>(expected, stored);
        AssertEx.SystemPropertiesChanged(original, stored);
        AssertJsonDocumentMatches(stored, response);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_SoftDeleted_ReturnsGone()
    {
        // Arrange
        var id = GetRandomId();
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);
        var original = MovieServer.GetMovieById(id)!;
        var expected = ClientMovie.From(original);
        expected.Title = "Replacement Title";
        expected.Rating = "PG-13";
        var json = CreateJsonDocument(expected);

        var table = client.GetRemoteTable("soft");

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => soft.ReplaceItemAsync(json)).ConfigureAwait(false);
        var stored = MovieServer.GetMovieById(id).Clone();

        // Assert
        Assert.Equal(HttpStatusCode.Gone, exception.Response?.StatusCode);
        Assert.Equal<IMovie>(original, stored);
        Assert.Equal<ITableData>(original, stored);
    }
}

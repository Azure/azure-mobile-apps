// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTableOfT;

[ExcludeFromCodeCoverage]
public class InsertItemAsync_Tests : BaseOperationTest
{
    [Theory, CombinatorialData]
    [Trait("Method", "CreateItemAsync")]
    public async Task CreateItemAsync_Basic(bool hasId)
    {
        var movieToAdd = GetSampleMovie<ClientMovie>();
        if (hasId)
        {
            movieToAdd.Id = Guid.NewGuid().ToString("N");
        }
        var result = movieToAdd.Clone();

        // Act
        await table.InsertItemAsync(result).ConfigureAwait(false);

        // Assert
        Assert.True(Guid.TryParse(result.Id, out _));
        if (hasId)
        {
            Assert.Equal(movieToAdd.Id, result.Id);
        }
        Assert.Equal<IMovie>(movieToAdd, result);
        Assert.Equal(MovieCount + 1, MovieServer.GetMovieCount());
        var entity = MovieServer.GetMovieById(result.Id)!;
        Assert.Equal<IMovie>(movieToAdd, entity);
        AssertEx.SystemPropertiesMatch(entity, result);
    }

    [Theory, CombinatorialData]
    [Trait("Method", "CreateItemAsync")]
    public async Task CreateItemAsync_OverwriteSystemProperties(bool useUpdatedAt, bool useVersion)
    {
        var movieToAdd = GetSampleMovie<ClientMovie>();
        movieToAdd.Id = Guid.NewGuid().ToString("N");
        if (useUpdatedAt)
        {
            movieToAdd.UpdatedAt = DateTimeOffset.Parse("2018-12-31T01:01:01.000Z");
        }
        if (useVersion)
        {
            movieToAdd.Version = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
        var result = movieToAdd.Clone();

        // Act
        await table.InsertItemAsync(result).ConfigureAwait(false);

        // Assert
        Assert.Equal(movieToAdd.Id, result.Id);
        Assert.Equal<IMovie>(movieToAdd, result);
        Assert.Equal(MovieCount + 1, MovieServer.GetMovieCount());

        var entity = MovieServer.GetMovieById(result.Id)!;
        if (useUpdatedAt)
            Assert.NotEqual(movieToAdd.UpdatedAt, result.UpdatedAt);
        if (useVersion)
            Assert.NotEqual(movieToAdd.Version, result.Version);
        Assert.Equal<IMovie>(movieToAdd, entity);
    }

    [Theory]
    [InlineData("duration", 50)]
    [InlineData("duration", 370)]
    [InlineData("duration", null)]
    [InlineData("rating", "M")]
    [InlineData("rating", "PG-13 but not always")]
    [InlineData("title", "a")]
    [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
    [InlineData("title", null)]
    [InlineData("year", 1900)]
    [InlineData("year", 2035)]
    [InlineData("year", null)]
    [Trait("Method", "CreateItemAsync")]
    public async Task CreateItemAsync_ValidationTest(string propName, object propValue)
    {
        Dictionary<string, object> movieToAdd = new()
        {
            { "id", "test-id" },
            { "updatedAt", DateTimeOffset.Parse("2018-12-31T01:01:01.000Z") },
            { "version", Convert.ToBase64String(Guid.NewGuid().ToByteArray()) },
            { "bestPictureWinner", false },
            { "duration", 120 },
            { "rating", "G" },
            { "releaseDate", DateTimeOffset.Parse("2018-12-30T05:30:00.000Z") },
            { "title", "Home Video" },
            { "year", 2021 }
        };
        if (propValue == null)
            movieToAdd.Remove(propName);
        else
            movieToAdd[propName] = propValue;

        // Act
        var table = client.GetRemoteTable<Dictionary<string, object>>("movies");
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.InsertItemAsync(movieToAdd)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Response?.StatusCode);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        Assert.Null(MovieServer.GetMovieById("test-id"));
    }

    [Fact]
    [Trait("Method", "CreateItemAsync")]
    public async Task CreateItemAsync_Conflict()
    {
        var movieToAdd = GetSampleMovie<ClientMovie>();
        movieToAdd.Id = GetRandomId();
        var expectedMovie = MovieServer.GetMovieById(movieToAdd.Id)!;

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.InsertItemAsync(movieToAdd)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        var entity = MovieServer.GetMovieById(movieToAdd.Id)!;
        AssertEx.SystemPropertiesSet(entity, startTime);
        AssertEx.SystemPropertiesMatch(entity, exception.Item);
        Assert.Equal<IMovie>(expectedMovie, exception.Item);
        Assert.Equal<IMovie>(expectedMovie, entity);
        Assert.Equal<ITableData>(expectedMovie, entity);
        AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable;

[ExcludeFromCodeCoverage]
public class InsertItemAsync_Tests : BaseOperationTest
{
    [Theory, CombinatorialData]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_Basic(bool hasId)
    {
        var movieToAdd = GetSampleMovie<ClientMovie>();
        if (hasId)
        {
            movieToAdd.Id = Guid.NewGuid().ToString("N");
        }
        var jsonDocument = CreateJsonDocument(movieToAdd);

        // Act
        var response = await table.InsertItemAsync(jsonDocument).ConfigureAwait(false);

        // Assert
        Assert.Equal(MovieCount + 1, MovieServer.GetMovieCount());

        // Ensure that the ID is present and the same (if specified)
        var insertedId = response.Value<string>("id");
        Assert.NotNull(insertedId);
        if (hasId)
        {
            Assert.Equal(movieToAdd.Id, insertedId);
        }

        // Ensure that there is a version
        var insertedVersion = response.Value<string>("version");
        Assert.NotNull(insertedVersion);

        // This is the entity that was actually inserted.
        var entity = MovieServer.GetMovieById(insertedId);
        Assert.Equal<IMovie>(movieToAdd, entity);
        AssertJsonDocumentMatches(entity, response);
    }

    [Theory, CombinatorialData]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_OverwriteSystemProperties(bool useUpdatedAt, bool useVersion)
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
        var jsonDocument = CreateJsonDocument(movieToAdd);

        // Act
        var response = await table.InsertItemAsync(jsonDocument).ConfigureAwait(false);

        // Assert
        Assert.Equal(MovieCount + 1, MovieServer.GetMovieCount());
        var insertedId = response.Value<string>("id");
        Assert.Equal(movieToAdd.Id, insertedId);

        var entity = MovieServer.GetMovieById(insertedId);
        Assert.Equal<IMovie>(movieToAdd, entity);
        AssertJsonDocumentMatches(entity, response);

        if (useUpdatedAt)
            Assert.NotEqual("2018-12-31T01:01:01.000Z", response.Value<string>("updatedAt"));
        if (useVersion)
            Assert.NotEqual(movieToAdd.Version, response.Value<string>("version"));
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
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_ValidationTest(string propName, object propValue)
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
        var jsonDocument = CreateJsonDocument(movieToAdd);

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.InsertItemAsync(jsonDocument)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Response?.StatusCode);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        Assert.Null(MovieServer.GetMovieById("test-id"));
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_Conflict()
    {
        var movieToAdd = GetSampleMovie<ClientMovie>();
        movieToAdd.Id = GetRandomId();
        var jsonDocument = CreateJsonDocument(movieToAdd);
        var expectedMovie = MovieServer.GetMovieById(movieToAdd.Id)!;

        // Act
        var exception = await Assert.ThrowsAsync<DatasyncConflictException>(() => table.InsertItemAsync(jsonDocument)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        var entity = MovieServer.GetMovieById(movieToAdd.Id)!;
        Assert.Equal<IMovie>(expectedMovie, entity);
        Assert.Equal<ITableData>(expectedMovie, entity);
        AssertJsonDocumentMatches(entity, exception.Value);
        AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
    }
}

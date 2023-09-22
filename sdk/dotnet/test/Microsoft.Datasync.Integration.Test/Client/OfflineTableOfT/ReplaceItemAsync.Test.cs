// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTableOfT;

[ExcludeFromCodeCoverage]
public class ReplaceItemAsync_Tests : BaseOperationTest
{
    public ReplaceItemAsync_Tests(ITestOutputHelper logger) : base(logger, false) { }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Basic()
    {
        await InitializeAsync();

        // Arrange
        var id = GetRandomId();
        var original = MovieServer.GetMovieById(id)!;
        var expected = ClientMovie.From(original);
        expected.Title = "Replacement Title";
        expected.Rating = "PG-13";

        // Act
        await table!.ReplaceItemAsync(expected);

        // There should be one pending operation
        Assert.Equal(1, client.PendingOperations);

        await table!.PushItemsAsync();
        var response = await table!.GetItemAsync(id);

        var stored = MovieServer.GetMovieById(id);

        // Assert
        Assert.Equal<IMovie>(expected, stored);
        AssertEx.SystemPropertiesChanged(original, stored);
        AssertEx.SystemPropertiesMatch(stored, response);
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
        await InitializeAsync();

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

        // Act
        await table!.ReplaceItemAsync(entity);
        var exception = await Assert.ThrowsAsync<PushFailedException>(() => table!.PushItemsAsync());

        var stored = MovieServer.GetMovieById(id);

        // Assert
        Assert.Single(exception.PushResult.Errors);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsWhenNotFound()
    {
        await InitializeAsync();

        // Arrange
        var obj = GetSampleMovie<ClientMovie>();
        obj.Id = "not-found";

        // Act
        await Assert.ThrowsAsync<OfflineStoreException>(() => table!.ReplaceItemAsync(obj));
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ConditionalFailure()
    {
        await InitializeAsync();

        // Arrange
        var id = GetRandomId();
        var original = MovieServer.GetMovieById(id)!;
        var expected = ClientMovie.From(original);
        expected.Title = "Replacement Title";
        expected.Rating = "PG-13";
        expected.Version = "dGVzdA==";

        // Act
        await table!.ReplaceItemAsync(expected);
        var exception = await Assert.ThrowsAsync<PushFailedException>(() => table!.PushItemsAsync());

        // Assert
        Assert.Single(exception.PushResult.Errors);
    }
}

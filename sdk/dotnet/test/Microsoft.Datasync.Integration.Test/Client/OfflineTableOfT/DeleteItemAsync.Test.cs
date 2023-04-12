// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTableOfT;

[ExcludeFromCodeCoverage]
public class DeleteItemAsync_Tests : BaseOperationTest
{
    public DeleteItemAsync_Tests(ITestOutputHelper logger) : base(logger) { }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Basic()
    {
        await InitializeAsync();

        // Arrange
        var id = GetRandomId();
        var item = ClientMovie.From(MovieServer.GetMovieById(id));

        // Act
        await table!.DeleteItemAsync(item);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        Assert.Equal(1, client.PendingOperations);

        await table!.PushItemsAsync();
        Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
        Assert.Null(MovieServer.GetMovieById(id));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_NotFound()
    {
        await InitializeAsync();

        // Arrange
        const string id = "not-found";
        var item = new ClientMovie { Id = id };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => table!.DeleteItemAsync(item));

        // Assert
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ConditionalSuccess()
    {
        await InitializeAsync();

        // Arrange
        var id = GetRandomId();
        var item = ClientMovie.From(MovieServer.GetMovieById(id));

        // Act
        await table!.DeleteItemAsync(item);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());

        await table!.PushItemsAsync();

        // Assert
        Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
        Assert.Null(MovieServer.GetMovieById(id));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ConditionalFailure()
    {
        await InitializeAsync();

        // Arrange
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id);
        var item = ClientMovie.From(MovieServer.GetMovieById(id));
        await ModifyServerVersionAsync(id);

        // Act
        await table!.DeleteItemAsync(item);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());

        var exception = await Assert.ThrowsAsync<PushFailedException>(() => table!.PushItemsAsync());

        // Assert
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        var entity = MovieServer.GetMovieById(id);

        // Check the PushFailedException
        Assert.Single(exception.PushResult.Errors);
        var error = exception.PushResult.Errors.First();
        AssertSystemPropertiesMatch(entity, error.Result);
    }
}

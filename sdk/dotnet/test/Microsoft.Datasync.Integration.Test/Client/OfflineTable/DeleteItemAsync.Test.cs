// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTable;

[ExcludeFromCodeCoverage]
public class DeleteItemAsync_Tests : BaseOperationTest
{
    public DeleteItemAsync_Tests(ITestOutputHelper logger) : base(logger) { }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Basic()
    {
        await InitializeAsync(true);

        // Arrange
        var idToDelete = GetRandomId();

        // Act
        var jsonDocument = await table!.GetItemAsync(idToDelete);
        Assert.NotNull(jsonDocument);

        await table!.DeleteItemAsync(jsonDocument);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        Assert.NotNull(MovieServer.GetMovieById(idToDelete));

        await table!.PushItemsAsync();
        Assert.Equal(MovieCount - 1, MovieServer.GetMovieCount());
        Assert.Null(MovieServer.GetMovieById(idToDelete));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_NotFound()
    {
        await InitializeAsync(true);

        // Arrange
        const string id = "not-found";
        var jsonDocument = CreateJsonDocument(new IdOnly { Id = id });

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => table!.DeleteItemAsync(jsonDocument));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ConditionalFailure()
    {
        await InitializeAsync(true);

        // Arrange
        var idToDelete = GetRandomId();

        // Act
        // Update the version on the server.
        await ModifyServerVersionAsync(idToDelete);

        var jsonDocument = await table!.GetItemAsync(idToDelete);
        Assert.NotNull(jsonDocument);

        await table!.DeleteItemAsync(jsonDocument);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        Assert.NotNull(MovieServer.GetMovieById(idToDelete));

        var exception = await Assert.ThrowsAsync<PushFailedException>(() => table!.PushItemsAsync());
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        Assert.NotNull(MovieServer.GetMovieById(idToDelete));

        Assert.NotNull(exception.PushResult);
        Assert.Single(exception.PushResult.Errors);
        var error = exception.PushResult.Errors.First();
        Assert.Equal(idToDelete, error.Item.Value<string>("id"));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_SoftDelete()
    {
        await InitializeAsync(true);

        // Arrange
        var idToDelete = GetRandomId();

        // Act
        var jsonDocument = await soft!.GetItemAsync(idToDelete);
        Assert.NotNull(jsonDocument);

        await soft!.DeleteItemAsync(jsonDocument);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        Assert.NotNull(MovieServer.GetMovieById(idToDelete));

        await soft!.PushItemsAsync();
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());
        var item = MovieServer.GetMovieById(idToDelete);
        Assert.True(item.Deleted);
    }
}

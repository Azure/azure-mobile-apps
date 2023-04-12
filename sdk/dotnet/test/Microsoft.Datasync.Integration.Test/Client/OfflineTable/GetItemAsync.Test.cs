// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTable;

[ExcludeFromCodeCoverage]
public class GetItemAsync_Tests : BaseOperationTest
{
    public GetItemAsync_Tests(ITestOutputHelper logger) : base(logger) { }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_Basic()
    {
        await InitializeAsync(true);

        // Arrange
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;

        // Act
        var response = await table!.GetItemAsync(id);

        // Assert
        Assert.NotNull(response);
        var movie = client.Serializer.Deserialize<ClientMovie>(response);
        Assert.Equal<IMovie>(expected, movie);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_NotFound()
    {
        await InitializeAsync(true);

        // Arrange
        const string id = "not-found";

        // Act
        var result = await table!.GetItemAsync(id);

        // Assert
        Assert.Null(result);
    }
}

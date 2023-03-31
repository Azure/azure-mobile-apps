// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTable;

[ExcludeFromCodeCoverage]
public class InsertItemWithIdGeneratorAsync_Tests : BaseOperationTest
{
    protected new readonly DatasyncClient client;

    public InsertItemWithIdGeneratorAsync_Tests(ITestOutputHelper logger) : base(logger, false)
    {
        client = GetMovieClientWithIdGenerator(store: store);
    }

    [Theory, CombinatorialData]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_Basic(bool hasId)
    {
        await InitializeAsync(true);

        var movieToAdd = GetSampleMovie<ClientMovie>();
        if (hasId)
        {
            movieToAdd.Id = MyIdGenerator(table?.TableName);
        }
        var jsonDocument = CreateJsonDocument(movieToAdd);

        // Act
        var response = await table!.InsertItemAsync(jsonDocument);
        Assert.Equal(MovieCount, MovieServer.GetMovieCount());

        // Ensure that the ID is present and the same (if specified)
        var insertedId = response.Value<string>("id");
        Assert.NotNull(insertedId);
        if (hasId)
        {
            Assert.Equal(movieToAdd.Id, insertedId);
        }

        await table!.PushItemsAsync();
        Assert.Equal(MovieCount + 1, MovieServer.GetMovieCount());

        var stored = await table!.GetItemAsync(insertedId);
        // Ensure that there is a version
        var insertedVersion = stored.Value<string>("version");
        Assert.NotNull(insertedVersion);

        // This is the entity that was actually inserted.
        var entity = MovieServer.GetMovieById(insertedId);
        Assert.Equal<IMovie>(movieToAdd, entity);
        AssertSystemPropertiesMatch(entity, stored);
    }

    [Theory, CombinatorialData]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_OverwriteSystemProperties(bool useUpdatedAt, bool useVersion)
    {
        await InitializeAsync(true);

        var movieToAdd = GetSampleMovie<ClientMovie>();
        movieToAdd.Id = MyIdGenerator(table?.TableName);
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
        await table!.InsertItemAsync(jsonDocument);
        await table!.PushItemsAsync();
        var response = await table!.GetItemAsync(movieToAdd.Id);

        // Assert
        Assert.Equal(MovieCount + 1, MovieServer.GetMovieCount());
        var insertedId = response.Value<string>("id");
        Assert.Equal(movieToAdd.Id, insertedId);

        var entity = MovieServer.GetMovieById(insertedId);
        Assert.Equal<IMovie>(movieToAdd, entity);
        AssertSystemPropertiesMatch(entity, response);

        if (useUpdatedAt)
            Assert.NotEqual("2018-12-31T01:01:01.000Z", response.Value<string>("updatedAt"));
        if (useVersion)
            Assert.NotEqual(movieToAdd.Version, response.Value<string>("version"));
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_Conflict()
    {
        await InitializeAsync(true);

        var movieToAdd = GetSampleMovie<ClientMovie>();
        movieToAdd.Id = GetRandomId();
        var jsonDocument = CreateJsonDocument(movieToAdd);

        // Act
        await Assert.ThrowsAsync<OfflineStoreException>(() => table!.InsertItemAsync(jsonDocument));
    }
}

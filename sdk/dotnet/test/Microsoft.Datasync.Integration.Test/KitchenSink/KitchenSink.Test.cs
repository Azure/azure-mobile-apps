// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using NSubstitute;
using Movies = Datasync.Common.Test.TestData.Movies;

namespace Microsoft.Datasync.Integration.Test.KitchenSink;

/// <summary>
/// A set of tests against the kitchen sink table - these are used for
/// specific client/server interactions where we can set up the entire
/// table alone.
/// </summary>
[ExcludeFromCodeCoverage]
public class KitchenSink_Tests : BaseOperationTest
{
    public KitchenSink_Tests(ITestOutputHelper logger) : base(logger)
    {
    }

    [Fact]
    public async Task KS1_NullRoundtrips()
    {
        // On client 1
        KitchenSinkDto client1dto = new() { StringValue = "This is a string", EnumOfNullableValue = KitchenSinkDtoState.Completed };
        await remoteTable.InsertItemAsync(client1dto);
        var remoteId = client1dto.Id;
        Assert.NotEmpty(remoteId);

        // On client 2
        await InitializeAsync();
        var pullQuery = offlineTable!.CreateQuery();
        await offlineTable!.PullItemsAsync(pullQuery, new PullOptions());
        var client2dto = await offlineTable.GetItemAsync(remoteId);
        Assert.NotNull(client2dto);
        Assert.Equal("This is a string", client2dto.StringValue);
        Assert.Equal(KitchenSinkDtoState.Completed, client2dto.EnumOfNullableValue);

        // Now update client 1
        client1dto = await remoteTable.GetItemAsync(remoteId);
        Assert.NotNull(client1dto);
        client1dto.StringValue = null;
        await remoteTable.ReplaceItemAsync(client1dto);

        // Finally, download the value on client 2
        await offlineTable!.PullItemsAsync();
        client2dto = await offlineTable.GetItemAsync(remoteId);
        Assert.NotNull(client2dto);
        // Issue 408 - cannot replace a string with a null.
        Assert.Null(client2dto.StringValue);

        Assert.NotNull(client2dto.EnumOfNullableValue);
    }

    [Fact]
    public async Task KS2_NullDoubleSearch()
    {
        // On client 1
        KitchenSinkDto client1dto = new() { NullableDouble = -1.0 };
        await remoteTable.InsertItemAsync(client1dto);
        var remoteId = client1dto.Id;
        Assert.NotEmpty(remoteId);

        // On client 2
        await InitializeAsync();
        var pullQuery = offlineTable!.CreateQuery();
        await offlineTable!.PullItemsAsync(pullQuery, new PullOptions());
        var client2dto = await offlineTable.GetItemAsync(remoteId);
        Assert.NotNull(client2dto);
        Assert.True(client2dto.NullableDouble < -0.5);

        // Finally, let's search!
        var elements = await offlineTable!.Where(x => x.NullableDouble < -0.5).ToListAsync();
        Assert.Single(elements);
        Assert.Equal(elements[0].Id, remoteId);
    }

    [Fact]
    public async Task KS3_DeferredTableDefinition()
    {
        var filename = Path.GetTempFileName();
        var connectionString = new UriBuilder(filename) { Query = "?mode=rwc" }.Uri.ToString();
        var store = new OfflineSQLiteStore(connectionString);
        var client = GetMovieClient(store: store);

        var table = client.GetOfflineTable<ClientMovie>("movies");

        var itemCount = await table.GetAsyncItems().CountAsync();
        Assert.Equal(0, itemCount);

        await table.PullItemsAsync();

        itemCount = await table.GetAsyncItems().CountAsync();
        Assert.Equal(Movies.Count, itemCount);
    }

    [Fact]
    public async Task KS4_WriteDeltaTokenInterval()
    {
        // On client 1
        for (int i = 1; i <= 50; i++)
        {
            KitchenSinkDto dto = new() { StringValue = $"String {i}", IntValue = i, EnumOfNullableValue = null, };
            await remoteTable.InsertItemAsync(dto);
        }

        // On client 2
        await InitializeAsync();
        var pullQuery = offlineTable!.CreateQuery();
        await offlineTable!.PullItemsAsync(pullQuery, new PullOptions() { WriteDeltaTokenInterval = 25 });
        // Make sure we have 50 values with IntValue > 0
        var entities = await offlineTable.Where(x => x.IntValue > 0).ToListAsync();
        Assert.Equal(50, entities.Count);
    }

    [Fact]
    public async Task KS5_CanSearchByEnum()
    {
        List<KitchenSinkDto> insertions = new()
        {
            new KitchenSinkDto { StringValue = "state=none", EnumValue = KitchenSinkDtoState.None },
            new KitchenSinkDto { StringValue = "state=completed", EnumValue = KitchenSinkDtoState.Completed },
            new KitchenSinkDto { StringValue = "state=failed", EnumValue = KitchenSinkDtoState.Failed }
        };
        foreach (var insertion in insertions)
        {
            await remoteTable.InsertItemAsync(insertion);
        }

        // Synchronize to offline table
        await InitializeAsync();
        var pullQuery = offlineTable!.CreateQuery();
        await offlineTable!.PullItemsAsync(pullQuery, new PullOptions());

        // Issue 610 - can we find the DTO that is completed?
        var items = await offlineTable!.Where(x => x.EnumValue == KitchenSinkDtoState.Completed).ToListAsync();
        Assert.Single(items);
        Assert.Equal("state=completed", items[0].StringValue);
    }

    [Theory]
    [InlineData("A & B")]
    [InlineData(@"A "" B")]
    [InlineData("A ' B")]
    [InlineData("A + B")]
    [InlineData("A / B")]
    public async Task KS6_SpecialCharacterOfflineWhere(string searchTarget)
    {
        var specialCharacters = @"!@#$%^&*)_-+={[}]|\:;""'<>,.?/".ToCharArray();
        foreach (var ch in specialCharacters)
        {
            var insertion = new KitchenSinkDto { StringValue = $"A {ch} B" };
            await remoteTable.InsertItemAsync(insertion);
        }

        // Synchronize to offline table
        await InitializeAsync();
        var pullQuery = offlineTable!.CreateQuery();
        await offlineTable!.PullItemsAsync(pullQuery, new PullOptions());

        // Check that all the data was stored
        var nItems = await offlineTable!.CountItemsAsync();
        Assert.Equal(specialCharacters.Length, nItems);

        var allItems = await offlineTable!.ToListAsync();

        // Issue 685 - can we find searches with specific characters
        var items = await offlineTable!.Where(x => x.StringValue.Equals(searchTarget)).ToListAsync();
        Assert.Single(items);
        Assert.Equal(searchTarget, items[0].StringValue);
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using Movies = Datasync.Common.Test.TestData.Movies;

namespace Microsoft.Datasync.Integration.Test.KitchenSink;

/// <summary>
/// A set of tests against the kitchen sink table - these are used for
/// specific client/server interactions where we can set up the entire
/// table alone.
/// </summary>
[ExcludeFromCodeCoverage]
public class KitchenSink_ContractResolver_Tests : BaseOperationContractResolverTest
{
    public KitchenSink_ContractResolver_Tests(ITestOutputHelper logger) : base(logger)
    {

    }

    [Fact]
    public async Task PascalCase_GetItemAsync()
    {
        // On client 1
        await InitializeAsync();
        KitchenSinkDto insertDto = new() { StringValue = "This is a string", EnumOfNullableValue = KitchenSinkDtoState.Completed };
        await offlineTable!.InsertItemAsync(insertDto);
        var offlineInsertId = insertDto.Id;
        Assert.NotEmpty(offlineInsertId);

        var pullQuery = offlineTable!.CreateQuery();
        await offlineTable!.PullItemsAsync(pullQuery, new PullOptions());
        var queryResultDto = await offlineTable.GetItemAsync(offlineInsertId);
        Assert.NotNull(queryResultDto);
        Assert.Equal(offlineInsertId, queryResultDto.Id);
        Assert.Equal("This is a string", queryResultDto.StringValue);
        Assert.Equal(KitchenSinkDtoState.Completed, queryResultDto.EnumOfNullableValue);
    }

    [Fact]
    public async Task PascalCase_ReplaceItemAsync()
    {
        await InitializeAsync();
        KitchenSinkDto insertDto = new() { StringValue = "This is a string", EnumOfNullableValue = KitchenSinkDtoState.Completed };
        await offlineTable!.InsertItemAsync(insertDto);
        var itemId = insertDto.Id;

        var pullQuery = offlineTable!.CreateQuery();
        await offlineTable!.PullItemsAsync(pullQuery, new PullOptions());
        var queryResultDto = await offlineTable.GetItemAsync(itemId);
        Assert.NotNull(queryResultDto);

        queryResultDto.StringValue = "This is the new string";
        await offlineTable!.ReplaceItemAsync(queryResultDto);
        await offlineTable!.PullItemsAsync(pullQuery, new PullOptions());

        var resultDto = await offlineTable!.GetItemAsync(itemId);
        Assert.Equal("This is the new string", resultDto.StringValue);
        Assert.NotEqual(queryResultDto.Version, resultDto.Version);
    }
}

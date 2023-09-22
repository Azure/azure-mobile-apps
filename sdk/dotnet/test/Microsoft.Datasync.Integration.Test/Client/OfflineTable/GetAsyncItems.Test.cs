// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTable;

[ExcludeFromCodeCoverage]
public class GetAsyncItems_Tests : BaseOperationTest
{
    public GetAsyncItems_Tests(ITestOutputHelper logger) : base(logger) { }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_RetrievesItems()
    {
        await InitializeAsync(true);

        // Arrange
        int count = 0;

        var enumerable = table!.GetAsyncItems("");
        Assert.NotNull(enumerable);

        var enumerator = enumerable!.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync())
        {
            count++;
            var item = enumerator.Current;
            var itemId = item.Value<string>("id");

            Assert.NotNull(item);
            Assert.NotNull(itemId);

            var expected = MovieServer.GetMovieById(itemId);
            AssertVersionMatches(expected.Version, item.Value<string>("version")!);
        }

        Assert.Equal(MovieCount, count);
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable;

[ExcludeFromCodeCoverage]
public class GetAsyncItems_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_RetrievesItems()
    {
        // Arrange
        int count = 0;

        var pageable = table.GetAsyncItems("$count=true") as AsyncPageable<JToken>;
        Assert.NotNull(pageable);

        var enumerator = pageable!.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            count++;
            var item = enumerator.Current;
            var itemId = item.Value<string>("id");

            Assert.NotNull(item);
            Assert.NotNull(itemId);

            var expected = MovieServer.GetMovieById(itemId);
            AssertVersionMatches(expected.Version, item.Value<string>("version")!);

            Assert.Equal(MovieCount, pageable.Count);
        }

        Assert.Equal(MovieCount, count);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_AsPages_RetrievesItems()
    {
        // Arrange
        int itemCount = 0, pageCount = 0;

        var pageable = (table.GetAsyncItems("$count=true") as AsyncPageable<JToken>)!.AsPages();
        Assert.NotNull(pageable);

        var enumerator = pageable.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            pageCount++;

            var page = enumerator.Current;
            Assert.NotNull(page);
            foreach (var item in page.Items)
            {
                itemCount++;
                var itemId = item.Value<string>("id");
                Assert.NotNull(itemId);
                var expected = MovieServer.GetMovieById(itemId);
                AssertVersionMatches(expected.Version, item.Value<string>("version")!);
            }
        }

        Assert.Equal(MovieCount, itemCount);
        Assert.Equal(3, pageCount);
    }
}

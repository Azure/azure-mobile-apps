// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    [Collection("Integration")]
    public class RemoteTable_GetItems_Tests : BaseTest
    {
        private readonly DatasyncClient client;
        private readonly IRemoteTable table;

        public RemoteTable_GetItems_Tests()
        {
            client = GetMovieClient();
            table = client.GetRemoteTable("movies");
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_RetrievesItems()
        {
            // Arrange
            int count = 0;

            var pageable = table.GetAsyncItems("$count=true");
            Assert.NotNull(pageable);

            var enumerator = pageable.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                count++;
                var item = enumerator.Current;
                var itemId = item.GetId();

                Assert.NotNull(item);
                Assert.NotNull(itemId);

                var expected = MovieServer.GetMovieById(itemId);
                AssertEx.JsonDocumentMatches(expected, item);

                Assert.Equal(MovieCount, pageable.Count);
                Assert.NotNull(pageable.CurrentResponse);
            }

            Assert.Equal(MovieCount, count);
        }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_AsPages_RetrievesItems()
        {
            // Arrange
            int itemCount = 0, pageCount = 0;

            var pageable = table.GetAsyncItems().AsPages();
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
                    var itemId = item.GetId();
                    Assert.NotNull(itemId);
                    var expected = MovieServer.GetMovieById(itemId);
                    AssertEx.JsonDocumentMatches(expected, item);
                }
            }

            Assert.Equal(MovieCount, itemCount);
            Assert.Equal(3, pageCount);
        }
    }
}

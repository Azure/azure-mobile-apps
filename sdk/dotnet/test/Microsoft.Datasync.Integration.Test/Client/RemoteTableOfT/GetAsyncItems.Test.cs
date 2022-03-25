// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTableOfT
{
    [ExcludeFromCodeCoverage]
    public class GetAsyncItems_Tests : BaseOperationTest
    {
        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_RetrievesItems()
        {
            // Arrange
            int count = 0;

            var pageable = table.GetAsyncItems<ClientMovie>("$count=true") as AsyncPageable<ClientMovie>;
            Assert.NotNull(pageable);

            var enumerator = pageable!.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                count++;
                var item = enumerator.Current;

                Assert.NotNull(item);
                Assert.NotNull(item.Id);

                var expected = MovieServer.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);

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

            var pageable = (table.GetAsyncItems<ClientMovie>("") as AsyncPageable<ClientMovie>)!.AsPages();
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
                    Assert.NotNull(item.Id);
                    var expected = MovieServer.GetMovieById(item.Id);
                    Assert.Equal<IMovie>(expected, item);
                }
            }

            Assert.Equal(MovieCount, itemCount);
            Assert.Equal(3, pageCount);
        }

        [Fact]
        [Trait("Method", "ToAsyncEnumerable")]
        public async Task ToAsyncEnumerable_RetrievesItems()
        {
            // Arrange
            int count = 0;

            var result = table.ToAsyncEnumerable();
            var enumerator = result.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                count++;
                var item = enumerator.Current;

                Assert.NotNull(item);
                Assert.NotNull(item.Id);

                var expected = MovieServer.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);
            }

            Assert.Equal(MovieCount, count);
        }
        
        [Theory]
        [ClassData(typeof(LinqTestCases))]
        [Trait("Method", "ToAsyncEnumerable")]
        public async Task ToAsyncEnumerable_WithLiveServer(LinqTestCase testCase)
        {
            // Arrange
            var query = new TableQuery<ClientMovie>(table as RemoteTable<ClientMovie>);

            // Act
            var pageable = testCase.LinqExpression.Invoke(query).ToAsyncEnumerable();
            var list = await pageable.ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(testCase.ResultCount, list.Count);
            var actualItems = list.Take(testCase.FirstResults.Length).Select(m => m.Id).ToArray();
            Assert.Equal(testCase.FirstResults, actualItems);
        }
    }
}

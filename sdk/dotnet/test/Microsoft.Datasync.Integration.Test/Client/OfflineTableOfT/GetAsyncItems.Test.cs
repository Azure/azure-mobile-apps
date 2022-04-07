// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Table;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTableOfT
{
    [ExcludeFromCodeCoverage]
    public class GetAsyncItems_Tests : BaseOperationTest
    {
        public GetAsyncItems_Tests(ITestOutputHelper logger) : base(logger, false) { }

        [Fact]
        [Trait("Method", "GetAsyncItems")]
        public async Task GetAsyncItems_RetrievesItems()
        {
            await InitializeAsync();

            // Arrange
            int count = 0;

            var query = table!.CreateQuery().IncludeTotalCount(true);
            var pageable = table!.GetAsyncItems(query) as AsyncPageable<ClientMovie>;
            Assert.NotNull(pageable);

            var enumerator = pageable!.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
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
        public async Task GetAsyncItems_NoArgs_RetrievesItems()
        {
            await InitializeAsync();

            // Arrange
            int count = 0;

            var pageable = table!.GetAsyncItems() as AsyncPageable<ClientMovie>;
            Assert.NotNull(pageable);

            var enumerator = pageable!.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
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

        [Fact]
        [Trait("Method", "ToAsyncEnumerable")]
        public async Task ToAsyncEnumerable_RetrievesItems()
        {
            await InitializeAsync();

            // Arrange
            int count = 0;

            var result = table!.ToAsyncEnumerable();
            var enumerator = result.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
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
            await InitializeAsync();

            // Arrange
            var query = table!.CreateQuery();

            // Act
            var pageable = testCase.LinqExpression.Invoke(query).ToAsyncEnumerable();
            var list = await pageable.ToListAsync();

            // Assert
            Assert.Equal(testCase.ResultCount, list.Count);
            var actualItems = list.Take(testCase.FirstResults.Length).Select(m => m.Id).ToArray();
            Assert.Equal(testCase.FirstResults, actualItems);
        }
    }
}

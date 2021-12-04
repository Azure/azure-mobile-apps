// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Commands;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client
{
    public class DatasyncTableQuery_Tests : BaseTest
    {
        [Theory]
        [ClassData(typeof(LinqTestCases))]
        [Trait("Method", "ToODataQueryString")]
        public async Task ToLazyObservableCollection_WithPageCount_WithLinq(LinqTestCase testcase)
        {
            // Arrange
            var client = GetMovieClient();
            var table = client.GetTable<ClientMovie>("movies");
            int loops = 0;
            int maxLoops = (MovieCount / 50) + 2;
            var query = new DatasyncTableQuery<ClientMovie>(table);

            // Act
            var sut = (testcase.LinqExpression.Invoke(query) as DatasyncTableQuery<ClientMovie>)?.ToLazyObservableCollection(50) as InternalLazyObservableCollection<ClientMovie>;
            var loadMore = sut!.LoadMoreCommand as IAsyncCommand;
            await WaitUntil(() => !sut.IsBusy).ConfigureAwait(false);
            while (loops < maxLoops && sut.HasMoreItems)
            {
                loops++;
                await loadMore!.ExecuteAsync().ConfigureAwait(false);
            }

            // Do one more load to make sure.
            await loadMore!.ExecuteAsync().ConfigureAwait(false);

            Assert.Equal(testcase.ResultCount, sut.Count);
            Assert.False(sut.HasMoreItems);
        }

        [Theory]
        [ClassData(typeof(LinqTestCases))]
        [Trait("Method", "ToAsyncPageable")]
        public async Task ToAsyncPageable_WithLiveServer(LinqTestCase testcase)
        {
            // Arrange
            var client = GetMovieClient();
            var table = client.GetTable<ClientMovie>("movies");
            var query = new DatasyncTableQuery<ClientMovie>(table as DatasyncTable<ClientMovie>);

            // Act
            var pageable = testcase.LinqExpression.Invoke(query).ToAsyncPageable();
            var list = await pageable.ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(testcase.ResultCount, list.Count);
            var actualItems = list.Take(testcase.FirstResults.Length).Select(m => m.Id).ToArray();
            Assert.Equal(testcase.FirstResults, actualItems);
        }


        [Theory]
        [ClassData(typeof(LinqTestCases))]
        [Trait("Method", "ToAsyncEnumerable")]
        internal async Task ToAsyncEnumerable_WithLiveServer(LinqTestCase testcase)
        {
            // Arrange
            var client = GetMovieClient();
            var table = client.GetTable<ClientMovie>("movies");
            var query = new DatasyncTableQuery<ClientMovie>(table as DatasyncTable<ClientMovie>);

            // Act
            var pageable = testcase.LinqExpression.Invoke(query).ToAsyncEnumerable();
            var list = await pageable.ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(testcase.ResultCount, list.Count);
            var actualItems = list.Take(testcase.FirstResults.Length).Select(m => m.Id).ToArray();
            Assert.Equal(testcase.FirstResults, actualItems);
        }
    }
}

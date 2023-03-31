// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTableOfT;

[ExcludeFromCodeCoverage]
public class CountItemsAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "CountItemsAsync")]
    public async Task CountItemsAsync_RetrievesCount()
    {
        // Act
        long count = await table!.CountItemsAsync();

        // Assert
        Assert.Equal(MovieCount, count);
    }

    #region LINQ tests
    /*
     * We don't run all the LINQ tests because we aren't actually testing the LINQ conversion here.
     */
    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_062()
    {
        await RunLinqTest(
            m => m.Where(x => x.Year > 2000 || x.Year < 1940),
            78
        );
    }

    [Fact]
    public async Task Linq_sync_1()
    {
        var dt = new DateTimeOffset(1994, 10, 14, 0, 0, 0, TimeSpan.Zero);
        await RunLinqTest(
            m => m.Where(x => x.UpdatedAt > dt).IncludeDeletedItems().OrderBy(x => x.UpdatedAt).IncludeTotalCount().Skip(25),
            248 /* Note that skip is ignored in the LINQ for Count; would be 223 otherwise */
        );
    }
    #endregion

    private async Task RunLinqTest(Func<ITableQuery<ClientMovie>, ITableQuery<ClientMovie>> linqExpression, int resultCount)
    {
        // Arrange
        var query = linqExpression.Invoke(table!.CreateQuery());

        // Act
        var count = await table.CountItemsAsync(query);
        var linqCount = await query.LongCountAsync();

        // Assert
        Assert.Equal(resultCount, count);
        Assert.Equal(resultCount, linqCount);
    }
}

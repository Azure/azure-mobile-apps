// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTableOfT;

[ExcludeFromCodeCoverage]
public class LinqOperation_Tests : BaseOperationTest
{
    public LinqOperation_Tests(ITestOutputHelper logger) : base(logger, false) { }

    [Fact]
    [Trait("Method", "ToArray")]
    public async Task ToArray_Test()
    {
        //Arrange
        await InitializeAsync();

        // Act
        var array = await table!.ToArrayAsync();

        // Assert
        Assert.NotNull(array);
        Assert.Equal(MovieCount, array.Length);
        foreach (var item in array)
        {
            Assert.NotNull(item.Id);
            var expected = MovieServer.GetMovieById(item.Id);
            Assert.Equal<IMovie>(expected, item);
        }
    }

    [Fact]
    [Trait("Method", "ToDictionary")]
    public async Task ToDictionary_Test()
    {
        //Arrange
        await InitializeAsync();

        // Act
        var dict = await table!.ToDictionaryAsync(m => m.Id);

        // Assert
        Assert.NotNull(dict);
        Assert.Equal(MovieCount, dict.Count);
        foreach (var item in dict)
        {
            Assert.NotNull(item.Key);
            var expected = MovieServer.GetMovieById(item.Key);
            Assert.Equal<IMovie>(expected, item.Value);
        }
    }

    [Fact]
    [Trait("Method", "ToEnumerable")]
    public async Task ToEnumerable_Test()
    {
        //Arrange
        await InitializeAsync();

        // Act
        var enumerable = table!.ToEnumerable();

        // Assert
        Assert.NotNull(enumerable);
        var count = 0;
        foreach (var item in enumerable)
        {
            count++;
            Assert.NotNull(item.Id);
            var expected = MovieServer.GetMovieById(item.Id);
            Assert.Equal<IMovie>(expected, item);
        }
        Assert.Equal(MovieCount, count);
    }

    [Fact]
    [Trait("Method", "ToHashSet")]
    public async Task ToHashSet_Test()
    {
        //Arrange
        await InitializeAsync();

        // Act
        var set = await table!.ToHashSetAsync();

        // Assert
        Assert.NotNull(set);
        foreach (var item in set)
        {
            Assert.NotNull(item.Id);
            var expected = MovieServer.GetMovieById(item.Id);
            Assert.Equal<IMovie>(expected, item);
        }
        Assert.Equal(MovieCount, set.Count);
    }

    [Fact]
    [Trait("Method", "ToList")]
    public async Task ToList_Test()
    {
        //Arrange
        await InitializeAsync();

        // Act
        var set = await table!.ToListAsync();

        // Assert
        Assert.NotNull(set);
        foreach (var item in set)
        {
            Assert.NotNull(item.Id);
            var expected = MovieServer.GetMovieById(item.Id);
            Assert.Equal<IMovie>(expected, item);
        }
        Assert.Equal(MovieCount, set.Count);
    }

    [Fact]
    [Trait("Method", "ToObservableCollection")]
    public async Task ToObservableCollection_Test_1()
    {
        //Arrange
        await InitializeAsync();

        // Act
        var oc = await table!.ToObservableCollection();

        // Assert
        Assert.NotNull(oc);
        foreach (var item in oc)
        {
            Assert.NotNull(item.Id);
            var expected = MovieServer.GetMovieById(item.Id);
            Assert.Equal<IMovie>(expected, item);
        }
        Assert.Equal(MovieCount, oc.Count);
    }

    [Fact]
    [Trait("Method", "ToObservableCollection")]
    public async Task ToObservableCollection_Test_2()
    {
        //Arrange
        await InitializeAsync();

        // Act
        var oc = new ConcurrentObservableCollection<ClientMovie>();
        await table!.ToObservableCollection(oc);

        // Assert
        Assert.NotNull(oc);
        foreach (var item in oc)
        {
            Assert.NotNull(item.Id);
            var expected = MovieServer.GetMovieById(item.Id);
            Assert.Equal<IMovie>(expected, item);
        }
        Assert.Equal(MovieCount, oc.Count);
    }

    [Fact]
    [Trait("Method", "Select")]
    public async Task Select_Test_1()
    {
        // Arrange
        await InitializeAsync();
        var movieId = GetRandomId();
        var expectedMovie = MovieServer.GetMovieById(movieId);

        // Act
        var items = await table!.Where(x => x.Id == movieId).Select(x => new { x.Title }).ToListAsync();

        // Assert
        Assert.Single(items);
    }
}

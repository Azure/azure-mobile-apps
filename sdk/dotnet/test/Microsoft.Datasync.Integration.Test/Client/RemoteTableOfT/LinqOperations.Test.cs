// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTableOfT;

[ExcludeFromCodeCoverage]
public class LinqOperation_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "ToArray")]
    public async Task ToArray_Test()
    {
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
    public void ToEnumerable_Test()
    {
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
}

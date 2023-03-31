// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;

using TestData = Datasync.Common.Test.TestData;

namespace Microsoft.Datasync.Client.Test;

[ExcludeFromCodeCoverage]
public class ConcurrentObservableCollection_Tests : BaseTest
{
    private readonly ConcurrentObservableCollection<EFMovie> movies;
    private int collectionChangedCallCount = 0;

    public ConcurrentObservableCollection_Tests()
    {
        var seed = TestData.Movies.OfType<EFMovie>().ToList();
        movies = new ConcurrentObservableCollection<EFMovie>(seed);
        movies.CollectionChanged += (sender, e) => collectionChangedCallCount++;
    }

    [Fact]
    public void ReplaceAll_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => movies.ReplaceAll(null));
    }

    [Fact]
    public void ReplaceAll_Empty_Clears()
    {
        movies.ReplaceAll(new List<EFMovie>());
        Assert.Empty(movies);
        Assert.Equal(1, collectionChangedCallCount);
    }

    [Fact]
    public void ReplaceAll_List_SetsList()
    {
        var data = TestData.Movies.OfType<EFMovie>().Take(10).ToList();
        movies.ReplaceAll(data);
        Assert.Equal(10, movies.Count);
        Assert.Equal(1, collectionChangedCallCount);
    }

    [Fact]
    public void AddRange_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => movies.AddRange(null));
    }

    [Fact]
    public void AddRange_Empty_ReturnsFalse()
    {
        bool actual = movies.AddRange(new List<EFMovie>());
        Assert.False(actual);
        Assert.Equal(0, collectionChangedCallCount);
    }

    [Fact]
    public void AddRange_Sequence_ReturnsTrue()
    {
        var d1 = TestData.Movies.OfType<EFMovie>().Take(10).ToList();
        var d2 = TestData.Movies.OfType<EFMovie>().Skip(10).Take(10).ToList();
        movies.ReplaceAll(d1);

        bool actual = movies.AddRange(d2);
        Assert.True(actual);
        Assert.Equal(20, movies.Count);
        Assert.Equal(2, collectionChangedCallCount);
    }

    [Fact]
    public void AddIfMissing_Null_Throws()
    {
        EFMovie item = movies.First();
        Assert.Throws<ArgumentNullException>(() => movies.AddIfMissing(null, item));
        Assert.Throws<ArgumentNullException>(() => movies.AddIfMissing(t => t.Id == item.Id, null));
    }

    [Fact]
    public void AddIfMissing_False_IfMatch()
    {
        EFMovie item = new() { Id = "foo" };
        int cnt = movies.Count;
        bool actual = movies.AddIfMissing(t => t.Id == "id-001", item);
        Assert.False(actual);
        Assert.Equal(cnt, movies.Count);
    }

    [Fact]
    public void AddIfMissing_True_IfNoMatch()
    {
        EFMovie item = new() { Id = "foo" };
        int cnt = movies.Count;
        bool actual = movies.AddIfMissing(t => t.Id == item.Id, item);
        Assert.True(actual);
        Assert.Equal(cnt + 1, movies.Count);
    }

    [Fact]
    public void RemoveIf_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => movies.RemoveIf(null));
    }

    [Fact]
    public void RemoveIf_NotMatched_False()
    {
        var cnt = movies.Count;
        var actual = movies.RemoveIf(t => t.Id == "foo");
        Assert.False(actual);
        Assert.Equal(cnt, movies.Count);
    }

    [Fact]
    public void RemoveIf_Matched_True()
    {
        var cnt = movies.Count;
        var actual = movies.RemoveIf(t => t.Id == "id-001");
        Assert.True(actual);
        Assert.Equal(cnt - 1, movies.Count);
    }

    [Fact]
    public void ReplaceIf_Null_Throws()
    {
        EFMovie item = movies.First();
        Assert.Throws<ArgumentNullException>(() => movies.ReplaceIf(null, item));
        Assert.Throws<ArgumentNullException>(() => movies.ReplaceIf(t => t.Id == item.Id, null));
    }

    [Fact]
    public void ReplaceIf_True_IfMatch()
    {
        EFMovie item = new() { Id = "foo" };
        int cnt = movies.Count;
        bool actual = movies.ReplaceIf(t => t.Id == "id-001", item);
        Assert.True(actual);
        Assert.Equal(cnt, movies.Count);
        Assert.Contains(item, movies);
    }

    [Fact]
    public void ReplaceIf_False_IfNoMatch()
    {
        EFMovie item = new() { Id = "foo" };
        int cnt = movies.Count;
        bool actual = movies.ReplaceIf(t => t.Id == item.Id, item);
        Assert.False(actual);
        Assert.Equal(cnt, movies.Count);
        Assert.DoesNotContain(item, movies);
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.TestData;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;

// These are explicit tests in the set.
#pragma warning disable RCS1033 // Remove redundant boolean literal.
#pragma warning disable RCS1049 // Simplify boolean comparison.
#pragma warning disable RCS1068 // Simplify logical negation.
#pragma warning disable RCS1155 // Use StringComparison when comparing strings.

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTableOfT;

[ExcludeFromCodeCoverage]
[Collection("RemoteServiceCollection")]
public class GetAsyncItems_Tests
{
    private readonly RemoteServiceFixture fixture;

    public GetAsyncItems_Tests(RemoteServiceFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_RetrievesItems()
    {
        // Arrange
        int count = 0;

        var pageable = fixture.MovieTable.GetAsyncItems<ClientMovie>("$count=true") as AsyncPageable<ClientMovie>;
        Assert.NotNull(pageable);

        var enumerator = pageable!.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            count++;
            var item = enumerator.Current;

            Assert.NotNull(item);
            Assert.NotNull(item.Id);

            var expected = fixture.MovieServer.GetMovieById(item.Id);
            Assert.Equal<IMovie>(expected, item);

            Assert.Equal(fixture.MovieCount, pageable.Count);
        }

        Assert.Equal(fixture.MovieCount, count);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_NoArgs_RetrievesItems()
    {
        // Arrange
        int count = 0;

        var pageable = fixture.MovieTable.GetAsyncItems() as AsyncPageable<ClientMovie>;
        Assert.NotNull(pageable);

        var enumerator = pageable!.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            count++;
            var item = enumerator.Current;

            Assert.NotNull(item);
            Assert.NotNull(item.Id);

            var expected = fixture.MovieServer.GetMovieById(item.Id);
            Assert.Equal<IMovie>(expected, item);
        }

        Assert.Equal(fixture.MovieCount, count);
    }

    [Fact]
    [Trait("Method", "GetAsyncItems")]
    public async Task GetAsyncItems_AsPages_RetrievesItems()
    {
        // Arrange
        int itemCount = 0, pageCount = 0;

        var pageable = (fixture.MovieTable.GetAsyncItems<ClientMovie>("") as AsyncPageable<ClientMovie>)!.AsPages();
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
                var expected = fixture.MovieServer.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);
            }
        }

        Assert.Equal(fixture.MovieCount, itemCount);
        Assert.Equal(3, pageCount);
    }

    [Fact]
    [Trait("Method", "ToAsyncEnumerable")]
    public async Task ToAsyncEnumerable_RetrievesItems()
    {
        // Arrange
        int count = 0;

        var result = fixture.MovieTable.ToAsyncEnumerable();
        var enumerator = result.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            count++;
            var item = enumerator.Current;

            Assert.NotNull(item);
            Assert.NotNull(item.Id);

            var expected = fixture.MovieServer.GetMovieById(item.Id);
            Assert.Equal<IMovie>(expected, item);
        }

        Assert.Equal(fixture.MovieCount, count);
    }

    #region LINQ tests against Remote Database
    /*
     * MAKE SURE YOU ADD YOUR LINQ BASED TEST TO BOTH OFFLINE AND REMOTE TEST SECTIONS
     */
    [Fact]
    public async Task ToAsyncEnumerable_Linq_base_1()
    {
        await RunLinqTest(
            m => m,
            Movies.Count,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_count_1()
    {
        await RunLinqTest(
            m => m.IncludeTotalCount(),
            Movies.Count,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_count_2()
    {
        await RunLinqTest(
            m => m.IncludeTotalCount(true),
            Movies.Count,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_count_3()
    {
        await RunLinqTest(
            m => m.IncludeTotalCount(false),
            Movies.Count,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_deleted_1()
    {
        await RunLinqTest(
            m => m.IncludeDeletedItems(),
            Movies.Count,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_deleted_2()
    {
        await RunLinqTest(
            m => m.IncludeDeletedItems(true),
            Movies.Count,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_deleted_3()
    {
        await RunLinqTest(
            m => m.IncludeDeletedItems(false),
            Movies.Count,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_orderby_1()
    {
        await RunLinqTest(
            m => m.OrderBy(x => x.BestPictureWinner),
            Movies.Count,
            new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_orderby_2()
    {
        await RunLinqTest(
            m => m.OrderBy(x => x.Duration),
            Movies.Count,
            new string[] { "id-227", "id-125", "id-133", "id-107", "id-118" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_orderby_3()
    {
        await RunLinqTest(
            m => m.OrderBy(x => x.Rating),
            Movies.Count,
            new string[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_orderby_4()
    {
        await RunLinqTest(
            m => m.OrderBy(x => x.Title),
            Movies.Count,
            new string[] { "id-005", "id-091", "id-243", "id-194", "id-060" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_orderby_5()
    {
        await RunLinqTest(
            m => m.OrderBy(x => x.ReleaseDate),
            Movies.Count,
            new string[] { "id-125", "id-133", "id-227", "id-118", "id-088" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_orderbydesc_1()
    {
        await RunLinqTest(
            m => m.OrderByDescending(x => x.BestPictureWinner),
            Movies.Count,
            new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_orderbydesc_2()
    {
        await RunLinqTest(
            m => m.OrderByDescending(x => x.Duration),
            Movies.Count,
            new string[] { "id-153", "id-065", "id-165", "id-008", "id-002" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_orderbydesc_3()
    {
        await RunLinqTest(
            m => m.OrderByDescending(x => x.Rating),
            Movies.Count,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_orderbydesc_4()
    {
        await RunLinqTest(
            m => m.OrderByDescending(x => x.Title),
            Movies.Count,
            new string[] { "id-107", "id-100", "id-123", "id-190", "id-149" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_orderbydesc_5()
    {
        await RunLinqTest(
            m => m.OrderByDescending(x => x.ReleaseDate),
            Movies.Count,
            new string[] { "id-188", "id-033", "id-122", "id-186", "id-064" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_thenby_1()
    {
        await RunLinqTest(
            m => m.ThenBy(x => x.BestPictureWinner),
            Movies.Count,
            new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_thenby_2()
    {
        await RunLinqTest(
            m => m.ThenBy(x => x.Duration),
            Movies.Count,
            new string[] { "id-227", "id-125", "id-133", "id-107", "id-118" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_thenby_3()
    {
        await RunLinqTest(
            m => m.ThenBy(x => x.Rating),
            Movies.Count,
            new string[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_thenby_4()
    {
        await RunLinqTest(
            m => m.ThenBy(x => x.Title),
            Movies.Count,
            new string[] { "id-005", "id-091", "id-243", "id-194", "id-060" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_thenby_5()
    {
        await RunLinqTest(
            m => m.ThenBy(x => x.ReleaseDate),
            Movies.Count,
            new string[] { "id-125", "id-133", "id-227", "id-118", "id-088" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_thenbydesc_1()
    {
        await RunLinqTest(
            m => m.ThenByDescending(x => x.BestPictureWinner),
            Movies.Count,
            new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_thenbydesc_2()
    {
        await RunLinqTest(
            m => m.ThenByDescending(x => x.Duration),
            Movies.Count,
            new string[] { "id-153", "id-065", "id-165", "id-008", "id-002" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_thenbydesc_3()
    {
        await RunLinqTest(
            m => m.ThenByDescending(x => x.Rating),
            Movies.Count,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_thenbydesc_4()
    {
        await RunLinqTest(
            m => m.ThenByDescending(x => x.Title),
            Movies.Count,
            new string[] { "id-107", "id-100", "id-123", "id-190", "id-149" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_thenbydesc_5()
    {
        await RunLinqTest(
            m => m.ThenByDescending(x => x.ReleaseDate),
            Movies.Count,
            new string[] { "id-188", "id-033", "id-122", "id-186", "id-064" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_ordering_1()
    {
        await RunLinqTest(
            m => m.OrderBy(x => x.Year).ThenBy(x => x.Rating),
            Movies.Count,
            new string[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_ordering_2()
    {
        await RunLinqTest(
            m => m.OrderBy(x => x.Year).ThenByDescending(x => x.Title),
            Movies.Count,
            new string[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_ordering_3()
    {
        await RunLinqTest(
            m => m.OrderByDescending(x => x.Year).ThenBy(x => x.Rating),
            Movies.Count,
            new string[] { "id-033", "id-122", "id-188", "id-102", "id-149" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_ordering_4()
    {
        await RunLinqTest(
            m => m.OrderByDescending(x => x.Rating).ThenByDescending(x => x.Title),
            Movies.Count,
            new string[] { "id-107", "id-160", "id-092", "id-176", "id-147" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_ordering_5()
    {
        await RunLinqTest(
            m => m.OrderBy(x => x.UpdatedAt),
            Movies.Count,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_skip_1()
    {
        await RunLinqTest(
            m => m.Skip(100),
            Movies.Count - 100,
            new string[] { "id-100", "id-101", "id-102", "id-103", "id-104" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_skip_2()
    {
        await RunLinqTest(
            m => m.Skip(200),
            Movies.Count - 200,
            new string[] { "id-200", "id-201", "id-202", "id-203", "id-204" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_take_1()
    {
        await RunLinqTest(
            m => m.Take(100),
            100,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_take_2()
    {
        await RunLinqTest(
            m => m.Take(200),
            200,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_skiptake_1()
    {
        await RunLinqTest(
            m => m.Skip(100).Take(50),
            50,
            new string[] { "id-100", "id-101", "id-102", "id-103", "id-104" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_skiptake_2()
    {
        await RunLinqTest(
            m => m.Skip(200).Take(25),
            25,
            new string[] { "id-200", "id-201", "id-202", "id-203", "id-204" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_001()
    {
        await RunLinqTest(
            m => m.Where(x => x.BestPictureWinner),
            38,
            new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_002()
    {
        await RunLinqTest(
            m => m.Where(x => !x.BestPictureWinner),
            210,
            new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_003()
    {
        await RunLinqTest(
            m => m.Where(x => x.BestPictureWinner == true),
            38,
            new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_004()
    {
        await RunLinqTest(
            m => m.Where(x => x.BestPictureWinner == false),
            210,
            new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_005()
    {
        await RunLinqTest(
            m => m.Where(x => x.BestPictureWinner != true),
            210,
            new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_006()
    {
        await RunLinqTest(
            m => m.Where(x => x.BestPictureWinner != false),
            38,
            new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_007()
    {
        await RunLinqTest(
            m => m.Where(x => !(x.BestPictureWinner == true)),
            210,
            new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_008()
    {
        await RunLinqTest(
            m => m.Where(x => !(x.BestPictureWinner != true)),
            38,
            new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_009()
    {
        await RunLinqTest(
            m => m.Where(x => !(x.BestPictureWinner == false)),
            38,
            new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_010()
    {
        await RunLinqTest(
            m => m.Where(x => !(x.BestPictureWinner != false)),
            210,
            new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_011()
    {
        await RunLinqTest(
            m => m.Where(x => x.Duration == 100),
            3,
            new string[] { "id-116", "id-159", "id-186" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_012()
    {
        await RunLinqTest(
            m => m.Where(x => x.Duration < 100),
            44,
            new string[] { "id-005", "id-037", "id-041", "id-044", "id-054" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_013()
    {
        await RunLinqTest(
            m => m.Where(x => x.Duration <= 100),
            47,
            new string[] { "id-005", "id-037", "id-041", "id-044", "id-054" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_014()
    {
        await RunLinqTest(
            m => m.Where(x => x.Duration > 90),
            227,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_015()
    {
        await RunLinqTest(
            m => m.Where(x => x.Duration >= 90),
            227,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_016()
    {
        await RunLinqTest(
            m => m.Where(x => x.Duration != 100),
            245,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_017()
    {
        await RunLinqTest(
            m => m.Where(x => !(x.Duration == 100)),
            245,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_018()
    {
        await RunLinqTest(
            m => m.Where(x => !(x.Duration < 100)),
            204,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_019()
    {
        await RunLinqTest(
            m => m.Where(x => !(x.Duration <= 100)),
            201,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_020()
    {
        await RunLinqTest(
            m => m.Where(x => !(x.Duration > 90)),
            21,
            new string[] { "id-041", "id-044", "id-054", "id-079", "id-089" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_021()
    {
        await RunLinqTest(
            m => m.Where(x => !(x.Duration >= 90)),
            21,
            new string[] { "id-041", "id-044", "id-054", "id-079", "id-089" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_022()
    {
        await RunLinqTest(
            m => m.Where(x => !(x.Duration != 100)),
            3,
            new string[] { "id-116", "id-159", "id-186" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_031()
    {
        var dt = new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc);
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate == dt),
            2,
            new string[] { "id-000", "id-003" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_032()
    {
        var dt = new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate > dt),
            69,
            new string[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_033()
    {
        var dt = new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate >= dt),
            69,
            new string[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_034()
    {
        var dt = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate < dt),
            179,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_035()
    {
        var dt = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate <= dt),
            179,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_036()
    {
        var dt = new DateTimeOffset(1994, 10, 14, 0, 0, 0, TimeSpan.Zero);
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate == dt),
            2,
            new string[] { "id-000", "id-003" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_037()
    {
        var dt = new DateTimeOffset(1999, 12, 31, 0, 0, 0, TimeSpan.Zero);
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate > dt),
            69,
            new string[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_038()
    {
        var dt = new DateTimeOffset(1999, 12, 31, 0, 0, 0, TimeSpan.Zero);
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate >= dt),
            69,
            new string[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_039()
    {
        var dt = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate < dt),
            179,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_040()
    {
        var dt = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate <= dt),
            179,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_050()
    {
        await RunLinqTest(
            m => m.Where(x => x.Title == "The Godfather"),
            1,
            new string[] { "id-001" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_051()
    {
        await RunLinqTest(
            m => m.Where(x => x.Title != "The Godfather"),
            247,
            new string[] { "id-000", "id-002", "id-003", "id-004", "id-005" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_052()
    {
        await RunLinqTest(
            m => m.Where(x => x.Rating != null),
            174,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-006" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_053()
    {
        await RunLinqTest(
            m => m.Where(x => x.Rating == null),
            74,
            new string[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_060()
    {
        await RunLinqTest(
            m => m.Where(x => x.Year > 1929 && x.Year < 1940),
            9,
            new string[] { "id-041", "id-044", "id-049", "id-106", "id-135" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_061()
    {
        await RunLinqTest(
            m => m.Where(x => x.Year >= 1930 && x.Year <= 1939),
            9,
            new string[] { "id-041", "id-044", "id-049", "id-106", "id-135" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_062()
    {
        await RunLinqTest(
            m => m.Where(x => x.Year > 2000 || x.Year < 1940),
            78,
            new string[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_063()
    {
        await RunLinqTest(
            m => m.Where(x => x.Year > 2000 || !x.BestPictureWinner),
            218,
            new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_064()
    {
        await RunLinqTest(
            m => m.Where(x => x.Year > 1929).Where(x => x.Year < 1940),
            9,
            new string[] { "id-041", "id-044", "id-049", "id-106", "id-135" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_065()
    {
        await RunLinqTest(
            m => m.Where(x => x.Year >= 1930).Where(x => x.Year <= 1939),
            9,
            new string[] { "id-041", "id-044", "id-049", "id-106", "id-135" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_066()
    {
        await RunLinqTest(
            m => m.Where(x => (x.Year >= 1930 && x.Year <= 1940) || (x.Year >= 1950 && x.Year <= 1960)),
            46,
            new string[] { "id-005", "id-016", "id-027", "id-028", "id-031" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_070()
    {
        await RunLinqTest(
            m => m.Where(x => (x.Year - 1900) > 80),
            134,
            new string[] { "id-000", "id-003", "id-006", "id-007", "id-008" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_071()
    {
        await RunLinqTest(
            m => m.Where(x => (x.Year + x.Duration) < 2100),
            103,
            new string[] { "id-005", "id-015", "id-016", "id-024", "id-026" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_072()
    {
        await RunLinqTest(
            m => m.Where(x => (x.Year - 1900) < x.Duration),
            230,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_073()
    {
        await RunLinqTest(
            m => m.Where(x => (x.Duration * 2) < 180),
            21,
            new string[] { "id-041", "id-044", "id-054", "id-079", "id-089" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_074()
    {
        await RunLinqTest(
            m => m.Where(x => (x.Year / 1000.5) == 2),
            6,
            new string[] { "id-012", "id-042", "id-061", "id-173", "id-194" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_075()
    {
        await RunLinqTest(
            m => m.Where(x => (x.Duration % 2) == 1),
            124,
            new string[] { "id-001", "id-004", "id-007", "id-008", "id-009" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_076()
    {
        await RunLinqTest(
            m => m.Where(x => (x.Year - 1900) >= 80 && (x.Year + 10) <= 2000 && x.Duration <= 120),
            13,
            new string[] { "id-026", "id-047", "id-081", "id-103", "id-121" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_080()
    {
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate.Day == 1),
            7,
            new string[] { "id-019", "id-048", "id-129", "id-131", "id-132" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_081()
    {
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate.Month == 11),
            14,
            new string[] { "id-011", "id-016", "id-030", "id-064", "id-085" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_082()
    {
        await RunLinqTest(
            m => m.Where(x => x.ReleaseDate.Year != x.Year),
            52,
            new string[] { "id-004", "id-016", "id-024", "id-027", "id-029" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_090()
    {
        await RunLinqTest(
            m => m.Where(x => x.Title.EndsWith("er")),
            12,
            new string[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_091()
    {
        await RunLinqTest(
            m => m.Where(x => x.Title.ToLower().EndsWith("er")),
            12,
            new string[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_092()
    {
        await RunLinqTest(
            m => m.Where(x => x.Title.ToUpper().EndsWith("ER")),
            12,
            new string[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_093()
    {
        await RunLinqTest(
            m => m.Where(x => x.Rating.StartsWith("PG")),
            68,
            new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_094()
    {
        await RunLinqTest(
            m => m.Where(x => x.Rating.ToLower().StartsWith("pg")),
            68,
            new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_095()
    {
        await RunLinqTest(
            m => m.Where(x => x.Rating.ToUpper().StartsWith("PG")),
            68,
            new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_096()
    {
        await RunLinqTest(
            m => m.Where(x => x.Rating.IndexOf('-') > 0),
            29,
            new string[] { "id-006", "id-008", "id-012", "id-013", "id-018" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_097()
    {
        await RunLinqTest(
            m => m.Where(x => x.Rating.Contains("PG")),
            68,
            new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_098()
    {
        await RunLinqTest(
            m => m.Where(x => x.Rating.Substring(0, 2) == "PG"),
            68,
            new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_099()
    {
        await RunLinqTest(
            m => m.Where(x => x.Title.Trim().Length > 10),
            178,
            new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_100()
    {
        await RunLinqTest(
            m => m.Where(x => (x.Title + x.Rating) == "Fight ClubR"),
            1,
            new string[] { "id-009" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_120()
    {
        await RunLinqTest(
            m => m.Where(x => Math.Round(x.Duration / 60.0) == 2),
            186,
            new string[] { "id-000", "id-005", "id-009", "id-010", "id-011" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_121()
    {
        await RunLinqTest(
            m => m.Where(x => Math.Ceiling(x.Duration / 60.0) == 2),
            124,
            new string[] { "id-005", "id-023", "id-024", "id-025", "id-026" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_122()
    {
        await RunLinqTest(
            m => m.Where(x => Math.Floor(x.Duration / 60.0) == 2),
            120,
            new string[] { "id-000", "id-001", "id-003", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_123()
    {
        await RunLinqTest(
            m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Round(x.Duration / 60.0) == 2),
            162,
            new string[] { "id-000", "id-005", "id-009", "id-010", "id-013" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_124()
    {
        await RunLinqTest(
            m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Ceiling(x.Duration / 60.0) == 2),
            113,
            new string[] { "id-005", "id-025", "id-026", "id-027", "id-028" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_125()
    {
        await RunLinqTest(
            m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Floor(x.Duration / 60.0) == 2),
            99,
            new string[] { "id-000", "id-003", "id-004", "id-006", "id-009" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_160()
    {
        await RunLinqTest(
            m => m.Where(x => Math.Round(x.Duration / 60.0) == (float)2),
            186,
            new string[] { "id-000", "id-005", "id-009", "id-010", "id-011" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_161()
    {
        await RunLinqTest(
            m => m.Where(x => (float)x.Year < 1990.5f),
            141,
            new string[] { "id-001", "id-002", "id-004", "id-005", "id-010" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_162()
    {
        await RunLinqTest(
            m => m.Where(x => x.Title.Equals("The Godfather")),
            1,
            new string[] { "id-001" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_163()
    {
        await RunLinqTest(
            m => m.Where(x => !x.Title.Equals("The Godfather")),
            247,
            new string[] { "id-000", "id-002", "id-003", "id-004", "id-005" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_164()
    {
        await RunLinqTest(
            m => m.Where(x => x.Title.Equals("The Godfather", StringComparison.Ordinal)),
            1,
            new string[] { "id-001" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_165()
    {
        await RunLinqTest(
            m => m.Where(x => !x.Title.Equals("The Godfather", StringComparison.Ordinal)),
            247,
            new string[] { "id-000", "id-002", "id-003", "id-004", "id-005" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_166()
    {
        await RunLinqTest(
            m => m.Where(x => x.Rating.StartsWith("PG", StringComparison.InvariantCulture)),
            68,
            new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_167()
    {
        await RunLinqTest(
            m => m.Where(x => x.Rating.StartsWith("pg", StringComparison.InvariantCultureIgnoreCase)),
            68,
            new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_168()
    {
        await RunLinqTest(
            m => m.Where(x => x.Title.EndsWith("er", StringComparison.InvariantCulture)),
            12,
            new string[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_where_169()
    {
        await RunLinqTest(
            m => m.Where(x => x.Title.EndsWith("eR", StringComparison.InvariantCultureIgnoreCase)),
            12,
            new string[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task ToAsyncEnumerable_Linq_sync_1()
    {
        var dt = new DateTimeOffset(1994, 10, 14, 0, 0, 0, TimeSpan.Zero);
        await RunLinqTest(
            m => m.Where(x => x.UpdatedAt > dt).IncludeDeletedItems().OrderBy(x => x.UpdatedAt).IncludeTotalCount().Skip(25),
            223,
            new string[] { "id-025", "id-026", "id-027", "id-028", "id-029" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_001()
    {
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id),
            365,
            new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003", "dtm-004", "dtm-005" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_002()
    {
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.DateOnly.Month == 3),
            31,
            new[] { "dtm-059", "dtm-060", "dtm-061", "dtm-062", "dtm-063", "dtm-064" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_003()
    {
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.DateOnly.Day == 21),
            12,
            new[] { "dtm-020", "dtm-051", "dtm-079", "dtm-110", "dtm-140", "dtm-171" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_004()
    {
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.DateOnly.Year == 2022),
            365,
            new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003", "dtm-004", "dtm-005" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_005()
    {
        DateOnly myDate = new(2022, 2, 14);
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.DateOnly == myDate),
            1,
            new[] { "dtm-044" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_006()
    {
        DateOnly myDate = new(2022, 12, 15);
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.DateOnly >= myDate),
            17,
            new[] { "dtm-348", "dtm-349", "dtm-350", "dtm-351" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_007()
    {
        DateOnly myDate = new(2022, 12, 15);
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.DateOnly > myDate),
            16,
            new[] { "dtm-349", "dtm-350", "dtm-351", "dtm-352" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_008()
    {
        DateOnly myDate = new(2022, 7, 14);
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.DateOnly <= myDate),
            195,
            new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003", "dtm-004", "dtm-005" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_009()
    {
        DateOnly myDate = new(2022, 7, 14);
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.DateOnly < myDate),
            194,
            new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003", "dtm-004", "dtm-005" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_010()
    {
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.TimeOnly.Second == 0),
            365,
            new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003", "dtm-004", "dtm-005" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_011()
    {
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.TimeOnly.Hour == 3),
            31,
            new[] { "dtm-059", "dtm-060", "dtm-061", "dtm-062", "dtm-063", "dtm-064" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_012()
    {
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.TimeOnly.Minute == 21),
            12,
            new[] { "dtm-020", "dtm-051", "dtm-079", "dtm-110", "dtm-140", "dtm-171" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_013()
    {
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.TimeOnly.Hour <= 12),
            365,
            new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003", "dtm-004", "dtm-005" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_014()
    {
        TimeOnly myTime = new(2, 14, 0);
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.TimeOnly == myTime),
            1,
            new[] { "dtm-044" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_015()
    {
        TimeOnly myTime = new(12, 15, 0);
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.TimeOnly >= myTime),
            17,
            new[] { "dtm-348", "dtm-349", "dtm-350", "dtm-351" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_016()
    {
        TimeOnly myTime = new(12, 15, 0);
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.TimeOnly > myTime),
            16,
            new[] { "dtm-349", "dtm-350", "dtm-351", "dtm-352" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_017()
    {
        TimeOnly myTime = new(7, 15, 0);
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.TimeOnly <= myTime),
            196,
            new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003" }
        );
    }

    [Fact]
    public async Task Linq_DateTime_018()
    {
        TimeOnly myTime = new(7, 15, 0);
        await RunDateTimeTest(
            q => q.OrderBy(m => m.Id).Where(f => f.TimeOnly < myTime),
            195,
            new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003" }
        );
    }
    #endregion

    private async Task RunLinqTest(Func<ITableQuery<ClientMovie>, ITableQuery<ClientMovie>> linqExpression, int resultCount, string[] firstResults)
    {
        // Arrange
        var query = new TableQuery<ClientMovie>(fixture.MovieTable as RemoteTable<ClientMovie>);

        // Act
        var pageable = linqExpression.Invoke(query).ToAsyncEnumerable();
        var list = await pageable.ToListAsync().ConfigureAwait(false);

        // Assert
        Assert.Equal(resultCount, list.Count);
        var actualItems = list.Take(firstResults.Length).Select(m => m.Id).ToArray();
        Assert.Equal(firstResults, actualItems);
    }

    private async Task RunDateTimeTest(Func<ITableQuery<DateTimeClientModel>, ITableQuery<DateTimeClientModel>> linqExpression, int resultCount, string[] firstResults)
    {
        var query = new TableQuery<DateTimeClientModel>(fixture.DateTimeTable as RemoteTable<DateTimeClientModel>);
        var list = await linqExpression.Invoke(query).ToListAsync();
        Assert.Equal(resultCount, list.Count);
        var actualItems = list.Take(firstResults.Length).Select(m => m.Id).ToArray();
        Assert.Equal(firstResults, actualItems);
    }
}

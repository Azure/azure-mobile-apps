// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Query;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTableOfT
{
    [ExcludeFromCodeCoverage]
    public class CountItemsAsync_Tests : BaseOperationTest
    {
        public CountItemsAsync_Tests(ITestOutputHelper output) : base(output) { }

        [Fact]
        [Trait("Method", "CountItemsAsync")]
        public async Task CountItemsAsync_RetrievesCount()
        {
            await InitializeAsync(true);

            // Act
            long count = await table!.CountItemsAsync();

            // Assert
            Assert.Equal(MovieCount, count);
        }

        #region LINQ tests
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
        public async Task ToAsyncEnumerable_Linq_sync_1()
        {
            var dt = new DateTimeOffset(1994, 10, 14, 0, 0, 0, TimeSpan.Zero);
            await RunLinqTest(
                m => m.Where(x => x.UpdatedAt > dt).IncludeDeletedItems().OrderBy(x => x.UpdatedAt).IncludeTotalCount().Skip(25),
                248, /* Note that skip is ignored in the LINQ for Count; would be 223 otherwise */
                new string[] { "id-025", "id-026", "id-027", "id-028", "id-029" }
            );
        }
        #endregion

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Keeping results around for consistency")]
        private async Task RunLinqTest(Func<ITableQuery<ClientMovie>, ITableQuery<ClientMovie>> linqExpression, int resultCount, string[] firstResults)
        {
            await InitializeAsync();

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
}

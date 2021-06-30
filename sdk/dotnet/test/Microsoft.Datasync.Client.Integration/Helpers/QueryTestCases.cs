// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.TestData;
using Microsoft.Datasync.Client.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Integration.Helpers
{
    /// <summary>
    /// A set of test cases for testing the LINQ coverage.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    internal class QueryTestCases : TheoryData<string, Func<ITableQuery<Movie>, ITableQuery<Movie>>, int, string[]>
    {
        /// <summary>
        /// Each test case is a quadlet:
        ///     1. The test name
        ///     2. The func to execute the query
        ///     3. The number of items we expect to receive
        ///     4. The IDs of the items we expect to receive
        /// </summary>
        [SuppressMessage("Simplification", "RCS1068:Simplify logical negation.", Justification = "For testing")]
        [SuppressMessage("Simplification", "RCS1049:Simplify boolean comparison.", Justification = "For testing")]
        [SuppressMessage("Redundancy", "RCS1033:Remove redundant boolean literal.", Justification = "For testing")]
        [SuppressMessage("Usage", "RCS1155:Use StringComparison when comparing strings.", Justification = "For testing")]
        public QueryTestCases()
        {
            DateTime dt1 = new(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset dto1 = new(1994, 10, 14, 0, 0, 0, TimeSpan.Zero);

            DateTime dt2 = new(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset dto2 = new(1999, 12, 31, 0, 0, 0, TimeSpan.Zero);

            DateTime dt3 = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset dto3 = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

            Add("base-1", m => m, Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" });

            Add("count-1", m => m.IncludeTotalCount(), Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" });
            Add("count-2", m => m.IncludeTotalCount(true), Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" });
            Add("count-3", m => m.IncludeTotalCount(false), Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" });

            Add("deleted-1", m => m.IncludeDeletedItems(), Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" });
            Add("deleted-2", m => m.IncludeDeletedItems(true), Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" });
            Add("deleted-3", m => m.IncludeDeletedItems(false), Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" });

            Add("orderby-1", m => m.OrderBy(x => x.BestPictureWinner), Movies.Count, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" });
            Add("orderby-2", m => m.OrderBy(x => x.Duration), Movies.Count, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" });
            Add("orderby-3", m => m.OrderBy(x => x.Rating), Movies.Count, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" });
            Add("orderby-4", m => m.OrderBy(x => x.Title), Movies.Count, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" });
            Add("orderby-5", m => m.OrderBy(x => x.ReleaseDate), Movies.Count, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" });

            Add("orderbydesc-1", m => m.OrderByDescending(x => x.BestPictureWinner), Movies.Count, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("orderbydesc-2", m => m.OrderByDescending(x => x.Duration), Movies.Count, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" });
            Add("orderbydesc-3", m => m.OrderByDescending(x => x.Rating), Movies.Count, new[] { "id-197", "id-107", "id-115", "id-028", "id-039" });
            Add("orderbydesc-4", m => m.OrderByDescending(x => x.Title), Movies.Count, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" });
            Add("orderbydesc-5", m => m.OrderByDescending(x => x.ReleaseDate), Movies.Count, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" });

            Add("thenby-1", m => m.ThenBy(x => x.BestPictureWinner), Movies.Count, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" });
            Add("thenby-2", m => m.ThenBy(x => x.Duration), Movies.Count, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" });
            Add("thenby-3", m => m.ThenBy(x => x.Rating), Movies.Count, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" });
            Add("thenby-4", m => m.ThenBy(x => x.Title), Movies.Count, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" });
            Add("thenby-5", m => m.ThenBy(x => x.ReleaseDate), Movies.Count, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" });

            Add("thenbydesc-1", m => m.ThenByDescending(x => x.BestPictureWinner), Movies.Count, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("thenbydesc-2", m => m.ThenByDescending(x => x.Duration), Movies.Count, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" });
            Add("thenbydesc-3", m => m.ThenByDescending(x => x.Rating), Movies.Count, new[] { "id-197", "id-107", "id-115", "id-028", "id-039" });
            Add("thenbydesc-4", m => m.ThenByDescending(x => x.Title), Movies.Count, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" });
            Add("thenbydesc-5", m => m.ThenByDescending(x => x.ReleaseDate), Movies.Count, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" });

            Add("ordering-1", m => m.OrderBy(x => x.Year).ThenBy(x => x.Rating), Movies.Count, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" });
            Add("ordering-2", m => m.OrderBy(x => x.Year).ThenByDescending(x => x.Title), Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" });
            Add("ordering-3", m => m.OrderByDescending(x => x.Year).ThenBy(x => x.Rating), Movies.Count, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" });
            Add("ordering-4", m => m.OrderByDescending(x => x.Rating).ThenByDescending(x => x.Title), Movies.Count, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" });

            Add("skip-1", m => m.Skip(100), Movies.Count - 100, new[] { "id-100", "id-101", "id-102", "id-103", "id-104" });
            Add("skip-2", m => m.Skip(200), Movies.Count - 200, new[] { "id-200", "id-201", "id-202", "id-203", "id-204" });

            Add("take-1", m => m.Take(100), 100, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" });
            Add("take-2", m => m.Take(200), 200, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" });

            Add("skiptake-1", m => m.Skip(100).Take(50), 50, new[] { "id-100", "id-101", "id-102", "id-103", "id-104" });
            Add("skiptake-2", m => m.Skip(200).Take(25), 25, new[] { "id-200", "id-201", "id-202", "id-203", "id-204" });

            // Boolean compare
            Add("where-001", m => m.Where(x => x.BestPictureWinner), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-002", m => m.Where(x => !x.BestPictureWinner), 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" });
            Add("where-003", m => m.Where(x => x.BestPictureWinner == true), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-004", m => m.Where(x => x.BestPictureWinner == false), 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" });
            Add("where-005", m => m.Where(x => x.BestPictureWinner != true), 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" });
            Add("where-006", m => m.Where(x => x.BestPictureWinner != false), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-007", m => m.Where(x => !(x.BestPictureWinner == true)), 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" });
            Add("where-008", m => m.Where(x => !(x.BestPictureWinner != true)), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-009", m => m.Where(x => !(x.BestPictureWinner == false)), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-010", m => m.Where(x => !(x.BestPictureWinner != false)), 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" });

            // Int compare
            Add("where-011", m => m.Where(x => x.Duration == 90), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-012", m => m.Where(x => x.Duration < 100), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-013", m => m.Where(x => x.Duration <= 100), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-014", m => m.Where(x => x.Duration > 90), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-015", m => m.Where(x => x.Duration >= 90), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-016", m => m.Where(x => x.Duration != 90), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-017", m => m.Where(x => !(x.Duration == 90)), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-018", m => m.Where(x => !(x.Duration < 100)), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-019", m => m.Where(x => !(x.Duration <= 100)), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-020", m => m.Where(x => !(x.Duration > 90)), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-021", m => m.Where(x => !(x.Duration >= 90)), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-022", m => m.Where(x => !(x.Duration != 90)), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });

            // Date compares
            Add("where-031", m => m.Where(x => x.ReleaseDate == dt1), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-032", m => m.Where(x => x.ReleaseDate > dt2), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-033", m => m.Where(x => x.ReleaseDate >= dt2), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-034", m => m.Where(x => x.ReleaseDate < dt3), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-035", m => m.Where(x => x.ReleaseDate <= dt3), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-036", m => m.Where(x => x.ReleaseDate == dto1), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-037", m => m.Where(x => x.ReleaseDate > dto2), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-038", m => m.Where(x => x.ReleaseDate >= dto2), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-039", m => m.Where(x => x.ReleaseDate < dto3), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-040", m => m.Where(x => x.ReleaseDate <= dto3), 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });

            // String compares
            Add("where-050", m => m.Where(x => x.Title == "The GodFather"), 1, new[] { "id-001" });
            Add("where-051", m => m.Where(x => x.Title != "The GodFather"), Movies.Count - 1, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-052", m => m.Where(x => x.Rating != null), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-053", m => m.Where(x => x.Rating == null), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });

            // Boolean combinations
            Add("where-060", m => m.Where(x => x.Year > 1929 && x.Year < 1940), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-061", m => m.Where(x => x.Year >= 1930 && x.Year <= 1939), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-062", m => m.Where(x => x.Year > 2000 || x.Year < 1940), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-063", m => m.Where(x => x.Year > 2000 || !x.BestPictureWinner), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-064", m => m.Where(x => x.Year > 1929).Where(x => x.Year < 1940), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-065", m => m.Where(x => x.Year >= 1930).Where(x => x.Year <= 1939), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-066", m => m.Where(x => (x.Year >= 1930 && x.Year <= 1940) || (x.Year >= 1950 && x.Year <= 1960)), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });

            // Integer sums
            Add("where-070", m => m.Where(x => (x.Year - 1900) > 80), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-071", m => m.Where(x => (x.Year + x.Duration) < 2100), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-072", m => m.Where(x => (x.Year - 1900) < x.Duration), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-073", m => m.Where(x => (x.Duration * 2) < 120), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-074", m => m.Where(x => (x.Year / 1000.5) == 2), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-075", m => m.Where(x => (x.Duration % 2) == 1), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-076", m => m.Where(x => (x.Year - 1900) >= 80 && (x.Year + 10) <= 2000 && x.Duration <= 120), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });

            // Date field accessors
            Add("where-080", m => m.Where(x => x.ReleaseDate.Day == 1), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-081", m => m.Where(x => x.ReleaseDate.Month == 11), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-082", m => m.Where(x => x.ReleaseDate.Year != x.Year), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });

            // String functions
            Add("where-090", m => m.Where(x => x.Title.EndsWith("er")), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-091", m => m.Where(x => x.Title.ToLower().EndsWith("er")), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-092", m => m.Where(x => x.Title.ToUpper().EndsWith("ER")), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-093", m => m.Where(x => x.Rating.StartsWith("PG")), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-094", m => m.Where(x => x.Rating.ToLower().StartsWith("pg")), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-095", m => m.Where(x => x.Rating.ToUpper().StartsWith("PG")), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-096", m => m.Where(x => x.Rating.IndexOf('-') > 0), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-097", m => m.Where(x => x.Rating.Contains("PG")), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-098", m => m.Where(x => x.Rating.Substring(0, 2) == "PG"), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-099", m => m.Where(x => x.Title.Trim().Length > 0), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-100", m => m.Where(x => x.Title.Replace(" ", "-").ToUpper().StartsWith("THE-")), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-101", m => m.Where(x => (x.Title + x.Rating) == "The Blob PG-13"), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });

            // Integer functions
            Add("where-120", m => m.Where(x => Math.Round(x.Duration / 60.0) == 2), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-121", m => m.Where(x => Math.Ceiling(x.Duration / 60.0) == 2), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-122", m => m.Where(x => Math.Floor(x.Duration / 60.0) == 2), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-123", m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Round(x.Duration / 60.0) == 2), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-124", m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Ceiling(x.Duration / 60.0) == 2), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-125", m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Floor(x.Duration / 60.0) == 2), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });

            // Conversions
            Add("where-160", m => m.Where(x => Math.Round(x.Duration / 60.0) == (float)2), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
            Add("where-161", m => m.Where(x => (float)x.Year < 2001.5f), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });

            // Your basic sync operations
            Add("sync-1", m => m.Where(x => x.UpdatedAt > dto1).IncludeDeletedItems().OrderBy(x => x.UpdatedAt).IncludeTotalCount().Skip(25), 238, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" });
        }
    }
}

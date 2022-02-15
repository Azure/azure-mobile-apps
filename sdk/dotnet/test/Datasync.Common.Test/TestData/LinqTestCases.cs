// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Datasync.Common.Test.TestData;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Query;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Datasync.Common.Test
{
    /// <summary>
    /// The model for each test case returned by the <see cref="LinqTestCases"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LinqTestCase
    {
        internal LinqTestCase(string testCase, Func<ITableQuery<ClientMovie>, ITableQuery<ClientMovie>> linqExpression, string oDataString, int resultCount, string[] firstResults)
        {
            ATestCaseName = testCase;
            LinqExpression = linqExpression;
            ODataString = oDataString;
            ResultCount = resultCount;
            FirstResults = firstResults.ToArray();
        }

        /// <summary>
        /// This begins with A because then it's shown first in the XUnit output
        /// </summary>
        public string ATestCaseName { get; }

        /// <summary>
        /// This is used in reporting for the test case
        /// </summary>
        public string Name { get => ATestCaseName; }

        public Func<ITableQuery<ClientMovie>, ITableQuery<ClientMovie>> LinqExpression { get; }

        public string ODataString { get; }
        public int ResultCount { get; }
        public string[] FirstResults { get; }
    }
    /// <summary>
    /// A set of test cases for testing the LINQ coverage.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class LinqTestCases : TheoryData<LinqTestCase>
    {
        /// <summary>
        /// Each test case is a triplet:
        ///     1. The test name
        ///     2. The func to execute the query
        ///     3. The resulting OData query
        ///     4. the number of records that are returned
        ///     5. The results when sending to the test service
        /// </summary>
        [SuppressMessage("Simplification", "RCS1068:Simplify logical negation.", Justification = "For testing")]
        [SuppressMessage("Simplification", "RCS1049:Simplify boolean comparison.", Justification = "For testing")]
        [SuppressMessage("Redundancy", "RCS1033:Remove redundant boolean literal.", Justification = "For testing")]
        [SuppressMessage("Usage", "RCS1155:Use StringComparison when comparing strings.", Justification = "For testing")]
        public LinqTestCases()
        {
            DateTime dt1 = new(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset dto1 = new(1994, 10, 14, 0, 0, 0, TimeSpan.Zero);
            const string dts1 = "1994-10-14T00:00:00.000Z";

            DateTime dt2 = new(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset dto2 = new(1999, 12, 31, 0, 0, 0, TimeSpan.Zero);
            const string dts2 = "1999-12-31T00:00:00.000Z";

            DateTime dt3 = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset dto3 = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
            const string dts3 = "2000-01-01T00:00:00.000Z";

            Add(new LinqTestCase("base-1", m => m, "", Movies.Count, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));

            Add(new LinqTestCase("count-1", m => m.IncludeTotalCount(), "$count=true", Movies.Count, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("count-2", m => m.IncludeTotalCount(true), "$count=true", Movies.Count, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("count-3", m => m.IncludeTotalCount(false), "", Movies.Count, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));

            Add(new LinqTestCase("deleted-1", m => m.IncludeDeletedItems(), "__includedeleted=true", Movies.Count, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("deleted-2", m => m.IncludeDeletedItems(true), "__includedeleted=true", Movies.Count, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("deleted-3", m => m.IncludeDeletedItems(false), "", Movies.Count, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));

            Add(new LinqTestCase("orderby-1", m => m.OrderBy(x => x.BestPictureWinner), "$orderby=bestPictureWinner", Movies.Count, new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            Add(new LinqTestCase("orderby-2", m => m.OrderBy(x => x.Duration), "$orderby=duration", Movies.Count, new string[] { "id-227", "id-125", "id-133", "id-107", "id-118" }));
            Add(new LinqTestCase("orderby-3", m => m.OrderBy(x => x.Rating), "$orderby=rating", Movies.Count, new string[] { "id-004", "id-005", "id-011", "id-016", "id-031" }));
            Add(new LinqTestCase("orderby-4", m => m.OrderBy(x => x.Title), "$orderby=title", Movies.Count, new string[] { "id-005", "id-091", "id-243", "id-194", "id-060" }));
            Add(new LinqTestCase("orderby-5", m => m.OrderBy(x => x.ReleaseDate), "$orderby=releaseDate", Movies.Count, new string[] { "id-125", "id-133", "id-227", "id-118", "id-088" }));

            Add(new LinqTestCase("orderbydesc-1", m => m.OrderByDescending(x => x.BestPictureWinner), "$orderby=bestPictureWinner desc", Movies.Count, new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            Add(new LinqTestCase("orderbydesc-2", m => m.OrderByDescending(x => x.Duration), "$orderby=duration desc", Movies.Count, new string[] { "id-153", "id-065", "id-165", "id-008", "id-002" }));
            Add(new LinqTestCase("orderbydesc-3", m => m.OrderByDescending(x => x.Rating), "$orderby=rating desc", Movies.Count, new string[] { "id-000", "id-001", "id-002", "id-003", "id-007" }));
            Add(new LinqTestCase("orderbydesc-4", m => m.OrderByDescending(x => x.Title), "$orderby=title desc", Movies.Count, new string[] { "id-107", "id-100", "id-123", "id-190", "id-149" }));
            Add(new LinqTestCase("orderbydesc-5", m => m.OrderByDescending(x => x.ReleaseDate), "$orderby=releaseDate desc", Movies.Count, new string[] { "id-188", "id-033", "id-122", "id-186", "id-064" }));

            Add(new LinqTestCase("thenby-1", m => m.ThenBy(x => x.BestPictureWinner), "$orderby=bestPictureWinner", Movies.Count, new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            Add(new LinqTestCase("thenby-2", m => m.ThenBy(x => x.Duration), "$orderby=duration", Movies.Count, new string[] { "id-227", "id-125", "id-133", "id-107", "id-118" }));
            Add(new LinqTestCase("thenby-3", m => m.ThenBy(x => x.Rating), "$orderby=rating", Movies.Count, new string[] { "id-004", "id-005", "id-011", "id-016", "id-031" }));
            Add(new LinqTestCase("thenby-4", m => m.ThenBy(x => x.Title), "$orderby=title", Movies.Count, new string[] { "id-005", "id-091", "id-243", "id-194", "id-060" }));
            Add(new LinqTestCase("thenby-5", m => m.ThenBy(x => x.ReleaseDate), "$orderby=releaseDate", Movies.Count, new string[] { "id-125", "id-133", "id-227", "id-118", "id-088" }));

            Add(new LinqTestCase("thenbydesc-1", m => m.ThenByDescending(x => x.BestPictureWinner), "$orderby=bestPictureWinner desc", Movies.Count, new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            Add(new LinqTestCase("thenbydesc-2", m => m.ThenByDescending(x => x.Duration), "$orderby=duration desc", Movies.Count, new string[] { "id-153", "id-065", "id-165", "id-008", "id-002" }));
            Add(new LinqTestCase("thenbydesc-3", m => m.ThenByDescending(x => x.Rating), "$orderby=rating desc", Movies.Count, new string[] { "id-000", "id-001", "id-002", "id-003", "id-007" }));
            Add(new LinqTestCase("thenbydesc-4", m => m.ThenByDescending(x => x.Title), "$orderby=title desc", Movies.Count, new string[] { "id-107", "id-100", "id-123", "id-190", "id-149" }));
            Add(new LinqTestCase("thenbydesc-5", m => m.ThenByDescending(x => x.ReleaseDate), "$orderby=releaseDate desc", Movies.Count, new string[] { "id-188", "id-033", "id-122", "id-186", "id-064" }));

            Add(new LinqTestCase("ordering-1", m => m.OrderBy(x => x.Year).ThenBy(x => x.Rating), "$orderby=year,rating", Movies.Count, new string[] { "id-125", "id-229", "id-133", "id-227", "id-118" }));
            Add(new LinqTestCase("ordering-2", m => m.OrderBy(x => x.Year).ThenByDescending(x => x.Title), "$orderby=year,title desc", Movies.Count, new string[] { "id-125", "id-229", "id-133", "id-227", "id-118" }));
            Add(new LinqTestCase("ordering-3", m => m.OrderByDescending(x => x.Year).ThenBy(x => x.Rating), "$orderby=year desc,rating", Movies.Count, new string[] { "id-033", "id-122", "id-188", "id-102", "id-149" }));
            Add(new LinqTestCase("ordering-4", m => m.OrderByDescending(x => x.Rating).ThenByDescending(x => x.Title), "$orderby=rating desc,title desc", Movies.Count, new string[] { "id-107", "id-160", "id-092", "id-176", "id-147" }));
            Add(new LinqTestCase("ordering-5", m => m.OrderBy(x => x.UpdatedAt), "$orderby=updatedAt", Movies.Count, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));

            Add(new LinqTestCase("skip-1", m => m.Skip(100), "$skip=100", Movies.Count - 100, new string[] { "id-100", "id-101", "id-102", "id-103", "id-104" }));
            Add(new LinqTestCase("skip-2", m => m.Skip(200), "$skip=200", Movies.Count - 200, new string[] { "id-200", "id-201", "id-202", "id-203", "id-204" }));

            Add(new LinqTestCase("take-1", m => m.Take(100), "$top=100", 100, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("take-2", m => m.Take(200), "$top=200", 200, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));

            Add(new LinqTestCase("skiptake-1", m => m.Skip(100).Take(50), "$skip=100&$top=50", 50, new string[] { "id-100", "id-101", "id-102", "id-103", "id-104" }));
            Add(new LinqTestCase("skiptake-2", m => m.Skip(200).Take(25), "$skip=200&$top=25", 25, new string[] { "id-200", "id-201", "id-202", "id-203", "id-204" }));

            // Boolean compare
            Add(new LinqTestCase("where-001", m => m.Where(x => x.BestPictureWinner), "$filter=bestPictureWinner", 38, new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            Add(new LinqTestCase("where-002", m => m.Where(x => !x.BestPictureWinner), "$filter=not(bestPictureWinner)", 210, new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            Add(new LinqTestCase("where-003", m => m.Where(x => x.BestPictureWinner == true), "$filter=(bestPictureWinner eq true)", 38, new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            Add(new LinqTestCase("where-004", m => m.Where(x => x.BestPictureWinner == false), "$filter=(bestPictureWinner eq false)", 210, new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            Add(new LinqTestCase("where-005", m => m.Where(x => x.BestPictureWinner != true), "$filter=(bestPictureWinner ne true)", 210, new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            Add(new LinqTestCase("where-006", m => m.Where(x => x.BestPictureWinner != false), "$filter=(bestPictureWinner ne false)", 38, new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            Add(new LinqTestCase("where-007", m => m.Where(x => !(x.BestPictureWinner == true)), "$filter=not((bestPictureWinner eq true))", 210, new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            Add(new LinqTestCase("where-008", m => m.Where(x => !(x.BestPictureWinner != true)), "$filter=not((bestPictureWinner ne true))", 38, new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            Add(new LinqTestCase("where-009", m => m.Where(x => !(x.BestPictureWinner == false)), "$filter=not((bestPictureWinner eq false))", 38, new string[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            Add(new LinqTestCase("where-010", m => m.Where(x => !(x.BestPictureWinner != false)), "$filter=not((bestPictureWinner ne false))", 210, new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));

            // Int compare
            Add(new LinqTestCase("where-011", m => m.Where(x => x.Duration == 100), "$filter=(duration eq 100)", 3, new string[] { "id-116", "id-159", "id-186" }));
            Add(new LinqTestCase("where-012", m => m.Where(x => x.Duration < 100), "$filter=(duration lt 100)", 44, new string[] { "id-005", "id-037", "id-041", "id-044", "id-054" }));
            Add(new LinqTestCase("where-013", m => m.Where(x => x.Duration <= 100), "$filter=(duration le 100)", 47, new string[] { "id-005", "id-037", "id-041", "id-044", "id-054" }));
            Add(new LinqTestCase("where-014", m => m.Where(x => x.Duration > 90), "$filter=(duration gt 90)", 227, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("where-015", m => m.Where(x => x.Duration >= 90), "$filter=(duration ge 90)", 227, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("where-016", m => m.Where(x => x.Duration != 100), "$filter=(duration ne 100)", 245, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("where-017", m => m.Where(x => !(x.Duration == 100)), "$filter=not((duration eq 100))", 245, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("where-018", m => m.Where(x => !(x.Duration < 100)), "$filter=not((duration lt 100))", 204, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("where-019", m => m.Where(x => !(x.Duration <= 100)), "$filter=not((duration le 100))", 201, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("where-020", m => m.Where(x => !(x.Duration > 90)), "$filter=not((duration gt 90))", 21, new string[] { "id-041", "id-044", "id-054", "id-079", "id-089" }));
            Add(new LinqTestCase("where-021", m => m.Where(x => !(x.Duration >= 90)), "$filter=not((duration ge 90))", 21, new string[] { "id-041", "id-044", "id-054", "id-079", "id-089" }));
            Add(new LinqTestCase("where-022", m => m.Where(x => !(x.Duration != 100)), "$filter=not((duration ne 100))", 3, new string[] { "id-116", "id-159", "id-186" }));

            // Date compares
            Add(new LinqTestCase("where-031", m => m.Where(x => x.ReleaseDate == dt1), $"$filter=(releaseDate eq cast({dts1},Edm.DateTimeOffset))", 2, new string[] { "id-000", "id-003" }));
            Add(new LinqTestCase("where-032", m => m.Where(x => x.ReleaseDate > dt2), $"$filter=(releaseDate gt cast({dts2},Edm.DateTimeOffset))", 69, new string[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            Add(new LinqTestCase("where-033", m => m.Where(x => x.ReleaseDate >= dt2), $"$filter=(releaseDate ge cast({dts2},Edm.DateTimeOffset))", 69, new string[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            Add(new LinqTestCase("where-034", m => m.Where(x => x.ReleaseDate < dt3), $"$filter=(releaseDate lt cast({dts3},Edm.DateTimeOffset))", 179, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("where-035", m => m.Where(x => x.ReleaseDate <= dt3), $"$filter=(releaseDate le cast({dts3},Edm.DateTimeOffset))", 179, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("where-036", m => m.Where(x => x.ReleaseDate == dto1), $"$filter=(releaseDate eq cast({dts1},Edm.DateTimeOffset))", 2, new string[] { "id-000", "id-003" }));
            Add(new LinqTestCase("where-037", m => m.Where(x => x.ReleaseDate > dto2), $"$filter=(releaseDate gt cast({dts2},Edm.DateTimeOffset))", 69, new string[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            Add(new LinqTestCase("where-038", m => m.Where(x => x.ReleaseDate >= dto2), $"$filter=(releaseDate ge cast({dts2},Edm.DateTimeOffset))", 69, new string[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            Add(new LinqTestCase("where-039", m => m.Where(x => x.ReleaseDate < dto3), $"$filter=(releaseDate lt cast({dts3},Edm.DateTimeOffset))", 179, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("where-040", m => m.Where(x => x.ReleaseDate <= dto3), $"$filter=(releaseDate le cast({dts3},Edm.DateTimeOffset))", 179, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));

            // String compares
            Add(new LinqTestCase("where-050", m => m.Where(x => x.Title == "The Godfather"), "$filter=(title eq 'The Godfather')", 1, new string[] { "id-001" }));
            Add(new LinqTestCase("where-051", m => m.Where(x => x.Title != "The Godfather"), "$filter=(title ne 'The Godfather')", 247, new string[] { "id-000", "id-002", "id-003", "id-004", "id-005" }));
            Add(new LinqTestCase("where-052", m => m.Where(x => x.Rating != null), "$filter=(rating ne null)", 174, new string[] { "id-000", "id-001", "id-002", "id-003", "id-006" }));
            Add(new LinqTestCase("where-053", m => m.Where(x => x.Rating == null), "$filter=(rating eq null)", 74, new string[] { "id-004", "id-005", "id-011", "id-016", "id-031" }));

            // Boolean combinations
            Add(new LinqTestCase("where-060", m => m.Where(x => x.Year > 1929 && x.Year < 1940), "$filter=((year gt 1929) and (year lt 1940))", 9, new string[] { "id-041", "id-044", "id-049", "id-106", "id-135" }));
            Add(new LinqTestCase("where-061", m => m.Where(x => x.Year >= 1930 && x.Year <= 1939), "$filter=((year ge 1930) and (year le 1939))", 9, new string[] { "id-041", "id-044", "id-049", "id-106", "id-135" }));
            Add(new LinqTestCase("where-062", m => m.Where(x => x.Year > 2000 || x.Year < 1940), "$filter=((year gt 2000) or (year lt 1940))", 78, new string[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            Add(new LinqTestCase("where-063", m => m.Where(x => x.Year > 2000 || !x.BestPictureWinner), "$filter=((year gt 2000) or not(bestPictureWinner))", 218, new string[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            Add(new LinqTestCase("where-064", m => m.Where(x => x.Year > 1929).Where(x => x.Year < 1940), "$filter=((year gt 1929) and (year lt 1940))", 9, new string[] { "id-041", "id-044", "id-049", "id-106", "id-135" }));
            Add(new LinqTestCase("where-065", m => m.Where(x => x.Year >= 1930).Where(x => x.Year <= 1939), "$filter=((year ge 1930) and (year le 1939))", 9, new string[] { "id-041", "id-044", "id-049", "id-106", "id-135" }));
            Add(new LinqTestCase("where-066", m => m.Where(x => (x.Year >= 1930 && x.Year <= 1940) || (x.Year >= 1950 && x.Year <= 1960)), "$filter=(((year ge 1930) and (year le 1940)) or ((year ge 1950) and (year le 1960)))", 46, new string[] { "id-005", "id-016", "id-027", "id-028", "id-031" }));

            // Integer sums
            Add(new LinqTestCase("where-070", m => m.Where(x => (x.Year - 1900) > 80), "$filter=((year sub 1900) gt 80)", 134, new string[] { "id-000", "id-003", "id-006", "id-007", "id-008" }));
            Add(new LinqTestCase("where-071", m => m.Where(x => (x.Year + x.Duration) < 2100), "$filter=((year add duration) lt 2100)", 103, new string[] { "id-005", "id-015", "id-016", "id-024", "id-026" }));
            Add(new LinqTestCase("where-072", m => m.Where(x => (x.Year - 1900) < x.Duration), "$filter=((year sub 1900) lt duration)", 230, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("where-073", m => m.Where(x => (x.Duration * 2) < 180), "$filter=((duration mul 2) lt 180)", 21, new string[] { "id-041", "id-044", "id-054", "id-079", "id-089" }));
            Add(new LinqTestCase("where-074", m => m.Where(x => (x.Year / 1000.5) == 2), "$filter=((year div 1000.5) eq 2.0)", 6, new string[] { "id-012", "id-042", "id-061", "id-173", "id-194" }));
            Add(new LinqTestCase("where-075", m => m.Where(x => (x.Duration % 2) == 1), "$filter=((duration mod 2) eq 1)", 124, new string[] { "id-001", "id-004", "id-007", "id-008", "id-009" }));
            Add(new LinqTestCase("where-076", m => m.Where(x => (x.Year - 1900) >= 80 && (x.Year + 10) <= 2000 && x.Duration <= 120), "$filter=((((year sub 1900) ge 80) and ((year add 10) le 2000)) and (duration le 120))", 13, new string[] { "id-026", "id-047", "id-081", "id-103", "id-121" }));

            // Date field accessors
            Add(new LinqTestCase("where-080", m => m.Where(x => x.ReleaseDate.Day == 1), "$filter=(day(releaseDate) eq 1)", 7, new string[] { "id-019", "id-048", "id-129", "id-131", "id-132" }));
            Add(new LinqTestCase("where-081", m => m.Where(x => x.ReleaseDate.Month == 11), "$filter=(month(releaseDate) eq 11)", 14, new string[] { "id-011", "id-016", "id-030", "id-064", "id-085" }));
            Add(new LinqTestCase("where-082", m => m.Where(x => x.ReleaseDate.Year != x.Year), "$filter=(year(releaseDate) ne year)", 52, new string[] { "id-004", "id-016", "id-024", "id-027", "id-029" }));

            // String functions
            Add(new LinqTestCase("where-090", m => m.Where(x => x.Title.EndsWith("er")), "$filter=endswith(title,'er')", 12, new string[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            Add(new LinqTestCase("where-091", m => m.Where(x => x.Title.ToLower().EndsWith("er")), "$filter=endswith(tolower(title),'er')", 12, new string[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            Add(new LinqTestCase("where-092", m => m.Where(x => x.Title.ToUpper().EndsWith("ER")), "$filter=endswith(toupper(title),'ER')", 12, new string[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            Add(new LinqTestCase("where-093", m => m.Where(x => x.Rating.StartsWith("PG")), "$filter=startswith(rating,'PG')", 68, new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }));
            Add(new LinqTestCase("where-094", m => m.Where(x => x.Rating.ToLower().StartsWith("pg")), "$filter=startswith(tolower(rating),'pg')", 68, new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }));
            Add(new LinqTestCase("where-095", m => m.Where(x => x.Rating.ToUpper().StartsWith("PG")), "$filter=startswith(toupper(rating),'PG')", 68, new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }));
            Add(new LinqTestCase("where-096", m => m.Where(x => x.Rating.IndexOf('-') > 0), "$filter=(indexof(rating,'-') gt 0)", 29, new string[] { "id-006", "id-008", "id-012", "id-013", "id-018" }));
            Add(new LinqTestCase("where-097", m => m.Where(x => x.Rating.Contains("PG")), "$filter=contains(rating,'PG')", 68, new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }));
            Add(new LinqTestCase("where-098", m => m.Where(x => x.Rating.Substring(0, 2) == "PG"), "$filter=(substring(rating,0,2) eq 'PG')", 68, new string[] { "id-006", "id-008", "id-010", "id-012", "id-013" }));
            Add(new LinqTestCase("where-099", m => m.Where(x => x.Title.Trim().Length > 10), "$filter=(length(trim(title)) gt 10)", 178, new string[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            Add(new LinqTestCase("where-100", m => m.Where(x => (x.Title + x.Rating) == "Fight ClubR"), "$filter=(concat(title,rating) eq 'Fight ClubR')", 1, new string[] { "id-009" }));

            // Integer functions
            Add(new LinqTestCase("where-120", m => m.Where(x => Math.Round(x.Duration / 60.0) == 2), "$filter=(round((duration div 60.0)) eq 2.0)", 186, new string[] { "id-000", "id-005", "id-009", "id-010", "id-011" }));
            Add(new LinqTestCase("where-121", m => m.Where(x => Math.Ceiling(x.Duration / 60.0) == 2), "$filter=(ceiling((duration div 60.0)) eq 2.0)", 124, new string[] { "id-005", "id-023", "id-024", "id-025", "id-026" }));
            Add(new LinqTestCase("where-122", m => m.Where(x => Math.Floor(x.Duration / 60.0) == 2), "$filter=(floor((duration div 60.0)) eq 2.0)", 120, new string[] { "id-000", "id-001", "id-003", "id-004", "id-006" }));
            Add(new LinqTestCase("where-123", m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Round(x.Duration / 60.0) == 2), "$filter=(not(bestPictureWinner) and (round((duration div 60.0)) eq 2.0))", 162, new string[] { "id-000", "id-005", "id-009", "id-010", "id-013" }));
            Add(new LinqTestCase("where-124", m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Ceiling(x.Duration / 60.0) == 2), "$filter=(not(bestPictureWinner) and (ceiling((duration div 60.0)) eq 2.0))", 113, new string[] { "id-005", "id-025", "id-026", "id-027", "id-028" }));
            Add(new LinqTestCase("where-125", m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Floor(x.Duration / 60.0) == 2), "$filter=(not(bestPictureWinner) and (floor((duration div 60.0)) eq 2.0))", 99, new string[] { "id-000", "id-003", "id-004", "id-006", "id-009" }));

            // Conversions
            Add(new LinqTestCase("where-160", m => m.Where(x => Math.Round(x.Duration / 60.0) == (float)2), "$filter=(round((duration div 60.0)) eq 2.0)", 186, new string[] { "id-000", "id-005", "id-009", "id-010", "id-011" }));
            Add(new LinqTestCase("where-161", m => m.Where(x => (float)x.Year < 1990.5f), "$filter=(year lt 1990.5f)", 141, new string[] { "id-001", "id-002", "id-004", "id-005", "id-010" }));

            // Your basic sync operations
            Add(new LinqTestCase("sync-1", m => m.Where(x => x.UpdatedAt > dto1).IncludeDeletedItems().OrderBy(x => x.UpdatedAt).IncludeTotalCount().Skip(25), $"__includedeleted=true&$count=true&$filter=(updatedAt gt cast({dts1},Edm.DateTimeOffset))&$orderby=updatedAt&$skip=25", 223, new string[] { "id-025", "id-026", "id-027", "id-028", "id-029" }));
        }
    }
}

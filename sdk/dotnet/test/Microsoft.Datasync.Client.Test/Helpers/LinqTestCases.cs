// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    /// <summary>
    /// A set of test cases for testing the LINQ coverage.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    internal class LinqTestCases : TheoryData<string, Func<ITableQuery<Movie>, ITableQuery<Movie>>, string>
    {
        /// <summary>
        /// Each test case is a triplet:
        ///     1. The test name
        ///     2. The func to execute the query
        ///     3. The resulting OData query
        /// </summary>
        [SuppressMessage("Simplification", "RCS1068:Simplify logical negation.", Justification = "For testing")]
        [SuppressMessage("Simplification", "RCS1049:Simplify boolean comparison.", Justification = "For testing")]
        [SuppressMessage("Redundancy", "RCS1033:Remove redundant boolean literal.", Justification = "For testing")]
        [SuppressMessage("Usage", "RCS1155:Use StringComparison when comparing strings.", Justification = "For testing")]
        public LinqTestCases()
        {
            DateTime dt1 = new(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset dto1 = new(1994, 10, 14, 0, 0, 0, TimeSpan.Zero);
            const string dts1 = "1994-10-14T00:00:00.000+00:00";

            DateTime dt2 = new(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset dto2 = new(1999, 12, 31, 0, 0, 0, TimeSpan.Zero);
            const string dts2 = "1999-12-31T00:00:00.000+00:00";

            DateTime dt3 = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset dto3 = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
            const string dts3 = "2000-01-01T00:00:00.000+00:00";

            Add("base-1", m => m, "");

            Add("count-1", m => m.IncludeTotalCount(), "$count=true");
            Add("count-2", m => m.IncludeTotalCount(true), "$count=true");
            Add("count-3", m => m.IncludeTotalCount(false), "");

            Add("deleted-1", m => m.IncludeDeletedItems(), "__includedeleted=true");
            Add("deleted-2", m => m.IncludeDeletedItems(true), "__includedeleted=true");
            Add("deleted-3", m => m.IncludeDeletedItems(false), "");

            Add("orderby-1", m => m.OrderBy(x => x.BestPictureWinner), "$orderby=bestPictureWinner");
            Add("orderby-2", m => m.OrderBy(x => x.Duration), "$orderby=duration");
            Add("orderby-3", m => m.OrderBy(x => x.Rating), "$orderby=mpaaRating");
            Add("orderby-4", m => m.OrderBy(x => x.Title), "$orderby=title");
            Add("orderby-5", m => m.OrderBy(x => x.ReleaseDate), "$orderby=releaseDate");
            Add("orderby-6", m => m.OrderBy(x => x.MovieRating), "$orderby=rating");

            Add("orderbydesc-1", m => m.OrderByDescending(x => x.BestPictureWinner), "$orderby=bestPictureWinner desc");
            Add("orderbydesc-2", m => m.OrderByDescending(x => x.Duration), "$orderby=duration desc");
            Add("orderbydesc-3", m => m.OrderByDescending(x => x.Rating), "$orderby=mpaaRating desc");
            Add("orderbydesc-4", m => m.OrderByDescending(x => x.Title), "$orderby=title desc");
            Add("orderbydesc-5", m => m.OrderByDescending(x => x.ReleaseDate), "$orderby=releaseDate desc");
            Add("orderbydesc-6", m => m.OrderByDescending(x => x.MovieRating), "$orderby=rating desc");

            Add("thenby-1", m => m.ThenBy(x => x.BestPictureWinner), "$orderby=bestPictureWinner");
            Add("thenby-2", m => m.ThenBy(x => x.Duration), "$orderby=duration");
            Add("thenby-3", m => m.ThenBy(x => x.Rating), "$orderby=mpaaRating");
            Add("thenby-4", m => m.ThenBy(x => x.Title), "$orderby=title");
            Add("thenby-5", m => m.ThenBy(x => x.ReleaseDate), "$orderby=releaseDate");

            Add("thenbydesc-1", m => m.ThenByDescending(x => x.BestPictureWinner), "$orderby=bestPictureWinner desc");
            Add("thenbydesc-2", m => m.ThenByDescending(x => x.Duration), "$orderby=duration desc");
            Add("thenbydesc-3", m => m.ThenByDescending(x => x.Rating), "$orderby=mpaaRating desc");
            Add("thenbydesc-4", m => m.ThenByDescending(x => x.Title), "$orderby=title desc");
            Add("thenbydesc-5", m => m.ThenByDescending(x => x.ReleaseDate), "$orderby=releaseDate desc");

            Add("ordering-1", m => m.OrderBy(x => x.Year).ThenBy(x => x.Rating), "$orderby=year,mpaaRating");
            Add("ordering-2", m => m.OrderBy(x => x.Year).ThenByDescending(x => x.Title), "$orderby=year,title desc");
            Add("ordering-3", m => m.OrderByDescending(x => x.Year).ThenBy(x => x.Rating), "$orderby=year desc,mpaaRating");
            Add("ordering-4", m => m.OrderByDescending(x => x.Rating).ThenByDescending(x => x.Title), "$orderby=mpaaRating desc,title desc");
            Add("ordering-5", m => m.OrderBy(x => x.UpdatedAt), "$orderby=updatedAt");

            Add("skip-1", m => m.Skip(100), "$skip=100");
            Add("skip-2", m => m.Skip(200), "$skip=200");

            Add("take-1", m => m.Take(100), "$top=100");
            Add("take-2", m => m.Take(200), "$top=200");

            Add("skiptake-1", m => m.Skip(100).Take(50), "$skip=100&$top=50");
            Add("skiptake-2", m => m.Skip(200).Take(25), "$skip=200&$top=25");

            // Boolean compare
            Add("where-001", m => m.Where(x => x.BestPictureWinner), "$filter=bestPictureWinner");
            Add("where-002", m => m.Where(x => !x.BestPictureWinner), "$filter=not(bestPictureWinner)");
            Add("where-003", m => m.Where(x => x.BestPictureWinner == true), "$filter=(bestPictureWinner eq true)");
            Add("where-004", m => m.Where(x => x.BestPictureWinner == false), "$filter=(bestPictureWinner eq false)");
            Add("where-005", m => m.Where(x => x.BestPictureWinner != true), "$filter=(bestPictureWinner ne true)");
            Add("where-006", m => m.Where(x => x.BestPictureWinner != false), "$filter=(bestPictureWinner ne false)");
            Add("where-007", m => m.Where(x => !(x.BestPictureWinner == true)), "$filter=not((bestPictureWinner eq true))");
            Add("where-008", m => m.Where(x => !(x.BestPictureWinner != true)), "$filter=not((bestPictureWinner ne true))");
            Add("where-009", m => m.Where(x => !(x.BestPictureWinner == false)), "$filter=not((bestPictureWinner eq false))");
            Add("where-010", m => m.Where(x => !(x.BestPictureWinner != false)), "$filter=not((bestPictureWinner ne false))");

            // Int compare
            Add("where-011", m => m.Where(x => x.Duration == 60), "$filter=(duration eq 60)");
            Add("where-012", m => m.Where(x => x.Duration < 100), "$filter=(duration lt 100)");
            Add("where-013", m => m.Where(x => x.Duration <= 100), "$filter=(duration le 100)");
            Add("where-014", m => m.Where(x => x.Duration > 90), "$filter=(duration gt 90)");
            Add("where-015", m => m.Where(x => x.Duration >= 90), "$filter=(duration ge 90)");
            Add("where-016", m => m.Where(x => x.Duration != 60), "$filter=(duration ne 60)");
            Add("where-017", m => m.Where(x => !(x.Duration == 60)), "$filter=not((duration eq 60))");
            Add("where-018", m => m.Where(x => !(x.Duration < 100)), "$filter=not((duration lt 100))");
            Add("where-019", m => m.Where(x => !(x.Duration <= 100)), "$filter=not((duration le 100))");
            Add("where-020", m => m.Where(x => !(x.Duration > 90)), "$filter=not((duration gt 90))");
            Add("where-021", m => m.Where(x => !(x.Duration >= 90)), "$filter=not((duration ge 90))");
            Add("where-022", m => m.Where(x => !(x.Duration != 60)), "$filter=not((duration ne 60))");

            // Date compares
            Add("where-031", m => m.Where(x => x.ReleaseDate == dt1), $"$filter=(releaseDate eq datetimeoffset'{dts1}')");
            Add("where-032", m => m.Where(x => x.ReleaseDate > dt2), $"$filter=(releaseDate gt datetimeoffset'{dts2}')");
            Add("where-033", m => m.Where(x => x.ReleaseDate >= dt2), $"$filter=(releaseDate ge datetimeoffset'{dts2}')");
            Add("where-034", m => m.Where(x => x.ReleaseDate < dt3), $"$filter=(releaseDate lt datetimeoffset'{dts3}')");
            Add("where-035", m => m.Where(x => x.ReleaseDate <= dt3), $"$filter=(releaseDate le datetimeoffset'{dts3}')");
            Add("where-036", m => m.Where(x => x.ReleaseDate == dto1), $"$filter=(releaseDate eq datetimeoffset'{dts1}')");
            Add("where-037", m => m.Where(x => x.ReleaseDate > dto2), $"$filter=(releaseDate gt datetimeoffset'{dts2}')");
            Add("where-038", m => m.Where(x => x.ReleaseDate >= dto2), $"$filter=(releaseDate ge datetimeoffset'{dts2}')");
            Add("where-039", m => m.Where(x => x.ReleaseDate < dto3), $"$filter=(releaseDate lt datetimeoffset'{dts3}')");
            Add("where-040", m => m.Where(x => x.ReleaseDate <= dto3), $"$filter=(releaseDate le datetimeoffset'{dts3}')");

            // String compares
            Add("where-050", m => m.Where(x => x.Title == "The GodFather"), "$filter=(title eq 'The GodFather')");
            Add("where-051", m => m.Where(x => x.Title != "The GodFather"), "$filter=(title ne 'The GodFather')");
            Add("where-052", m => m.Where(x => x.Rating != null), "$filter=(mpaaRating ne null)");
            Add("where-053", m => m.Where(x => x.Rating == null), "$filter=(mpaaRating eq null)");

            // Boolean combinations
            Add("where-060", m => m.Where(x => x.Year > 1929 && x.Year < 1940), "$filter=((year gt 1929) and (year lt 1940))");
            Add("where-061", m => m.Where(x => x.Year >= 1930 && x.Year <= 1939), "$filter=((year ge 1930) and (year le 1939))");
            Add("where-062", m => m.Where(x => x.Year > 2000 || x.Year < 1940), "$filter=((year gt 2000) or (year lt 1940))");
            Add("where-063", m => m.Where(x => x.Year > 2000 || !x.BestPictureWinner), "$filter=((year gt 2000) or not(bestPictureWinner))");
            Add("where-064", m => m.Where(x => x.Year > 1929).Where(x => x.Year < 1940), "$filter=((year gt 1929) and (year lt 1940))");
            Add("where-065", m => m.Where(x => x.Year >= 1930).Where(x => x.Year <= 1939), "$filter=((year ge 1930) and (year le 1939))");
            Add("where-066", m => m.Where(x => (x.Year >= 1930 && x.Year <= 1940) || (x.Year >= 1950 && x.Year <= 1960)), "$filter=(((year ge 1930) and (year le 1940)) or ((year ge 1950) and (year le 1960)))");

            // Integer sums
            Add("where-070", m => m.Where(x => (x.Year - 1900) > 80), "$filter=((year sub 1900) gt 80)");
            Add("where-071", m => m.Where(x => (x.Year + x.Duration) < 2100), "$filter=((year add duration) lt 2100)");
            Add("where-072", m => m.Where(x => (x.Year - 1900) < x.Duration), "$filter=((year sub 1900) lt duration)");
            Add("where-073", m => m.Where(x => (x.Duration * 2) < 120), "$filter=((duration mul 2) lt 120)");
            Add("where-074", m => m.Where(x => (x.Year / 1000.5) == 2), "$filter=((year div 1000.5) eq 2.0)");
            Add("where-075", m => m.Where(x => (x.Duration % 2) == 1), "$filter=((duration mod 2) eq 1)");
            Add("where-076", m => m.Where(x => (x.Year - 1900) >= 80 && (x.Year + 10) <= 2000 && x.Duration <= 120), "$filter=((((year sub 1900) ge 80) and ((year add 10) le 2000)) and (duration le 120))");

            // Date field accessors
            Add("where-080", m => m.Where(x => x.ReleaseDate.Day == 1), "$filter=(day(releaseDate) eq 1)");
            Add("where-081", m => m.Where(x => x.ReleaseDate.Month == 11), "$filter=(month(releaseDate) eq 11)");
            Add("where-082", m => m.Where(x => x.ReleaseDate.Year != x.Year), "$filter=(year(releaseDate) ne year)");

            // String functions
            Add("where-090", m => m.Where(x => x.Title.EndsWith("er")), "$filter=endswith(title,'er')");
            Add("where-091", m => m.Where(x => x.Title.ToLower().EndsWith("er")), "$filter=endswith(tolower(title),'er')");
            Add("where-092", m => m.Where(x => x.Title.ToUpper().EndsWith("ER")), "$filter=endswith(toupper(title),'ER')");
            Add("where-093", m => m.Where(x => x.Rating.StartsWith("PG")), "$filter=startswith(mpaaRating,'PG')");
            Add("where-094", m => m.Where(x => x.Rating.ToLower().StartsWith("pg")), "$filter=startswith(tolower(mpaaRating),'pg')");
            Add("where-095", m => m.Where(x => x.Rating.ToUpper().StartsWith("PG")), "$filter=startswith(toupper(mpaaRating),'PG')");
            Add("where-096", m => m.Where(x => x.Rating.IndexOf('-') > 0), "$filter=(indexof(mpaaRating,'-') gt 0)");
            Add("where-097", m => m.Where(x => x.Rating.Contains("PG")), "$filter=contains(mpaaRating,'PG')");
            Add("where-098", m => m.Where(x => x.Rating.Substring(0, 2) == "PG"), "$filter=(substring(mpaaRating,0,2) eq 'PG')");
            Add("where-099", m => m.Where(x => x.Title.Trim().Length > 0), "$filter=(length(trim(title)) gt 0)");
            Add("where-100", m => m.Where(x => x.Title.Replace(" ", "-").ToUpper().StartsWith("THE-")), "$filter=startswith(toupper(replace(title,' ','-')),'THE-')");
            Add("where-101", m => m.Where(x => (x.Title + x.Rating) == "The Blob PG-13"), "$filter=(concat(title,mpaaRating) eq 'The Blob PG-13')");

            // Integer functions
            Add("where-120", m => m.Where(x => Math.Round(x.Duration / 60.0) == 2), "$filter=(round((duration div 60.0)) eq 2.0)");
            Add("where-121", m => m.Where(x => Math.Ceiling(x.Duration / 60.0) == 2), "$filter=(ceiling((duration div 60.0)) eq 2.0)");
            Add("where-122", m => m.Where(x => Math.Floor(x.Duration / 60.0) == 2), "$filter=(floor((duration div 60.0)) eq 2.0)");
            Add("where-123", m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Round(x.Duration / 60.0) == 2), "$filter=(not(bestPictureWinner) and (round((duration div 60.0)) eq 2.0))");
            Add("where-124", m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Ceiling(x.Duration / 60.0) == 2), "$filter=(not(bestPictureWinner) and (ceiling((duration div 60.0)) eq 2.0))");
            Add("where-125", m => m.Where(x => !x.BestPictureWinner).Where(x => Math.Floor(x.Duration / 60.0) == 2), "$filter=(not(bestPictureWinner) and (floor((duration div 60.0)) eq 2.0))");

            // Enum comparisons
            Add("where-140", m => m.Where(x => x.MovieRating == MovieRating.PG13), "$filter=(rating eq 2)");
            Add("where-141", m => m.Where(x => x.MovieRating.ToString() == "PG13"), "$filter=(rating eq 'PG13')");
            Add("where-142", m => m.Where(x => x.MovieRating == (MovieRating)1), "$filter=(rating eq 1)");

            // Conversions
            Add("where-160", m => m.Where(x => Math.Round(x.Duration / 60.0) == (float)2), "$filter=(round((duration div 60.0)) eq 2.0)");
            Add("where-161", m => m.Where(x => (float)x.Year < 2001.5f), "$filter=(year lt 2001.5f)");

            // Your basic sync operations
            Add("sync-1", m => m.Where(x => x.UpdatedAt > dto1).IncludeDeletedItems().OrderBy(x => x.UpdatedAt).IncludeTotalCount().Skip(25), $"__includedeleted=true&$count=true&$filter=(updatedAt gt datetimeoffset'{dts1}')&$orderby=updatedAt&$skip=25");
        }
    }
}

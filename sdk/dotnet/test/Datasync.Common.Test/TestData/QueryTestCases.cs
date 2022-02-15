// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Datasync.Common.Test.TestData
{
    [ExcludeFromCodeCoverage]
    public class QueryTestCase
    {
        public QueryTestCase(string pathAndQuery, int itemCount, string nextLinkQuery, long totalCount, string[] firstItems, string username = null)
        {
            PathAndQuery = pathAndQuery;
            ItemCount = itemCount;
            NextLinkQuery = nextLinkQuery;
            TotalCount = totalCount;
            FirstItems = firstItems.ToArray();
            Username = username;
        }

        public string PathAndQuery { get; }

        public int ItemCount { get; }

        public string NextLinkQuery { get; }

        public long TotalCount { get; }

        public string[] FirstItems { get; }

        public string Username { get; }
    }

    [ExcludeFromCodeCoverage]
    public class QueryTestCases : TheoryData<QueryTestCase>
    {
        public static QueryTestCase AuthenticatedTestcase { get; }
            = new QueryTestCase("tables/movies_rated", 95, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }, "success");

        public QueryTestCases()
        {
            /* 001 */ Add(new QueryTestCase("tables/movies", 100, "tables/movies?$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 002 */ Add(new QueryTestCase("tables/movies?$count=true", 100, "tables/movies?$count=true&$skip=100", Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 003 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 2, new[] { "id-061", "id-173" }));
            /* 004 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 13, null, 13, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }));
            /* 005 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=(year div 1000.5) eq 2", 6, null, 6, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }));
            /* 006 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 46, null, 46, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }));
            /* 007 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=(year sub 1900) ge 80", 100, "tables/movies?$count=true&$filter=(year sub 1900) ge 80&$skip=100", 138, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }));
            /* 008 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=bestPictureWinner eq false", 100, "tables/movies?$count=true&$filter=bestPictureWinner eq false&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 009 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 11, null, 11, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }));
            /* 010 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 21, null, 21, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }));
            /* 011 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 24, null, 24, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }));
            /* 012 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=bestPictureWinner eq true", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 013 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=bestPictureWinner ne false", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 014 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=bestPictureWinner ne true", 100, "tables/movies?$count=true&$filter=bestPictureWinner ne true&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 015 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2&$skip=100", 124, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }));
            /* 016 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=day(releaseDate) eq 1", 7, null, 7, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }));
            /* 017 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=duration ge 60", 100, "tables/movies?$count=true&$filter=duration ge 60&$skip=100", Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 018 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=endswith(title, 'er')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 019 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=endswith(tolower(title), 'er')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 020 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=endswith(toupper(title), 'ER')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 021 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2&$skip=100", 120, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }));
            /* 022 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=month(releaseDate) eq 11", 14, null, 14, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }));
            /* 023 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=not(bestPictureWinner eq false)", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 024 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=not(bestPictureWinner eq true)", 100, "tables/movies?$count=true&$filter=not(bestPictureWinner eq true)&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 025 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=not(bestPictureWinner ne false)", 100, "tables/movies?$count=true&$filter=not(bestPictureWinner ne false)&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 026 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=not(bestPictureWinner ne true)", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 027 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=rating eq 'R'", 95, null, 95, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }));
            /* 028 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=rating ne 'PG-13'", 100, "tables/movies?$count=true&$filter=rating ne 'PG-13'&$skip=100", 220, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 029 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=rating eq null", 74, null, 74, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }));
            /* 030 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 2, new[] { "id-000", "id-003" }));
            /* 031 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 032 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 033 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 034 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 035 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=round(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=round(duration div 60.0) eq 2&$skip=100", Movies.MovieList.Count(x => Math.Round(x.Duration / 60.0) == 2.0), new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }));
            /* 036 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=startswith(rating, 'PG')", 68, null, 68, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" }));
            /* 037 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=startswith(tolower(title), 'the')", 63, null, 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }));
            /* 038 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=startswith(toupper(title), 'THE')", 63, null, 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }));
            /* 039 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=year eq 1994", 5, null, 5, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }));
            /* 040 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=year ge 2000 and year le 2009", 55, null, 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }));
            /* 041 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=year ge 2000", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 042 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=year gt 1999 and year lt 2010", 55, null, 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }));
            /* 043 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=year gt 1999", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 044 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=year le 2000", 100, "tables/movies?$count=true&$filter=year le 2000&$skip=100", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 045 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=year lt 2001", 100, "tables/movies?$count=true&$filter=year lt 2001&$skip=100", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 046 */ Add(new QueryTestCase("tables/movies?$count=true&$filter=year(releaseDate) eq 1994", 6, null, 6, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }));
            /* 047 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=bestPictureWinner asc", 100, "tables/movies?$count=true&$orderby=bestPictureWinner asc&$skip=100", Movies.Count, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 048 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=bestPictureWinner desc", 100, "tables/movies?$count=true&$orderby=bestPictureWinner desc&$skip=100", Movies.Count, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 049 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=duration asc", 100, "tables/movies?$count=true&$orderby=duration asc&$skip=100", Movies.Count, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }));
            /* 050 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=duration desc", 100, "tables/movies?$count=true&$orderby=duration desc&$skip=100", Movies.Count, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }));
            /* 051 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=rating asc", 100, "tables/movies?$count=true&$orderby=rating asc&$skip=100", Movies.Count, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }));
            /* 052 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=rating desc", 100, "tables/movies?$count=true&$orderby=rating desc&$skip=100", Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }));
            /* 053 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=releaseDate asc", 100, "tables/movies?$count=true&$orderby=releaseDate asc&$skip=100", Movies.Count, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }));
            /* 054 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=releaseDate desc", 100, "tables/movies?$count=true&$orderby=releaseDate desc&$skip=100", Movies.Count, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }));
            /* 055 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=title asc", 100, "tables/movies?$count=true&$orderby=title asc&$skip=100", Movies.Count, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }));
            /* 056 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=title desc", 100, "tables/movies?$count=true&$orderby=title desc&$skip=100", Movies.Count, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }));
            /* 057 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=year asc", 100, "tables/movies?$count=true&$orderby=year asc&$skip=100", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }));
            /* 058 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=year asc,title asc", 100, "tables/movies?$count=true&$orderby=year asc,title asc&$skip=100", Movies.Count, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }));
            /* 059 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=year asc,title desc", 100, "tables/movies?$count=true&$orderby=year asc,title desc&$skip=100", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }));
            /* 060 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=year desc", 100, "tables/movies?$count=true&$orderby=year desc&$skip=100", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }));
            /* 061 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=year desc,title asc", 100, "tables/movies?$count=true&$orderby=year desc,title asc&$skip=100", Movies.Count, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }));
            /* 062 */ Add(new QueryTestCase("tables/movies?$count=true&$orderby=year desc,title desc", 100, "tables/movies?$count=true&$orderby=year desc,title desc&$skip=100", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }));
            /* 063 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 2, new[] { "id-061", "id-173" }));
            /* 064 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 13, null, 13, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }));
            /* 065 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=(year div 1000.5) eq 2", 6, null, 6, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }));
            /* 066 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 46, null, 46, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }));
            /* 067 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=(year sub 1900) ge 80", 100, "tables/movies?$count=true&$filter=(year sub 1900) ge 80&$skip=100&$top=25", 138, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }));
            /* 068 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq false", 100, "tables/movies?$count=true&$filter=bestPictureWinner eq false&$skip=100&$top=25", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 069 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 11, null, 11, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }));
            /* 070 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 21, null, 21, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }));
            /* 071 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 24, null, 24, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }));
            /* 072 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 073 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=bestPictureWinner ne false", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 074 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=bestPictureWinner ne true", 100, "tables/movies?$count=true&$filter=bestPictureWinner ne true&$skip=100&$top=25", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 075 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=ceiling(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2&$skip=100&$top=25", 124, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }));
            /* 076 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=day(releaseDate) eq 1", 7, null, 7, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }));
            /* 077 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=duration ge 60", 100, "tables/movies?$count=true&$filter=duration ge 60&$skip=100&$top=25", Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 078 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=endswith(title, 'er')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 079 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=endswith(tolower(title), 'er')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 080 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=endswith(toupper(title), 'ER')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 081 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=floor(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2&$skip=100&$top=25", 120, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }));
            /* 082 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=month(releaseDate) eq 11", 14, null, 14, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }));
            /* 083 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner eq false)", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 084 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner eq true)", 100, "tables/movies?$count=true&$filter=not(bestPictureWinner eq true)&$skip=100&$top=25", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 085 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner ne false)", 100, "tables/movies?$count=true&$filter=not(bestPictureWinner ne false)&$skip=100&$top=25", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 086 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner ne true)", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 087 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=rating eq 'R'", 95, null, 95, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }));
            /* 088 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=rating ne 'PG-13'", 100, "tables/movies?$count=true&$filter=rating ne 'PG-13'&$skip=100&$top=25", 220, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 089 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=rating eq null", 74, null, 74, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }));
            /* 090 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 2, new[] { "id-000", "id-003" }));
            /* 091 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 092 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 093 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100&$top=25", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 094 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100&$top=25", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 095 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=round(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=round(duration div 60.0) eq 2&$skip=100&$top=25", 186, new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }));
            /* 096 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=startswith(rating, 'PG')", 68, null, 68, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" }));
            /* 097 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=startswith(tolower(title), 'the')", 63, null, 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }));
            /* 098 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=startswith(toupper(title), 'THE')", 63, null, 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }));
            /* 099 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=year eq 1994", 5, null, 5, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }));
            /* 100 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=year ge 2000 and year le 2009", 55, null, 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }));
            /* 101 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=year ge 2000", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 102 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=year gt 1999 and year lt 2010", 55, null, 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }));
            /* 103 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=year gt 1999", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 104 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=year le 2000", 100, "tables/movies?$count=true&$filter=year le 2000&$skip=100&$top=25", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 105 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=year lt 2001", 100, "tables/movies?$count=true&$filter=year lt 2001&$skip=100&$top=25", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 106 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$filter=year(releaseDate) eq 1994", 6, null, 6, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }));
            /* 107 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=bestPictureWinner asc", 100, "tables/movies?$count=true&$orderby=bestPictureWinner asc&$skip=100&$top=25", Movies.Count, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 108 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=bestPictureWinner desc", 100, "tables/movies?$count=true&$orderby=bestPictureWinner desc&$skip=100&$top=25", Movies.Count, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 109 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=duration asc", 100, "tables/movies?$count=true&$orderby=duration asc&$skip=100&$top=25", Movies.Count, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }));
            /* 110 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=duration desc", 100, "tables/movies?$count=true&$orderby=duration desc&$skip=100&$top=25", Movies.Count, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }));
            /* 111 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=rating asc", 100, "tables/movies?$count=true&$orderby=rating asc&$skip=100&$top=25", Movies.Count, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }));
            /* 112 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=rating desc", 100, "tables/movies?$count=true&$orderby=rating desc&$skip=100&$top=25", Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }));
            /* 113 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=releaseDate asc", 100, "tables/movies?$count=true&$orderby=releaseDate asc&$skip=100&$top=25", Movies.Count, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }));
            /* 114 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=releaseDate desc", 100, "tables/movies?$count=true&$orderby=releaseDate desc&$skip=100&$top=25", Movies.Count, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }));
            /* 115 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=title asc", 100, "tables/movies?$count=true&$orderby=title asc&$skip=100&$top=25", Movies.Count, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }));
            /* 116 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=title desc", 100, "tables/movies?$count=true&$orderby=title desc&$skip=100&$top=25", Movies.Count, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }));
            /* 117 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=year asc", 100, "tables/movies?$count=true&$orderby=year asc&$skip=100&$top=25", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }));
            /* 118 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=year asc,title asc", 100, "tables/movies?$count=true&$orderby=year asc,title asc&$skip=100&$top=25", Movies.Count, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }));
            /* 119 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=year asc,title desc", 100, "tables/movies?$count=true&$orderby=year asc,title desc&$skip=100&$top=25", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }));
            /* 120 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=year desc", 100, "tables/movies?$count=true&$orderby=year desc&$skip=100&$top=25", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }));
            /* 121 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=year desc,title asc", 100, "tables/movies?$count=true&$orderby=year desc,title asc&$skip=100&$top=25", Movies.Count, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }));
            /* 122 */ Add(new QueryTestCase("tables/movies?$count=true&$top=125&$orderby=year desc,title desc", 100, "tables/movies?$count=true&$orderby=year desc,title desc&$skip=100&$top=25", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }));
            /* 123 */ Add(new QueryTestCase("tables/movies?$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 0, new[] { "id-061", "id-173" }));
            /* 124 */ Add(new QueryTestCase("tables/movies?$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 13, null, 0, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }));
            /* 125 */ Add(new QueryTestCase("tables/movies?$filter=(year div 1000.5) eq 2", 6, null, 0, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }));
            /* 126 */ Add(new QueryTestCase("tables/movies?$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 46, null, 0, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }));
            /* 127 */ Add(new QueryTestCase("tables/movies?$filter=(year sub 1900) ge 80", 100, "tables/movies?$filter=(year sub 1900) ge 80&$skip=100", 0, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }));
            /* 128 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner eq false", 100, "tables/movies?$filter=bestPictureWinner eq false&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 129 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 11, null, 0, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }));
            /* 130 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 21, null, 0, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }));
            /* 131 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 24, null, 0, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }));
            /* 132 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner eq true", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 133 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner ne false", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 134 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner ne true", 100, "tables/movies?$filter=bestPictureWinner ne true&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 135 */ Add(new QueryTestCase("tables/movies?$filter=ceiling(duration div 60.0) eq 2", 100, "tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=100", 0, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }));
            /* 136 */ Add(new QueryTestCase("tables/movies?$filter=day(releaseDate) eq 1", 7, null, 0, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }));
            /* 137 */ Add(new QueryTestCase("tables/movies?$filter=duration ge 60", 100, "tables/movies?$filter=duration ge 60&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 138 */ Add(new QueryTestCase("tables/movies?$filter=endswith(title, 'er')", 12, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 139 */ Add(new QueryTestCase("tables/movies?$filter=endswith(tolower(title), 'er')", 12, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 140 */ Add(new QueryTestCase("tables/movies?$filter=endswith(toupper(title), 'ER')", 12, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 141 */ Add(new QueryTestCase("tables/movies?$filter=floor(duration div 60.0) eq 2", 100, "tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=100", 0, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }));
            /* 142 */ Add(new QueryTestCase("tables/movies?$filter=month(releaseDate) eq 11", 14, null, 0, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }));
            /* 143 */ Add(new QueryTestCase("tables/movies?$filter=not(bestPictureWinner eq false)", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 144 */ Add(new QueryTestCase("tables/movies?$filter=not(bestPictureWinner eq true)", 100, "tables/movies?$filter=not(bestPictureWinner eq true)&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 145 */ Add(new QueryTestCase("tables/movies?$filter=not(bestPictureWinner ne false)", 100, "tables/movies?$filter=not(bestPictureWinner ne false)&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 146 */ Add(new QueryTestCase("tables/movies?$filter=not(bestPictureWinner ne true)", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 147 */ Add(new QueryTestCase("tables/movies?$filter=rating eq 'R'", 95, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }));
            /* 148 */ Add(new QueryTestCase("tables/movies?$filter=rating ne 'PG-13'", 100, "tables/movies?$filter=rating ne 'PG-13'&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 149 */ Add(new QueryTestCase("tables/movies?$filter=rating eq null", 74, null, 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }));
            /* 150 */ Add(new QueryTestCase("tables/movies?$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 0, new[] { "id-000", "id-003" }));
            /* 151 */ Add(new QueryTestCase("tables/movies?$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 152 */ Add(new QueryTestCase("tables/movies?$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 153 */ Add(new QueryTestCase("tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 154 */ Add(new QueryTestCase("tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 155 */ Add(new QueryTestCase("tables/movies?$filter=round(duration div 60.0) eq 2", 100, "tables/movies?$filter=round(duration div 60.0) eq 2&$skip=100", 0, new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }));
            /* 156 */ Add(new QueryTestCase("tables/movies?$filter=startswith(rating, 'PG')", 68, null, 0, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" }));
            /* 157 */ Add(new QueryTestCase("tables/movies?$filter=startswith(tolower(title), 'the')", 63, null, 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }));
            /* 158 */ Add(new QueryTestCase("tables/movies?$filter=startswith(toupper(title), 'THE')", 63, null, 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }));
            /* 159 */ Add(new QueryTestCase("tables/movies?$filter=year eq 1994", 5, null, 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }));
            /* 160 */ Add(new QueryTestCase("tables/movies?$filter=year ge 2000 and year le 2009", 55, null, 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }));
            /* 161 */ Add(new QueryTestCase("tables/movies?$filter=year ge 2000", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 162 */ Add(new QueryTestCase("tables/movies?$filter=year gt 1999 and year lt 2010", 55, null, 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }));
            /* 163 */ Add(new QueryTestCase("tables/movies?$filter=year gt 1999", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 164 */ Add(new QueryTestCase("tables/movies?$filter=year le 2000", 100, "tables/movies?$filter=year le 2000&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 165 */ Add(new QueryTestCase("tables/movies?$filter=year lt 2001", 100, "tables/movies?$filter=year lt 2001&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 166 */ Add(new QueryTestCase("tables/movies?$filter=year(releaseDate) eq 1994", 6, null, 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }));
            /* 167 */ Add(new QueryTestCase("tables/movies?$orderby=bestPictureWinner asc", 100, "tables/movies?$orderby=bestPictureWinner asc&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 168 */ Add(new QueryTestCase("tables/movies?$orderby=bestPictureWinner desc", 100, "tables/movies?$orderby=bestPictureWinner desc&$skip=100", 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 169 */ Add(new QueryTestCase("tables/movies?$orderby=duration asc", 100, "tables/movies?$orderby=duration asc&$skip=100", 0, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }));
            /* 170 */ Add(new QueryTestCase("tables/movies?$orderby=duration desc", 100, "tables/movies?$orderby=duration desc&$skip=100", 0, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }));
            /* 171 */ Add(new QueryTestCase("tables/movies?$orderby=rating asc", 100, "tables/movies?$orderby=rating asc&$skip=100", 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }));
            /* 172 */ Add(new QueryTestCase("tables/movies?$orderby=rating desc", 100, "tables/movies?$orderby=rating desc&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }));
            /* 173 */ Add(new QueryTestCase("tables/movies?$orderby=releaseDate asc", 100, "tables/movies?$orderby=releaseDate asc&$skip=100", 0, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }));
            /* 174 */ Add(new QueryTestCase("tables/movies?$orderby=releaseDate desc", 100, "tables/movies?$orderby=releaseDate desc&$skip=100", 0, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }));
            /* 175 */ Add(new QueryTestCase("tables/movies?$orderby=title asc", 100, "tables/movies?$orderby=title asc&$skip=100", 0, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }));
            /* 176 */ Add(new QueryTestCase("tables/movies?$orderby=title desc", 100, "tables/movies?$orderby=title desc&$skip=100", 0, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }));
            /* 177 */ Add(new QueryTestCase("tables/movies?$orderby=year asc", 100, "tables/movies?$orderby=year asc&$skip=100", 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }));
            /* 178 */ Add(new QueryTestCase("tables/movies?$orderby=year asc,title asc", 100, "tables/movies?$orderby=year asc,title asc&$skip=100", 0, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }));
            /* 179 */ Add(new QueryTestCase("tables/movies?$orderby=year asc,title desc", 100, "tables/movies?$orderby=year asc,title desc&$skip=100", 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }));
            /* 180 */ Add(new QueryTestCase("tables/movies?$orderby=year desc", 100, "tables/movies?$orderby=year desc&$skip=100", 0, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }));
            /* 181 */ Add(new QueryTestCase("tables/movies?$orderby=year desc,title asc", 100, "tables/movies?$orderby=year desc,title asc&$skip=100", 0, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }));
            /* 182 */ Add(new QueryTestCase("tables/movies?$orderby=year desc,title desc", 100, "tables/movies?$orderby=year desc,title desc&$skip=100", 0, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }));
            /* 183 */ Add(new QueryTestCase("tables/movies?$filter=((year div 1000.5) eq 2) and (rating eq 'R')&$skip=5", 0, null, 0, Array.Empty<string>()));
            /* 184 */ Add(new QueryTestCase("tables/movies?$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)&$skip=5", 8, null, 0, new[] { "id-142", "id-143", "id-162", "id-166", "id-172" }));
            /* 185 */ Add(new QueryTestCase("tables/movies?$filter=(year div 1000.5) eq 2&$skip=5", 1, null, 0, new[] { "id-216" }));
            /* 186 */ Add(new QueryTestCase("tables/movies?$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)&$skip=5", 41, null, 0, new[] { "id-040", "id-041", "id-044", "id-046", "id-049" }));
            /* 187 */ Add(new QueryTestCase("tables/movies?$filter=(year sub 1900) ge 80&$skip=5", 100, "tables/movies?$filter=(year sub 1900) ge 80&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }));
            /* 188 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner eq false&$skip=5", 100, "tables/movies?$filter=bestPictureWinner eq false&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }));
            /* 189 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2&$skip=5", 6, null, 0, new[] { "id-150", "id-155", "id-186", "id-189", "id-196" }));
            /* 190 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2&$skip=5", 16, null, 0, new[] { "id-062", "id-083", "id-087", "id-092", "id-093" }));
            /* 191 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2&$skip=5", 19, null, 0, new[] { "id-092", "id-093", "id-094", "id-096", "id-112" }));
            /* 192 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner eq true&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }));
            /* 193 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner ne false&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }));
            /* 194 */ Add(new QueryTestCase("tables/movies?$filter=bestPictureWinner ne true&$skip=5", 100, "tables/movies?$filter=bestPictureWinner ne true&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }));
            /* 195 */ Add(new QueryTestCase("tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=5", 100, "tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=105", 0, new[] { "id-027", "id-028", "id-030", "id-031", "id-032" }));
            /* 196 */ Add(new QueryTestCase("tables/movies?$filter=day(releaseDate) eq 1&$skip=5", 2, null, 0, new[] { "id-197", "id-215" }));
            /* 197 */ Add(new QueryTestCase("tables/movies?$filter=duration ge 60&$skip=5", 100, "tables/movies?$filter=duration ge 60&$skip=105", 0, new[] { "id-005", "id-006", "id-007", "id-008", "id-009" }));
            /* 198 */ Add(new QueryTestCase("tables/movies?$filter=endswith(title, 'er')&$skip=5", 7, null, 0, new[] { "id-170", "id-193", "id-197", "id-205", "id-217" }));
            /* 199 */ Add(new QueryTestCase("tables/movies?$filter=endswith(tolower(title), 'er')&$skip=5", 7, null, 0, new[] { "id-170", "id-193", "id-197", "id-205", "id-217" }));
            /* 200 */ Add(new QueryTestCase("tables/movies?$filter=endswith(toupper(title), 'ER')&$skip=5", 7, null, 0, new[] { "id-170", "id-193", "id-197", "id-205", "id-217" }));
            /* 201 */ Add(new QueryTestCase("tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=5", 100, "tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=105", 0, new[] { "id-009", "id-010", "id-011", "id-012", "id-013" }));
            /* 202 */ Add(new QueryTestCase("tables/movies?$filter=month(releaseDate) eq 11&$skip=5", 9, null, 0, new[] { "id-115", "id-131", "id-136", "id-146", "id-167" }));
            /* 203 */ Add(new QueryTestCase("tables/movies?$filter=not(bestPictureWinner eq false)&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }));
            /* 204 */ Add(new QueryTestCase("tables/movies?$filter=not(bestPictureWinner eq true)&$skip=5", 100, "tables/movies?$filter=not(bestPictureWinner eq true)&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }));
            /* 205 */ Add(new QueryTestCase("tables/movies?$filter=not(bestPictureWinner ne false)&$skip=5", 100, "tables/movies?$filter=not(bestPictureWinner ne false)&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }));
            /* 206 */ Add(new QueryTestCase("tables/movies?$filter=not(bestPictureWinner ne true)&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }));
            /* 207 */ Add(new QueryTestCase("tables/movies?$filter=rating eq null&$skip=5", 69, null, 0, new[] { "id-040", "id-041", "id-044", "id-046", "id-049" }));
            /* 208 */ Add(new QueryTestCase("tables/movies?$filter=rating eq 'R'&$skip=5", 90, null, 0, new[] { "id-009", "id-014", "id-017", "id-019", "id-022" }));
            /* 209 */ Add(new QueryTestCase("tables/movies?$filter=rating ne 'PG-13'&$skip=5", 100, "tables/movies?$filter=rating ne 'PG-13'&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }));
            /* 210 */ Add(new QueryTestCase("tables/movies?$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 0, null, 0, Array.Empty<string>()));
            /* 211 */ Add(new QueryTestCase("tables/movies?$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }));
            /* 212 */ Add(new QueryTestCase("tables/movies?$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }));
            /* 213 */ Add(new QueryTestCase("tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 100, "tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }));
            /* 214 */ Add(new QueryTestCase("tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 100, "tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }));
            /* 215 */ Add(new QueryTestCase("tables/movies?$filter=round(duration div 60.0) eq 2&$skip=5", 100, "tables/movies?$filter=round(duration div 60.0) eq 2&$skip=105", 0, new[] { "id-013", "id-014", "id-015", "id-016", "id-017" }));
            /* 216 */ Add(new QueryTestCase("tables/movies?$filter=startswith(rating, 'PG')&$skip=5", 63, null, 0, new[] { "id-015", "id-018", "id-020", "id-021", "id-024" }));
            /* 217 */ Add(new QueryTestCase("tables/movies?$filter=startswith(tolower(title), 'the')&$skip=5", 58, null, 0, new[] { "id-008", "id-012", "id-017", "id-020", "id-023" }));
            /* 218 */ Add(new QueryTestCase("tables/movies?$filter=startswith(toupper(title), 'THE')&$skip=5", 58, null, 0, new[] { "id-008", "id-012", "id-017", "id-020", "id-023" }));
            /* 219 */ Add(new QueryTestCase("tables/movies?$filter=year eq 1994&$skip=5", 0, null, 0, Array.Empty<string>()));
            /* 220 */ Add(new QueryTestCase("tables/movies?$filter=year ge 2000 and year le 2009&$skip=5", 50, null, 0, new[] { "id-032", "id-042", "id-050", "id-051", "id-058" }));
            /* 221 */ Add(new QueryTestCase("tables/movies?$filter=year ge 2000&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }));
            /* 222 */ Add(new QueryTestCase("tables/movies?$filter=year gt 1999 and year lt 2010&$skip=5", 50, null, 0, new[] { "id-032", "id-042", "id-050", "id-051", "id-058" }));
            /* 223 */ Add(new QueryTestCase("tables/movies?$filter=year gt 1999&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }));
            /* 224 */ Add(new QueryTestCase("tables/movies?$filter=year le 2000&$skip=5", 100, "tables/movies?$filter=year le 2000&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }));
            /* 225 */ Add(new QueryTestCase("tables/movies?$filter=year lt 2001&$skip=5", 100, "tables/movies?$filter=year lt 2001&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }));
            /* 226 */ Add(new QueryTestCase("tables/movies?$filter=year(releaseDate) eq 1994&$skip=5", 1, null, 0, new[] { "id-217" }));
            /* 227 */ Add(new QueryTestCase("tables/movies?$orderby=bestPictureWinner asc&$skip=5", 100, "tables/movies?$orderby=bestPictureWinner asc&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }));
            /* 228 */ Add(new QueryTestCase("tables/movies?$orderby=bestPictureWinner desc&$skip=5", 100, "tables/movies?$orderby=bestPictureWinner desc&$skip=105", 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }));
            /* 229 */ Add(new QueryTestCase("tables/movies?$orderby=duration asc&$skip=5", 100, "tables/movies?$orderby=duration asc&$skip=105", 0, new[] { "id-238", "id-201", "id-115", "id-229", "id-181" }));
            /* 230 */ Add(new QueryTestCase("tables/movies?$orderby=duration desc&$skip=5", 100, "tables/movies?$orderby=duration desc&$skip=105", 0, new[] { "id-007", "id-183", "id-063", "id-202", "id-130" }));
            /* 231 */ Add(new QueryTestCase("tables/movies?$orderby=rating asc&$skip=5", 100, "tables/movies?$orderby=rating asc&$skip=105", 0, new[] { "id-040", "id-041", "id-044", "id-046", "id-049" }));
            /* 232 */ Add(new QueryTestCase("tables/movies?$orderby=rating desc&$skip=5", 100, "tables/movies?$orderby=rating desc&$skip=105", 0, new[] { "id-009", "id-014", "id-017", "id-019", "id-022" }));
            /* 233 */ Add(new QueryTestCase("tables/movies?$orderby=releaseDate asc&$skip=5", 100, "tables/movies?$orderby=releaseDate asc&$skip=105", 0, new[] { "id-229", "id-224", "id-041", "id-049", "id-135" }));
            /* 234 */ Add(new QueryTestCase("tables/movies?$orderby=releaseDate desc&$skip=5", 100, "tables/movies?$orderby=releaseDate desc&$skip=105", 0, new[] { "id-149", "id-213", "id-102", "id-155", "id-169" }));
            /* 235 */ Add(new QueryTestCase("tables/movies?$orderby=title asc&$skip=5", 100, "tables/movies?$orderby=title asc&$skip=105", 0, new[] { "id-214", "id-102", "id-215", "id-039", "id-057" }));
            /* 236 */ Add(new QueryTestCase("tables/movies?$orderby=title desc&$skip=5", 100, "tables/movies?$orderby=title desc&$skip=105", 0, new[] { "id-058", "id-046", "id-160", "id-092", "id-176" }));
            /* 237 */ Add(new QueryTestCase("tables/movies?$orderby=year asc&$skip=5", 100, "tables/movies?$orderby=year asc&$skip=105", 0, new[] { "id-088", "id-224", "id-041", "id-049", "id-135" }));
            /* 238 */ Add(new QueryTestCase("tables/movies?$orderby=year asc,title asc&$skip=5", 100, "tables/movies?$orderby=year asc,title asc&$skip=105", 0, new[] { "id-088", "id-224", "id-041", "id-049", "id-135" }));
            /* 239 */ Add(new QueryTestCase("tables/movies?$orderby=year asc,title desc&$skip=5", 100, "tables/movies?$orderby=year asc,title desc&$skip=105", 0, new[] { "id-088", "id-224", "id-049", "id-041", "id-135" }));
            /* 240 */ Add(new QueryTestCase("tables/movies?$orderby=year desc&$skip=5", 100, "tables/movies?$orderby=year desc&$skip=105", 0, new[] { "id-149", "id-186", "id-213", "id-013", "id-053" }));
            /* 241 */ Add(new QueryTestCase("tables/movies?$orderby=year desc,title asc&$skip=5", 100, "tables/movies?$orderby=year desc,title asc&$skip=105", 0, new[] { "id-186", "id-064", "id-149", "id-169", "id-161" }));
            /* 242 */ Add(new QueryTestCase("tables/movies?$orderby=year desc,title desc&$skip=5", 100, "tables/movies?$orderby=year desc,title desc&$skip=105", 0, new[] { "id-186", "id-213", "id-102", "id-053", "id-155" }));
            /* 243 */ Add(new QueryTestCase("tables/movies?$skip=0", 100, "tables/movies?$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 244 */ Add(new QueryTestCase("tables/movies?$skip=100", 100, "tables/movies?$skip=200", 0, new[] { "id-100", "id-101", "id-102", "id-103", "id-104" }));
            /* 245 */ Add(new QueryTestCase("tables/movies?$skip=200", 48, null, 0, new[] { "id-200", "id-201", "id-202", "id-203", "id-204" }));
            /* 246 */ Add(new QueryTestCase("tables/movies?$skip=300", 0, null, 0, Array.Empty<string>()));
            /* 247 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 0, new[] { "id-061", "id-173" }));
            /* 248 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 5, null, 0, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }));
            /* 249 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=(year div 1000.5) eq 2", 5, null, 0, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }));
            /* 250 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 5, null, 0, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }));
            /* 251 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=(year sub 1900) ge 80", 5, null, 0, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }));
            /* 252 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=bestPictureWinner eq false", 5, null, 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 253 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 5, null, 0, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }));
            /* 254 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 5, null, 0, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }));
            /* 255 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 5, null, 0, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }));
            /* 256 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=bestPictureWinner eq true", 5, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 257 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=bestPictureWinner ne false", 5, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 258 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=bestPictureWinner ne true", 5, null, 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 259 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=ceiling(duration div 60.0) eq 2", 5, null, 0, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }));
            /* 260 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=day(releaseDate) eq 1", 5, null, 0, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }));
            /* 261 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=duration ge 60", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 262 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=endswith(title, 'er')", 5, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 263 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=endswith(tolower(title), 'er')", 5, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 264 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=endswith(toupper(title), 'ER')", 5, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }));
            /* 265 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=floor(duration div 60.0) eq 2", 5, null, 0, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }));
            /* 266 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=month(releaseDate) eq 11", 5, null, 0, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }));
            /* 267 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=not(bestPictureWinner eq false)", 5, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 268 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=not(bestPictureWinner eq true)", 5, null, 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 269 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=not(bestPictureWinner ne false)", 5, null, 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 270 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=not(bestPictureWinner ne true)", 5, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 271 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=rating eq 'R'", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }));
            /* 272 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=rating ne 'PG-13'", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 273 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=rating eq null", 5, null, 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }));
            /* 274 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 0, new[] { "id-000", "id-003" }));
            /* 275 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 276 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 277 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 278 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 279 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=round(duration div 60.0) eq 2", 5, null, 0, new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }));
            /* 280 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=startswith(rating, 'PG')", 5, null, 0, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" }));
            /* 281 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=startswith(tolower(title), 'the')", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }));
            /* 282 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=startswith(toupper(title), 'THE')", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }));
            /* 283 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=year eq 1994", 5, null, 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }));
            /* 284 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=year ge 2000 and year le 2009", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }));
            /* 285 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=year ge 2000", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 286 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=year gt 1999 and year lt 2010", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }));
            /* 287 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=year gt 1999", 5, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }));
            /* 288 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=year le 2000", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 289 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=year lt 2001", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }));
            /* 290 */ Add(new QueryTestCase("tables/movies?$top=5&$filter=year(releaseDate) eq 1994", 5, null, 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }));
            /* 291 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=bestPictureWinner asc", 5, null, 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }));
            /* 292 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=bestPictureWinner desc", 5, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }));
            /* 293 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=duration asc", 5, null, 0, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }));
            /* 294 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=duration desc", 5, null, 0, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }));
            /* 295 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=rating asc", 5, null, 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }));
            /* 296 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=rating desc", 5, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }));
            /* 297 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=releaseDate asc", 5, null, 0, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }));
            /* 298 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=releaseDate desc", 5, null, 0, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }));
            /* 299 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=title asc", 5, null, 0, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }));
            /* 300 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=title desc", 5, null, 0, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }));
            /* 301 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=year asc", 5, null, 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }));
            /* 302 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=year asc,title asc", 5, null, 0, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }));
            /* 303 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=year asc,title desc", 5, null, 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }));
            /* 304 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=year desc", 5, null, 0, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }));
            /* 305 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=year desc,title asc", 5, null, 0, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }));
            /* 306 */ Add(new QueryTestCase("tables/movies?$top=5&$orderby=year desc,title desc", 5, null, 0, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }));
        }
    }
}

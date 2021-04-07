// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AzureMobile.Common.Test;
using Microsoft.AzureMobile.Common.Test.Extensions;
using Microsoft.AzureMobile.Common.Test.Models;
using Microsoft.AzureMobile.Common.Test.TestData;
using Microsoft.AzureMobile.WebService.Test;
using Xunit;

namespace Microsoft.AzureMobile.Server.Test.Tables.HTTP
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Query_Tests
    {
        #region Test Artifacts
        public class PageOfItems<T> where T : class
        {
            public T[] Items { get; set; }
            public long Count { get; set; }
            public Uri NextLink { get; set; }
        }

        public class ClientObject
        {
            [JsonExtensionData]
            public Dictionary<string, object> Data { get; set; }
        }
        #endregion

        /// <summary>
        /// Basic query tests - these will do tests against tables/movies in various modes to ensure that the OData
        /// query items pass.
        /// </summary>
        /// <param name="query">The query to send</param>
        /// <param name="expectedItemCount">Response: the number of items</param>
        /// <param name="expectedNextLinkQuery">Response: the NextLink entity</param>
        /// <param name="expectedTotalCount">Response: The Count entity</param>
        /// <param name="firstExpectedItems">Response: The IDs of the first elements in the Items entity</param>
        /// <param name="headerName">Optional header name to send with the request</param>
        /// <param name="headerValue">Optional header value to send with the request</param>
        [Theory]
        [InlineData("tables/movies", 100, "tables/movies?$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true", 100, "tables/movies?$count=true&$skip=100", Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 2, new[] { "id-061", "id-173" })]
        [InlineData("tables/movies?$count=true&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 13, null, 13, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" })]
        [InlineData("tables/movies?$count=true&$filter=(year div 1000.5) eq 2", 6, null, 6, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" })]
        [InlineData("tables/movies?$count=true&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 46, null, 46, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" })]
        [InlineData("tables/movies?$count=true&$filter=(year sub 1900) ge 80", 100, "tables/movies?$count=true&$filter=(year sub 1900) ge 80&$skip=100", 138, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner eq false", 100, "tables/movies?$count=true&$filter=bestPictureWinner eq false&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 11, null, 11, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 21, null, 21, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 24, null, 24, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner eq true", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner ne false", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$filter=bestPictureWinner ne true", 100, "tables/movies?$count=true&$filter=bestPictureWinner ne true&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2&$skip=100", 124, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" })]
        [InlineData("tables/movies?$count=true&$filter=day(releaseDate) eq 1", 7, null, 7, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" })]
        [InlineData("tables/movies?$count=true&$filter=duration ge 60", 100, "tables/movies?$count=true&$filter=duration ge 60&$skip=100", Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=endswith(title, 'er')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$filter=endswith(tolower(title), 'er')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$filter=endswith(toupper(title), 'ER')", 12, null, 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2&$skip=100", 120, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=month(releaseDate) eq 11", 14, null, 14, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" })]
        [InlineData("tables/movies?$count=true&$filter=not(bestPictureWinner eq false)", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$filter=not(bestPictureWinner eq true)", 100, "tables/movies?$count=true&$filter=not(bestPictureWinner eq true)&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=not(bestPictureWinner ne false)", 100, "tables/movies?$count=true&$filter=not(bestPictureWinner ne false)&$skip=100", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=not(bestPictureWinner ne true)", 38, null, 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$filter=rating eq 'R'", 94, null, 94, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/movies?$count=true&$filter=rating ne 'PG-13'", 100, "tables/movies?$count=true&$filter=rating ne 'PG-13'&$skip=100", 220, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=rating eq null", 74, null, 74, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$count=true&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 2, new[] { "id-000", "id-003" })]
        [InlineData("tables/movies?$count=true&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=round(duration div 60.0) eq 2", 100, "tables/movies?$count=true&$filter=round(duration div 60.0) eq 2&$skip=100", 186, new[] { "id-000", "id-005", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$count=true&$filter=startsWith(rating, 'PG')", 64, null, 64, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" })]
        [InlineData("tables/movies?$count=true&$filter=startsWith(tolower(title), 'the')", 63, null, 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=startsWith(toupper(title), 'THE')", 63, null, 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$filter=year eq 1994", 5, null, 5, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$count=true&$filter=year ge 2000 and year le 2009", 55, null, 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$count=true&$filter=year ge 2000", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$filter=year gt 1999 and year lt 2010", 55, null, 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$count=true&$filter=year gt 1999", 69, null, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$filter=year le 2000", 100, "tables/movies?$count=true&$filter=year le 2000&$skip=100", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=year lt 2001", 100, "tables/movies?$count=true&$filter=year lt 2001&$skip=100", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$filter=year(releaseDate) eq 1994", 6, null, 6, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$count=true&$orderBy=bestPictureWinner asc", 100, "tables/movies?$count=true&$orderBy=bestPictureWinner asc&$skip=100", Movies.Count, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$orderBy=bestPictureWinner desc", 100, "tables/movies?$count=true&$orderBy=bestPictureWinner desc&$skip=100", Movies.Count, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$orderBy=duration asc", 100, "tables/movies?$count=true&$orderBy=duration asc&$skip=100", Movies.Count, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" })]
        [InlineData("tables/movies?$count=true&$orderBy=duration desc", 100, "tables/movies?$count=true&$orderBy=duration desc&$skip=100", Movies.Count, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" })]
        [InlineData("tables/movies?$count=true&$orderBy=rating asc", 100, "tables/movies?$count=true&$orderBy=rating asc&$skip=100", Movies.Count, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$count=true&$orderBy=rating desc", 100, "tables/movies?$count=true&$orderBy=rating desc&$skip=100", Movies.Count, new[] { "id-197", "id-107", "id-115", "id-028", "id-039" })]
        [InlineData("tables/movies?$count=true&$orderBy=releaseDate asc", 100, "tables/movies?$count=true&$orderBy=releaseDate asc&$skip=100", Movies.Count, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" })]
        [InlineData("tables/movies?$count=true&$orderBy=releaseDate desc", 100, "tables/movies?$count=true&$orderBy=releaseDate desc&$skip=100", Movies.Count, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" })]
        [InlineData("tables/movies?$count=true&$orderBy=title asc", 100, "tables/movies?$count=true&$orderBy=title asc&$skip=100", Movies.Count, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" })]
        [InlineData("tables/movies?$count=true&$orderBy=title desc", 100, "tables/movies?$count=true&$orderBy=title desc&$skip=100", Movies.Count, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" })]
        [InlineData("tables/movies?$count=true&$orderBy=year asc", 100, "tables/movies?$count=true&$orderBy=year asc&$skip=100", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$count=true&$orderBy=year asc,title asc", 100, "tables/movies?$count=true&$orderBy=year asc,title asc&$skip=100", Movies.Count, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" })]
        [InlineData("tables/movies?$count=true&$orderBy=year asc,title desc", 100, "tables/movies?$count=true&$orderBy=year asc,title desc&$skip=100", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$count=true&$orderBy=year desc", 100, "tables/movies?$count=true&$orderBy=year desc&$skip=100", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" })]
        [InlineData("tables/movies?$count=true&$orderBy=year desc,title asc", 100, "tables/movies?$count=true&$orderBy=year desc,title asc&$skip=100", Movies.Count, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" })]
        [InlineData("tables/movies?$count=true&$orderBy=year desc,title desc", 100, "tables/movies?$count=true&$orderBy=year desc,title desc&$skip=100", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 2, new[] { "id-061", "id-173" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 5, "tables/movies?$count=true&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)&$skip=5&$top=5", 13, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=(year div 1000.5) eq 2", 5, "tables/movies?$count=true&$filter=(year div 1000.5) eq 2&$skip=5&$top=5", 6, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 5, "tables/movies?$count=true&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)&$skip=5&$top=5", 46, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=(year sub 1900) ge 80", 5, "tables/movies?$count=true&$filter=(year sub 1900) ge 80&$skip=5&$top=5", 138, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=bestPictureWinner eq false", 5, "tables/movies?$count=true&$filter=bestPictureWinner eq false&$skip=5&$top=5", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 5, "tables/movies?$count=true&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2&$skip=5&$top=5", 11, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 5, "tables/movies?$count=true&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2&$skip=5&$top=5", 21, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 5, "tables/movies?$count=true&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2&$skip=5&$top=5", 24, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=bestPictureWinner eq true", 5, "tables/movies?$count=true&$filter=bestPictureWinner eq true&$skip=5&$top=5", 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=bestPictureWinner ne false", 5, "tables/movies?$count=true&$filter=bestPictureWinner ne false&$skip=5&$top=5", 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=bestPictureWinner ne true", 5, "tables/movies?$count=true&$filter=bestPictureWinner ne true&$skip=5&$top=5", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=ceiling(duration div 60.0) eq 2", 5, "tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2&$skip=5&$top=5", 124, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=day(releaseDate) eq 1", 5, "tables/movies?$count=true&$filter=day(releaseDate) eq 1&$skip=5&$top=5", 7, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=duration ge 60", 5, "tables/movies?$count=true&$filter=duration ge 60&$skip=5&$top=5", Movies.Count, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=endswith(title, 'er')", 5, "tables/movies?$count=true&$filter=endswith(title, 'er')&$skip=5&$top=5", 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=endswith(tolower(title), 'er')", 5, "tables/movies?$count=true&$filter=endswith(tolower(title), 'er')&$skip=5&$top=5", 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=endswith(toupper(title), 'ER')", 5, "tables/movies?$count=true&$filter=endswith(toupper(title), 'ER')&$skip=5&$top=5", 12, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=floor(duration div 60.0) eq 2", 5, "tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2&$skip=5&$top=5", 120, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=month(releaseDate) eq 11", 5, "tables/movies?$count=true&$filter=month(releaseDate) eq 11&$skip=5&$top=5", 14, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=not(bestPictureWinner eq false)", 5, "tables/movies?$count=true&$filter=not(bestPictureWinner eq false)&$skip=5&$top=5", 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=not(bestPictureWinner eq true)", 5, "tables/movies?$count=true&$filter=not(bestPictureWinner eq true)&$skip=5&$top=5", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=not(bestPictureWinner ne false)", 5, "tables/movies?$count=true&$filter=not(bestPictureWinner ne false)&$skip=5&$top=5", 210, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=not(bestPictureWinner ne true)", 5, "tables/movies?$count=true&$filter=not(bestPictureWinner ne true)&$skip=5&$top=5", 38, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=rating eq 'R'", 5, "tables/movies?$count=true&$filter=rating eq 'R'&$skip=5&$top=5", 94, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=rating ne 'PG-13'", 5, "tables/movies?$count=true&$filter=rating ne 'PG-13'&$skip=5&$top=5", 220, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=rating eq null", 5, "tables/movies?$count=true&$filter=rating eq null&$skip=5&$top=5", 74, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 2, new[] { "id-000", "id-003" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 5, "tables/movies?$count=true&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5&$top=5", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 5, "tables/movies?$count=true&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5&$top=5", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 5, "tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5&$top=5", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 5, "tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5&$top=5", 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=round(duration div 60.0) eq 2", 5, "tables/movies?$count=true&$filter=round(duration div 60.0) eq 2&$skip=5&$top=5", 186, new[] { "id-000", "id-005", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=startsWith(rating, 'PG')", 5, "tables/movies?$count=true&$filter=startsWith(rating, 'PG')&$skip=5&$top=5", 64, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=startsWith(tolower(title), 'the')", 5, "tables/movies?$count=true&$filter=startsWith(tolower(title), 'the')&$skip=5&$top=5", 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=startsWith(toupper(title), 'THE')", 5, "tables/movies?$count=true&$filter=startsWith(toupper(title), 'THE')&$skip=5&$top=5", 63, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=year eq 1994", 5, null, 5, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=year ge 2000 and year le 2009", 5, "tables/movies?$count=true&$filter=year ge 2000 and year le 2009&$skip=5&$top=5", 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=year ge 2000", 5, "tables/movies?$count=true&$filter=year ge 2000&$skip=5&$top=5", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=year gt 1999 and year lt 2010", 5, "tables/movies?$count=true&$filter=year gt 1999 and year lt 2010&$skip=5&$top=5", 55, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=year gt 1999", 5, "tables/movies?$count=true&$filter=year gt 1999&$skip=5&$top=5", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=year le 2000", 5, "tables/movies?$count=true&$filter=year le 2000&$skip=5&$top=5", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=year lt 2001", 5, "tables/movies?$count=true&$filter=year lt 2001&$skip=5&$top=5", 185, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$count=true&$top=5&$filter=year(releaseDate) eq 1994", 5, "tables/movies?$count=true&$filter=year(releaseDate) eq 1994&$skip=5&$top=5", 6, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=bestPictureWinner asc", 5, "tables/movies?$count=true&$orderBy=bestPictureWinner asc&$skip=5&$top=5", Movies.Count, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=bestPictureWinner desc", 5, "tables/movies?$count=true&$orderBy=bestPictureWinner desc&$skip=5&$top=5", Movies.Count, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=duration asc", 5, "tables/movies?$count=true&$orderBy=duration asc&$skip=5&$top=5", Movies.Count, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=duration desc", 5, "tables/movies?$count=true&$orderBy=duration desc&$skip=5&$top=5", Movies.Count, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=rating asc", 5, "tables/movies?$count=true&$orderBy=rating asc&$skip=5&$top=5", Movies.Count, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=rating desc", 5, "tables/movies?$count=true&$orderBy=rating desc&$skip=5&$top=5", Movies.Count, new[] { "id-197", "id-107", "id-115", "id-028", "id-039" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=releaseDate asc", 5, "tables/movies?$count=true&$orderBy=releaseDate asc&$skip=5&$top=5", Movies.Count, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=releaseDate desc", 5, "tables/movies?$count=true&$orderBy=releaseDate desc&$skip=5&$top=5", Movies.Count, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=title asc", 5, "tables/movies?$count=true&$orderBy=title asc&$skip=5&$top=5", Movies.Count, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=title desc", 5, "tables/movies?$count=true&$orderBy=title desc&$skip=5&$top=5", Movies.Count, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=year asc", 5, "tables/movies?$count=true&$orderBy=year asc&$skip=5&$top=5", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=year asc,title asc", 5, "tables/movies?$count=true&$orderBy=year asc,title asc&$skip=5&$top=5", Movies.Count, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=year asc,title desc", 5, "tables/movies?$count=true&$orderBy=year asc,title desc&$skip=5&$top=5", Movies.Count, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=year desc", 5, "tables/movies?$count=true&$orderBy=year desc&$skip=5&$top=5", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=year desc,title asc", 5, "tables/movies?$count=true&$orderBy=year desc,title asc&$skip=5&$top=5", Movies.Count, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" })]
        [InlineData("tables/movies?$count=true&$top=5&$orderBy=year desc,title desc", 5, "tables/movies?$count=true&$orderBy=year desc,title desc&$skip=5&$top=5", Movies.Count, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" })]
        [InlineData("tables/movies?$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 0, new[] { "id-061", "id-173" })]
        [InlineData("tables/movies?$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 13, null, 0, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" })]
        [InlineData("tables/movies?$filter=(year div 1000.5) eq 2", 6, null, 0, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" })]
        [InlineData("tables/movies?$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 46, null, 0, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" })]
        [InlineData("tables/movies?$filter=(year sub 1900) ge 80", 100, "tables/movies?$filter=(year sub 1900) ge 80&$skip=100", 0, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq false", 100, "tables/movies?$filter=bestPictureWinner eq false&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 11, null, 0, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 21, null, 0, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 24, null, 0, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$filter=bestPictureWinner ne false", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$filter=bestPictureWinner ne true", 100, "tables/movies?$filter=bestPictureWinner ne true&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$filter=ceiling(duration div 60.0) eq 2", 100, "tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=100", 0, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" })]
        [InlineData("tables/movies?$filter=day(releaseDate) eq 1", 7, null, 0, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" })]
        [InlineData("tables/movies?$filter=duration ge 60", 100, "tables/movies?$filter=duration ge 60&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=endswith(title, 'er')", 12, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$filter=endswith(tolower(title), 'er')", 12, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$filter=endswith(toupper(title), 'ER')", 12, null, 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$filter=floor(duration div 60.0) eq 2", 100, "tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=100", 0, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" })]
        [InlineData("tables/movies?$filter=month(releaseDate) eq 11", 14, null, 0, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner eq false)", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner eq true)", 100, "tables/movies?$filter=not(bestPictureWinner eq true)&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner ne false)", 100, "tables/movies?$filter=not(bestPictureWinner ne false)&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner ne true)", 38, null, 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$filter=rating eq 'R'", 94, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/movies?$filter=rating ne 'PG-13'", 100, "tables/movies?$filter=rating ne 'PG-13'&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=rating eq null", 74, null, 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 0, new[] { "id-000", "id-003" })]
        [InlineData("tables/movies?$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 100, "tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=round(duration div 60.0) eq 2", 100, "tables/movies?$filter=round(duration div 60.0) eq 2&$skip=100", 0, new[] { "id-000", "id-005", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=startsWith(rating, 'PG')", 64, null, 0, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" })]
        [InlineData("tables/movies?$filter=startsWith(tolower(title), 'the')", 63, null, 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$filter=startsWith(toupper(title), 'THE')", 63, null, 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$filter=year eq 1994", 5, null, 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$filter=year ge 2000 and year le 2009", 55, null, 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$filter=year ge 2000", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$filter=year gt 1999 and year lt 2010", 55, null, 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$filter=year gt 1999", 69, null, 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$filter=year le 2000", 100, "tables/movies?$filter=year le 2000&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=year lt 2001", 100, "tables/movies?$filter=year lt 2001&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$filter=year(releaseDate) eq 1994", 6, null, 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$orderBy=bestPictureWinner asc", 100, "tables/movies?$orderBy=bestPictureWinner asc&$skip=100", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$orderBy=bestPictureWinner desc", 100, "tables/movies?$orderBy=bestPictureWinner desc&$skip=100", 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$orderBy=duration asc", 100, "tables/movies?$orderBy=duration asc&$skip=100", 0, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" })]
        [InlineData("tables/movies?$orderBy=duration desc", 100, "tables/movies?$orderBy=duration desc&$skip=100", 0, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" })]
        [InlineData("tables/movies?$orderBy=rating asc", 100, "tables/movies?$orderBy=rating asc&$skip=100", 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$orderBy=rating desc", 100, "tables/movies?$orderBy=rating desc&$skip=100", 0, new[] { "id-197", "id-107", "id-115", "id-028", "id-039" })]
        [InlineData("tables/movies?$orderBy=releaseDate asc", 100, "tables/movies?$orderBy=releaseDate asc&$skip=100", 0, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" })]
        [InlineData("tables/movies?$orderBy=releaseDate desc", 100, "tables/movies?$orderBy=releaseDate desc&$skip=100", 0, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" })]
        [InlineData("tables/movies?$orderBy=title asc", 100, "tables/movies?$orderBy=title asc&$skip=100", 0, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" })]
        [InlineData("tables/movies?$orderBy=title desc", 100, "tables/movies?$orderBy=title desc&$skip=100", 0, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" })]
        [InlineData("tables/movies?$orderBy=year asc", 100, "tables/movies?$orderBy=year asc&$skip=100", 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$orderBy=year asc,title asc", 100, "tables/movies?$orderBy=year asc,title asc&$skip=100", 0, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" })]
        [InlineData("tables/movies?$orderBy=year asc,title desc", 100, "tables/movies?$orderBy=year asc,title desc&$skip=100", 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$orderBy=year desc", 100, "tables/movies?$orderBy=year desc&$skip=100", 0, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" })]
        [InlineData("tables/movies?$orderBy=year desc,title asc", 100, "tables/movies?$orderBy=year desc,title asc&$skip=100", 0, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" })]
        [InlineData("tables/movies?$orderBy=year desc,title desc", 100, "tables/movies?$orderBy=year desc,title desc&$skip=100", 0, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" })]
        [InlineData("tables/movies?$filter=((year div 1000.5) eq 2) and (rating eq 'R')&$skip=5", 0, null, 0, new string[] { })]
        [InlineData("tables/movies?$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)&$skip=5", 8, null, 0, new[] { "id-142", "id-143", "id-162", "id-166", "id-172" })]
        [InlineData("tables/movies?$filter=(year div 1000.5) eq 2&$skip=5", 1, null, 0, new[] { "id-216" })]
        [InlineData("tables/movies?$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)&$skip=5", 41, null, 0, new[] { "id-040", "id-041", "id-044", "id-046", "id-049" })]
        [InlineData("tables/movies?$filter=(year sub 1900) ge 80&$skip=5", 100, "tables/movies?$filter=(year sub 1900) ge 80&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq false&$skip=5", 100, "tables/movies?$filter=bestPictureWinner eq false&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2&$skip=5", 6, null, 0, new[] { "id-150", "id-155", "id-186", "id-189", "id-196" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2&$skip=5", 16, null, 0, new[] { "id-062", "id-083", "id-087", "id-092", "id-093" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2&$skip=5", 19, null, 0, new[] { "id-092", "id-093", "id-094", "id-096", "id-112" })]
        [InlineData("tables/movies?$filter=bestPictureWinner eq true&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" })]
        [InlineData("tables/movies?$filter=bestPictureWinner ne false&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" })]
        [InlineData("tables/movies?$filter=bestPictureWinner ne true&$skip=5", 100, "tables/movies?$filter=bestPictureWinner ne true&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=5", 100, "tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=105", 0, new[] { "id-027", "id-028", "id-030", "id-031", "id-032" })]
        [InlineData("tables/movies?$filter=day(releaseDate) eq 1&$skip=5", 2, null, 0, new[] { "id-197", "id-215" })]
        [InlineData("tables/movies?$filter=duration ge 60&$skip=5", 100, "tables/movies?$filter=duration ge 60&$skip=105", 0, new[] { "id-005", "id-006", "id-007", "id-008", "id-009" })]
        [InlineData("tables/movies?$filter=endswith(title, 'er')&$skip=5", 7, null, 0, new[] { "id-170", "id-193", "id-197", "id-205", "id-217" })]
        [InlineData("tables/movies?$filter=endswith(tolower(title), 'er')&$skip=5", 7, null, 0, new[] { "id-170", "id-193", "id-197", "id-205", "id-217" })]
        [InlineData("tables/movies?$filter=endswith(toupper(title), 'ER')&$skip=5", 7, null, 0, new[] { "id-170", "id-193", "id-197", "id-205", "id-217" })]
        [InlineData("tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=5", 100, "tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=105", 0, new[] { "id-009", "id-010", "id-011", "id-012", "id-013" })]
        [InlineData("tables/movies?$filter=month(releaseDate) eq 11&$skip=5", 9, null, 0, new[] { "id-115", "id-131", "id-136", "id-146", "id-167" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner eq false)&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner eq true)&$skip=5", 100, "tables/movies?$filter=not(bestPictureWinner eq true)&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner ne false)&$skip=5", 100, "tables/movies?$filter=not(bestPictureWinner ne false)&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$filter=not(bestPictureWinner ne true)&$skip=5", 33, null, 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" })]
        [InlineData("tables/movies?$filter=rating eq null&$skip=5", 69, null, 0, new[] { "id-040", "id-041", "id-044", "id-046", "id-049" })]
        [InlineData("tables/movies?$filter=rating eq 'R'&$skip=5", 89, null, 0, new[] { "id-009", "id-014", "id-017", "id-019", "id-022" })]
        [InlineData("tables/movies?$filter=rating ne 'PG-13'&$skip=5", 100, "tables/movies?$filter=rating ne 'PG-13'&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 0, null, 0, new string[] { })]
        [InlineData("tables/movies?$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
        [InlineData("tables/movies?$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
        [InlineData("tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 100, "tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5", 100, "tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=round(duration div 60.0) eq 2&$skip=5", 100, "tables/movies?$filter=round(duration div 60.0) eq 2&$skip=105", 0, new[] { "id-013", "id-014", "id-015", "id-016", "id-017" })]
        [InlineData("tables/movies?$filter=startsWith(rating, 'PG')&$skip=5", 59, null, 0, new[] { "id-015", "id-018", "id-020", "id-021", "id-024" })]
        [InlineData("tables/movies?$filter=startsWith(tolower(title), 'the')&$skip=5", 58, null, 0, new[] { "id-008", "id-012", "id-017", "id-020", "id-023" })]
        [InlineData("tables/movies?$filter=startsWith(toupper(title), 'THE')&$skip=5", 58, null, 0, new[] { "id-008", "id-012", "id-017", "id-020", "id-023" })]
        [InlineData("tables/movies?$filter=year eq 1994&$skip=5", 0, null, 0, new string[] { })]
        [InlineData("tables/movies?$filter=year ge 2000 and year le 2009&$skip=5", 50, null, 0, new[] { "id-032", "id-042", "id-050", "id-051", "id-058" })]
        [InlineData("tables/movies?$filter=year ge 2000&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
        [InlineData("tables/movies?$filter=year gt 1999 and year lt 2010&$skip=5", 50, null, 0, new[] { "id-032", "id-042", "id-050", "id-051", "id-058" })]
        [InlineData("tables/movies?$filter=year gt 1999&$skip=5", 64, null, 0, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
        [InlineData("tables/movies?$filter=year le 2000&$skip=5", 100, "tables/movies?$filter=year le 2000&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=year lt 2001&$skip=5", 100, "tables/movies?$filter=year lt 2001&$skip=105", 0, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$filter=year(releaseDate) eq 1994&$skip=5", 1, null, 0, new[] { "id-217" })]
        [InlineData("tables/movies?$orderBy=bestPictureWinner asc&$skip=5", 100, "tables/movies?$orderBy=bestPictureWinner asc&$skip=105", 0, new[] { "id-009", "id-010", "id-012", "id-013", "id-014" })]
        [InlineData("tables/movies?$orderBy=bestPictureWinner desc&$skip=5", 100, "tables/movies?$orderBy=bestPictureWinner desc&$skip=105", 0, new[] { "id-018", "id-023", "id-024", "id-048", "id-051" })]
        [InlineData("tables/movies?$orderBy=duration asc&$skip=5", 100, "tables/movies?$orderBy=duration asc&$skip=105", 0, new[] { "id-238", "id-201", "id-115", "id-229", "id-181" })]
        [InlineData("tables/movies?$orderBy=duration desc&$skip=5", 100, "tables/movies?$orderBy=duration desc&$skip=105", 0, new[] { "id-007", "id-183", "id-063", "id-202", "id-130" })]
        [InlineData("tables/movies?$orderBy=rating asc&$skip=5", 100, "tables/movies?$orderBy=rating asc&$skip=105", 0, new[] { "id-040", "id-041", "id-044", "id-046", "id-049" })]
        [InlineData("tables/movies?$orderBy=rating desc&$skip=5", 100, "tables/movies?$orderBy=rating desc&$skip=105", 0, new[] { "id-148", "id-000", "id-001", "id-002", "id-003" })]
        [InlineData("tables/movies?$orderBy=releaseDate asc&$skip=5", 100, "tables/movies?$orderBy=releaseDate asc&$skip=105", 0, new[] { "id-229", "id-224", "id-041", "id-049", "id-135" })]
        [InlineData("tables/movies?$orderBy=releaseDate desc&$skip=5", 100, "tables/movies?$orderBy=releaseDate desc&$skip=105", 0, new[] { "id-149", "id-213", "id-102", "id-155", "id-169" })]
        [InlineData("tables/movies?$orderBy=title asc&$skip=5", 100, "tables/movies?$orderBy=title asc&$skip=105", 0, new[] { "id-214", "id-102", "id-215", "id-039", "id-057" })]
        [InlineData("tables/movies?$orderBy=title desc&$skip=5", 100, "tables/movies?$orderBy=title desc&$skip=105", 0, new[] { "id-058", "id-046", "id-160", "id-092", "id-176" })]
        [InlineData("tables/movies?$orderBy=year asc&$skip=5", 100, "tables/movies?$orderBy=year asc&$skip=105", 0, new[] { "id-088", "id-224", "id-041", "id-049", "id-135" })]
        [InlineData("tables/movies?$orderBy=year asc,title asc&$skip=5", 100, "tables/movies?$orderBy=year asc,title asc&$skip=105", 0, new[] { "id-088", "id-224", "id-041", "id-049", "id-135" })]
        [InlineData("tables/movies?$orderBy=year asc,title desc&$skip=5", 100, "tables/movies?$orderBy=year asc,title desc&$skip=105", 0, new[] { "id-088", "id-224", "id-049", "id-041", "id-135" })]
        [InlineData("tables/movies?$orderBy=year desc&$skip=5", 100, "tables/movies?$orderBy=year desc&$skip=105", 0, new[] { "id-149", "id-186", "id-213", "id-013", "id-053" })]
        [InlineData("tables/movies?$orderBy=year desc,title asc&$skip=5", 100, "tables/movies?$orderBy=year desc,title asc&$skip=105", 0, new[] { "id-186", "id-064", "id-149", "id-169", "id-161" })]
        [InlineData("tables/movies?$orderBy=year desc,title desc&$skip=5", 100, "tables/movies?$orderBy=year desc,title desc&$skip=105", 0, new[] { "id-186", "id-213", "id-102", "id-053", "id-155" })]
        [InlineData("tables/movies?$skip=0", 100, "tables/movies?$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$skip=100", 100, "tables/movies?$skip=200", 0, new[] { "id-100", "id-101", "id-102", "id-103", "id-104" })]
        [InlineData("tables/movies?$skip=200", 48, null, 0, new[] { "id-200", "id-201", "id-202", "id-203", "id-204" })]
        [InlineData("tables/movies?$skip=300", 0, null, 0, new string[] { })]
        [InlineData("tables/movies?$top=5&$filter=((year div 1000.5) eq 2) and (rating eq 'R')", 2, null, 0, new[] { "id-061", "id-173" })]
        [InlineData("tables/movies?$top=5&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", 5, "tables/movies?$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)&$skip=5&$top=5", 0, new[] { "id-026", "id-047", "id-081", "id-103", "id-121" })]
        [InlineData("tables/movies?$top=5&$filter=(year div 1000.5) eq 2", 5, "tables/movies?$filter=(year div 1000.5) eq 2&$skip=5&$top=5", 0, new[] { "id-012", "id-042", "id-061", "id-173", "id-194" })]
        [InlineData("tables/movies?$top=5&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", 5, "tables/movies?$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)&$skip=5&$top=5", 0, new[] { "id-005", "id-016", "id-027", "id-028", "id-031" })]
        [InlineData("tables/movies?$top=5&$filter=(year sub 1900) ge 80", 5, "tables/movies?$filter=(year sub 1900) ge 80&$skip=5&$top=5", 0, new[] { "id-000", "id-003", "id-006", "id-007", "id-008" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner eq false", 5, "tables/movies?$filter=bestPictureWinner eq false&$skip=5&$top=5", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", 5, "tables/movies?$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2&$skip=5&$top=5", 0, new[] { "id-023", "id-024", "id-112", "id-135", "id-142" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", 5, "tables/movies?$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2&$skip=5&$top=5", 0, new[] { "id-001", "id-011", "id-018", "id-048", "id-051" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", 5, "tables/movies?$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2&$skip=5&$top=5", 0, new[] { "id-011", "id-018", "id-023", "id-024", "id-048" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner eq true", 5, "tables/movies?$filter=bestPictureWinner eq true&$skip=5&$top=5", 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner ne false", 5, "tables/movies?$filter=bestPictureWinner ne false&$skip=5&$top=5", 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$top=5&$filter=bestPictureWinner ne true", 5, "tables/movies?$filter=bestPictureWinner ne true&$skip=5&$top=5", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=ceiling(duration div 60.0) eq 2", 5, "tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=5&$top=5", 0, new[] { "id-005", "id-023", "id-024", "id-025", "id-026" })]
        [InlineData("tables/movies?$top=5&$filter=day(releaseDate) eq 1", 5, "tables/movies?$filter=day(releaseDate) eq 1&$skip=5&$top=5", 0, new[] { "id-019", "id-048", "id-129", "id-131", "id-132" })]
        [InlineData("tables/movies?$top=5&$filter=duration ge 60", 5, "tables/movies?$filter=duration ge 60&$skip=5&$top=5", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=endswith(title, 'er')", 5, "tables/movies?$filter=endswith(title, 'er')&$skip=5&$top=5", 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$top=5&$filter=endswith(tolower(title), 'er')", 5, "tables/movies?$filter=endswith(tolower(title), 'er')&$skip=5&$top=5", 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$top=5&$filter=endswith(toupper(title), 'ER')", 5, "tables/movies?$filter=endswith(toupper(title), 'ER')&$skip=5&$top=5", 0, new[] { "id-001", "id-052", "id-121", "id-130", "id-164" })]
        [InlineData("tables/movies?$top=5&$filter=floor(duration div 60.0) eq 2", 5, "tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=5&$top=5", 0, new[] { "id-000", "id-001", "id-003", "id-004", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=month(releaseDate) eq 11", 5, "tables/movies?$filter=month(releaseDate) eq 11&$skip=5&$top=5", 0, new[] { "id-011", "id-016", "id-030", "id-064", "id-085" })]
        [InlineData("tables/movies?$top=5&$filter=not(bestPictureWinner eq false)", 5, "tables/movies?$filter=not(bestPictureWinner eq false)&$skip=5&$top=5", 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$top=5&$filter=not(bestPictureWinner eq true)", 5, "tables/movies?$filter=not(bestPictureWinner eq true)&$skip=5&$top=5", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=not(bestPictureWinner ne false)", 5, "tables/movies?$filter=not(bestPictureWinner ne false)&$skip=5&$top=5", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=not(bestPictureWinner ne true)", 5, "tables/movies?$filter=not(bestPictureWinner ne true)&$skip=5&$top=5", 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$top=5&$filter=rating eq 'R'", 5, "tables/movies?$filter=rating eq 'R'&$skip=5&$top=5", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/movies?$top=5&$filter=rating ne 'PG-13'", 5, "tables/movies?$filter=rating ne 'PG-13'&$skip=5&$top=5", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=rating eq null", 5, "tables/movies?$filter=rating eq null&$skip=5&$top=5", 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$top=5&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", 2, null, 0, new[] { "id-000", "id-003" })]
        [InlineData("tables/movies?$top=5&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 5, "tables/movies?$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5&$top=5", 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$top=5&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)", 5, "tables/movies?$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5&$top=5", 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$top=5&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 5, "tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5&$top=5", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)", 5, "tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5&$top=5", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=round(duration div 60.0) eq 2", 5, "tables/movies?$filter=round(duration div 60.0) eq 2&$skip=5&$top=5", 0, new[] { "id-000", "id-005", "id-009", "id-010", "id-011" })]
        [InlineData("tables/movies?$top=5&$filter=startsWith(rating, 'PG')", 5, "tables/movies?$filter=startsWith(rating, 'PG')&$skip=5&$top=5", 0, new[] { "id-006", "id-008", "id-010", "id-012", "id-013" })]
        [InlineData("tables/movies?$top=5&$filter=startsWith(tolower(title), 'the')", 5, "tables/movies?$filter=startsWith(tolower(title), 'the')&$skip=5&$top=5", 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=startsWith(toupper(title), 'THE')", 5, "tables/movies?$filter=startsWith(toupper(title), 'THE')&$skip=5&$top=5", 0, new[] { "id-000", "id-001", "id-002", "id-004", "id-006" })]
        [InlineData("tables/movies?$top=5&$filter=year eq 1994", 5, null, 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$top=5&$filter=year ge 2000 and year le 2009", 5, "tables/movies?$filter=year ge 2000 and year le 2009&$skip=5&$top=5", 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$top=5&$filter=year ge 2000", 5, "tables/movies?$filter=year ge 2000&$skip=5&$top=5", 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$top=5&$filter=year gt 1999 and year lt 2010", 5, "tables/movies?$filter=year gt 1999 and year lt 2010&$skip=5&$top=5", 0, new[] { "id-006", "id-008", "id-012", "id-019", "id-020" })]
        [InlineData("tables/movies?$top=5&$filter=year gt 1999", 5, "tables/movies?$filter=year gt 1999&$skip=5&$top=5", 0, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
        [InlineData("tables/movies?$top=5&$filter=year le 2000", 5, "tables/movies?$filter=year le 2000&$skip=5&$top=5", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=year lt 2001", 5, "tables/movies?$filter=year lt 2001&$skip=5&$top=5", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies?$top=5&$filter=year(releaseDate) eq 1994", 5, "tables/movies?$filter=year(releaseDate) eq 1994&$skip=5&$top=5", 0, new[] { "id-000", "id-003", "id-018", "id-030", "id-079" })]
        [InlineData("tables/movies?$top=5&$orderBy=bestPictureWinner asc", 5, "tables/movies?$orderBy=bestPictureWinner asc&$skip=5&$top=5", 0, new[] { "id-000", "id-003", "id-004", "id-005", "id-006" })]
        [InlineData("tables/movies?$top=5&$orderBy=bestPictureWinner desc", 5, "tables/movies?$orderBy=bestPictureWinner desc&$skip=5&$top=5", 0, new[] { "id-001", "id-002", "id-007", "id-008", "id-011" })]
        [InlineData("tables/movies?$top=5&$orderBy=duration asc", 5, "tables/movies?$orderBy=duration asc&$skip=5&$top=5", 0, new[] { "id-227", "id-125", "id-133", "id-107", "id-118" })]
        [InlineData("tables/movies?$top=5&$orderBy=duration desc", 5, "tables/movies?$orderBy=duration desc&$skip=5&$top=5", 0, new[] { "id-153", "id-065", "id-165", "id-008", "id-002" })]
        [InlineData("tables/movies?$top=5&$orderBy=rating asc", 5, "tables/movies?$orderBy=rating asc&$skip=5&$top=5", 0, new[] { "id-004", "id-005", "id-011", "id-016", "id-031" })]
        [InlineData("tables/movies?$top=5&$orderBy=rating desc", 5, "tables/movies?$orderBy=rating desc&$skip=5&$top=5", 0, new[] { "id-197", "id-107", "id-115", "id-028", "id-039" })]
        [InlineData("tables/movies?$top=5&$orderBy=releaseDate asc", 5, "tables/movies?$orderBy=releaseDate asc&$skip=5&$top=5", 0, new[] { "id-125", "id-133", "id-227", "id-118", "id-088" })]
        [InlineData("tables/movies?$top=5&$orderBy=releaseDate desc", 5, "tables/movies?$orderBy=releaseDate desc&$skip=5&$top=5", 0, new[] { "id-188", "id-033", "id-122", "id-186", "id-064" })]
        [InlineData("tables/movies?$top=5&$orderBy=title asc", 5, "tables/movies?$orderBy=title asc&$skip=5&$top=5", 0, new[] { "id-005", "id-091", "id-243", "id-194", "id-060" })]
        [InlineData("tables/movies?$top=5&$orderBy=title desc", 5, "tables/movies?$orderBy=title desc&$skip=5&$top=5", 0, new[] { "id-107", "id-100", "id-123", "id-190", "id-149" })]
        [InlineData("tables/movies?$top=5&$orderBy=year asc", 5, "tables/movies?$orderBy=year asc&$skip=5&$top=5", 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$top=5&$orderBy=year asc,title asc", 5, "tables/movies?$orderBy=year asc,title asc&$skip=5&$top=5", 0, new[] { "id-125", "id-229", "id-227", "id-133", "id-118" })]
        [InlineData("tables/movies?$top=5&$orderBy=year asc,title desc", 5, "tables/movies?$orderBy=year asc,title desc&$skip=5&$top=5", 0, new[] { "id-125", "id-229", "id-133", "id-227", "id-118" })]
        [InlineData("tables/movies?$top=5&$orderBy=year desc", 5, "tables/movies?$orderBy=year desc&$skip=5&$top=5", 0, new[] { "id-033", "id-122", "id-188", "id-064", "id-102" })]
        [InlineData("tables/movies?$top=5&$orderBy=year desc,title asc", 5, "tables/movies?$orderBy=year desc,title asc&$skip=5&$top=5", 0, new[] { "id-188", "id-122", "id-033", "id-102", "id-213" })]
        [InlineData("tables/movies?$top=5&$orderBy=year desc,title desc", 5, "tables/movies?$orderBy=year desc,title desc&$skip=5&$top=5", 0, new[] { "id-033", "id-122", "id-188", "id-149", "id-064" })]
        [InlineData("tables/movies_pagesize", 25, "tables/movies_pagesize?$skip=25", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
        [InlineData("tables/movies_pagesize?$skip=25", 25, "tables/movies_pagesize?$skip=50", 0, new[] { "id-025", "id-026", "id-027", "id-028", "id-029" })]
        [InlineData("tables/movies_rated", 94, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }, "X-Auth", "success")]
        public async Task BasicQueryTest(string query, int expectedItemCount, string expectedNextLinkQuery, long expectedTotalCount, string[] firstExpectedItems, string headerName = null, string headerValue = null)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            Dictionary<string, string> headers = new();
            if (headerName != null && headerValue != null) headers.Add(headerName, headerValue);

            // Act
            var response = await server.SendRequest(HttpMethod.Get, query, headers).ConfigureAwait(false);

            // Assert

            // Response has the right Status Code
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Response payload can be decoded
            var result = response.DeserializeContent<PageOfItems<ClientMovie>>();
            Assert.NotNull(result);

            // Payload has the right content
            Assert.Equal(expectedItemCount, result.Items.Length);
            Assert.Equal(expectedNextLinkQuery, result.NextLink == null ? null : Uri.UnescapeDataString(result.NextLink.PathAndQuery).TrimStart('/'));
            Assert.Equal(expectedTotalCount, result.Count);

            // The first n items must match what is expected
            Assert.True(result.Items.Length >= firstExpectedItems.Length);
            Assert.Equal(firstExpectedItems, result.Items.Take(firstExpectedItems.Length).Select(m => m.Id).ToArray());
            for (int idx = 0; idx < firstExpectedItems.Length; idx++)
            {
                var expected = repository.GetEntity(firstExpectedItems[idx]);
                var actual = result.Items[idx];

                Assert.Equal<IMovie>(expected, actual);
                AssertEx.SystemPropertiesMatch(expected, actual);
            }
        }

        /// <summary>
        /// We do a bunch of tests for select by dealing with the overflow properties capabilities within
        /// System.Text.Json - we are not interested in the search (we're doing the same thing over and
        /// over).  Instead, we are ensuring the right selections are made.
        /// </summary>
        /// <param name="selection">The selection to use</param>
        /// <returns></returns>
        [Theory, CombinatorialData]
        public async Task SelectQueryTest(bool sId, bool sUpdatedAt, bool sVersion, bool sDeleted, bool sBPW, bool sduration, bool srating, bool sreleaseDate, bool stitle, bool syear)
        {
            // Arrange
            var server = Program.CreateTestServer();
            List<string> selection = new();
            if (sId) selection.Add("id");
            if (sUpdatedAt) selection.Add("updatedAt");
            if (sVersion) selection.Add("version");
            if (sDeleted) selection.Add("deleted");
            if (sBPW) selection.Add("bestPictureWinner");
            if (sduration) selection.Add("duration");
            if (srating) selection.Add("rating");
            if (sreleaseDate) selection.Add("releaseDate");
            if (stitle) selection.Add("title");
            if (syear) selection.Add("year");
            if (selection.Count == 0) return;
            var query = $"tables/movies?$top=5&$skip=5&$select={string.Join(',', selection)}";

            // Act
            var response = await server.SendRequest(HttpMethod.Get, query).ConfigureAwait(false);

            // Assert

            // Response has the right Status Code
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Response payload can be decoded
            var result = response.DeserializeContent<PageOfItems<ClientObject>>();
            Assert.NotNull(result);

            // There are items in the response payload
            Assert.NotNull(result.Items);
            foreach (var item in result.Items)
            {
                // Each item in the payload has the requested properties.
                foreach (var property in selection)
                {
                    Assert.True(item.Data.ContainsKey(property));
                }
            }
        }

        [Theory]
        [InlineData("tables/movies", 248, 3)]
        [InlineData("tables/movies?$top=50", 248, 5)]
        [InlineData("tables/movies?$count=true", 248, 3)]
        [InlineData("tables/movies?$top=50&$count=true", 248, 5)]
        [InlineData("tables/movies?$filter=releaseDate ge cast(1960-01-01T05:30:00Z,Edm.DateTimeOffset)&orderby=releaseDate asc", 186, 2)]
        [InlineData("tables/movies?$filter=releaseDate ge cast(1960-01-01T05:30:00Z,Edm.DateTimeOffset)&orderby=releaseDate asc&$top=20", 186, 10)]
        public async Task PagingTest(string startQuery, int expectedCount, int expectedLoops)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var query = startQuery;
            int loops = 0;
            var items = new Dictionary<string, ClientMovie>();

            //Act
            do
            {
                loops++;

                var response = await server.SendRequest(HttpMethod.Get, query).ConfigureAwait(false);
                Assert.True(response.IsSuccessStatusCode);
                var result = response.DeserializeContent<PageOfItems<ClientMovie>>();
                Assert.NotNull(result?.Items);
                foreach (var item in result.Items)
                {
                    items.Add(item.Id!, item);
                }

                if (result.NextLink != null)
                {
                    query = result.NextLink.PathAndQuery;
                }
                else
                {
                    break;
                }
            } while (loops < expectedLoops + 2);

            // Assert
            Assert.Equal(expectedCount, items.Count);
            Assert.Equal(expectedLoops, loops);
        }

        [Theory]
        [InlineData("tables/notfound", HttpStatusCode.NotFound)]
        [InlineData("tables/movies?$filter=(year - 1900) ge 100", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$filter=missing eq 20", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$orderby=duration fizz", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$orderby=missing asc", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$select=foo", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$select=year rating", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$skip=-1", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$skip=NaN", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$top=-1", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$top=1000000", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies?$top=NaN", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_pagesize?$top=26", HttpStatusCode.BadRequest)]
        [InlineData("tables/movies_rated", HttpStatusCode.Unauthorized)]
        [InlineData("tables/movies_rated", HttpStatusCode.Unauthorized, "X-Auth", "failed")]
        public async Task FailedQueryTest(string relativeUri, HttpStatusCode expectedStatusCode, string headerName = null, string headerValue = null)
        {
            // Arrange
            var server = Program.CreateTestServer();
            Dictionary<string, string> headers = new();
            if (headerName != null && headerValue != null) headers.Add(headerName, headerValue);

            // Act
            var response = await server.SendRequest(HttpMethod.Get, relativeUri, headers).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        /// <summary>
        /// There are 248 movies, 154 of which are not R-rated (and hence not deleted)
        /// </summary>
        /// <param name="query"></param>
        /// <param name="expectedItemCount"></param>
        /// <param name="expectedNextLinkQuery"></param>
        /// <param name="expectedTotalCount"></param>
        /// <param name="firstExpectedItems"></param>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        [Theory]
        [InlineData("tables/soft", 100, "tables/soft?$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }, "X-ZUMO-Options", "include:deleted")]
        [InlineData("tables/soft?$count=true", 100, "tables/soft?$count=true&$skip=100", 154, new[] { "id-004", "id-005", "id-006", "id-008", "id-010" })]
        [InlineData("tables/soft?$filter=deleted eq false&__includedeleted=true", 100, "tables/soft?$filter=deleted eq false&__includedeleted=true&$skip=100", 0, new[] { "id-004", "id-005", "id-006", "id-008", "id-010" })]
        [InlineData("tables/soft?$filter=deleted eq true&__includedeleted=true", 94, null, 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-007" })]
        [InlineData("tables/soft?__includedeleted=true", 100, "tables/soft?__includedeleted=true&$skip=100", 0, new[] { "id-000", "id-001", "id-002", "id-003", "id-004", "id-005" })]
        public async Task SoftDeleteQueryTest(string query, int expectedItemCount, string expectedNextLinkQuery, long expectedTotalCount, string[] firstExpectedItems, string headerName = null, string headerValue = null)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<SoftMovie>();
            Dictionary<string, string> headers = new();
            if (headerName != null && headerValue != null) headers.Add(headerName, headerValue);

            // Act
            var response = await server.SendRequest(HttpMethod.Get, query, headers).ConfigureAwait(false);

            // Assert

            // Response has the right Status Code
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Response payload can be decoded
            var result = response.DeserializeContent<PageOfItems<ClientMovie>>();
            Assert.NotNull(result);

            // Payload has the right content
            Assert.Equal(expectedItemCount, result.Items.Length);
            Assert.Equal(expectedNextLinkQuery, result.NextLink == null ? null : Uri.UnescapeDataString(result.NextLink.PathAndQuery).TrimStart('/'));
            Assert.Equal(expectedTotalCount, result.Count);

            // The first n items must match what is expected
            Assert.True(result.Items.Length >= firstExpectedItems.Length);
            Assert.Equal(firstExpectedItems, result.Items.Take(firstExpectedItems.Length).Select(m => m.Id).ToArray());
            for (int idx = 0; idx < firstExpectedItems.Length; idx++)
            {
                var expected = repository.GetEntity(firstExpectedItems[idx]);
                var actual = result.Items[idx];

                Assert.Equal<IMovie>(expected, actual);
                AssertEx.SystemPropertiesMatch(expected, actual);
            }
        }

    }
}

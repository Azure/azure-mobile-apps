// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using DeviceTests.Shared.Helpers;
using DeviceTests.Shared.Helpers.Data;
using DeviceTests.Shared.Helpers.Models;
using DeviceTests.Shared.TestPlatform;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

// The LINQ to OData Driver needs to be updated to support StringComparison.  It only supports ToLower()/ToUpper() right now
#pragma warning disable RCS1155 // Use StringComparison when comparing strings.

namespace DeviceTests.Shared.Tests
{
    /// <summary>
    /// This test is allowed to run in parallel on the devices because it is query only.
    /// </summary>
    public class Query : E2ETestBase
    {
        private const int VeryLargeTopValue = 1001;

        [Fact]
        public async Task SimpleDataSource()
        {
            // This test is known to fail in Xamarin -- see https://bugzilla.xamarin.com/show_bug.cgi?id=22955
            // Get the Movie table
            IMobileServiceTable<Movie> table = GetClient().GetTable<Movie>();

            // Create a new CollectionView
            MobileServiceCollection<Movie, Movie> dataSource = await table.Take(1000).OrderByDescending(m => m.Title).ToCollectionAsync();

            Assert.Equal(248, dataSource.Count);
            Assert.Equal((long)-1, dataSource.TotalCount);
            Assert.Equal("Yojimbo", dataSource[0].Title);
        }

        [Fact]
        public async Task SimpleDataSourceWithTotalCount()
        {
            // Get the Movie table
            IMobileServiceTable<Movie> table = GetClient().GetTable<Movie>();

            // Create a new CollectionView
            MobileServiceCollection<Movie, Movie> dataSource = await table.Take(5).IncludeTotalCount().ToCollectionAsync();

            Assert.Equal(5, dataSource.Count);
            Assert.Equal((long)248, dataSource.TotalCount);
        }

        [Fact]
        public Task Query_Int_GTLT_Movies90()
            => CreateQueryTestIntId(
                "GreaterThan and LessThan - Movies from the 90s", 
                m => m.Year > 1989 && m.Year < 2000);

        [Fact]
        public Task Query_Int_GELE_Movies90()
            => CreateQueryTestIntId(
                "GreaterEqual and LessEqual - Movies from the 90s", 
                m => m.Year >= 1990 && m.Year <= 1999);

        [Fact]
        public Task Query_Int_Compoint_ORofANDs_Movies30s50s()
            => CreateQueryTestIntId(
                "Compound statement - OR of ANDs - Movies from the 30s and 50s",
                m => ((m.Year >= 1930) && (m.Year < 1940)) || ((m.Year >= 1950) && (m.Year < 1960)));

        [Fact]
        public Task Query_Int_Division_Movies2001RatingNE_R()
            => CreateQueryTestIntId(
                "Division, equal and different - Movies from the year 2001 with rating other than R",
                m => ((m.Year / 1000.5) == 2) && (m.MpaaRating != "R"));

        [Fact]
        public Task Query_Int_AND_Movies90sLT2hours()
            => CreateQueryTestIntId(
                "Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours",
                m => ((m.Year - 1900) >= 80) && (m.Year + 10 < 2000) && (m.Duration < 120));

        [Fact]
        public Task Query_String_GTLT_Movies90()
             => CreateQueryTestStringId(
                "GreaterThan and LessThan - Movies from the 90s",
                m => m.Year > 1989 && m.Year < 2000);

        [Fact]
        public Task Query_String_GELE_Movies90()
            => CreateQueryTestStringId(
                "GreaterEqual and LessEqual - Movies from the 90s",
                m => m.Year >= 1990 && m.Year <= 1999);

        [Fact]
        public Task Query_String_Compoint_ORofANDs_Movies30s50s()
            => CreateQueryTestStringId(
                "Compound statement - OR of ANDs - Movies from the 30s and 50s",
                m => ((m.Year >= 1930) && (m.Year < 1940)) || ((m.Year >= 1950) && (m.Year < 1960)));

        [Fact]
        public Task Query_String_Division_Movies2001RatingNE_R()
            => CreateQueryTestStringId(
                "Division, equal and different - Movies from the year 2001 with rating other than R",
                m => ((m.Year / 1000.5) == 2) && (m.MpaaRating != "R"));

        [Fact]
        public Task Query_String_AND_Movies90sLT2hours()
            => CreateQueryTestStringId(
                "Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours",
                m => ((m.Year - 1900) >= 80) && (m.Year + 10 < 2000) && (m.Duration < 120));

        [Fact]
        public Task Query_Int_StartsWith()
            => CreateQueryTestIntId(
                "String: StartsWith - Movies which starts with 'The'",
                m => m.Title.StartsWith("The"), 100);

        [Fact]
        public Task Query_Int_StartsWithInsensitive()
            => CreateQueryTestIntId(
                "String: StartsWith, case insensitive - Movies which start with 'the'",
                m => m.Title.ToLower().StartsWith("the"), 100);

        [Fact]
        public Task Query_Int_EndsWith()
            => CreateQueryTestIntId(
                "String: EndsWith, case insensitive - Movies which end with 'r'",
                m => m.Title.ToLower().EndsWith("r"));

        [Fact]
        public Task Query_Int_EndsWithInsensitive()
            => CreateQueryTestIntId(
                "String: Contains - Movies which contain the word 'one', case insensitive",
                m => m.Title.ToUpper().Contains("ONE"));

        [Fact]
        public Task Query_Int_Length()
            => CreateQueryTestIntId(
                "String: Length - Movies with small names",
                m => m.Title.Length < 10, 200);

        [Fact]
        public Task Query_Int_Substring_1param()
            => CreateQueryTestIntId(
                "String: Substring (1 parameter), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1) == "r");

        [Fact]
        public Task Query_Int_Substring_2param()
            => CreateQueryTestIntId(
                "String: Substring (2 parameters), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1, 1) == "r");

        [Fact]
        public Task Query_Int_Concat()
            => CreateQueryTestIntId(
                "String: Concat - Movies rated 'PG' or 'PG-13' from the 2000s",
                m => m.Year >= 2000 && string.Concat(m.MpaaRating, "-13").StartsWith("PG-13"));

        [Fact]
        public Task Query_String_StartsWith()
            => CreateQueryTestStringId(
                "String: StartsWith - Movies which starts with 'The'",
                m => m.Title.StartsWith("The"), 100);

        [Fact]
        public Task Query_String_StartsWithInsensitive()
            => CreateQueryTestStringId(
                "String: StartsWith, case insensitive - Movies which start with 'the'",
                m => m.Title.ToLower().StartsWith("the"), 100);

        [Fact]
        public Task Query_String_EndsWith()
            => CreateQueryTestStringId(
                "String: EndsWith, case insensitive - Movies which end with 'r'",
                m => m.Title.ToLower().EndsWith("r"));

        [Fact]
        public Task Query_String_EndsWithInsensitive()
            => CreateQueryTestStringId(
                "String: Contains - Movies which contain the word 'one', case insensitive",
                m => m.Title.ToUpper().Contains("ONE"));

        [Fact]
        public Task Query_String_Length()
            => CreateQueryTestStringId(
                "String: Length - Movies with small names",
                m => m.Title.Length < 10, 200);

        [Fact]
        public Task Query_String_Substring_1param()
            => CreateQueryTestStringId(
                "String: Substring (1 parameter), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1) == "r");

        [Fact]
        public Task Query_String_Substring_2param()
            => CreateQueryTestStringId(
                "String: Substring (2 parameters), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1, 1) == "r");

        [Fact]
        public Task Query_String_Concat()
            => CreateQueryTestStringId(
                "String: Concat - Movies rated 'PG' or 'PG-13' from the 2000s",
                m => m.Year >= 2000 && string.Concat(m.MpaaRating, "-13").StartsWith("PG-13"));

        [Fact]
        public Task Query_Int_StringEquals()
            => CreateQueryTestIntId(
                "String equals - Movies since 1980 with rating PG-13",
                m => m.Year >= 1980 && m.MpaaRating == "PG-13", 100);

        [Fact]
        public Task Query_Int_StringEqualsNull()
            => CreateQueryTestIntId(
                "String field, comparison to null - Movies since 1980 without a MPAA rating",
                m => m.Year >= 1980 && m.MpaaRating == null,
                whereLambda: m => m.Year >= 1980 && m.MpaaRating == null);

        [Fact]
        public Task Query_Int_StringNotEqualsNull()
            => CreateQueryTestIntId(
                "String field, comparison (not equal) to null - Movies before 1970 with a MPAA rating",
                m => m.Year < 1970 && m.MpaaRating != null,
                whereLambda: m => m.Year < 1970 && m.MpaaRating != null);

        [Fact]
        public Task Query_String_StringEquals()
            => CreateQueryTestStringId(
                "String equals - Movies since 1980 with rating PG-13",
                m => m.Year >= 1980 && m.MpaaRating == "PG-13", 100);

        [Fact]
        public Task Query_String_StringEqualsNull()
            => CreateQueryTestStringId(
                "String field, comparison to null - Movies since 1980 without a MPAA rating",
                m => m.Year >= 1980 && m.MpaaRating == null,
                whereLambda: m => m.Year >= 1980 && m.MpaaRating == null);

        [Fact]
        public Task Query_String_StringNotEqualsNull()
            => CreateQueryTestStringId(
                "String field, comparison (not equal) to null - Movies before 1970 with a MPAA rating",
                m => m.Year < 1970 && m.MpaaRating != null,
                whereLambda: m => m.Year < 1970 && m.MpaaRating != null);

        [Fact]
        public Task Query_Int_Floor()
            => CreateQueryTestIntId(
                "Floor - Movies which last more than 3 hours",
                m => Math.Floor(m.Duration / 60.0) >= 3);

        [Fact]
        public Task Query_Int_Ceiling()
            => CreateQueryTestIntId(
                "Ceiling - Best picture winners which last at most 2 hours",
                m => m.BestPictureWinner == true && Math.Ceiling(m.Duration / 60.0) == 2);

        [Fact]
        public Task Query_Int_Round()
            => CreateQueryTestIntId(
                "Round - Best picture winners which last more than 2.5 hours",
                m => m.BestPictureWinner == true && Math.Round(m.Duration / 60.0) > 2);

        [Fact]
        public Task Query_String_Floor()
            => CreateQueryTestStringId(
                "Floor - Movies which last more than 3 hours",
                m => Math.Floor(m.Duration / 60.0) >= 3);

        [Fact]
        public Task Query_String_Ceiling()
            => CreateQueryTestStringId(
                "Ceiling - Best picture winners which last at most 2 hours",
                m => m.BestPictureWinner == true && Math.Ceiling(m.Duration / 60.0) == 2);

        [Fact]
        public Task Query_String_Round()
            => CreateQueryTestStringId(
                "Round - Best picture winners which last more than 2.5 hours",
                m => m.BestPictureWinner == true && Math.Round(m.Duration / 60.0) > 2);

        [Fact]
        public Task Query_Int_DateGTLT()
            => CreateQueryTestIntId(
                "Date: Greater than, less than - Movies with release date in the 70s",
                m => m.ReleaseDate > new DateTime(1969, 12, 31, 0, 0, 0, DateTimeKind.Utc) && m.ReleaseDate < new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        [Fact]
        public Task Query_Int_DateGELT()
            => CreateQueryTestIntId("Date: Greater than, less than - Movies with release date in the 80s",
                m => m.ReleaseDate >= new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc) && m.ReleaseDate < new DateTime(1989, 12, 31, 23, 59, 59, DateTimeKind.Utc));

        [Fact]
        public Task Query_Int_DateEQ()
            => CreateQueryTestIntId(
                "Date: Equal - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)",
                m => m.ReleaseDate == new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc));

        [Fact]
        public Task Query_String_DateGTLT()
            => CreateQueryTestStringId(
                "Date: Greater than, less than - Movies with release date in the 70s",
                m => m.ReleaseDate > new DateTime(1969, 12, 31, 0, 0, 0, DateTimeKind.Utc) && m.ReleaseDate < new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        [Fact]
        public Task Query_String_DateGELT()
            => CreateQueryTestStringId(
                "Date: Greater than, less than - Movies with release date in the 80s",
                m => m.ReleaseDate >= new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc) && m.ReleaseDate < new DateTime(1989, 12, 31, 23, 59, 59, DateTimeKind.Utc));

        [Fact]
        public Task Query_String_DateEQ()
            => CreateQueryTestStringId(
                "Date: Equal - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)",
                m => m.ReleaseDate == new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc));

        [Fact]
        public Task Query_Int_DateMonthEQ()
            => CreateQueryTestIntId(
                "Date (month): Movies released in November",
                m => m.ReleaseDate.Month == 11);

        [Fact]
        public Task Query_Int_DateDayEQ()
            => CreateQueryTestIntId(
                "Date (day): Movies released in the first day of the month",
                m => m.ReleaseDate.Day == 1);

        [Fact]
        public Task Query_Int_DateYearNE()
            => CreateQueryTestIntId(
                "Date (year): Movies whose year is different than its release year",
                m => m.ReleaseDate.Year != m.Year, 100);

        [Fact]
        public Task Query_String_DateMOnthEQ()
            => CreateQueryTestStringId(
                "Date (month): Movies released in November",
                m => m.ReleaseDate.Month == 11);

        [Fact]
        public Task Query_String_DateDayEQ()
            => CreateQueryTestStringId(
                "Date (day): Movies released in the first day of the month",
                m => m.ReleaseDate.Day == 1);

        [Fact]
        public Task Query_String_DateYearNE()
            => CreateQueryTestStringId(
                "Date (year): Movies whose year is different than its release year",
                m => m.ReleaseDate.Year != m.Year, 100);

        [Fact]
        public Task Query_Int_BoolTrue()
            => CreateQueryTestIntId(
                "Bool: equal to true - Best picture winners before 1950",
                m => m.Year < 1950 && m.BestPictureWinner == true);

        [Fact]
        public Task Query_Int_BoolFalse()
            => CreateQueryTestIntId(
                "Bool: equal to false - Best picture winners after 2000",
                m => m.Year >= 2000 && !(m.BestPictureWinner == false));

        [Fact]
        public Task Query_Int_BoolNotFalse()
            => CreateQueryTestIntId(
                "Bool: not equal to false - Best picture winners after 2000",
                m => m.BestPictureWinner != false && m.Year >= 2000);

        [Fact]
        public Task Query_String_BoolTrue()
            => CreateQueryTestStringId(
                "Bool: equal to true - Best picture winners before 1950",
                m => m.Year < 1950 && m.BestPictureWinner == true);

        [Fact]
        public Task Query_String_BoolFalse()
            => CreateQueryTestStringId(
                "Bool: equal to false - Best picture winners after 2000",
                m => m.Year >= 2000 && !(m.BestPictureWinner == false));

        [Fact]
        public Task Query_String_BoolNotFalse()
            => CreateQueryTestStringId(
                "Bool: not equal to false - Best picture winners after 2000",
                m => m.BestPictureWinner != false && m.Year >= 2000);

        [Fact]
        public Task Query_Int_Top500()
            => CreateQueryTestIntId(
                "Get all using large $top - 500", 
                null, 500);

        [Fact]
        public Task Query_Int_Skip500()
            => CreateQueryTestIntId(
                "Skip all using large skip - 500", 
                null, null, 500, new[] { new OrderByClause("Title", true) });

        [Fact]
        public Task Query_Int_Top10()
            => CreateQueryTestIntId(
                "Get first ($top) - 10", 
                null, 10);

        [Fact]
        public Task Query_Int_Last10()
            => CreateQueryTestIntId(
                "Get last ($skip) - 10", 
                null, null, QueryTestData.TestIntIdMovies.Length - 10, new[] { new OrderByClause("Title", true) });

        [Fact]
        public Task Query_Int_SkipTakeTotals()
            => CreateQueryTestIntId(
                "Skip, take, includeTotalCount - movies 11-20, ordered by title",
                null, 10, 10, new[] { new OrderByClause("Title", true) }, null, true);

        [Fact]
        public Task Query_Int_SkipTakeTotalsFilter()
            => CreateQueryTestIntId(
                "Skip, take, filter includeTotalCount - movies 11-20 which won a best picture award, ordered by year",
                m => m.BestPictureWinner == true, 10, 10, new[] { new OrderByClause("Year", false) }, null, true);

        [Fact]
        public Task Query_String_Top500()
            => CreateQueryTestStringId(
                "Get all using large $top - 500", 
                null, 500);

        [Fact]
        public Task Query_String_Skip500()
            => CreateQueryTestStringId(
                "Skip all using large skip - 500", 
                null, null, 500);

        [Fact]
        public Task Query_String_Top10()
            => CreateQueryTestStringId(
                "Get first ($top) - 10", 
                null, 10);

        [Fact]
        public Task Query_String_Last10()
            => CreateQueryTestStringId(
                "Get last ($skip) - 10", 
                null, null, QueryTestData.TestMovies().Length - 10);

        [Fact]
        public Task Query_String_SkipTakeTotals()
            => CreateQueryTestStringId(
                "Skip, take, includeTotalCount - movies 11-20, ordered by title",
                null, 10, 10, new[] { new OrderByClause("Title", true) }, null, true);

        [Fact]
        public Task Query_String_SkipTakeTotalFilter()
            => CreateQueryTestStringId(
                "Skip, take, filter includeTotalCount - movies 11-20 which won a best picture award, ordered by year",
                m => m.BestPictureWinner == true, 10, 10, new[] { new OrderByClause("Year", false) }, null, true);

        [Fact]
        public Task Query_Int_OrderByDateString()
            => CreateQueryTestIntId(
                "Order by date and string - 50 movies, ordered by release date, then title",
                null, 50, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) });

        [Fact]
        public Task Query_Int_OrderByNumber()
            => CreateQueryTestIntId(
                "Order by number - 30 shortest movies since 1970",
                m => m.Year >= 1970, 30, null, new[] { new OrderByClause("Duration", true), new OrderByClause("Title", true) }, null, true);

        [Fact]
        public Task Query_String_OrderByDateString()
            => CreateQueryTestStringId(
                "Order by date and string - 50 movies, ordered by release date, then title",
                null, 50, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) });

        [Fact]
        public Task Query_String_OrderByNumber()
            => CreateQueryTestStringId(
                "Order by number - 30 shortest movies since 1970",
                m => m.Year >= 1970, 30, null, new[] { new OrderByClause("Duration", true), new OrderByClause("Title", true) }, null, true);

        [Fact]
        public Task Query_Int_SelectOneField()
            => CreateQueryTestIntId(
                "Select one field - Only title of movies from 2008",
                m => m.Year == 2008, null, null, null, m => m.Title);

        [Fact]
        public Task Query_Int_SelectMultiField()
            => CreateQueryTestIntId(
                "Select multiple fields - Nicely formatted list of movies from the 2000's",
                m => m.Year >= 2000, 200, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) },
                m => string.Format("{0} {1} - {2} minutes", m.Title.PadRight(30), m.BestPictureWinner ? "(best picture)" : "", m.Duration));

        [Fact]
        public Task Query_String_SelectOneField()
            => CreateQueryTestStringId(
                "Select one field - Only title of movies from 2008",
                m => m.Year == 2008, null, null, null, m => m.Title);

        [Fact]
        public Task Query_String_SelectMultiField()
            => CreateQueryTestStringId(
                "Select multiple fields - Nicely formatted list of movies from the 2000's",
                m => m.Year >= 2000, 200, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) },
                m => string.Format("{0} {1} - {2} minutes", m.Title.PadRight(30), m.BestPictureWinner ? "(best picture)" : "", m.Duration));

        [Fact]
        public Task Query_Int_ODataQuery()
            => CreateQueryTestIntId(
                "Passing OData query directly - movies from the 80's, ordered by Title, items 3, 4 and 5",
                whereClause: m => m.Year >= 1980 && m.Year <= 1989,
                top: 3, skip: 2,
                orderBy: new OrderByClause[] { new OrderByClause("Title", true) },
                odataQueryExpression: "$filter=((Year ge 1980) and (Year le 1989))&$top=3&$skip=2&$orderby=Title asc");

        [Fact]
        public Task Query_String_ODataQuery()
            => CreateQueryTestStringId(
                "Passing OData query directly - movies from the 80's, ordered by Title, items 3, 4 and 5",
                whereClause: m => m.Year >= 1980 && m.Year <= 1989,
                top: 3, skip: 2,
                orderBy: new OrderByClause[] { new OrderByClause("Title", true) },
                odataQueryExpression: "$filter=((Year ge 1980) and (Year le 1989))&$top=3&$skip=2&$orderby=Title asc");

        [Fact]
        public Task Query_IntNeg_LargeTop()
            => CreateQueryTest<IntIdMovie, MobileServiceInvalidOperationException>("[Int id] (Neg) Very large top value", m => m.Year > 2000, VeryLargeTopValue);

        [Fact]
        public Task Query_StringNeg_LargeTop()
            => CreateQueryTest<Movie, MobileServiceInvalidOperationException>("[String id] (Neg) Very large top value", m => m.Year > 2000, VeryLargeTopValue);

        [Fact]
        public Task Query_IntNeg_UnsupportedPredicate()
            => CreateQueryTest<IntIdMovie, NotSupportedException>("[Int id] (Neg) Unsupported predicate: unsupported arithmetic",
                m => Math.Sqrt(m.Year) > 43);

        [Fact]
        public Task Query_StringNeg_UnsupportedPredicate()
            => CreateQueryTest<Movie, NotSupportedException>("[String id] (Neg) Unsupported predicate: unsupported arithmetic",
                m => Math.Sqrt(m.Year) > 43);

        [Fact]
        public Task Query_Neg_Lookup()
            => Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                var table = GetClient().GetTable<IntIdMovie>();
                await table.LookupAsync(-1);
            });

        class OrderByClause
        {
            public OrderByClause(string fieldName, bool isAscending)
            {
                this.FieldName = fieldName;
                this.IsAscending = isAscending;
            }

            public bool IsAscending { get; private set; }
            public string FieldName { get; private set; }
        }

        private async Task CreateQueryTestIntId(
            string name, Expression<Func<IntIdMovie, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<IntIdMovie, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataQueryExpression = null, Func<IntIdMovie, bool> whereLambda = null)
        {
            await CreateQueryTest<IntIdMovie, ExceptionTypeWhichWillNeverBeThrown>(
                "[Int id] " + name, whereClause, top, skip, orderBy, selectExpression, includeTotalCount, odataQueryExpression, false, whereLambda);
        }

        private async Task CreateQueryTestStringId(
            string name, Expression<Func<Movie, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<Movie, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataQueryExpression = null, Func<Movie, bool> whereLambda = null)
        {
            await CreateQueryTest<Movie, ExceptionTypeWhichWillNeverBeThrown>(
                "[String id] " + name, whereClause, top, skip, orderBy, selectExpression, includeTotalCount, odataQueryExpression, true, whereLambda);
        }

        private async Task CreateQueryTest<MovieType>(
            string name, Expression<Func<MovieType, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<MovieType, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataQueryExpression = null, bool useStringIdTable = false,
            Func<MovieType, bool> whereLambda = null) where MovieType : class, IMovie
        {
            await CreateQueryTest<MovieType, ExceptionTypeWhichWillNeverBeThrown>(name, whereClause, top, skip, orderBy, selectExpression, includeTotalCount, odataQueryExpression, whereLambda: whereLambda);
        }

        private async Task CreateQueryTest<MovieType, TExpectedException>(
            string name, Expression<Func<MovieType, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<MovieType, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataExpression = null, bool useStringIdTable = false,
            Func<MovieType, bool> whereLambda = null)
            where MovieType : class, IMovie
            where TExpectedException : Exception
        {
            Assert.False(whereClause == null && whereLambda != null);

            try
            {
                var table = this.GetClient().GetTable<MovieType>();
                IEnumerable<MovieType> readMovies = null;
                IEnumerable<string> readProjectedMovies = null;

                if (odataExpression == null)
                {
                    IMobileServiceTableQuery<MovieType> query = null;
                    IMobileServiceTableQuery<string> selectedQuery = null;

                    if (whereClause != null)
                    {
                        query = table.Where(whereClause);
                    }

                    if (orderBy != null)
                    {
                        if (query == null)
                        {
                            query = table.Where(m => m.Duration == m.Duration);
                        }

                        query = ApplyOrdering(query, orderBy);
                    }

                    if (top.HasValue)
                    {
                        query = query == null ? table.Take(top.Value) : query.Take(top.Value);
                    }

                    if (skip.HasValue)
                    {
                        query = query == null ? table.Skip(skip.Value) : query.Skip(skip.Value);
                    }

                    if (selectExpression != null)
                    {
                        selectedQuery = query == null ? table.Select(selectExpression) : query.Select(selectExpression);
                    }

                    if (includeTotalCount.HasValue)
                    {
                        query = query.IncludeTotalCount();
                    }

                    if (selectedQuery == null)
                    {
                        // Both ways of querying should be equivalent, so using both with equal probability here.
                        // TODO: Make it deterministic
                        var tickCount = Environment.TickCount;
                        if ((tickCount % 2) == 0)
                        {
                            readMovies = await query.ToEnumerableAsync();
                        }
                        else
                        {
                            readMovies = await table.ReadAsync(query);
                        }
                    }
                    else
                    {
                        readProjectedMovies = await selectedQuery.ToEnumerableAsync();
                    }
                }
                else
                {
                    JToken result = await table.ReadAsync(odataExpression);
                    readMovies = result.ToObject<IEnumerable<MovieType>>();
                }

                long actualTotalCount = -1;
#pragma warning disable CS0618 // Type or member is obsolete
                ITotalCountProvider totalCountProvider = (readMovies as ITotalCountProvider) ?? (readProjectedMovies as ITotalCountProvider);
#pragma warning restore CS0618 // Type or member is obsolete
                if (totalCountProvider != null)
                {
                    actualTotalCount = totalCountProvider.TotalCount;
                }

                IEnumerable<MovieType> expectedData;
                if (useStringIdTable)
                {
                    var movies = QueryTestData.TestMovies();
                    expectedData = new MovieType[movies.Length];
                    for (var i = 0; i < movies.Length; i++)
                    {
                        ((MovieType[])expectedData)[i] = (MovieType)(IMovie)movies[i];
                    }
                }
                else
                {
                    expectedData = QueryTestData.TestIntIdMovies.Select(s => (MovieType)(IMovie)s);
                }

                // Due to a Xamarin.iOS bug, Expression.Compile() does not work for some expression trees,
                // in which case we allow the caller to provide a lambda directly and we use it instead of
                // compiling the expression tree.
                if (whereLambda != null)
                {
                    expectedData = expectedData.Where(whereLambda);
                }
                else if (whereClause != null)
                {
                    expectedData = expectedData.Where(whereClause.Compile());
                }

                long expectedTotalCount = -1;
                if (includeTotalCount.HasValue && includeTotalCount.Value)
                {
                    expectedTotalCount = expectedData.Count();
                }

                if (orderBy != null)
                {
                    expectedData = ApplyOrdering(expectedData, orderBy);
                }

                if (skip.HasValue)
                {
                    expectedData = expectedData.Skip(skip.Value);
                }

                if (top.HasValue)
                {
                    expectedData = expectedData.Take(top.Value);
                }

                if (includeTotalCount.HasValue)
                {
                    Assert.Equal(expectedTotalCount, actualTotalCount);
                }

                List<string> errors = new List<string>();
                bool expectedDataIsSameAsReadData;

                if (selectExpression != null)
                {
                    string[] expectedProjectedData = expectedData.Select(selectExpression.Compile()).ToArray();
                    expectedDataIsSameAsReadData = Utilities.CompareArrays(expectedProjectedData, readProjectedMovies.ToArray(), errors);
                }
                else
                {
                    expectedDataIsSameAsReadData = Utilities.CompareArrays(expectedData.ToArray(), readMovies.ToArray(), errors);
                }

                Assert.True(expectedDataIsSameAsReadData);

                if (typeof(TExpectedException) != typeof(ExceptionTypeWhichWillNeverBeThrown))
                {
                    Assert.True(false, "Test should have failed");
                    return;
                }
            }
            catch (TExpectedException)
            {
                return;
            }
        }

        private static IMobileServiceTableQuery<MovieType> ApplyOrdering<MovieType>(IMobileServiceTableQuery<MovieType> query, OrderByClause[] orderBy)
            where MovieType : class, IMovie
        {
            if (orderBy.Length == 1)
            {
                if (orderBy[0].IsAscending && orderBy[0].FieldName == "Title")
                {
                    return query.OrderBy(m => m.Title);
                }
                else if (!orderBy[0].IsAscending && orderBy[0].FieldName == "Year")
                {
                    return query.OrderByDescending(m => m.Year);
                }
            }
            else if (orderBy.Length == 2)
            {
                if (orderBy[1].FieldName == "Title" && orderBy[1].IsAscending)
                {
                    if (orderBy[0].FieldName == "Duration" && orderBy[0].IsAscending)
                    {
                        return query.OrderBy(m => m.Duration).ThenBy(m => m.Title);
                    }
                    else if (orderBy[0].FieldName == "ReleaseDate" && !orderBy[0].IsAscending)
                    {
                        return query.OrderByDescending(m => m.ReleaseDate).ThenBy(m => m.Title);
                    }
                }
            }

            throw new NotImplementedException(string.Format("Ordering by [{0}] not implemented yet",
                string.Join(", ", orderBy.Select(c => string.Format("{0} {1}", c.FieldName, c.IsAscending ? "asc" : "desc")))));
        }

        private static IEnumerable<MovieType> ApplyOrdering<MovieType>(IEnumerable<MovieType> data, OrderByClause[] orderBy)
            where MovieType : class, IMovie
        {
            if (orderBy.Length == 1)
            {
                if (orderBy[0].IsAscending && orderBy[0].FieldName == "Title")
                {
                    return data.OrderBy(m => m.Title);
                }
                else if (!orderBy[0].IsAscending && orderBy[0].FieldName == "Year")
                {
                    return data.OrderByDescending(m => m.Year);
                }
            }
            else if (orderBy.Length == 2)
            {
                if (orderBy[1].FieldName == "Title" && orderBy[1].IsAscending)
                {
                    if (orderBy[0].FieldName == "Duration" && orderBy[0].IsAscending)
                    {
                        return data.OrderBy(m => m.Duration).ThenBy(m => m.Title);
                    }
                    else if (orderBy[0].FieldName == "ReleaseDate" && !orderBy[0].IsAscending)
                    {
                        return data.OrderByDescending(m => m.ReleaseDate).ThenBy(m => m.Title);
                    }
                }
            }

            throw new NotImplementedException(string.Format("Ordering by [{0}] not implemented yet",
                string.Join(", ", orderBy.Select(c => string.Format("{0} {1}", c.FieldName, c.IsAscending ? "asc" : "desc")))));
        }
    }
}

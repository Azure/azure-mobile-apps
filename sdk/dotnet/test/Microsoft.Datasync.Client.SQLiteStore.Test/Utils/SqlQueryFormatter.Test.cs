// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.SQLiteStore.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Datasync.Client.SQLiteStore.Test.Utils
{
    [ExcludeFromCodeCoverage]
    public class SqlQueryFormatter_Tests
    {
        private readonly ITestOutputHelper testLogger;

        public SqlQueryFormatter_Tests(ITestOutputHelper logger)
        {
            testLogger = logger;
        }

        [Theory]
        [InlineData("", "SELECT * FROM [movies]", 0, "{}")]
        [InlineData("$count=true", "SELECT * FROM [movies]", 0, "{}")]
        [InlineData("$filter=((year div 1000.5) eq 2) and (rating eq 'R')", "SELECT * FROM [movies] WHERE ((([year] / @p1) = @p2) AND ([rating] = @p3))", 3, "{\"@p1\":1000.5,\"@p2\":2,\"@p3\":\"R\"}")]
        [InlineData("$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", "SELECT * FROM [movies] WHERE (((([year] - @p1) >= @p2) AND (([year] + @p3) <= @p4)) AND ([duration] <= @p5))", 5, "{\"@p1\":1900,\"@p2\":80,\"@p3\":10,\"@p4\":2000,\"@p5\":120}")]
        [InlineData("$filter=(year div 1000.5) eq 2", "SELECT * FROM [movies] WHERE (([year] / @p1) = @p2)", 2, "{\"@p1\":1000.5,\"@p2\":2}")]
        [InlineData("$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", "SELECT * FROM [movies] WHERE ((([year] >= @p1) AND ([year] <= @p2)) OR (([year] >= @p3) AND ([year] <= @p4)))", 4, "{\"@p1\":1930,\"@p2\":1940,\"@p3\":1950,\"@p4\":1960}")]
        [InlineData("$filter=(year sub 1900) ge 80", "SELECT * FROM [movies] WHERE (([year] - @p1) >= @p2)", 2, "{\"@p1\":1900,\"@p2\":80}")]
        [InlineData("$filter=bestPictureWinner eq false", "SELECT * FROM [movies] WHERE ([bestPictureWinner] = @p1)", 1, "{\"@p1\":0}")]
        [InlineData("$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (([bestPictureWinner] = @p1) AND (CEILING(([duration] / @p2)) = @p3))", 3, "{\"@p1\":1,\"@p2\":60.0,\"@p3\":2}")]
        [InlineData("$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (([bestPictureWinner] = @p1) AND (FLOOR(([duration] / @p2)) = @p3))", 3, "{\"@p1\":1,\"@p2\":60.0,\"@p3\":2}")]
        [InlineData("$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (([bestPictureWinner] = @p1) AND (ROUND(([duration] / @p2),0) = @p3))", 3, "{\"@p1\":1,\"@p2\":60.0,\"@p3\":2}")]
        [InlineData("$filter=bestPictureWinner eq true", "SELECT * FROM [movies] WHERE ([bestPictureWinner] = @p1)", 1, "{\"@p1\":1}")]
        [InlineData("$filter=bestPictureWinner ne false", "SELECT * FROM [movies] WHERE ([bestPictureWinner] != @p1)", 1, "{\"@p1\":0}")]
        [InlineData("$filter=bestPictureWinner ne true", "SELECT * FROM [movies] WHERE ([bestPictureWinner] != @p1)", 1, "{\"@p1\":1}")]
        [InlineData("$filter=ceiling(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (CEILING(([duration] / @p1)) = @p2)", 2, "{\"@p1\":60.0,\"@p2\":2}")]
        [InlineData("$filter=day(releaseDate) eq 1", "SELECT * FROM [movies] WHERE (CAST(strftime('%d', datetime([releaseDate], 'unixepoch')) AS INTEGER) = @p1)", 1, "{\"@p1\":1}")]
        [InlineData("$filter=duration ge 60", "SELECT * FROM [movies] WHERE ([duration] >= @p1)", 1, "{\"@p1\":60}")]
        [InlineData("$filter=endswith(title)", "", 0, "{}")]
        [InlineData("$filter=endswith(tolower(title))", "", 0, "{}")]
        [InlineData("$filter=endswith(toupper(title))", "", 0, "{}")]
        [InlineData("$filter=floor(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (FLOOR(([duration] / @p1)) = @p2)", 2, "{\"@p1\":60.0,\"@p2\":2}")]
        [InlineData("$filter=month(releaseDate) eq 11", "SELECT * FROM [movies] WHERE (CAST(strftime('%m', datetime([releaseDate], 'unixepoch')) AS INTEGER) = @p1)", 1, "{\"@p1\":11}")]
        [InlineData("$filter=not(bestPictureWinner eq false)", "SELECT * FROM [movies] WHERE NOT(([bestPictureWinner] = @p1))", 1, "{\"@p1\":0}")]
        [InlineData("$filter=not(bestPictureWinner eq true)", "SELECT * FROM [movies] WHERE NOT(([bestPictureWinner] = @p1))", 1, "{\"@p1\":1}")]
        [InlineData("$filter=not(bestPictureWinner ne false)", "SELECT * FROM [movies] WHERE NOT(([bestPictureWinner] != @p1))", 1, "{\"@p1\":0}")]
        [InlineData("$filter=not(bestPictureWinner ne true)", "SELECT * FROM [movies] WHERE NOT(([bestPictureWinner] != @p1))", 1, "{\"@p1\":1}")]
        [InlineData("$filter=rating eq null", "SELECT * FROM [movies] WHERE ([rating] IS NULL)", 0, "{}")]
        [InlineData("$filter=rating eq 'R'", "SELECT * FROM [movies] WHERE ([rating] = @p1)", 1, "{\"@p1\":\"R\"}")]
        [InlineData("$filter=rating ne 'PG-13'", "SELECT * FROM [movies] WHERE ([rating] != @p1)", 1, "{\"@p1\":\"PG-13\"}")]
        [InlineData("$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z)", "", 0, "{}")]
        [InlineData("$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z)", "", 0, "{}")]
        [InlineData("$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z)", "", 0, "{}")]
        [InlineData("$filter=releaseDate le cast(2000-01-01T00:00:00.000Z)", "", 0, "{}")]
        [InlineData("$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z)", "", 0, "{}")]
        [InlineData("$filter=round(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (ROUND(([duration] / @p1),0) = @p2)", 2, "{\"@p1\":60.0,\"@p2\":2}")]
        [InlineData("$filter=startswith(rating)", "", 0, "{}")]
        [InlineData("$filter=startswith(tolower(title))", "", 0, "{}")]
        [InlineData("$filter=startswith(toupper(title))", "", 0, "{}")]
        [InlineData("$filter=year eq 1994", "SELECT * FROM [movies] WHERE ([year] = @p1)", 1, "{\"@p1\":1994}")]
        [InlineData("$filter=year ge 2000 and year le 2009", "SELECT * FROM [movies] WHERE (([year] >= @p1) AND ([year] <= @p2))", 2, "{\"@p1\":2000,\"@p2\":2009}")]
        [InlineData("$filter=year ge 2000", "SELECT * FROM [movies] WHERE ([year] >= @p1)", 1, "{\"@p1\":2000}")]
        [InlineData("$filter=year gt 1999 and year lt 2010", "SELECT * FROM [movies] WHERE (([year] > @p1) AND ([year] < @p2))", 2, "{\"@p1\":1999,\"@p2\":2010}")]
        [InlineData("$filter=year gt 1999", "SELECT * FROM [movies] WHERE ([year] > @p1)", 1, "{\"@p1\":1999}")]
        [InlineData("$filter=year le 2000", "SELECT * FROM [movies] WHERE ([year] <= @p1)", 1, "{\"@p1\":2000}")]
        [InlineData("$filter=year lt 2001", "SELECT * FROM [movies] WHERE ([year] < @p1)", 1, "{\"@p1\":2001}")]
        [InlineData("$filter=year(releaseDate) eq 1994", "SELECT * FROM [movies] WHERE (CAST(strftime('%Y', datetime([releaseDate], 'unixepoch')) AS INTEGER) = @p1)", 1, "{\"@p1\":1994}")]
        [InlineData("$orderby=bestPictureWinner asc", "SELECT * FROM [movies] ORDER BY [bestPictureWinner]", 0, "{}")]
        [InlineData("$orderby=bestPictureWinner desc", "SELECT * FROM [movies] ORDER BY [bestPictureWinner] DESC", 0, "{}")]
        [InlineData("$orderby=duration asc", "SELECT * FROM [movies] ORDER BY [duration]", 0, "{}")]
        [InlineData("$orderby=duration desc", "SELECT * FROM [movies] ORDER BY [duration] DESC", 0, "{}")]
        [InlineData("$orderby=rating asc", "SELECT * FROM [movies] ORDER BY [rating]", 0, "{}")]
        [InlineData("$orderby=rating desc", "SELECT * FROM [movies] ORDER BY [rating] DESC", 0, "{}")]
        [InlineData("$orderby=releaseDate asc", "SELECT * FROM [movies] ORDER BY [releaseDate]", 0, "{}")]
        [InlineData("$orderby=releaseDate desc", "SELECT * FROM [movies] ORDER BY [releaseDate] DESC", 0, "{}")]
        [InlineData("$orderby=title asc", "SELECT * FROM [movies] ORDER BY [title]", 0, "{}")]
        [InlineData("$orderby=title desc", "SELECT * FROM [movies] ORDER BY [title] DESC", 0, "{}")]
        [InlineData("$orderby=year asc", "SELECT * FROM [movies] ORDER BY [year]", 0, "{}")]
        [InlineData("$orderby=year,title asc", "SELECT * FROM [movies] ORDER BY [year], [title]", 0, "{}")]
        [InlineData("$orderby=year,title desc", "SELECT * FROM [movies] ORDER BY [year], [title] DESC", 0, "{}")]
        [InlineData("$orderby=year desc", "SELECT * FROM [movies] ORDER BY [year] DESC", 0, "{}")]
        [InlineData("$orderby=year desc,title", "SELECT * FROM [movies] ORDER BY [year] DESC, [title]", 0, "{}")]
        [InlineData("$orderby=year desc,title desc", "SELECT * FROM [movies] ORDER BY [year] DESC, [title] DESC", 0, "{}")]
        [InlineData("$top=125&$filter=((year div 1000.5) eq 2) and (rating eq 'R')", "SELECT * FROM [movies] WHERE ((([year] / @p1) = @p2) AND ([rating] = @p3)) LIMIT 125", 3, "{\"@p1\":1000.5,\"@p2\":2,\"@p3\":\"R\"}")]
        [InlineData("$top=125&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)", "SELECT * FROM [movies] WHERE (((([year] - @p1) >= @p2) AND (([year] + @p3) <= @p4)) AND ([duration] <= @p5)) LIMIT 125", 5, "{\"@p1\":1900,\"@p2\":80,\"@p3\":10,\"@p4\":2000,\"@p5\":120}")]
        [InlineData("$top=125&$filter=(year div 1000.5) eq 2", "SELECT * FROM [movies] WHERE (([year] / @p1) = @p2) LIMIT 125", 2, "{\"@p1\":1000.5,\"@p2\":2}")]
        [InlineData("$top=125&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)", "SELECT * FROM [movies] WHERE ((([year] >= @p1) AND ([year] <= @p2)) OR (([year] >= @p3) AND ([year] <= @p4))) LIMIT 125", 4, "{\"@p1\":1930,\"@p2\":1940,\"@p3\":1950,\"@p4\":1960}")]
        [InlineData("$top=125&$filter=(year sub 1900) ge 80", "SELECT * FROM [movies] WHERE (([year] - @p1) >= @p2) LIMIT 125", 2, "{\"@p1\":1900,\"@p2\":80}")]
        [InlineData("$top=125&$filter=bestPictureWinner eq false", "SELECT * FROM [movies] WHERE ([bestPictureWinner] = @p1) LIMIT 125", 1, "{\"@p1\":0}")]
        [InlineData("$top=125&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (([bestPictureWinner] = @p1) AND (CEILING(([duration] / @p2)) = @p3)) LIMIT 125", 3, "{\"@p1\":1,\"@p2\":60.0,\"@p3\":2}")]
        [InlineData("$top=125&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (([bestPictureWinner] = @p1) AND (FLOOR(([duration] / @p2)) = @p3)) LIMIT 125", 3, "{\"@p1\":1,\"@p2\":60.0,\"@p3\":2}")]
        [InlineData("$top=125&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (([bestPictureWinner] = @p1) AND (ROUND(([duration] / @p2),0) = @p3)) LIMIT 125", 3, "{\"@p1\":1,\"@p2\":60.0,\"@p3\":2}")]
        [InlineData("$top=125&$filter=bestPictureWinner eq true", "SELECT * FROM [movies] WHERE ([bestPictureWinner] = @p1) LIMIT 125", 1, "{\"@p1\":1}")]
        [InlineData("$top=125&$filter=bestPictureWinner ne false", "SELECT * FROM [movies] WHERE ([bestPictureWinner] != @p1) LIMIT 125", 1, "{\"@p1\":0}")]
        [InlineData("$top=125&$filter=bestPictureWinner ne true", "SELECT * FROM [movies] WHERE ([bestPictureWinner] != @p1) LIMIT 125", 1, "{\"@p1\":1}")]
        [InlineData("$top=125&$filter=ceiling(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (CEILING(([duration] / @p1)) = @p2) LIMIT 125", 2, "{\"@p1\":60.0,\"@p2\":2}")]
        [InlineData("$top=125&$filter=day(releaseDate) eq 1", "SELECT * FROM [movies] WHERE (CAST(strftime('%d', datetime([releaseDate], 'unixepoch')) AS INTEGER) = @p1) LIMIT 125", 1, "{\"@p1\":1}")]
        [InlineData("$top=125&$filter=duration ge 60", "SELECT * FROM [movies] WHERE ([duration] >= @p1) LIMIT 125", 1, "{\"@p1\":60}")]
        [InlineData("$top=125&$filter=endswith(title)", "", 0, "{}")]
        [InlineData("$top=125&$filter=endswith(tolower(title))", "", 0, "{}")]
        [InlineData("$top=125&$filter=endswith(toupper(title))", "", 0, "{}")]
        [InlineData("$top=125&$filter=floor(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (FLOOR(([duration] / @p1)) = @p2) LIMIT 125", 2, "{\"@p1\":60.0,\"@p2\":2}")]
        [InlineData("$top=125&$filter=month(releaseDate) eq 11", "SELECT * FROM [movies] WHERE (CAST(strftime('%m', datetime([releaseDate], 'unixepoch')) AS INTEGER) = @p1) LIMIT 125", 1, "{\"@p1\":11}")]
        [InlineData("$top=125&$filter=not(bestPictureWinner eq false)", "SELECT * FROM [movies] WHERE NOT(([bestPictureWinner] = @p1)) LIMIT 125", 1, "{\"@p1\":0}")]
        [InlineData("$top=125&$filter=not(bestPictureWinner eq true)", "SELECT * FROM [movies] WHERE NOT(([bestPictureWinner] = @p1)) LIMIT 125", 1, "{\"@p1\":1}")]
        [InlineData("$top=125&$filter=not(bestPictureWinner ne false)", "SELECT * FROM [movies] WHERE NOT(([bestPictureWinner] != @p1)) LIMIT 125", 1, "{\"@p1\":0}")]
        [InlineData("$top=125&$filter=not(bestPictureWinner ne true)", "SELECT * FROM [movies] WHERE NOT(([bestPictureWinner] != @p1)) LIMIT 125", 1, "{\"@p1\":1}")]
        [InlineData("$top=125&$filter=rating eq null", "SELECT * FROM [movies] WHERE ([rating] IS NULL) LIMIT 125", 0, "{}")]
        [InlineData("$top=125&$filter=rating eq 'R'", "SELECT * FROM [movies] WHERE ([rating] = @p1) LIMIT 125", 1, "{\"@p1\":\"R\"}")]
        [InlineData("$top=125&$filter=rating ne 'PG-13'", "SELECT * FROM [movies] WHERE ([rating] != @p1) LIMIT 125", 1, "{\"@p1\":\"PG-13\"}")]
        [InlineData("$top=125&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z)", "", 0, "{}")]
        [InlineData("$top=125&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z)", "", 0, "{}")]
        [InlineData("$top=125&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z)", "", 0, "{}")]
        [InlineData("$top=125&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z)", "", 0, "{}")]
        [InlineData("$top=125&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z)", "", 0, "{}")]
        [InlineData("$top=125&$filter=round(duration div 60.0) eq 2", "SELECT * FROM [movies] WHERE (ROUND(([duration] / @p1),0) = @p2) LIMIT 125", 2, "{\"@p1\":60.0,\"@p2\":2}")]
        [InlineData("$top=125&$filter=startswith(rating)", "", 0, "{}")]
        [InlineData("$top=125&$filter=startswith(tolower(title))", "", 0, "{}")]
        [InlineData("$top=125&$filter=startswith(toupper(title))", "", 0, "{}")]
        [InlineData("$top=125&$filter=year eq 1994", "SELECT * FROM [movies] WHERE ([year] = @p1) LIMIT 125", 1, "{\"@p1\":1994}")]
        [InlineData("$top=125&$filter=year ge 2000 and year le 2009", "SELECT * FROM [movies] WHERE (([year] >= @p1) AND ([year] <= @p2)) LIMIT 125", 2, "{\"@p1\":2000,\"@p2\":2009}")]
        [InlineData("$top=125&$filter=year ge 2000", "SELECT * FROM [movies] WHERE ([year] >= @p1) LIMIT 125", 1, "{\"@p1\":2000}")]
        [InlineData("$top=125&$filter=year gt 1999 and year lt 2010", "SELECT * FROM [movies] WHERE (([year] > @p1) AND ([year] < @p2)) LIMIT 125", 2, "{\"@p1\":1999,\"@p2\":2010}")]
        [InlineData("$top=125&$filter=year gt 1999", "SELECT * FROM [movies] WHERE ([year] > @p1) LIMIT 125", 1, "{\"@p1\":1999}")]
        [InlineData("$top=125&$filter=year le 2000", "SELECT * FROM [movies] WHERE ([year] <= @p1) LIMIT 125", 1, "{\"@p1\":2000}")]
        [InlineData("$top=125&$filter=year lt 2001", "SELECT * FROM [movies] WHERE ([year] < @p1) LIMIT 125", 1, "{\"@p1\":2001}")]
        [InlineData("$top=125&$filter=year(releaseDate) eq 1994", "SELECT * FROM [movies] WHERE (CAST(strftime('%Y', datetime([releaseDate], 'unixepoch')) AS INTEGER) = @p1) LIMIT 125", 1, "{\"@p1\":1994}")]
        [InlineData("$top=125&$orderby=bestPictureWinner asc", "SELECT * FROM [movies] ORDER BY [bestPictureWinner] LIMIT 125", 0, "{}")]
        [InlineData("$top=125&$orderby=bestPictureWinner desc", "SELECT * FROM [movies] ORDER BY [bestPictureWinner] DESC LIMIT 125", 0, "{}")]
        [InlineData("$top=125&$orderby=duration", "SELECT * FROM [movies] ORDER BY [duration] LIMIT 125", 0, "{}")]
        [InlineData("$top=125&$orderby=duration desc", "SELECT * FROM [movies] ORDER BY [duration] DESC LIMIT 125", 0, "{}")]
        public void SqlQueryFormatter_Works(string filter, string expected, int paramCount, string expectedParameters)
        {
            QueryDescription query = QueryDescription.Parse("movies", filter);

            string actual = SqlQueryFormatter.FormatSelectStatement(query, out Dictionary<string, object> parameters);
            testLogger.WriteLine(filter);
            testLogger.WriteLine(actual);
            testLogger.WriteLine(JObject.FromObject(parameters).ToString(Formatting.None).Replace("\"", "\\\""));

            Assert.Equal(expected, actual);
            Assert.NotNull(parameters);
            Assert.Equal(paramCount, parameters.Count);
            Assert.Equal(expectedParameters, JObject.FromObject(parameters).ToString(Formatting.None));
        }
    }
}

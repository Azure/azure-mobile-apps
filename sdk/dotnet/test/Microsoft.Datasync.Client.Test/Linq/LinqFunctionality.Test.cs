// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Linq;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Linq
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class LinqFunctionality : BaseTest
    {
        private class SelectResult
        {
            public string Id { get; set; }
            public string Title { get; set; }
        }

        [Theory]
        [ClassData(typeof(LinqTestCases))]
        internal void LinqODataConversions(string testCase, Func<ITableQuery<Movie>, ITableQuery<Movie>> func, string expected)
        {
            // Arrange
            var table = new DatasyncTable<Movie>(new Uri("https://localhost/tables/movies"), HttpClient, ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act
            var actual = (func.Invoke(query) as DatasyncTableQuery<Movie>)?.ToODataQueryString();
            var tester = Uri.UnescapeDataString(actual);

            // Assert
            Assert.NotNull(actual);
            Assert.True(tester.Equals(expected), $"Test '{testCase}' did not match\nExpected: {expected}\nActual  : {tester}");
        }

        [Theory]
        [ClassData(typeof(LinqTestCases))]
        internal void LinqODataWithSelectConversions(string testCase, Func<ITableQuery<Movie>, ITableQuery<Movie>> func, string expected)
        {
            // Arrange
            var table = new DatasyncTable<Movie>(new Uri("https://localhost/tables/movies"), HttpClient, ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Need to make sure the $select statement is added in the right spot.
            var splitArgs = expected.Split('&').ToList();
            splitArgs.Add("$select=id,title");
            splitArgs.Sort();
            var expectedWithSelect = string.Join('&', splitArgs).TrimStart('&');

            // Act
            var actual = (func.Invoke(query).Select(m => new SelectResult { Id = m.Id, Title = m.Title }) as DatasyncTableQuery<SelectResult>)?.ToODataQueryString();
            var tester = Uri.UnescapeDataString(actual);

            // Assert
            Assert.NotNull(actual);
            Assert.True(tester.Equals(expectedWithSelect), $"Test '{testCase}' did not match (with select)\nExpected: {expectedWithSelect}\nActual  : {tester}");
        }

        [Fact]
        public void Linq_NotSupportedProperties()
        {
            // Arrange
            var table = new DatasyncTable<Movie>(new Uri("https://localhost/tables/movies"), HttpClient, ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act
            var actual = query.Where(m => m.ReleaseDate.UtcDateTime > new DateTime(2001, 12, 31)) as DatasyncTableQuery<Movie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedMethods()
        {
            // Arrange
            var table = new DatasyncTable<Movie>(new Uri("https://localhost/tables/movies"), HttpClient, ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act
            var actual = query.Where(m => m.Title.LastIndexOf("er") > 0) as DatasyncTableQuery<Movie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedBinaryOperators()
        {
            // Arrange
            var table = new DatasyncTable<Movie>(new Uri("https://localhost/tables/movies"), HttpClient, ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act
            var actual = query.Where(m => (m.Year ^ 1024) == 0) as DatasyncTableQuery<Movie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedUnaryOperators()
        {
            // Arrange
            var table = new DatasyncTable<Movie>(new Uri("https://localhost/tables/movies"), HttpClient, ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act
            var actual = query.Where(m => (5 * (-m.Duration)) > -180) as DatasyncTableQuery<Movie>;
            Assert.Throws<NotSupportedException>(() => actual.ToODataQueryString());
        }

        [Fact]
        public void Linq_NotSupportedDistinctLinqStatement()
        {
            // Arrange
            var table = new DatasyncTable<Movie>(new Uri("https://localhost/tables/movies"), HttpClient, ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table);

            // Act - really - you should NOT be doing this!
            query.Query = query.Query.Distinct();

            // Assert
            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_NegateNotSupported()
        {
            // Arrange
            var table = new DatasyncTable<Movie>(new Uri("https://localhost/tables/movies"), HttpClient, ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table).Where(m => (-m.Year) <= -2000) as DatasyncTableQuery<Movie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_InvalidOrderBy_Lambda()
        {
            // Arrange
            var table = new DatasyncTable<Movie>(new Uri("https://localhost/tables/movies"), HttpClient, ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table).OrderBy(m => m.Id == "foo" ? "yes" : "no") as DatasyncTableQuery<Movie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_InvalidOrderBy_Method()
        {
            // Arrange
            var table = new DatasyncTable<Movie>(new Uri("https://localhost/tables/movies"), HttpClient, ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table).OrderBy(m => m.GetHashCode()) as DatasyncTableQuery<Movie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }

        [Fact]
        public void Linq_InvalidOrderBy_ToString()
        {
            // Arrange
            var table = new DatasyncTable<Movie>(new Uri("https://localhost/tables/movies"), HttpClient, ClientOptions);
            var query = new DatasyncTableQuery<Movie>(table).OrderBy(m => m.ReleaseDate.ToString("o")) as DatasyncTableQuery<Movie>;

            Assert.Throws<NotSupportedException>(() => query.ToODataQueryString());
        }
    }
}

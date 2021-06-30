// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Integration.Helpers;
using Microsoft.Datasync.Client.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Integration
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Query_Tests : BaseTest
    {
        [Theory]
        [ClassData(typeof(QueryTestCases))]
        public async Task Query_ToAsyncEnumerable(string testCase, Func<ITableQuery<Movie>, ITableQuery<Movie>> func, int expectedItemCount, string[] firstItemsExpected)
        {
            // Arrange
            var client = GetClient();
            var table = client.GetTable<Movie>();
            var query = table.WithParameter("testcase", testCase);

            // Act
            var actual = func.Invoke(query).ToAsyncEnumerable();
            List<Movie> items = new();
            await foreach (var item in actual)
            {
                items.Add(item);
            }
            var firstItemsActual = items.Take(firstItemsExpected.Length).Select(m => m.Id).ToArray();

            // Assert
            Assert.True(expectedItemCount == items.Count,
                $"Expected Counts differ for test case {testCase}\nExpected: {expectedItemCount}\nActual  : {items.Count}\n");
            Assert.True(firstItemsExpected.SequenceEqual(firstItemsActual),
                $"Expected initial items differ for test case {testCase}\nExpected: {string.Join(",", firstItemsExpected)}\nActual  : {string.Join(",", firstItemsActual)}");
        }
    }
}

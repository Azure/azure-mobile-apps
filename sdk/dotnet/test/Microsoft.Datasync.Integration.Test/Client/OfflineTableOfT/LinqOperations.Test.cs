﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTableOfT
{
    [ExcludeFromCodeCoverage]
    public class LinqOperation_Tests : BaseOperationTest
    {
        public LinqOperation_Tests(ITestOutputHelper logger) : base(logger, false) { }

        [Fact]
        [Trait("Method", "ToArray")]
        public async Task ToArray_Test()
        {
            //Arrange
            await InitializeAsync();

            // Act
            var array = await table!.ToArrayAsync();

            // Assert
            Assert.NotNull(array);
            Assert.Equal(MovieCount, array.Length);
            foreach (var item in array)
            {
                Assert.NotNull(item.Id);
                var expected = MovieServer.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);
            }
        }

        [Fact]
        [Trait("Method", "ToDictionary")]
        public async Task ToDictionary_Test()
        {
            //Arrange
            await InitializeAsync();

            // Act
            var dict = await table!.ToDictionaryAsync(m => m.Id);

            // Assert
            Assert.NotNull(dict);
            Assert.Equal(MovieCount, dict.Count);
            foreach (var item in dict)
            {
                Assert.NotNull(item.Key);
                var expected = MovieServer.GetMovieById(item.Key);
                Assert.Equal<IMovie>(expected, item.Value);
            }
        }

        [Fact]
        [Trait("Method", "ToEnumerable")]
        public async Task ToEnumerable_Test()
        {
            //Arrange
            await InitializeAsync();

            // Act
            var enumerable = table!.ToEnumerable();

            // Assert
            Assert.NotNull(enumerable);
            var count = 0;
            foreach (var item in enumerable)
            {
                count++;
                Assert.NotNull(item.Id);
                var expected = MovieServer.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);
            }
            Assert.Equal(MovieCount, count);
        }

        [Fact]
        [Trait("Method", "ToHashSet")]
        public async Task ToHashSet_Test()
        {
            //Arrange
            await InitializeAsync();

            // Act
            var set = await table!.ToHashSetAsync();

            // Assert
            Assert.NotNull(set);
            foreach (var item in set)
            {
                Assert.NotNull(item.Id);
                var expected = MovieServer.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);
            }
            Assert.Equal(MovieCount, set.Count);
        }

        [Fact]
        [Trait("Method", "ToList")]
        public async Task ToList_Test()
        {
            //Arrange
            await InitializeAsync();

            // Act
            var set = await table!.ToListAsync();

            // Assert
            Assert.NotNull(set);
            foreach (var item in set)
            {
                Assert.NotNull(item.Id);
                var expected = MovieServer.GetMovieById(item.Id);
                Assert.Equal<IMovie>(expected, item);
            }
            Assert.Equal(MovieCount, set.Count);
        }
    }
}
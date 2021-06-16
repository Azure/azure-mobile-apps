// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Extensions
{
    [ExcludeFromCodeCoverage]
    public class IDictionary_Tests
    {
        #region AddRange
        [Fact]
        public void AddRange_NullList_Throws()
        {
            // Arrange
            Dictionary<string, int> sut = new();
            IEnumerable<KeyValuePair<string, int>> list = null;

            // Act
            Assert.Throws<ArgumentNullException>(() => sut.AddRange(list));
        }

        [Fact]
        public void AddRange_EmptyList_AddsNothing()
        {
            // Arrange
            Dictionary<string, int> sut = new() { { "the-answer", 42 } };
            Dictionary<string, int> list = new();
            Dictionary<string, int> expected = new()
            {
                { "the-answer", 42 }
            };

            // Act
            var actual = sut.AddRange(list);

            // Assert
            Assert.Same(sut, actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddRange_SingleItem_AddsItem()
        {
            // Arrange
            Dictionary<string, int> sut = new() { { "the-answer", 42 } };
            Dictionary<string, int> list = new() { { "life", 7 } };
            Dictionary<string, int> expected = new()
            {
                { "the-answer", 42 },
                { "life", 7 }
            };

            // Act
            var actual = sut.AddRange(list);

            // Assert
            Assert.Same(sut, actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddRange_MultipleItems_AddsItems()
        {
            // Arrange
            Dictionary<string, int> sut = new() { { "the-answer", 42 } };
            Dictionary<string, int> list = new() { { "life", 7 }, { "universe", 21 } };
            Dictionary<string, int> expected = new()
            {
                { "the-answer", 42 },
                { "life", 7 },
                { "universe", 21}
            };

            // Act
            var actual = sut.AddRange(list);

            // Assert
            Assert.Same(sut, actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddRange_SingleItem_OverwritesItem()
        {
            // Arrange
            Dictionary<string, int> sut = new() { { "the-answer", 42 } };
            Dictionary<string, int> list = new() { { "the-answer", 14 } };
            Dictionary<string, int> expected = new()
            {
                { "the-answer", 14 }
            };

            // Act
            var actual = sut.AddRange(list);

            // Assert
            Assert.Same(sut, actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddRange_MultipleItems_OverwritesItem()
        {
            // Arrange
            Dictionary<string, int> sut = new() { { "the-answer", 42 } };
            Dictionary<string, int> list = new() { { "the-answer", 14 }, { "life", 7 }, { "the-universe", 21 } };
            Dictionary<string, int> expected = new()
            {
                { "the-answer", 14 },
                { "life", 7 },
                { "the-universe", 21 }
            };

            // Act
            var actual = sut.AddRange(list);

            // Assert
            Assert.Same(sut, actual);
            Assert.Equal(expected, actual);
        }
        #endregion
    }
}

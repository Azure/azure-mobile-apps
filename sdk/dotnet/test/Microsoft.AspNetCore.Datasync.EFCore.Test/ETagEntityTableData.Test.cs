// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Xunit;

namespace Microsoft.AspNetCore.Datasync.EFCore.Test
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class EtagEntityTableData_Tests
    {
        #region Test Artifacts
        private class Entity : ETagEntityTableData
        {
        }

        private static readonly DateTimeOffset[] testTimes = new DateTimeOffset[] {
            DateTimeOffset.MinValue,
            DateTimeOffset.Now.AddDays(-1),
            DateTimeOffset.Now,
            DateTimeOffset.MaxValue
        };

        private static readonly byte[][] testVersions = new byte[][]
        {
            Array.Empty<byte>(),
            Encoding.UTF8.GetBytes("abcd"),
            Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N"))
        };
        #endregion

        [Theory, PairwiseData]
        public void EqualityTests(
            [CombinatorialValues("", "t1", "t2")] string aID,
            bool aDeleted,
            [CombinatorialRange(0, 4)] int aUpdatedIndex,
            [CombinatorialRange(0, 3)] int aVersionIndex,
            [CombinatorialValues("", "t1", "t2")] string bID,
            bool bDeleted,
            [CombinatorialRange(0, 4)] int bUpdatedIndex,
            [CombinatorialRange(0, 3)] int bVersionIndex)
        {
            // Arrange
            var a = new Entity { Id = aID, Deleted = aDeleted, UpdatedAt = testTimes[aUpdatedIndex], Version = testVersions[aVersionIndex] };
            var b = new Entity { Id = bID, Deleted = bDeleted, UpdatedAt = testTimes[bUpdatedIndex], Version = testVersions[bVersionIndex] };
            var expected = aID == bID && aDeleted == bDeleted && aUpdatedIndex == bUpdatedIndex && aVersionIndex == bVersionIndex;

            // Act
            var actual = a.Equals(b);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Equals_NullOther_ReturnsFalse()
        {
            // Arrange
            var source = new Entity { Id = "test", Version = testVersions[1], UpdatedAt = DateTimeOffset.Now };
            ITableData other = null;

            // Act
            var actual = source.Equals(other);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void EntityTag_IsSet_WhenVersionUpdated()
        {
            // Arrange
            var source = new Entity { Id = "test", Version = Encoding.UTF8.GetBytes("abcd"), UpdatedAt = DateTimeOffset.Now };

            // Assert
            Assert.Equal("abcd", source.EntityTag);
        }

        [Fact]
        public void Version_IsSet_WhenEntityTagUpdated()
        {
            // Arrange
            var source = new Entity { Id = "test", EntityTag = "abcd", UpdatedAt = DateTimeOffset.Now };

            // Assert
            Assert.Equal(new byte[] { 0x61, 0x62, 0x63, 0x64 }, source.Version);
        }
    }
}

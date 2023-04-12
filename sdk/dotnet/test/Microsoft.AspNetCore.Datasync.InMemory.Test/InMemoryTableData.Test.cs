// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.InMemory.Test;

[ExcludeFromCodeCoverage]
public class InMemoryTableData_Tests
{
    #region Test Artifacts
    private class Entity : InMemoryTableData
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
        new byte[] { 0x01 },
        Guid.NewGuid().ToByteArray()
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
        var source = new Entity { Id = "test", Version = new byte[] { 0x02 }, UpdatedAt = DateTimeOffset.Now };
        ITableData other = null;

        // Act
        var actual = source.Equals(other);

        // Assert
        Assert.False(actual);
    }
}

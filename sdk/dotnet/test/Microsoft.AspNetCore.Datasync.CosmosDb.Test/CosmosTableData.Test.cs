﻿using Microsoft.AspNetCore.Datasync.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Datasync.CosmosDb.Test;

[ExcludeFromCodeCoverage]
public class CosmosTableData_Tests
{
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Deleted_CanBeRead(bool deleted)
    {
        // Arrange
        var source = new Entity { Id = "test", EntityTag = null, UpdatedAt = DateTimeOffset.Now, Deleted = deleted };

        // Assert
        Assert.Equal(deleted, source.Deleted);
    }

    [Fact]
    public void EntityTag_IsSet_WhenVersionUpdated()
    {
        // Arrange
        var source = new Entity { Id = "test", Version = Encoding.UTF8.GetBytes("abcd"), UpdatedAt = DateTimeOffset.Now };

        // Assert
        Assert.Equal("abcd", source.EntityTag);
    }

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

    [Theory, CombinatorialData]
    public void Equals_Works([CombinatorialRange(0, 5)] int offset)
    {
        DateTimeOffset dto = DateTimeOffset.Parse("2021-01-01T01:00:00Z");
        List<Entity> testEntities = new()
        {
            null,
            new Entity { Id = "nottest", EntityTag = "abcd", UpdatedAt = dto, Deleted = false },
            new Entity { Id = "test", EntityTag = "efgh", UpdatedAt = dto, Deleted = false },
            new Entity { Id = "test", EntityTag = "abcd", UpdatedAt = DateTimeOffset.UtcNow, Deleted = false },
            new Entity { Id = "test", EntityTag = "abcd", UpdatedAt = dto, Deleted = true }
        };
        var test = testEntities[offset];
        var source = new Entity { Id = "test", EntityTag = "abcd", UpdatedAt = DateTimeOffset.Parse("2021-01-01T01:00:00Z"), Deleted = false };

        Assert.False(source.Equals(test));
    }

    [Fact]
    public void Version_Empty_WhenEntityTagEmpty()
    {
        // Arrange
        var source = new Entity { Id = "test", EntityTag = null, UpdatedAt = DateTimeOffset.Now };

        // Assert
        Assert.Empty(source.Version);
    }

    [Fact]
    public void Version_IsSet_WhenEntityTagUpdated()
    {
        // Arrange
        var source = new Entity { Id = "test", EntityTag = "abcd", UpdatedAt = DateTimeOffset.Now };

        // Assert
        Assert.Equal(new byte[] { 0x61, 0x62, 0x63, 0x64 }, source.Version);
    }

    private class Entity : CosmosTableData
    {
    }
}
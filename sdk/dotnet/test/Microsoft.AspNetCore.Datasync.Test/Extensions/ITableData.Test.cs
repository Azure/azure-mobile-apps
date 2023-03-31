// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Datasync.Test.Extensions;

[ExcludeFromCodeCoverage]
public class ITableData_Tests
{
    [Fact]
    public void GetETag_Null_WhenNullEntity()
    {
        // Arrange
        ITableData entity = null;

        // Act
        string actual = entity.GetETag();

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetETag_Null_WhenNullVersion()
    {
        // Arrange
        ITableData entity = new InMemoryEntity { Version = null };

        // Act
        string actual = entity.GetETag();

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetETag_Null_WhenEmptyVersion()
    {
        // Arrange
        ITableData entity = new InMemoryEntity { Version = Array.Empty<byte>() };

        // Act
        string actual = entity.GetETag();

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetETag_Valid_WhenFilledVersion()
    {
        // Arrange
        ITableData entity = new InMemoryEntity { Version = Guid.NewGuid().ToByteArray() };

        // Act
        string actual = entity.GetETag();

        // Assert
        Assert.NotEmpty(actual);
        Assert.Matches("^\"[a-zA-Z0-9+/=]{24}\"$", actual);
    }

    [Fact]
    public void HasValidVersion_False_OnNullEntity()
    {
        // Arrange
        ITableData entity = null;

        // Act
        bool actual = entity.HasValidVersion();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void HasValidVersion_False_OnNullVersion()
    {
        // Arrange
        ITableData entity = new InMemoryEntity { Version = null };

        // Act
        bool actual = entity.HasValidVersion();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void HasValidVersion_False_OnEmptyVersion()
    {
        // Arrange
        ITableData entity = new InMemoryEntity { Version = Array.Empty<byte>() };

        // Act
        bool actual = entity.HasValidVersion();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void HasValidVersion_True_OnFilledVersion()
    {
        // Arrange
        ITableData entity = new InMemoryEntity { Version = Guid.NewGuid().ToByteArray() };

        // Act
        bool actual = entity.HasValidVersion();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void ToEntityTagHeaderValue_Null_WhenNullEntity()
    {
        // Arrange
        ITableData entity = null;

        // Act
        EntityTagHeaderValue actual = entity.ToEntityTagHeaderValue();

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ToEntityTagHeaderValue_Null_WhenNullVersion()
    {
        // Arrange
        ITableData entity = new InMemoryEntity { Version = null };

        // Act
        EntityTagHeaderValue actual = entity.ToEntityTagHeaderValue();

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ToEntityTagHeaderValue_Null_WhenEmptyVersion()
    {
        // Arrange
        ITableData entity = new InMemoryEntity { Version = Array.Empty<byte>() };

        // Act
        EntityTagHeaderValue actual = entity.ToEntityTagHeaderValue();

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ToEntityTagHeaderValue_Valid_WhenFilledVersion()
    {
        // Arrange
        ITableData entity = new InMemoryEntity { Version = Guid.NewGuid().ToByteArray() };

        // Act
        EntityTagHeaderValue actual = entity.ToEntityTagHeaderValue();

        // Assert
        Assert.NotNull(actual);
        Assert.False(actual.IsWeak);
        Assert.Matches("^\"[a-zA-Z0-9+/=]{24}\"$", actual.Tag.ToString());
    }
}

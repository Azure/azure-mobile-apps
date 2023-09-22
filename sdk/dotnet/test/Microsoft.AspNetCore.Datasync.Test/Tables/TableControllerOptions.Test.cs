// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.Test.Tables;

[ExcludeFromCodeCoverage]
public class TableControllerOptions_Tests
{
    [Fact]
    public void PageSize_IsReasonableByDefault()
    {
        // Act
        var options = new TableControllerOptions();

        // Assert
        Assert.Equal(100, options.PageSize);
    }

    [Fact]
    public void PageSize_CanRoundtrip()
    {
        // Act
        var options = new TableControllerOptions { PageSize = 50 };

        // Assert
        Assert.Equal(50, options.PageSize);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(512000)]
    public void PageSize_Throws_WhenOutOfRange(int pageSize)
    {
        // Arrange
        var options = new TableControllerOptions();

        // Act
        Assert.Throws<ArgumentException>(() => options.PageSize = pageSize);
    }

    [Fact]
    public void MaxTop_IsReasonableByDefault()
    {
        // Act
        var options = new TableControllerOptions();

        // Assert
        Assert.True(options.MaxTop > 10000);
    }

    [Fact]
    public void MaxTop_CanRoundtrip()
    {
        // Act
        var options = new TableControllerOptions { MaxTop = 50 };

        // Assert
        Assert.Equal(50, options.MaxTop);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void MaxTop_Throws_WhenOutOfRange(int maxTop)
    {
        // Arrange
        var options = new TableControllerOptions();

        // Act
        Assert.Throws<ArgumentException>(() => options.MaxTop = maxTop);
    }
}

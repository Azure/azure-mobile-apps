// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.AzureMobile.Server.Test.Tables
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
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
    }
}

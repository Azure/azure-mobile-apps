// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.Datasync.InMemory;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.Datasync.Test.Extensions
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class ITableData_Tests
    {
        #region Test artifacts
        private class Entity : InMemoryTableData
        {
        }
        #endregion

        #region GetETag
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
            ITableData entity = new Entity { Version = null };

            // Act
            string actual = entity.GetETag();

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetETag_Null_WhenEmptyVersion()
        {
            // Arrange
            ITableData entity = new Entity { Version = Array.Empty<byte>() };

            // Act
            string actual = entity.GetETag();

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetETag_Valid_WhenFilledVersion()
        {
            // Arrange
            ITableData entity = new Entity { Version = Guid.NewGuid().ToByteArray() };

            // Act
            string actual = entity.GetETag();

            // Assert
            Assert.NotEmpty(actual);
            Assert.Matches("^\"[a-zA-Z0-9+/=]{24}\"$", actual);
        }
        #endregion

        #region HasValidVersion
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
            ITableData entity = new Entity { Version = null };

            // Act
            bool actual = entity.HasValidVersion();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void HasValidVersion_False_OnEmptyVersion()
        {
            // Arrange
            ITableData entity = new Entity { Version = Array.Empty<byte>() };

            // Act
            bool actual = entity.HasValidVersion();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void HasValidVersion_True_OnFilledVersion()
        {
            // Arrange
            ITableData entity = new Entity { Version = Guid.NewGuid().ToByteArray() };

            // Act
            bool actual = entity.HasValidVersion();

            // Assert
            Assert.True(actual);
        }
        #endregion

        #region ToEntityTagHeaderValue
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
            ITableData entity = new Entity { Version = null };

            // Act
            EntityTagHeaderValue actual = entity.ToEntityTagHeaderValue();

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void ToEntityTagHeaderValue_Null_WhenEmptyVersion()
        {
            // Arrange
            ITableData entity = new Entity { Version = Array.Empty<byte>() };

            // Act
            EntityTagHeaderValue actual = entity.ToEntityTagHeaderValue();

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void ToEntityTagHeaderValue_Valid_WhenFilledVersion()
        {
            // Arrange
            ITableData entity = new Entity { Version = Guid.NewGuid().ToByteArray() };

            // Act
            EntityTagHeaderValue actual = entity.ToEntityTagHeaderValue();

            // Assert
            Assert.NotNull(actual);
            Assert.False(actual.IsWeak);
            Assert.Matches("^\"[a-zA-Z0-9+/=]{24}\"$", actual.Tag.ToString());
        }
        #endregion
    }
}

// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Text;
using Moq;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class StorageDataTests
    {
        private const string PartitionKey = "你好";
        private const string RowKey = "世界";

        private Mock<StorageData> dataMock;
        private StorageData data;

        public StorageDataTests()
        {
            this.dataMock = new Mock<StorageData>(PartitionKey, RowKey) { CallBase = true };
            this.data = this.dataMock.Object;
        }

        [Fact]
        public void PartitionKey_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.data, d => d.PartitionKey, PropertySetter.NullRoundtrips, defaultValue: PartitionKey, roundtripValue: "Hello");
        }

        [Fact]
        public void RowKey_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.data, d => d.RowKey, PropertySetter.NullRoundtrips, defaultValue: RowKey, roundtripValue: "World");
        }

        [Fact]
        public void Id_GetsPartitionAndRowKeyAsCompositeKey()
        {
            // Act
            string actual = this.data.Id;

            // Assert
            Assert.Equal("'你好','世界'", actual);
        }

        [Fact]
        public void Id_GetsPartitionAndRowKeyFromCompositeKey()
        {
            // Arrange
            this.data.PartitionKey = null;
            this.data.RowKey = null;

            // Act
            string actual = this.data.Id;

            // Assert
            Assert.Equal("'',''", actual);
        }

        [Fact]
        public void Id_SetsPartitionAndRowKeyFromCompositeKey()
        {
            // Arrange
            string key = "'Hello','World'";

            // Act
            this.data.Id = key;

            // Assert
            Assert.Equal(this.data.PartitionKey, "Hello");
            Assert.Equal(this.data.RowKey, "World");
        }

        [Fact]
        public void Id_Set_Throws_IfInvalidCompositeKey()
        {
            // Arrange
            string key = "HelloWorld";

            // Act
            ArgumentException ex = Assert.Throws<ArgumentException>(() => this.data.Id = key);

            // Assert
            Assert.Equal("The key 'HelloWorld' is not valid. It must be a comma separated tuple representing a partition key and a row key.", ex.Message);
        }

        [Fact]
        public void Version_GetsETagIfNull()
        {
            // Act
            this.data.ETag = null;
            byte[] actual = this.data.Version;

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void Version_SetsETagIfNull()
        {
            // Act
            this.data.Version = null;

            // Assert
            Assert.Null(this.data.ETag);
        }

        [Fact]
        public void Version_GetsETag()
        {
            // Arrange
            DateTimeOffset utc = DateTimeOffset.UtcNow;
            this.data.ETag = utc.ToString();

            // Act
            byte[] actual = this.data.Version;

            // Assert
            string actualUtc = Encoding.UTF8.GetString(actual);
            Assert.Equal(utc.ToString(), actualUtc);
        }

        [Fact]
        public void Version_SetsETag()
        {
            // Arrange
            DateTimeOffset utc = DateTimeOffset.UtcNow;
            this.data.ETag = utc.ToString();

            // Act
            this.data.Version = Encoding.UTF8.GetBytes(utc.ToString());
            string actual = this.data.ETag;

            // Assert
            Assert.Equal(utc.ToString(), actual);
        }

        [Fact]
        public void UpdatedAt_GetsTimestamp()
        {
            // Arrange
            DateTimeOffset expected = DateTimeOffset.UtcNow;
            this.data.Timestamp = expected;

            // Act
            DateTimeOffset? actual = this.data.UpdatedAt;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UpdatedAt_SetsTimestamp()
        {
            // Arrange
            DateTimeOffset expected = DateTimeOffset.UtcNow;
            this.data.UpdatedAt = expected;

            // Act
            DateTimeOffset actual = this.data.Timestamp;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}

// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using Xunit;

namespace MobileClient.Tests.Table
{
    public class MobileServiceObjectReader_Test
    {
        [Fact]
        public void GetUpdatedAt_ReturnsSystemProperty_ByDefault()
        {
            DateTimeOffset value = DateTimeOffset.Parse("2001-02-03T00:00:00.0000000+00:00");

            DateTimeOffset? actualValue = new MobileServiceObjectReader().GetUpdatedAt(new JObject() { { "updatedAt", new JValue(value) } });

            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void GetUpdatedAt_ReturnsNull_WhenNotPresent()
        {
            Assert.Null(new MobileServiceObjectReader().GetUpdatedAt(new JObject()));
        }

        [Fact]
        public void GetUpdatedAt_ReturnsCustomProperty_WhenSet()
        {
            DateTimeOffset value = DateTimeOffset.Parse("2001-02-03T00:00:00.0000000+00:00");
            var reader = new MobileServiceObjectReader() { UpdatedAtPropertyName = "Timestamp" };

            DateTimeOffset? actualValue = reader.GetUpdatedAt(new JObject() { { "Timestamp", new JValue(value) } });

            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void GetCreatedAt_ReturnsSystemProperty_ByDefault()
        {
            DateTimeOffset value = DateTimeOffset.Parse("2001-02-03T00:00:00.0000000+00:00");

            DateTimeOffset? actualValue = new MobileServiceObjectReader().GetCreatedAt(new JObject() { { "createdAt", new JValue(value) } });

            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void GetCreatedAt_ReturnsNull_WhenNotPresent()
        {
            Assert.Null(new MobileServiceObjectReader().GetCreatedAt(new JObject()));
        }

        [Fact]
        public void GetCreatedAt_ReturnsCustomProperty_WhenSet()
        {
            DateTimeOffset value = DateTimeOffset.Parse("2001-02-03T00:00:00.0000000+00:00");
            var reader = new MobileServiceObjectReader() { CreatedAtPropertyName = "Timestamp" };

            DateTimeOffset? actualValue = reader.GetCreatedAt(new JObject() { { "Timestamp", new JValue(value) } });

            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void GetId_ReturnsSystemProperty_ByDefault()
        {
            string value = "abc";

            string actualValue = new MobileServiceObjectReader().GetId(new JObject() { { "id", new JValue(value) } });

            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void GetId_ReturnsNull_WhenNotPresent()
        {
            Assert.Null(new MobileServiceObjectReader().GetId(new JObject()));
        }

        [Fact]
        public void GetId_ReturnsCustomProperty_WhenSet()
        {
            string value = "abc";
            var reader = new MobileServiceObjectReader() { IdPropertyName = "Name" };

            string actualValue = reader.GetId(new JObject() { { "Name", new JValue(value) } });

            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void GetVersion_ReturnsSystemProperty_ByDefault()
        {
            string value = "abc";

            string actualValue = new MobileServiceObjectReader().GetVersion(new JObject() { { "version", new JValue(value) } });

            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void GetVersion_ReturnsNull_WhenNotPresent()
        {
            Assert.Null(new MobileServiceObjectReader().GetVersion(new JObject()));
        }

        [Fact]
        public void GetVersion_ReturnsCustomProperty_WhenSet()
        {
            string value = "abc";
            var reader = new MobileServiceObjectReader() { VersionPropertyName = "ETag" };

            string actualValue = reader.GetVersion(new JObject() { { "ETag", new JValue(value) } });

            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void IsDeleted_ReturnsSystemProperty_ByDefault()
        {
            bool value = true;

            bool actualValue = new MobileServiceObjectReader().IsDeleted(new JObject() { { "deleted", new JValue(value) } });

            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void IsDeleted_ReturnsFalse_WhenNotPresent()
        {
            Assert.False(new MobileServiceObjectReader().IsDeleted(new JObject()));
        }

        [Fact]
        public void IsDeleted_ReturnsCustomProperty_WhenSet()
        {
            bool value = true;
            var reader = new MobileServiceObjectReader() { DeletedPropertyName = "Deleted" };

            bool actualValue = reader.IsDeleted(new JObject() { { "Deleted", new JValue(value) } });

            Assert.Equal(value, actualValue);
        }
    }
}

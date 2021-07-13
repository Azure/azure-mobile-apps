// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Xunit;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class DatasyncClientData_Tests : BaseTest
    {
        private class TestClass : DatasyncClientData, IEquatable<TestClass>
        {
            bool IEquatable<TestClass>.Equals(TestClass other)
                => Id == other.Id && UpdatedAt == other.UpdatedAt && Version == other.Version && Deleted == other.Deleted;

            public override bool Equals(object obj) => obj is TestClass other && Equals(other);

            public override int GetHashCode() => Id.GetHashCode();
        }

        [Fact]
        public void DatasyncClientData_CanSerialize()
        {
            TestClass foo = new() { Id = "1234", UpdatedAt = new DateTimeOffset(2001, 12, 31, 08, 00, 00, TimeSpan.Zero), Version = "opaque", Deleted = true };
            const string expected = "{\"id\":\"1234\",\"deleted\":true,\"updatedAt\":\"2001-12-31T08:00:00.000Z\",\"version\":\"opaque\"}";
            var actual = JsonSerializer.Serialize(foo, ClientOptions.SerializerOptions);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DatasyncClientData_CanDeserialize()
        {
            const string json = "{\"id\":\"1234\",\"deleted\":true,\"updatedAt\":\"2001-12-31T08:00:00.000Z\",\"version\":\"opaque\"}";
            TestClass expected = new() { Id = "1234", UpdatedAt = new DateTimeOffset(2001, 12, 31, 08, 00, 00, TimeSpan.Zero), Version = "opaque", Deleted = true };
            var actual = JsonSerializer.Deserialize<TestClass>(json, ClientOptions.DeserializerOptions);
            Assert.Equal(expected, actual);
        }
    }
}

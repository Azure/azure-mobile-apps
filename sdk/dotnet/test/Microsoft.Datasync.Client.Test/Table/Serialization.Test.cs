// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table
{
    /// <summary>
    /// Testing JSON Serialization using the default serializer.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Serialization_Test : BaseTest
    {
        private class TestClass
        {
            public DateTime? ReleaseDate { get; set; }
            public DateTimeOffset? UpdatedAt { get; set; }
        }

        #region IsoDateTimeConverter
        [Fact]
        public void Serializer_DeserializesDate()
        {
            const string json = "{\"releaseDate\":\"2011-11-27T08:14:32.500Z\"}";
            var actual = JsonSerializer.Deserialize<TestClass>(json, ClientOptions.DeserializerOptions);
            var expected = new DateTime(2011, 11, 27, 8, 14, 32, 500, DateTimeKind.Utc).ToLocalTime();
            Assert.Equal(expected, actual.ReleaseDate);
        }

        [Fact]
        public void Serializer_SerializesDate()
        {
            var testClass = new TestClass
            {
                ReleaseDate = new DateTime(2011, 11, 27, 8, 14, 32, 500, DateTimeKind.Utc)
            };
            const string expected = "{\"releaseDate\":\"2011-11-27T08:14:32.500Z\"}";
            var actual = JsonSerializer.Serialize(testClass, ClientOptions.SerializerOptions);
            Assert.Equal(expected, actual);
        }
        #endregion

        #region IsoDateTimeOffsetConverter
        [Fact]
        public void Serializer_SerializesDTO()
        {
            var testClass = new TestClass
            {
                UpdatedAt = new DateTimeOffset(2011, 11, 27, 8, 14, 32, 500, TimeSpan.Zero)
            };
            const string expected = "{\"updatedAt\":\"2011-11-27T08:14:32.500Z\"}";
            var actual = JsonSerializer.Serialize(testClass, ClientOptions.SerializerOptions);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Serializer_DeserializesDTO()
        {
            const string json = "{\"updatedAt\":\"2011-11-27T08:14:32.500Z\"}";
            var actual = JsonSerializer.Deserialize<TestClass>(json, ClientOptions.DeserializerOptions);
            var expected = new DateTimeOffset(2011, 11, 27, 8, 14, 32, 500, TimeSpan.Zero);
            Assert.Equal(expected, actual.UpdatedAt);
        }
        #endregion
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
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

        [Fact]
        public void Serializer_DeserializesDate()
        {
            var options = new DatasyncClientOptions();
            const string json = "{\"releaseDate\":\"2011-11-27T08:14:32.500Z\"}";
            var actual = JsonSerializer.Deserialize<TestClass>(json, options.DeserializerOptions);
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
            var options = new DatasyncClientOptions();
            const string expected = "{\"releaseDate\":\"2011-11-27T08:14:32.500Z\"}";
            var actual = JsonSerializer.Serialize(testClass, options.SerializerOptions);
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void Serializer_SerializesDTO()
        {
            var testClass = new TestClass
            {
                UpdatedAt = new DateTimeOffset(2011, 11, 27, 8, 14, 32, 500, TimeSpan.Zero)
            };
            var options = new DatasyncClientOptions();
            const string expected = "{\"updatedAt\":\"2011-11-27T08:14:32.500Z\"}";
            var actual = JsonSerializer.Serialize(testClass, options.SerializerOptions);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Serializer_DeserializesDTO()
        {
            const string json = "{\"updatedAt\":\"2011-11-27T08:14:32.500Z\"}";
            var options = new DatasyncClientOptions();
            var actual = JsonSerializer.Deserialize<TestClass>(json, options.DeserializerOptions);
            var expected = new DateTimeOffset(2011, 11, 27, 8, 14, 32, 500, TimeSpan.Zero);
            Assert.Equal(expected, actual.UpdatedAt);
        }
    }
}

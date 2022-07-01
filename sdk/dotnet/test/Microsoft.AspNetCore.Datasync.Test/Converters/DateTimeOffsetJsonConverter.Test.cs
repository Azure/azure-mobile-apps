// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.AspNetCore.Datasync.Converters;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Datasync.Test.Converters
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class DateTimeOffsetJsonConverter_Tests
    {
        private readonly JsonSerializerSettings _settings;

        public DateTimeOffsetJsonConverter_Tests()
        {
            _settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            _settings.Converters.Add(new DateTimeOffsetJsonConverter());
        }

        [Fact]
        public void Converter_ReadsJson()
        {
            string json = "{\"updatedAt\":\"2021-08-21T12:30:15.123+00:00\"}";
            DateTimeOffset value = DateTimeOffset.Parse("2021-08-21T12:30:15.123+00:00");

            DTOIdEntity entity = JsonConvert.DeserializeObject<DTOIdEntity>(json, _settings);
            AssertEx.CloseTo(value, entity.UpdatedAt.Value);
        }

        [Fact]
        public void Converter_WritesJson()
        {
            string json = "{\"UpdatedAt\":\"2021-08-21T12:30:15.123+00:00\",\"Number\":0}";
            DTOIdEntity entity = new() { UpdatedAt = DateTimeOffset.Parse("2021-08-21T12:30:15.1234567+00:00") };
            string actual = JsonConvert.SerializeObject(entity, _settings);
            Assert.Equal(json, actual);
        }

        [Fact]
        public void Converter_WritesJson_WithTimeZone()
        {
            string json = "{\"UpdatedAt\":\"2021-08-21T12:30:15.123+00:00\",\"Number\":0}";
            DTOIdEntity entity = new() { UpdatedAt = DateTimeOffset.Parse("2021-08-21T20:30:15.1234567+08:00") };
            string actual = JsonConvert.SerializeObject(entity, _settings);
            Assert.Equal(json, actual);
        }

        [Fact]
        public void Converter_ThrowsOnBadDateInInput()
        {
            string json = "{\"updatedAt\":\"foo\"}";

            Assert.ThrowsAny<JsonException>(() => JsonConvert.DeserializeObject<DTOIdEntity>(json, _settings));
        }

        #region Models
        public class DTOIdEntity
        {
            public string Id { get; set; }
            public string Version { get; set; }
            public DateTimeOffset? UpdatedAt { get; set; }
            public long Number { get; set; }
        }
        #endregion
    }
}

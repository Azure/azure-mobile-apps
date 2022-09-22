// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Serialization
{
    [ExcludeFromCodeCoverage]
    public class ServiceSerializer_Tests : BaseTest
    {
        private readonly ServiceSerializer serializer;

        public ServiceSerializer_Tests()
        {
            serializer = new();
        }

        [Fact]
        public void Ctor_CreatesSerializerSettings()
        {
            Assert.NotNull(serializer.SerializerSettings);
        }

        [Fact]
        public void SerializerSettings_CanRoundtrip()
        {
            var settings = new DatasyncSerializerSettings();
            serializer.SerializerSettings = settings;
            Assert.Same(settings, serializer.SerializerSettings);
        }

        [Fact]
        public void EnsureIdIsStringId_Works_WithValidModel()
        {
            ServiceSerializer.EnsureIdIsString<IdEntity>();
        }

        [Fact]
        public void EnsureIdIsStringId_Throws_WithMissingIdColumn()
        {
            Assert.Throws<ArgumentException>(() => ServiceSerializer.EnsureIdIsString<BadEntityNoId>());
        }

        [Fact]
        public void EnsureIdIsStringId_Throws_WithNonStringIdColumn()
        {
            Assert.Throws<ArgumentException>(() => ServiceSerializer.EnsureIdIsString<BadEntityIntId>());
        }

        [Fact]
        public void GetId_ReturnsId()
        {
            JObject sut = JObject.Parse("{\"id\":\"1234\"}");
            Assert.Equal("1234", ServiceSerializer.GetId(sut));
        }

        [Fact]
        public void GetId_IgnoreCase_ReturnsId()
        {
            JObject sut = JObject.Parse("{\"Id\":\"1234\"}");
            Assert.Equal("1234", ServiceSerializer.GetId(sut, ignoreCase: true));
        }

        [Fact]
        public void GetId_IgnoreCase_Throws()
        {
            JObject sut = JObject.Parse("{\"iD\":\"1234\"}");
            Assert.Throws<ArgumentException>(() => ServiceSerializer.GetId(sut, ignoreCase: false));
        }

        [Fact]
        public void GetId_NoId_Throws()
        {
            JObject sut = JObject.Parse("{}");
            Assert.Throws<ArgumentException>(() => ServiceSerializer.GetId(sut));
        }

        [Fact]
        public void GetId_NoId_AllowDefault_Ok()
        {
            JObject sut = JObject.Parse("{}");
            Assert.Null(ServiceSerializer.GetId(sut, allowDefault: true));
        }

        [Fact]
        [Trait("Method", "GetUpdatedAt")]
        public void GetUpdatedAt_ReturnsNullWhenMissing()
        {
            JObject sut = JObject.Parse("{\"id\":\"1234\"}");
            Assert.Null(ServiceSerializer.GetUpdatedAt(sut));
        }

        [Fact]
        [Trait("Method", "GetUpdatedAt")]
        public void GetUpdatedAt_ReturnsTheDateWhenPresent()
        {
            JObject sut = JObject.Parse("{\"updatedAt\":\"2021-03-16T12:32:05.000+00:00\"}");
            DateTimeOffset expected = DateTimeOffset.Parse("2021-03-16T12:32:05.000+00:00");
            Assert.Equal(expected.ToUniversalTime(), ServiceSerializer.GetUpdatedAt(sut)?.ToUniversalTime());
        }

        [Fact]
        [Trait("Method", "IsDeleted")]
        public void IsDeleted_ReturnsFalseWhenMissing()
        {
            JObject sut = JObject.Parse("{\"id\":\"1234\"}");
            Assert.False(ServiceSerializer.IsDeleted(sut));
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [Trait("Method", "IsDeleted")]
        public void IsDeleted_ReturnsCorrectlyWhenPresent(string value, bool expected)
        {
            JObject sut = JObject.Parse($"{{\"deleted\":{value}}}");
            Assert.Equal(expected, ServiceSerializer.IsDeleted(sut));
        }

        [Fact]
        [Trait("Method", "SetIdToDefault")]
        public void SetIdToDefault_JObject_Works()
        {
            JObject sut = JObject.Parse("{\"id\":\"1234\"}");
            var serializer = new ServiceSerializer();
            serializer.SetIdToDefault(sut);
            Assert.Null(sut.Value<string>("id"));
        }
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Serialization
{
    [ExcludeFromCodeCoverage]
    public class ServiceSerializer_Tests : BaseTest
    {
        private ServiceSerializer serializer;

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
    }
}

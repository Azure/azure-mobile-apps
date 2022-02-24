// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Serialization
{
    [ExcludeFromCodeCoverage]
    public class ObjectReader_Tests : BaseTest
    {
        private const string format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK";

        [Theory, CombinatorialData]
        [Trait("Method", "GetSystemProperties<T>")]
        public void GetSystemProperties_JToken_Works(bool hasId, bool hasVersion, bool hasUpdatedAt)
        {
            string expectedId = Guid.NewGuid().ToString("N");
            string expectedVersion = Guid.NewGuid().ToString("N");
            DateTimeOffset expectedUpdatedAt = DateTimeOffset.UtcNow;

            JObject sut = new();
            if (hasId) sut.Add("id", expectedId);
            if (hasVersion) sut.Add("version", expectedVersion);
            if (hasUpdatedAt) sut.Add("updatedAt", expectedUpdatedAt.ToString(format));

            ObjectReader.GetSystemProperties(sut, out SystemProperties actual);
            Assert.NotNull(actual);
            if (hasId)
            {
                Assert.Equal(expectedId, actual.Id);
            }
            else
            {
                Assert.Null(actual.Id);
            }

            if (hasVersion)
            {
                Assert.Equal(expectedVersion, actual.Version);
            }
            else
            {
                Assert.Null(actual.Version);
            }

            if (hasUpdatedAt)
            {
                Assert.Equal(expectedUpdatedAt.ToString(format), actual.UpdatedAt?.ToString(format));
            }
            else
            {
                Assert.Null(actual.UpdatedAt);
            }
        }

        [Theory, CombinatorialData]
        [Trait("Method", "GetSystemProperties<T>")]
        public void GetSystemProperties_JToken_ThrowsOnInvalidType(bool hasId, bool hasVersion, bool hasUpdatedAt)
        {
            if (!hasId && !hasVersion && !hasUpdatedAt)
            {
                // If there is nothing, the error is not thrown.
                return;
            }
            const int testVal = 42;
            JObject sut = new();
            if (hasId) sut.Add("id", testVal);
            if (hasVersion) sut.Add("version", testVal);
            if (hasUpdatedAt) sut.Add("updatedAt", testVal);

            Assert.Throws<InvalidOperationException>(() => ObjectReader.GetSystemProperties(sut, out _));
        }

        [Theory, CombinatorialData]
        [Trait("Method", "GetSystemProperties<T>")]
        public void GetSystemProperties_Model_Works(bool hasId, bool hasVersion, bool hasUpdatedAt)
        {
            string expectedId = Guid.NewGuid().ToString("N");
            string expectedVersion = Guid.NewGuid().ToString("N");
            DateTimeOffset expectedUpdatedAt = DateTimeOffset.UtcNow;

            DTOIdEntity sut = new();
            if (hasId) sut.Id = expectedId;
            if (hasVersion) sut.Version = expectedVersion;
            if (hasUpdatedAt) sut.UpdatedAt = expectedUpdatedAt;

            ObjectReader.GetSystemProperties(sut, out SystemProperties actual);

            if (hasId)
            {
                Assert.Equal(expectedId, actual.Id);
            }
            else
            {
                Assert.Null(actual.Id);
            }

            if (hasVersion)
            {
                Assert.Equal(expectedVersion, actual.Version);
            }
            else
            {
                Assert.Null(actual.Version);
            }

            if (hasUpdatedAt)
            {
                Assert.Equal(expectedUpdatedAt, actual.UpdatedAt);
            }
            else
            {
                Assert.Null(actual.UpdatedAt);
            }
        }

        [Theory, CombinatorialData]
        [Trait("Method", "GetSystemProperties<T>")]
        public void GetSystemProperties_AltModel_Works(bool hasId, bool hasVersion, bool hasUpdatedAt)
        {
            string expectedId = Guid.NewGuid().ToString("N");
            byte[] expectedVersion = Guid.NewGuid().ToByteArray();
            string expectedUpdatedAt = DateTimeOffset.UtcNow.ToString(format);

            AltModel sut = new();
            if (hasId) sut.Id = expectedId;
            sut.VersionAsBytes = hasVersion ? expectedVersion : Array.Empty<byte>();
            if (hasUpdatedAt) sut.UpdatedAt = expectedUpdatedAt;

            ObjectReader.GetSystemProperties(sut, out SystemProperties actual);

            if (hasId)
            {
                Assert.Equal(expectedId, actual.Id);
            }
            else
            {
                Assert.Null(actual.Id);
            }

            if (hasVersion)
            {
                Assert.Equal(Encoding.UTF8.GetString(expectedVersion), actual.Version);
            }
            else
            {
                Assert.Null(actual.Version);
            }

            if (hasUpdatedAt)
            {
                Assert.Equal(expectedUpdatedAt, actual.UpdatedAt?.ToString(format));
            }
            else
            {
                Assert.Null(actual.UpdatedAt);
            }
        }

        [Theory, CombinatorialData]
        [Trait("Method", "GetSystemProperties<T>")]
        public void GetSystemProperties_JsonModel_Works(bool hasId, bool hasVersion, bool hasUpdatedAt)
        {
            string expectedId = Guid.NewGuid().ToString("N");
            string expectedVersion = Guid.NewGuid().ToString("N");
            DateTimeOffset expectedUpdatedAt = DateTimeOffset.UtcNow;

            JsonEntity sut = new();
            if (hasId) sut.JsonId = expectedId;
            if (hasVersion) sut.JsonVersion = expectedVersion;
            if (hasUpdatedAt) sut.JsonUpdatedAt = expectedUpdatedAt;

            ObjectReader.GetSystemProperties(sut, out SystemProperties actual);

            if (hasId)
            {
                Assert.Equal(expectedId, actual.Id);
            }
            else
            {
                Assert.Null(actual.Id);
            }

            if (hasVersion)
            {
                Assert.Equal(expectedVersion, actual.Version);
            }
            else
            {
                Assert.Null(actual.Version);
            }

            if (hasUpdatedAt)
            {
                Assert.Equal(expectedUpdatedAt, actual.UpdatedAt);
            }
            else
            {
                Assert.Null(actual.UpdatedAt);
            }
        }

        [Fact]
        [Trait("Method", "GetSystemProperties<T>")]
        public void GetSystemProperties_Model_ThrowsOnInvalidIdType()
        {
            ModelIntId sut = new();
            Assert.Throws<InvalidOperationException>(() => ObjectReader.GetSystemProperties(sut, out _));
        }

        [Fact]
        [Trait("Method", "GetSystemProperties<T>")]
        public void GetSystemProperties_Model_ThrowsOnInvalidVersionType()
        {
            ModelIntVersion sut = new();
            Assert.Throws<InvalidOperationException>(() => ObjectReader.GetSystemProperties(sut, out _));
        }

        [Fact]
        [Trait("Method", "GetSystemProperties<T>")]
        public void GetSystemProperties_Model_ThrowsOnInvalidUpdatedAtType()
        {
            ModelIntUpdatedAt sut = new();
            Assert.Throws<InvalidOperationException>(() => ObjectReader.GetSystemProperties(sut, out _));
        }

        #region Test Entities
        internal class JsonEntity
        {
            [JsonProperty("id")]
            public string JsonId { get; set; }

            [JsonProperty("version")]
            public string JsonVersion { get; set; }

            [JsonProperty("updatedAt")]
            public DateTimeOffset JsonUpdatedAt { get; set; }
        }

        internal class AltModel
        {
            public string Id { get; set; }

            [JsonProperty("version")]
            public byte[] VersionAsBytes { get; set; }

            public string UpdatedAt { get; set; }
        }

        internal class ModelIntId
        {
            public int Id { get; set; }
        }

        internal class ModelIntVersion
        {
            public int Version { get; set; }
        }

        internal class ModelIntUpdatedAt
        {
            public int UpdatedAt { get; set; }
        }
        #endregion
    }
}

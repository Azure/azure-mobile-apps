// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
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

        [Fact]
        [Trait("Method", "GetSystemProperties<T>")]
        public void GetSystemProperties_Model_ThrowsOnInvalidType()
        {
            NoIdEntity sut = new();
            Assert.Throws<InvalidOperationException>(() => ObjectReader.GetSystemProperties(sut, out _));
        }
    }
}

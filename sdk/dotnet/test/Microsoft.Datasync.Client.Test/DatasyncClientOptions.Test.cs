// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage]
    public class DatasyncClientOptions_Test
    {
        [Fact]
        [Trait("Method", "DeserializerOptions")]
        public void DeserializerOptions_IsSet()
        {
            var sut = new DatasyncClientOptions();
            Assert.NotNull(sut.DeserializerOptions);
            Assert.Equal(JsonNamingPolicy.CamelCase, sut.DeserializerOptions.PropertyNamingPolicy);
        }

        [Fact]
        [Trait("Method", "DeserializerOptions")]
        public void DeserializerOptions_Roundtrips()
        {
            var opts = new JsonSerializerOptions();
            var sut = new DatasyncClientOptions { DeserializerOptions = opts };
            Assert.Same(opts, sut.DeserializerOptions);
        }

        [Fact]
        [Trait("Method", "HttpPipeline")]
        public void HttpPipeline_IsEmpty()
        {
            var sut = new DatasyncClientOptions();
            Assert.Empty(sut.HttpPipeline);
        }

        [Fact]
        [Trait("Method", "HttpPipeline")]
        public void HttpPipeline_CanRoundTrip()
        {
            var testHandler = new MockDelegatingHandler();
            var sut = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { testHandler } };
            Assert.Single(sut.HttpPipeline);
            Assert.Same(testHandler, sut.HttpPipeline.First());
        }

        [Fact]
        [Trait("Method", "InstallationId")]
        public void InstallationId_IsNotEmpty()
        {
            var sut = new DatasyncClientOptions();
            Assert.False(string.IsNullOrWhiteSpace(sut.InstallationId));
        }

        [Fact]
        [Trait("Method", "InstallationId")]
        public void InstallationId_DoesNotChange()
        {
            var opt1 = new DatasyncClientOptions();
            var installationId = opt1.InstallationId;

            var opt2 = new DatasyncClientOptions();
            Assert.Equal(installationId, opt2.InstallationId);
        }

        [Fact]
        [Trait("Method", "InstallationId")]
        public void InstallationId_RoundTrips()
        {
            var sut = new DatasyncClientOptions { InstallationId = "foo" };
            Assert.Equal("foo", sut.InstallationId);
        }

        [Fact]
        [Trait("Method", "SerializerOptions")]
        public void SerializerOptions_IsSet()
        {
            var sut = new DatasyncClientOptions();
            Assert.NotNull(sut.SerializerOptions);
            Assert.Equal(JsonNamingPolicy.CamelCase, sut.SerializerOptions.PropertyNamingPolicy);
        }

        [Fact]
        [Trait("Method", "SerializerOptions")]
        public void SerializerOptions_Roundtrips()
        {
            var opts = new JsonSerializerOptions();
            var sut = new DatasyncClientOptions { SerializerOptions = opts };
            Assert.Same(opts, sut.SerializerOptions);
        }

        [Fact]
        [Trait("Method", "TablesPrefix")]
        public void TablesPrefix_IsSet()
        {
            var sut = new DatasyncClientOptions();
            Assert.NotNull(sut.TablesPrefix);
            Assert.Equal("/tables/", sut.TablesPrefix);
        }

        [Fact]
        [Trait("Method", "TablesPrefix")]
        public void TablesPrefix_Roundtrips()
        {
            var sut = new DatasyncClientOptions { TablesPrefix = "/api" };
            Assert.Equal("/api", sut.TablesPrefix);
        }

        [Fact]
        [Trait("Method", "UserAgent")]
        public void UserAgent_IsNotNull()
        {
            var sut = new DatasyncClientOptions();
            Assert.NotEmpty(sut.UserAgent);
            Assert.StartsWith("Datasync/", sut.UserAgent);
            Assert.Contains($" ({Platform.UserAgentDetails})", sut.UserAgent);
        }

        [Fact]
        [Trait("Method", "UserAgent")]
        public void UserAgent_CanRoundTrip()
        {
            var sut = new DatasyncClientOptions { UserAgent = "Test/1.0" };
            Assert.Equal("Test/1.0", sut.UserAgent);
        }
    }
}

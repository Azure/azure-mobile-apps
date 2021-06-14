// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class DatasyncClientOptions_Tests
    {
        #region Ctor
        [Fact]
        public void Ctor_ReasonableDefaults()
        {
            // Act
            var actual = new DatasyncClientOptions();

            // Assert
            Assert.Equal("tables", actual.DefaultTablesUri);
            Assert.Equal(JsonNamingPolicy.CamelCase, actual.DeserializerOptions.PropertyNamingPolicy);
            Assert.Empty(actual.HttpPipeline);
            Assert.Equal(JsonNamingPolicy.CamelCase, actual.SerializerOptions.PropertyNamingPolicy);
            Assert.Equal("3.0.0", actual.ProtocolVersion);
        }
        #endregion

        #region DefaultTablesUri
        [Fact]
        public void DefaultTablesUri_CanRoundTrip()
        {
            // Act
            var actual = new DatasyncClientOptions() { DefaultTablesUri = "api" };

            // Assert
            Assert.Equal("api", actual.DefaultTablesUri);
        }

        [Fact]
        public void DefaultTablesUri_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DatasyncClientOptions() { DefaultTablesUri = null });
        }
        #endregion

        #region DeserializerOptions
        [Fact]
        public void DeserializerOptions_CanRoundTrip()
        {
            // Arrange
            var options = new JsonSerializerOptions();

            // Act
            var actual = new DatasyncClientOptions() { DeserializerOptions = options };

            // Assert
            Assert.Same(options, actual.DeserializerOptions);
        }

        [Fact]
        public void DeserializerOptions_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DatasyncClientOptions() { DeserializerOptions = null });
        }
        #endregion

        #region HttpMessageHandlers
        [Fact]
        public void HttpPipeline_CanBeEmpty()
        {
            var actual = new DatasyncClientOptions() { HttpPipeline = Array.Empty<HttpMessageHandler>() };
            Assert.Empty(actual.HttpPipeline);
        }

        [Fact]
        public void HttpPipeline_DelegatingHandlers()
        {
            var pipeline = new HttpMessageHandler[] { new MockMessageHandler() };
            var actual = new DatasyncClientOptions() { HttpPipeline = pipeline };
            Assert.NotEmpty(actual.HttpPipeline);
            Assert.Same(pipeline[0], actual.HttpPipeline.First());
        }

        [Fact]
        public void HttpPipeline_EndsWithHttpClientHandler()
        {
            var pipeline = new HttpMessageHandler[] { new MockMessageHandler(), new HttpClientHandler() };
            var actual = new DatasyncClientOptions() { HttpPipeline = pipeline };
            Assert.NotEmpty(actual.HttpPipeline);
            Assert.Same(pipeline[0], actual.HttpPipeline.First());
            Assert.Same(pipeline[1], actual.HttpPipeline.Last());
        }

        [Fact]
        public void HttpMessageHandlers_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DatasyncClientOptions() { HttpPipeline = null });
        }
        #endregion

        #region SerializerOptions
        [Fact]
        public void SerializerOptions_CanRoundTrip()
        {
            // Arrange
            var options = new JsonSerializerOptions();

            // Act
            var actual = new DatasyncClientOptions() { SerializerOptions = options };

            // Assert
            Assert.Same(options, actual.SerializerOptions);
        }

        [Fact]
        public void SerializerOptions_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DatasyncClientOptions() { SerializerOptions = null });
        }
        #endregion
    }
}

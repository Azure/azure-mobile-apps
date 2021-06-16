// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class DatasyncClientOptions_Tests : BaseTest
    {
        #region Ctor
        [Fact]
        public void Ctor_ProvidesDefaults()
        {
            var options = new DatasyncClientOptions();

            Assert.Equal(JsonNamingPolicy.CamelCase, options.DeserializerOptions.PropertyNamingPolicy);
            Assert.Empty(options.HttpPipeline);
            Assert.Equal(JsonNamingPolicy.CamelCase, options.SerializerOptions.PropertyNamingPolicy);
            Assert.Equal(JsonIgnoreCondition.WhenWritingDefault, options.SerializerOptions.DefaultIgnoreCondition);
            Assert.Equal("tables", options.TablesUri);
        }
        #endregion

        #region DeserializerOptions
        [Fact]
        public void DeserializerOptions_Null_Throws()
        {
            var options = new DatasyncClientOptions();
            Assert.Throws<ArgumentNullException>(() => options.DeserializerOptions = null);
        }

        [Fact]
        public void DeserializerOptions_Roundtrips()
        {
            var options = new DatasyncClientOptions { DeserializerOptions = SerializerOptions };
            Assert.Same(SerializerOptions, options.DeserializerOptions);
        }
        #endregion

        #region HttpPipeline
        [Fact]
        public void HttpPipeline_Null_Throws()
        {
            var options = new DatasyncClientOptions();
            Assert.Throws<ArgumentNullException>(() => options.HttpPipeline = null);
        }

        [Fact]
        public void HttpPipeline_Roundtrips()
        {
            var handler = new HttpClientHandler();
            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { handler } };
            Assert.Single(options.HttpPipeline);
            Assert.Same(handler, options.HttpPipeline.First());
        }
        #endregion

        #region SerializerOptions
        [Fact]
        public void SerializerOptions_Null_Throws()
        {
            var options = new DatasyncClientOptions();
            Assert.Throws<ArgumentNullException>(() => options.SerializerOptions = null);
        }

        [Fact]
        public void SerializerOptions_Roundtrips()
        {
            var options = new DatasyncClientOptions { SerializerOptions = SerializerOptions };
            Assert.Same(SerializerOptions, options.SerializerOptions);
        }
        #endregion

        #region TablesUri
        [Fact]
        public void TablesUri_Null_Throws()
        {
            var options = new DatasyncClientOptions();
            Assert.Throws<ArgumentNullException>(() => options.TablesUri = null);
        }

        [Theory]
        [InlineData("api", "api")]
        [InlineData("/api", "api")]
        [InlineData("/api/", "api")]
        [InlineData("api/", "api")]
        public void TablesUri_RoundTrips(string sut, string expected)
        {
            var options = new DatasyncClientOptions { TablesUri = sut };
            Assert.Equal(expected, options.TablesUri);
        }
        #endregion
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage]
    public class HttpRequestMessage_Test : BaseTest
    {
        private class TestClass
        {
            public string Id { get; set; }
            public DateTimeOffset? UpdatedAt { get; set; }
            public long Number { get; set; }
        }

        #region WithHeader
        [Fact]
        [Trait("Method", "WithHeader")]
        public void WithHeader_AddsNormalHeader()
        {
            var sut = new HttpRequestMessage(HttpMethod.Post, "");
            var result = sut.WithHeader("X-ZUMO-TEST", "value");
            AssertEx.Contains("X-ZUMO-TEST", "value", result.Headers);
            Assert.Same(sut, result);
        }

        [Theory]
        [InlineData("X-ZUMO-TEST", null)]
        [InlineData("X-ZUMO-TEST", "")]
        [InlineData("X-ZUMO-TEST", "   ")]
        [InlineData(null, "X-ZUMO-TEST")]
        [InlineData("", "X-ZUMO-TEST")]
        [InlineData("    ", "X-ZUMO-TEST")]
        [Trait("Method", "WithHeader")]
        public void WithHeader_SkipsEmptyHeader(string name, string value)
        {
            var sut = new HttpRequestMessage(HttpMethod.Post, "");
            var expectedCount = sut.Headers.Count();
            var result = sut.WithHeader(name, value);
            Assert.Equal(expectedCount, result.Headers.Count());    // Nothing additional added
            Assert.False(result.Headers.Contains("X-ZUMO-TEST"));   // The specific header is not there
            Assert.Same(sut, result);
        }

        [Fact]
        [Trait("Method", "WithHeader")]
        public void WithHeader_Throws_SettingContentHeader()
        {
            var sut = new HttpRequestMessage(HttpMethod.Get, "");
            Assert.Throws<InvalidOperationException>(() => sut.WithHeader("Content-Type", "text/plain; encoding=utf-8"));
        }
        #endregion

        #region WithHeaders
        [Fact]
        [Trait("Method", "WithHeaders")]
        public void WithHeaders_AddsWholeDictionary()
        {
            var sut = new HttpRequestMessage(HttpMethod.Get, "");
            var dict = new Dictionary<string, string>()
            {
                { "X-ZUMO-FEATURES", "TT" },
                { "X-ZUMO-TEST", "value" }
            };
            var expectedCount = sut.Headers.Count();
            var result = sut.WithHeaders(dict);
            Assert.Equal(expectedCount + 2, result.Headers.Count());
            Assert.Same(sut, result);
            AssertEx.Contains("X-ZUMO-FEATURES", "TT", result.Headers);
            AssertEx.Contains("X-ZUMO-TEST", "value", result.Headers);
        }

        [Fact]
        [Trait("Method", "WithHeaders")]
        public void WithHeaders_AcceptsNull()
        {
            var sut = new HttpRequestMessage(HttpMethod.Get, "");
            var expectedCount = sut.Headers.Count();
            var result = sut.WithHeaders(null);
            Assert.Equal(expectedCount, result.Headers.Count());
            Assert.Same(sut, result);
        }

        [Fact]
        [Trait("Method", "WithHeaders")]
        public void WithHeaders_AcceptsEmpty()
        {
            var sut = new HttpRequestMessage(HttpMethod.Get, "");
            var dict = new Dictionary<string, string>();
            var expectedCount = sut.Headers.Count();
            var result = sut.WithHeaders(dict);
            Assert.Equal(expectedCount, result.Headers.Count());
            Assert.Same(sut, result);
        }

        [Fact]
        [Trait("Method", "WithHeaders")]
        public void WithHeaders_Throws_WithContentHeader()
        {
            var sut = new HttpRequestMessage(HttpMethod.Get, "");
            var dict = new Dictionary<string, string>()
            {
                { "X-ZUMO-FEATURES", "TT" },
                { "Content-Type", "text/plain; encoding=utf-8" }
            };
            Assert.Throws<InvalidOperationException>(() => sut.WithHeaders(dict));
        }
        #endregion

        #region WithFeatureHeader
        [Fact]
        [Trait("Method", "WithFeatureHeader")]
        public void WithFeatureHeader_None_NoAdditions()
        {
            var sut = new HttpRequestMessage(HttpMethod.Get, "");
            var expectedCount = sut.Headers.Count();
            var result = sut.WithFeatureHeader(DatasyncFeatures.None);
            Assert.Equal(expectedCount, result.Headers.Count());
            Assert.Same(sut, result);
            Assert.False(result.Headers.Contains("X-ZUMO-FEATURES"));
        }

        [Theory]
        [InlineData(DatasyncFeatures.TypedTable, "TT")]
        [InlineData(DatasyncFeatures.UntypedTable, "TU")]
        [InlineData(DatasyncFeatures.Offline, "OL")]
        [InlineData(DatasyncFeatures.TypedTable | DatasyncFeatures.Offline, "TT,OL")]
        [InlineData(DatasyncFeatures.UntypedTable | DatasyncFeatures.Offline, "TU,OL")]
        [Trait("Method", "WithFeatureHeader")]
        internal void WithFeatureHeader_Value_SetsHeader(DatasyncFeatures features, string expected)
        {
            var sut = new HttpRequestMessage(HttpMethod.Get, "");
            var result = sut.WithFeatureHeader(features);
            AssertEx.Contains("X-ZUMO-FEATURES", expected, result.Headers);
        }
        #endregion

        #region WithContent
        [Fact]
        [Trait("Method", "WithContent")]
        public void WithContent_SerializesContent()
        {
            var sut = new HttpRequestMessage(HttpMethod.Get, "");
            var testClass = new TestClass { Id = "1234", UpdatedAt = new DateTimeOffset(2011, 11, 27, 8, 14, 32, 500, TimeSpan.Zero), Number = 42L };
            const string expected = "{\"id\":\"1234\",\"updatedAt\":\"2011-11-27T08:14:32.500Z\",\"number\":42}";
            var result = sut.WithContent(testClass, ClientOptions.SerializerOptions);
            var actual = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(expected, actual);
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
            Assert.Equal("utf-8", result.Content.Headers.ContentType.CharSet);
            Assert.Same(sut, result);
        }

        [Fact]
        [Trait("Method", "WithContent")]
        public void WithContent_SerializesContent_WithMediaType()
        {
            var sut = new HttpRequestMessage(HttpMethod.Get, "");
            var testClass = new TestClass { Id = "1234", UpdatedAt = new DateTimeOffset(2011, 11, 27, 8, 14, 32, 500, TimeSpan.Zero), Number = 42L };
            const string expected = "{\"id\":\"1234\",\"updatedAt\":\"2011-11-27T08:14:32.500Z\",\"number\":42}";
            var result = sut.WithContent(testClass, ClientOptions.SerializerOptions, "application/json+test");
            var actual = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(expected, actual);
            Assert.Equal("application/json+test", result.Content.Headers.ContentType.MediaType);
            Assert.Equal("utf-8", result.Content.Headers.ContentType.CharSet);
            Assert.Same(sut, result);
        }

        [Fact]
        [Trait("Method", "WithContent")]
        public void WithContent_NullContent_Throws()
        {
            var sut = new HttpRequestMessage(HttpMethod.Get, "");
            TestClass testClass = null;
            Assert.Throws<ArgumentNullException>(() => sut.WithContent(testClass, ClientOptions.SerializerOptions));
        }

        [Fact]
        [Trait("Method", "WithContent")]
        public void WithContent_NullSerializerOptions_Throws()
        {
            var sut = new HttpRequestMessage(HttpMethod.Get, "");
            var testClass = new TestClass { Id = "1234", UpdatedAt = new DateTimeOffset(2011, 11, 27, 8, 14, 32, 500, TimeSpan.Zero), Number = 42L };
            Assert.Throws<ArgumentNullException>(() => sut.WithContent(testClass, null));
        }
        #endregion
    }
}

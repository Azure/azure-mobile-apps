// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class HttpCondition_Tests
    {
        #region IfExists / IfNotExists
        [Fact]
        [Trait("Method", "IfExists")]
        public void IfExists_GeneratesObject()
        {
            HttpCondition condition = HttpCondition.IfExists();

            Assert.NotNull(condition);
            Assert.Contains("(If-Match=*)", condition.ToString());
        }

        [Fact]
        [Trait("Method", "IfNotExists")]
        public void IfNotExists_GeneratesObject()
        {
            HttpCondition condition = HttpCondition.IfNotExists();

            Assert.NotNull(condition);
            Assert.Contains("(If-None-Match=*)", condition.ToString());
        }
        #endregion

        #region IfMatch(byte[])
        [Fact]
        [Trait("Method", "IfMatch(byte[])")]
        public void IfMatch_NullByteArray_Throws()
        {
            byte[] version = null;

            Assert.Throws<ArgumentNullException>(() => HttpCondition.IfMatch(version));
        }

        [Fact]
        [Trait("Method", "IfMatch(byte[])")]
        public void IfMatch_EmptyByteArray_Throws()
        {
            byte[] version = Array.Empty<byte>();

            Assert.Throws<ArgumentException>(() => HttpCondition.IfMatch(version));
        }

        [Fact]
        [Trait("Method", "IfMatch(byte[])")]
        public void IfMatch_FilledByteArray_GeneratedObject()
        {
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();

            HttpCondition condition = HttpCondition.IfMatch(version);

            Assert.NotNull(condition);
            Assert.Contains("(If-Match=\"6lo9BvUDHEOw7CYe4kkGUQ==\")", condition.ToString());
        }
        #endregion

        #region IfMatch(string)
        [Fact]
        [Trait("Method", "IfMatch(string)")]
        public void IfMatch_NullString_Throws()
        {
            const string version = null;

            Assert.Throws<ArgumentNullException>(() => HttpCondition.IfMatch(version));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("    ")]
        [Trait("Method", "IfMatch(string)")]
        public void IfMatch_EmptyString_Throws(string version)
        {
            Assert.Throws<ArgumentException>(() => HttpCondition.IfMatch(version));
        }

        [Theory]
        [InlineData("\"etag\"", "\"etag\"")]
        [InlineData("etag", "\"etag\"")]
        [InlineData("*", "*")]
        [Trait("Method", "IfMatch(string)")]
        public void IfMatch_FilledString_GeneratesObject(string version, string expected)
        {
            HttpCondition condition = HttpCondition.IfMatch(version);

            Assert.NotNull(condition);
            Assert.Contains($"(If-Match={expected})", condition.ToString());
        }
        #endregion

        #region IfNotMatch(byte[])
        [Fact]
        [Trait("Method", "IfNotMatch(byte[])")]
        public void IfNotMatch_NullByteArray_Throws()
        {
            byte[] version = null;

            Assert.Throws<ArgumentNullException>(() => HttpCondition.IfNotMatch(version));
        }

        [Fact]
        [Trait("Method", "IfNotMatch(byte[])")]
        public void IfNotMatch_EmptyByteArray_Throws()
        {
            byte[] version = Array.Empty<byte>();

            Assert.Throws<ArgumentException>(() => HttpCondition.IfNotMatch(version));
        }

        [Fact]
        [Trait("Method", "IfNotMatch(byte[])")]
        public void IfNotMatch_FilledByteArray_GeneratedObject()
        {
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();

            HttpCondition condition = HttpCondition.IfNotMatch(version);

            Assert.NotNull(condition);
            Assert.Contains("(If-None-Match=\"6lo9BvUDHEOw7CYe4kkGUQ==\")", condition.ToString());
        }
        #endregion

        #region IfNotMatch(string)
        [Fact]
        [Trait("Method", "IfNotMatch(string)")]
        public void IfNotMatch_NullString_Throws()
        {
            const string version = null;

            Assert.Throws<ArgumentNullException>(() => HttpCondition.IfNotMatch(version));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("    ")]
        [Trait("Method", "IfNotMatch(string)")]
        public void IfNotMatch_EmptyString_Throws(string version)
        {
            Assert.Throws<ArgumentException>(() => HttpCondition.IfNotMatch(version));
        }

        [Theory]
        [InlineData("\"etag\"", "\"etag\"")]
        [InlineData("etag", "\"etag\"")]
        [InlineData("*", "*")]
        [Trait("Method", "IfNotMatch(string)")]
        public void IfNotMatch_FilledString_GeneratesObject(string version, string expected)
        {
            HttpCondition condition = HttpCondition.IfNotMatch(version);

            Assert.NotNull(condition);
            Assert.Contains($"(If-None-Match={expected})", condition.ToString());
        }
        #endregion

        #region AddToHeaders
        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => HttpCondition.IfExists().AddToHeaders(null));
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_Exists_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            var condition = HttpCondition.IfExists();

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-Match", new[] { "*" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_NotExists_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            var condition = HttpCondition.IfNotExists();

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-None-Match", new[] { "*" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_MatchByte_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();
            var condition = HttpCondition.IfMatch(version);

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-Match", new[] { "\"6lo9BvUDHEOw7CYe4kkGUQ==\"" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_MatchString_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            const string version = "etag";
            var condition = HttpCondition.IfMatch(version);

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-Match", new[] { "\"etag\"" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_NotMatchByte_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();
            var condition = HttpCondition.IfNotMatch(version);

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-None-Match", new[] { "\"6lo9BvUDHEOw7CYe4kkGUQ==\"" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_NotMatchString_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            const string version = "etag";
            var condition = HttpCondition.IfNotMatch(version);

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-None-Match", new[] { "\"etag\"" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_Exists_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-Match", "\"replaceme\"");
            var condition = HttpCondition.IfExists();

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-Match", new[] { "*" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_NotExists_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-None-Match", "\"replaceme\"");
            var condition = HttpCondition.IfNotExists();

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-None-Match", new[] { "*" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_MatchByte_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-Match", "\"replaceme\"");
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();
            var condition = HttpCondition.IfMatch(version);

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-Match", new[] { "\"6lo9BvUDHEOw7CYe4kkGUQ==\"" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_MatchString_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-Match", "\"replaceme\"");
            const string version = "etag";
            var condition = HttpCondition.IfMatch(version);

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-Match", new[] { "\"etag\"" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_NotMatchByte_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-None-Match", "\"replaceme\"");
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();
            var condition = HttpCondition.IfNotMatch(version);

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-None-Match", new[] { "\"6lo9BvUDHEOw7CYe4kkGUQ==\"" }, request.Headers);
        }

        [Fact]
        [Trait("Method", "AddToHeaders")]
        public void AddToHeaders_NotMatchString_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-None-Match", "\"replaceme\"");
            const string version = "etag";
            var condition = HttpCondition.IfNotMatch(version);

            condition.AddToHeaders(request.Headers);

            AssertEx.HasValue("If-None-Match", new[] { "\"etag\"" }, request.Headers);
        }
        #endregion
    }
}

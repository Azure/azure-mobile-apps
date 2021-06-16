// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Extensions
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class HttpRequestMessage_Tests : BaseTest
    {
        #region WithJsonPayload
        [Fact]
        public void WithJsonPayload_NullPayload_Throws()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            MockObject payload = null;

            // Act
            Assert.Throws<ArgumentNullException>(() => request.WithJsonPayload(payload, SerializerOptions));
        }

        [Fact]
        public void WithJsonPayload_NullOptions_Throws()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            MockObject payload = new() { StringValue = "test" };

            // Act
            Assert.Throws<ArgumentNullException>(() => request.WithJsonPayload(payload, null));
        }

        [Fact]
        public void WithJsonPayload_NullContentType_Throws()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            MockObject payload = new() { StringValue = "test" };

            // Act
            Assert.Throws<ArgumentNullException>(() => request.WithJsonPayload(payload, null, SerializerOptions));
        }

        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        public void WithJsonPayload_EmptyContentType_Throws(string contentType)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            MockObject payload = new() { StringValue = "test" };

            // Act
            Assert.Throws<ArgumentException>(() => request.WithJsonPayload(payload, contentType, SerializerOptions));
        }

        [Fact]
        public void WithJsonPayload_SerializesData()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            MockObject payload = new() { StringValue = "test" };
            const string expected = "{\"stringValue\":\"test\"}";

            var actual = request.WithJsonPayload(payload, SerializerOptions);

            // Assert
            Assert.Same(request, actual);
            Assert.Equal(expected, actual.Content.ReadAsStringAsync().Result);
            Assert.Equal("application/json; charset=utf-8", actual.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public void WithJsonPayload_SetsMediaType()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            MockObject payload = new() { StringValue = "test" };
            const string expected = "{\"stringValue\":\"test\"}";

            var actual = request.WithJsonPayload(payload, "application/merge-patch+json", SerializerOptions);

            // Assert
            Assert.Same(request, actual);
            Assert.Equal(expected, actual.Content.ReadAsStringAsync().Result);
            Assert.Equal("application/merge-patch+json; charset=utf-8", actual.Content.Headers.ContentType.ToString());
        }
        #endregion

        #region WithPrecondition
        [Fact]
        public void WithPrecondition_Null_DoesntChangeHeaders()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-Match", "\"replaceme\"");
            request.Headers.Add("If-None-Match", "\"replaceme\"");
            request.Headers.Add("X-TestHeader", "test");

            var actual = request.WithPrecondition(null);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-Match", new[] { "\"replaceme\"" }, actual.Headers);
            AssertEx.HasValue("If-None-Match", new[] { "\"replaceme\"" }, actual.Headers);
            AssertEx.HasValue("X-TestHeader", new[] { "test" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_Exists_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            var condition = HttpCondition.IfExists();

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-Match", new[] { "*" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_NotExists_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            var condition = HttpCondition.IfNotExists();

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-None-Match", new[] { "*" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_MatchByte_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();
            var condition = HttpCondition.IfMatch(version);

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-Match", new[] { "\"6lo9BvUDHEOw7CYe4kkGUQ==\"" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_MatchString_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            const string version = "etag";
            var condition = HttpCondition.IfMatch(version);

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-Match", new[] { "\"etag\"" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_NotMatchByte_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();
            var condition = HttpCondition.IfNotMatch(version);

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-None-Match", new[] { "\"6lo9BvUDHEOw7CYe4kkGUQ==\"" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_NotMatchString_AddsHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            const string version = "etag";
            var condition = HttpCondition.IfNotMatch(version);

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-None-Match", new[] { "\"etag\"" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_Exists_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-Match", "\"replaceme\"");
            var condition = HttpCondition.IfExists();

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-Match", new[] { "*" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_NotExists_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-None-Match", "\"replaceme\"");
            var condition = HttpCondition.IfNotExists();

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-None-Match", new[] { "*" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_MatchByte_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-Match", "\"replaceme\"");
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();
            var condition = HttpCondition.IfMatch(version);

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-Match", new[] { "\"6lo9BvUDHEOw7CYe4kkGUQ==\"" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_MatchString_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-Match", "\"replaceme\"");
            const string version = "etag";
            var condition = HttpCondition.IfMatch(version);

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-Match", new[] { "\"etag\"" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_NotMatchByte_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-None-Match", "\"replaceme\"");
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();
            var condition = HttpCondition.IfNotMatch(version);

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-None-Match", new[] { "\"6lo9BvUDHEOw7CYe4kkGUQ==\"" }, actual.Headers);
        }

        [Fact]
        public void WithPrecondition_NotMatchString_ReplacesHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost"));
            request.Headers.Add("If-None-Match", "\"replaceme\"");
            const string version = "etag";
            var condition = HttpCondition.IfNotMatch(version);

            var actual = request.WithPrecondition(condition);

            Assert.Same(request, actual);
            AssertEx.HasValue("If-None-Match", new[] { "\"etag\"" }, actual.Headers);
        }
        #endregion

        #region WithQueryString
        [Theory]
        [InlineData("http://localhost", null, "http://localhost/")]
        [InlineData("http://localhost", "", "http://localhost/")]
        [InlineData("http://localhost", "  ", "http://localhost/")]
        [InlineData("http://localhost", "a=b&c=d", "http://localhost/?a=b&c=d")]
        [InlineData("http://localhost", "  a=b&c=d", "http://localhost/?a=b&c=d")]
        [InlineData("http://localhost", "a=b&c=d  ", "http://localhost/?a=b&c=d")]
        [InlineData("http://localhost", "?a=b&c=d", "http://localhost/?a=b&c=d")]
        [InlineData("http://localhost?__filter=foo", null, "http://localhost/")]
        [InlineData("http://localhost?__filter=foo", "", "http://localhost/")]
        [InlineData("http://localhost?__filter=foo", "  ", "http://localhost/")]
        [InlineData("http://localhost?__filter=foo", "a=b&c=d", "http://localhost/?a=b&c=d")]
        [InlineData("http://localhost?__filter=foo", "  a=b&c=d", "http://localhost/?a=b&c=d")]
        [InlineData("http://localhost?__filter=foo", "a=b&c=d  ", "http://localhost/?a=b&c=d")]
        [InlineData("http://localhost#fragment", null, "http://localhost/#fragment")]
        [InlineData("http://localhost#fragment", "", "http://localhost/#fragment")]
        [InlineData("http://localhost#fragment", "  ", "http://localhost/#fragment")]
        [InlineData("http://localhost#fragment", "a=b&c=d", "http://localhost/?a=b&c=d#fragment")]
        [InlineData("http://localhost#fragment", "  a=b&c=d", "http://localhost/?a=b&c=d#fragment")]
        [InlineData("http://localhost#fragment", "a=b&c=d  ", "http://localhost/?a=b&c=d#fragment")]
        public void WithQueryString_Works(string sut, string query, string expected)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(sut));
            var actual = request.WithQueryString(query);
            Assert.Equal(expected, actual.RequestUri.ToString());
        }
        #endregion
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage]
    public class Precondition_Test : BaseTest
    {
        [Fact]
        [Trait("Method", "IfMatch.Any")]
        public void IfExists_GeneratesObject()
        {
            IfMatch condition = IfMatch.Any();
            Assert.NotNull(condition);
            Assert.Contains("(If-Match=*)", condition.ToString());
        }

        [Fact]
        [Trait("Method", "IfNoneMatch.Any")]
        public void IfNotExists_GeneratesObject()
        {
            IfNoneMatch condition = IfNoneMatch.Any();
            Assert.NotNull(condition);
            Assert.Contains("(If-None-Match=*)", condition.ToString());
        }

        [Fact]
        [Trait("Method", "IfMatch.Version")]
        public void IfMatch_NullByteArray_Throws()
        {
            byte[] version = null;
            Assert.Throws<ArgumentNullException>(() => IfMatch.Version(version));
        }

        [Fact]
        [Trait("Method", "IfMatch.Version")]
        public void IfMatch_EmptyByteArray_Throws()
        {
            byte[] version = Array.Empty<byte>();
            Assert.Throws<ArgumentException>(() => IfMatch.Version(version));
        }

        [Fact]
        [Trait("Method", "IfMatch.Version")]
        public void IfMatch_FilledByteArray_GeneratedObject()
        {
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();
            var condition = IfMatch.Version(version);
            Assert.NotNull(condition);
            Assert.Contains("(If-Match=\"6lo9BvUDHEOw7CYe4kkGUQ==\")", condition.ToString());
        }

        [Fact]
        [Trait("Method", "IfMatch.Version")]
        public void IfMatch_NullString_Throws()
        {
            const string version = null;
            Assert.Throws<ArgumentNullException>(() => IfMatch.Version(version));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("    ")]
        [Trait("Method", "IfMatch.Version")]
        public void IfMatch_EmptyString_Throws(string version)
        {
            Assert.Throws<ArgumentException>(() => IfMatch.Version(version));
        }

        [Theory]
        [InlineData("\"etag\"", "\"etag\"")]
        [InlineData("etag", "\"etag\"")]
        [InlineData("*", "*")]
        [Trait("Method", "IfMatch.Version")]
        public void IfMatch_FilledString_GeneratesObject(string version, string expected)
        {
            var condition = IfMatch.Version(version);
            Assert.NotNull(condition);
            Assert.Contains($"(If-Match={expected})", condition.ToString());
        }

        [Fact]
        [Trait("Method", "IfNoneMatch.Version")]
        public void IfNotMatch_NullByteArray_Throws()
        {
            byte[] version = null;
            Assert.Throws<ArgumentNullException>(() => IfNoneMatch.Version(version));
        }

        [Fact]
        [Trait("Method", "IfNoneMatch.Version")]
        public void IfNotMatch_EmptyByteArray_Throws()
        {
            byte[] version = Array.Empty<byte>();
            Assert.Throws<ArgumentException>(() => IfNoneMatch.Version(version));
        }

        [Fact]
        [Trait("Method", "IfNoneMatch.Version")]
        public void IfNotMatch_FilledByteArray_GeneratedObject()
        {
            byte[] version = Guid.Parse("063d5aea-03f5-431c-b0ec-261ee2490651").ToByteArray();
            var condition = IfNoneMatch.Version(version);
            Assert.NotNull(condition);
            Assert.Contains("(If-None-Match=\"6lo9BvUDHEOw7CYe4kkGUQ==\")", condition.ToString());
        }

        [Fact]
        [Trait("Method", "IfNoneMatch.Version")]
        public void IfNotMatch_NullString_Throws()
        {
            const string version = null;
            Assert.Throws<ArgumentNullException>(() => IfNoneMatch.Version(version));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("    ")]
        [Trait("Method", "IfNoneMatch.Version")]
        public void IfNotMatch_EmptyString_Throws(string version)
        {
            Assert.Throws<ArgumentException>(() => IfNoneMatch.Version(version));
        }

        [Theory]
        [InlineData("\"etag\"", "\"etag\"")]
        [InlineData("etag", "\"etag\"")]
        [InlineData("*", "*")]
        [Trait("Method", "IfNoneMatch.Version")]
        public void IfNotMatch_FilledString_GeneratesObject(string version, string expected)
        {
            var condition = IfNoneMatch.Version(version);
            Assert.NotNull(condition);
            Assert.Contains($"(If-None-Match={expected})", condition.ToString());
        }
    }
}

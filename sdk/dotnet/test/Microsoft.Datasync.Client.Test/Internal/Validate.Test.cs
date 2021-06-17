// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Internal;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Internal
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Validate_Tests
    {
        #region IsNotNull(object,string)
        [Fact]
        public void IsNotNull_Null_Throws()
        {
            object sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsNotNull(sut, nameof(sut)));
        }

        [Fact]
        public void IsNotNull_NotNull_Passes()
        {
            object sut = new();
            Validate.IsNotNull(sut, nameof(sut));
            Assert.NotNull(sut);
        }
        #endregion

        #region IsNotNullOrEmpty(byte[],string)
        [Fact]
        public void IsNotNullOrEmpty_Byte_Null_Throws()
        {
            byte[] sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsNotNullOrEmpty(sut, nameof(sut)));
        }

        [Fact]
        public void IsNotNullOrEmpty_Byte_Empty_Throws()
        {
            byte[] sut = Array.Empty<byte>();
            Assert.Throws<ArgumentException>(() => Validate.IsNotNullOrEmpty(sut, nameof(sut)));
        }

        [Fact]
        public void IsNotNullOrEmpty_Byte_Filled_Passes()
        {
            byte[] sut = Guid.NewGuid().ToByteArray();
            Validate.IsNotNullOrEmpty(sut, nameof(sut));
            Assert.NotEmpty(sut);
        }
        #endregion

        #region IsNotNullOrEmpty(string,string)
        [Fact]
        public void IsNotNullOrEmpty_String_Null_Throws()
        {
            string sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsNotNullOrEmpty(sut, nameof(sut)));
        }

        [Fact]
        public void IsNotNullOrEmpty_String_Empty_Throws()
        {
            string sut = "";
            Assert.Throws<ArgumentException>(() => Validate.IsNotNullOrEmpty(sut, nameof(sut)));
        }

        [Fact]
        public void IsNotNullOrEmpty_String_Filled_Passes()
        {
            string sut = Guid.NewGuid().ToString();
            Validate.IsNotNullOrEmpty(sut, nameof(sut));
            Assert.NotEmpty(sut);
        }
        #endregion

        #region IsNotNullOrWhitespace(string,string)
        [Fact]
        public void IsNotNullOrWhitespace_String_Null_Throws()
        {
            string sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsNotNullOrWhitespace(sut, nameof(sut)));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("    ")]
        public void IsNotNullOrWhitespace_String_Empty_Throws(string sut)
        {
            Assert.Throws<ArgumentException>(() => Validate.IsNotNullOrWhitespace(sut, nameof(sut)));
        }

        [Fact]
        public void IsNotNullOrWhitespace_String_Filled_Passes()
        {
            string sut = Guid.NewGuid().ToString();
            Validate.IsNotNullOrWhitespace(sut, nameof(sut));
            Assert.NotEmpty(sut);
        }
        #endregion

        #region IsPreconditionheader(string,string)
        [Fact]
        public void IsPreconditionHeader_Null_Throws()
        {
            string sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsPreconditionHeader(sut, nameof(sut)));
        }

        [Theory]
        [InlineData("ETag")]
        [InlineData("Host")]
        [InlineData("If-Modified-Since")]
        [InlineData("If-Unmodified-Since")]
        [InlineData("ZUMO-API-VERSION")]
        public void IsPreconditionHeader_InvalidHeader_Throws(string sut)
        {
            Assert.Throws<ArgumentException>(() => Validate.IsPreconditionHeader(sut, nameof(sut)));
        }

        [Theory]
        [InlineData("if-match")]
        [InlineData("If-Match")]
        [InlineData("if-none-match")]
        [InlineData("If-None-Match")]
        public void IsPreconditionHeader_String_Filled_Passes(string sut)
        {
            Validate.IsPreconditionHeader(sut, nameof(sut));
            Assert.NotEmpty(sut);
        }
        #endregion

        #region IsValidEndpoint(Uri,string)
        [Fact]
        public void IsValidEndpoint_Null_Throws()
        {
            Uri sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsValidEndpoint(sut, nameof(sut)));
        }

        [Fact]
        public void IsValidEndpoint_RelativeUri_Throws()
        {
            Uri sut = new("a/b", UriKind.Relative);
            Assert.Throws<UriFormatException>(() => Validate.IsValidEndpoint(sut, nameof(sut)));
        }

        [Theory]
        [InlineData("file://localhost/foo")]
        [InlineData("http://foo.azurewebsites.net")]
        public void IsValidEndpoint_InvalidUri_Throws(string endpoint)
        {
            var sut = new Uri(endpoint);
            Assert.Throws<UriFormatException>(() => Validate.IsValidEndpoint(sut, nameof(sut)));
        }

        [Theory]
        [InlineData("http://localhost")]
        [InlineData("http://127.0.0.1")]
        [InlineData("https://foo.azurewebsites.net")]
        [InlineData("http://localhost/tables")]
        [InlineData("http://127.0.0.1/tables")]
        [InlineData("https://foo.azurewebsites.net/tables")]
        public void IsValidEndpoint_Passes(string endpoint)
        {
            var sut = new Uri(endpoint);
            Validate.IsValidEndpoint(sut, nameof(sut));
        }
        #endregion
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.TestData;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Utils
{
    [ExcludeFromCodeCoverage]
    public class Validate_Tests
    {
        [Fact]
        [Trait("Method", "IsNotNull(object,string)")]
        public void IsNotNull_Null_Throws()
        {
            object sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsNotNull(sut, nameof(sut)));
        }

        [Fact]
        [Trait("Method", "IsNotNull(object,string)")]
        public void IsNotNull_NotNull_Passes()
        {
            object sut = new();
            Validate.IsNotNull(sut, nameof(sut));
        }

        [Fact]
        [Trait("Method", "IsNotNullOrEmpty")]
        public void IsNotNullOrEmpty_Byte_Null_Throws()
        {
            byte[] sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsNotNullOrEmpty(sut, nameof(sut)));
        }

        [Fact]
        [Trait("Method", "IsNotNullOrEmpty")]
        public void IsNotNullOrEmpty_Byte_Empty_Throws()
        {
            byte[] sut = Array.Empty<byte>();
            Assert.Throws<ArgumentException>(() => Validate.IsNotNullOrEmpty(sut, nameof(sut)));
        }

        [Fact]
        [Trait("Method", "IsNotNullOrEmpty")]
        public void IsNotNullOrEmpty_Byte_Filled_Passes()
        {
            byte[] sut = Guid.NewGuid().ToByteArray();
            Validate.IsNotNullOrEmpty(sut, nameof(sut));
            Assert.NotEmpty(sut);
        }

        [Fact]
        [Trait("Method", "IsNotNullOrEmpty")]
        public void IsNotNullOrEmpty_String_Null_Throws()
        {
            const string sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsNotNullOrEmpty(sut, nameof(sut)));
        }

        [Fact]
        [Trait("Method", "IsNotNullOrEmpty")]
        public void IsNotNullOrEmpty_String_Empty_Throws()
        {
            const string sut = "";
            Assert.Throws<ArgumentException>(() => Validate.IsNotNullOrEmpty(sut, nameof(sut)));
        }

        [Fact]
        [Trait("Method", "IsNotNullOrEmpty")]
        public void IsNotNullOrEmpty_String_Filled_Passes()
        {
            string sut = Guid.NewGuid().ToString();
            Validate.IsNotNullOrEmpty(sut, nameof(sut));
            Assert.NotEmpty(sut);
        }

        [Fact]
        [Trait("Method", "IsNotNullOrEmpty")]
        public void IsNotNullOrEmpty_Dictionary_Null_Throws()
        {
            Dictionary<string, string> sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsNotNullOrEmpty(sut, nameof(sut)));
        }

        [Fact]
        [Trait("Method", "IsNotNullOrEmpty")]
        public void IsNotNullOrEmpty_Dictionary_Empty_Throws()
        {
            Dictionary<string, string> sut = new();
            Assert.Throws<ArgumentException>(() => Validate.IsNotNullOrEmpty(sut, nameof(sut)));
        }

        [Fact]
        [Trait("Method", "IsNotNullOrEmpty")]
        public void IsNotNullOrEmpty_Dictionary_Filled_Passes()
        {
            Dictionary<string, string> sut = new() { { "test", "test" } };
            Validate.IsNotNullOrEmpty(sut, nameof(sut));
            Assert.NotEmpty(sut);
        }

        [Fact]
        [Trait("Method", "IsNotNullOrWhitespace")]
        public void IsNotNullOrWhitespace_String_Null_Throws()
        {
            const string sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsNotNullOrWhitespace(sut, nameof(sut)));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("    ")]
        [Trait("Method", "IsNotNullOrWhitespace")]
        public void IsNotNullOrWhitespace_String_Empty_Throws(string sut)
        {
            Assert.Throws<ArgumentException>(() => Validate.IsNotNullOrWhitespace(sut, nameof(sut)));
        }

        [Fact]
        [Trait("Method", "IsNotNullOrWhitespace")]
        public void IsNotNullOrWhitespace_String_Filled_Passes()
        {
            string sut = Guid.NewGuid().ToString();
            Validate.IsNotNullOrWhitespace(sut, nameof(sut));
            Assert.NotEmpty(sut);
        }

        [Fact]
        public void IsRelativeUri_Null_Throws()
        {
            const string sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsRelativeUri(sut, nameof(sut)));
        }

        [Theory]
        [InlineData("\"")]
        [InlineData("<")]
        [InlineData(">")]
        [InlineData("\\")]
        [InlineData("^")]
        [InlineData("`")]
        [InlineData("{")]
        [InlineData("|")]
        [InlineData("}")]
        [Trait("Method", "IsRelativeUri")]
        public void IsRelativeUri_Invalid_Throws(string sut)
        {
            Assert.Throws<ArgumentException>(() => Validate.IsRelativeUri(sut, nameof(sut)));
        }

        [Theory]
        [InlineData("tables/foo")]
        [InlineData("/tables/foo")]
        public void IsRelativeUri_Valid_Passes(string sut)
        {
            Validate.IsRelativeUri(sut, nameof(sut));
        }

        [Fact]
        [Trait("Method", "IsValidEndpoint(Uri,string)")]
        public void IsValidEndpoint_Null_Throws()
        {
            Uri sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsValidEndpoint(sut, nameof(sut)));
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("", true)]
        [InlineData("http://", false)]
        [InlineData("http://", true)]
        [InlineData("file://localhost/foo", false)]
        [InlineData("http://foo.azurewebsites.net", false)]
        [InlineData("http://foo.azure-api.net", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000/myapi", false)]
        [InlineData("http://10.0.0.8", false)]
        [InlineData("http://10.0.0.8:3000", false)]
        [InlineData("http://10.0.0.8:3000/myapi", false)]
        [InlineData("foo/bar", true)]
        [Trait("Method", "IsValidEndpoint(Uri,string)")]
        public void IsValidEndpoint_Invalid_Throws(string endpoint, bool isRelative = false)
        {
            Assert.Throws<UriFormatException>(() => Validate.IsValidEndpoint(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint), "sut"));
        }

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "IsValidEndpoint(Uri,string)")]
        public void IsValidEndpoint_Valid_Passes(EndpointTestCase testcase)
        {
            Uri sut = new(testcase.BaseEndpoint);
            Validate.IsValidEndpoint(sut, nameof(sut));
        }

        [Fact]
        [Trait("Method", "IsValidId")]
        public void IsValidId_Null_Throws()
        {
            const string sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsValidId(sut, nameof(sut)));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("abcdef gh")]
        [InlineData("!!!")]
        [InlineData("?")]
        [InlineData(";")]
        [InlineData("{EA235ADF-9F38-44EA-8DA4-EF3D24755767}")]
        [InlineData("###")]
        [Trait("Method", "IsValidId")]
        public void IsValidId_InvalidId_Throws(string sut)
        {
            Assert.Throws<ArgumentException>(() => Validate.IsValidId(sut, nameof(sut)));
        }

        [Theory]
        [InlineData("db0ec08d-46a9-465d-9f5e-0066a3ee5b5f")]
        [InlineData("0123456789")]
        [InlineData("abcdefgh")]
        [InlineData("db0ec08d_46a9_465d_9f5e_0066a3ee5b5f")]
        [InlineData("db0ec08d.46a9.465d.9f5e.0066a3ee5b5f")]
        [Trait("Method", "IsValidId")]
        public void IsValidId_Passes(string sut)
        {
            Validate.IsValidId(sut, nameof(sut));
        }
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.TestData;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Xunit;

#pragma warning disable RCS1196 // Call extension method as instance method.

namespace Microsoft.Datasync.Client.Test.Utils
{
    [ExcludeFromCodeCoverage]
    public class StdLibExtensions_Test
    {
        [Fact]
        [Trait("Method", "NormalizeEndpoint(Uri)")]
        public void NormalizeEndpoint_Null_Throws()
        {
            Uri sut = null;
            Assert.Throws<ArgumentNullException>(() => StdLibExtensions.NormalizeEndpoint(sut));
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
        [Trait("Method", "NormalizeEndpoint(Uri)")]
        public void NormalizeEndpoint_Invalid_Throws(string endpoint, bool isRelative = false)
        {
            Assert.Throws<UriFormatException>(() => StdLibExtensions.NormalizeEndpoint(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint)));
        }

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "NormalizeEndpoint(Uri)")]
        public void NormalizeEndpoint_Valid_Passes(EndpointTestCase testcase)
        {
            Uri sut = new(testcase.BaseEndpoint);
            Assert.Equal(testcase.NormalizedEndpoint, sut.NormalizeEndpoint().ToString());
        }

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
        [Trait("Method", "WithQuery(string)")]
        public void WithQuery_Works(string sut, string query, string expected)
        {
            var builder = new UriBuilder(sut);
            var actual = builder.WithQuery(query);
            Assert.Same(builder, actual);
            Assert.Equal(expected, actual.Uri.ToString());
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("[]", null)]
        [InlineData("1234", null)]
        [InlineData("{}", null)]
        [InlineData("{\"version\":\"1234\"}", null)]
        [InlineData("{\"Id\":\"1234\"}", null)]
        [InlineData("{\"id\":\"1234\"}", "1234")]
        [InlineData("{\"version\":\"1234\",\"id\":\"5678\"}", "5678")]
        [InlineData("{\"id\":\"1234\",\"version\":\"5678\"}", "1234")]
        public void GetId_Works(string sut, string expected)
        {
            JsonDocument document = sut == null ? null : JsonDocument.Parse(sut);
            Assert.Equal(expected, document.GetId());
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("[]", null)]
        [InlineData("1234", null)]
        [InlineData("{}", null)]
        [InlineData("{\"version\":\"1234\"}", "1234")]
        [InlineData("{\"Version\":\"1234\"}", null)]
        [InlineData("{\"id\":\"1234\"}", null)]
        [InlineData("{\"version\":\"1234\",\"id\":\"5678\"}", "1234")]
        [InlineData("{\"id\":\"1234\",\"version\":\"5678\"}", "5678")]
        public void GetVersion_Works(string sut, string expected)
        {
            JsonDocument document = sut == null ? null : JsonDocument.Parse(sut);
            Assert.Equal(expected, document.GetVersion());
        }
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.TestData;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
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
    }
}

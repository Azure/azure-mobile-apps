// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

#pragma warning disable RCS1196 // Call extension method as instance method.

namespace Microsoft.Datasync.Client.Test.Utils
{
    [ExcludeFromCodeCoverage]
    public class StdLibExtensions_Test : OldBaseTest
    {
        #region NormalizeEndpoint(Uri)
        [Fact]
        [Trait("Method", "NormalizeEndpoint(Uri)")]
        public void NormalizeEndpoint_Null_Throws()
        {
            Uri sut = null;
            Assert.Throws<ArgumentNullException>(() => StdLibExtensions.NormalizeEndpoint(sut));
        }

        [Theory, ClassData(typeof(TestCases.Invalid_Endpoints))]
        [Trait("Method", "NormalizeEndpoint(Uri)")]
        public void NormalizeEndpoint_Invalid_Throws(string endpoint, bool isRelative = false)
        {
            Assert.Throws<UriFormatException>(() => StdLibExtensions.NormalizeEndpoint(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint)));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "NormalizeEndpoint(Uri)")]
        public void NormalizeEndpoint_Valid_Passes(string endpoint, string normalizedEndpoint)
        {
            Uri sut = new(endpoint);
            Assert.Equal(normalizedEndpoint, sut.NormalizeEndpoint().ToString());
        }
        #endregion

        #region WithQuery(string)
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
        #endregion
    }
}

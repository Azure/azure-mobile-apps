// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Extensions
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class UriBuilder_Tests
    {
        #region Normalized
        [Theory]
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://localhost/tables", "http://localhost/tables/")]
        [InlineData("http://localhost/tables/", "http://localhost/tables/")]
        [InlineData("http://localhost/tables/foo", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo/", "http://localhost/tables/foo/")]
        [InlineData("http://localhost#fragment", "http://localhost/")]
        [InlineData("http://localhost/tables#fragment", "http://localhost/tables/")]
        [InlineData("http://localhost/tables/#fragment", "http://localhost/tables/")]
        [InlineData("http://localhost/tables/foo#fragment", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo/#fragment", "http://localhost/tables/foo/")]
        [InlineData("http://localhost?test=true", "http://localhost/")]
        [InlineData("http://localhost/tables?test=true", "http://localhost/tables/")]
        [InlineData("http://localhost/tables/?test=true", "http://localhost/tables/")]
        [InlineData("http://localhost/tables/foo?test=true", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo/?test=true", "http://localhost/tables/foo/")]
        [InlineData("http://localhost?test=true#fragment", "http://localhost/")]
        [InlineData("http://localhost/tables?test=true#fragment", "http://localhost/tables/")]
        [InlineData("http://localhost/tables/?test=true#fragment", "http://localhost/tables/")]
        [InlineData("http://localhost/tables/foo?test=true#fragment", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo/?test=true#fragment", "http://localhost/tables/foo/")]
        [Trait("Method", "Normalized")]
        public void Normalized_Works(string sut, string expected)
        {
            var builder = new UriBuilder(sut);
            var actual = builder.Normalized();
            Assert.Same(builder, actual);
            Assert.Equal(expected, actual.Uri.ToString());
        }
        #endregion

        #region WithFragment
        [Theory]
        [InlineData("http://localhost", null, "http://localhost/")]
        [InlineData("http://localhost", "", "http://localhost/")]
        [InlineData("http://localhost", "  ", "http://localhost/")]
        [InlineData("http://localhost", "newfrag", "http://localhost/#newfrag")]
        [InlineData("http://localhost", "  newfrag", "http://localhost/#newfrag")]
        [InlineData("http://localhost", "newfrag  ", "http://localhost/#newfrag")]
        [InlineData("http://localhost", "#newfrag", "http://localhost/#newfrag")]
        [InlineData("http://localhost?__filter=foo", null, "http://localhost/?__filter=foo")]
        [InlineData("http://localhost?__filter=foo", "", "http://localhost/?__filter=foo")]
        [InlineData("http://localhost?__filter=foo", "  ", "http://localhost/?__filter=foo")]
        [InlineData("http://localhost?__filter=foo", "newfrag", "http://localhost/?__filter=foo#newfrag")]
        [InlineData("http://localhost?__filter=foo", "  newfrag", "http://localhost/?__filter=foo#newfrag")]
        [InlineData("http://localhost?__filter=foo", "newfrag  ", "http://localhost/?__filter=foo#newfrag")]
        [InlineData("http://localhost#fragment", null, "http://localhost/")]
        [InlineData("http://localhost#fragment", "", "http://localhost/")]
        [InlineData("http://localhost#fragment", "  ", "http://localhost/")]
        [InlineData("http://localhost?a=b&c=d#fragment", "newfrag", "http://localhost/?a=b&c=d#newfrag")]
        [InlineData("http://localhost?a=b&c=d#fragment", "  newfrag", "http://localhost/?a=b&c=d#newfrag")]
        [InlineData("http://localhost?a=b&c=d#fragment", "newfrag  ", "http://localhost/?a=b&c=d#newfrag")]
        [Trait("Method", "WithFragment")]
        public void Withfragment_Works(string sut, string q, string expected)
        {
            var builder = new UriBuilder(sut);
            var actual = builder.WithFragment(q);
            Assert.Same(builder, actual);
            Assert.Equal(expected, actual.Uri.ToString());
        }
        #endregion

        #region WithQuery
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
        [Trait("Method", "WithQueryString")]
        public void WithQueryString_Works(string sut, string query, string expected)
        {
            var builder = new UriBuilder(sut);
            var actual = builder.WithQuery(query);
            Assert.Same(builder, actual);
            Assert.Equal(expected, actual.Uri.ToString());
        }
        #endregion

        #region WithTrailingSlash
        [Theory]
        [InlineData("http://localhost", "http://localhost/")]
        [InlineData("http://localhost/tables", "http://localhost/tables/")]
        [InlineData("http://localhost/tables/", "http://localhost/tables/")]
        [InlineData("http://localhost/tables/foo", "http://localhost/tables/foo/")]
        [InlineData("http://localhost/tables/foo/", "http://localhost/tables/foo/")]
        [InlineData("http://localhost#fragment", "http://localhost/#fragment")]
        [InlineData("http://localhost/tables#fragment", "http://localhost/tables/#fragment")]
        [InlineData("http://localhost/tables/#fragment", "http://localhost/tables/#fragment")]
        [InlineData("http://localhost/tables/foo#fragment", "http://localhost/tables/foo/#fragment")]
        [InlineData("http://localhost/tables/foo/#fragment", "http://localhost/tables/foo/#fragment")]
        [InlineData("http://localhost?test=true", "http://localhost/?test=true")]
        [InlineData("http://localhost/tables?test=true", "http://localhost/tables/?test=true")]
        [InlineData("http://localhost/tables/?test=true", "http://localhost/tables/?test=true")]
        [InlineData("http://localhost/tables/foo?test=true", "http://localhost/tables/foo/?test=true")]
        [InlineData("http://localhost/tables/foo/?test=true", "http://localhost/tables/foo/?test=true")]
        [InlineData("http://localhost?test=true#fragment", "http://localhost/?test=true#fragment")]
        [InlineData("http://localhost/tables?test=true#fragment", "http://localhost/tables/?test=true#fragment")]
        [InlineData("http://localhost/tables/?test=true#fragment", "http://localhost/tables/?test=true#fragment")]
        [InlineData("http://localhost/tables/foo?test=true#fragment", "http://localhost/tables/foo/?test=true#fragment")]
        [InlineData("http://localhost/tables/foo/?test=true#fragment", "http://localhost/tables/foo/?test=true#fragment")]
        [Trait("Method", "WithtrailingSlash")]
        public void WithTrailingSlash_Works(string sut, string expected)
        {
            var builder = new UriBuilder(sut);
            var actual = builder.WithTrailingSlash();
            Assert.Same(builder, actual);
            Assert.Equal(expected, actual.Uri.ToString());
        }
        #endregion
    }
}

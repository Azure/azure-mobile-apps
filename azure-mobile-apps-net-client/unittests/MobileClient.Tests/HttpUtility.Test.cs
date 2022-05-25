// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using System;
using Xunit;

namespace MobileClient.Tests
{
    public class HttpUtility_Test
    {
        [Theory]
        [InlineData("https://test.com/", "/about?$filter=a eq b&$orderby=c", false, "https://test.com/about?$filter=a eq b&$orderby=c")]
        [InlineData("https://test.com", "https://test.com/about?$filter=a eq b&$orderby=c", true, "https://test.com/about?$filter=a eq b&$orderby=c")]
        [InlineData("https://test.com/testmobileapp/", "https://test.com/testmobileapp/about?$filter=a eq b&$orderby=c", true, "https://test.com/testmobileapp/about?$filter=a eq b&$orderby=c")]
        [InlineData("https://test.com/testmobileapp", "https://test.com/testmobileapp/about?$filter=a eq b&$orderby=c", true, "https://test.com/testmobileapp/about?$filter=a eq b&$orderby=c")]
        public void TryParseQueryUri_ReturnsTrue_WhenQueryIsRelativeOrAbsoluteUri(string serviceUri, string query, bool isAbsolute, string expected)
        {
            Assert.True(HttpUtility.TryParseQueryUri(new Uri(serviceUri), query, out Uri result, out bool absolute));
            Assert.Equal(absolute, isAbsolute);
            Assert.Equal(Uri.UnescapeDataString(expected), Uri.UnescapeDataString(result.AbsoluteUri));
        }

        [Theory]
        [InlineData("https://test.com/", "about?$filter=a eq b&$orderby=c")]
        [InlineData("http://test.com", "$filter=a eq b&$orderby=c")]
        [InlineData("https://test.com/testmobileapp/", "$filter=a eq b&$orderby=c")]
        [InlineData("http://www.test.com/testmobileapp", "$filter=a eq b&$orderby=c")]
        public void TryParseQueryUri_ReturnsFalse_WhenQueryIsNotRelativeOrAbsoluteUri(string serviceUri, string query)
        {
            Assert.False(HttpUtility.TryParseQueryUri(new Uri(serviceUri), query, out Uri result, out bool absolute));
            Assert.False(absolute);
            Assert.Null(result);
        }

        [Theory]
        [InlineData("http://contoso.com/asdf?$filter=3", "http://contoso.com/asdf")]
        [InlineData("http://contoso.com/asdf/def?$filter=3", "http://contoso.com/asdf/def")]
        [InlineData("https://contoso.com/asdf/def?$filter=3", "https://contoso.com/asdf/def")]
        public void GetUriWithoutQuery_ReturnsUriWithPath(string serviceUri, string expected)
            => Assert.Equal(Uri.UnescapeDataString(expected), Uri.UnescapeDataString(HttpUtility.GetUriWithoutQuery(new Uri(serviceUri))));
    }
}

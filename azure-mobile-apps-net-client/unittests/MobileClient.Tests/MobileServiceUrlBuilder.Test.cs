// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;

using Xunit;

namespace MobileClient.Tests
{
    public class MobileServiceUrlBuilder_Tests
    {
        [Fact]
        public void GetQueryString_Basic()
        {
            var parameters = new Dictionary<string, string>() { { "x", "$y" }, { "&hello", "?good bye" }, { "a$", "b" } };
            Assert.Equal("x=%24y&%26hello=%3Fgood%20bye&a%24=b", MobileServiceUrlBuilder.GetQueryString(parameters));
        }

        [Fact]
        public void GetQueryString_Null()
            => Assert.Null(MobileServiceUrlBuilder.GetQueryString(null));

        [Fact]
        public void GetQueryString_Empty()
            => Assert.Null(MobileServiceUrlBuilder.GetQueryString(new Dictionary<string, string>()));

        [Fact]
        public void GetQueryString_Invalid()
        {
            var parameters = new Dictionary<string, string>() { { "$x", "someValue" } };
            Assert.Throws<ArgumentException>(() => MobileServiceUrlBuilder.GetQueryString(parameters));
        }

        [Theory]
        [InlineData("somePath?x=y&a=b", "somePath", "x=y&a=b")]
        [InlineData("somePath?x=y&a=b", "somePath", "?x=y&a=b")]
        [InlineData("somePath", "somePath", null)]
        [InlineData("somePath", "somePath", "")]
        public void CombinePathAndQueryTest(string expected, string path, string query)
            => Assert.Equal(expected, MobileServiceUrlBuilder.CombinePathAndQuery(path, query));

        [Theory]
        [InlineData("http://abc", "http://abc/")]
        [InlineData("http://abc/", "http://abc/")]
        [InlineData("http://abc/def", "http://abc/def/")]
        [InlineData("http://abc/def/", "http://abc/def/")]
        [InlineData("http://abc/     ", "http://abc/     /")]
        [InlineData("http://abc/def/     ", "http://abc/def/     /")]
        public void AddTrailingSlashTest(string path, string expected)
            => Assert.Equal(expected, MobileServiceUrlBuilder.AddTrailingSlash(path));


    }
}

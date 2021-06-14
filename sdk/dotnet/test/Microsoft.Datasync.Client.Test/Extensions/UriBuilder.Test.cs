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
        private static readonly Uri Endpoint = new("https://localhost:443/tables/foo?a=b&c=d#fragment");

        [Theory]
        [InlineData("", "https://localhost:443/tables/foo?a=b&c=d")]
        [InlineData("newfrag", "https://localhost:443/tables/foo?a=b&c=d#newfrag")]
        public void WithFragment_Works(string fragment, string expected)
        {
            var actual = new UriBuilder(Endpoint).WithFragment(fragment).ToString();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("foo.azurewebsites.net", "https://foo.azurewebsites.net:443/tables/foo?a=b&c=d#fragment")]
        [InlineData("127.0.0.1", "https://127.0.0.1:443/tables/foo?a=b&c=d#fragment")]
        public void WithHost_Works(string fragment, string expected)
        {
            var actual = new UriBuilder(Endpoint).WithHost(fragment).ToString();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("", "", "https://localhost:443/tables/foo?a=b&c=d#fragment")]
        [InlineData("username", "password", "https://username:password@localhost:443/tables/foo?a=b&c=d#fragment")]
        public void WithCredentials_Works(string username, string password, string expected)
        {
            var actual = new UriBuilder(Endpoint).WithCredentials(username, password).ToString();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("/tables/bar", "https://localhost:443/tables/bar?a=b&c=d#fragment")]
        public void WithPath_Works(string fragment, string expected)
        {
            var actual = new UriBuilder(Endpoint).WithPath(fragment).ToString();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(80, "https://localhost:80/tables/foo?a=b&c=d#fragment")]
        [InlineData(-1, "https://localhost/tables/foo?a=b&c=d#fragment")]
        public void WithPort_Works(int port, string expected)
        {
            var actual = new UriBuilder(Endpoint).WithPort(port).ToString();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(-0)]
        [InlineData(128000)]
        public void WithPort_Throws_WhenExpected(int port)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new UriBuilder(Endpoint).WithPort(port).ToString());
        }

        [Theory]
        [InlineData("trust=true", "https://localhost:443/tables/foo?trust=true#fragment")]
        public void WithQuery_Works(string query, string expected)
        {
            var actual = new UriBuilder(Endpoint).WithQuery(query).ToString();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("http", "http://localhost:443/tables/foo?a=b&c=d#fragment")]
        [InlineData("https", "https://localhost:443/tables/foo?a=b&c=d#fragment")]
        public void WithScheme_Works(string scheme, string expected)
        {
            var actual = new UriBuilder(Endpoint).WithScheme(scheme).ToString();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("file")]
        [InlineData("mailto")]
        [InlineData("mqtt")]
        public void WithScheme_Throws_WhenExpected(string scheme)
        {
            Assert.Throws<NotSupportedException>(() => new UriBuilder(Endpoint).WithScheme(scheme).ToString());
        }

        [Theory]
        [InlineData("/tables/bar", "https://localhost:443/tables/bar/?a=b&c=d#fragment")]
        [InlineData("/tables/bar/", "https://localhost:443/tables/bar/?a=b&c=d#fragment")]
        public void WithTrailingSlash_Works(string path, string expected)
        {
            var actual = new UriBuilder(Endpoint).WithPath(path).WithTrailingSlash().ToString();
            Assert.Equal(expected, actual);
        }
    }
}

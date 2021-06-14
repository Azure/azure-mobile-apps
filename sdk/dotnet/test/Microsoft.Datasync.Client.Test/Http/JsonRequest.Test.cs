// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class JsonRequest_Tests
    {
        // It's important that this has a slash-on-the-end
        private static readonly Uri Endpoint = new("https://testsite.azurewebsites.net/tables/foo/");

        [Fact]
        public void Ctor_ProvidesReasonableDefaults()
        {
            var sut = new JsonRequest();

            Assert.Equal(HttpMethod.Get, sut.Method);
            Assert.False(sut.RequestUri.IsAbsoluteUri);
            Assert.Equal("/", sut.RequestUri.ToString());
            Assert.Empty(sut.QueryParameters);
            Assert.Empty(sut.Headers);
            Assert.Null(sut.Payload);
        }

        [Fact]
        public void Method_Roundtrips()
        {
            var sut = new JsonRequest()
            {
                Method = HttpMethod.Post
            };

            Assert.Equal(HttpMethod.Post, sut.Method);
        }

        [Fact]
        public void RequestUri_Roudtrips()
        {
            var uri = new Uri("http://localhost");
            var sut = new JsonRequest()
            {
                RequestUri = uri
            };

            Assert.Same(uri, sut.RequestUri);
        }

        [Fact]
        public void RequestUri_Throws_WhenNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonRequest() { RequestUri = null });
        }

        [Fact]
        public void QueryParameters_CanRoundTrip()
        {
            var qp = new Dictionary<string, string>();
            var sut = new JsonRequest() { QueryParameters = qp };

            Assert.Same(qp, sut.QueryParameters);
        }

        [Fact]
        public void QueryParameters_Throws_WhenNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonRequest() { QueryParameters = null });
        }

        [Fact]
        public void Headers_CanRoundTrip()
        {
            var hdrs = new Dictionary<string, string>();
            var sut = new JsonRequest() { Headers = hdrs };

            Assert.Same(hdrs, sut.Headers);
        }

        [Fact]
        public void Headers_Throws_WhenNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonRequest() { Headers = null });
        }

        [Fact]
        public void Payload_CanRoundTrip()
        {
            const string payload = "test";
            var sut = new JsonRequest() { Payload = payload };

            Assert.Equal(payload, sut.Payload);
        }

        [Fact]
        public void ToAbsoluteUri_Throws_WhenNull()
        {
            var sut = new JsonRequest();

            Assert.Throws<ArgumentNullException>(() => sut.ToAbsoluteUri(null));
        }

        [Fact]
        public void ToAbsoluteUri_Throws_WhenNotAbsolute()
        {
            var sut = new JsonRequest();

            Assert.Throws<ArgumentException>(() => sut.ToAbsoluteUri(new Uri("a/b", UriKind.Relative)));
        }

        [Fact]
        public void ToAbsoluteUri_UsesRequestUri_WhenAbsolute()
        {
            var sut = new JsonRequest() { RequestUri = new Uri("http://localhost/tables/foo/1234") };
            var actual = sut.ToAbsoluteUri(Endpoint);
            Assert.Equal("http://localhost/tables/foo/1234", actual.ToString());
        }

        [Fact]
        public void ToAbsoluteUri_UsesEndpoint_WhenRelative()
        {
            var sut = new JsonRequest() { RequestUri = new Uri("1234", UriKind.Relative) };
            var actual = sut.ToAbsoluteUri(Endpoint);
            Assert.Equal("https://testsite.azurewebsites.net/tables/foo/1234", actual.ToString());
        }

        [Fact]
        public void ToAbsoluteUri_AddsQueryParameters()
        {
            var sut = new JsonRequest() { RequestUri = new Uri("1234", UriKind.Relative) };
            sut.QueryParameters.Add("tracking", "true");
            var actual = sut.ToAbsoluteUri(Endpoint);
            Assert.Equal("https://testsite.azurewebsites.net/tables/foo/1234?tracking=true", actual.ToString());
        }

        [Fact]
        public void ToAbsoluteUri_EncodesQueryParameters()
        {
            var sut = new JsonRequest() { RequestUri = new Uri("1234", UriKind.Relative) };
            sut.QueryParameters.Add("$filter", "((field eq 'foo') and (test ge 12))");
            sut.QueryParameters.Add("$select", "a,b,c");
            var actual = sut.ToAbsoluteUri(Endpoint);
            Assert.Equal("https://testsite.azurewebsites.net/tables/foo/1234?%24filter=((field+eq+%27foo%27)+and+(test+ge+12))&%24select=a%2cb%2cc", actual.ToString());
        }

        [Fact]
        public void ToHttpRequestMessage_SimpleRequest_NoPayload()
        {
            var sut = new JsonRequest()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("1234", UriKind.Relative),
                Headers = new Dictionary<string, string>() { { "If-Match", "*" } },
                QueryParameters = new Dictionary<string, string>() { { "tracking", "true" } }
            };
            var actual = sut.ToHttpRequestMessage(Endpoint);

            Assert.Equal(HttpMethod.Delete, actual.Method);
            Assert.Equal("https://testsite.azurewebsites.net/tables/foo/1234?tracking=true", actual.RequestUri.ToString());
            AssertEx.HeaderMatches("If-Match", "*", actual.Headers);
            Assert.Null(actual.Content);
        }

        [Fact]
        public void ToHttpRequestMessage_EncodesContent()
        {
            var sut = new JsonRequest()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("1234", UriKind.Relative),
                Headers = new Dictionary<string, string>() { { "If-Match", "*" } },
                QueryParameters = new Dictionary<string, string>() { { "tracking", "true" } },
                Payload = "test payload"
            };
            var actual = sut.ToHttpRequestMessage(Endpoint);

            Assert.Equal(HttpMethod.Post, actual.Method);
            Assert.Equal("https://testsite.azurewebsites.net/tables/foo/1234?tracking=true", actual.RequestUri.ToString());
            AssertEx.HeaderMatches("If-Match", "*", actual.Headers);
            Assert.NotNull(actual.Content);
            Assert.Equal("text/plain", actual.Content.Headers.ContentType.MediaType);
            Assert.Equal("test payload", actual.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ToHttpRequestMessage_CanSetContentType()
        {
            var sut = new JsonRequest()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("1234", UriKind.Relative),
                Headers = new Dictionary<string, string>() { { "Content-Type", "text/test-data" } },
                QueryParameters = new Dictionary<string, string>() { { "tracking", "true" } },
                Payload = "test payload"
            };
            var actual = sut.ToHttpRequestMessage(Endpoint);

            Assert.Equal(HttpMethod.Post, actual.Method);
            Assert.Equal("https://testsite.azurewebsites.net/tables/foo/1234?tracking=true", actual.RequestUri.ToString());
            Assert.NotNull(actual.Content);
            Assert.Equal("text/test-data", actual.Content.Headers.ContentType.MediaType);
            Assert.Equal("test payload", actual.Content.ReadAsStringAsync().Result);
        }
    }
}

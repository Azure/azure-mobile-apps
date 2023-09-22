// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Microsoft.Datasync.Client.Test.Http;

[ExcludeFromCodeCoverage]
public class HttpMessageExtensions_Tests : BaseTest
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("\"abcd\"", "abcd")]
    [InlineData("\"\"", "")]
    [InlineData("\"foo\\\"bar\"", "foo\"bar")]
    [Trait("Method", "GetVersion(EntityTagHeaderValue)")]
    public void GetVersion_Works(string tagValue, string expected)
    {
        EntityTagHeaderValue sut = tagValue == null ? null : new(tagValue, false);
        string actual = sut.GetVersion();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("\"", "\"\\\"\"")]
    [InlineData("abcd", "\"abcd\"")]
    [InlineData("foo\"bar", "\"foo\\\"bar\"")]
    [InlineData("foo\\\"bar", "\"foo\\\"bar\"")]
    [Trait("Method", "ToETagValue(string)")]
    public void ToETagValue(string version, string expected)
    {
        string actual = version.ToETagValue();
        Assert.Equal(expected, actual);
    }

    [Fact]
    [Trait("Method", "HasContent(HttpContent)")]
    public void HasContent_NullContent_Works()
    {
        HttpContent content = null;
        Assert.False(content.HasContent());
    }

    [Fact]
    [Trait("Method", "HasContent(HttpContent)")]
    public void HasContent_EmptyContent_Works()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        Assert.False(response.Content.HasContent());
    }

    [Fact]
    [Trait("Method", "HasContent(HttpContent")]
    public void HasContent_StringContent_Works()
    {
        var content = new StringContent("some content");
        Assert.True(content.HasContent());
    }

    [Fact]
    [Trait("Method", "HasContent(HttpResponseMessage)")]
    public void HasContent_EmptyResponse_Works()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        Assert.False(response.HasContent());
    }

    [Fact]
    [Trait("Method", "HasContent(HttpResponseMessage)")]
    public void HasContent_StringResponse_Works()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("some content") };
        Assert.True(response.HasContent());
    }

    [Theory]
    [InlineData(null, null, false)]
    [InlineData("None", null, false)]
    [InlineData("Accept-Encoding", null, true)]
    [InlineData(null, "foo", false)]
    [InlineData(null, "gzip", true)]
    [InlineData(null, "deflate", true)]
    [InlineData(null, "br", true)]
    [InlineData(null, "compress", true)]
    [InlineData("None", "foo", false)]
    [InlineData("None", "gzip", true)]
    [InlineData("None", "deflate", true)]
    [InlineData("None", "br", true)]
    [InlineData("None", "compress", true)]
    [InlineData("Accept-Encoding", "foo", false)]
    [InlineData("Accept-Encoding", "gzip", true)]
    [InlineData("Accept-Encoding", "deflate", true)]
    [InlineData("Accept-Encoding", "br", true)]
    [InlineData("Accept-Encoding", "compress", true)]
    [Trait("Method", "IsCompressed(HttpResponseMessage)")]
    public void IsCompressed_Works(string vary, string contentEncoding, bool expected)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        if (vary != null)
        {
            response.Headers.Add("Vary", vary);
        }
        if (contentEncoding != null)
        {
            response.Content = new StringContent("{}", Encoding.UTF8, "application/json");
            response.Content.Headers.Add("Content-Encoding", contentEncoding);
        }

        var actual = response.IsCompressed();
        Assert.Equal(expected, actual);
    }
}

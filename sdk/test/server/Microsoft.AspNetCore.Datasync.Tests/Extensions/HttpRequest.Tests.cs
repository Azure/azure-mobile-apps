// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Datasync.Tests.Extensions;

[ExcludeFromCodeCoverage]
public class HttpRequest_Tests
{
    #region Test Artifacts
    private readonly EntityTableData entity = new()
    {
        Id = Guid.NewGuid().ToString("N"),
        Deleted = false,
        UpdatedAt = DateTimeOffset.Parse("2023-01-30T13:30:15Z"),
        Version = new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F }
    };

    // These have to be RFC 1123 dates, so we have to use a fixed date
    private const string earlierTestDate = "Sun, 29 Jan 2023 13:30:15 GMT";
    private const string laterTestDate = "Tue, 31 Jan 2023 13:30:15 GMT";
    private const string matchingETag = "\"AQBCIkeP\"";
    private const string nonMatchingETag = "\"Foo\"";
    #endregion

    [Theory]
    [InlineData("http", 80, true)]
    [InlineData("http", 443, false)]
    [InlineData("http", 8000, false)]
    [InlineData("https", 80, false)]
    [InlineData("https", 443, true)]
    [InlineData("https", 8000, false)]
    public void IsDefaultPort_Works(string scheme, int port, bool expected)
    {
        UriBuilder sut = new() { Scheme = scheme, Port = port };
        sut.IsDefaultPort().Should().Be(expected);
    }

    [Theory]
    [InlineData("GET", null, null, new byte[] { })]
    [InlineData("GET", "If-Match", matchingETag, new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F })]
    [InlineData("GET", "If-None-Match", nonMatchingETag, new byte[] { })]
    [InlineData("GET", "If-Modified-Since", earlierTestDate, new byte[] { })]
    [InlineData("GET", "If-Unmodified-Since", laterTestDate, new byte[] { })]
    [InlineData("POST", null, null, new byte[] { })]
    [InlineData("POST", "If-Match", matchingETag, new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F })]
    [InlineData("POST", "If-None-Match", nonMatchingETag, new byte[] { })]
    [InlineData("POST", "If-Modified-Since", earlierTestDate, new byte[] { })]
    [InlineData("POST", "If-Unmodified-Since", laterTestDate, new byte[] { })]
    public void ParseConditionalRequestTests_Success(string method, string headerName, string headerValue, byte[] expectedVersion)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        if (!string.IsNullOrEmpty(headerName))
        {
            context.Request.Headers.Add(headerName, headerValue);
        }
        context.Request.ParseConditionalRequest(entity, out byte[] version);
        version.Should().BeEquivalentTo(expectedVersion);
    }

    [Theory]
    [InlineData("GET", "If-None-Match", matchingETag)]
    [InlineData("GET", "If-Modified-Since", laterTestDate)]
    public void ParseConditionalRequest_Throws_NotModified(string method, string headerName, string headerValue)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        if (!string.IsNullOrEmpty(headerName))
        {
            context.Request.Headers.Add(headerName, headerValue);
        }
        byte[] version = null;
        Action act = () => context.Request.ParseConditionalRequest(entity, out version);
        act.Should().Throw<HttpException>().WithStatusCode(304);
    }

    [Theory]
    [InlineData("GET", "If-Unmodified-Since", earlierTestDate)]
    [InlineData("POST", "If-Match", nonMatchingETag)]
    [InlineData("POST", "If-Modified-Since", laterTestDate)]
    [InlineData("POST", "If-None-Match", matchingETag)]
    [InlineData("POST", "If-Unmodified-Since", earlierTestDate)]
    public void ParseConditionalRequest_Throws_PreconditionFailed(string method, string headerName, string headerValue)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        if (!string.IsNullOrEmpty(headerName))
        {
            context.Request.Headers.Add(headerName, headerValue);
        }
        byte[] version = null;
        Action act = () => context.Request.ParseConditionalRequest(entity, out version);
        act.Should().Throw<HttpException>().WithStatusCode(412).And.WithPayload(entity);
    }
}

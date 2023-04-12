// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Datasync.Test.Extensions;

[ExcludeFromCodeCoverage]
public class HttpRequest_Tests
{
    private readonly InMemoryEntity testEntity = new()
    {
        Id = Guid.NewGuid().ToString("N"),
        Deleted = false,
        UpdatedAt = DateTimeOffset.Parse("Wed, 30 Jan 2019 13:30:15 GMT"),
        Version = new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F }
    };

    private const string earlierTestDate = "Tue, 29 Jan 2019 13:30:15 GMT";
    private const string laterTestDate = "Thu, 31 Jan 2019 13:30:15 GMT";
    private const string matchingETag = "\"AQBCIkeP\"";
    private const string nonMatchingETag = "\"Foo\"";

    [Theory]
    [InlineData("GET", false, null, null, null)]
    [InlineData("GET", false, "If-None-Match", nonMatchingETag, null)]
    [InlineData("GET", false, "If-None-Match", matchingETag, null)]
    [InlineData("GET", true, null, null, null)]
    [InlineData("GET", true, "If-Match", matchingETag, new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F })]
    [InlineData("GET", true, "If-None-Match", nonMatchingETag, null)]
    [InlineData("GET", true, "If-Unmodified-Since", laterTestDate, null)]
    [InlineData("POST", false, null, null, null)]
    [InlineData("POST", false, "If-None-Match", nonMatchingETag, null)]
    [InlineData("POST", false, "If-None-Match", matchingETag, null)]
    [InlineData("POST", true, null, null, null)]
    [InlineData("POST", true, "If-Match", matchingETag, new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F })]
    [InlineData("POST", true, "If-None-Match", nonMatchingETag, null)]
    [InlineData("POST", true, "If-Modified-Since", earlierTestDate, null)]
    [InlineData("POST", true, "If-Unmodified-Since", laterTestDate, null)]
    public void SuccessfulParseConditionalRequestTests(string method, bool usesObject, string headerName, string headerValue, byte[] expectedVersion)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Method = method;
        if (headerName != null)
        {
            request.Headers.Add(headerName, headerValue);
        }
        InMemoryEntity entity = usesObject ? testEntity : null;

        // Act
        request.ParseConditionalRequest(entity, out byte[] version);

        // Assert
        Assert.Equal(expectedVersion, version);
    }

    [Theory]
    [InlineData("GET", true, "If-None-Match", matchingETag)]
    [InlineData("GET", true, "If-Modified-Since", laterTestDate)]
    public void NotModifiedTests(string method, bool usesObject, string headerName, string headerValue)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Method = method;
        if (headerName != null)
        {
            request.Headers.Add(headerName, headerValue);
        }
        InMemoryEntity entity = usesObject ? testEntity : null;
        byte[] version = null;

        // Act
        var ex = Assert.Throws<NotModifiedException>(() => request.ParseConditionalRequest(entity, out version));

        // Assert
        Assert.Null(ex.Payload);
        Assert.Null(version);
    }

    [Theory]
    [InlineData("GET", false, "If-Match", matchingETag)]
    [InlineData("GET", false, "If-Match", nonMatchingETag)]
    [InlineData("GET", false, "If-Modified-Since", earlierTestDate)]
    [InlineData("GET", false, "If-Modified-Since", laterTestDate)]
    [InlineData("GET", false, "If-Unmodified-Since", earlierTestDate)]
    [InlineData("GET", false, "If-Unmodified-Since", laterTestDate)]
    [InlineData("GET", true, "If-Unmodified-Since", earlierTestDate)]
    [InlineData("POST", false, "If-Match", nonMatchingETag)]
    [InlineData("POST", false, "If-Match", matchingETag)]
    [InlineData("POST", false, "If-Modified-Since", earlierTestDate)]
    [InlineData("POST", false, "If-Modified-Since", laterTestDate)]
    [InlineData("POST", false, "If-Unmodified-Since", earlierTestDate)]
    [InlineData("POST", false, "If-Unmodified-Since", laterTestDate)]
    [InlineData("POST", true, "If-Match", nonMatchingETag)]
    [InlineData("POST", true, "If-Modified-Since", laterTestDate)]
    [InlineData("POST", true, "If-None-Match", matchingETag)]
    [InlineData("POST", true, "If-Unmodified-Since", earlierTestDate)]
    public void PreconditionFailedTests(string method, bool usesObject, string headerName, string headerValue)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Method = method;
        if (headerName != null)
        {
            request.Headers.Add(headerName, headerValue);
        }
        InMemoryEntity entity = usesObject ? testEntity : null;
        byte[] version = null;

        // Act
        var ex = Assert.Throws<PreconditionFailedException>(() => request.ParseConditionalRequest(entity, out version));

        // Assert
        if (usesObject)
        {
            Assert.Equal(testEntity, ex.Payload);
        }
        Assert.Null(version);
    }

    [Theory]
    [InlineData("http", 80, true)]
    [InlineData("http", 443, false)]
    [InlineData("http", 8000, false)]
    [InlineData("https", 80, false)]
    [InlineData("https", 443, true)]
    [InlineData("https", 8000, false)]
    public void IsDefaultPort_Works(string scheme, int port, bool expected)
    {
        // Arrange
        var builder = new UriBuilder { Scheme = scheme, Port = port };

        // Act
        bool actual = builder.IsDefaultPort();

        // Assert
        Assert.Equal(expected, actual);
    }
}

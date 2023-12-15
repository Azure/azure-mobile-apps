// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Collections.Specialized;
using System.Web;

namespace Microsoft.AspNetCore.Datasync.Tests.Extensions;

[ExcludeFromCodeCoverage]
public class HttpRequestExtensions_Tests
{
    #region Test Artifacts
    private readonly TableData testEntity = new()
    {
        Id = "",
        Deleted = false,
        UpdatedAt = DateTimeOffset.Parse("2023-11-13T12:53:13.123Z"),
        Version = new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F }
    };

    private const string matchingETag = "\"AQBCIkeP\"";
    private const string nonMatchingETag = "\"Foo\"";

    private const string earlierTestDate = "Mon, 13 Nov 2023 11:57:00 GMT";
    private const string laterTestDate = "Mon, 13 Nov 2023 14:30:00 GMT";
    #endregion

    [Theory]
    [InlineData(null, true)]
    [InlineData(earlierTestDate, true)]
    [InlineData(laterTestDate, false)]
    public void DateTimeOffset_IsAfter_NullComparison(string dateString, bool expected)
    {
        DateTimeOffset sut = DateTimeOffset.Parse("2023-11-13T12:53:13.123Z");
        DateTimeOffset? dto = dateString == null ? null : DateTimeOffset.Parse(dateString);
        sut.IsAfter(dto).Should().Be(expected);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(earlierTestDate, false)]
    [InlineData(laterTestDate, true)]
    public void DateTimeOffset_IsBefore_NullComparison(string dateString, bool expected)
    {
        DateTimeOffset sut = DateTimeOffset.Parse("2023-11-13T12:53:13.123Z");
        DateTimeOffset? dto = dateString == null ? null : DateTimeOffset.Parse(dateString);
        sut.IsBefore(dto).Should().Be(expected);
    }

    [Theory]
    [InlineData(false, null, true, true)]
    [InlineData(false, matchingETag, true, true)]
    [InlineData(false, nonMatchingETag, true, false)]
    [InlineData(true, matchingETag, true, false)]
    [InlineData(true, nonMatchingETag, true, false)]
    [InlineData(false, null, false, false)]
    [InlineData(false, matchingETag, false, false)]
    [InlineData(false, nonMatchingETag, false, false)]
    [InlineData(true, matchingETag, false, false)]
    [InlineData(true, nonMatchingETag, false, false)]
    public void EntityTagHeaderValue_Matches_Working(bool isWeak, string tagValue, bool useRealVersion, bool expected)
    {
        EntityTagHeaderValue sut = tagValue == null ? EntityTagHeaderValue.Any : new(tagValue, isWeak);
        byte[] version = useRealVersion ? testEntity.Version : Array.Empty<byte>();
        sut.Matches(version).Should().Be(expected);
    }

    [Theory]
    [InlineData("GET", null, null, false)]
    [InlineData("POST", null, null, false)]
    [InlineData("Get", "If-Match", matchingETag, true)]
    [InlineData("get", "If-None-Match", nonMatchingETag, false)]
    [InlineData("geT", "If-Modified-Since", earlierTestDate, false)]
    [InlineData("GEt", "If-Unmodified-Since", laterTestDate, false)]
    [InlineData("Post", "If-Match", matchingETag, true)]
    [InlineData("post", "If-None-Match", nonMatchingETag, false)]
    [InlineData("poST", "If-Modified-Since", earlierTestDate, false)]
    [InlineData("POst", "If-Unmodified-Since", laterTestDate, false)]
    public void ParseConditionalRequest_Success(string method, string headerName, string headerValue, bool expectedVersion)
    {
        HttpContext context = new DefaultHttpContext();
        context.Request.Method = method;
        if (headerName != null && headerValue != null)
        {
            context.Request.Headers[headerName] = headerValue;
        }

        context.Request.ParseConditionalRequest<TableData>(testEntity, out byte[] version);

        if (expectedVersion)
        {
            version.Should().BeEquivalentTo(testEntity.Version);
        }
        else
        {
            version.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData("Get", "If-Match", nonMatchingETag, 412)]
    [InlineData("get", "If-None-Match", matchingETag, 304)]
    [InlineData("get", "If-Modified-Since", laterTestDate, 304)]
    [InlineData("geT", "If-Unmodified-Since", earlierTestDate, 412)]
    [InlineData("Post", "If-Match", nonMatchingETag, 412)]
    [InlineData("post", "If-None-Match", matchingETag, 412)]
    [InlineData("POST", "If-Modified-Since", laterTestDate, 412)]
    [InlineData("PoSt", "If-Unmodified-Since", earlierTestDate, 412)]
    public void ParseConditionalRequest_Failure(string method, string headerName, string headerValue, int expectedStatusCode)
    {
        HttpContext context = new DefaultHttpContext();
        context.Request.Method = method;
        if (headerName != null && headerValue != null)
        {
            context.Request.Headers[headerName] = headerValue;
        }

        Action act = () => context.Request.ParseConditionalRequest<TableData>(testEntity, out byte[] version);

        if (expectedStatusCode == 412)
        {
            act.Should().Throw<HttpException>().WithStatusCode(expectedStatusCode).And.WithPayload(testEntity);
        }
        else
        {
            act.Should().Throw<HttpException>().WithStatusCode(expectedStatusCode);
        }
    }

    [Theory]
    [InlineData("__includedeleted=false", false)]
    [InlineData("", false)]
    [InlineData("$filter=deleted eq false", false)]
    [InlineData("__includedeleted=true", true)]
    public void ShouldIncludeDeletedItems_Works(string queryString, bool expected)
    {
        DefaultHttpContext context = new();

        NameValueCollection nvc = HttpUtility.ParseQueryString(queryString);
        Dictionary<string, StringValues> dict = nvc.AllKeys.ToDictionary(k => k, k => new StringValues(nvc[k]));
        context.Request.Query = new QueryCollection(dict);

        context.Request.ShouldIncludeDeletedItems().Should().Be(expected);
    }

    [Fact]
    public void EntityTagHeaderValue_ToByteArray_Wildcard_ReturnsEmptyArray()
    {
        EntityTagHeaderValue sut = new("*");
        byte[] actual = sut.ToByteArray();
        actual.Should().BeEmpty();
    }

    [Fact]
    public void EntityTagHeaderValue_ToByteArray_Works()
    {
        EntityTagHeaderValue sut = new(matchingETag);
        byte[] expected = testEntity.Version;
        byte[] actual = sut.ToByteArray();
        actual.Should().BeEquivalentTo(expected);
    }
}

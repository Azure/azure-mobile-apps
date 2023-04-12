// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Http;

namespace Microsoft.Datasync.Client.Test.Http;

[ExcludeFromCodeCoverage]
public class ServiceRequest_Tests : BaseTest
{
    #region Helpers
    /// <summary>
    /// A <see cref="MemberDataAttribute"/> helper to provide the
    /// list of supported HTTP methods.
    /// </summary>
    public static IEnumerable<object[]> HttpMethods()
    {
        yield return new object[] { HttpMethod.Get };
        yield return new object[] { HttpMethod.Post };
        yield return new object[] { HttpMethod.Delete };
        yield return new object[] { HttpMethod.Put };
        yield return new object[] { ServiceRequest.PATCH };
    }
    #endregion

    [Fact]
    [Trait("Method", "ToHttpRequestMessage")]
    public void ToHttpRequestMessage_NullUri_Throws()
    {
        var sut = new ServiceRequest();
        Assert.Throws<ArgumentNullException>(() => sut.ToHttpRequestMessage());
    }

    [Theory]
    [MemberData(nameof(HttpMethods))]
    [Trait("Method", "ToHttpRequestMessage")]
    public void ToHttpRequestMessage_NullBaseUri_Creates(HttpMethod method)
    {
        var sut = new ServiceRequest { Method = method, UriPathAndQuery = "/tables/movies/" };
        var actual = sut.ToHttpRequestMessage();

        Assert.Equal(method, actual.Method);
        Assert.Equal(new Uri("/tables/movies/", UriKind.Relative), actual.RequestUri);
    }

    [Theory]
    [MemberData(nameof(HttpMethods))]
    [Trait("Method", "ToHttpRequestMessage")]
    public void ToHttpRequestMessage_WithBaseUri_Creates(HttpMethod method)
    {
        var sut = new ServiceRequest { Method = method, UriPathAndQuery = "/tables/movies/" };
        var actual = sut.ToHttpRequestMessage(Endpoint);

        Assert.Equal(method, actual.Method);
        Assert.Equal(new Uri(Endpoint, "/tables/movies/"), actual.RequestUri);
    }

    [Theory]
    [MemberData(nameof(HttpMethods))]
    [Trait("Method", "ToHttpRequestMessage")]
    public void ToHttpRequestMessage_WithBaseUriAndPath_Creates(HttpMethod method)
    {
        var sut = new ServiceRequest { Method = method, UriPathAndQuery = "tables/movies/" };
        var actual = sut.ToHttpRequestMessage(new Uri("https://localhost/api/"));

        Assert.Equal(method, actual.Method);
        Assert.Equal("https://localhost/api/tables/movies/", actual.RequestUri.ToString());
    }

    [Theory]
    [MemberData(nameof(HttpMethods))]
    [Trait("Method", "ToHttpRequestMessage")]
    public void ToHttpRequestMessage_NullBaseUriAndHeaders_Creates(HttpMethod method)
    {
        var sut = new ServiceRequest
        {
            Method = method,
            UriPathAndQuery = "/tables/movies/",
            RequestHeaders = new Dictionary<string, string>()
            {
                { "If-Match", "\"etag\"" }
            }
        };
        var actual = sut.ToHttpRequestMessage();

        Assert.Equal(method, actual.Method);
        Assert.Equal(new Uri("/tables/movies/", UriKind.Relative), actual.RequestUri);
        AssertEx.HasHeader(actual.Headers, "If-Match", "\"etag\"");
    }

    [Theory]
    [MemberData(nameof(HttpMethods))]
    [Trait("Method", "ToHttpRequestMessage")]
    public void ToHttpRequestMessage_WithBaseUriAndHeaders_Creates(HttpMethod method)
    {
        var sut = new ServiceRequest
        {
            Method = method,
            UriPathAndQuery = "/tables/movies/",
            RequestHeaders = new Dictionary<string, string>()
            {
                { "If-Match", "\"etag\"" }
            }
        };
        var actual = sut.ToHttpRequestMessage(Endpoint);

        Assert.Equal(method, actual.Method);
        Assert.Equal(new Uri(Endpoint, "/tables/movies/"), actual.RequestUri);
        AssertEx.HasHeader(actual.Headers, "If-Match", "\"etag\"");
    }

    [Theory]
    [MemberData(nameof(HttpMethods))]
    [Trait("Method", "ToHttpRequestMessage")]
    public async Task ToHttpRequestMessage_WithBaseUriAndContent_Creates(HttpMethod method)
    {
        const string expectedContent = "{\"test\":\"string\"}";
        var sut = new ServiceRequest
        {
            Method = method,
            UriPathAndQuery = "/tables/movies/",
            Content = expectedContent
        };
        var actual = sut.ToHttpRequestMessage(Endpoint);

        Assert.Equal(method, actual.Method);
        Assert.Equal(new Uri(Endpoint, "/tables/movies/"), actual.RequestUri);
        Assert.Equal("application/json", actual.Content.Headers.ContentType.MediaType);
        var actualContent = await actual.Content.ReadAsStringAsync();
        Assert.Equal(expectedContent, actualContent);
    }
}

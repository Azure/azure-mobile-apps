// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Extensions;

namespace Microsoft.Datasync.Integration.Test.Server;

[ExcludeFromCodeCoverage]
[Collection("Integration")]
public class Read_Tests : BaseTest
{
    public Read_Tests(ITestOutputHelper logger) : base(logger) { }

    [Theory, CombinatorialData]
    public async Task BasicReadTests([CombinatorialValues("movies", "movies_pagesize")] string table)
    {
        string id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;

        var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/{table}/{id}");
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var actual = response.DeserializeContent<ClientMovie>();
        Assert.Equal<IMovie>(expected, actual!);
        AssertEx.SystemPropertiesMatch(expected, actual);
        AssertEx.ResponseHasConditionalHeaders(expected, response);
    }

    [Fact]
    public async Task UpdatedAt_CorrectFormat()
    {
        string id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        var expectedDTO = expected.UpdatedAt.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

        var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/movies/{id}");
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var actual = response.DeserializeContent<Dictionary<string, object>>();
        Assert.True(actual.ContainsKey("updatedAt"));
        Assert.Equal(expectedDTO, actual["updatedAt"].ToString());
    }

    [Fact]
    public async Task DateOnly_CorrectFormat()
    {
        var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/datetime?$orderby=updatedAt&$skip=5&$top=1");
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var actual = response.DeserializeContent<PageOfItems<DateTimeDto>>();
        Assert.Single(actual.Items);
        var item = actual.Items[0];
        Assert.NotNull(item);
        Assert.Equal("2022-01-06", item.DateOnly);
        Assert.Equal("01:06:00", item.TimeOnly);
    }

    [Theory]
    [InlineData("tables/movies/not-found", HttpStatusCode.NotFound)]
    [InlineData("tables/movies_pagesize/not-found", HttpStatusCode.NotFound)]
    public async Task FailedReadTests(string relativeUri, HttpStatusCode expectedStatusCode)
    {
        var response = await MovieServer.SendRequest(HttpMethod.Get, relativeUri);
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory, CombinatorialData]
    public async Task AuthenticatedReadTests(
        [CombinatorialValues(0, 1, 2, 3, 7, 14, 25)] int index,
        [CombinatorialValues(null, "failed", "success")] string userId,
        [CombinatorialValues("movies_rated", "movies_legal")] string table)
    {
        string id = Utils.GetMovieId(index);
        var expected = MovieServer.GetMovieById(id)!;
        Dictionary<string, string> headers = new();
        Utils.AddAuthHeaders(headers, userId);

        var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/{table}/{id}", headers);

        if (userId != "success")
        {
            var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
            await AssertResponseWithLoggingAsync(statusCode, response);
        }
        else
        {
            await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
            var actual = response.DeserializeContent<ClientMovie>();
            Assert.Equal<IMovie>(expected, actual!);
            AssertEx.SystemPropertiesMatch(expected, actual);
            AssertEx.ResponseHasConditionalHeaders(expected, response);
        }
    }

    [Theory]
    [InlineData("If-Match", null, HttpStatusCode.OK)]
    [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", null, HttpStatusCode.NotModified)]
    [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.OK)]
    public async Task ConditionalVersionReadTests(string headerName, string headerValue, HttpStatusCode expectedStatusCode)
    {
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        Dictionary<string, string> headers = new()
        {
            { headerName, headerValue ?? expected.GetETag() }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/movies/{id}", headers);
        await AssertResponseWithLoggingAsync(expectedStatusCode, response);
        if (expectedStatusCode == HttpStatusCode.OK || expectedStatusCode == HttpStatusCode.PreconditionFailed)
        {
            var actual = response.DeserializeContent<ClientMovie>();
            Assert.Equal<IMovie>(expected, actual!);
            AssertEx.SystemPropertiesMatch(expected, actual);
            AssertEx.ResponseHasConditionalHeaders(expected, response);
        }
    }

    [Theory]
    [InlineData("If-Modified-Since", -1, HttpStatusCode.OK)]
    [InlineData("If-Modified-Since", 1, HttpStatusCode.NotModified)]
    [InlineData("If-Unmodified-Since", 1, HttpStatusCode.OK)]
    [InlineData("If-Unmodified-Since", -1, HttpStatusCode.PreconditionFailed)]
    public async Task ConditionalReadTests(string headerName, int offset, HttpStatusCode expectedStatusCode)
    {
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        Dictionary<string, string> headers = new()
        {
            { headerName, expected.UpdatedAt.AddHours(offset).ToString("R") }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/movies/{id}", headers);
        await AssertResponseWithLoggingAsync(expectedStatusCode, response);
        if (expectedStatusCode == HttpStatusCode.OK || expectedStatusCode == HttpStatusCode.PreconditionFailed)
        {
            var actual = response.DeserializeContent<ClientMovie>();
            Assert.Equal<IMovie>(expected, actual!);
            AssertEx.SystemPropertiesMatch(expected, actual);
            AssertEx.ResponseHasConditionalHeaders(expected, response);
        }
    }

    [Theory, CombinatorialData]
    public async Task ReadSoftDeletedItem_WorksIfNotDeleted([CombinatorialValues("soft", "soft_logged")] string table)
    {
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;

        var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/{table}/{id}");
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var actual = response.DeserializeContent<ClientMovie>();
        Assert.Equal<IMovie>(expected, actual!);
        AssertEx.SystemPropertiesMatch(expected, actual);
        AssertEx.ResponseHasConditionalHeaders(expected, response);
    }

    [Theory, CombinatorialData]
    public async Task ReadSoftDeletedItem_ReturnsGoneIfDeleted([CombinatorialValues("soft", "soft_logged")] string table)
    {
        var id = GetRandomId();
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id);

        var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/{table}/{id}");
        await AssertResponseWithLoggingAsync(HttpStatusCode.Gone, response);
    }

    [Theory, CombinatorialData]
    public async Task ReadSoftDeletedItem_ReturnsIfDeletedItemsIncluded([CombinatorialValues("soft", "soft_logged")] string table)
    {
        var id = GetRandomId();
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id);
        var expected = MovieServer.GetMovieById(id)!;

        var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/{table}/{id}?__includedeleted=true");
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var actual = response.DeserializeContent<ClientMovie>();
        Assert.Equal<IMovie>(expected, actual!);
        AssertEx.SystemPropertiesMatch(expected, actual);
        AssertEx.ResponseHasConditionalHeaders(expected, response);
    }
}

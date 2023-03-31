// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Extensions;
using TestData = Datasync.Common.Test.TestData;

namespace Microsoft.Datasync.Integration.Test.Server;

[ExcludeFromCodeCoverage]
[Collection("Integration")]
public class Delete_Tests : BaseTest
{
    public Delete_Tests(ITestOutputHelper helper) : base(helper) { }

    [Fact]
    public async Task BasicDeleteTests()
    {
        var id = GetRandomId();

        var response = await MovieServer.SendRequest(HttpMethod.Delete, $"tables/movies/{id}");
        await AssertResponseWithLoggingAsync(HttpStatusCode.NoContent, response);
        Assert.Equal(TestData.Movies.Count - 1, MovieServer.GetMovieCount());
        Assert.Null(MovieServer.GetMovieById(id));
    }

    [Theory]
    [InlineData("tables/movies/not-found", HttpStatusCode.NotFound)]
    [InlineData("tables/movies_pagesize/not-found", HttpStatusCode.NotFound)]
    public async Task FailedDeleteTests(string relativeUri, HttpStatusCode expectedStatusCode)
    {
        var response = await MovieServer.SendRequest(HttpMethod.Delete, relativeUri);
        await AssertResponseWithLoggingAsync(expectedStatusCode, response);
        Assert.Equal(TestData.Movies.Count, MovieServer.GetMovieCount());
    }

    [Theory, CombinatorialData]
    public async Task AuthenticatedDeleteTests(
        [CombinatorialValues(0, 1, 2, 3, 7, 14, 25)] int index,
        [CombinatorialValues(null, "failed", "success")] string userId,
        [CombinatorialValues("movies_rated", "movies_legal")] string table)
    {
        string id = Utils.GetMovieId(index);
        Dictionary<string, string> headers = new();
        Utils.AddAuthHeaders(headers, userId);

        var response = await MovieServer.SendRequest(HttpMethod.Delete, $"tables/{table}/{id}", headers);

        if (userId != "success")
        {
            var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
            await AssertResponseWithLoggingAsync(statusCode, response);
            Assert.Equal(TestData.Movies.Count, MovieServer.GetMovieCount());
            Assert.NotNull(MovieServer.GetMovieById(id));
        }
        else
        {
            await AssertResponseWithLoggingAsync(HttpStatusCode.NoContent, response);
            Assert.Equal(TestData.Movies.Count - 1, MovieServer.GetMovieCount());
            Assert.Null(MovieServer.GetMovieById(id));
        }
    }

    [Theory]
    [InlineData("If-Match", null, HttpStatusCode.NoContent)]
    [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", null, HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.NoContent)]
    public async Task Delete_OnVersion(string headerName, string? headerValue, HttpStatusCode expectedStatusCode)
    {
        const string id = "id-107";
        var expected = MovieServer.GetMovieById(id)!;
        Dictionary<string, string> headers = new()
        {
            { headerName, headerValue ?? expected.GetETag() }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Delete, $"tables/movies/{id}", headers);
        await AssertResponseWithLoggingAsync(expectedStatusCode, response);
        switch (expectedStatusCode)
        {
            case HttpStatusCode.NoContent:
                Assert.Equal(TestData.Movies.Count - 1, MovieServer.GetMovieCount());
                Assert.Null(MovieServer.GetMovieById(id));
                break;
            case HttpStatusCode.PreconditionFailed:
                var actual = response.DeserializeContent<ClientMovie>();
                Assert.NotNull(actual);
                Assert.Equal<IMovie>(expected, actual!);
                AssertEx.SystemPropertiesMatch(expected, actual);
                AssertEx.ResponseHasConditionalHeaders(expected, response);
                break;
        }
    }

    [Theory]
    [InlineData("If-Modified-Since", -1, HttpStatusCode.NoContent)]
    [InlineData("If-Modified-Since", 1, HttpStatusCode.PreconditionFailed)]
    [InlineData("If-Unmodified-Since", 1, HttpStatusCode.NoContent)]
    [InlineData("If-Unmodified-Since", -1, HttpStatusCode.PreconditionFailed)]
    public async Task Delete_OnModified(string headerName, int offset, HttpStatusCode expectedStatusCode)
    {
        const string id = "id-107";
        var expected = MovieServer.GetMovieById(id)!;
        Dictionary<string, string> headers = new()
        {
            { headerName, expected.UpdatedAt.AddHours(offset).ToString("R") }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Delete, $"tables/movies/{id}", headers);
        await AssertResponseWithLoggingAsync(expectedStatusCode, response);
        switch (expectedStatusCode)
        {
            case HttpStatusCode.NoContent:
                Assert.Equal(TestData.Movies.Count - 1, MovieServer.GetMovieCount());
                Assert.Null(MovieServer.GetMovieById(id));
                break;
            case HttpStatusCode.PreconditionFailed:
                var actual = response.DeserializeContent<ClientMovie>();
                Assert.NotNull(actual);
                Assert.Equal<IMovie>(expected, actual!);
                AssertEx.SystemPropertiesMatch(expected, actual);
                AssertEx.ResponseHasConditionalHeaders(expected, response);
                break;
        }
    }

    [Theory, CombinatorialData]
    public async Task SoftDeleteItem_SetsDeletedFlag([CombinatorialValues("soft", "soft_logged")] string table)
    {
        var id = GetRandomId();

        var response = await MovieServer.SendRequest(HttpMethod.Delete, $"tables/{table}/{id}");
        await AssertResponseWithLoggingAsync(HttpStatusCode.NoContent, response);
        Assert.Equal(TestData.Movies.Count, MovieServer.GetMovieCount());
        var entity = MovieServer.GetMovieById(id)!;
        Assert.True(entity.Deleted);
    }

    [Theory, CombinatorialData]
    public async Task SoftDeleteItem_GoneWhenDeleted([CombinatorialValues("soft", "soft_logged")] string table)
    {
        var id = GetRandomId();
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id);

        var response = await MovieServer.SendRequest(HttpMethod.Delete, $"tables/{table}/{id}");
        await AssertResponseWithLoggingAsync(HttpStatusCode.Gone, response);
        Assert.Equal(TestData.Movies.Count, MovieServer.GetMovieCount());
        var currentEntity = MovieServer.GetMovieById(id)!;
        Assert.True(currentEntity.Deleted);
    }
}

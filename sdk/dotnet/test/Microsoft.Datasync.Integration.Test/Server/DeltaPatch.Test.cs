// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.Extensions;

namespace Microsoft.Datasync.Integration.Test.Server;

[ExcludeFromCodeCoverage]
[Collection("Integration")]
public class DeltaPatch_Tests : BaseTest
{
    [Theory, CombinatorialData]
    public async Task BasicPatchTests([CombinatorialValues("movies", "movies_pagesize")] string table)
    {
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        expected.Title = "Test Movie Title";
        expected.Rating = "PG-13";

        var patchDoc = new Dictionary<string, object>()
        {
            { "title", "Test Movie Title" },
            { "rating", "PG-13" }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Patch, $"tables/{table}/{id}", patchDoc, "application/json", null);
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var result = response.DeserializeContent<ClientMovie>();
        Assert.NotNull(result);

        var stored = MovieServer.GetMovieById(id);
        Assert.NotNull(stored);

        AssertEx.SystemPropertiesSet(stored, StartTime);
        AssertEx.SystemPropertiesChanged(expected, stored);
        AssertEx.SystemPropertiesMatch(stored, result);
        Assert.Equal<IMovie>(expected, result!);
        AssertEx.ResponseHasConditionalHeaders(stored, response);
    }

    [Theory, CombinatorialData]
    public async Task CannotPatchSystemProperties(
        [CombinatorialValues("movies", "movies_pagesize")] string table,
        [CombinatorialValues("Id", "UpdatedAt", "Version")] string propName)
    {
        // Arrange
        Dictionary<string, string> propValues = new()
        {
            { "Id", "test-id" },
            { "UpdatedAt", "2018-12-31T05:00:00.000Z" },
            { "Version", "dGVzdA==" }
        };

        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        var patchDoc = new Dictionary<string, object>()
        {
            { propName, propValues[propName] }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Patch, $"tables/{table}/{id}", patchDoc, "application/json", null);
        await AssertResponseWithLoggingAsync(HttpStatusCode.BadRequest, response);
        var stored = MovieServer.GetMovieById(id);
        Assert.NotNull(stored);
        Assert.Equal<IMovie>(expected, stored!);
        Assert.Equal<ITableData>(expected, stored!);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "tables/movies/missing-id")]
    [InlineData(HttpStatusCode.NotFound, "tables/movies_pagesize/missing-id")]
    public async Task PatchFailureTests(HttpStatusCode expectedStatusCode, string relativeUri)
    {
        var patchDoc = new Dictionary<string, object>()
        {
            { "Title", "Test Movie Title" },
            { "Rating", "PG-13" }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Patch, relativeUri, patchDoc, "application/json", null);
        await AssertResponseWithLoggingAsync(expectedStatusCode, response);
    }

    [Fact]
    public async Task PatchFailedWithWrongContentType()
    {
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        var patchDoc = new Dictionary<string, object>()
        {
            { "Title", "Test Movie Title" },
            { "Rating", "PG-13" }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Patch, $"tables/movies/{id}", patchDoc, "application/json+problem", null);
        await AssertResponseWithLoggingAsync(HttpStatusCode.UnsupportedMediaType, response);
        var stored = MovieServer.GetMovieById(id);
        Assert.NotNull(stored);
        Assert.Equal<IMovie>(expected, stored!);
        Assert.Equal<ITableData>(expected, stored!);
    }

    [Fact]
    public async Task PatchFailedWithMalformedJson()
    {
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        const string patchDoc = "{ \"some-malformed-json\": null";

        var response = await MovieServer.SendRequest(HttpMethod.Patch, $"tables/movies/{id}", patchDoc, "application/json", null);
        await AssertResponseWithLoggingAsync(HttpStatusCode.BadRequest, response);
        var stored = MovieServer.GetMovieById(id);
        Assert.NotNull(stored);
        Assert.Equal<IMovie>(expected, stored!);
        Assert.Equal<ITableData>(expected, stored!);
    }

    [Theory]
    [InlineData("duration", 50)]
    [InlineData("duration", 370)]
    [InlineData("rating", "M")]
    [InlineData("rating", "PG-13 but not always")]
    [InlineData("title", "a")]
    [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
    [InlineData("year", 1900)]
    [InlineData("year", 2035)]
    [InlineData("Duration", 50)]
    [InlineData("Duration", 370)]
    [InlineData("Rating", "M")]
    [InlineData("Rating", "PG-13 but not always")]
    [InlineData("Title", "a")]
    [InlineData("Title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
    [InlineData("Year", 1900)]
    [InlineData("Year", 2035)]
    public async Task PatchValidationFailureTests(string propName, object propValue)
    {
        string id = GetRandomId();
        var patchDoc = new Dictionary<string, object>()
        {
            { propName, propValue }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Patch, $"tables/movies/{id}", patchDoc, "application/json", null);
        await AssertResponseWithLoggingAsync(HttpStatusCode.BadRequest, response);
    }

    [Theory, CombinatorialData]
    public async Task AuthenticatedPatchTests(
        [CombinatorialValues(0, 1, 2, 3, 7, 14, 25)] int index,
        [CombinatorialValues(null, "failed", "success")] string userId,
        [CombinatorialValues("movies_rated", "movies_legal")] string table)
    {
        var id = Utils.GetMovieId(index);
        var original = MovieServer.GetMovieById(id)!;
        var expected = original.Clone();
        expected.Title = "TEST MOVIE TITLE"; // Upper Cased because of the PreCommitHook
        expected.Rating = "PG-13";

        var patchDoc = new Dictionary<string, object>()
        {
            { "Title", "Test Movie Title" },
            { "Rating", "PG-13" }
        };

        Dictionary<string, string> headers = new();
        Utils.AddAuthHeaders(headers, userId);

        var response = await MovieServer.SendRequest(HttpMethod.Patch, $"tables/{table}/{id}", patchDoc, "application/json", headers);

        if (userId != "success")
        {
            var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
            await AssertResponseWithLoggingAsync(statusCode, response);
            var stored = MovieServer.GetMovieById(id);
            Assert.NotNull(stored);
            Assert.Equal<IMovie>(original, stored!);
            Assert.Equal<ITableData>(original, stored!);
        }
        else
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = response.DeserializeContent<ClientMovie>();
            var stored = MovieServer.GetMovieById(id);
            Assert.NotNull(stored);
            AssertEx.SystemPropertiesSet(stored, StartTime);
            AssertEx.SystemPropertiesChanged(expected, stored);
            AssertEx.SystemPropertiesMatch(stored, result);
            Assert.Equal<IMovie>(expected, result!);
            AssertEx.ResponseHasConditionalHeaders(stored, response);
        }
    }

    [Theory]
    [InlineData("If-Match", null, HttpStatusCode.OK)]
    [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", null, HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.OK)]
    public async Task ConditionalVersionPatchTests(string headerName, string? headerValue, HttpStatusCode expectedStatusCode)
    {
        string id = GetRandomId();
        var entity = MovieServer.GetMovieById(id)!;
        var expected = entity.Clone();
        Dictionary<string, string> headers = new()
        {
            { headerName, headerValue ?? entity.GetETag() }
        };
        var patchDoc = new Dictionary<string, object>()
        {
            { "Title", "Test Movie Title" },
            { "Rating", "PG-13" }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Patch, $"tables/movies/{id}", patchDoc, "application/json", headers);
        await AssertResponseWithLoggingAsync(expectedStatusCode, response);
        var actual = response.DeserializeContent<ClientMovie>();
        var stored = MovieServer.GetMovieById(id);
        Assert.NotNull(stored);

        switch (expectedStatusCode)
        {
            case HttpStatusCode.OK:
                // Do the replacement in the expected
                expected.Title = "Test Movie Title";
                expected.Rating = "PG-13";

                AssertEx.SystemPropertiesSet(stored, StartTime);
                AssertEx.SystemPropertiesChanged(expected, stored);
                AssertEx.SystemPropertiesMatch(stored, actual);
                Assert.Equal<IMovie>(expected, actual!);
                AssertEx.ResponseHasConditionalHeaders(stored, response);
                break;
            case HttpStatusCode.PreconditionFailed:
                Assert.Equal<IMovie>(expected, actual!);
                AssertEx.SystemPropertiesMatch(expected, actual);
                AssertEx.ResponseHasConditionalHeaders(expected, response);
                break;
        }
    }

    [Theory]
    [InlineData("If-Modified-Since", -1, HttpStatusCode.OK)]
    [InlineData("If-Modified-Since", 1, HttpStatusCode.PreconditionFailed)]
    [InlineData("If-Unmodified-Since", 1, HttpStatusCode.OK)]
    [InlineData("If-Unmodified-Since", -1, HttpStatusCode.PreconditionFailed)]
    public async Task ConditionalModifiedPatchTests(string headerName, int offset, HttpStatusCode expectedStatusCode)
    {
        string id = GetRandomId();
        var entity = MovieServer.GetMovieById(id)!;
        var expected = entity.Clone();
        Dictionary<string, string> headers = new()
        {
            { headerName, entity.UpdatedAt.AddHours(offset).ToString("R") }
        };
        var patchDoc = new Dictionary<string, object>()
        {
            { "Title", "Test Movie Title" },
            { "Rating", "PG-13" }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Patch, $"tables/movies/{id}", patchDoc, "application/json", headers);
        await AssertResponseWithLoggingAsync(expectedStatusCode, response);
        var actual = response.DeserializeContent<ClientMovie>();
        var stored = MovieServer.GetMovieById(id);
        Assert.NotNull(stored);

        switch (expectedStatusCode)
        {
            case HttpStatusCode.OK:
                // Do the replacement in the expected
                expected.Title = "Test Movie Title";
                expected.Rating = "PG-13";

                AssertEx.SystemPropertiesSet(stored, StartTime);
                AssertEx.SystemPropertiesChanged(expected, stored);
                AssertEx.SystemPropertiesMatch(stored, actual);
                Assert.Equal<IMovie>(expected, actual!);
                AssertEx.ResponseHasConditionalHeaders(stored, response);
                break;
            case HttpStatusCode.PreconditionFailed:
                Assert.Equal<IMovie>(expected, actual!);
                AssertEx.SystemPropertiesMatch(expected, actual);
                AssertEx.ResponseHasConditionalHeaders(expected, response);
                break;
        }
    }

    [Theory, CombinatorialData]
    public async Task SoftDeletePatch_PatchDeletedItem_ReturnsGone([CombinatorialValues("soft", "soft_logged")] string table)
    {
        var id = GetRandomId();
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id);

        var patchDoc = new Dictionary<string, object>()
        {
            { "Title", "Test Movie Title" },
            { "Rating", "PG-13" }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Patch, $"tables/{table}/{id}", patchDoc, "application/json", null);
        await AssertResponseWithLoggingAsync(HttpStatusCode.Gone, response);
    }

    [Fact(Skip = "Flaky test")]
    public async Task SoftDeletePatch_CanUndeleteDeletedItem()
    {
        var id = GetRandomId();
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id);

        var expected = MovieServer.GetMovieById(id)!;
        expected.Deleted = false;

        var patchDoc = new Dictionary<string, object>()
        {
            { "deleted", false }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Patch, $"tables/soft/{id}", patchDoc, "application/json", null);
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var result = response.DeserializeContent<ClientMovie>();
        var stored = MovieServer.GetMovieById(id)!;
        AssertEx.SystemPropertiesSet(stored, StartTime);
        AssertEx.SystemPropertiesChanged(expected, stored);
        AssertEx.SystemPropertiesMatch(stored, result);
        Assert.Equal<IMovie>(expected, result!);
        AssertEx.ResponseHasConditionalHeaders(stored, response);
    }

    [Theory]
    [InlineData("soft_logged")]
    public async Task SoftDeletePatch_PatchNotDeletedItem(string table)
    {
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        expected.Title = "Test Movie Title";
        expected.Rating = "PG-13";

        var patchDoc = new Dictionary<string, object>()
        {
            { "Title", "Test Movie Title" },
            { "Rating", "PG-13" }
        };

        var response = await MovieServer.SendRequest(HttpMethod.Patch, $"tables/{table}/{id}", patchDoc, "application/json", null);
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var result = response.DeserializeContent<ClientMovie>();
        var stored = MovieServer.GetMovieById(id);
        Assert.NotNull(stored);
        AssertEx.SystemPropertiesSet(stored, StartTime);
        AssertEx.SystemPropertiesChanged(expected, stored);
        AssertEx.SystemPropertiesMatch(stored, result);
        Assert.Equal<IMovie>(expected, result!);
        AssertEx.ResponseHasConditionalHeaders(stored, response);
    }
}

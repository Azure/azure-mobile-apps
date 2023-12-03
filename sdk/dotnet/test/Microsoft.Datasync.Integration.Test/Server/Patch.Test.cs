﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.Extensions;
using System.Globalization;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Microsoft.Datasync.Integration.Test.Server;

[ExcludeFromCodeCoverage]
[Collection("Integration")]
public class Patch_Tests : BaseTest
{
    public Patch_Tests(ITestOutputHelper logger) : base(logger) { }

    [SkippableTheory, CombinatorialData]
    public async Task BasicPatchTests([CombinatorialValues("movies", "movies_pagesize")] string table)
    {
        Skip.If(BuildEnvironment.IsPipeline());
        
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        expected.Title = "Test Movie Title";
        expected.Rating = "PG-13";
        var patchDoc = new PatchOperation[]
        {
            new PatchOperation("replace", "title", "Test Movie Title"),
            new PatchOperation("replace", "rating", "PG-13")
        };

        var response = await MovieServer.SendPatch($"tables/{table}/{id}", patchDoc);
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var result = response.DeserializeContent<ClientMovie>();
        var stored = MovieServer.GetMovieById(id);
        AssertEx.SystemPropertiesSet(stored, StartTime);
        AssertEx.SystemPropertiesChanged(expected, stored);
        AssertEx.SystemPropertiesMatch(stored, result);
        Assert.Equal<IMovie>(expected, result!);
        AssertEx.ResponseHasConditionalHeaders(stored, response);
    }

    [SkippableTheory, CombinatorialData]
    public async Task CannotPatchSystemProperties(
        [CombinatorialValues("movies", "movies_pagesize")] string table,
        [CombinatorialValues("/id", "/updatedAt", "/version")] string propName)
    {
        Skip.If(BuildEnvironment.IsPipeline());
        
        Dictionary<string, string> propValues = new()
        {
            { "/id", "test-id" },
            { "/updatedAt", "2018-12-31T05:00:00.000Z" },
            { "/version", "dGVzdA==" }
        };
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        var patchDoc = new PatchOperation[] { new PatchOperation("replace", propName, propValues[propName]) };

        var response = await MovieServer.SendPatch($"tables/{table}/{id}", patchDoc);
        await AssertResponseWithLoggingAsync(HttpStatusCode.BadRequest, response);
        var stored = MovieServer.GetMovieById(id);
        Assert.Equal<IMovie>(expected, stored!);
        Assert.Equal<ITableData>(expected, stored!);
    }

    [SkippableTheory, CombinatorialData]
    public async Task CanPatchNonModifiedSystemProperties(
        [CombinatorialValues("movies", "movies_pagesize")] string table,
        [CombinatorialValues("/id", "/updatedAt", "/version")] string propName)
    {
        Skip.If(BuildEnvironment.IsPipeline());

        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        Dictionary<string, string> propValues = new()
        {
            { "/id", id },
            { "/updatedAt", expected.UpdatedAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture) },
            { "/version", Convert.ToBase64String(expected.Version) }
        };
        var patchDoc = new PatchOperation[] { new PatchOperation("replace", propName, propValues[propName]) };

        var response = await MovieServer.SendPatch($"tables/{table}/{id}", patchDoc);
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var result = response.DeserializeContent<ClientMovie>();
        var stored = MovieServer.GetMovieById(id);
        AssertEx.SystemPropertiesSet(stored, StartTime);
        AssertEx.SystemPropertiesChanged(expected, stored);
        AssertEx.SystemPropertiesMatch(stored, result);
        Assert.Equal<IMovie>(expected, result!);
        AssertEx.ResponseHasConditionalHeaders(stored, response);
    }

    [SkippableTheory]
    [InlineData(HttpStatusCode.NotFound, "tables/movies/missing-id")]
    [InlineData(HttpStatusCode.NotFound, "tables/movies_pagesize/missing-id")]
    public async Task PatchFailureTests(HttpStatusCode expectedStatusCode, string relativeUri)
    {
        Skip.If(BuildEnvironment.IsPipeline());
        
        PatchOperation[] patchDoc = new PatchOperation[]
        {
            new PatchOperation("replace", "title", "Home Video"),
            new PatchOperation("replace", "rating", "PG-13")
        };

        var response = await MovieServer.SendPatch(relativeUri, patchDoc);
        await AssertResponseWithLoggingAsync(expectedStatusCode, response);
    }

    [SkippableFact]
    public async Task PatchFailedWithWrongContentType()
    {
        Skip.If(BuildEnvironment.IsPipeline());
        
        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        PatchOperation[] patchDoc = new PatchOperation[]
        {
            new PatchOperation("replace", "title", "Home Video"),
            new PatchOperation("replace", "rating", "PG-13")
        };

        var response = await MovieServer.SendPatch($"tables/movies/{id}", patchDoc, "application/json+problem");
        await AssertResponseWithLoggingAsync(HttpStatusCode.UnsupportedMediaType, response);
        var stored = MovieServer.GetMovieById(id);
        Assert.Equal<IMovie>(expected, stored!);
        Assert.Equal<ITableData>(expected, stored!);
    }

    [Theory]
    [InlineData("duration", 50)]
    [InlineData("duration", 370)]
    [InlineData("duration", null)]
    [InlineData("rating", "M")]
    [InlineData("rating", "PG-13 but not always")]
    [InlineData("title", "a")]
    [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
    [InlineData("title", null)]
    [InlineData("year", 1900)]
    [InlineData("year", 2035)]
    [InlineData("year", null)]
    public async Task PatchValidationFailureTests(string propName, object propValue)
    {
        // Arrange
        string id = GetRandomId();
        var patchDoc = new PatchOperation[]
        {
            propValue == null ? new PatchOperation("remove", $"/{propName}") : new PatchOperation("replace", $"/{propName}", propValue)
        };

        var response = await MovieServer.SendPatch($"tables/movies/{id}", patchDoc);
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

        var patchDoc = new PatchOperation[]
        {
            new PatchOperation("replace", "title", "Test Movie Title"),
            new PatchOperation("replace", "rating", "PG-13")
        };

        Dictionary<string, string> headers = new();
        Utils.AddAuthHeaders(headers, userId);

        var response = await MovieServer.SendPatch($"tables/{table}/{id}", patchDoc, headers);
        var stored = MovieServer.GetMovieById(id);
        if (userId != "success")
        {
            var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
            await AssertResponseWithLoggingAsync(statusCode, response);
            Assert.Equal<IMovie>(original, stored!);
            Assert.Equal<ITableData>(original, stored!);
        }
        else
        {
            await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
            var result = response.DeserializeContent<ClientMovie>();
            AssertEx.SystemPropertiesSet(stored, StartTime);
            AssertEx.SystemPropertiesChanged(expected, stored);
            AssertEx.SystemPropertiesMatch(stored, result);
            Assert.Equal<IMovie>(expected, result!);
            AssertEx.ResponseHasConditionalHeaders(stored, response);
        }
    }

    [SkippableTheory]
    [InlineData("If-Match", null, HttpStatusCode.OK)]
    [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", null, HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.OK)]
    public async Task ConditionalVersionPatchTests(string headerName, string headerValue, HttpStatusCode expectedStatusCode)
    {
        Skip.If(BuildEnvironment.IsPipeline());

        string id = GetRandomId();
        var entity = MovieServer.GetMovieById(id)!;
        var expected = entity.Clone();
        Dictionary<string, string> headers = new()
        {
            { headerName, headerValue ?? entity.GetETag() }
        };
        var patchDoc = new PatchOperation[]
        {
            new PatchOperation("replace", "title", "Test Movie Title"),
            new PatchOperation("replace", "rating", "PG-13")
        };

        var response = await MovieServer.SendPatch($"tables/movies/{id}", patchDoc, headers);
        await AssertResponseWithLoggingAsync(expectedStatusCode, response);
        var actual = response.DeserializeContent<ClientMovie>();
        var stored = MovieServer.GetMovieById(id)!;
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

    [SkippableTheory]
    [InlineData("If-Modified-Since", -1, HttpStatusCode.OK)]
    [InlineData("If-Modified-Since", 1, HttpStatusCode.PreconditionFailed)]
    [InlineData("If-Unmodified-Since", 1, HttpStatusCode.OK)]
    [InlineData("If-Unmodified-Since", -1, HttpStatusCode.PreconditionFailed)]
    public async Task ConditionalPatchTests(string headerName, int offset, HttpStatusCode expectedStatusCode)
    {
        Skip.If(BuildEnvironment.IsPipeline());
        
        string id = GetRandomId();
        var entity = MovieServer.GetMovieById(id)!;
        Dictionary<string, string> headers = new()
        {
            { headerName, entity.UpdatedAt.AddHours(offset).ToString("R") }
        };
        var patchDoc = new PatchOperation[]
        {
            new PatchOperation("replace", "title", "Test Movie Title"),
            new PatchOperation("replace", "rating", "PG-13")
        };
        var expected = MovieServer.GetMovieById(id)!;

        var response = await MovieServer.SendPatch($"tables/movies/{id}", patchDoc, headers);
        await AssertResponseWithLoggingAsync(expectedStatusCode, response);
        var actual = response.DeserializeContent<ClientMovie>();
        var stored = MovieServer.GetMovieById(id)!;
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

        var patchDoc = new PatchOperation[]
        {
            new PatchOperation("replace", "title", "Test Movie Title"),
            new PatchOperation("replace", "rating", "PG-13")
        };

        var response = await MovieServer.SendPatch($"tables/{table}/{id}", patchDoc);
        await AssertResponseWithLoggingAsync(HttpStatusCode.Gone, response);
    }

    [SkippableFact]
    public async Task SoftDeletePatch_CanUndeleteDeletedItem()
    {
        Skip.If(BuildEnvironment.IsPipeline());

        var id = GetRandomId();
        await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id);

        var expected = MovieServer.GetMovieById(id)!;
        expected.Deleted = false;
        var patchDoc = new PatchOperation[]
        {
            new PatchOperation("replace", "deleted", false)
        };

        var response = await MovieServer.SendPatch($"tables/soft/{id}", patchDoc);
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var result = response.DeserializeContent<ClientMovie>();
        var stored = MovieServer.GetMovieById(id);
        AssertEx.SystemPropertiesSet(stored, StartTime);
        AssertEx.SystemPropertiesChanged(expected, stored);
        AssertEx.SystemPropertiesMatch(stored, result);
        Assert.Equal<IMovie>(expected, result!);
        AssertEx.ResponseHasConditionalHeaders(stored, response);
    }

    [SkippableTheory]
    [InlineData("soft")]
    [InlineData("soft_logged")]
    public async Task SoftDeletePatch_PatchNotDeletedItem(string table)
    {
        Skip.If(BuildEnvironment.IsPipeline());

        var id = GetRandomId();
        var expected = MovieServer.GetMovieById(id)!;
        expected.Title = "Test Movie Title";
        expected.Rating = "PG-13";

        var patchDoc = new PatchOperation[]
        {
            new PatchOperation("replace", "title", "Test Movie Title"),
            new PatchOperation("replace", "rating", "PG-13")
        };

        var response = await MovieServer.SendPatch($"tables/{table}/{id}", patchDoc);
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
        var result = response.DeserializeContent<ClientMovie>();
        var stored = MovieServer.GetMovieById(id);
        AssertEx.SystemPropertiesSet(stored, StartTime);
        AssertEx.SystemPropertiesChanged(expected, stored);
        AssertEx.SystemPropertiesMatch(stored, result);
        Assert.Equal<IMovie>(expected, result!);
        AssertEx.ResponseHasConditionalHeaders(stored, response);
    }
}

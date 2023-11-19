// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Datasync.Common.TestData;
using Microsoft.AspNetCore.Datasync.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Microsoft.AspNetCore.Datasync.Tests;

[ExcludeFromCodeCoverage]
public class Service_Tests : IClassFixture<DatasyncServiceFactory>
{
    #region Setup
    private readonly DatasyncServiceFactory _factory;
    private readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;

    public Service_Tests(DatasyncServiceFactory factory)
    {
        _factory = factory;
    }

    public HttpClient Client { get => _factory.CreateClient(); }

    /// <summary>
    /// Gets a copy of the movie as a cleint movie.
    /// </summary>
    private static ClientMovie GetClientMovie(MovieBase source)
    {
        ClientMovie destination = new();
        source.CopyTo(destination);
        return destination;
    }

    /// <summary>
    /// Gets a copy of the movie from the database and returns it as a ClientMovie.
    /// </summary>
    /// <param name="id">The ID of the movie.</param>
    private ClientMovie GetClientMovie(string id)
    {
        using var context = _factory.Server.Host.Services.GetRequiredService<SqliteDbContext>();
        SqliteEntityMovie serviceMovie = context.Movies.Find(id);
        return new ClientMovie(serviceMovie);
    }
    #endregion

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_WorksAsExpected(bool hasId)
    {
        ClientMovie entity = GetClientMovie(Movies.BlackPanther);
        entity.Id = hasId ? "39594e79-1ff9-4969-a9ca-406246280a38" : null;

        HttpResponseMessage result = await Client.PostAsJsonAsync("/tables/movies", entity);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
        // TODO: Check the Location header
        result.Content.Headers.ContentType.MediaType.Should().Be("application/json");
        result.Content.Headers.ContentLength.Should().BeGreaterThan(0);

        ClientMovie actual = await result.Content.ReadFromJsonAsync<ClientMovie>();
        actual.Should().BeEquivalentTo<IMovie>(entity);
        if (hasId)
        {
            actual.Id.Should().Be(entity.Id);
        }
        else
        {
            actual.Id.Should().NotBeNullOrEmpty().And.BeAGuid();
        }

        actual.Version.Should().NotBeNullOrEmpty();
        actual.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        actual.Deleted.Should().BeFalse();
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
    public async Task Create_ValidationError_ReturnsBadRequest(string propName, object propValue)
    {
        throw new NotImplementedException();
    }

    [Theory]
    [InlineData("id-100")]
    public async Task Create_ExistingId_ReturnsConflict(string id)
    {
        ClientMovie entity = GetClientMovie(Movies.BlackPanther); entity.Id = id;
        ClientMovie source = GetClientMovie(id);

        HttpResponseMessage result = await Client.PostAsJsonAsync("/tables/movies", entity);

        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.Content.Headers.ContentType.MediaType.Should().Be("application/json");
        result.Content.Headers.ContentLength.Should().BeGreaterThan(0);

        ClientMovie actual = await result.Content.ReadFromJsonAsync<ClientMovie>();
        actual.Should().BeEquivalentTo<IMovie>(entity);
        actual.Id.Should().Be(source.Id);
        actual.UpdatedAt.Should().Be(source.UpdatedAt);
        actual.Version.Should().Be(source.Version);
        actual.Deleted.Should().Be(source.Deleted);
    }
}

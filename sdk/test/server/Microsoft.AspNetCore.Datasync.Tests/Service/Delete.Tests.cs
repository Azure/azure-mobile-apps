// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Net;

namespace Microsoft.AspNetCore.Datasync.Tests.Service;

[ExcludeFromCodeCoverage]
[Collection("ServiceTests")]
public class Delete_Tests : ServiceTest, IClassFixture<ServiceApplicationFactory>
{
    public Delete_Tests(ServiceApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Delete_ById_Returns204()
    {
        var existingMovie = factory.GetRandomMovie();

        var response = await client.DeleteAsync($"{factory.MovieEndpoint}/{existingMovie.Id}");
        response.Should().HaveStatusCode(HttpStatusCode.NoContent);

        var serverEntity = factory.GetServerEntityById<InMemoryMovie>(existingMovie.Id);
        serverEntity.Should().BeNull();
    }

    [Theory]
    [InlineData("If-Match", null, HttpStatusCode.NoContent)]
    [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", null, HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.NoContent)]
    public async Task Delete_WithVersioning_Works(string headerName, string value, HttpStatusCode expectedStatusCode)
    {
        var existingMovie = factory.GetRandomMovie();
        var etag = value ?? $"\"{Convert.ToBase64String(existingMovie.Version)}\"";

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{factory.MovieEndpoint}/{existingMovie.Id}");
        request.Headers.Add(headerName, etag);

        var response = await client.SendAsync(request);
        response.Should().HaveStatusCode(expectedStatusCode);

        // TODO: If HttpStatusCode.PreconditionFailed, verify that the returned entity matches the server entity.
    }

    [Fact]
    public async Task Delete_MissingId_Returns404()
    {
        var response = await client.DeleteAsync($"{factory.MovieEndpoint}/missing");
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NotSoftDeleted_Returns204()
    {
        var existingMovie = factory.GetRandomMovie();
        byte[] existingVersion = existingMovie.Version.ToArray();
        DateTimeOffset existingUpdatedAt = (DateTimeOffset)existingMovie.UpdatedAt;

        var response = await client.DeleteAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}");
        response.Should().HaveStatusCode(HttpStatusCode.NoContent);

        var serverEntity = factory.GetServerEntityById<InMemoryMovie>(existingMovie.Id);
        serverEntity.Should().NotBeNull();
        serverEntity.UpdatedAt.Should().NotBe(existingUpdatedAt).And.BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        serverEntity.Version.Should().NotBeEquivalentTo(existingVersion);
        serverEntity.Deleted.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_SoftDeletedId_Returns410()
    {
        var existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);

        var response = await client.DeleteAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}");
        response.Should().HaveStatusCode(HttpStatusCode.Gone);

        var serverEntity = factory.GetServerEntityById<InMemoryMovie>(existingMovie.Id);
        serverEntity.Should().NotBeNull();
    }
}

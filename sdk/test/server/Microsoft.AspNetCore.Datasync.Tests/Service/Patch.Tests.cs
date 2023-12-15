using Azure.Core.Pipeline;
using Datasync.Common.Models;
using System.Net;
using System.Text;

namespace Microsoft.AspNetCore.Datasync.Tests.Service;

[ExcludeFromCodeCoverage]
public class Patch_Tests : IClassFixture<ServiceApplicationFactory>
{
    private readonly HttpClient client;
    private readonly ServiceApplicationFactory factory;
    private readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;

    public Patch_Tests(ServiceApplicationFactory factory)
    {
        this.factory = factory;
        this.client = factory.CreateClient();
    }

    [Fact]
    public async Task Patch_Works()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        const string patchDoc = @"[{""op"":""replace"",""path"":""/title"",""value"":""New Title""}]";
        var response = await client.PatchAsync($"{factory.MovieEndpoint}/{existingMovie.Id}", new StringContent(patchDoc, Encoding.UTF8, "application/json-patch+json"));
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull();
        clientMovie.Id.Should().Be(existingMovie.Id);
        clientMovie.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        clientMovie.Version.Should().NotBeNullOrEmpty().And.NotBe(Convert.ToBase64String(existingMovie.Version));

        InMemoryMovie inMemoryMovie = factory.GetServerEntityById<InMemoryMovie>(clientMovie.Id);
        clientMovie.Should().BeEquivalentTo<IMovie>(inMemoryMovie);
        clientMovie.UpdatedAt.Should().Be(inMemoryMovie.UpdatedAt);
        clientMovie.Version.Should().Be(Convert.ToBase64String(inMemoryMovie.Version));
        clientMovie.Title.Should().Be("New Title");

        response.Headers.Location.Should().Be($"{client.BaseAddress}{factory.MovieEndpoint}/{clientMovie.Id}");
        response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
    }

    [Fact]
    public async Task Patch_MissingId_Works()
    {
        const string patchDoc = @"[{""op"":""replace"",""path"":""/title"",""value"":""New Title""}]";
        var response = await client.PatchAsync($"{factory.MovieEndpoint}/missing", new StringContent(patchDoc, Encoding.UTF8, "application/json-patch+json"));
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("If-Match", null, HttpStatusCode.OK)]
    [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", null, HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.OK)]
    public async Task Patch_WithVersioning_Works(string headerName, string headerValue, HttpStatusCode expectedStatusCode)
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        byte[] existingMovieVersion = existingMovie.Version.ToArray();
        const string patchDoc = @"[{""op"":""replace"",""path"":""/title"",""value"":""New Title""}]";
        string etag = headerValue ?? $"\"{Convert.ToBase64String(existingMovie.Version)}\"";

        HttpRequestMessage request = new(HttpMethod.Patch, $"{factory.MovieEndpoint}/{existingMovie.Id}");
        request.Headers.Add(headerName, etag);
        request.Content = new StringContent(patchDoc, Encoding.UTF8, "application/json-patch+json");

        var response = await client.SendAsync(request);
        response.Should().HaveStatusCode(expectedStatusCode);

        InMemoryMovie content = await response.Content.ReadFromJsonAsync<InMemoryMovie>();

        if (expectedStatusCode == HttpStatusCode.PreconditionFailed)
        {
            response.Headers.ETag?.Tag.Should().Be($"\"{Convert.ToBase64String(existingMovie.Version)}\"");
            content.Should().NotBeNull().And.BeEquivalentTo<IMovie>(existingMovie);
            content.Title.Should().NotBe("New Title");
            content.Version.Should().BeEquivalentTo(existingMovieVersion);
        }

        if (expectedStatusCode == HttpStatusCode.OK)
        {
            response.Headers.ETag?.Tag.Should().NotBe($"\"{Convert.ToBase64String(existingMovie.Version)}\"");
            content.Should().NotBeNull().And.BeEquivalentTo<IMovie>(existingMovie, x => x.Excluding(m => m.Title));
            content.Title.Should().Be("New Title");
            content.Version.Should().NotBeEquivalentTo(existingMovieVersion);
            content.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        }
    }

    [Fact]
    public async Task Patch_SystemProperties_Retuns400()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        const string patchDoc = @"[{""op"":""replace"",""path"":""/updatedAt"",""value"":""2023-12-15T13:54:23.000Z""}]";
        var response = await client.PatchAsync($"{factory.MovieEndpoint}/{existingMovie.Id}", new StringContent(patchDoc, Encoding.UTF8, "application/json-patch+json"));
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Parse_UnsupportedMediaType()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        const string patchDoc = @"[{""op"":""replace"",""path"":""/title"",""value"":""New Title""}]";
        var response = await client.PatchAsync($"{factory.MovieEndpoint}/{existingMovie.Id}", new StringContent(patchDoc, Encoding.UTF8, "application/json+problem"));
        response.Should().HaveStatusCode(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task Patch_SoftDeleted_Returns410()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);
        const string patchDoc = @"[{""op"":""replace"",""path"":""/title"",""value"":""New Title""}]";
        var response = await client.PatchAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}", new StringContent(patchDoc, Encoding.UTF8, "application/json-patch+json"));
        response.Should().HaveStatusCode(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Patch_SoftDeleted_CanUndelete()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);
        const string patchDoc = @"[{""op"":""replace"",""path"":""/deleted"",""value"":false}]";
        var response = await client.PatchAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}", new StringContent(patchDoc, Encoding.UTF8, "application/json-patch+json"));
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull();
        clientMovie.Id.Should().Be(existingMovie.Id);
        clientMovie.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        clientMovie.Deleted.Should().BeFalse();
        clientMovie.Version.Should().NotBeNullOrEmpty().And.NotBe(Convert.ToBase64String(existingMovie.Version));

        InMemoryMovie inMemoryMovie = factory.GetServerEntityById<InMemoryMovie>(clientMovie.Id);
        clientMovie.Should().BeEquivalentTo<IMovie>(inMemoryMovie);
        inMemoryMovie.Deleted.Should().BeFalse();
    }
}

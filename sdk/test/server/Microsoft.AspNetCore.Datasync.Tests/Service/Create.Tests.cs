using Datasync.Common.Models;
using Datasync.Common.TestData;
using Microsoft.AspNetCore.Datasync.InMemory;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync.Tests.Service;

[ExcludeFromCodeCoverage]
public class Create_Tests : IClassFixture<ServiceApplicationFactory>
{
    private readonly ServiceApplicationFactory factory;
    private readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;

    public Create_Tests(ServiceApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("de76a422-7fb0-4f1f-9bb4-12b3c7882541")]
    public async Task Create_WithValidInput_Works(string id)
    {
        HttpClient client = factory.CreateClient();
        ClientMovie source = new(Movies.BlackPanther) { Id = id };
        string content = JsonSerializer.Serialize(source);
        HttpResponseMessage response = await client.PostAsync(factory.MovieEndpoint, new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.Created);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull();
        clientMovie.Id.Should().NotBeNullOrEmpty();
        if (!string.IsNullOrEmpty(id))
        {
            clientMovie.Id.Should().Be(id);
        }
        clientMovie.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        clientMovie.Version.Should().NotBeNullOrEmpty();

        InMemoryMovie inMemoryMovie = factory.GetMovieById(clientMovie.Id);
        clientMovie.Should().BeEquivalentTo<IMovie>(inMemoryMovie);
        clientMovie.UpdatedAt.Should().Be(inMemoryMovie.UpdatedAt);
        clientMovie.Version.Should().Be(Convert.ToBase64String(inMemoryMovie.Version));

        response.Headers.Location.Should().Be($"{client.BaseAddress}{factory.MovieEndpoint}/{clientMovie.Id}");
        response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
    }

    [Fact]
    public async Task Create_Conflict()
    {
        HttpClient client = factory.CreateClient();
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        ClientMovie source = new(Movies.BlackPanther) { Id = existingMovie.Id };
        string content = JsonSerializer.Serialize(source);
        HttpResponseMessage response = await client.PostAsync(factory.MovieEndpoint, new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.Conflict);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull().And.BeEquivalentTo<IMovie>(existingMovie);
        clientMovie.Id.Should().Be(existingMovie.Id);
        clientMovie.UpdatedAt.Should().Be(existingMovie.UpdatedAt);
        clientMovie.Version.Should().Be(Convert.ToBase64String(existingMovie.Version));

        response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
    }
}

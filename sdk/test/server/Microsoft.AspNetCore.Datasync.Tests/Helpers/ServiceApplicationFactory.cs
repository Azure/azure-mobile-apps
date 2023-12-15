using Datasync.Common.Models;
using Microsoft.AspNetCore.Datasync.InMemory;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Microsoft.AspNetCore.Datasync.Tests.Helpers;

[ExcludeFromCodeCoverage]
public class ServiceApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // base.ConfigureWebHost(builder);
    }

    internal InMemoryMovie GetMovieById(string id)
    {
        using IServiceScope scope = Services.CreateScope();
        InMemoryRepository<InMemoryMovie> repository = scope.ServiceProvider.GetRequiredService<IRepository<InMemoryMovie>>() as InMemoryRepository<InMemoryMovie>;
        return repository.GetEntity(id);
    }

    internal InMemoryMovie GetRandomMovie()
    {
        using IServiceScope scope = Services.CreateScope();
        InMemoryRepository<InMemoryMovie> repository = scope.ServiceProvider.GetRequiredService<IRepository<InMemoryMovie>>() as InMemoryRepository<InMemoryMovie>;
        List<InMemoryMovie> entities = repository.GetEntities();
        return entities[new Random().Next(entities.Count)];
    }

    internal string MovieEndpoint = "api/in-memory/movies";
}

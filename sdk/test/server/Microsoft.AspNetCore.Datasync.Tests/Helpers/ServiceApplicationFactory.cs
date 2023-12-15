// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

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

    internal string KitchenSinkEndpoint = "api/in-memory/kitchensink";
    internal string MovieEndpoint = "api/in-memory/movies";
    internal string SoftDeletedMovieEndpoint = "api/in-memory/softmovies";

    internal TEntity GetServerEntityById<TEntity>(string id) where TEntity : InMemoryTableData
    {
        using IServiceScope scope = Services.CreateScope();
        InMemoryRepository<TEntity> repository = scope.ServiceProvider.GetRequiredService<IRepository<TEntity>>() as InMemoryRepository<TEntity>;
        return repository.GetEntity(id);
    }

    internal void SoftDelete<TEntity>(TEntity entity) where TEntity : InMemoryTableData
    {
        using IServiceScope scope = Services.CreateScope();
        InMemoryRepository<TEntity> repository = scope.ServiceProvider.GetRequiredService<IRepository<TEntity>>() as InMemoryRepository<TEntity>;
        entity.Deleted = true;
        repository.StoreEntity(entity);
    }

    internal InMemoryMovie GetRandomMovie()
    {
        using IServiceScope scope = Services.CreateScope();
        InMemoryRepository<InMemoryMovie> repository = scope.ServiceProvider.GetRequiredService<IRepository<InMemoryMovie>>() as InMemoryRepository<InMemoryMovie>;
        List<InMemoryMovie> entities = repository.GetEntities();
        return entities[new Random().Next(entities.Count)];
    }
}

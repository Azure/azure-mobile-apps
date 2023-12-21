// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Microsoft.AspNetCore.Datasync.InMemory;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;

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
    internal string PagedMovieEndpoint = "api/in-memory/pagedmovies";
    internal string SoftDeletedMovieEndpoint = "api/in-memory/softmovies";

    internal int Count<TEntity>() where TEntity : InMemoryTableData
    {
        using IServiceScope scope = Services.CreateScope();
        InMemoryRepository<TEntity> repository = scope.ServiceProvider.GetRequiredService<IRepository<TEntity>>() as InMemoryRepository<TEntity>;
        return repository.GetEntities().Count;
    }

    internal TEntity GetServerEntityById<TEntity>(string id) where TEntity : InMemoryTableData
    {
        using IServiceScope scope = Services.CreateScope();
        InMemoryRepository<TEntity> repository = scope.ServiceProvider.GetRequiredService<IRepository<TEntity>>() as InMemoryRepository<TEntity>;
        return repository.GetEntity(id);
    }

    internal void SoftDelete<TEntity>(TEntity entity, bool deleted = true) where TEntity : InMemoryTableData
    {
        using IServiceScope scope = Services.CreateScope();
        InMemoryRepository<TEntity> repository = scope.ServiceProvider.GetRequiredService<IRepository<TEntity>>() as InMemoryRepository<TEntity>;
        entity.Deleted = deleted;
        repository.StoreEntity(entity);
    }

    internal void SoftDelete<TEntity>(Expression<Func<TEntity, bool>> expression, bool deleted = true) where TEntity : InMemoryTableData
    {
        using IServiceScope scope = Services.CreateScope();
        InMemoryRepository<TEntity> repository = scope.ServiceProvider.GetRequiredService<IRepository<TEntity>>() as InMemoryRepository<TEntity>;
        foreach (TEntity entity in repository.GetEntities().Where(expression.Compile()))
        {
            entity.Deleted = deleted;
            repository.StoreEntity(entity);
        }
    }

    internal void Store<TEntity>(TEntity entity) where TEntity : InMemoryTableData
    {
        using IServiceScope scope = Services.CreateScope();
        InMemoryRepository<TEntity> repository = scope.ServiceProvider.GetRequiredService<IRepository<TEntity>>() as InMemoryRepository<TEntity>;
        repository.StoreEntity(entity);
    }

    internal InMemoryMovie GetRandomMovie()
    {
        // Note that we don't use all movies, since some of them are not "valid", which will result in a 400 error instead
        // of the expected error when testing replace or create functionality.
        using IServiceScope scope = Services.CreateScope();
        InMemoryRepository<InMemoryMovie> repository = scope.ServiceProvider.GetRequiredService<IRepository<InMemoryMovie>>() as InMemoryRepository<InMemoryMovie>;
        List<InMemoryMovie> entities = repository.GetEntities().Where(x => IsValid(x)).ToList();
        return entities[new Random().Next(entities.Count)];
    }

    /// <summary>
    /// Checks that the movie is "valid" according to the server.
    /// </summary>
    /// <param name="movie">The movie to check.</param>
    /// <returns><c>true</c> if valid, <c>false</c> otherwise.</returns>
    internal static bool IsValid(IMovie movie)
    {
        return movie.Title.Length >= 2 && movie.Title.Length <= 60
            && movie.Year >= 1920 && movie.Year <= 2030
            && movie.Duration >= 60 && movie.Duration <= 360;
    }
}

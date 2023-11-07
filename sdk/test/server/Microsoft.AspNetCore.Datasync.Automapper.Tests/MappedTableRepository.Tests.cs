// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using AutoMapper;
using Datasync.Common.Tests;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Datasync.Automapper.Tests;

[ExcludeFromCodeCoverage]
public class MappedTableRepository_Tests : RepositoryTests<MovieDto>
{
    #region Setup
    private readonly ITestOutputHelper output;
    private TestDbContext context;
    private EntityTableRepository<EntityMovie> innerRepository;
    private IMapper mapper;
    private MappedTableRepository<EntityMovie, MovieDto> repository;
    private List<MovieDto> movies;

    public MappedTableRepository_Tests(ITestOutputHelper output) : base()
    {
        this.output = output;
    }

    protected override Task<MovieDto> GetEntityAsync(string id)
        => Task.FromResult(mapper.Map<MovieDto>(context.Movies.AsNoTracking().SingleOrDefault(m => m.Id == id)));

    protected override Task<int> GetEntityCountAsync()
        => Task.FromResult(context.Movies.Count());

    protected override Task<IRepository<MovieDto>> GetPopulatedRepositoryAsync()
    {
        context = TestDbContext.CreateContext(output);
        EntityTableRepositoryOptions options = new() { DatabaseUpdatesTimestamp = false, DatabaseUpdatesVersion = false };
        innerRepository = new EntityTableRepository<EntityMovie>(context, options);

        mapper = new MapperConfiguration(c => c.AddProfile(new MapperProfile())).CreateMapper();
        repository = new MappedTableRepository<EntityMovie, MovieDto>(mapper, innerRepository);
        movies = context.Movies.AsNoTracking().ToList().ConvertAll(m => mapper.Map<MovieDto>(m));
        return Task.FromResult<IRepository<MovieDto>>(repository);
    }

    protected override Task<string> GetRandomEntityIdAsync(bool exists)
    {
        Random random = new();
        return Task.FromResult(exists ? movies[random.Next(movies.Count)].Id : Guid.NewGuid().ToString());
    }
    #endregion
}
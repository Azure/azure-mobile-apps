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
    private readonly TestDbContext context;
    private readonly EntityTableRepository<EntityMovie> innerRepository;
    private readonly IMapper mapper;
    private readonly MappedTableRepository<EntityMovie, MovieDto> repository;

    public MappedTableRepository_Tests(ITestOutputHelper output) : base()
    {
        context = TestDbContext.CreateContext(output);
        EntityTableRepositoryOptions options = new() { DatabaseUpdatesTimestamp = false, DatabaseUpdatesVersion = false };
        innerRepository = new EntityTableRepository<EntityMovie>(context, options);

        mapper = new MapperConfiguration(c => c.AddProfile(new MapperProfile())).CreateMapper();
        repository = new MappedTableRepository<EntityMovie, MovieDto>(mapper, innerRepository);
        Repository = repository;
    }

    protected override MovieDto GetEntity(string id)
        => mapper.Map<MovieDto>(context.Movies.AsNoTracking().SingleOrDefault(m => m.Id == id));

    protected override int GetEntityCount()
        => context.Movies.Count();
    #endregion
}
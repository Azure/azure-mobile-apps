// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using AutoMapper;
using Microsoft.AspNetCore.Datasync.EFCore;

namespace Microsoft.AspNetCore.Datasync.Automapper.Test;

[ExcludeFromCodeCoverage]
public class MappedTableRepository_Tests
{
    #region Test Artifacts
    /// <summary>
    /// A basic movie without any adornment that does not exist in the movie data. Tests must clone this
    /// object and then adjust.
    /// </summary>
    private readonly MovieDto blackPantherMovie = new()
    {
        BestPictureWinner = true,
        Duration = 134,
        Rating = "PG-13",
        ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
        Title = "Black Panther",
        Year = 2018
    };
    #endregion

    /// <summary>
    /// Reference to the <see cref="MovieDbContext"/> full of seeded data.
    /// </summary>
    private readonly MovieDbContext context;

    /// <summary>
    /// Reference to the repository under test.
    /// </summary>
    private readonly EntityTableRepository<EFMovie> entityRepository;

    /// <summary>
    /// Reference to the mapped repository under test
    /// </summary>
    private readonly MappedTableRepository<EFMovie, MovieDto> repository;

    /// <summary>
    /// The mapper to use between EFMovie and MovieDto
    /// </summary>
    private readonly IMapper mapper;

    public MappedTableRepository_Tests()
    {
        context = MovieDbContext.CreateContext();
        entityRepository = new EntityTableRepository<EFMovie>(context);

        var config = new MapperConfiguration(c => { c.AddProfile(new MapperProfile()); });
        mapper = config.CreateMapper();

        repository = new MappedTableRepository<EFMovie, MovieDto>(mapper, entityRepository);
    }

    [Fact]
    public void EntityTableRepository_CanCreate_WithContext()
    {
        Assert.NotNull(repository);
    }

    [Fact]
    public void AsQueryable_Returns_IQueryable()
    {
        var actual = repository.AsQueryable();
        Assert.IsAssignableFrom<IQueryable<MovieDto>>(actual);
        Assert.Equal(Movies.Count, actual.Count());
    }

    [Fact]
    public async Task CreateAsync_CreatesNewEntity_WithSpecifiedId()
    {
        var item = blackPantherMovie;
        item.Id = "movie-blackpanther";

        await repository.CreateAsync(item);

        Assert.Equal<IMovie>(blackPantherMovie, item);
        Assert.Equal("movie-blackpanther", item.Id);
        AssertEx.SystemPropertiesSet(item);
    }

    [Fact]
    public async Task CreateAsync_CreatesNewEntity_WithNullId()
    {
        var item = blackPantherMovie;

        await repository.CreateAsync(item);

        Assert.Equal<IMovie>(blackPantherMovie, item);
        Assert.True(Guid.TryParse(item.Id, out _));
        AssertEx.SystemPropertiesSet(item);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict()
    {
        var id = Movies.GetRandomId();
        var item = blackPantherMovie;
        item.Id = id;

        var ex = await Assert.ThrowsAsync<ConflictException>(() => repository.CreateAsync(item));

        var entity = context.GetMovieById(id);
        Assert.NotSame(entity, ex.Payload);
        Assert.Equal(entity!, (IMovie)ex.Payload);
        Assert.Equal(entity!, (ITableData)ex.Payload);
    }

    [Theory]
    [InlineData("id")]
    public async Task DeleteAsync_Throws_WhenNotFound(string id)
    {
        await Assert.ThrowsAsync<NotFoundException>(() => repository.DeleteAsync(id));
        Assert.Equal(Movies.Count, repository.AsQueryable().Count());
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenVersionMismatch()
    {
        var id = Movies.GetRandomId();
        var version = Guid.NewGuid().ToByteArray();

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(id, version));

        var entity = context.GetMovieById(id);
        Assert.Equal(Movies.Count, repository.AsQueryable().Count());
        Assert.NotSame(entity, ex.Payload);
        Assert.Equal(entity!, (IMovie)ex.Payload);
        Assert.Equal(entity!, (ITableData)ex.Payload);
    }

    [Fact]
    public async Task DeleteAsync_Deletes_WhenVersionMatch()
    {
        var id = Movies.GetRandomId();

        await repository.DeleteAsync(id);

        var entity = context.GetMovieById(id);
        Assert.Null(entity);
    }

    [Fact]
    public async Task DeleteAsync_Deletes_WhenNoVersion()
    {
        var id = Movies.GetRandomId();

        await repository.DeleteAsync(id);

        var entity = context.GetMovieById(id);
        Assert.Null(entity);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenEntityVersionNull()
    {
        var id = Movies.GetRandomId();
        var entity = context.GetMovieById(id);
        var version = Guid.NewGuid().ToByteArray();
        entity!.Version = null;

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(id, version));

        entity = context.GetMovieById(id);
        Assert.NotNull(entity);
        Assert.NotNull(ex.Payload);
        Assert.NotSame(entity, ex.Payload);
    }

    [Fact]
    public async Task ReadAsync_ReturnsDisconnectedEntity()
    {
        var id = Movies.GetRandomId();

        var actual = await repository.ReadAsync(id);

        var expected = context.GetMovieById(id);
        Assert.NotSame(expected, actual);
        Assert.Equal<IMovie>(expected!, actual);
        Assert.Equal<ITableData>(expected!, actual);
    }

    [Theory]
    [InlineData("id")]
    public async Task ReadAsync_ReturnsNull_IfMissing(string id)
    {
        var actual = await repository.ReadAsync(id);

        Assert.Null(actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ReplaceAsync_Throws_OnNullId(string id)
    {
        var entity = blackPantherMovie;
        entity.Id = id;

        await Assert.ThrowsAsync<BadRequestException>(() => repository.ReplaceAsync(entity));
    }

    [Theory]
    [InlineData("id")]
    public async Task ReplaceAsync_Throws_OnMissingEntity(string id)
    {
        var entity = blackPantherMovie;
        entity.Id = id;

        await Assert.ThrowsAsync<NotFoundException>(() => repository.ReplaceAsync(entity));
    }

    [Fact]
    public async Task ReplaceAsync_Throws_OnVersionMismatch()
    {
        var entity = blackPantherMovie;
        entity.Id = Movies.GetRandomId();
        var version = Guid.NewGuid().ToByteArray();

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.ReplaceAsync(entity, version));

        var expected = context.GetMovieById(entity.Id);
        Assert.NotSame(expected, ex.Payload);
        Assert.Equal(expected!, (IMovie)ex.Payload);
        Assert.Equal(expected!, (ITableData)ex.Payload);
    }

    [Fact]
    public async Task ReplaceAsync_Replaces_OnVersionMatch()
    {
        var id = Movies.GetRandomId();
        var original = context.GetMovieById(id);
        var entity = blackPantherMovie;
        entity.Id = original!.Id;
        var version = original.Version.ToArray();

        await repository.ReplaceAsync(entity, version);

        var expected = context.GetMovieById(entity.Id);
        Assert.NotSame(expected, entity);
        Assert.Equal<IMovie>(expected!, entity);
        AssertEx.SystemPropertiesChanged(original, entity);
    }

    [Fact]
    public async Task ReplaceAsync_Replaces_OnNoVersion()
    {
        var entity = blackPantherMovie;
        entity.Id = Movies.GetRandomId();
        var original = (context.GetMovieById(entity.Id)!).Clone();

        await repository.ReplaceAsync(entity);

        var expected = context.GetMovieById(entity.Id);
        Assert.NotSame(expected, entity);
        Assert.Equal<IMovie>(expected!, entity);
        AssertEx.SystemPropertiesChanged(original, entity);
    }

    [Fact]
    public async Task ReplaceAsync_Throws_WhenEntityVersionNull()
    {
        var replacement = blackPantherMovie;
        replacement.Id = Movies.GetRandomId();
        var original = context.GetMovieById(replacement.Id);
        original!.Version = null;
        var version = Guid.NewGuid().ToByteArray();

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.ReplaceAsync(replacement, version));

        Assert.NotSame(original, ex.Payload);
        Assert.Equal(original, (IMovie)ex.Payload);
    }

    [Fact]
    public async Task ReplaceAsync_Throws_OnDbError()
    {
        var entity = blackPantherMovie;
        entity.Id = Movies.GetRandomId();
        context.Connection!.Close(); // Force an error

        var ex = await Assert.ThrowsAsync<RepositoryException>(() => repository.ReplaceAsync(entity));

        Assert.NotNull(ex.InnerException);
    }
}
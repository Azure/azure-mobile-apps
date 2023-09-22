// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.InMemory.Test;

[ExcludeFromCodeCoverage]
public class InMemoryRepository_Tests
{
    #region Test Artifacts
    /// <summary>
    /// A basic movie without any adornment that does not exist in the movie data. Tests must clone this
    /// object and then adjust.
    /// </summary>
    private readonly InMemoryMovie blackPantherMovie = new()
    {
        BestPictureWinner = true,
        Duration = 134,
        Rating = "PG-13",
        ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
        Title = "Black Panther",
        Year = 2018
    };

    private readonly InMemoryRepository<InMemoryMovie> repository;
    #endregion

    public InMemoryRepository_Tests()
    {
        repository = new InMemoryRepository<InMemoryMovie>(Movies.OfType<InMemoryMovie>());
    }

    [Fact]
    public void Ctor_Empty()
    {
        var repository = new InMemoryRepository<InMemoryMovie>();

        Assert.NotNull(repository);
        Assert.NotNull(repository.Entities);
        Assert.Empty(repository.Entities);
    }

    [Fact]
    public void Ctor_Seeded()
    {
        Assert.NotNull(repository);
        Assert.NotNull(repository.Entities);
        Assert.NotEmpty(repository.Entities);
    }

    [Fact]
    public void AsQueryable_ReturnsQueryable()
    {
        Assert.IsAssignableFrom<IQueryable<InMemoryMovie>>(repository.AsQueryable());
    }

    [Fact]
    public void AsQueryable_CanThrow()
    {
        repository.ThrowException = new ApplicationException();
        Assert.Throws<ApplicationException>(() => repository.AsQueryable());
    }

    [Fact]
    public async void AsQueryableAsync_ReturnsQueryable()
    {
        Assert.IsAssignableFrom<IQueryable<InMemoryMovie>>(await repository.AsQueryableAsync());
    }

    [Fact]
    public void AsQueryable_CanRetrieveSingleItems()
    {
        var id = Movies.GetRandomId();

        var actual = repository.AsQueryable().Single(m => m.Id == id);

        var expected = repository.GetEntity(id);
        Assert.Equal<IMovie>(expected, actual);
        Assert.Equal<ITableData>(expected, actual);
    }

    [Fact]
    public void AsQueryable_CanRetrieveFilteredLists()
    {
        var ratedMovies = repository.AsQueryable().Where(m => m.Rating == "R").ToList();
        Assert.Equal(95, ratedMovies.Count);
    }

    [Fact]
    public async Task CreateAsync_Throws_OnForcedException()
    {
        repository.ThrowException = new DatasyncException("test exception");
        await Assert.ThrowsAsync<DatasyncException>(() => repository.CreateAsync(blackPantherMovie));
    }

    [Fact]
    public async Task CreateAsync_Throws_OnNullEntity()
    {
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(null));
        Assert.Equal("entity", ex.ParamName);
    }

    [Fact]
    public async Task CreateAsync_CreatesNewEntity_WithSpecifiedId()
    {
        var item = blackPantherMovie.Clone();
        item.Id = "movie-blackpanther";

        await repository.CreateAsync(item);

        Assert.Equal<IMovie>(blackPantherMovie, item);
        Assert.Equal("movie-blackpanther", item.Id);
        AssertEx.SystemPropertiesSet(item);
    }

    [Fact]
    public async Task CreateAsync_CreatesNewEntity_WithNullId()
    {
        var item = blackPantherMovie.Clone();

        await repository.CreateAsync(item);

        Assert.Equal<IMovie>(blackPantherMovie, item);
        Assert.True(Guid.TryParse(item.Id, out _));
        AssertEx.SystemPropertiesSet(item);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict()
    {
        var id = Movies.GetRandomId();
        var item = blackPantherMovie.Clone();
        item.Id = id;

        var ex = await Assert.ThrowsAsync<ConflictException>(() => repository.CreateAsync(item));

        var entity = repository.GetEntity(id);
        Assert.NotSame(entity, ex.Payload);
        Assert.Equal<IMovie>(entity, ex.Payload as IMovie);
        Assert.Equal<ITableData>(entity, ex.Payload as ITableData);
    }

    [Fact]
    public async Task CreateAsync_UpdatesUpdatedAt()
    {
        var item = blackPantherMovie.Clone();
        item.UpdatedAt = DateTimeOffset.UtcNow.AddMonths(-1);

        await repository.CreateAsync(item);

        Assert.Equal<IMovie>(blackPantherMovie, item);
        AssertEx.SystemPropertiesSet(item);
    }

    [Fact]
    public async Task CreateAsync_UpdatesVersion()
    {
        var item = blackPantherMovie.Clone();
        var version = Guid.NewGuid().ToByteArray();
        item.Version = version.ToArray();

        await repository.CreateAsync(item);

        Assert.Equal<IMovie>(blackPantherMovie, item);
        AssertEx.SystemPropertiesSet(item);
        Assert.False(item.Version.SequenceEqual(version));
    }

    [Fact]
    public async Task CreateAsync_StoresDisconnectedEntity()
    {
        var item = blackPantherMovie.Clone();
        item.Id = "movie-blackpanther";

        await repository.CreateAsync(item);

        var entity = repository.GetEntity(item.Id);
        Assert.NotSame(entity, item);
    }

    [Fact]
    public async Task DeleteAsync_Throws_OnForcedException()
    {
        repository.ThrowException = new DatasyncException("test exception");
        var id = Movies.GetRandomId();
        await Assert.ThrowsAsync<DatasyncException>(() => repository.DeleteAsync(id));
    }

    [Fact]
    public async Task DeleteAsync_Throws_OnNullId()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => repository.DeleteAsync(null));
        Assert.Equal(Movies.Count, repository.Entities.Count);
    }

    [Fact]
    public async Task DeleteAsync_Throws_OnEmptyId()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => repository.DeleteAsync(""));
        Assert.Equal(Movies.Count, repository.Entities.Count);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("id-0000")]
    [InlineData("id-000 is super long")]
    [InlineData("id-300")]
    public async Task DeleteAsync_Throws_WhenNotFound(string id)
    {
        await Assert.ThrowsAsync<NotFoundException>(() => repository.DeleteAsync(id));
        Assert.Equal(Movies.Count, repository.Entities.Count);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenVersionMismatch()
    {
        var id = Movies.GetRandomId();
        var version = Guid.NewGuid().ToByteArray();

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(id, version));

        var entity = repository.GetEntity(id);
        Assert.Equal(Movies.Count, repository.Entities.Count);
        Assert.NotSame(entity, ex.Payload);
        Assert.Equal<IMovie>(entity, ex.Payload as IMovie);
        Assert.Equal<ITableData>(entity, ex.Payload as ITableData);
    }

    [Fact]
    public async Task DeleteAsync_Deletes_WhenVersionMatch()
    {
        var id = Movies.GetRandomId();

        await repository.DeleteAsync(id);

        var entity = repository.GetEntity(id);
        Assert.Null(entity);
    }

    [Fact]
    public async Task DeleteAsync_Deletes_WhenNoVersion()
    {
        var id = Movies.GetRandomId();

        await repository.DeleteAsync(id);

        var entity = repository.GetEntity(id);
        Assert.Null(entity);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenEntityVersionNull()
    {
        var id = Movies.GetRandomId();
        var entity = repository.GetEntity(id);
        var version = Guid.NewGuid().ToByteArray();
        entity.Version = null;

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(id, version));

        entity = repository.GetEntity(id);
        Assert.NotNull(entity);
        Assert.NotNull(ex.Payload);
        Assert.NotSame(entity, ex.Payload);
    }

    [Fact]
    public async Task ReadAsync_Throws_OnForcedException()
    {
        repository.ThrowException = new DatasyncException("test exception");
        var id = Movies.GetRandomId();
        await Assert.ThrowsAsync<DatasyncException>(() => repository.ReadAsync(id));
    }

    [Fact]
    public async Task ReadAsync_ReturnsDisconnectedEntity()
    {
        var id = Movies.GetRandomId();

        var actual = await repository.ReadAsync(id);

        var expected = repository.GetEntity(id);
        Assert.NotSame(expected, actual);
        Assert.Equal<IMovie>(expected, actual);
        Assert.Equal<ITableData>(expected, actual);
    }

    [Fact]
    public async Task ReadAsync_Throws_OnNullId()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => repository.ReadAsync(null));
    }

    [Fact]
    public async Task ReadAsync_Throws_OnEmptyId()
    {
         await Assert.ThrowsAsync<BadRequestException>(() => repository.ReadAsync(""));
    }

    [Theory]
    [InlineData("id")]
    [InlineData("id-0000")]
    [InlineData("id-000 is super long")]
    [InlineData("id-300")]
    public async Task ReadAsync_ReturnsNull_IfMissing(string id)
    {
        var actual = await repository.ReadAsync(id);

        Assert.Null(actual);
    }

    [Fact]
    public async Task ReplaceAsync_Throws_OnForcedException()
    {
        repository.ThrowException = new DatasyncException("test exception");
        var item = blackPantherMovie.Clone();
        item.Id = Movies.GetRandomId();
        await Assert.ThrowsAsync<DatasyncException>(() => repository.ReplaceAsync(item));
    }

    [Fact]
    public async Task ReplaceAsync_Throws_OnNullEntity()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.ReplaceAsync(null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ReplaceAsync_Throws_OnNullId(string id)
    {
        var entity = blackPantherMovie.Clone();
        entity.Id = id;

        await Assert.ThrowsAsync<BadRequestException>(() => repository.ReplaceAsync(entity));
    }

    [Theory]
    [InlineData("id")]
    [InlineData("id-0000")]
    [InlineData("id-000 is super long")]
    [InlineData("id-300")]
    public async Task ReplaceAsync_Throws_OnMissingEntity(string id)
    {
        var entity = blackPantherMovie.Clone();
        entity.Id = id;

        await Assert.ThrowsAsync<NotFoundException>(() => repository.ReplaceAsync(entity));
    }

    [Fact]
    public async Task ReplaceAsync_Throws_OnVersionMismatch()
    {
        var entity = blackPantherMovie.Clone();
        entity.Id = Movies.GetRandomId();
        var version = Guid.NewGuid().ToByteArray();

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.ReplaceAsync(entity, version));

        var expected = repository.GetEntity(entity.Id);
        Assert.NotSame(expected, ex.Payload);
        Assert.Equal<IMovie>(expected, ex.Payload as IMovie);
        Assert.Equal<ITableData>(expected, ex.Payload as ITableData);
    }

    [Fact]
    public async Task ReplaceAsync_Replaces_OnVersionMatch()
    {
        var original = repository.GetEntity(Movies.GetRandomId()).Clone();
        var entity = blackPantherMovie.Clone();
        entity.Id = original.Id;
        var version = original.Version.ToArray();

        await repository.ReplaceAsync(entity, version);

        var expected = repository.GetEntity(entity.Id);
        Assert.NotSame(expected, entity);
        Assert.Equal<IMovie>(expected, entity);
        AssertEx.SystemPropertiesChanged(original, entity);
    }

    [Fact]
    public async Task ReplaceAsync_Replaces_OnNoVersion()
    {
        var entity = blackPantherMovie.Clone();
        entity.Id = Movies.GetRandomId();
        var original = repository.GetEntity(entity.Id).Clone();

        await repository.ReplaceAsync(entity);

        var expected = repository.GetEntity(entity.Id);
        Assert.NotSame(expected, entity);
        Assert.Equal<IMovie>(expected, entity);
        AssertEx.SystemPropertiesChanged(original, entity);
    }

    [Fact]
    public async Task ReplaceAsync_Throws_WhenEntityVersionNull()
    {
        var replacement = blackPantherMovie.Clone();
        replacement.Id = Movies.GetRandomId();
        var original = repository.GetEntity(replacement.Id);
        original.Version = null;
        var version = Guid.NewGuid().ToByteArray();

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.ReplaceAsync(replacement, version));

        Assert.NotSame(original, ex.Payload);
        Assert.Equal<IMovie>(original, ex.Payload as IMovie);
    }
}

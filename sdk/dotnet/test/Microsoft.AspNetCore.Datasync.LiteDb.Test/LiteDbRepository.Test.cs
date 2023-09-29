// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using LiteDB;

namespace Microsoft.AspNetCore.Datasync.LiteDb.Test;

[ExcludeFromCodeCoverage]
public class LiteDbRepository_Tests : IDisposable
{
    #region Test Artifacts
    /// <summary>
    /// A basic movie without any adornment that does not exist in the movie data. Tests must clone this
    /// object and then adjust.
    /// </summary>
    private readonly LiteDbMovie blackPantherMovie = new()
    {
        BestPictureWinner = true,
        Duration = 134,
        Rating = "PG-13",
        ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
        Title = "Black Panther",
        Year = 2018
    };

    private readonly string dbFilename;
    private readonly LiteDatabase database;
    private readonly ILiteCollection<LiteDbMovie> collection;
    private readonly LiteDbRepository<LiteDbMovie> repository;
    #endregion

    public LiteDbRepository_Tests()
    {
        dbFilename = Path.GetTempFileName();
        database = new LiteDatabase($"Filename={dbFilename};Connection=direct;InitialSize=0");

        collection = database.GetCollection<LiteDbMovie>("litedbmovies");
        foreach (var movie in Movies.OfType<LiteDbMovie>())
        {
            movie.UpdatedAt = DateTimeOffset.Now;
            movie.Version = Guid.NewGuid().ToByteArray();
            collection.Insert(movie);
        }

        repository = new LiteDbRepository<LiteDbMovie>(database);
    }

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Test Case - no inherited classes")]
    public void Dispose()
    {
        database.Dispose();
        File.Delete(dbFilename);
    }

    [Fact]
    public void Ctor_NullConnection_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new LiteDbRepository<LiteDbMovie>(null));
    }

    [Fact]
    public void AsQueryable_ReturnsQueryable()
    {
        Assert.IsAssignableFrom<IQueryable<LiteDbMovie>>(repository.AsQueryable());
    }

    [Fact]
    public void AsQueryable_CanRetrieveSingleItems()
    {
        var id = Movies.GetRandomId();

        var actual = repository.AsQueryable().Single(m => m.Id == id);
        var expected = collection.FindById(id);

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
    public async Task CreateAsync_Throws_Null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(null));
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

        var entity = collection.FindById(id);
        Assert.NotSame(entity, ex.Payload);
        Assert.Equal(entity, (IMovie)ex.Payload);
        Assert.Equal(entity, (ITableData)ex.Payload);
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

        var entity = collection.FindById(item.Id);
        Assert.NotSame(entity, item);
    }

    [Fact]
    public async Task DeleteAsync_Throws_OnNullId()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => repository.DeleteAsync(null));
        Assert.Equal(Movies.Count, collection.Count());
    }

    [Fact]
    public async Task DeleteAsync_Throws_OnEmptyId()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => repository.DeleteAsync(""));
        Assert.Equal(Movies.Count, collection.Count());
    }

    [Theory]
    [InlineData("id")]
    [InlineData("id-0000")]
    [InlineData("id-000 is super long")]
    [InlineData("id-300")]
    public async Task DeleteAsync_Throws_WhenNotFound(string id)
    {
        await Assert.ThrowsAsync<NotFoundException>(() => repository.DeleteAsync(id));
        Assert.Equal(Movies.Count, collection.Count());
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenVersionMismatch()
    {
        var id = Movies.GetRandomId();
        var version = Guid.NewGuid().ToByteArray();

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(id, version));

        var entity = collection.FindById(id);
        Assert.Equal(Movies.Count, collection.Count());
        Assert.NotSame(entity, ex.Payload);
        Assert.Equal(entity, (IMovie)ex.Payload);
        Assert.Equal(entity, (ITableData)ex.Payload);
    }

    [Fact]
    public async Task DeleteAsync_Deletes_WhenVersionMatch()
    {
        var id = Movies.GetRandomId();

        await repository.DeleteAsync(id);

        var entity = collection.FindById(id);
        Assert.Null(entity);
    }

    [Fact]
    public async Task DeleteAsync_Deletes_WhenNoVersion()
    {
        var id = Movies.GetRandomId();

        await repository.DeleteAsync(id);

        var entity = collection.FindById(id);
        Assert.Null(entity);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenEntityVersionNull()
    {
        var id = Movies.GetRandomId();
        var entity = collection.FindById(id);
        var version = Guid.NewGuid().ToByteArray();
        entity.Version = null;

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(id, version));

        entity = collection.FindById(id);
        Assert.NotNull(entity);
        Assert.NotNull(ex.Payload);
        Assert.NotSame(entity, ex.Payload);
    }

    [Fact]
    public async Task ReadAsync_ReturnsDisconnectedEntity()
    {
        var id = Movies.GetRandomId();

        var actual = await repository.ReadAsync(id);

        var expected = collection.FindById(id);
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
    public async Task ReplaceAsync_Throws_OnNull()
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

        var expected = collection.FindById(entity.Id);
        Assert.NotSame(expected, ex.Payload);
        Assert.Equal(expected, (IMovie)ex.Payload);
        Assert.Equal(expected, (ITableData)ex.Payload);
    }

    [Fact]
    public async Task ReplaceAsync_Replaces_OnVersionMatch()
    {
        var original = collection.FindById(Movies.GetRandomId()).Clone();
        var entity = blackPantherMovie.Clone();
        entity.Id = original.Id;
        var version = original.Version.ToArray();

        await repository.ReplaceAsync(entity, version);

        var expected = collection.FindById(entity.Id);
        Assert.NotSame(expected, entity);
        Assert.Equal<IMovie>(expected, entity);
        AssertEx.SystemPropertiesChanged(original, entity);
    }

    [Fact]
    public async Task ReplaceAsync_Replaces_OnNoVersion()
    {
        var entity = blackPantherMovie.Clone();
        entity.Id = Movies.GetRandomId();
        var original = collection.FindById(entity.Id).Clone();

        await repository.ReplaceAsync(entity);

        var expected = collection.FindById(entity.Id);
        Assert.NotSame(expected, entity);
        Assert.Equal<IMovie>(expected, entity);
        AssertEx.SystemPropertiesChanged(original, entity);
    }

    [Fact]
    public async Task ReplaceAsync_Throws_WhenEntityVersionNull()
    {
        var replacement = blackPantherMovie.Clone();
        replacement.Id = Movies.GetRandomId();
        var original = collection.FindById(replacement.Id);
        original.Version = null;
        var version = Guid.NewGuid().ToByteArray();

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.ReplaceAsync(replacement, version));

        Assert.NotSame(original, ex.Payload);
        Assert.Equal(original, ex.Payload as IMovie);
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    public void PreconditionFailed_Works(bool v1IsNull, bool v2IsNull, bool expected)
    {
        byte[] v1 = v1IsNull ? null : new byte[] { 0x0A, 0x0B, 0x0C };
        byte[] v2 = v2IsNull ? null : new byte[] { 0x0A, 0x0B, 0x0C };

        Assert.Equal(expected, LiteDbRepository<LiteDbMovie>.PreconditionFailed(v1, v2));
    }
}

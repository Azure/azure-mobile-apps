// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.LiteDb;

namespace Microsoft.AspNetCore.Datasync.EFCore.Test;

[ExcludeFromCodeCoverage]
public class EntityTableRepository_Tests
{
    #region Test Artifacts
    /// <summary>
    /// A basic movie without any adornment that does not exist in the movie data. Tests must clone this
    /// object and then adjust.
    /// </summary>
    private readonly EFMovie blackPantherMovie = new()
    {
        BestPictureWinner = true,
        Duration = 134,
        Rating = "PG-13",
        ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
        Title = "Black Panther",
        Year = 2018
    };

    internal class NotEntityModel : ITableData
    {
        public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public byte[] Version { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTimeOffset UpdatedAt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Deleted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Equals(ITableData other)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    /// <summary>
    /// Reference to the <see cref="MovieDbContext"/> full of seeded data.
    /// </summary>
    private readonly MovieDbContext context;

    /// <summary>
    /// Reference to the repository under test.
    /// </summary>
    private readonly EntityTableRepository<EFMovie> repository;

    public EntityTableRepository_Tests()
    {
        context = MovieDbContext.CreateContext();
        repository = new EntityTableRepository<EFMovie>(context);
    }

    [Fact]
    public void EntityTableRepository_CanCreate_WithContext()
    {
        Assert.NotNull(repository);
    }

    [Fact]
    public void EntityTableRepository_Throws_WithNullContext()
    {
        Assert.Throws<ArgumentNullException>(() => new EntityTableRepository<EFMovie>(null));
    }

    [Fact]
    public void EntityTableRepository_Throws_WithMissingSet()
    {
        Assert.Throws<ArgumentException>(() => new EntityTableRepository<EFError>(context));
    }

    [Fact]
    public void EntityTableRepository_Allows_ETagEntityTableData_GenericParam()
    {
        var sut = new EntityTableRepository<ETagModel>(context);
        Assert.NotNull(sut);
    }

    [Fact]
    public void EntityTableRepository_Throws_OnNonSupported_GenericParam()
    {
        Assert.Throws<InvalidCastException>(() => new EntityTableRepository<NotEntityModel>(context));
    }

    [Fact]
    public void AsQueryable_Returns_IQueryable()
    {
        var actual = repository.AsQueryable();
        Assert.IsAssignableFrom<IQueryable<EFMovie>>(actual);
        Assert.Equal(Movies.Count, actual.Count());
    }

    [Fact]
    public async void AsQueryableAsync_Returns_IQueryable()
    {
        var actual = await repository.AsQueryableAsync();
        Assert.IsAssignableFrom<IQueryable<EFMovie>>(actual);
        Assert.Equal(Movies.Count, actual.Count());
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

        var entity = context.GetMovieById(id);
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
    public async Task CreateAsync_Throws_OnDbError()
    {
        var item = blackPantherMovie.Clone();
        var version = Guid.NewGuid().ToByteArray();
        item.Version = version.ToArray();
        context.Connection.Close(); // Force an error

        var ex = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateAsync(item));

        Assert.NotNull(ex.InnerException);
    }

    [Fact]
    public async Task DeleteAsync_Throws_OnNullId()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => repository.DeleteAsync(null));
        Assert.Equal(Movies.Count, repository.AsQueryable().Count());
    }

    [Fact]
    public async Task DeleteAsync_Throws_OnEmptyId()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => repository.DeleteAsync(""));
        Assert.Equal(Movies.Count, repository.AsQueryable().Count());
    }

    [Theory]
    [InlineData("id")]
    [InlineData("id-0000")]
    [InlineData("id-000 is super long")]
    [InlineData("id-300")]
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
        Assert.Equal<IMovie>(entity, ex.Payload as IMovie);
        Assert.Equal<ITableData>(entity, ex.Payload as ITableData);
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
        entity.Version = null;

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(id, version));

        entity = context.GetMovieById(id);
        Assert.NotNull(entity);
        Assert.NotNull(ex.Payload);
        Assert.NotSame(entity, ex.Payload);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenUpdateError()
    {
        var id = Movies.GetRandomId();
        context.Connection.Close(); // Force a database error

        var ex = await Assert.ThrowsAsync<RepositoryException>(() => repository.DeleteAsync(id));

        Assert.NotNull(ex.InnerException);
    }

    [Fact]
    public async Task ReadAsync_ReturnsDisconnectedEntity()
    {
        var id = Movies.GetRandomId();

        var actual = await repository.ReadAsync(id);

        var expected = context.GetMovieById(id);
        Assert.NotSame(expected, actual);
        Assert.Equal<IMovie>(expected, actual);
        Assert.Equal<ITableData>(expected, actual);
    }

    [Fact]
    public async Task ReadAsync_Throws_OnNullId()
    {
        _ = await Assert.ThrowsAsync<BadRequestException>(() => repository.ReadAsync(null));
    }

    [Fact]
    public async Task ReadAsync_Throws_OnEmptyId()
    {
        _ = await Assert.ThrowsAsync<BadRequestException>(() => repository.ReadAsync(""));
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

        var expected = context.GetMovieById(entity.Id);
        Assert.NotSame(expected, ex.Payload);
        Assert.Equal<IMovie>(expected, ex.Payload as IMovie);
        Assert.Equal<ITableData>(expected, ex.Payload as ITableData);
    }

    [Fact]
    public async Task ReplaceAsync_Replaces_OnVersionMatch()
    {
        var id = Movies.GetRandomId();
        var original = context.GetMovieById(id);
        var entity = blackPantherMovie.Clone();
        entity.Id = original.Id;
        var version = original.Version.ToArray();

        await repository.ReplaceAsync(entity, version);

        var expected = context.GetMovieById(entity.Id);
        Assert.NotSame(expected, entity);
        Assert.Equal<IMovie>(expected, entity);
        AssertEx.SystemPropertiesChanged(original, entity);
    }

    [Fact]
    public async Task ReplaceAsync_Replaces_OnNoVersion()
    {
        var entity = blackPantherMovie.Clone();
        entity.Id = Movies.GetRandomId();
        var original = (context.GetMovieById(entity.Id)).Clone();

        await repository.ReplaceAsync(entity);

        var expected = context.GetMovieById(entity.Id);
        Assert.NotSame(expected, entity);
        Assert.Equal<IMovie>(expected, entity);
        AssertEx.SystemPropertiesChanged(original, entity);
    }

    [Fact]
    public async Task ReplaceAsync_Throws_WhenEntityVersionNull()
    {
        var replacement = blackPantherMovie.Clone();
        replacement.Id = Movies.GetRandomId();
        var original = context.GetMovieById(replacement.Id);
        original.Version = null;
        var version = Guid.NewGuid().ToByteArray();

        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.ReplaceAsync(replacement, version));

        Assert.NotSame(original, ex.Payload);
        Assert.Equal<IMovie>(original, ex.Payload as IMovie);
    }

    [Fact]
    public async Task ReplaceAsync_Throws_OnDbError()
    {
        var entity = blackPantherMovie.Clone();
        entity.Id = Movies.GetRandomId();
        context.Connection.Close(); // Force an error

        var ex = await Assert.ThrowsAsync<RepositoryException>(() => repository.ReplaceAsync(entity));

        Assert.NotNull(ex.InnerException);
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    public void PreconditionFailed_Works(bool v1IsNull, bool v2IsNull, bool expected)
    {
        byte[] v1 = v1IsNull ? null : new byte[] { 0x0A, 0x0B, 0x0C };
        byte[] v2 = v2IsNull ? null : new byte[] { 0x0A, 0x0B, 0x0C };

        Assert.Equal(expected, EntityTableRepository<EFMovie>.PreconditionFailed(v1, v2));
    }
}

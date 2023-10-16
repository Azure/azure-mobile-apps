// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.CosmosDb.Test.Helpers;

namespace Microsoft.AspNetCore.Datasync.CosmosDb.Test;

[ExcludeFromCodeCoverage]
public class CosmosTableRepository_Tests : IDisposable
{
    private readonly CosmosMovie blackPantherMovie = new()
    {
        BestPictureWinner = true,
        Duration = 134,
        Rating = "PG-13",
        ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
        Title = "Black Panther",
        Year = 2018
    };
    private Container movieContainer;
    private CosmosTableRepository<CosmosMovie> repository;

    public CosmosTableRepository_Tests()
    {
        movieContainer = CosmosDbHelper.GetContainer().Result;
        repository = new(movieContainer);
    }

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Test Case - no inherited classes")]
    public void Dispose()
    {
        movieContainer.Database.DeleteAsync().Wait();
    }

    // TODO multi-partition key tests with doubles and bools for keys

    [Fact]
    public void CosmosTableRepository_CanCreate_WithContainer()
    {
        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void CosmosTableRepository_Throws_WithNullContainer()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new CosmosTableRepository<CosmosMovie>(null));
    }

    // TODO are these applicable to Cosmos?
    //[Fact]
    //public void CosmosTableRepository_Throws_WithMissingSet()
    //{
    //    Assert.Throws<ArgumentException>(() => new EntityTableRepository<EFError>(context));
    //}

    //[Fact]
    //public void CosmosTableRepository_Allows_ETagEntityTableData_GenericParam()
    //{
    //    var sut = new EntityTableRepository<ETagModel>(context);
    //    Assert.NotNull(sut);
    //}

    //[Fact]
    //public void CosmosTableRepository_Throws_OnNonSupported_GenericParam()
    //{
    //    Assert.Throws<InvalidCastException>(() => new EntityTableRepository<NotEntityModel>(context));
    //}

    [Fact]
    public void AsQueryable_Returns_IQueryable()
    {
        // Act
        var actual = repository.AsQueryable();
        
        // Assert
        Assert.IsAssignableFrom<IQueryable<CosmosMovie>>(actual);
        Assert.Equal(Movies.Count, actual.Count());
    }

    [Fact]
    public async void AsQueryableAsync_Returns_IQueryable()
    {
        // Act
        var actual = await repository.AsQueryableAsync();
       
        // Assert
        Assert.IsAssignableFrom<IQueryable<CosmosMovie>>(actual);
        Assert.Equal(Movies.Count, actual.Count());
    }

    [Fact]
    public async Task AsQueryable_CanRetrieveSingleItems()
    {
        // Arrange
        var id = Movies.GetRandomId();

        // Act
        var actual = repository.AsQueryable().Single(m => m.Id == id);
        CosmosMovie expected = await movieContainer.ReadItemAsync<CosmosMovie>(id, new PartitionKey(id));

        // Assert
        Assert.Equal<IMovie>(expected, actual);
        Assert.Equal<ITableData>(expected, actual);
    }

    [Fact]
    public void AsQueryable_CanRetrieveFilteredLists()
    {
        // Act
        var ratedMovies = repository.AsQueryable().Where(m => m.Rating == "R").ToList();

        // Assert
        Assert.Equal(95, ratedMovies.Count);
    }

    [Fact]
    public async Task CreateAsync_CreatesNewEntity_WithNullId()
    {
        // Arrange
        var item = blackPantherMovie.Clone();

        // Act
        await repository.CreateAsync(item);

        // Assert
        Assert.Equal<IMovie>(blackPantherMovie, item);
        Assert.True(Guid.TryParse(item.Id, out _));
        AssertEx.SystemPropertiesSet(item);

    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict()
    {
        // Arrange
        var id = Movies.GetRandomId();
        var item = blackPantherMovie.Clone();
        item.Id = id;

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => repository.CreateAsync(item));
        
        CosmosMovie entity = await movieContainer.ReadItemAsync<CosmosMovie>(id, new PartitionKey(item.Id));
        Assert.NotSame(entity, ex.Payload);
        Assert.Equal(entity, ex.Payload as IMovie);
        Assert.Equal(entity, ex.Payload as ITableData);
    }

    [Fact]
    public async Task CreateAsync_UpdatesUpdatedAt()
    {
        // Arrange
        var item = blackPantherMovie.Clone();
        item.UpdatedAt = DateTimeOffset.UtcNow.AddMonths(-1);

        // Act
        await repository.CreateAsync(item);

        // Assert
        Assert.Equal<IMovie>(blackPantherMovie, item);
        AssertEx.SystemPropertiesSet(item);
    }

    [Fact]
    public async Task CreateAsync_UpdatesVersion()
    {
        // Arrange
        var item = blackPantherMovie.Clone();
        var version = Guid.NewGuid().ToByteArray();
        item.Version = version.ToArray();

        // Act
        await repository.CreateAsync(item);

        // Assert
        Assert.Equal<IMovie>(blackPantherMovie, item);
        AssertEx.SystemPropertiesSet(item);
        Assert.False(item.Version.SequenceEqual(version));
    }

    // TODO is this applicable to Cosmos?
    //[Fact]
    //public async Task CreateAsync_Throws_OnDbError()
    //{
    //    // Arrange
    //    await Initialize();
    //    var item = blackPantherMovie.Clone();
    //    var version = Guid.NewGuid().ToByteArray();
    //    item.Version = version.ToArray();
    //    context.Connection.Close(); // Force an error

    //    // Act & Assert
    //    var ex = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateAsync(item));

    //    Assert.NotNull(ex.InnerException);
    //}

    [Fact]
    public async Task DeleteAsync_Deletes_WhenNoVersion()
    {
        // Arrange
        var movie = Movies.GetRandomMovie<CosmosMovie>();

        // Act
        await repository.DeleteAsync(movie.Id);

        // Assert
        var ex = await Assert.ThrowsAsync<CosmosException>(() => movieContainer.ReadItemAsync<CosmosMovie>(movie.Id, new(movie.Id)));
        Assert.Equal(System.Net.HttpStatusCode.NotFound, ex.StatusCode);
    }

    // TODO is this applicable to Cosmos?
    //[Fact]
    //public async Task DeleteAsync_Throws_WhenEntityVersionNull()
    //{
    //    // Arrange
    //    var id = Movies.GetRandomId();
    //    CosmosMovie entity = await movieContainer.ReadItemAsync<CosmosMovie>(id, new PartitionKey(id));
    //    var version = Guid.NewGuid().ToByteArray();
    //    entity.Version = null;

    //    // Act & Assert
    //    var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(id, version));

    //    entity = await movieContainer.ReadItemAsync<CosmosMovie>(id, new PartitionKey(id));
    //    Assert.NotNull(entity);
    //    Assert.NotNull(ex.Payload);
    //    Assert.NotSame(entity, ex.Payload);
    //}

    [Fact]
    public async Task DeleteAsync_Throws_WhenEntityVersionsDiffer()
    {
        // Arrange
        var movie = Movies.GetRandomMovie<CosmosMovie>();
        var version = Guid.NewGuid().ToByteArray();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(movie.Id, version));

        // Assert
        var entity = await movieContainer.ReadItemAsync<CosmosMovie>(movie.Id, new PartitionKey(movie.Id));
        Assert.NotNull(entity);
        Assert.NotNull(ex.Payload);
        Assert.NotSame(entity, ex.Payload);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenUpdateError()
    {
        // Arrange
        var id = Movies.GetRandomId();
        await movieContainer.DeleteContainerAsync(); // Force a database error

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RepositoryException>(() => repository.DeleteAsync(id));

        Assert.NotNull(ex.InnerException);
    }

    [Fact]
    public async Task ReadAsync_ReturnsDisconnectedEntity()
    {
        // Arrange
        var movie = Movies.GetRandomMovie<CosmosMovie>();

        // Act
        var actual = await repository.ReadAsync(movie.Id);

        // Assert
        CosmosMovie expected = await movieContainer.ReadItemAsync<CosmosMovie>(movie.Id, new PartitionKey(movie.Id));
        Assert.NotSame(expected, actual);
        Assert.Equal<IMovie>(expected, actual);
        Assert.Equal<ITableData>(expected, actual);
    }

    [Fact]
    public async Task ReadAsync_Throws_OnNullId()
    {
        // Act & Assert
        _ = await Assert.ThrowsAsync<BadRequestException>(() => repository.ReadAsync(null));
    }

    [Fact]
    public async Task ReadAsync_Throws_OnEmptyId()
    {
        // Act & Assert
        _ = await Assert.ThrowsAsync<BadRequestException>(() => repository.ReadAsync(""));
    }

    [Theory]
    [InlineData("id")]
    [InlineData("id-0000")]
    [InlineData("id-000 is super long")]
    [InlineData("id-300")]
    public async Task ReadAsync_ReturnsNull_IfMissing(string id)
    {
        // Act
        var actual = await repository.ReadAsync(id);

        Assert.Null(actual);
    }

    [Fact]
    public async Task ReplaceAsync_Throws_OnNullEntity()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.ReplaceAsync(null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ReplaceAsync_Throws_OnNullId(string id)
    {
        // Arrange
        var entity = blackPantherMovie.Clone();
        entity.Id = id;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => repository.ReplaceAsync(entity));
    }

    [Theory]
    [InlineData("id")]
    [InlineData("id-0000")]
    [InlineData("id-000 is super long")]
    [InlineData("id-300")]
    public async Task ReplaceAsync_Throws_OnMissingEntity(string id)
    {
        // Arrange
        var entity = blackPantherMovie.Clone();
        entity.Id = id;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => repository.ReplaceAsync(entity));
    }

    [Fact]
    public async Task ReplaceAsync_Throws_OnVersionMismatch()
    {
        // Arrange
        var entity = blackPantherMovie.Clone();
        entity.Id = Movies.GetRandomId();
        var version = Guid.NewGuid().ToByteArray();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.ReplaceAsync(entity, version));

        // Assert
        CosmosMovie expected = await movieContainer.ReadItemAsync<CosmosMovie>(entity.Id, new PartitionKey(entity.Id));
        Assert.NotSame(expected, ex.Payload);
        Assert.Equal(expected, ex.Payload as IMovie);
        Assert.Equal(expected, ex.Payload as ITableData);
    }

    [Fact]
    public async Task ReplaceAsync_Replaces_OnVersionMatch()
    {
        // Arrange
        var id = Movies.GetRandomId();
        CosmosMovie original = await movieContainer.ReadItemAsync<CosmosMovie>(id, new PartitionKey(id));
        var entity = blackPantherMovie.Clone();
        entity.Id = original.Id;
        var version = original.Version.ToArray();

        // Act
        await repository.ReplaceAsync(entity, version);

        // Assert
        CosmosMovie expected = await movieContainer.ReadItemAsync<CosmosMovie>(entity.Id, new PartitionKey(entity.Id));
        Assert.NotSame(expected, entity);
        Assert.Equal<IMovie>(expected, entity);
        AssertEx.SystemPropertiesChanged(original, entity);
    }

    [Fact]
    public async Task ReplaceAsync_Replaces_OnNoVersion()
    {
        // Arrange
        var entity = blackPantherMovie.Clone();
        entity.Id = Movies.GetRandomId();
        CosmosMovie originalFromDatabase = await movieContainer.ReadItemAsync<CosmosMovie>(entity.Id, new PartitionKey(entity.Id));
        var original = originalFromDatabase.Clone();

        // Act
        await repository.ReplaceAsync(entity);

        // Assert
        CosmosMovie expected = await movieContainer.ReadItemAsync<CosmosMovie>(entity.Id, new PartitionKey(entity.Id));
        Assert.NotSame(expected, entity);
        Assert.Equal<IMovie>(expected, entity);
        AssertEx.SystemPropertiesChanged(original, entity);
    }

    // TODO is this applicable to Cosmos?
    //[Fact]
    //public async Task ReplaceAsync_Throws_WhenEntityVersionNull()
    //{
    //    // Arrange
    //    var replacement = blackPantherMovie.Clone();
    //    replacement.Id = Movies.GetRandomId();
    //    CosmosMovie original = await movieContainer.ReadItemAsync<CosmosMovie>(replacement.Id, new PartitionKey(replacement.Id));
    //    original.Version = null;
    //    var version = Guid.NewGuid().ToByteArray();

    //    // Act & Assert
    //    var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.ReplaceAsync(replacement, version));

    //    Assert.NotSame(original, ex.Payload);
    //    Assert.Equal(original, ex.Payload as IMovie);
    //}

    [Fact]
    public async Task ReplaceAsync_Throws_OnDbError()
    {
        // Arrange
        var entity = blackPantherMovie.Clone();
        entity.Id = Movies.GetRandomId();
        await movieContainer.DeleteContainerAsync(); // Force a database error

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RepositoryException>(() => repository.ReplaceAsync(entity));

        Assert.NotNull(ex.InnerException);
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    public void PreconditionFailed_Works(bool v1IsNull, bool v2IsNull, bool expected)
    {
        // Arrange
        byte[] v1 = v1IsNull ? null : new byte[] { 0x0A, 0x0B, 0x0C };
        byte[] v2 = v2IsNull ? null : new byte[] { 0x0A, 0x0B, 0x0C };

        // Act & Assert
        Assert.Equal(expected, CosmosTableRepository<CosmosMovie>.PreconditionFailed(v1, v2));
    }
}

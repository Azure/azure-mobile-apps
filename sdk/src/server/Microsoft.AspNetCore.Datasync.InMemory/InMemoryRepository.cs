// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;

namespace Microsoft.AspNetCore.Datasync.InMemory;

/// <summary>
/// An implementation of an in-memory repository pattern.  This is used during testing the Azure Mobile Apps
/// library and is not recommended for production use.
/// </summary>
/// <typeparam name="TEntity">The type of entity to store.</typeparam>
public class InMemoryRepository<TEntity> : IRepository<TEntity> where TEntity : InMemoryTableData
{
    private readonly ConcurrentDictionary<string, TEntity> _entities = new();

    // We include these to avoid bringing in a new Abstractions package just for four ints.
    private const int Status400BadRequest = 400;
    private const int Status404NotFound = 404;
    private const int Status409Conflict = 409;
    private const int Status412PreconditionFailed = 412;

    /// <summary>
    /// Creates a new empty <see cref="InMemoryRepository{TEntity}"/> instance.
    /// </summary>
    public InMemoryRepository()
    {
    }

    /// <summary>
    /// Creates a new populated <see cref="InMemoryRepository{TEntity}"/> instance.
    /// </summary>
    /// <param name="seedEntities">A set of entities to be stored in the repository.</param>
    public InMemoryRepository(IEnumerable<TEntity> seedEntities)
    {
        foreach (TEntity entity in seedEntities)
        {
            CreateEntity(entity);
        }
    }

    #region Internal properties and methods for testing
    /// <summary>
    /// If set, the data store will throw this exception.  This is used to test error handling.
    /// </summary>
    internal Exception? ThrowException { get; set; }

    /// <summary>
    /// Used in assertions to get the raw entity from the data store.  Returns null if the entity does not exist.
    /// </summary>
    /// <param name="id">The globally unique ID for the entity.</param>
    /// <returns>The stored entity, or <c>null</c> if the entity does not exist.</returns>
    internal TEntity? GetEntity(string id) => _entities.TryGetValue(id, out var entity) ? entity : null;

    /// <summary>
    /// Used in assertions to get all the entities in the data store.
    /// </summary>
    /// <returns>The entities in the data store.</returns>
    internal List<TEntity> GetEntities() => _entities.Values.ToList();
    #endregion

    #region Private methods
    /// <summary>
    /// Produces a disconnected copy of the entity.
    /// </summary>
    private static TEntity Disconnect(TEntity entity)
        => AnyClone.CloneExtensions.Clone(entity);

    /// <summary>
    /// Updates the system properties and stores the new entity into the data store.
    /// </summary>
    /// <param name="entity">The entity to store.</param>
    private void StoreEntity(TEntity entity)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.Version = Guid.NewGuid().ToByteArray();
        _entities[entity.Id] = Disconnect(entity);
    }

    /// <summary>
    /// If the <see cref="ThrowException"/> has been set, throw it.
    /// </summary>
    private void ThrowExceptionIfSet()
    {
        if (ThrowException != null)
        {
            throw ThrowException;
        }
    }
    #endregion

    #region IRepository{TEntity}
    /// <inheritdoc />
    public ValueTask<IQueryable<TEntity>> AsQueryableAsync(CancellationToken cancellationToken = default)
    {
        ThrowExceptionIfSet();
        return ValueTask.FromResult(_entities.Values.AsQueryable());
    }

    /// <inheritdoc />
    public ValueTask CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ThrowExceptionIfSet();
        CreateEntity(entity);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DeleteAsync(string id, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        ThrowExceptionIfSet();
        DeleteEntity(id, version);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<TEntity> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        ThrowExceptionIfSet();
        if (string.IsNullOrEmpty(id))
        {
            throw new HttpException(Status400BadRequest);
        }
        if (!_entities.TryGetValue(id, out TEntity? entity))
        {
            throw new HttpException(Status404NotFound);
        }
        return ValueTask.FromResult(Disconnect(entity));
    }

    /// <inheritdoc />
    public ValueTask ReplaceAsync(TEntity entity, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        ThrowExceptionIfSet();
        ReplaceEntity(entity, version);
        return ValueTask.CompletedTask;
    }
    #endregion

    #region Synchronous Methods
    internal void CreateEntity(TEntity entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
        }
        if (_entities.TryGetValue(entity.Id, out TEntity? storedEntity))
        {
            throw new HttpException(Status409Conflict) { Payload = Disconnect(storedEntity) };
        }
        StoreEntity(entity);
    }

    internal void DeleteEntity(string id, byte[]? version = null)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new HttpException(Status400BadRequest);
        }
        if (!_entities.TryGetValue(id, out TEntity? storedEntity))
        {
            throw new HttpException(Status404NotFound);
        }
        if (version != null && !storedEntity.Version.SequenceEqual(version))
        {
            throw new HttpException(Status412PreconditionFailed) { Payload = Disconnect(storedEntity) };
        }
        _entities.TryRemove(id, out _);
    }

    internal void ReplaceEntity(TEntity entity, byte[]? version = null)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            throw new HttpException(Status400BadRequest);
        }
        if (!_entities.TryGetValue(entity.Id, out TEntity? storedEntity))
        {
            throw new HttpException(Status404NotFound);
        }
        if (version != null && !storedEntity.Version.SequenceEqual(version))
        {
            throw new HttpException(Status412PreconditionFailed) { Payload = Disconnect(storedEntity) };
        }
        StoreEntity(entity);
    }
    #endregion
}

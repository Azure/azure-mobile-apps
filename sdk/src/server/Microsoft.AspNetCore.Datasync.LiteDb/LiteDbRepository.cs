// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using LiteDB;

namespace Microsoft.AspNetCore.Datasync.LiteDb;

/// <summary>
/// A test repository that implements the <see cref="IRepository{TEntity}"/> interface for
/// Azure Mobile Apps, but stores data in a local repository.
/// </summary>
/// <typeparam name="TEntity">The type of entity to store</typeparam>
public class LiteDbRepository<TEntity> : IRepository<TEntity> where TEntity : LiteDbTableData
{
    private readonly LiteDatabase connection;

    // Note: on a web server (like this is normally used in), we expect the LiteDatabase to be
    // a singleton and we want to ensure that writes to the database are serialized. Thus, we
    // use an async semaphore to ensure that only one thread is writing to the database at a time.
    private readonly static SemaphoreSlim semaphore = new(1, 1);

    // We include these to avoid bringing in a new Abstractions package just for four ints.
    private const int Status400BadRequest = 400;
    private const int Status404NotFound = 404;
    private const int Status409Conflict = 409;
    private const int Status412PreconditionFailed = 412;

    /// <summary>
    /// Creates a new <see cref="LiteDbRepository{TEntity}"/> using the provided database
    /// connection and the class name as the collection name.
    /// </summary>
    /// <param name="databaseConnection">The <see cref="LiteDatabase"/> to use for storing entities.</param>
    public LiteDbRepository(LiteDatabase databaseConnection) : this(databaseConnection, typeof(TEntity).Name.ToLowerInvariant() + "s")
    {
    }

    /// <summary>
    /// Creates a new <see cref="LiteDbRepository{TEntity}"/> using the provided database
    /// connection and a specific collection name.
    /// </summary>
    /// <param name="databaseConnection">The <see cref="LiteDatabase"/> to use for storing entities.</param>
    /// <param name="collectionName">The name of the collection.</param>
    public LiteDbRepository(LiteDatabase databaseConnection, string collectionName)
    {
        connection = databaseConnection;
        Collection = connection.GetCollection<TEntity>(collectionName);
        Collection.EnsureIndex(x => x.UpdatedAt);
    }

    /// <summary>
    /// The collection within the LiteDb database that is used to store the entities.
    /// </summary>
    public ILiteCollection<TEntity> Collection { get; }

    /// <summary>
    /// Updates the system properties for the provided entity on write.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    private static void UpdateEntity(TEntity entity)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.Version = Guid.NewGuid().ToByteArray();
    }

    /// <summary>
    /// Executes the provided action within a lock on the collection.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes when the action is finished.</returns>
    private async ValueTask LockCollectionAsync(Action action, CancellationToken cancellationToken = default)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            action.Invoke();
        }
        finally
        {
            semaphore.Release();
        }
    }

    #region IRepository{TEntity}
    /// <inheritdoc/>
    public ValueTask<IQueryable<TEntity>> AsQueryableAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Collection.FindAll().AsQueryable());

    /// <inheritdoc/>
    public async ValueTask CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
        }
        await LockCollectionAsync(() =>
        {
            TEntity existingEntity = Collection.FindById(entity.Id);
            if (existingEntity != null)
            {
                throw new HttpException(Status409Conflict) { Payload = existingEntity };
            }
            UpdateEntity(entity);
            Collection.Insert(entity);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async ValueTask DeleteAsync(string id, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new HttpException(Status400BadRequest);
        }
        await LockCollectionAsync(() =>
        {
            TEntity storedEntity = Collection.FindById(id) ?? throw new HttpException(Status404NotFound);
            if (version != null && !storedEntity.Version.SequenceEqual(version))
            {
                throw new HttpException(Status412PreconditionFailed) { Payload = storedEntity };
            }
            Collection.Delete(id);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public ValueTask<TEntity> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new HttpException(Status400BadRequest);
        }
        TEntity entity = Collection.FindById(id) ?? throw new HttpException(Status404NotFound);
        return ValueTask.FromResult(entity);
    }

    /// <inheritdoc/>
    public async ValueTask ReplaceAsync(TEntity entity, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            throw new HttpException(Status400BadRequest);
        }
        await LockCollectionAsync(() =>
        {
            TEntity storedEntity = Collection.FindById(entity.Id) ?? throw new HttpException(Status404NotFound);
            if (version != null && !storedEntity.Version.SequenceEqual(version))
            {
                throw new HttpException(Status412PreconditionFailed) { Payload = storedEntity };
            }
            UpdateEntity(entity);
            Collection.Update(entity);
        }, cancellationToken);
    }
    #endregion
}

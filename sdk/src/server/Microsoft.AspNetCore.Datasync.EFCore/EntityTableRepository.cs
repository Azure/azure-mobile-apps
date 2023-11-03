// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.Datasync.EFCore;

/// <summary>
/// An implementation of the <see cref="IRepository{TEntity}"/> interface for
/// Azure Mobile Apps that stores data using Entity Framework Core.
/// </summary>
/// <typeparam name="TEntity">The type of entity to store</typeparam>
public class EntityTableRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntityTableData
{
    protected DbContext Context { get; }
    protected DbSet<TEntity> DataSet { get; }
    protected EntityTableRepositoryOptions Options { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="EntityTableRepository{TEntity}"/> class, using the default options.
    /// </summary>
    /// <param name="context">The database context for the backend store.</param>
    /// <exception cref="ArgumentException">Thrown if the <typeparamref name="TEntity"/> is not registered with the <paramref name="context"/>.</exception>"
    public EntityTableRepository(DbContext context) : this(context, new EntityTableRepositoryOptions())
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="EntityTableRepository{TEntity}"/> class, using specific options.
    /// </summary>
    /// <param name="context">The database context for the backend store.</param>
    /// <param name="options">The options to use for this instance.</param>
    /// <exception cref="ArgumentException">Thrown if the <typeparamref name="TEntity"/> is not registered with the <paramref name="context"/>.</exception>"
    public EntityTableRepository(DbContext context, EntityTableRepositoryOptions options)
    {
        Context = context;
        Options = options;
        try
        {
            DataSet = context.Set<TEntity>();
            _ = DataSet.Local;
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException($"Unregistered entity type: {typeof(TEntity).Name}", nameof(context));
        }
    }

    #region Private Methods
    /// <summary>
    /// Updates the metadata in the entity.  Not all database drivers support the <see cref="TimestampAttribute"/>
    /// or the <see cref="DatabaseGeneratedAttribute"/> in the correct way, so this method is used to ensure that
    /// the right values are set.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    protected void UpdateEntity(TEntity entity)
    {
        if (!Options.DatabaseUpdatesTimestamp)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
        if (!Options.DatabaseUpdatesVersion)
        {
            entity.Version = Guid.NewGuid().ToByteArray();
        }
    }

    /// <summary>
    /// Runs the inner part of an operation on the database, catching all the normal exceptions and reformatting
    /// them as appropriate.
    /// </summary>
    /// <param name="id">The ID of the entity being operated on.</param>
    /// <param name="action">The operation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes when the operation is finished.</returns>
    /// <exception cref="HttpException">Thrown if a concurrency exception occurs.</exception>
    /// <exception cref="RepositoryException">Throw if an error in the backend occurs.</exception>
    protected async Task WrapExceptionAsync(string id, Func<Task> action, CancellationToken cancellationToken = default)
    {
        try
        {
            await action.Invoke().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new HttpException(HttpStatusCodes.Status409Conflict, ex.Message, ex) { Payload = await GetEntityAsync(id, cancellationToken).ConfigureAwait(false) };
        }
        catch (DbUpdateException ex)
        {
            throw new RepositoryException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Retrieves an untracked version of an entity from the database.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An untracked version of the entity.</returns>
    protected Task<TEntity> GetEntityAsync(string id, CancellationToken cancellationToken = default)
        => DataSet.AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken);
    #endregion

    #region IRepository{TEntity}
    /// <inheritdoc />
    public virtual ValueTask<IQueryable<TEntity>> AsQueryableAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(DataSet.AsNoTracking());

    /// <inheritdoc />
    public virtual async ValueTask CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString("N");
        }
        await WrapExceptionAsync(entity.Id, async () =>
        {
            if (DataSet.Any(x => x.Id == entity.Id))
            {
                throw new HttpException(HttpStatusCodes.Status409Conflict) { Payload = await GetEntityAsync(entity.Id, cancellationToken).ConfigureAwait(false) };
            }
            UpdateEntity(entity);
            DataSet.Add(entity);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var updatedAt = Context.Entry(entity).Property(x => x.UpdatedAt).CurrentValue;
            var version = Context.Entry(entity).Property(x => x.Version).CurrentValue;
        }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async ValueTask DeleteAsync(string id, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new HttpException(HttpStatusCodes.Status400BadRequest);
        }

        await WrapExceptionAsync(id, async () =>
        {
            TEntity storedEntity = await DataSet.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false) ?? throw new HttpException(HttpStatusCodes.Status404NotFound);
            if (version != null && !storedEntity.Version.SequenceEqual(version))
            {
                throw new HttpException(HttpStatusCodes.Status412PreconditionFailed) { Payload = await GetEntityAsync(id, cancellationToken).ConfigureAwait(false) };
            }
            DataSet.Remove(storedEntity);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async ValueTask<TEntity> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new HttpException(HttpStatusCodes.Status400BadRequest);
        }
        TEntity? entity = await DataSet.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity == null)
        {
            throw new HttpException(HttpStatusCodes.Status404NotFound);
        }
        return entity;
    }

    /// <inheritdoc />
    public virtual async ValueTask ReplaceAsync(TEntity entity, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            throw new HttpException(HttpStatusCodes.Status400BadRequest);
        }
        await WrapExceptionAsync(entity.Id, async () =>
        {
            TEntity storedEntity = await DataSet.FindAsync(new object[] { entity.Id }, cancellationToken).ConfigureAwait(false) ?? throw new HttpException(HttpStatusCodes.Status404NotFound);
            if (version != null && !storedEntity.Version.SequenceEqual(version))
            {
                throw new HttpException(HttpStatusCodes.Status412PreconditionFailed) { Payload = await GetEntityAsync(entity.Id, cancellationToken).ConfigureAwait(false) };
            }
            UpdateEntity(entity);
            Context.Entry(storedEntity).CurrentValues.SetValues(entity);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            entity.UpdatedAt = storedEntity.UpdatedAt;
            entity.Version = storedEntity.Version.ToArray();
        }, cancellationToken);
    }
    #endregion
}

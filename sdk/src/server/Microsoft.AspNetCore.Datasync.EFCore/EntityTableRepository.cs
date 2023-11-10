// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync.EFCore;

/// <summary>
/// An implementation of the <see cref="IRepository{TEntity}"/> interface for
/// Azure Mobile Apps that stores data using Entity Framework Core.
/// </summary>
/// <typeparam name="TEntity">The type of entity to store</typeparam>
public class EntityTableRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntityTableData
{
    /// <summary>
    /// The <see cref="DbContext"/> used for saving changes to the entity set.
    /// </summary>
    protected DbContext Context { get; }

    /// <summary>
    /// The entity set for the repository.
    /// </summary>
    protected DbSet<TEntity> DataSet { get; }

    /// <summary>
    /// If <c>true</c>, then UpdatedAt is set to <see cref="DateTimeOffset.UtcNow"/> before saving.
    /// </summary>
    private bool shouldUpdateUpdatedAt;

    /// <summary>
    /// If <c>true</c>, then Version is set to a new GUID before saving.
    /// </summary>
    private bool shouldUpdateVersion;

    /// <summary>
    /// Creates a new instance of the <see cref="EntityTableRepository{TEntity}"/> class, using specific options.
    /// </summary>
    /// <param name="context">The database context for the backend store.</param>
    /// <exception cref="ArgumentException">Thrown if the <typeparamref name="TEntity"/> is not registered with the <paramref name="context"/>.</exception>"
    public EntityTableRepository(DbContext context)
    {
        Context = context;
        try
        {
            DataSet = context.Set<TEntity>();
            _ = DataSet.Local;
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException($"Unregistered entity type: {typeof(TEntity).Name}", nameof(context));
        }
        shouldUpdateUpdatedAt = Attribute.IsDefined(typeof(TEntity).GetProperty(nameof(ITableData.UpdatedAt))!, typeof(UpdatedByRepositoryAttribute));
        shouldUpdateVersion = Attribute.IsDefined(typeof(TEntity).GetProperty(nameof(ITableData.Version))!, typeof(UpdatedByRepositoryAttribute));
    }

    #region Private Methods
    /// <summary>
    /// Retrieves an untracked version of an entity from the database.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An untracked version of the entity.</returns>
    protected Task<TEntity> GetEntityAsync(string id, CancellationToken cancellationToken = default)
        => DataSet.AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken);

    /// <summary>
    /// Updates the managed properties for this entity if required.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    internal void UpdateManagedProperties(TEntity entity)
    {
        if (shouldUpdateUpdatedAt)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
        if (shouldUpdateVersion)
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
    internal async Task WrapExceptionAsync(string id, Func<Task> action, CancellationToken cancellationToken = default)
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
    #endregion

    #region IRepository{TEntity}
    /// <inheritdoc />
    public virtual ValueTask<IQueryable<TEntity>> AsQueryableAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(DataSet.AsNoTracking());

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1827:Do not use Count() or LongCount() when Any() can be used", Justification = "Cosmos EF Core driver does not support .Any()")]
    public virtual async ValueTask CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString("N");
        }
        await WrapExceptionAsync(entity.Id, async () =>
        {
            if (DataSet.Count(x => x.Id == entity.Id) > 0)
            {
                throw new HttpException(HttpStatusCodes.Status409Conflict) { Payload = await GetEntityAsync(entity.Id, cancellationToken).ConfigureAwait(false) };
            }
            UpdateManagedProperties(entity);
            DataSet.Add(entity);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
        TEntity entity = await DataSet.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false)
            ?? throw new HttpException(HttpStatusCodes.Status404NotFound);
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
            UpdateManagedProperties(entity);
            Context.Entry(storedEntity).CurrentValues.SetValues(entity);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }
    #endregion
}

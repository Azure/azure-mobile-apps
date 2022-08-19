// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.Datasync.EFCore
{
    /// <summary>
    /// An implementation of an <see cref="IRepository{TEntity}"/> that stores
    /// data in an Entity Framework configured database.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being stored.</typeparam>
    public class EntityTableRepository<TEntity> : IRepository<TEntity> where TEntity : class, ITableData
    {
        /// <summary>
        /// The EF Core <see cref="DbContext"/> for requests to the backend.
        /// </summary>
        internal DbContext Context { get; }

        /// <summary>
        /// The <see cref="DbSet{TEntity}"/> for the data set within EF Core.
        /// </summary>
        internal DbSet<TEntity> DataSet { get; }

        /// <summary>
        /// Create a new <see cref="EntityTableRepository{TEntity}"/> for accessing the database.
        /// This is the normal ctor for this repository.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> for the backend store.</param>
        public EntityTableRepository(DbContext context)
        {
            // Type check - only known derivates are allowed.
            var typeInfo = typeof(TEntity);
            if (!typeInfo.IsSubclassOf(typeof(EntityTableData)) && !typeInfo.IsSubclassOf(typeof(ETagEntityTableData)))
            {
                throw new InvalidCastException($"Entity type {typeof(TEntity).Name} is not a valid entity type.");
            }

            Context = context ?? throw new ArgumentNullException(nameof(context));
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

        /// <summary>
        /// Returns an unexecuted <see cref="IQueryable{T}"/> that represents the data store as a whole.
        /// This is adjusted by the <see cref="TableController{TEntity}"/> to account for filtering and
        /// paging requests.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> for the entities in the data store.</returns>
        public IQueryable<TEntity> AsQueryable() => DataSet.AsQueryable();

        /// <summary>
        /// Returns an unexecuted <see cref="IQueryable{T}"/> that represents the data store as a whole.
        /// This is adjusted by the <see cref="TableController{TEntity}"/> to account for filtering and
        /// paging requests.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> for the entities in the data store.</returns>
        public Task<IQueryable<TEntity>> AsQueryableAsync() => Task.FromResult(AsQueryable());

        /// <summary>
        /// Creates an entity within the data store. After completion, the system properties
        /// within the entity have been updated with new values.
        /// </summary>
        /// <param name="entity">The entity to be created.</param>
        /// <param name="token">A cancellation token.</param>
        /// <exception cref="ConflictException">if the entity to be created already exists.</exception>
        /// <exception cref="RepositoryException">if an error occurs in the data store.</exception>
        public async Task CreateAsync(TEntity entity, CancellationToken token = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                var storeEntity = entity.Id == null ? null : await LookupAsync(entity.Id, token).ConfigureAwait(false);
                if (storeEntity != null)
                {
                    throw new ConflictException(Disconnect(storeEntity));
                }
                if (entity.Id == null)
                {
                    entity.Id = Guid.NewGuid().ToString("N");
                }
                entity.UpdatedAt = DateTimeOffset.UtcNow;
                DataSet.Add(entity);
                await Context.SaveChangesAsync(token).ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                throw new RepositoryException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Removes an entity from the data store. If a <c>version</c> is provided, the version
        /// must match the entity version.
        /// </summary>
        /// <param name="id">The globally unique ID of the entity to be removed.</param>
        /// <param name="version">The (optional) version of the entity to be removed.</param>
        /// <param name="token">A cancellation token.</param>
        /// <exception cref="NotFoundException">if the entity does not exist.</exception>
        /// <exception cref="PreconditionFailedException">if the entity version does not match the provided version</exception>
        /// <exception cref="RepositoryException">if an error occurs in the data store.</exception>
        public async Task DeleteAsync(string id, byte[] version = null, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException();
            }

            try
            {
                var storeEntity = await LookupAsync(id, token).ConfigureAwait(false);
                if (storeEntity == null)
                {
                    throw new NotFoundException();
                }

                if (version != null && storeEntity.Version?.SequenceEqual(version) != true)
                {
                    throw new PreconditionFailedException(Disconnect(storeEntity));
                }

                DataSet.Remove(storeEntity);
                await Context.SaveChangesAsync(token).ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                throw new RepositoryException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Reads the entity from the data store.
        /// </summary>
        /// <remarks>
        /// It is important that the entity returned is "disconnected" from the store. Some controller
        /// methods alter the entity to conform to the new spec.  If the entity is connected to the
        /// data store, then the data store is updated at the same time, resulting in data leakage
        /// problems.
        /// </remarks>
        /// <param name="id">The globally unique ID of the entity to be read.</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>The entity, or null if the entity does not exist.</returns>
        /// <exception cref="RepositoryException">if an error occurs in the data store.</exception>
        public async Task<TEntity> ReadAsync(string id, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException();
            }

            return Disconnect(await LookupAsync(id, token).ConfigureAwait(false));
        }

        /// <summary>
        /// Replace the entity within the store with the provided entity.  If a <c>version</c> is
        /// specified, then the version must match.  On return, the system properties of the entity
        /// will be updated.
        /// </summary>
        /// <param name="entity">The replacement entity.</param>
        /// <param name="version">The (optional) version of the entity to be replaced</param>
        /// <param name="token">A cancellation token</param>
        /// <exception cref="BadRequestException">if the entity does not have an ID</exception>
        /// <exception cref="NotFoundException">if the entity does not exist</exception>
        /// <exception cref="ConflictException">if the entity version does not match the provided version</exception>
        /// <exception cref="RepositoryException">if an error occurs in the data store.</exception>
        public async Task ReplaceAsync(TEntity entity, byte[] version = null, CancellationToken token = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (string.IsNullOrEmpty(entity.Id))
            {
                throw new BadRequestException();
            }

            try
            {
                var storeEntity = await LookupAsync(entity.Id, token).ConfigureAwait(false);
                if (storeEntity == null)
                {
                    throw new NotFoundException();
                }

                if (version != null && storeEntity.Version?.SequenceEqual(version) != true)
                {
                    throw new PreconditionFailedException(Disconnect(storeEntity));
                }

                entity.UpdatedAt = DateTimeOffset.UtcNow;
                Context.Entry(storeEntity).CurrentValues.SetValues(entity);
                await Context.SaveChangesAsync(token).ConfigureAwait(false);

                // Copy the stored values for the metadata back into the entity.
                entity.Version = storeEntity.Version;
                entity.UpdatedAt = storeEntity.UpdatedAt;
            }
            catch (DbUpdateException ex)
            {
                throw new RepositoryException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Obtains a connected entity from the data set.
        /// </summary>
        /// <param name="id">The ID of the entity</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>The entity, or null if no entity exists.</returns>
        internal ValueTask<TEntity> LookupAsync(string id, CancellationToken token = default)
            => DataSet.FindAsync(new[] { id }, token);

        /// <summary>
        /// Gets a disconnected (as much as we can) copy of the entity provided.
        /// </summary>
        /// <param name="entity">The entity to disconnect</param>
        /// <returns>A non-tracked version of the entity.</returns>
        private TEntity Disconnect(TEntity entity)
            => entity == null ? null : Context.Entry(entity).CurrentValues.Clone().ToObject() as TEntity;
    }
}

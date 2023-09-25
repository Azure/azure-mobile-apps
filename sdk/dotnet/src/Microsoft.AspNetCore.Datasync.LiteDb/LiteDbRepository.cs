// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using LiteDB;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Datasync.LiteDb
{
    /// <summary>
    /// A test repository that implements the <see cref="IRepository{TEntity}"/> interface for
    /// Azure Mobile Apps, but stores data in a local repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to store</typeparam>
    public class LiteDbRepository<TEntity> : IRepository<TEntity> where TEntity : LiteDbTableData
    {
        private readonly object writeLock = new();
        private readonly LiteDatabase connection;

        /// <summary>
        /// Creates a new <see cref="LiteDbRepository{TEntity}"/>, using the provided database
        /// connection.
        /// </summary>
        /// <param name="databaseConnection">The database connection.</param>
        /// <param name="collectionName">The name of the collection (optional - uses the entity name by default).</param>
        public LiteDbRepository(LiteDatabase databaseConnection, string collectionName = null)
        {
            collectionName ??= typeof(TEntity).Name.ToLowerInvariant() + 's';

            connection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
            Collection = connection.GetCollection<TEntity>(collectionName);
            Collection.EnsureIndex(x => x.UpdatedAt);
        }

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

        #region IRepository<TEntity> Interface
        /// <summary>
        /// Returns an unexecuted <see cref="IQueryable{T}"/> that represents the data store
        /// as a whole. This is adjusted by the <see cref="TableController{TEntity}"/> to account
        /// for the filtering and paging requested.
        /// </summary>
        /// <remarks>We provide an "IEnumerable" instead that iterates over the entire collection.</remarks>
        /// <param name="token">A cancellation token</param>
        /// <returns>An <see cref="IQueryable{T}"/> for the entities in the data store</returns>
        public IQueryable<TEntity> AsQueryable()
            => Collection.FindAll().AsQueryable();

        /// <summary>
        /// Create a new entity within the backend data store.  If the entity does not
        /// have an ID, one is created. After completion, the system properties will be
        /// filled with new values and the item will be marked as not deleted.
        /// </summary>
        /// <param name="entity">The entity to be created.</param>
        /// <param name="token">A cancellation token</param>
        /// <exception cref="ConflictException">if the Id representing the entity already exists.</exception>
        /// <exception cref="RepositoryException">if a backend data store error occurred.</exception>
        public Task CreateAsync(TEntity entity, CancellationToken token = default)
        {
            CreateEntity(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes the specified entity within the backend store. If a <c>version</c> is specified,
        /// then the version must match the entity to be deleted.
        /// </summary>
        /// <param name="id">The globally unique ID of the entity to be deleted.</param>
        /// <param name="version">The (optional) version of the entity to be deleted.</param>
        /// <param name="token">A cancellation token</param>
        /// <exception cref="NotFoundException">if the entity does not exist.</exception>
        /// <exception cref="ConflictException">if the entity version does not match that which is provided.</exception>
        /// <exception cref="RepositoryException">if a backend data store error occurred.</exception>
        public Task DeleteAsync(string id, byte[] version = null, CancellationToken token = default)
        {
            DeleteEntity(id, version);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reads an entity from the backend data store.
        /// </summary>
        /// <remarks>
        /// **IMPORTANT** The entity that is returned must be a "disconnected" entity.  Do not
        /// just return the entity from the store as the store may be adjusted accidentally.
        /// </remarks>
        /// <param name="id">The globally unique ID of the entity to be deleted.</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>The entity</returns>
        /// <exception cref="RepositoryException">if a backend data store error occurred.</exception>
        public Task<TEntity> ReadAsync(string id, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException();
            }

            TEntity entity = Collection.FindById(id);
            return Task.FromResult(entity);
        }

        /// <summary>
        /// Replaces the specified entity within the backend store.  If a <c>version</c> is specified,
        /// then the version must match the entity to be replaced.
        /// </summary>
        /// <param name="entity">The replacement entity.</param>
        /// <param name="version">The (optional) version of the entity to be deleted.</param>
        /// <param name="token">A cancellation token</param>
        /// <exception cref="BadRequestException">if the entity does not have an ID</exception>
        /// <exception cref="NotFoundException">if the entity does not exist.</exception>
        /// <exception cref="ConflictException">if the entity version does not match that which is provided.</exception>
        /// <exception cref="RepositoryException">if a backend data store error occurred.</exception>
        public Task ReplaceAsync(TEntity entity, byte[] version = null, CancellationToken token = default)
        {
            ReplaceEntity(entity, version);
            return Task.CompletedTask;
        }
        #endregion

        #region Synchronous Methods
        /// <summary>
        /// Create a new entity within the backend data store.  If the entity does not
        /// have an ID, one is created. After completion, the system properties will be
        /// filled with new values and the item will be marked as not deleted.
        /// </summary>
        /// <param name="entity">The entity to be created.</param>
        /// <exception cref="ConflictException">if the Id representing the entity already exists.</exception>
        internal void CreateEntity(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }

            lock (writeLock)
            {
                TEntity existingEntity = Collection.FindById(entity.Id);
                if (existingEntity != null)
                {
                    throw new ConflictException(existingEntity);
                }
                UpdateEntity(entity);
                Collection.Insert(entity);
            }
        }

        /// <summary>
        /// Deletes the specified entity within the backend store. If a <c>version</c> is specified,
        /// then the version must match the entity to be deleted.
        /// </summary>
        /// <param name="id">The globally unique ID of the entity to be deleted.</param>
        /// <param name="version">The (optional) version of the entity to be deleted.</param>
        /// <exception cref="NotFoundException">if the entity does not exist.</exception>
        /// <exception cref="PreconditionFailedException">if the entity version does not match that which is provided.</exception>
        internal void DeleteEntity(string id, byte[] version = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException();
            }

            lock (writeLock)
            {
                TEntity existingEntity = Collection.FindById(id);
                if (existingEntity == null)
                {
                    throw new NotFoundException();
                }

                if (PreconditionFailed(version, existingEntity.Version))
                {
                    throw new PreconditionFailedException(existingEntity);
                }
                Collection.Delete(id);
            }
        }

        /// <summary>
        /// Replaces the specified entity within the backend store.  If a <c>version</c> is specified,
        /// then the version must match the entity to be replaced.
        /// </summary>
        /// <param name="entity">The replacement entity.</param>
        /// <param name="version">The (optional) version of the entity to be deleted.</param>
        /// <exception cref="BadRequestException">if the entity does not have an ID</exception>
        /// <exception cref="NotFoundException">if the entity does not exist.</exception>
        /// <exception cref="PreconditionFailedException">if the entity version does not match that which is provided.</exception>
        internal void ReplaceEntity(TEntity entity, byte[] version = null)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (string.IsNullOrEmpty(entity.Id))
            {
                throw new BadRequestException();
            }

            lock (writeLock)
            {
                TEntity existingEntity = Collection.FindById(entity.Id);
                if (existingEntity == null)
                {
                    throw new NotFoundException();
                }

                if (PreconditionFailed(version, existingEntity.Version))
                {
                    throw new PreconditionFailedException(existingEntity);
                }
                UpdateEntity(entity);
                Collection.Update(entity);
            }
        }
        #endregion

        /// <summary>
        /// Checks that the version provided matches the version in the database.
        /// </summary>
        /// <param name="requiredVersion">The requ</param>
        /// <param name="currentVersion"></param>
        /// <returns>True if we need to throw a <see cref="PreconditionFailedException"/>.</returns>
        internal static bool PreconditionFailed(byte[] expectedVersion, byte[] currentVersion)
           => expectedVersion != null && currentVersion?.SequenceEqual(expectedVersion) != true;
    }
}

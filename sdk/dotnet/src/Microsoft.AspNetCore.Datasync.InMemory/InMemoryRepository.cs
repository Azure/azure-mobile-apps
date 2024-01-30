// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Datasync.InMemory
{
    /// <summary>
    /// A test repository that implements the <see cref="IRepository{TEntity}"/> interface for
    /// Azure Mobile Apps, but stores data in a local repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to store</typeparam>
    public class InMemoryRepository<TEntity> : IRepository<TEntity> where TEntity : InMemoryTableData
    {
        private readonly ConcurrentDictionary<string, TEntity> _store = new();

        /// <summary>
        /// Creates a new <see cref="InMemoryRepository{TEntity}"/>, potentially seeded with
        /// some data.
        /// </summary>
        /// <param name="seedEntities">A list of entities to use to seed the store.</param>
        public InMemoryRepository(IEnumerable<TEntity> seedEntities = null)
        {
            if (seedEntities != null)
            {
                foreach (TEntity entity in seedEntities)
                {
                    entity.Id ??= Guid.NewGuid().ToString("N");
                    StoreEntity(entity);
                }
            }
        }

        #region Internal Properties and Methods for testing
        /// <summary>
        /// Used for testing the explicit actions in the <see cref="TableController{TEntity}"/> when a data
        /// store error occurs.  When set, any calls will fail with this error.
        /// </summary>
        internal Exception ThrowException { get; set; }

        /// <summary>
        /// Used in assertions to get an entity from the store.  Returns null if the entity does not exist.
        /// </summary>
        /// <param name="id">The id to retrieve.</param>
        /// <returns>The entity, or null if it doesn't exist.</returns>
        internal TEntity GetEntity(string id) => _store.TryGetValue(id, out TEntity entity) ? entity : null;

        /// <summary>
        /// Used in assertions to get the full list of entities from the store.
        /// </summary>
        internal List<TEntity> Entities { get => _store.Values.ToList(); }

        /// <summary>
        /// Used in testing to clear the store so that results are predicatable.
        /// </summary>
        internal void Clear() => _store.Clear();
        #endregion

        #region Private Methods
        /// <summary>
        /// Disconnects the entity from the store by making a "true copy" of the entity provided.
        /// </summary>
        /// <param name="entity">The entity to disconnect</param>
        /// <returns>The disconnected entity</returns>
        private static TEntity Disconnect(TEntity entity)
        {
            string json = JsonConvert.SerializeObject(entity);
            return JsonConvert.DeserializeObject<TEntity>(json);
        }

        /// <summary>
        /// Checks that the version provided matches the version in the database.
        /// </summary>
        /// <param name="expectedVersion">The version that the client provides.</param>
        /// <param name="currentVersion">The version that is stored in the database.</param>
        /// <returns>True if we need to throw a <see cref="PreconditionFailedException"/>.</returns>
        protected virtual bool PreconditionFailed(byte[] expectedVersion, byte[] currentVersion)
           => expectedVersion != null && currentVersion?.SequenceEqual(expectedVersion) != true;

        /// <summary>
        /// Updates the system properties for the provided entity on write.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        private void StoreEntity(TEntity entity)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            entity.Version = Guid.NewGuid().ToByteArray();
            _store[entity.Id] = Disconnect(entity);
        }

        /// <summary>
        /// Used during testing to throw an exception if one has been set.
        /// </summary>
        protected void ThrowExceptionIfSet()
        {
            if (ThrowException != null)
            {
                throw ThrowException;
            }
        }
        #endregion

        /// <summary>
        /// Returns an unexecuted <see cref="IQueryable{T}"/> that represents the data store
        /// as a whole. This is adjusted by the <see cref="TableController{TEntity}"/> to account
        /// for the filtering and paging requested.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> for the entities in the data store</returns>
        public IQueryable<TEntity> AsQueryable()
        {
            ThrowExceptionIfSet();
            return _store.Values.AsQueryable();
        }

        /// <summary>
        /// Returns an unexecuted <see cref="IQueryable{T}"/> that represents the data store
        /// as a whole. This is adjusted by the <see cref="TableController{TEntity}"/> to account
        /// for the filtering and paging requested.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> for the entities in the data store</returns>
        public Task<IQueryable<TEntity>> AsQueryableAsync() => Task.FromResult(AsQueryable());

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
            ThrowExceptionIfSet();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }
            if (_store.TryGetValue(entity.Id, out TEntity storedEntity))
            {
                throw new ConflictException(Disconnect(storedEntity));
            }
            StoreEntity(entity);
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
            ThrowExceptionIfSet();
            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException();
            }
            if (!_store.TryGetValue(id, out TEntity entity))
            {
                throw new NotFoundException();
            }
            if (PreconditionFailed(version, entity.Version))
            {
                throw new PreconditionFailedException(Disconnect(entity));
            }
            _store.TryRemove(id, out _);
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
            ThrowExceptionIfSet();
            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException();
            }
            if (!_store.TryGetValue(id, out TEntity entity))
            {
                return Task.FromResult<TEntity>(null);
            }
            return Task.FromResult(Disconnect(entity));
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
            ThrowExceptionIfSet();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (string.IsNullOrEmpty(entity.Id))
            {
                throw new BadRequestException();
            }
            if (!_store.TryGetValue(entity.Id, out TEntity storedEntity))
            {
                throw new NotFoundException();
            }
            if (PreconditionFailed(version, storedEntity.Version))
            {
                throw new PreconditionFailedException(Disconnect(storedEntity));
            }
            StoreEntity(entity);
            return Task.CompletedTask;
        }
    }
}

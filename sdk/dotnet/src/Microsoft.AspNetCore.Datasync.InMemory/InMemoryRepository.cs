// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
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
        private readonly Dictionary<string, TEntity> _store = new();
        private readonly object writeLock = new();

        /// <summary>
        /// Creates a new <see cref="InMemoryRepository{TEntity}"/>, potentially seeded with
        /// some data.
        /// </summary>
        /// <param name="seedEntities">A list of entities to use to seed the store.</param>
        public InMemoryRepository(IEnumerable<TEntity> seedEntities = null)
        {
            if (seedEntities != null)
            {
                foreach (var entity in seedEntities)
                {
                    CreateEntity(entity);
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
        internal TEntity GetEntity(string id) => _store.ContainsKey(id) ? _store[id] : null;

        /// <summary>
        /// Used in assertions to get the full list of entities from the store.
        /// </summary>
        internal List<TEntity> Entities { get => _store.Values.ToList(); }
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
        /// Updates the system properties for the provided entity on write.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        private static void UpdateEntity(TEntity entity)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            entity.Version = Guid.NewGuid().ToByteArray();
        }
        #endregion

        #region IRepository<TEntity> Interface
        /// <summary>
        /// Returns an unexecuted <see cref="IQueryable{T}"/> that represents the data store
        /// as a whole. This is adjusted by the <see cref="TableController{TEntity}"/> to account
        /// for the filtering and paging requested.
        /// </summary>
        /// <param name="token">A cancellation token</param>
        /// <returns>An <see cref="IQueryable{T}"/> for the entities in the data store</returns>
        public IQueryable<TEntity> AsQueryable()
        {
            if (ThrowException != null)
            {
                throw ThrowException;
            }

            return _store.Values.AsQueryable();
        }

        /// <summary>
        /// Returns an unexecuted <see cref="IQueryable{T}"/> that represents the data store as a whole.
        /// This is adjusted by the <see cref="TableController{TEntity}"/> to account for filtering and
        /// paging requests.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> for the entities in the data store.</returns>
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
            if (ThrowException != null)
            {
                throw ThrowException;
            }

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
            if (ThrowException != null)
            {
                throw ThrowException;
            }

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
        /// <exception cref="NotFoundException">if the entity does not exist</exception>
        /// <exception cref="RepositoryException">if a backend data store error occurred.</exception>
        public Task<TEntity> ReadAsync(string id, CancellationToken token = default)
        {
            if (ThrowException != null)
            {
                throw ThrowException;
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException();
            }

            TEntity entity = _store.ContainsKey(id) ? Disconnect(_store[id]) : null;
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
            if (ThrowException != null)
            {
                throw ThrowException;
            }

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
                if (_store.ContainsKey(entity.Id))
                {
                    throw new ConflictException(Disconnect(_store[entity.Id]));
                }
                UpdateEntity(entity);
                _store[entity.Id] = Disconnect(entity);
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
                if (!_store.ContainsKey(id))
                {
                    throw new NotFoundException();
                }
                TEntity existing = _store[id];
                if (version != null && existing.Version?.SequenceEqual(version) != true)
                {
                    throw new PreconditionFailedException(Disconnect(existing));
                }
                _store.Remove(id);
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
                if (!_store.ContainsKey(entity.Id))
                {
                    throw new NotFoundException();
                }

                TEntity existing = _store[entity.Id];
                if (version != null && existing.Version?.SequenceEqual(version) != true)
                {
                    throw new PreconditionFailedException(Disconnect(existing));
                }
                UpdateEntity(entity);
                _store[entity.Id] = Disconnect(entity);
            }
        }
        #endregion
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// A definition of the backend data store operations that the <see cref="TableController{TEntity}"/>
    /// will perform in response to client operations.  This equates to a basic CRUDL repository
    /// pattern.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity stored in the repository.</typeparam>
    public interface IRepository<TEntity> where TEntity : ITableData
    {
        /// <summary>
        /// Returns an unexecuted <see cref="IQueryable{T}"/> that represents the data store as a whole.
        /// This is adjusted by the <see cref="TableController{TEntity}"/> to account for filtering and
        /// paging requests.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> for the entities in the data store.</returns>
        IQueryable<TEntity> AsQueryable();

        /// <summary>
        /// Returns an unexecuted <see cref="IQueryable{T}"/> that represents the data store as a whole.
        /// This is adjusted by the <see cref="TableController{TEntity}"/> to account for filtering and
        /// paging requests.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> for the entities in the data store.</returns>
        Task<IQueryable<TEntity>> AsQueryableAsync() { return Task.FromResult(AsQueryable()); }

        /// <summary>
        /// Creates an entity within the data store. After completion, the system properties
        /// within the entity have been updated with new values.
        /// </summary>
        /// <param name="entity">The entity to be created.</param>
        /// <param name="token">A cancellation token.</param>
        /// <exception cref="ConflictException">if the entity to be created already exists.</exception>
        /// <exception cref="RepositoryException">if an error occurs in the data store.</exception>
        Task CreateAsync(TEntity entity, CancellationToken token = default);

        /// <summary>
        /// Removes an entity from the data store. If a <c>version</c> is provided, the version
        /// must match the entity version.
        /// </summary>
        /// <param name="id">The globally unique ID of the entity to be removed.</param>
        /// <param name="version">The (optional) version of the entity to be removed.</param>
        /// <param name="token">A cancellation token.</param>
        /// <exception cref="NotFoundException">if the entity does not exist.</exception>
        /// <exception cref="ConflictException">if the entity version does not match the provided version</exception>
        /// <exception cref="RepositoryException">if an error occurs in the data store.</exception>
        Task DeleteAsync(string id, byte[] version = null, CancellationToken token = default);

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
        Task<TEntity> ReadAsync(string id, CancellationToken token = default);

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
        Task ReplaceAsync(TEntity entity, byte[] version = null, CancellationToken token = default);
    }
}

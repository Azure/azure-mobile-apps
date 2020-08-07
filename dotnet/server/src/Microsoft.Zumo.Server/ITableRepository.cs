// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Zumo.Server
{
    /// <summary>
    /// Provides an abstraction for accessing a backend store for a <see cref="TableController{T}"/>.  All backend
    /// access is provided through this interface.
    /// </summary>
    /// <typeparam name="TEntity">The model type for entities within the table</typeparam>
    public interface ITableRepository<TEntity> where TEntity : ITableData
    {
        /// <summary>
        /// Builds an <see cref="IQueryable{T}"/> to be executed against a store supporting LINQ for querying data.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> that has not yet been executed.</returns>
        IQueryable<TEntity> AsQueryable();

        /// <summary>
        /// Creates a new entity within the store.  The entity must not exist. 
        /// is thrown.
        /// </summary>
        /// <param name="item">The new form of the entity</param>
        /// <returns>The new entity</returns>
        /// <throws><see cref="EntityExistsException"/> if the entity to be added already exists.</throws>
        Task<TEntity> CreateAsync(TEntity item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the entity within the store.  If the entity does not exist, false is returned.
        /// </summary>
        /// <param name="id">The id of the entity</param>
        /// <throws><see cref="EntityDoesNotExistException"/> if the entity does not exist.</throws>
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Looks up a single entity in the store.
        /// </summary>
        /// <param name="id">The id of the entity to be returned</param>
        /// <returns>The entity, or null if no entity is available.</returns>
        ValueTask<TEntity> LookupAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces an entity within the store.  The entity must exist.  If not, then null is returned
        /// is thrown.
        /// </summary>
        /// <param name="item">The new form of the entity</param>
        /// <returns>The new entity</returns>
        /// <throws><see cref="EntityDoesNotExistException"/> if the entity does not exist.</throws>
        Task<TEntity> ReplaceAsync(TEntity item, CancellationToken cancellationToken = default);
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync;

/// <summary>
/// The definition of the interface to a backend data store that will store
/// synchronizable entities.  The table controllers will use a repository to
/// store and retrieve entities from the data store.
/// </summary>
/// <typeparam name="TEntity">The type of entity being stored in the repository.</typeparam>
public interface IRepository<TEntity> where TEntity : ITableData
{
    /// <summary>
    /// Returns an unexecuted <see cref="IQueryable{T}"/> for the data set.  This will be adjusted
    /// by the table controller to account for data views, filtering and paging requests.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns an unexecuted <see cref="IQueryable{T}"/> when complete.</returns>
    /// <exception cref="RepositoryException">Thrown is there is an error in the repository.</exception>
    ValueTask<IQueryable<TEntity>> AsQueryableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entity in the data store.  After completion, the system properties within the
    /// entity have been updated to reflect the current state of the entity in the data store.
    /// </summary>
    /// <param name="entity">The entity to be created.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes when the entity is created.</returns>
    /// <exception cref="HttpException">Thrown if the entity creation would produce a normal HTTP error.</exception>
    /// <exception cref="RepositoryException">Thrown is there is an error in the repository.</exception>
    ValueTask CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an existing entity in the data store.  If the version is provided, the entity will only
    /// be removed if the version matches the version stored with the entity in the data store.
    /// </summary>
    /// <param name="id">The globally unique identifier for the entity.</param>
    /// <param name="version">The (optional) version for the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes when the entity is removed.</returns>
    /// <exception cref="HttpException">Thrown if the entity creation would produce a normal HTTP error.</exception>
    /// <exception cref="RepositoryException">Thrown is there is an error in the repository.</exception>
    ValueTask DeleteAsync(string id, byte[]? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads an existing entity from the data store.
    /// </summary>
    /// <param name="id">The globally unique identifier for the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the entity when the operation is complete.</returns>
    /// <exception cref="HttpException">Thrown if the entity creation would produce a normal HTTP error.</exception>
    /// <exception cref="RepositoryException">Thrown is there is an error in the repository.</exception>
    ValueTask<TEntity> ReadAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an existing entity in the data store.  If the version is provided, the entity will only
    /// be replaced if the version matches the version stored with the entity in the data store.
    /// </summary>
    /// <param name="entity">The new value for the entity.</param>
    /// <param name="version">The (optional) version for the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes when the entity is removed.</returns>
    /// <exception cref="HttpException">Thrown if the entity creation would produce a normal HTTP error.</exception>
    /// <exception cref="RepositoryException">Thrown is there is an error in the repository.</exception>
    ValueTask ReplaceAsync(TEntity entity, byte[]? version = null, CancellationToken cancellationToken = default);
}

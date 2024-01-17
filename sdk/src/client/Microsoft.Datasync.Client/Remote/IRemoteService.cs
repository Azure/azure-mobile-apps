// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Remote;

/// <summary>
/// The definition of the operations that can be done against a read-write remote table.
/// </summary>
/// <typeparam name="TEntity">The type of the entity returned by the table connection.</typeparam>
public interface IRemoteService<TEntity> : IReadonlyRemoteService<TEntity> where TEntity : IClientTableData
{
    /// <summary>
    /// Add the provided entity to the remote table.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="options">The remote operation options to use for this request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the added entity when the operation is completed.</returns>
    /// <exception cref="RemoteServiceException">Thrown if the remote service returns an error.</exception>
    ValueTask<TEntity> AddAsync(TEntity entity, RemoteOperationOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the specified entity from the remote table, using the provided ID and version.
    /// </summary>
    /// <param name="id">The globally unique ID for the entity.</param>
    /// <param name="version">If provided, the current version of the entity.</param>
    /// <param name="options">The remote operation options to use for this request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns when the operation is completed.</returns>
    /// <exception cref="RemoteServiceException">Thrown if the remote service returns an error.</exception>
    ValueTask RemoveAsync(string id, byte[]? version, RemoteOperationOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the specified entity in the remote table, using new data from the provided entity.
    /// </summary>
    /// <param name="entity">The entity to replace with new data.</param>
    /// <param name="options">The remote operation options to use for this request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the updated entity when the operation is completed.</returns>
    /// <exception cref="RemoteServiceException">Thrown if the remote service returns an error.</exception>
    ValueTask<TEntity> UpdateAsync(TEntity entity, RemoteOperationOptions options, CancellationToken cancellationToken = default);
}

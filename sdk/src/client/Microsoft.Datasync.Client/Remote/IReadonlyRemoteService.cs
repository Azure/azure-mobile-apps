// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Remote;

/// <summary>
/// The definition of the operations that can be done against a read-only remote table.
/// </summary>
/// <typeparam name="TEntity">The type of the entity returned by the table connection.</typeparam>
public interface IReadonlyRemoteService<TEntity> where TEntity : IClientTableData
{
    /// <summary>
    /// Retrieves an entity from the remote table using the provided ID.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="options">The remote operation options to use in executing the request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the entity or <c>null</c> if the entity does not exist when complete.</returns>
    /// <exception cref="RemoteServiceException">Thrown if the remote service returns an error.</exception>
    ValueTask<TEntity?> FindOrDefaultAsync(string id, RemoteOperationOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a query on the remote table, using the provided remote query.
    /// </summary>
    /// <typeparam name="TResult">The type of entity returned by the query.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="options">The remote operation options to use in executing the request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns a page of results when complete.</returns>
    /// <exception cref="RemoteServiceException">Thrown if the remote service returns an error.</exception>
    ValueTask<Page<TResult>> QueryAsync<TResult>(IRemoteQuery<TResult> query, RemoteOperationOptions options, CancellationToken cancellationToken = default) where TResult : IClientTableData;

    /// <summary>
    /// Executes a follow-on query on the remote table, using the previous page as source material for the current page.
    /// </summary>
    /// <typeparam name="TResult">The type of entity returned by the query.</typeparam>
    /// <param name="previousPage">The previous page of results.</param>
    /// <param name="options">The remote operation options to use in executing the request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A page of results.</returns>
    /// <exception cref="RemoteServiceException">Thrown if the remote service returns an error.</exception>
    ValueTask<Page<TResult>> QueryNextAsync<TResult>(Page<TResult> previousPage, RemoteOperationOptions options, CancellationToken cancellationToken = default);
}

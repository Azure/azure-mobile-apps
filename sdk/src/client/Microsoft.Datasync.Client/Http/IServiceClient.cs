// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Http;

/// <summary>
/// This defines the methods that are required to interact with a Datasync service.
/// </summary>
public interface IServiceClient<TEntity> : IDisposable where TEntity : ClientTableData, new()
{
    /// <summary>
    /// Creates a new entity in the service. The resulting saved entity is returned when the operation
    /// is complete.
    /// </summary>
    /// <param name="entity">The entity to be created.</param>
    /// <param name="options">Any options to be used in sending the request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the saved entity when complete.</returns>
    /// <exception cref="DatasyncConflictException">thrown if the service returns a conflict.</exception>
    /// <exception cref="DatasyncServiceException">thrown if the service returns an error.</exception>
    Task<TEntity> CreateAsync(TEntity entity, ServiceOperationOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the entity identified by the provided entity.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="options">Any options to be used in sending the request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes when the operation is complete.</returns>
    /// <exception cref="DatasyncConflictException">thrown if the service returns a conflict.</exception>
    /// <exception cref="DatasyncServiceException">thrown if the service returns an error.</exception>
    Task DeleteAsync(string id, ServiceOperationOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the entity identified by the provided ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="options">Any options to be used in sending the request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the saved entity when complete.</returns>
    /// <exception cref="DatasyncServiceException">thrown if the service returns an error.</exception>
    Task<TEntity> GetAsync(string id, ServiceOperationOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of entities from the service after executing the query provided in the options.
    /// </summary>
    /// <param name="options">The query options to use for this operation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A page of entities from the service</returns>
    /// <exception cref="DatasyncServiceException">thrown if the service returns an error.</exception>
    Task<Page<TResult>> QueryAsync<TResult>(QueryOperationOptions options, CancellationToken cancellationToken = default) where TResult : new();

    /// <summary>
    /// Replaces the server side entity with the entity provided.
    /// </summary>
    /// <param name="entity">The updated data for the entity.</param>
    /// <param name="options">Any options to be used in sending the request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the saved entity when complete.</returns>
    /// <exception cref="DatasyncConflictException">thrown if the service returns a conflict.</exception>
    /// <exception cref="DatasyncServiceException">thrown if the service returns an error.</exception>
    Task<TEntity> ReplaceAsync(TEntity entity, ServiceOperationOptions options, CancellationToken cancellationToken = default);
}

/// <summary>
/// A set of extension methods for working with the <see cref="IServiceClient{TEntity}"/> interface.
/// </summary>
public static class IServiceClientExtensions
{
    /// <summary>
    /// Creates a new entity in the service. The resulting saved entity is returned when the operation
    /// is complete.
    /// </summary>
    /// <param name="entity">The entity to be created.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the saved entity when complete.</returns>
    /// <exception cref="DatasyncConflictException">thrown if the service returns a conflict.</exception>
    /// <exception cref="DatasyncServiceException">thrown if the service returns an error.</exception>
    public static Task CreateAsync<TEntity>(this IServiceClient<TEntity> client, TEntity entity, CancellationToken cancellationToken = default) where TEntity : ClientTableData, new()
        => client.CreateAsync(entity, new ServiceOperationOptions(), cancellationToken);

    /// <summary>
    /// Deletes the provided entity if the entity version matches the server-side version.
    /// </summary>
    /// <param name="entity">The entity to be deleted.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes when the operation is complete.</returns>
    /// <exception cref="DatasyncConflictException">thrown if the service returns a conflict.</exception>
    /// <exception cref="DatasyncServiceException">thrown if the service returns an error.</exception>
    public static Task DeleteAsync<TEntity>(this IServiceClient<TEntity> client, TEntity entity, CancellationToken cancellationToken = default) where TEntity : ClientTableData, new()
        => client.DeleteAsync(entity, new ServiceOperationOptions(), cancellationToken);

    /// <summary>
    /// Deletes the provided entity if the entity version matches the server-side version..
    /// </summary>
    /// <param name="entity">The entity to be deleted.</param>
    /// <param name="options">Any options to be used in sending the request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes when the operation is complete.</returns>
    /// <exception cref="DatasyncConflictException">thrown if the service returns a conflict.</exception>
    /// <exception cref="DatasyncServiceException">thrown if the service returns an error.</exception>
    public static Task DeleteAsync<TEntity>(this IServiceClient<TEntity> client, TEntity entity, ServiceOperationOptions options, CancellationToken cancellationToken = default) where TEntity : ClientTableData, new()
    {
        options.RequireVersion ??= entity.Version;
        return client.DeleteAsync(entity.Id, options, cancellationToken);
    }

    /// <summary>
    /// Returns a page of entities from the service using the full data set.
    /// </summary>
    /// <param name="options">The query options to use for this operation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A page of entities from the service</returns>
    /// <exception cref="DatasyncServiceException">thrown if the service returns an error.</exception>
    public static Task<Page<TEntity>> QueryAsync<TEntity>(this IServiceClient<TEntity> client, CancellationToken cancellationToken = default) where TEntity : ClientTableData, new()
        => client.QueryAsync<TEntity>(new QueryOperationOptions(), cancellationToken);

    /// <summary>
    /// Replaces the server side entity with the entity provided if the entity version matches the server-side version.
    /// </summary>
    /// <param name="entity">The updated data for the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the saved entity when complete.</returns>
    /// <exception cref="DatasyncConflictException">thrown if the service returns a conflict.</exception>
    /// <exception cref="DatasyncServiceException">thrown if the service returns an error.</exception>
    public static Task<TEntity> ReplaceAsync<TEntity>(this IServiceClient<TEntity> client, TEntity entity, CancellationToken cancellationToken = default) where TEntity : ClientTableData, new()
        => client.ReplaceAsync(entity, new ServiceOperationOptions() { RequireVersion = entity.Version }, cancellationToken);
}

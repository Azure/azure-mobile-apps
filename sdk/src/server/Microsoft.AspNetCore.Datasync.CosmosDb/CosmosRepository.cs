// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.Azure.Cosmos;
using System.Net;
using System.Text;
using CosmosContainer = Microsoft.Azure.Cosmos.Container;

namespace Microsoft.AspNetCore.Datasync.CosmosDb;

public class CosmosRepository<TEntity> : IRepository<TEntity> where TEntity : CosmosTableData
{
    private bool _containerChecked = false;
    private readonly SemaphoreSlim _checkSemaphore = new(1, 1);
    private readonly CosmosContainer _container;
    private readonly ICosmosRepositoryOptions<TEntity> _options;

    /// <summary>
    /// Creates a new <see cref="CosmosRepository{TEntity}"/> using the provided container and
    /// default options.
    /// </summary>
    /// <param name="container">The Cosmos container to use for storing data.</param>
    public CosmosRepository(CosmosContainer container) : this(container, new CosmosRepositoryOptions<TEntity>())
    {
    }

    /// <summary>
    /// Creates a new <see cref="CosmosRepository{TEntity}"/> using the provided container and
    /// specific options.
    /// </summary>
    /// <param name="container">The Cosmos container to use for storing data.</param>
    /// <param name="options">The options to use for configuring this repository.</param>
    /// <exception cref="InvalidCastException">If the entity being used is invalid.</exception>
    public CosmosRepository(CosmosContainer container, ICosmosRepositoryOptions<TEntity> options)
    {
        if (typeof(TEntity).Name.Equals("User", StringComparison.OrdinalIgnoreCase))
        {
            // User is a reserved entity type name in Cosmos and cannot be used with .AsQueryable()
            throw new InvalidCastException("User is a reserved entity type name");
        }
        _container = container;
        _options = options;
    }

    /// <summary>
    /// Retrieves the <see cref="ItemRequestOptions"/> with an optional If-Match header set.
    /// </summary>
    /// <param name="version">The version required for the If-Match header.</param>
    /// <returns>The <see cref="ItemRequestOptions"/> to use for an operation.</returns>
    protected ItemRequestOptions ItemRequestOptionsWithVersion(byte[]? version)
    {
        if (version == null || version.Length == 0)
        {
            return _options.ItemRequestOptions;
        }

        ItemRequestOptions requestOptions = (ItemRequestOptions)_options.ItemRequestOptions.ShallowCopy();
        requestOptions.IfMatchEtag = Encoding.UTF8.GetString(version);
        return requestOptions;
    }

    /// <summary>
    /// Parses the provided ID into the entity ID and the partition key.
    /// </summary>
    /// <param name="id">The provided ID.</param>
    /// <param name="partitionKey">On return, set to the required partition key.</param>
    /// <returns>The entity ID, as stored in the database.</returns>
    /// <exception cref="HttpException">If the provided ID is not valid.</exception>
    internal string ParseEntityId(string id, out PartitionKey partitionKey)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new HttpException(HttpStatusCodes.Status400BadRequest);
        }
        string entityId = _options.TryGetPartitionKey(id, out partitionKey);
        if (string.IsNullOrEmpty(entityId))
        {
            throw new HttpException(HttpStatusCodes.Status400BadRequest);
        }
        return entityId;
    }

    /// <summary>
    /// Validates that the container exists in the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes when the operation is finished.</returns>
    /// <exception cref="InvalidOperationException">if the container does not exist.</exception>
    internal async ValueTask ValidateContainerExistsAsync(CancellationToken cancellationToken = default)
    {
        if (_containerChecked)
        {
            return;
        }
        await _checkSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_containerChecked)
            {
                return;
            }
            // This line throws if the container does not exist.
            _ = await _container.ReadContainerAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            _containerChecked = true;
        }
        finally
        {
            _checkSemaphore.Release();
        }
        throw new NotImplementedException();
    }

    /// <summary>
    /// Performs the provided action asynchronously, wrapping the exceptions that may be generated with the appropriate response exceptions.
    /// </summary>
    /// <param name="entityId">The entity ID being used for the operation.</param>
    /// <param name="partitionKey">The partition key being used for the operation.</param>
    /// <param name="action">The action to be performed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes when the operation is finished.</returns>
    /// <exception cref="HttpException"></exception>
    /// <exception cref="RepositoryException"></exception>
    internal async Task WrapExceptionAsync(string entityId, PartitionKey partitionKey, Func<Task> action, CancellationToken cancellationToken = default)
    {
        try
        {
            await action.Invoke().ConfigureAwait(false);
        }
        catch (CosmosException exception)
        {
            if (exception.StatusCode == HttpStatusCode.NotFound)
            {
                throw new HttpException(HttpStatusCodes.Status404NotFound);
            }
            else if (exception.StatusCode == HttpStatusCode.Conflict || exception.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                throw new HttpException((int)exception.StatusCode)
                {
                    Payload = await _container.ReadItemAsync<TEntity>(entityId, partitionKey, _options.ItemRequestOptions, cancellationToken).ConfigureAwait(false)
                };
            }
            else
            {
                throw new RepositoryException(exception.Message, exception);
            }
        }
    }

    /// <summary>
    /// Performs the provided action asynchronously, wrapping the exceptions that may be generated with the appropriate response exceptions, and returning the result.
    /// </summary>
    /// <param name="action">The action to be performed.</param>
    /// <returns>A task that returns the entity when the operation is finished.</returns>
    /// <exception cref="HttpException"></exception>
    /// <exception cref="RepositoryException"></exception>
    internal static async Task<TEntity> WrapExceptionAsync(Func<Task<TEntity>> action)
    {
        try
        {
            return await action.Invoke().ConfigureAwait(false);
        }
        catch (CosmosException exception)
        {
            if (exception.StatusCode == HttpStatusCode.NotFound)
            {
                throw new HttpException(HttpStatusCodes.Status404NotFound);
            }
            else
            {
                throw new RepositoryException(exception.Message, exception);
            }
        }
    }

    #region IRepository<TEntity
    /// <inheritdoc />
    public async ValueTask<IQueryable<TEntity>> AsQueryableAsync(CancellationToken cancellationToken = default)
    {
        await ValidateContainerExistsAsync(cancellationToken).ConfigureAwait(false);
        return _container.GetItemLinqQueryable<TEntity>(true);
    }

    /// <inheritdoc />
    public async ValueTask CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await ValidateContainerExistsAsync(cancellationToken).ConfigureAwait(false);

        string id = await _options.CreateEntityIdAsync(entity, cancellationToken).ConfigureAwait(false);
        string entityId = ParseEntityId(id, out PartitionKey partitionKey);
        await WrapExceptionAsync(entity.Id, partitionKey, async () =>
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            ItemResponse<TEntity> response = await _container.CreateItemAsync(entity, partitionKey, _options.ItemRequestOptions, cancellationToken).ConfigureAwait(false);
            entity.Id = id;
            entity.UpdatedAt = response.Resource.UpdatedAt;
            entity.Version = response.Resource.Version;
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DeleteAsync(string id, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        await ValidateContainerExistsAsync(cancellationToken).ConfigureAwait(false);
        string entityId = ParseEntityId(id, out PartitionKey partitionKey);
        await WrapExceptionAsync(entityId, partitionKey, async () =>
        {
            ItemRequestOptions requestOptions = ItemRequestOptionsWithVersion(version);
            await _container.DeleteItemAsync<TEntity>(entityId, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<TEntity> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        await ValidateContainerExistsAsync(cancellationToken).ConfigureAwait(false);
        string entityId = ParseEntityId(id, out PartitionKey partitionKey);
        return await CosmosRepository<TEntity>.WrapExceptionAsync(async () =>
        {
            TEntity entity = await _container.ReadItemAsync<TEntity>(entityId, partitionKey, _options.ItemRequestOptions, cancellationToken).ConfigureAwait(false);
            entity.Id = id; // We always get the same ID back, so we don't need to calculate it.
            return entity;
        });
    }

    /// <inheritdoc />
    public async ValueTask ReplaceAsync(TEntity entity, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        await ValidateContainerExistsAsync(cancellationToken).ConfigureAwait(false);
        string id = entity.Id;
        string entityId = ParseEntityId(id, out PartitionKey partitionKey);
        await WrapExceptionAsync(entityId, partitionKey, async () =>
        {
            entity.Id = entityId;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            ItemRequestOptions requestOptions = ItemRequestOptionsWithVersion(version);
            ItemResponse<TEntity> response = await _container.ReplaceItemAsync(entity, entityId, partitionKey, requestOptions, cancellationToken).ConfigureAwait(false);
            entity.Id = id;
            entity.UpdatedAt = response.Resource.UpdatedAt;
            entity.Version = response.Resource.Version;
        }, cancellationToken).ConfigureAwait(false);
    }
    #endregion
}

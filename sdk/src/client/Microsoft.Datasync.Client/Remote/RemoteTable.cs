// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Remote;

/// <summary>
/// Concrete implementation of the <see cref="IRemoteTable{TEntity}"/> that is used to communicate
/// with a backend datasync service.
/// </summary>
/// <typeparam name="TEntity">The type of entity stored in the table.</typeparam>
public class RemoteTable<TEntity> : IRemoteTable<TEntity> where TEntity : IClientTableData
{
    /// <summary>
    /// The default name of the datasync client.
    /// </summary>
    private const string DatasyncClientName = "DatasyncClient";

    /// <summary>
    /// Creates a new <see cref="RemoteTable{TEntity}"/> instance using the provided options.
    /// </summary>
    /// <param name="options">The options to use for this connection.</param>
    public RemoteTable(DatasyncClientOptions options) : this(options, DatasyncClientName)
    {
    }

    /// <summary>
    /// Creates a new <see cref="RemoteTable{TEntity}"/> instance using the provided options.
    /// </summary>
    /// <param name="options">The options to use for this connection.</param>
    /// <param name="clientName">The name of the HTTP client to create when requesting via the client factory.</param>
    public RemoteTable(DatasyncClientOptions options, string clientName)
    {
        Options = options;
        Client = options.HttpClientFactory.CreateClient(clientName);
        string path = ResolveTableEndpointPath();
        if (path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("http://"))
        {
            Endpoint = new Uri(path);
        }
        else
        {
            if (Client.BaseAddress == null)
            {
                throw new ArgumentException("The client factory did not provide a base address, which is required for relative paths");
            }
            Endpoint = new Uri(Client.BaseAddress, path);
        }
        Ensure.That(Endpoint).IsValidEndpoint();
    }

    /// <summary>
    /// The HTTP client to use for communicating with the remote service.
    /// </summary>
    internal HttpClient Client { get; }

    /// <summary>
    /// The absolute URI for the base endpoint of the remote table.
    /// </summary>
    internal Uri Endpoint { get; }

    /// <summary>
    /// The current options for this remote table.
    /// </summary>
    internal DatasyncClientOptions Options { get; }

    /// <summary>
    /// Given the type of the entity, resolve the endpoint path for the table.
    /// </summary>
    /// <returns>The path to the table endpoint.</returns>
    internal string ResolveTableEndpointPath()
    {
        string tableName = typeof(TEntity).Name;
        if (typeof(TEntity).GetCustomAttributes(typeof(OfflineDatasyncTableAttribute), true).FirstOrDefault() is OfflineDatasyncTableAttribute tableAttribute)
        {
            if (tableAttribute.Endpoint != null)
            {
                return tableAttribute.Endpoint;
            }
            else if (tableAttribute.Table != null)
            {
                tableName = tableAttribute.Table;
            }
        }
        return Options.TableEndpointResolver.Invoke(tableName);
    }

    #region IQueryable<TEntity>
    /// <inheritdoc />
    public Type ElementType
        => throw new NotImplementedException();

    /// <inheritdoc />
    public Expression Expression { get; internal set; } = Expression.Constant(null);

    /// <inheritdoc />
    public IQueryProvider Provider
        => throw new NotImplementedException();

    /// <inheritdoc />
    public IEnumerator<TEntity> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region IReadonlyRemoteTable<TEntity>
    /// <inheritdoc />
    public ValueTask<TEntity?> FindOrDefaultAsync(string id, RemoteOperationOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region IRemoteTable<TEntity>
    /// <inheritdoc />
    public ValueTask<TEntity> AddAsync(TEntity entity, RemoteOperationOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask RemoveAsync(string id, byte[]? version, RemoteOperationOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask<TEntity> UpdateAsync(TEntity entity, RemoteOperationOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    #endregion
}

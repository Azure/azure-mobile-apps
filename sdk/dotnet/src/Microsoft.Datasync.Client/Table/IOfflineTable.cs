// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// The read portion of the offline table API, providing access to untyped (JSON) objects
    /// within an offline table.
    /// </summary>
    public interface IReadOnlyOfflineTable
    {
        /// <summary>
        /// The service client being used for communication.
        /// </summary>
        DatasyncClient ServiceClient { get; }

        /// <summary>
        /// The name of the table.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Count the number of items that would be returned by the provided query, without returning
        /// all the values.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the number of items that will be in the result set when the query finishes.</returns>
        Task<long> CountItemsAsync(string query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute a query against a remote table.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns>A task that returns the results when the query finishes.</returns>
        IAsyncEnumerable<JObject> GetAsyncItems(string query);

        /// <summary>
        /// Retrieve an item from the remote table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        Task<JObject> GetItemAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pulls the items matching the provided query from the remote table.
        /// </summary>
        /// <param name="query">The OData query that determines which items to pull from the remote table.</param>
        /// <param name="options">The options used to configure the pull operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation has finished.</returns>
        Task PullItemsAsync(string query, PullOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all the items in the offline table that match the query.
        /// </summary>
        /// <param name="query">An OData query that determines which items to delete.</param>
        /// <param name="options">The options used to configure the purge operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the purge operation has finished.</returns>
        Task PurgeItemsAsync(string query, PurgeOptions options, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Definition of the operations that can be done against an offline table
    /// with untyped (JSON) object.
    /// </summary>
    public interface IOfflineTable : IReadOnlyOfflineTable
    {
        /// <summary>
        /// Deletes an item from the remote table.
        /// </summary>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the response when complete.</returns>
        Task DeleteItemAsync(JObject instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to insert into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the inserted data when complete.</returns>
        Task<JObject> InsertItemAsync(JObject instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pushes all operations for this table in the operations queue to the remote service.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the push operation has finished.</returns>
        Task PushItemsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Pushes all operations for this table in the operations queue to the remote service.
        /// </summary>
        /// <param name="options">The push operation options.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the push operation has finished.</returns>
        Task PushItemsAsync(PushOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to replace into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the replaced data when complete.</returns>
        Task ReplaceItemAsync(JObject instance, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// The read portion of the API to operate on a strongly typed offline table.
    /// </summary>
    /// <typeparam name="T">The type of the entities that the offline table contains.</typeparam>
    public interface IReadOnlyOfflineTable<T> : IReadOnlyOfflineTable, ILinqMethods<T>
    {
        /// <summary>
        /// Creates a blank query for the current table.
        /// </summary>
        /// <returns>A query against the table.</returns>
        ITableQuery<T> CreateQuery();

        /// <summary>
        /// Count the number of items that would be returned by the provided query, without returning
        /// all the values.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the number of items that will be in the result set when the query finishes.</returns>
        Task<long> CountItemsAsync(ITableQuery<T> query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all instances from the table as an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        IAsyncEnumerable<T> GetAsyncItems();

        /// <summary>
        /// Executes a query against the offline table.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned by the query.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        IAsyncEnumerable<U> GetAsyncItems<U>(ITableQuery<U> query);

        /// <summary>
        /// Retrieve an item from the remote table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        new Task<T> GetItemAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pulls the items matching the provided query from the remote table.
        /// </summary>
        /// <typeparam name="U">The type of the data transfer object (DTO) or model that is returned by the query.</typeparam>
        /// <param name="query">The OData query that determines which items to pull from the remote table.</param>
        /// <param name="options">The options used to configure the pull operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation has finished.</returns>
        Task PullItemsAsync<U>(ITableQuery<U> query, PullOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all the items in the offline table that match the query.
        /// </summary>
        /// <typeparam name="U">The type of the data transfer object (DTO) or model that is returned by the query.</typeparam>
        /// <param name="query">An OData query that determines which items to delete.</param>
        /// <param name="options">The options used to configure the purge operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the purge operation has finished.</returns>
        Task PurgeItemsAsync<U>(ITableQuery<U> query, PurgeOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes the current instance with the latest values from the table.
        /// </summary>
        /// <param name="instance">The instance to refresh.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        Task RefreshItemAsync(T instance, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Definition of the operations that can be done against an offline table with strongly typed objects.
    /// </summary>
    /// <typeparam name="T">The type of the entities that the offline table contains.</typeparam>
    public interface IOfflineTable<T> : IOfflineTable, IReadOnlyOfflineTable<T>
    {
        /// <summary>
        /// Deletes an item from the remote table.
        /// </summary>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation complete.</returns>
        Task DeleteItemAsync(T instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts the instance in the remote table.
        /// </summary>
        /// <param name="instance">The instance to insert.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        Task InsertItemAsync(T instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces the current instance with the provided instance in the remote table.
        /// </summary>
        /// <param name="instance">The instance to replace.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        Task ReplaceItemAsync(T instance, CancellationToken cancellationToken = default);
    }
}

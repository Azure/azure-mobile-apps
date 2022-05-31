// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Definition of the operations that can be done against a remote table
    /// with untyped (JSON) object.
    /// </summary>
    public interface IRemoteTable
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
        /// Deletes an item from the remote table.
        /// </summary>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the response when complete.</returns>
        Task<JToken> DeleteItemAsync(JObject instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute a query against a remote table.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns>A task that returns the results when the query finishes.</returns>
        IAsyncEnumerable<JToken> GetAsyncItems(string query);

        /// <summary>
        /// Retrieve an item from the remote table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        Task<JToken> GetItemAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to insert into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the inserted data when complete.</returns>
        Task<JToken> InsertItemAsync(JObject instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to replace into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the replaced data when complete.</returns>
        Task<JToken> ReplaceItemAsync(JObject instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Undeletes an item in the remote table.
        /// </summary>
        /// <remarks>
        /// This requires that the table supports soft-delete.
        /// </remarks>
        /// <param name="instance">The instance to undelete in the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the response when complete.</returns>
        Task<JToken> UndeleteItemAsync(JObject instance, CancellationToken cancellationToken = default);
    }

    public interface IRemoteTable<T> : IRemoteTable, ILinqMethods<T>
    {
        /// <summary>
        /// Creates a blank query for the current table.
        /// </summary>
        /// <returns>A query against the table.</returns>
        ITableQuery<T> CreateQuery();

        /// <summary>
        /// Deletes an item from the remote table.
        /// </summary>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation complete.</returns>
        Task DeleteItemAsync(T instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all instances from the table as an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        IAsyncEnumerable<T> GetAsyncItems();

        /// <summary>
        /// Executes a query against the remote table.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned by the query.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        IAsyncEnumerable<U> GetAsyncItems<U>(string query);

        /// <summary>
        /// Executes a query against the remote table.
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
        /// Inserts the instance in the remote table.
        /// </summary>
        /// <param name="instance">The instance to insert.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        Task InsertItemAsync(T instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes the current instance with the latest values from the table.
        /// </summary>
        /// <param name="instance">The instance to refresh.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        Task RefreshItemAsync(T instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces the current instance with the provided instance in the remote table.
        /// </summary>
        /// <param name="instance">The instance to replace.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        Task ReplaceItemAsync(T instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Undeletes an item in the remote table.
        /// </summary>
        /// <remarks>
        /// This requires that the table supports soft-delete.
        /// </remarks>
        /// <param name="instance">The instance to undelete in the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation complete.</returns>
        Task UndeleteItemAsync(T instance, CancellationToken cancellationToken = default);
    }
}

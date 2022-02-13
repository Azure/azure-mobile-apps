// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Definition of the base operations you can perform on a datasync table.
    /// </summary>
    /// <remarks>
    /// The ID property within the DTO must either be marked with the <c>Key</c>
    /// attribute or must be named "Id".</remarks>
    /// <typeparam name="T">The type of the data stored in the datasync table</typeparam>
    public interface IRemoteTable<T> : IRemoteTable, ILinqMethods<T>
    {
        /// <summary>
        /// Creates a query based on this table.
        /// </summary>
        /// <returns>A query against this table.</returns>
        ITableQuery<T> CreateQuery();

        /// <summary>
        /// Deletes an existing item within the table.
        /// </summary>
        /// <param name="item">The ID of the item to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="ServiceResponse"/> object.</returns>
        Task<ServiceResponse> DeleteItemAsync(T item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of items based on a query.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned - can be a subset of properties in the table entity</typeparam>
        /// <param name="query">The query string to send to the service</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="AsyncPageable{T}"/> for retrieving the items asynchronously.</returns>
        AsyncPageable<U> GetAsyncItems<U>(string query = "", CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of items based on a query.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned - can be a subset of properties in the table entity</typeparam>
        /// <param name="query">The query definition to send to the service</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe</param>
        /// <returns>An <see cref="AsyncPageable{T}"/> for retrieving the items asynchronously.</returns>
        AsyncPageable<U> GetAsyncItems<U>(ITableQuery<U> query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        new Task<ServiceResponse<T>> GetItemAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new item within the table.
        /// </summary>
        /// <param name="item">The item to add to the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        Task<ServiceResponse<T>> InsertItemAsync(T item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refresh the current instance with the latest values from the table.
        /// </summary>
        /// <param name="item">The item to refresh.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The refreshed item.</returns>
        Task<T> RefreshItemAsync(T item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replace the item with a new copy of the item.  Note that the item must have an Id (either called Id,
        /// or decorated with the <see cref="KeyAttribute"/> attribute) that is a string and set.
        /// </summary>
        /// <param name="item">the replacement item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        Task<ServiceResponse<T>> ReplaceItemAsync(T item, CancellationToken cancellationToken = default);

        /// <summary>
        ///Update the specified item with the provided changes.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="changes">A list of changes to apply to the item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        new Task<ServiceResponse<T>> UpdateItemAsync(string id, IReadOnlyDictionary<string, object> changes, IfMatch precondition = null, CancellationToken cancellationToken = default);
    }
}

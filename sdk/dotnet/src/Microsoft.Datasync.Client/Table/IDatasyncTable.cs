// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Table;
using System;
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
    public interface IDatasyncTable<T> where T : notnull
    {
        /// <summary>
        /// Converts the current table to a table with a new type, but the same endpoint, client, and options.
        /// </summary>
        /// <typeparam name="U">The new type of the supported items</typeparam>
        /// <returns>The new table</returns>
        IDatasyncTable<U> WithType<U>() where U : notnull;

        /// <summary>
        /// The base <see cref="Uri"/> for the table.
        /// </summary>
        Uri Endpoint { get; }

        /// <summary>
        /// The <see cref="DatasyncClientOptions"/> for the table.
        /// </summary>
        DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// Creates a new item within the table.
        /// </summary>
        /// <param name="item">The item to add to the table.</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        Task<HttpResponse<T>> CreateItemAsync(T item, CancellationToken token = default);

        /// <summary>
        /// Deletes an existing item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to delete.</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse"/> object.</returns>
        Task<HttpResponse> DeleteItemAsync(string id, HttpCondition? precondition = null, CancellationToken token = default);

        /// <summary>
        /// Retrieves a list of items based on a query.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned - can be a subset of properties in the table entity</typeparam>
        /// <param name="query">The query string to send to the service</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="AsyncPageable{T}"/> for retrieving the items asynchronously.</returns>
        AsyncPageable<U> GetAsyncItems<U>(string query = "", CancellationToken token = default) where U : notnull;

        /// <summary>
        /// Retrieves an item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        Task<HttpResponse<T>> GetItemAsync(string id, HttpCondition? precondition = null, CancellationToken token = default);

        /// <summary>
        /// Replace the item with a new copy of the item.  Note that the item must have an Id (either called Id,
        /// or decorated with the <see cref="KeyAttribute"/> attribute) that is a string and set.
        /// </summary>
        /// <param name="item">the replacement item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        Task<HttpResponse<T>> ReplaceItemAsync(T item, HttpCondition? precondition = null, CancellationToken token = default);

        /// <summary>
        ///Update the specified item with the provided changes.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="changes">A list of changes to apply to the item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        Task<HttpResponse<T>> UpdateItemAsync(string id, IReadOnlyDictionary<string, object> changes, HttpCondition? precondition = null, CancellationToken token = default);
    }
}

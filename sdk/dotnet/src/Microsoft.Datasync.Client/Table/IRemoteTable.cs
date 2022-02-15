// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Table;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// The definition on how we communicate with a datasync service using JSON objects.
    /// </summary>
    public interface IRemoteTable
    {
        /// <summary>
        /// The base <see cref="Uri"/> for the table.
        /// </summary>
        Uri Endpoint { get; }

        /// <summary>
        /// The <see cref="DatasyncClientOptions"/> for the table.
        /// </summary>
        DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// An event that is fired when the table is modified - an entity is either
        /// created, deleted, or updated.
        /// </summary>
        event EventHandler<TableModifiedEventArgs> TableModified;

        /// <summary>
        /// Deletes an item from the store.  If the version is provided, then the document
        /// is only deleted if the store version matches the provided version.
        /// </summary>
        /// <remarks>
        /// The item provided must have an ID, and may have a version.  All other properties
        /// on the JSON document are ignored.
        /// </remarks>
        /// <param name="item">The item to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The service response if successful.</returns>
        /// <exception cref="ArgumentException">if the item provided does not have an ID.</exception>
        /// <exception cref="DatasyncConflictException{T}">if there is a version mismatch.</exception>
        /// <exception cref="DatasyncOperationException">if an HTTP error is received from the service.</exception>
        Task<ServiceResponse> DeleteItemAsync(JsonDocument item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of items based on a query.
        /// </summary>
        /// <param name="query">The query string to send to the service.  This can be any OData compatible query string.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>An <see cref="AsyncPageable{T}"/> for retrieving the items asynchronously.</returns>
        AsyncPageable<JsonDocument> GetAsyncItems(string query = "", CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The service response containing the object if successful.</returns>
        Task<ServiceResponse<JsonDocument>> GetItemAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts an item into the store. 
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The service response containing the inserted object if successful.</returns>
        /// <exception cref="DatasyncConflictException{T}">if the item to insert has an ID and that ID already exists in the table.</exception>
        /// <exception cref="DatasyncOperationException">if an HTTP error is received from the service.</exception>
        Task<ServiceResponse<JsonDocument>> InsertItemAsync(JsonDocument item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replace the item with a new copy of the item.
        /// </summary>
        /// <remarks>
        /// The item must contain an "id" field.  If the item also contains a "version" field, the version
        /// must match the version on the server.</remarks>
        /// <param name="item">the replacement item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        /// <exception cref="DatasyncConflictException{T}">if there is a version mismatch.</exception>
        /// <exception cref="DatasyncOperationException">if an HTTP error is received from the service.</exception>
        Task<ServiceResponse<JsonDocument>> ReplaceItemAsync(JsonDocument item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the specified item with the provided changes.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="changes">A list of changes to apply to the item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        Task<ServiceResponse<JsonDocument>> UpdateItemAsync(string id, IReadOnlyDictionary<string, object> changes, IfMatch precondition = null, CancellationToken cancellationToken = default);
    }
}

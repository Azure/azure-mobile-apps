﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Microsoft.Zumo.MobileData.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Zumo.MobileData
{
    /// <summary>
    /// Provides operations that handle a single Azure Mobile Table.
    /// </summary>
    /// <typeparam name="T">The type of the entity stored in the table.</typeparam>
    public class MobileTable<T> where T : TableData
    {
        /// <summary>
        /// Initializes a <see cref="MobileDataTable{T}"/> instance.
        /// </summary>
        /// <param name="client">The <see cref="MobileTableClient"/> that created this table reference.</param>
        /// <param name="endpoint">The absolute Uri to the table controller endpoint.</param>
        internal MobileTable(MobileTableClient client, Uri endpoint)
        {
            Arguments.IsAbsoluteUri(endpoint, nameof(endpoint));

            Client = new ServiceRestClient<T>(endpoint, client.Credential, client.ClientOptions);
        }

        /// <summary>
        /// The REST Client used for communicating with to the service
        /// </summary>
        internal ServiceRestClient<T> Client { get; }

        #region GetMetadata
        /// <summary>
        /// Obtains the table metadata.
        /// </summary>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The table metadata</returns>
        public virtual Task<Response<MobileTableMetadata>> GetMetadataAsync(CancellationToken cancellationToken = default)
            => GetMetadataAsync(new MobileTableQueryOptions(), cancellationToken);

        /// <summary>
        /// Obtains the table metadata.  The count returned is contstrained by the query provided.
        /// </summary>
        /// <param name="query">The <see cref="MobileTableQueryOptions"/> describing the query</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The table metadata</returns>
        public virtual async Task<Response<MobileTableMetadata>> GetMetadataAsync(MobileTableQueryOptions query, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(query, nameof(query));

            using Request request = Client.CreateGetMetadataRequest(query);
            Response response = await Client.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            return response.Status switch
            {
                200 => await Client.CreateResponseAsync<MobileTableMetadata>(response, cancellationToken).ConfigureAwait(false),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase),
            };
        }

        /// <summary>
        /// Obtains the table metadata.
        /// </summary>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The table metadata</returns>
        public virtual Response<MobileTableMetadata> GetMetadata(CancellationToken cancellationToken = default)
            => GetMetadata(new MobileTableQueryOptions(), cancellationToken);

        /// <summary>
        /// Obtains the table metadata.  The count returned is contstrained by the query provided.
        /// </summary>
        /// <param name="query">The <see cref="MobileTableQueryOptions"/> describing the query</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The table metadata</returns>
        public virtual Response<MobileTableMetadata> GetMetadata(MobileTableQueryOptions query, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(query, nameof(query));

            using Request request = Client.CreateGetMetadataRequest(query);
            Response response = Client.SendRequest(request, cancellationToken);

            return response.Status switch
            {
                200 => Client.CreateResponse<MobileTableMetadata>(response, cancellationToken),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase),
            };
        }
        #endregion

        #region DeleteItem
        /// <summary>
        /// Deletes an item from the backend table.  By default, the item is only deleted if it matches the item version.
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <param name="conditions">The set of conditional request options to apply as headers to the request.</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A <see cref="Response"/> from the backend service.</returns>
        public virtual async Task<Response> DeleteItemAsync(T item, MatchConditions conditions = null, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            using Request request = Client.CreateDeleteRequest(item, conditions);
            Response response = await Client.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            switch (response.Status)
            {
                case 200:
                case 204:
                    return response;
                case 412:
                    throw new ConflictException<T>(Client.CreateResponse(response, cancellationToken));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Deletes an item from the backend table.  By default, the item is only deleted if it matches the item version.
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <param name="conditions">The set of conditional request options to apply as headers to the request.</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A <see cref="Response"/> from the backend service.</returns>
        public virtual Response DeleteItem(T item, MatchConditions conditions = null, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            using Request request = Client.CreateDeleteRequest(item, conditions);
            Response response = Client.Pipeline.SendRequest(request, cancellationToken);

            switch (response.Status)
            {
                case 200:
                case 204:
                    return response;
                case 412:
                    throw new ConflictException<T>(Client.CreateResponse(response, cancellationToken));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }
        #endregion

        #region GetItem
        /// <summary>
        /// Retrieves an item from the backend table, using the ID as an identifier.
        /// </summary>
        /// <param name="id">The ID of the record to retrieve</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The item as it appears in the backend service</returns>
        public virtual async Task<Response<T>> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNullOrEmpty(id, nameof(id));

            using Request request = Client.CreateGetRequest(id, default);
            Response response = await Client.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            return response.Status switch
            {
                200 => await Client.CreateResponseAsync(response, cancellationToken).ConfigureAwait(false),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase),
            };
        }

        /// <summary>
        /// Retrieves an item from the backend table, using the ID as an identifier.
        /// </summary>
        /// <param name="id">The ID of the record to retrieve</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The item as it appears in the backend service</returns>
        public virtual Response<T> GetItem(string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNullOrEmpty(id, nameof(id));

            using Request request = Client.CreateGetRequest(id, default);
            Response response = Client.SendRequest(request, cancellationToken);

            return response.Status switch
            {
                200 => Client.CreateResponse(response, cancellationToken),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase),
            };
        }
        #endregion

        #region GetItems
        /// <summary>
        /// Retrieves a list of items from the service.
        /// </summary>
        /// <param name="options">The query to send to the remote server</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A pageable list of items</returns>
        public virtual AsyncPageable<T> GetItemsAsync(MobileTableQueryOptions options = null, CancellationToken cancellationToken = default)
        {
            return PageResponseEnumerator.CreateAsyncEnumerable(nextLink => GetItemsPageAsync(options, nextLink, cancellationToken));
        }

        /// <summary>
        /// Retrieves a list of items from the service.
        /// </summary>
        /// <param name="options">The query to send to the remote server</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A pageable list of items</returns>
        public virtual Pageable<T> GetItems(MobileTableQueryOptions options = null, CancellationToken cancellationToken = default)
        {
            return PageResponseEnumerator.CreateEnumerable(nextLink => GetItemsPage(options, nextLink, cancellationToken));
        }

        /// <summary>
        /// Fetches a single page in the server-side paging result of a list operation.
        /// </summary>
        /// <param name="options">The query to send to the service</param>
        /// <param name="pageLink">The link to the page</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A single page of results</returns>
        private async Task<Page<T>> GetItemsPageAsync(MobileTableQueryOptions options, string pageLink, CancellationToken cancellationToken = default)
        {
            using Request request = Client.CreateListRequest(options, pageLink);
            Response response = await Client.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            switch (response.Status)
            {
                case 200:
                    PagedResult<T> data = await Client.CreatePagedResultAsync(response, cancellationToken).ConfigureAwait(false);
                    return Page<T>.FromValues(data.Values, data.NextLink, response);
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Fetches a single page in the server-side paging result of a list operation.
        /// </summary>
        /// <param name="options">The query to send to the service</param>
        /// <param name="pageLink">The link to the page</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A single page of results</returns>
        private Page<T> GetItemsPage(MobileTableQueryOptions options, string pageLink, CancellationToken cancellationToken = default)
        {
            using Request request = Client.CreateListRequest(options, pageLink);
            Response response = Client.SendRequest(request, cancellationToken);

            switch (response.Status)
            {
                case 200:
                    PagedResult<T> data = Client.CreatePagedResult(response, cancellationToken);
                    return Page<T>.FromValues(data.Values, data.NextLink, response);
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }
        #endregion

        #region InsertItem
        /// <summary>
        /// Inserts a new item into the backend table.  The item must not already exist.  If an Id
        /// is not provided in the local item, one will be added.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The item that was stored in the backend service</returns>
        public virtual async Task<Response<T>> InsertItemAsync(T item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));

            using Request request = Client.CreateInsertRequest(item);
            Response response = await Client.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            switch (response.Status)
            {
                case 201:
                    return await Client.CreateResponseAsync(response, cancellationToken).ConfigureAwait(false);
                case 409:
                case 412:
                    throw new ConflictException<T>(await Client.CreateResponseAsync(response, cancellationToken).ConfigureAwait(false));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Inserts a new item into the backend table.  The item must not already exist.  If an Id
        /// is not provided in the local item, one will be added.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The item that was stored in the backend service</returns>
        public virtual Response<T> InsertItem(T item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));

            using Request request = Client.CreateInsertRequest(item);
            Response response = Client.SendRequest(request, cancellationToken);

            switch (response.Status)
            {
                case 201:
                    return Client.CreateResponse(response, cancellationToken);
                case 409:
                case 412:
                    throw new ConflictException<T>(Client.CreateResponse(response, cancellationToken));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }
        #endregion

        #region ReplaceItem
        /// <summary>
        /// Replaces the item in the backend service with the local item, providing additional match conditions are met.  By default, replacement
        /// only happens if the version provided matches the version on the server.
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <param name="conditions">The <see cref="MatchConditions"/> to be met</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The updated item, as it appears in the backend service.</returns>
        public virtual async Task<Response<T>> ReplaceItemAsync(T item, MatchConditions conditions = null, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNull(item.Id, nameof(item.Id));

            using Request request = Client.CreateReplaceRequest(item, conditions);
            Response response = await Client.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            switch (response.Status)
            {
                case 200:
                    return await Client.CreateResponseAsync(response, cancellationToken).ConfigureAwait(false);
                case 409:
                case 412:
                    throw new ConflictException<T>(await Client.CreateResponseAsync(response, cancellationToken).ConfigureAwait(false));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Replaces the item in the backend service with the local item, providing additional match conditions are met.  By default, replacement
        /// only happens if the version provided matches the version on the server.
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <param name="conditions">The <see cref="MatchConditions"/> to be met</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The updated item, as it appears in the backend service.</returns>
        public virtual Response<T> ReplaceItem(T item, MatchConditions conditions = null, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNull(item.Id, nameof(item.Id));

            using Request request = Client.CreateReplaceRequest(item, conditions);
            Response response = Client.SendRequest(request, cancellationToken);

            switch (response.Status)
            {
                case 200:
                    return Client.CreateResponse(response, cancellationToken);
                case 409:
                case 412:
                    throw new ConflictException<T>(Client.CreateResponse(response, cancellationToken));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }
        #endregion
    }
}
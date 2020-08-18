// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Microsoft.Zumo.MobileData.Internal;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Microsoft.Zumo.MobileData
{
    /// <summary>
    /// Implementation of <see cref="AsyncPageable{T}"/> that also implements a first page / next page logic
    /// that records the count, if available.
    /// </summary>
    /// <typeparam name="T">The type of entity that is being paged through</typeparam>
    public class MobileTableAsyncPageable<T> : AsyncPageable<T> where T : TableData
    {
        private readonly ServiceRestClient<T> _client;
        private readonly MobileTableQueryOptions _options;
        private CancellationToken _token;
        private long nResults = 0;

        internal MobileTableAsyncPageable(ServiceRestClient<T> client, MobileTableQueryOptions options, CancellationToken cancellationToken = default)
        {
            _client = client;
            _options = options;
            _token = cancellationToken;
        }

        /// <summary>
        /// The count of the number of entities that will be returned by this pageable.  This is an
        /// approximation since the count can change over time.
        /// </summary>
        public long? Count { get; private set; }

        /// <summary>
        /// Obtains the enumerated list response as a set of pages with consecutive requests to the service.
        /// </summary>
        /// <param name="continuationToken">The continuation token (really the skip value)</param>
        /// <param name="pageSizeHint">The size of pages needed/</param>
        /// <returns></returns>
        public override async IAsyncEnumerable<Page<T>> AsPages(string? continuationToken = default, int? pageSizeHint = default)
        {
            do
            {
                Page<T> pageResponse = (continuationToken == null) ? await GetFirstPageAsync(pageSizeHint) : await GetNextPageAsync(continuationToken, pageSizeHint);
                yield return pageResponse;
                continuationToken = pageResponse.ContinuationToken;
            } while (continuationToken != null);
        }

        /// <summary>
        /// Obtains the first page in the sequence of pages.
        /// </summary>
        /// <param name="pageSizeHint">The size of pages needed</param>
        /// <returns></returns>
        private async Task<Page<T>> GetFirstPageAsync(int? pageSizeHint = null)
        {
            using Request request = _client.CreateListPageRequest(_options, null, true, pageSizeHint);
            Response response = await _client.SendRequestAsync(request, _token).ConfigureAwait(false);
            switch (response.Status)
            {
                case 200:
                    PagedResult<T> data = await _client.DeserializeAsync<PagedResult<T>>(response, _token).ConfigureAwait(false);
                    Count = data.Count;
                    return PageOfData(data.Results, response);
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Obtains subsequent pages (after the first page) in the sequence of pages.
        /// </summary>
        /// <param name="continuationToken">The skip value</param>
        /// <param name="pageSizeHint">The size of pages needed</param>
        /// <returns></returns>
        private async Task<Page<T>> GetNextPageAsync(string continuationToken, int? pageSizeHint = null)
        {
            using Request request = _client.CreateListPageRequest(_options, continuationToken, false, pageSizeHint);
            Response response = await _client.SendRequestAsync(request, _token).ConfigureAwait(false);
            switch (response.Status)
            {
                case 200:
                    T[] data = await _client.DeserializeAsync<T[]>(response, _token).ConfigureAwait(false);
                    return PageOfData(data, response);
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Converts a list of items into a <see cref="Page{T}"/>
        /// </summary>
        /// <param name="data">the array of data</param>
        /// <param name="response">the response object</param>
        /// <returns></returns>
        private Page<T> PageOfData(T[] data, Response response)
        {
            nResults += data.Length;
            return Page<T>.FromValues(data, data.Length > 0 ? $"{nResults}" : null, response);
        }
    }
}

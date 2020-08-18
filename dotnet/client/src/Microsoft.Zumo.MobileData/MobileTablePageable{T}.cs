using Azure;
using Azure.Core;
using Microsoft.Zumo.MobileData.Internal;
using System.Collections.Generic;
using System.Threading;

#nullable enable

namespace Microsoft.Zumo.MobileData
{
    /// <summary>
    /// Implementation of <see cref="Pageable{T}"/> that also implements a first page / next page logic
    /// that records the count, if available.
    /// </summary>
    /// <typeparam name="T">The type of entity that is being paged through</typeparam>
    public class MobileTablePageable<T> : Pageable<T> where T : TableData
    {
        private readonly ServiceRestClient<T> _client;
        private readonly MobileTableQueryOptions _options;
        private CancellationToken _token;
        private long nResults = 0;

        internal MobileTablePageable(ServiceRestClient<T> client, MobileTableQueryOptions options, CancellationToken cancellationToken = default)
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
        public override IEnumerable<Page<T>> AsPages(string? continuationToken = default, int? pageSizeHint = default)
        {
            do
            {
                Page<T> pageResponse = (continuationToken == null) ? GetFirstPage(pageSizeHint) : GetNextPage(continuationToken, pageSizeHint);
                yield return pageResponse;
                continuationToken = pageResponse.ContinuationToken;
            } while (continuationToken != null);
        }

        /// <summary>
        /// Obtains the first page in the sequence of pages.
        /// </summary>
        /// <param name="pageSizeHint">The size of pages needed</param>
        /// <returns></returns>
        private Page<T> GetFirstPage(int? pageSizeHint = null)
        {
            using Request request = _client.CreateListPageRequest(_options, null, true, pageSizeHint);
            Response response = _client.SendRequest(request, _token);
            switch (response.Status)
            {
                case 200:
                    PagedResult<T> data = _client.Deserialize<PagedResult<T>>(response, _token);
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
        private Page<T> GetNextPage(string continuationToken, int? pageSizeHint = null)
        {
            using Request request = _client.CreateListPageRequest(_options, continuationToken, false, pageSizeHint);
            Response response = _client.SendRequest(request, _token);
            switch (response.Status)
            {
                case 200:
                    T[] data = _client.Deserialize<T[]>(response, _token);
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

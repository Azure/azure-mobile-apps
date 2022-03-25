// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Table
{
    /// <summary>
    /// A collection of values that may take multiple service requests to iterate over.
    /// </summary>
    /// <typeparam name="T">The type of the entity being iterated over.</typeparam>
    /// <example>
    /// Example of enumerating an <see cref="AsyncPageable{T}"/> using the <c>async foreach</c> loop:
    /// <code snippet="Snippet:AsyncPageable">
    /// AsyncPageable&lt;Movie&gt; allMovies = table.GetItemsAsync();
    /// await foreach (Movie movie in allMovies)
    /// {
    ///   ProcessMovie(movie);
    /// }
    /// </code>
    /// </example>
    public abstract class AsyncPageable<T> : IAsyncEnumerable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncPageable{T}"/> class.
        /// </summary>
        /// <remarks>
        /// Used for unit test mocking.
        /// </remarks>
        protected AsyncPageable()
        {
        }

        /// <summary>
        /// Enumerate the values a <see cref="Page{T}"/> of items at a time.  This may make multiple service requests.
        /// </summary>
        /// <param name="continuationToken">A continuation token indicating where to resume paging, or null to begin paging from the beginning</param>
        /// <returns>An async sequence of <see cref="Page{T}"/>s</returns>
        public abstract IAsyncEnumerable<Page<T>> AsPages(string continuationToken = default);

        /// <summary>
        /// Enumerate the values in the collection asynchronously.  This may make multiple service requests.
        /// </summary>
        /// <param name="token">A <see cref="CancellationToken"/> used for requests made while enumerating asynchronously.</param>
        /// <returns>An async sequence of values.</returns>
        public virtual async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token = default)
        {
            await foreach (Page<T> page in AsPages().ConfigureAwait(false).WithCancellation(token))
            {
                Count = page.Count;
                if (page.Items != null)
                {
                    foreach (T value in page.Items)
                    {
                        yield return value;
                    }
                }
            }
        }

        /// <summary>
        /// The total number of items that would be returned by the query, if not for paging.
        /// This is populated only if the total count ($count=true) is requested on the query.
        /// </summary>
        public long? Count { get; private set; }
    }

    /// <summary>
    /// A concrete implementation of the <see cref="AsyncPageable{T}"/> class that reads
    /// each page in a query from a paging function that does the actual request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class FuncAsyncPageable<T> : AsyncPageable<T>
    {
        /// <summary>
        /// The internal storage for the paging function to call for the next page.
        /// </summary>
        private readonly Func<string, Task<Page<T>>> _pageFunc;

        /// <summary>
        /// Creates a new <see cref="AsyncPageable{T}"/> with a function iterator.
        /// </summary>
        /// <param name="pageFunc">The function that gets the next page</param>
        public FuncAsyncPageable(Func<string, Task<Page<T>>> pageFunc)
        {
            _pageFunc = pageFunc;
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<Page<T>> AsPages(string requestUri = default)
        {
            do
            {
                Page<T> pageResponse = await _pageFunc(requestUri).ConfigureAwait(false);
                requestUri = pageResponse.NextLink?.ToString();
                yield return pageResponse ?? new Page<T>();
            } while (requestUri != null);
        }
    }
}

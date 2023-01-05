// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A set of extension methods for the <see cref="IOfflineTable"/> and <see cref="IOfflineTable{T}"/> classes.
    /// </summary>
    public static class IOfflineTableExtensions
    {
        /// <summary>
        /// Pull all items from the remote table to the offline table.
        /// </summary>
        /// <param name="table">The table reference.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation is complete.</returns>
        public static Task PullItemsAsync(this IOfflineTable table, CancellationToken cancellationToken = default)
           => table.PullItemsAsync(string.Empty, new PullOptions(), cancellationToken);

        /// <summary>
        /// Pull all items from the remote table to the offline table.
        /// </summary>
        /// <param name="table">The table reference.</param>
        /// <param name="options">The pull options to use.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation is complete.</returns>
        public static Task PullItemsAsync(this IOfflineTable table, PullOptions options, CancellationToken cancellationToken = default)
            => table.PullItemsAsync(string.Empty, options, cancellationToken);

        /// <summary>
        /// Pull all items matching the OData query string from the remote table to the offline table.
        /// </summary>
        /// <param name="table">The table reference.</param>
        /// <param name="query">The OData query string.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation is complete.</returns>
        public static Task PullItemsAsync(this IOfflineTable table, string query, CancellationToken cancellationToken = default)
            => table.PullItemsAsync(query, new PullOptions(), cancellationToken);

        /// <summary>
        /// Pull all items matching the LINQ query from the remote table to the offline table.
        /// </summary>
        /// <param name="table">The table reference.</param>
        /// <param name="query">The LINQ query.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation is complete.</returns>
        public static Task PullItemsAsync<T, U>(this IOfflineTable<T> table, ITableQuery<U> query, CancellationToken cancellationToken = default)
            => table.PullItemsAsync(query, new PullOptions(), cancellationToken);

        /// <summary>
        /// Count the number of items that would be returned from the table without returning all the values.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the number of items that will be in the result set when the query finishes.</returns>
        public static Task<long> CountItemsAsync<T>(this IOfflineTable<T> table, CancellationToken cancellationToken = default)
            => table.CountItemsAsync(table.CreateQuery(), cancellationToken);
    }
}

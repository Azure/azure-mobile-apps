// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A set of extension methods for the <see cref="IRemoteTable"/> and <see cref="IRemoteTable{T}"/> classes.
    /// </summary>
    public static class IRemoteTableExtensions
    {
        /// <summary>
        /// Count the number of items that would be returned from the table without returning all the values.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the number of items that will be in the result set when the query finishes.</returns>
        public static Task<long> CountItemsAsync<T>(this IRemoteTable<T> table, CancellationToken cancellationToken = default)
            => table.CountItemsAsync(table.CreateQuery(), cancellationToken);
    }
}

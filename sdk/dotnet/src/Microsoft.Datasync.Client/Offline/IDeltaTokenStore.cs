// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;
using System;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// Definition of the delta token store, which holds the last sync times
    /// for a specific query.
    /// </summary>
    /// <remarks>
    /// Delta tokens are stored for each query (identified by a user-specified queryId) to determine
    /// the minimum value for updatedAt that must be queried to get incremental changes to the table.
    /// </remarks>
    public interface IDeltaTokenStore
    {
        /// <summary>
        /// Obtains the current delta token for a table/queryId from persistent store.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="queryId">The query ID of the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the delta token when complete.</returns>
        Task<DateTimeOffset> GetDeltaTokenAsync(string tableName, string queryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets the delta token for a table/queryId from persistent store.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="queryId">The query ID of the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the delta token has been reset.</returns>
        Task ResetDeltaTokenAsync(string tableName, string queryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the delta token for a table/queryId from persistent store.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="queryId">The query ID of the table.</param>
        /// <param name="deltaToken">The value of the delta token.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the delta token has been set in the persistent store.</returns>
        Task SetDeltaTokenAsync(string tableName, string queryId, DateTimeOffset deltaToken, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provider interface that is added to an offline store when the offline store implements its
    /// own delta token store.
    /// </summary>
    public interface IDeltaTokenStoreProvider
    {
        /// <summary>
        /// Retrieves a fully configured <see cref="IDeltaTokenStore"/> from the offline store.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that resolves to the delta token store when complete.</returns>
        Task<IDeltaTokenStore> GetDeltaTokenStoreAsync(CancellationToken cancellationToken = default);
    }
}
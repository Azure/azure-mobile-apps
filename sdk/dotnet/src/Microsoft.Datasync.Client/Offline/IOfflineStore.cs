// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// Allows saving and reading data in the local offline tables.  Implementors of
    /// new offline stores should implement based on <see cref="AbstractOfflineStore"/>, and not
    /// this interface.
    /// </summary>
    public interface IOfflineStore : IDisposable
    {
        /// <summary>
        /// Deletes items from the table where the items are identified by a query.
        /// </summary>
        /// <param name="query">A query description that identifies the items to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task the completes when the items have been deleted from the table.</returns>
        Task DeleteAsync(QueryDescription query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes items from the table where the items are identified by their ID.
        /// </summary>
        /// <param name="tableName">The name of the table where the items are located.</param>
        /// <param name="ids">A list of IDs to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task the completes when the items have been deleted from the table.</returns>
        Task DeleteAsync(string tableName, IEnumerable<string> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns items from the table.
        /// </summary>
        /// <param name="query">A query describing the items to be returned.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> of the items returned.</returns>
        IAsyncEnumerable<JToken> GetAsyncItems(QueryDescription query);

        /// <summary>
        /// Returns a single item by the ID of the item.
        /// </summary>
        /// <param name="tableName">The table name holding the item.</param>
        /// <param name="id">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        Task<JObject> GetItemAsync(string tableName, string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes the store for use.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the store is ready to use.</returns>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates or inserts the item(s) provided in the table.
        /// </summary>
        /// <param name="tableName">The table to be used for the operation.</param>
        /// <param name="items">The item(s) to be updated or inserted.</param>
        /// <param name="ignoreMissingColumns">If <c>true</c>, extra properties on the item can be ignored.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item has been updated or inserted into the table.</returns>
        Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// A set of offline store extension methods.
    /// </summary>
    internal static class IOfflineStoreExtensions
    {
        /// <summary>
        /// Deletes an item with the specified id in the local table.
        /// </summary>
        /// <param name="store">Instance of <see cref="IOfflineStore"/></param>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="id">Id for the object to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that compltes when delete has been executed on local table.</returns>
        public static Task DeleteAsync(this IOfflineStore store, string tableName, string id, CancellationToken cancellationToken = default)
            => store.DeleteAsync(tableName, new[] { id }, cancellationToken);

        /// <summary>
        /// Updates or inserts data in local table.
        /// </summary>
        /// <param name="store">Instance of <see cref="IOfflineStore"/></param>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="item">Item to be inserted.</param>
        /// <param name="fromServer">
        /// <c>true</c> if the call is made based on data coming from the server e.g. in a pull operation;
        /// <c>false</c> if the call is made by the client, such as insert or update calls on an <see cref="IOfflineTable"/>.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when item has been upserted in local table.</returns>
        public static Task UpsertAsync(this IOfflineStore store, string tableName, JObject item, bool fromServer, CancellationToken cancellationToken = default)
            => store.UpsertAsync(tableName, new[] { item }, fromServer, cancellationToken);

        /// <summary>
        /// Gets the first value (if available) from the <see cref="IAsyncEnumerable{T}"/> value.
        /// </summary>
        /// <typeparam name="T">The type of the model in the enumerable.</typeparam>
        /// <param name="source">The enumerable.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The first item returned, or <c>default</c></returns>
        public static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(source, nameof(source));

            var enumerator = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    return enumerator.Current;
                }
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }

            return default;
        }
    }
}

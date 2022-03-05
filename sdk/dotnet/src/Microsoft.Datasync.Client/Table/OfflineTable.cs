// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Table
{
    /// <summary>
    /// Provides the operations that can be done against an offline table
    /// with untyped (JSON) object.
    /// </summary>
    internal class OfflineTable : IOfflineTable
    {
        /// <summary>
        /// The sync context to use for processing an offline table request.
        /// </summary>
        private readonly SyncContext _context;

        /// <summary>
        /// Creates a new <see cref="RemoteTable"/> instance to perform
        /// untyped (JSON) requests to an offline table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="serviceClient">The service client that created this table.</param>
        internal OfflineTable(string tableName, DatasyncClient serviceClient)
        {
            Arguments.IsValidTableName(tableName, nameof(tableName));
            Arguments.IsNotNull(serviceClient, nameof(serviceClient));

            if (serviceClient.SyncContext.OfflineStore == null)
            {
                throw new InvalidOperationException("An offline store must be defined before offline operations can be used.");
            }

            ServiceClient = serviceClient;
            TableName = tableName;
            _context = serviceClient.SyncContext;
        }

        #region IOfflineTable
        /// <summary>
        /// The service client being used for communication.
        /// </summary>
        public DatasyncClient ServiceClient { get; }

        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Deletes an item from the remote table.
        /// </summary>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the response when complete.</returns>
        public async Task DeleteItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            string id = ServiceSerializer.GetId(instance);
            await _context.DeleteAsync(TableName, id, instance, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Execute a query against a remote table.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the results when the query finishes.</returns>
        public IAsyncEnumerable<JToken> GetAsyncItems(string query)
            => new FuncAsyncPageable<JToken>(nextLink => GetNextPageAsync(query, nextLink));

        /// <summary>
        /// Retrieve an item from the remote table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public Task<JObject> GetItemAsync(string id, CancellationToken cancellationToken = default)
            => _context.GetAsync(TableName, id, cancellationToken);

        /// <summary>
        /// Inserts an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to insert into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the inserted data when complete.</returns>
        public async Task<JObject> InsertItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            string id = ServiceSerializer.GetId(instance, allowDefault: true);
            if (id == null)
            {
                id = Guid.NewGuid().ToString("N");
                instance = (JObject)instance.DeepClone();
                instance[SystemProperties.JsonIdProperty] = id;
            }

            await _context.InsertAsync(TableName, id, instance, cancellationToken).ConfigureAwait(false);
            return instance;
        }

        /// <summary>
        /// Pulls the items matching the provided query from the remote table.
        /// </summary>
        /// <param name="query">The OData query that determines which items to pull from the remote table.</param>
        /// <param name="options">The options used to configure the pull operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation has finished.</returns>
        public Task PullItemsAsync(string query, PullOptions options, CancellationToken cancellationToken = default)
            => _context.PullAsync(TableName, query, options, cancellationToken);

        /// <summary>
        /// Deletes all the items in the offline table that match the query.
        /// </summary>
        /// <param name="query">An OData query that determines which items to delete.</param>
        /// <param name="options">The options used to configure the purge operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the purge operation has finished.</returns>
        public Task PurgeItemsAsync(string query, PurgeOptions options, CancellationToken cancellationToken = default)
            => _context.PurgeAsync(TableName, query, options, cancellationToken);

        /// <summary>
        /// Replaces an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to replace into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the replaced data when complete.</returns>
        public async Task ReplaceItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            string id = ServiceSerializer.GetId(instance);
            instance = ServiceSerializer.RemoveSystemProperties(instance, out string version);
            if (version != null)
            {
                instance[SystemProperties.JsonVersionProperty] = version;
            }
            await _context.UpdateAsync(TableName, id, instance, cancellationToken).ConfigureAwait(false);
        }
        #endregion

        /// <summary>
        /// Gets the next page of items from the list.  If the <c>nextLink</c> is set, use that for
        /// the query; otherwise use the <c>query</c>
        /// </summary>
        /// <param name="query">The initial query.</param>
        /// <param name="nextLink">The next link.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        private async Task<Page<JToken>> GetNextPageAsync(string query, string nextLink, CancellationToken cancellationToken = default)
        {
            if (nextLink != null)
            {
                var requestUri = new Uri(nextLink);
                query = requestUri.Query.TrimStart('?');
            }
            return await _context.GetPageAsync(TableName, query, cancellationToken).ConfigureAwait(false);
        }
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
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
        /// The Id generator to use for item.
        /// </summary>
#nullable enable
        public Func<string, string>? IdGenerator;
#nullable disable

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

            if (serviceClient.SyncContext?.OfflineStore == null)
            {
                throw new InvalidOperationException("An offline store must be defined before offline operations can be used.");
            }

            ServiceClient = serviceClient;
            TableName = tableName;
            _context = serviceClient.SyncContext;
            IdGenerator = serviceClient.ClientOptions.IdGenerator;
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
        /// Count the number of items that would be returned by the provided query, without returning
        /// all the values.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns>A task that returns the number of items that will be in the result set when the query finishes.</returns>
        public Task<long> CountItemsAsync(string query, CancellationToken cancellationToken = default)
            => _context.CountItemsAsync(TableName, query, cancellationToken);

        /// <summary>
        /// Deletes an item from the offline table.
        /// </summary>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the response when complete.</returns>
        public Task DeleteItemAsync(JObject instance, CancellationToken cancellationToken = default)
            => _context.DeleteItemAsync(TableName, instance, cancellationToken);

        /// <summary>
        /// Execute a query against an offline table.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns>A task that returns the results when the query finishes.</returns>
        public IAsyncEnumerable<JObject> GetAsyncItems(string query)
            => new FuncAsyncPageable<JObject>(nextLink => GetNextPageAsync(query, nextLink));

        /// <summary>
        /// Retrieve an item from the offline table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public Task<JObject> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidId(id, nameof(id));
            return _context.GetItemAsync(TableName, id, cancellationToken);
        }

        /// <summary>
        /// Inserts an item into the offline table.
        /// </summary>
        /// <param name="instance">The instance to insert into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the inserted data when complete.</returns>
        public async Task<JObject> InsertItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            string id = ServiceSerializer.GetId(instance, allowDefault: true);
            if (id == null)
            {
                id = IdGenerator?.Invoke(TableName) ?? Guid.NewGuid().ToString("N");
                instance = (JObject)instance.DeepClone();
                instance[SystemProperties.JsonIdProperty] = id;
            }
            await _context.InsertItemAsync(TableName, instance, cancellationToken);
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
            => _context.PullItemsAsync(TableName, query, options, cancellationToken);

        /// <summary>
        /// Deletes all the items in the offline table that match the query.
        /// </summary>
        /// <param name="query">An OData query that determines which items to delete.</param>
        /// <param name="options">The options used to configure the purge operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the purge operation has finished.</returns>
        public Task PurgeItemsAsync(string query, PurgeOptions options, CancellationToken cancellationToken = default)
            => _context.PurgeItemsAsync(TableName, query, options, cancellationToken);

        /// <summary>
        /// Pushes items in the operations queue for this table to the remote service.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns></returns>
        public Task PushItemsAsync(CancellationToken cancellationToken = default)
            => _context.PushItemsAsync(TableName, cancellationToken);

        /// <summary>
        /// Pushes items in the operations queue for this table to the remote service.
        /// </summary>
        /// <param name="options">The push operation options.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns></returns>
        public Task PushItemsAsync(PushOptions options, CancellationToken cancellationToken = default)
            => _context.PushItemsAsync(TableName, options, cancellationToken);

        /// <summary>
        /// Replaces an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to replace into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the replaced data when complete.</returns>
        public Task ReplaceItemAsync(JObject instance, CancellationToken cancellationToken = default)
            => _context.ReplaceItemAsync(TableName, instance, cancellationToken);

        #endregion

        /// <summary>
        /// Gets the next page of items from the list.  If the <c>nextLink</c> is set, use that for
        /// the query; otherwise use the <c>query</c>
        /// </summary>
        /// <param name="query">The initial query.</param>
        /// <param name="nextLink">The next link.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        protected Task<Page<JObject>> GetNextPageAsync(string query, string nextLink, CancellationToken cancellationToken = default)
            => _context.GetNextPageAsync(TableName, nextLink != null ? new Uri(nextLink).Query.TrimStart('?') : query, cancellationToken);
    }
}

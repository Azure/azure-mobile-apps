// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.InMemoryStore
{
    internal class InMemoryTable
    {
        // The contents of the table.
        private readonly Dictionary<string, JObject> content = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTable"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="tableDefinition">The table definition.</param>
        public InMemoryTable(string tableName, JObject tableDefinition)
        {
            Arguments.IsValidTableName(tableName, true, nameof(tableName));
            Arguments.IsNotNull(tableDefinition, nameof(tableDefinition));

            if (!tableDefinition.ContainsKey(SystemProperties.JsonIdProperty))
            {
                throw new OfflineStoreException($"The definition for table '{tableName}' does not contain an '{SystemProperties.JsonIdProperty}' field.");
            }
            if (tableDefinition[SystemProperties.JsonIdProperty].Type != JTokenType.String)
            {
                throw new OfflineStoreException($"The definition for the '{SystemProperties.JsonIdProperty}' in '{tableName}' must be a string.");
            }

            TableName = tableName;
            TableDefinition = tableDefinition;
        }

        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// The table definition.
        /// </summary>
        public JObject TableDefinition { get; }

        /// <summary>
        /// Deletes items from the table where the items are identified by a query.
        /// </summary>
        /// <param name="query">A query description that identifies the items to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public Task DeleteAsync(QueryDescription query, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(query, nameof(query));

            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes items from the table where the items are identified by their ID.
        /// </summary>
        /// <param name="ids">A list of IDs to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public Task DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(ids, nameof(ids));
            foreach (var id in ids)
            {
                content.Remove(id);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a single item by the ID of the item.
        /// </summary>
        /// <param name="id">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public Task<JObject> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidId(id, nameof(id));
            return Task.FromResult(content.ContainsKey(id) ? content[id] : null);
        }

        /// <summary>
        /// Returns items from the table.
        /// </summary>
        /// <param name="query">A query describing the items to be returned.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        public Task<Page<JObject>> GetPageAsync(QueryDescription query, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(query, nameof(query));
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates or inserts the item(s) provided in the table.
        /// </summary>
        /// <param name="items">The item(s) to be updated or inserted.</param>
        /// <param name="ignoreMissingColumns">If <c>true</c>, extra properties on the item can be ignored.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item has been updated or inserted into the table.</returns>
        public Task UpsertAsync(IEnumerable<JObject> items, bool ignoreMissingColumns, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(items, nameof(items));
            throw new NotImplementedException();
        }
    }
}

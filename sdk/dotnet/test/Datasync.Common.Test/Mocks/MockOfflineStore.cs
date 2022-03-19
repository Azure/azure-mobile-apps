// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Query.OData;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockTable = System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject>;

namespace Datasync.Common.Test.Mocks
{
    /// <summary>
    /// An implementation of the <see cref="IOfflineStore"/> that stores data in
    /// a loose manner in dictionaries.
    /// </summary>
    /// <seealso cref="Microsoft.Datasync.Client.Offline.IOfflineStore" />
    [ExcludeFromCodeCoverage]
    public class MockOfflineStore : IOfflineStore
    {
        /// <summary>
        /// The mapping of a table name to the table contents.  Used for storing the
        /// contents of each table.
        /// </summary>
        public readonly Dictionary<string, MockTable> TableMap = new();

        /// <summary>
        /// The list of queries (in order) executed for reading items on a table.
        /// </summary>
        public readonly List<QueryDescription> ReadQueries = new();

        /// <summary>
        /// The list of queries (in order) executed for deleting items on a table.
        /// </summary>
        public readonly List<QueryDescription> DeleteQueries = new();

        /// <summary>
        /// The responses given to successive read requests; used if the <see cref="ReadAysncFunc"/>
        /// is not set.
        /// </summary>
        public Queue<string> ReadResponses = new();

        /// <summary>
        /// A func that returns a given <see cref="JToken"/> for the provided <see cref="QueryDescription"/>.
        /// </summary>
        public Func<QueryDescription, IEnumerable<JObject>> ReadAsyncFunc { get; set; }

        /// <summary>
        /// A func that returns a page of items (for testing multi-page responses)
        /// </summary>
        public Func<QueryDescription, Page<JObject>> ReadPageFunc { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets or sets the exception to throw.
        /// </summary>
        public Exception ExceptionToThrow { get; set; }

        #region IOfflineStore
        /// <summary>
        /// Deletes items from the table where the items are identified by a query.
        /// </summary>
        /// <param name="query">A query description that identifies the items to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public Task DeleteAsync(QueryDescription query, CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            DeleteQueries.Add(query);
            var table = GetOrCreateTable(query.TableName);
            var items = FilterItemList(query, table.Values).Select(o => o.Value<string>("id"));
            foreach (var item in items)
            {
                table.Remove(item);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes items from the table where the items are identified by their ID.
        /// </summary>
        /// <param name="tableName">The name of the table where the items are located.</param>
        /// <param name="ids">A list of IDs to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public Task DeleteAsync(string tableName, IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            var table = GetOrCreateTable(tableName);
            foreach (string id in ids)
            {
                table.Remove(id);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns items from the table.
        /// </summary>
        /// <param name="query">A query describing the items to be returned.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        public Task<JObject> GetItemAsync(string tableName, string id, CancellationToken cancellationToken = default)
        {
            var table = GetOrCreateTable(tableName);
            table.TryGetValue(id, out JObject item);
            return Task.FromResult(item);
        }

        /// <summary>
        /// Returns items from the table.
        /// </summary>
        /// <param name="query">A query describing the items to be returned.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        public Task<Page<JObject>> GetPageAsync(QueryDescription query, CancellationToken cancellationToken = default)
        {
            var table = GetOrCreateTable(query.TableName);
            IEnumerable<JObject> items;

            // Handle the operations queue.
            if (query.TableName == SystemTables.OperationsQueue)
            {
                items = FilterItemList(query, table.Values);
            }
            else if (ReadAsyncFunc != null)
            {
                items = ReadAsyncFunc(query);
            }
            else
            {
                var arr = JArray.Parse(ReadResponses.Dequeue());
                items = (IEnumerable<JObject>)arr.ToArray();
            }

            if (query.IncludeTotalCount)
            {
                return Task.FromResult(new Page<JObject> { Count = items.Count(), Items = items });
            }
            else
            {
                return Task.FromResult(new Page<JObject> { Items = items });
            }
        }

        /// <summary>
        /// Initializes the store for use.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the store is ready to use.</returns>
        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            IsInitialized = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates or inserts the item(s) provided in the table.
        /// </summary>
        /// <param name="tableName">The table to be used for the operation.</param>
        /// <param name="items">The item(s) to be updated or inserted.</param>
        /// <param name="ignoreMissingColumns">If <c>true</c>, extra properties on the item can be ignored.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item has been updated or inserted into the table.</returns>
        public Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns, CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            var table = GetOrCreateTable(tableName);
            foreach (JObject item in items)
            {
                var id = ServiceSerializer.GetId(item);
                table[id] = item;
            }
            return Task.CompletedTask;
        }
        #endregion

        /// <summary>
        /// Upserts items into the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="items">The items.</param>
        public void Upsert(string tableName, IEnumerable<JObject> items)
        {
            var table = GetOrCreateTable(tableName);
            foreach (JObject item in items)
            {
                var id = ServiceSerializer.GetId(item);
                table[id] = item;
            }
        }

        /// <summary>
        /// Gets the or create table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        private MockTable GetOrCreateTable(string tableName)
        {
            if (!TableMap.TryGetValue(tableName, out MockTable table))
            {
                TableMap[tableName] = table = new MockTable();
            }
            return table;
        }

        /// <summary>
        /// Selects the correct set of entities based on the provided OData string.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="items">The items in the table.</param>
        /// <returns>The filtered items list.</returns>
        private static IEnumerable<JObject> FilterItemList(QueryDescription query, IEnumerable<JObject> items)
        {
            var odata = query.ToODataString();

            if (odata.Contains("$orderby=sequence desc")) // the query to take total count and max sequence
            {
                return items.OrderByDescending(o => o.Value<long>("sequence"));
            }
            else if (odata.StartsWith("$filter=((tableKind eq ") && odata.Contains("(sequence gt "))
            {
                var sequenceCompareNode = ((BinaryOperatorNode)query.Filter).RightOperand as BinaryOperatorNode;

                return items.Where(o => o.Value<long>("sequence") > (long)((ConstantNode)sequenceCompareNode.RightOperand).Value).OrderBy(o => o.Value<long>("sequence"));
            }
            else if (odata.Contains("(sequence gt ")) // the query to get next operation
            {
                return items.Where(o => o.Value<long>("sequence") > (long)((ConstantNode)((BinaryOperatorNode)query.Filter).RightOperand).Value).OrderBy(o => o.Value<long>("sequence"));
            }
            else if (odata.Contains(") and (itemId eq '")) // the query to retrive operation by item id
            {
                string targetTable = ((ConstantNode)((BinaryOperatorNode)((BinaryOperatorNode)query.Filter).LeftOperand).RightOperand).Value.ToString();
                string targetId = ((ConstantNode)((BinaryOperatorNode)((BinaryOperatorNode)query.Filter).RightOperand).RightOperand).Value.ToString();
                return items.Where(o => o.Value<string>("itemId") == targetId && o.Value<string>("tableName") == targetTable);
            }
            else if (odata.Contains("$filter=(tableName eq '"))
            {
                return items.Where(o => o.Value<string>("tableName") == ((ConstantNode)((BinaryOperatorNode)query.Filter).RightOperand).Value.ToString());
            }
            return items;
        }

        /// <summary>
        /// Determines if the operations queue contains the specified item.
        /// </summary>
        /// <param name="kind">The kind.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public JObject FindInQueue(TableOperationKind kind, string tableName, string itemId)
        {
            int opKind = (int)kind;

            if (!TableMap.ContainsKey(SystemTables.OperationsQueue))
            {
                throw new InvalidOperationException("Operations Queue has not been initialized.");
            }
            return TableMap[SystemTables.OperationsQueue].Values
                .SingleOrDefault(v => v.Value<int>("kind") == opKind && v.Value<string>("tableName") == tableName && v.Value<string>("itemId") == itemId);
        }

        /// <summary>
        /// Determines if the table contains the relevant item.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the item is the same as the item in the table, <c>false</c> otherwise.</returns>
        public bool TableContains(string tableName, JObject item)
        {
            string id = ServiceSerializer.GetId(item);
            if (!TableMap.ContainsKey(tableName) || !TableMap[tableName].ContainsKey(id))
            {
                return false;
            }
            return TableMap[tableName][id].Equals(item);
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

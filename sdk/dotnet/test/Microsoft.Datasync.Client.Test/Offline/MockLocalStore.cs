// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.OData;
using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MockTable = System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject>;

namespace Microsoft.Datasync.Client.Test.Offline
{
    /// <summary>
    /// A mock version of the offline store.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MockLocalStore : IOfflineStore
    {
        /// <summary>
        /// The list of tables in the store.
        /// </summary>
        public readonly Dictionary<string, MockTable> TableMap = new();

        /// <summary>
        /// Stored read queries for testing.
        /// </summary>
        public List<QueryDescription> ReadQueries { get; } = new();

        /// <summary>
        /// Stored test queries used for deleting items in the store.
        /// </summary>
        public List<QueryDescription> DeleteQueries { get; } = new();

        /// <summary>
        /// A queue of read responses.
        /// </summary>
        public Queue<string> ReadResponses { get; } = new();

        /// <summary>
        /// A function that can read a query description and produce a JToken,
        /// used during the read operations.
        /// </summary>
        public Func<QueryDescription, JToken> ReadAsyncFunc { get; set; }

        #region IOfflineStore
        public Task DeleteAsync(QueryDescription query, CancellationToken token = default)
        {
            DeleteQueries.Add(query);
            GetTable(query.TableName).Clear();
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string tableName, IEnumerable<string> ids, CancellationToken token = default)
        {
            foreach (string id in ids)
            {
                MockTable table = GetTable(tableName);
                table.Remove(id);
            }
            return Task.CompletedTask;
        }

        public virtual Task<JObject> GetItemAsync(string tableName, string id, CancellationToken token = default)
        {
            MockTable table = GetTable(tableName);
            table.TryGetValue(id, out JObject item);
            return Task.FromResult(item);
        }

        public Task InitializeAsync(CancellationToken token = default)
            => Task.CompletedTask;

        public Task<Page<JToken>> GetPageAsync(QueryDescription query, CancellationToken token = default)
        {
            if (query.TableName == OfflineSystemTables.OperationsQueue || query.TableName == OfflineSystemTables.SyncErrors)
            {
                MockTable table = GetTable(query.TableName);

                IEnumerable<JObject> items = table.Values;
                if (query.TableName == OfflineSystemTables.OperationsQueue)
                {
                    string odata = query.ToODataString();
                    if (odata.Contains("$orderby=sequence desc")) // the query to take total count and max sequence
                    {
                        items = items.OrderBy(o => o.Value<long>("sequence"));
                    }
                    else if (odata.StartsWith("$filter=((tableKind eq ") && odata.Contains("(sequence gt "))
                    {
                        var sequenceCompareNode = ((BinaryOperatorNode)query.Filter).RightOperand as BinaryOperatorNode;

                        items = items.Where(o => o.Value<long>("sequence") > (long)((ConstantNode)sequenceCompareNode.RightOperand).Value);
                        items = items.OrderBy(o => o.Value<long>("sequence"));
                    }
                    else if (odata.Contains("(sequence gt ")) // the query to get next operation
                    {
                        items = items.Where(o => o.Value<long>("sequence") > (long)((ConstantNode)((BinaryOperatorNode)query.Filter).RightOperand).Value);
                        items = items.OrderBy(o => o.Value<long>("sequence"));
                    }
                    else if (odata.Contains(") and (itemId eq '")) // the query to retrive operation by item id
                    {
                        string targetTable = ((ConstantNode)((BinaryOperatorNode)((BinaryOperatorNode)query.Filter).LeftOperand).RightOperand).Value.ToString();
                        string targetId = ((ConstantNode)((BinaryOperatorNode)((BinaryOperatorNode)query.Filter).RightOperand).RightOperand).Value.ToString();
                        items = items.Where(o => o.Value<string>("itemId") == targetId && o.Value<string>("tableName") == targetTable);
                    }
                    else if (odata.Contains("$filter=(tableName eq '"))
                    {
                        items = items.Where(o => o.Value<string>("tableName") == ((ConstantNode)((BinaryOperatorNode)query.Filter).RightOperand).Value.ToString());
                    }
                }

                Page<JToken> result = new()
                {
                    Count = items.Count(),
                    Items = items.ToArray()
                };
                return Task.FromResult(result);
            }

            ReadQueries.Add(query);
            JToken response = (ReadAsyncFunc != null) ? ReadAsyncFunc(query) : JToken.Parse(ReadResponses.Dequeue());
            Page<JToken> pagedResult = new();

            // Response can either be a JArray of JObjects, or a JObject, containing count and values.
            if (response is JArray array)
            {
                pagedResult.Items = array.ToArray();
            }
            else if (response is JObject value)
            {
                pagedResult.Count = value.Value<long>("count");
                pagedResult.Items = (value["items"] as JArray).ToArray();
            }
            return Task.FromResult(pagedResult);
        }

        public Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool fromServer, CancellationToken token = default)
        {
            foreach (JObject item in items)
            {
                MockTable table = GetTable(tableName);
                table[item.Value<string>("id")] = item;
            }
            return Task.FromResult(0);
        }
        #endregion

        private Dictionary<string, JObject> GetTable(string tableName)
        {
            if (!TableMap.TryGetValue(tableName, out MockTable table))
            {
                TableMap[tableName] = table = new MockTable();
            }
            return table;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

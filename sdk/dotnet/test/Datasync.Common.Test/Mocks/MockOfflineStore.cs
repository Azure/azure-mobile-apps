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
using System.Text.RegularExpressions;
using System.Web;
using MockTable = System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject>;

namespace Datasync.Common.Test.Mocks;

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

    /// <summary>
    /// Gets or sets the read exception to throw.
    /// </summary>
    /// <value>
    /// The read exception to throw.
    /// </value>
    public Exception ReadExceptionToThrow { get; set; }

    #region IOfflineStore
    public void DefineTable(string tableName, JObject definition)
    {
        _ = GetOrCreateTable(tableName);
    }

    public void DefineTable<T>(string tableName, DatasyncSerializerSettings settings)
    {
        _ = GetOrCreateTable(tableName);
    }

    public bool TableIsDefined(string tableName)
        => TableMap.ContainsKey(tableName);

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
        if (ReadExceptionToThrow != null)
        {
            throw ReadExceptionToThrow;
        }

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
        else if (ReadResponses.Count > 0)
        {
            var arr = JArray.Parse(ReadResponses.Dequeue());
            items = (IEnumerable<JObject>)arr.ToArray();
        }
        else if (ReadPageFunc != null)
        {
            return Task.FromResult(ReadPageFunc.Invoke(query));
        }
        else
        {
            items = table.Values;
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
    /// Gets the list of offline tables that have been defined.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the list of tables that have been defined.</returns>
    public Task<IList<string>> GetTablesAsync(CancellationToken cancellationToken = default)
    {
        var list = TableMap.Keys.Where(t => !t.StartsWith("__")).ToList();
        return Task.FromResult((IList<string>)list);
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
    public MockTable GetOrCreateTable(string tableName)
    {
        if (!TableMap.TryGetValue(tableName, out MockTable table))
        {
            TableMap[tableName] = table = new MockTable();
        }
        return table;
    }

    /// <summary>
    /// Gets from value from the <see cref="ConstantNode"/> as the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the value of the node.</typeparam>
    /// <param name="node">The node.</param>
    /// <returns>The value of the node.</returns>
    private static T GetFromConstantNode<T>(QueryNode node) => (T)((ConstantNode)node).Value;

    /// <summary>
    /// Selects the correct set of entities based on the provided OData string.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="items">The items in the table.</param>
    /// <returns>The filtered items list.</returns>
    private static IEnumerable<JObject> FilterItemList(QueryDescription query, IEnumerable<JObject> items)
    {
        var odataString = query.ToODataString();
        var odataQuery = ODataQuery.Parse(odataString);
        if (!items.Any())
        {
            return items;
        }

        string filter = odataQuery.Filter;
        if (!string.IsNullOrEmpty(filter))
        {
            if (Regex.IsMatch(filter, "^\\(\\(sequence gt [0-9]+L\\) and \\(tableName eq '[^']+'\\)\\)$"))
            {
                BinaryOperatorNode sequenceComparison = (BinaryOperatorNode)((BinaryOperatorNode)query.Filter).LeftOperand;
                long sequence = GetFromConstantNode<long>(sequenceComparison.RightOperand);
                BinaryOperatorNode tableComparison = (BinaryOperatorNode)((BinaryOperatorNode)query.Filter).RightOperand;
                string tableName = GetFromConstantNode<string>(tableComparison.RightOperand);
                items = items.Where(item => item.Value<long>("sequence") > sequence && item.Value<string>("tableName") == tableName);
            }
            else if (Regex.IsMatch(filter, "^\\(\\(tableName eq '[^']+'\\) and \\(itemId eq '[^']+'\\)\\)$"))
            {
                BinaryOperatorNode tableComparison = (BinaryOperatorNode)((BinaryOperatorNode)query.Filter).LeftOperand;
                string tableName = GetFromConstantNode<string>(tableComparison.RightOperand);
                BinaryOperatorNode itemIdComparison = (BinaryOperatorNode)((BinaryOperatorNode)query.Filter).RightOperand;
                string itemId = GetFromConstantNode<string>(itemIdComparison.RightOperand);
                items = items.Where(item => item.Value<string>("tableName") == tableName && item.Value<string>("itemId") == itemId);
            }
            else if (Regex.IsMatch(filter, "^\\(tableName eq '[^']+'\\)$"))
            {
                BinaryOperatorNode tableComparison = (BinaryOperatorNode)query.Filter;
                string tableName = GetFromConstantNode<string>(tableComparison.RightOperand);
                items = items.Where(item => item.Value<string>("tableName") == tableName);
            }
            else if (Regex.IsMatch(filter, "^\\(sequence gt [0-9]+L\\)$"))
            {
                BinaryOperatorNode sequenceComparison = (BinaryOperatorNode)query.Filter;
                long sequence = GetFromConstantNode<long>(sequenceComparison.RightOperand);
                items = items.Where(item => item.Value<long>("sequence") > sequence);
            }
            else if (Regex.IsMatch(filter, "^\\(deleted eq (true|false)\\)$"))
            {
                BinaryOperatorNode deletedComparison = (BinaryOperatorNode)query.Filter;
                bool elem = GetFromConstantNode<bool>(deletedComparison.RightOperand);
                items = items.Where(item => item.Value<bool>("deleted") == elem);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        if (odataQuery.OrderBy?.Length > 0)
        {
            foreach (var orderBy in odataQuery.OrderBy)
            {
                bool descending = false;
                string field = orderBy;
                if (orderBy.EndsWith(" desc"))
                {
                    descending = true;
                    field = orderBy.Replace(" desc", "");
                }

                items = field switch
                {
                    "sequence" => descending ? items.OrderByDescending(i => i.Value<long>(field)) : items.OrderBy(i => i.Value<long>(field)),
                    _ => throw new NotImplementedException()
                };
            }
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

    private class ODataQuery
    {
        public string Filter { get; set; }
        public string[] OrderBy { get; set; }
        public string[] Select { get; set; }
        public int Skip { get; set; } = 0;
        public int Top { get; set; } = 0;
        public Dictionary<string, string> Parameters { get; set; } = new();

        public static ODataQuery Parse(string odata)
        {
            var result = new ODataQuery();
            var parameters = HttpUtility.ParseQueryString(odata);
            foreach (var parameter in parameters.AllKeys)
            {
                switch (parameter.ToLowerInvariant())
                {
                    case "$filter":
                        result.Filter = parameters[parameter];
                        break;
                    case "$orderby":
                        result.OrderBy = parameters[parameter].Split(',');
                        break;
                    case "$select":
                        result.Select = parameters[parameter].Split(',');
                        break;
                    case "$skip":
                        result.Skip = Int32.Parse(parameters[parameter]);
                        break;
                    case "$top":
                        result.Top = Int32.Parse(parameters[parameter]);
                        break;
                    default:
                        result.Parameters.Add(parameter, parameters[parameter]);
                        break;
                }
            }
            return result;
        }

        public static ODataQuery Parse(QueryDescription query)
            => Parse(query.ToODataString());
    }
}

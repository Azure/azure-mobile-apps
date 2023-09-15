// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.SQLiteStore.Driver;
using Microsoft.Datasync.Client.SQLiteStore.Utils;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.SQLiteStore
{
    /// <summary>
    /// An implementation of <see cref="IOfflineStore"/> using SQLite as the persistent store.
    /// </summary>
    public class OfflineSQLiteStore : AbstractOfflineStore, IDeltaTokenStoreProvider
    {
        /// <summary>
        /// The mapping from the table name to the table definition.  This is built using the
        /// <see cref="DefineTable(string, JObject)"/> method before store initialization.
        /// </summary>
        private readonly Dictionary<string, TableDefinition> tableMap = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// A lock that is used to serialize write operations to the database.
        /// </summary>
        private readonly DisposableLock operationLock = new();

        /// <summary>
        /// Creates a new instance of <see cref="OfflineSQLiteStore"/> using the provided connection string.
        /// </summary>
        /// <remarks>
        /// <para>If the connection string starts with <c>file:</c>, then it is considered to be a URI filename and
        /// should be structured as such.  This allows the setting of any options (such as mode, cache, etc.)
        /// if needed.</para>
        /// <para>If the connection string does not start with file:, then it should be an absolute path (which starts
        /// with a <c>/</c>).</para>
        /// </remarks>
        /// <see href="https://sqlite.org/c3ref/open.html"/>
        /// <param name="connectionString">The connection string to use for persistent storage.</param>
        public OfflineSQLiteStore(string connectionString)
        {
            Arguments.IsNotNullOrWhitespace(connectionString, nameof(connectionString));
            if (!connectionString.StartsWith("file:"))
            {
                throw new ArgumentException("The connection string must be a Uri string valid for SQLite");
            }
            DbConnection = new SqliteConnection(connectionString);
        }

        /// <summary>
        /// Creates a new instance of <see cref="OfflineSQLiteStore"/> using the provided connection string.
        /// </summary>
        /// <remarks>
        /// <para>If the connection string starts with <c>file:</c>, then it is considered to be a URI filename and
        /// should be structured as such.  This allows the setting of any options (such as mode, cache, etc.)
        /// if needed.</para>
        /// <para>If the connection string does not start with file:, then it should be an absolute path (which starts
        /// with a <c>/</c>).</para>
        /// </remarks>
        /// <see href="https://sqlite.org/c3ref/open.html"/>
        /// <param name="connectionString">The connection string to use for persistent storage.</param>
        /// <param name="logger">The logger to use for logging SQL requests.</param>
        public OfflineSQLiteStore(string connectionString, ILogger logger) : this(connectionString)
        {
            Logger = logger;
        }

        /// <summary>
        /// The database connection.
        /// </summary>
        internal SqliteConnection DbConnection { get; }

        /// <summary>
        /// The logging service
        /// </summary>
        public ILogger Logger { get; set; }

        #region AbstractOfflineStore
        /// <summary>
        /// Defines the local table in the persistent store.
        /// </summary>
        /// <param name="tableName">The name of the local table.</param>
        /// <param name="tableDefinition">The definition of the table.</param>
        public override void DefineTable(string tableName, JObject tableDefinition)
        {
            Arguments.IsValidTableName(tableName, true, nameof(tableName));
            Arguments.IsNotNull(tableDefinition, nameof(tableDefinition));

            if (!tableMap.ContainsKey(tableName))
            {
                Logger?.LogDebug("Created table definition for table {tableName}", tableName);
                tableMap.Add(tableName, new TableDefinition(tableName, tableDefinition));
            }
        }

        /// <summary>
        /// Determines if a table is defined.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>true if the table is defined.</returns>
        public override bool TableIsDefined(string tableName)
            => tableMap.ContainsKey(tableName);

        /// <summary>
        /// Deletes items from the table where the items are identified by a query.
        /// </summary>
        /// <param name="query">A query description that identifies the items to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public override async Task DeleteAsync(QueryDescription query, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(query, nameof(query));
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            _ = GetTableOrThrow(query.TableName); // Validate that the table exists.

            string sql = SqlStatements.DeleteFromTable(query, out Dictionary<string, object> parameters);
            using (operationLock.AcquireLock())
            {
                ExecuteNonQueryInternal(sql, parameters);
            }
        }

        /// <summary>
        /// Deletes items from the table where the items are identified by their ID.
        /// </summary>
        /// <param name="tableName">The name of the table where the items are located.</param>
        /// <param name="ids">A list of IDs to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public override async Task DeleteAsync(string tableName, IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidTableName(tableName, true, nameof(tableName));
            Arguments.IsNotNull(ids, nameof(ids));
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            var parameters = ids.ToParameterList("id");
            if (parameters.Count == 0)
            {
                return; // Don't execute a statement if there is nothing to execute.
            }
            string sql = SqlStatements.DeleteIdsFromTable(tableName, SystemProperties.JsonIdProperty, parameters.Keys);
            using (operationLock.AcquireLock())
            {
                ExecuteNonQueryInternal(sql, parameters);
            }
        }

        /// <summary>
        /// Returns a single item by the ID of the item.
        /// </summary>
        /// <param name="tableName">The table name holding the item.</param>
        /// <param name="id">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public override async Task<JObject> GetItemAsync(string tableName, string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidTableName(tableName, true, nameof(tableName));
            Arguments.IsValidId(id, nameof(id));
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            Dictionary<string, object> parameters = new[] { id }.ToParameterList("id");
            string sql = SqlStatements.GetItemById(tableName, SystemProperties.JsonIdProperty, parameters.Keys.First());
            using (operationLock.AcquireLock())
            {
                IList<JObject> results = ExecuteQueryInternal(tableName, sql, parameters);
                return results.FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns items from the table.
        /// </summary>
        /// <param name="query">A query describing the items to be returned.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        public override async Task<Page<JObject>> GetPageAsync(QueryDescription query, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(query, nameof(query));
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            string queryStmt = SqlStatements.SelectFromTable(query, out Dictionary<string, object> parameters);
            using (operationLock.AcquireLock())
            {
                IList<JObject> rows = ExecuteQueryInternal(query.TableName, queryStmt, parameters);
                Page<JObject> result = new() { Items = rows };
                if (query.IncludeTotalCount)
                {
                    string countStmt = SqlStatements.CountFromTable(query, out Dictionary<string, object> countParams);
                    IList<JObject> countRows = ExecuteQueryInternal(query.TableName, countStmt, countParams);
                    result.Count = countRows[0].Value<long>("count");
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the list of offline tables that have been defined.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the list of tables that have been defined.</returns>
        public override async Task<IList<string>> GetTablesAsync(CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            return tableMap.Keys.Where(t => !t.StartsWith("__")).ToList();
        }

        /// <summary>
        /// Creates the Delta Token Store implementation that works with this store.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that resolves to the delta token store when complete</returns>
        public Task<IDeltaTokenStore> GetDeltaTokenStoreAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IDeltaTokenStore>(new SQLiteDeltaTokenStore(this));

        /// <summary>
        /// Initialize the store.  This is over-ridden by the store implementation to provide a point
        /// where the tables can be created or updated.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the store is initialized.</returns>
        protected override Task InitializeOfflineStoreAsync(CancellationToken cancellationToken)
        {
            // Define the internal tables.
            DefineTable(SystemTables.Configuration, SQLiteDeltaTokenStore.TableDefinition);

            // Now that all the tables are defined, actually create the tables!
            tableMap.Values
                .Where(table => !table.IsInDatabase)
                .ToList()
                .ForEach(table => CreateTableFromDefinition(table));

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
        public override async Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidTableName(tableName, true, nameof(tableName));
            Arguments.IsNotNull(items, nameof(items));
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            TableDefinition table = GetTableOrThrow(tableName);
            var first = items.FirstOrDefault();
            if (first == null)
            {
                return;
            }

            var columns = new List<ColumnDefinition>();
            foreach (var prop in first.Properties())
            {
                if (!table.TryGetValue(prop.Name, out ColumnDefinition column) && !ignoreMissingColumns)
                {
                    throw new InvalidOperationException($"Column '{prop.Name}' is not defined on table '{tableName}'");
                }
                if (column != null)
                {
                    columns.Add(column);
                }
            }

            if (columns.Count == 0)
            {
                return;
            }

            using (operationLock.AcquireLock())
            {
                ExecuteNonQueryInternal("BEGIN TRANSACTION");
                try
                {
                    BatchInsert(tableName, items, columns.Where(c => c.IsIdColumn).Take(1).ToList());
                    BatchUpdate(tableName, items, columns);
                    ExecuteNonQueryInternal("COMMIT TRANSACTION");
                }
                catch (SQLiteException)
                {
                    ExecuteNonQueryInternal("ROLLBACK");
                    throw;
                }
            }
        }
        #endregion

        /// <summary>
        /// Executes a SQL query against the store.  This is usedul for running arbitrary queries that are supported
        /// by SQLite but not the SDK LINQ provider.
        /// </summary>
        /// <remarks>If doing a JOIN between two tables, then use the <see cref="ExecuteQueryAsync(JObject, string, IDictionary{string, object}, CancellationToken)"/>
        /// version to define the field mapping.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        /// <param name="parameters">A list of parameter values for referenced parameters in the SQL statement.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The list of rows returned by the query.</returns>
        public async Task<IList<JObject>> ExecuteQueryAsync(string tableName, string sqlStatement, IDictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidTableName(tableName, nameof(tableName));
            Arguments.IsNotNullOrWhitespace(sqlStatement, nameof(sqlStatement));
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            var tableDefinition = GetTableOrThrow(tableName);
            using (operationLock.AcquireLock())
            {
                return ExecuteQueryInternal(tableDefinition, sqlStatement, parameters);
            }
        }

        /// <summary>
        /// Executes a SQL query against the store.  This is usedul for running arbitrary queries that are supported
        /// by SQLite but not the SDK LINQ provider.
        /// </summary>
        /// <remarks>
        /// If doing a query on a single table, use <see cref="ExecuteQueryAsync(string, string, IDictionary{string, object}, CancellationToken)"/> instead.
        /// </remarks>
        /// <param name="definition">The definition of the result set.</param>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        /// <param name="parameters">A list of parameter values for referenced parameters in the SQL statement.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The list of rows returned by the query.</returns>
        public async Task<IList<JObject>> ExecuteQueryAsync(JObject definition, string sqlStatement, IDictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(definition, nameof(definition));
            Arguments.IsNotNullOrWhitespace(sqlStatement, nameof(sqlStatement));
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            var tableDefinition = new TableDefinition("", definition);
            using (operationLock.AcquireLock())
            {
                return ExecuteQueryInternal(tableDefinition, sqlStatement, parameters);
            }
        }

        /// <summary>
        /// Do a batch update to set the list of columns for the list of items in the given table.
        /// </summary>
        /// <param name="tableName">The table to update.</param>
        /// <param name="items">The list of items to update (with new values).</param>
        /// <param name="columns">The list of columns to update.</param>
        protected void BatchInsert(string tableName, IEnumerable<JObject> items, List<ColumnDefinition> columns)
        {
            int batchSize = DbConnection.MaxParametersPerQuery / columns.Count;
            if (batchSize == 0)
            {
                throw new InvalidOperationException($"The number of fields per entity in an upsert is limited to {DbConnection.MaxParametersPerQuery}");
            }

            foreach (var batch in items.Split(maxLength: batchSize))
            {
                var sql = SqlStatements.BatchInsert(tableName, columns, batch, out Dictionary<string, object> parameters);
                if (sql != null)
                {
                    ExecuteNonQueryInternal(sql, parameters);
                }
            }
        }

        /// <summary>
        /// Do a batch update to set the list of columns for the list of items in the given table.
        /// </summary>
        /// <param name="tableName">The table to update.</param>
        /// <param name="items">The list of items to update (with new values).</param>
        /// <param name="columns">The list of columns to update.</param>
        protected void BatchUpdate(string tableName, IEnumerable<JObject> items, List<ColumnDefinition> columns)
        {
            if (columns.Count <= 1)
            {
                // For batch updates to work, there has to be at least one column desides Id that needs to be updated.
                return;
            }
            if (columns.Count > DbConnection.MaxParametersPerQuery)
            {
                throw new InvalidOperationException($"The number of fields per entity in an upsert is limited to {DbConnection.MaxParametersPerQuery}");
            }

            foreach (JObject item in items)
            {
                string sql = SqlStatements.BatchUpdate(tableName, columns, item, out Dictionary<string, object> parameters);
                if (sql != null)
                {
                    ExecuteNonQueryInternal(sql, parameters);
                }
            }
        }

        /// <summary>
        /// Creates or updates the table definition in the SQLite database to the provided definition.
        /// </summary>
        /// <param name="tableDefinition"></param>
        protected void CreateTableFromDefinition(TableDefinition tableDefinition)
        {
            var idColumn = tableDefinition.Columns.First(c => c.IsIdColumn);
            var columnDefinitions = tableDefinition.Columns.Where(c => !c.IsIdColumn);

            string createTableSql = SqlStatements.CreateTableFromColumns(tableDefinition.TableName, idColumn, columnDefinitions);
            ExecuteNonQueryInternal(createTableSql);

            string tableInfoSql = SqlStatements.GetTableInformation(tableDefinition.TableName);
            IDictionary<string, JObject> existingColumns = ExecuteQueryInternal(tableInfoSql).ToDictionary(c => c.Value<string>("name"), StringComparer.OrdinalIgnoreCase);

            // Process changes to the table definition - column(s) added
            foreach (var column in tableDefinition.Columns.Where(c => !existingColumns.ContainsKey(c.Name)))
            {
                string addColumnSql = SqlStatements.AddColumnToTable(tableDefinition.TableName, column);
                ExecuteNonQueryInternal(addColumnSql);
            }

            // Process changes to the table definition - column(s) removed
            foreach (var column in existingColumns.Keys.Where(c => !tableDefinition.ContainsKey(c)))
            {
                string dropColumnSql = SqlStatements.DropColumnFromTable(tableDefinition.TableName, column);
                ExecuteNonQueryInternal(dropColumnSql);
            }

            // TODO: Detect changes in the SQLite definition (column types) and throw an exception if the store type does not match.
        }

        /// <summary>
        /// Executes a SQL statement that does not produce any output.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        /// <param name="parameters">The parameters that are referenced in the SQL statement.</param>
        protected void ExecuteNonQueryInternal(string sqlStatement, IDictionary<string, object> parameters = null)
        {
            Arguments.IsNotNullOrWhitespace(sqlStatement, nameof(sqlStatement));
            parameters ??= new Dictionary<string, object>();

            LogSqlStatement(sqlStatement, parameters);
            using SqliteStatement stmt = DbConnection.PrepareStatement(sqlStatement);
            stmt.BindParameters(parameters);
            stmt.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a SQL query.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        /// <param name="parameters">The parameters that are referenced in the SQL statement.</param>
        /// <returns>The result of the query as a list of rows.</returns>
        protected IList<JObject> ExecuteQueryInternal(string sqlStatement, Dictionary<string, object> parameters = null)
            => ExecuteQueryInternal(new TableDefinition(), sqlStatement, parameters);

        /// <summary>
        /// Executes a SQL query.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        /// <param name="parameters">The parameters that are referenced in the SQL statement.</param>
        /// <returns>The result of the query as a list of rows.</returns>
        protected IList<JObject> ExecuteQueryInternal(string tableName, string sqlStatement, Dictionary<string, object> parameters = null)
            => ExecuteQueryInternal(GetTableOrThrow(tableName), sqlStatement, parameters);

        /// <summary>
        /// Executes a SQL query.
        /// </summary>
        /// <param name="tableDefinition">The definition of the result set.</param>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        /// <param name="parameters">The parameters that are referenced in the SQL statement.</param>
        /// <returns>The result of the query as a list of rows.</returns>
        protected IList<JObject> ExecuteQueryInternal(TableDefinition tableDefinition, string sqlStatement, IDictionary<string, object> parameters = null)
        {
            Arguments.IsNotNull(tableDefinition, nameof(tableDefinition));
            Arguments.IsNotNullOrWhitespace(sqlStatement, nameof(sqlStatement));
            parameters ??= new Dictionary<string, object>();

            LogSqlStatement(sqlStatement, parameters);
            var rows = new List<JObject>();
            using var statement = DbConnection.PrepareStatement(sqlStatement);
            statement.BindParameters(parameters);
            foreach (var row in statement.ExecuteQuery())
            {
                var obj = new JObject();
                foreach (var prop in row)
                {
                    if (tableDefinition.TryGetValue(prop.Key, out ColumnDefinition column))
                    {
                        obj[prop.Key] = column.DeserializeValue(prop.Value);
                    }
                    else
                    {
                        obj[prop.Key] = prop.Value == null ? null : JToken.FromObject(prop.Value);
                    }
                }
                rows.Add(obj);
            }
            return rows;
        }

        /// <summary>
        /// Obtains the table definition for a defined table, or throws if not available.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The table definition</returns>
        /// <exception cref="InvalidOperationException">If the table is not defined.</exception>
        private TableDefinition GetTableOrThrow(string tableName)
        {
            if (tableMap.TryGetValue(tableName, out TableDefinition table))
            {
                return table;
            }
            throw new InvalidOperationException($"Table '{tableName}' is not defined.");
        }

        /// <summary>
        /// Logs a SQL execution to the logging service.
        /// </summary>
        /// <param name="sqlStatement">The SQL Statement</param>
        /// <param name="parameters">The List of parameters</param>
        private void LogSqlStatement(string sqlStatement, IDictionary<string, object> parameters)
        {
            // Early return.
            if (Logger == null) return;

            Logger?.LogDebug("SQL STMT: {sqlStatement}", sqlStatement);
            if (parameters.Count > 0)
            {
                Logger?.LogDebug("SQL PARAMS: {params}", string.Join(";", parameters.ToList().Select(x => $"{x.Key}={x.Value}")));
            }
        }

        /// <summary>
        /// Dispose of the database connection.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DbConnection.Dispose();
            }
        }
    }
}

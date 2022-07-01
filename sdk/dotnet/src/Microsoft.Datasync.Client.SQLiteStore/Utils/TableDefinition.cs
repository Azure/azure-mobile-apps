// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Datasync.Client.SQLiteStore
{
    /// <summary>
    /// A <see cref="TableDefinition"/> is just a dictionary of column definitions.
    /// </summary>
    public class TableDefinition : Dictionary<string, ColumnDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="TableDefinition"/> that is empty.
        /// </summary>
        public TableDefinition()
        {
            TableName = string.Empty;
            IsInDatabase = false;
        }

        /// <summary>
        /// Creates a new <see cref="TableDefinition"/> with the supplied table definition.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="definition">The table definition.</param>
        public TableDefinition(string tableName, JObject definition) : base(StringComparer.OrdinalIgnoreCase)
        {
            TableName = tableName;

            // Add the ID column if it isn't present.
            if (!definition.TryGetValue(SystemProperties.JsonIdProperty, out _))
            {
                definition[SystemProperties.JsonIdProperty] = string.Empty;
            }

            var columns = definition.Properties().Select(prop => new ColumnDefinition(prop.Name, prop.Value.Type, SqlColumnType.Get(prop.Value.Type, allowNull: false)));
            foreach (var column in columns)
            {
                Add(column.Name, column);
            }
        }

        /// <summary>
        /// A flag to indicate whether the database definition has already been done.
        /// </summary>
        public bool IsInDatabase { get; set; }


        public string TableName { get; }

        /// <summary>
        /// The list of column definitions in the table.
        /// </summary>
        public IEnumerable<ColumnDefinition> Columns { get => Values; }
    }
}

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
        /// Creates a new <see cref="TableDefinition"/> with the supplied table definition.
        /// </summary>
        /// <param name="definition">The table definition.</param>
        public TableDefinition(IDictionary<string, ColumnDefinition> definition) : base(definition, StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Creates a new <see cref="TableDefinition"/> with the supplied table definition.
        /// </summary>
        /// <param name="definition">The table definition.</param>
        public TableDefinition(JObject definition) : base(StringComparer.OrdinalIgnoreCase)
        {
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
    }
}

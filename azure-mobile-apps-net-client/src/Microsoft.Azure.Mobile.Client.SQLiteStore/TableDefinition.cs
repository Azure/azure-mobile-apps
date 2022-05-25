// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    /// <summary>
    /// A class that represents the structure of table on local store
    /// </summary>
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    public class TableDefinition : Dictionary<string, ColumnDefinition>
    {
        /// <summary>
        /// The list of system properties
        /// </summary>
        public MobileServiceSystemProperties SystemProperties { get; private set; }

        /// <summary>
        /// Creates an empty table definition
        /// </summary>
        public TableDefinition()
        {
        }

        /// <summary>
        /// Creates a new table definition based on the column definitions provided
        /// </summary>
        /// <param name="definition">The list of columns to be defined</param>
        /// <param name="systemProperties">The system properties for this table</param>
        public TableDefinition(IDictionary<string, ColumnDefinition> definition, MobileServiceSystemProperties systemProperties)
            : base(definition, StringComparer.OrdinalIgnoreCase)
        {
            this.SystemProperties = systemProperties;
        }
    }
}

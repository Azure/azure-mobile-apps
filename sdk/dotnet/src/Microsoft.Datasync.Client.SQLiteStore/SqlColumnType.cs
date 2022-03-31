// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.SQLiteStore
{
    /// <summary>
    /// The list of the SQL column types.
    /// </summary>
    internal static class SqlColumnType
    {
        public const string Integer = "INTEGER";
        public const string Text = "TEXT";
        public const string None = "NONE";
        public const string Real = "REAL";
        public const string Numeric = "NUMERIC";
        public const string Boolean = "BOOLEAN";
        public const string DateTime = "DATETIME";
        public const string Float = "FLOAT";
        public const string Blob = "BLOB";
        public const string Guid = "GUID";
        public const string Json = "JSON";
        public const string Uri = "URI";
        public const string TimeSpan = "TIMESPAN";

        /// <summary>
        /// A mapping from the <see cref="JTokenType"/> to the equivalent SQL column type for
        /// supported mappings.
        /// </summary>
        private static readonly Dictionary<JTokenType, string> columnMapping = new()
        {
            { JTokenType.Boolean, SqlColumnType.Boolean },
            { JTokenType.Integer, SqlColumnType.Integer },
            { JTokenType.Date, SqlColumnType.DateTime },
            { JTokenType.Float, SqlColumnType.Float },
            { JTokenType.String, SqlColumnType.Text },
            { JTokenType.Guid, SqlColumnType.Guid },
            { JTokenType.Array, SqlColumnType.Json },
            { JTokenType.Object, SqlColumnType.Json },
            { JTokenType.Bytes, SqlColumnType.Blob },
            { JTokenType.Uri, SqlColumnType.Uri },
            { JTokenType.TimeSpan, SqlColumnType.TimeSpan }
        };

        /// <summary>
        /// Converts the incoming <see cref="JTokenType"/> type into the equivalent SQL column type.
        /// </summary>
        /// <param name="type">The incoming <see cref="JTokenType"/> type.</param>
        /// <param name="allowNull">If <c>true</c>, supports <see cref="JTokenType.Null"/>.</param>
        /// <returns>The store type.</returns>
        /// <exception cref="NotSupportedException">If the provided type is not supported by this store.</exception>
        public static string Get(JTokenType type, bool allowNull)
        {
            if (columnMapping.ContainsKey(type))
            {
                return columnMapping[type];
            }
            else if (type == JTokenType.Null && allowNull)
            {
                return null;
            }
            else
            {
                throw new NotSupportedException($"Property of type '{type}' is not supported.");
            }
        }
    }
}

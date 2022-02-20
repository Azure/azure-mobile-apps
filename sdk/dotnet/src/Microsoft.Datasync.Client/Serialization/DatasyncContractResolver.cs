// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// An <see cref="IContractResolver"/> implementation that is used with
    /// a <see cref="DatasyncClient"/>.
    /// </summary>
    public class DatasyncContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, string> tableNameCache = new();

        /// <summary>
        /// Indicates if the property names should be camel-cased when serialized
        /// out of JSON.
        /// </summary>
        internal bool CamelCasePropertyNames { get; set; }

        /// <summary>
        /// Returns a table name for a type, accounting for table renaming via the
        /// <see cref="DataTableAttribute"/> and the <see cref="JsonContainerAttribute"/>.
        /// </summary>
        /// <param name="type">The type for which to return the table name.</param>
        /// <returns>The table name.</returns>
        public string ResolveTableName(Type type)
        {
            Arguments.IsNotNull(type, nameof(type));

            string name = null;
            lock (tableNameCache)
            {
                if (!tableNameCache.TryGetValue(type, out name))
                {
                    // Default is the name of the type, but lower-case.
                    name = type.Name.ToLowerInvariant();

                    if (type.HasAttribute<JsonContainerAttribute>(out JsonContainerAttribute jsonattr))
                    {
                        if (!string.IsNullOrWhiteSpace(jsonattr.Title))
                        {
                            name = jsonattr.Title;
                        }
                    }

                    if (type.HasAttribute<DataTableAttribute>(out DataTableAttribute dtattr))
                    {
                        if (!string.IsNullOrWhiteSpace(dtattr.Name))
                        {
                            name = dtattr.Name;
                        }
                    }

                    tableNameCache[type] = name;
                    CreateContract(type);  // Build a JsonContract now to catch any contract errors early
                }
            }

            return name;
        }
    }
}
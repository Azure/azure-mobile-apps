// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using NJsonSchema.Generation;
using System;
using System.Linq;

namespace Microsoft.AspNetCore.Datasync.NSwag
{
    /// <summary>
    /// Schema processor for the Datasync Framework.
    /// </summary>
    public class DatasyncSchemaProcessor : ISchemaProcessor
    {
        /// <summary>
        /// List of the system properties within the <see cref="ITableData"/> interface.
        /// </summary>
        private static readonly string[] systemProperties = new string[] { "deleted", "updatedAt", "version" };

        /// <summary>
        /// Processes each schema in turn, doing required modifications.
        /// </summary>
        /// <param name="context">The schema processor context.</param>
        public void Process(SchemaProcessorContext context)
        {
            // This schema processor only processes types that implement <see cref="ITableData"/>.
            if (context.ContextualType.Type.GetInterfaces().Contains(typeof(ITableData)))
            {
                // This is an implementation of ITableData, so we need to make the system properties read-only.
                foreach (var prop in context.Schema.Properties)
                {
                    if (systemProperties.Contains(prop.Key))
                    {
                        prop.Value.IsReadOnly = true;
                    }
                }
            }
        }
    }
}

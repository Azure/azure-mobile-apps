// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using NJsonSchema;
using NJsonSchema.Generation;

namespace Microsoft.AspNetCore.Datasync.NSwag;

/// <summary>
/// The schema processor for Azure Mobile Apps entities based on <see cref="ITableData"/>.
/// </summary>
public class DatasyncSchemaProcessor : ISchemaProcessor
{
    /// <summary>
    /// The list of system properties within the <see cref="ITableData"/> interface.
    /// </summary>
    private static readonly string[] systemProperties = new[] { "deleted", "updatedAt", "version" };

    /// <inheritdoc/>
    public void Process(SchemaProcessorContext context)
    {
        if (context.ContextualType.Type.GetInterfaces().Contains(typeof(ITableData)))
        {
            foreach (KeyValuePair<string, JsonSchemaProperty> prop in context.Schema.Properties)
            {
                if (systemProperties.Contains(prop.Key))
                {
                    prop.Value.IsReadOnly = true;
                }
            }
        }
    }
}

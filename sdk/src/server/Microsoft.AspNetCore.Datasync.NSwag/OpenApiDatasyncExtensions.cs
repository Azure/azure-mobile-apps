// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using NSwag.Generation.AspNetCore;

namespace Microsoft.AspNetCore.Datasync.NSwag;

/// <summary>
/// A set of extension methods for working with OpenApi types.
/// </summary>
public static class OpenApiDatasyncExtensions
{
    /// <summary>
    /// Incorporates the operation and schema processors for supporting Azure Mobile Apps into
    /// the NSwag document processors.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddOpenApiDocument(settings => options.AddDatasyncProcessors());
    /// </code>
    /// </example>
    /// <param name="settings">The NSwag settings object.</param>
    public static void AddDatasyncProcessors(this AspNetCoreOpenApiDocumentGeneratorSettings settings)
    {
        settings.OperationProcessors.Add(new DatasyncOperationProcessor());
        settings.SchemaProcessors.Add(new DatasyncSchemaProcessor());
        settings.FlattenInheritanceHierarchy = true;
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Microsoft.AspNetCore.Datasync.Swashbuckle;

/// <summary>
/// Extensions to enable the developer to wire in the Datasync Controllers
/// Swashbuckle <see cref="IDocumentFilter"/> into the Swagger pipeline.
/// </summary>
public static class SwaggerGenExtensions
{
    /// <summary>
    /// Adds the necessary <see cref="IDocumentFilter"/> to the Swagger pipeline for
    /// handling Azure Mobile Apps table controllers.
    /// </summary>
    /// <param name="options">The SwaggerGen options.</param>
    public static void AddDatasyncControllers(this SwaggerGenOptions options)
        => options.AddDatasyncControllers(Assembly.GetCallingAssembly());

    /// <summary>
    /// Adds the necessary <see cref="IDocumentFilter"/> to the Swagger pipeline for
    /// handling Azure Mobile Apps table controllers.
    /// </summary>
    /// <param name="options">The SwaggerGen options.</param>
    /// <param name="assembly">The assembly containing the table controllers.</param>
    public static void AddDatasyncControllers(this SwaggerGenOptions options, Assembly assembly)
    {
        options.DocumentFilter<DatasyncDocumentFilter>(assembly);
    }
}

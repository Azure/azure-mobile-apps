// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// Extensions to enable the developer to wire in the Datasync Controllers Swashbuckle
    /// <see cref="IDocumentFilter"/> into the Swagger pipeline.
    /// </summary>
    public static class DatasyncSwashbuckleExtensions
    {
        public static void AddDatasyncControllers(this SwaggerGenOptions options)
        {
            options.DocumentFilter<DatasyncDocumentFilter>(Assembly.GetCallingAssembly());
        }
    }
}

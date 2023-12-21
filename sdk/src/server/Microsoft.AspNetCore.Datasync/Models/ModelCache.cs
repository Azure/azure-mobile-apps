// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Collections.Concurrent;

namespace Microsoft.AspNetCore.Datasync.Tables;

/// <summary>
/// Model cache for the <see cref="IEdmModel"/> needed for each table controller.
/// </summary>
internal static class ModelCache
{
    private static readonly Lazy<ConcurrentDictionary<Type, IEdmModel>> _cache = new(() => new ConcurrentDictionary<Type, IEdmModel>());

    /// <summary>
    /// Returns a cached version of the <see cref="IEdmModel"/> for a specific type.
    /// </summary>
    /// <param name="type">The type to reference.</param>
    /// <returns>The <see cref="IEdmModel"/> for the type.</returns>
    internal static IEdmModel GetEdmModel(Type type)
    {
        if (!_cache.Value.TryGetValue(type, out IEdmModel? model))
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EnableLowerCamelCase();
            modelBuilder.AddEntityType(type);
            model = modelBuilder.GetEdmModel();
            _cache.Value.TryAdd(type, model);
        }
        return model;
    }
}
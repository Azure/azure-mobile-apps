// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Concurrent;

namespace Microsoft.AspNetCore.Datasync.Tables;

/// <summary>
/// There is a memory issue if we create EdmModels for each request.  This class
/// caches the <see cref="IEdmModel"/> needed for each table controller.
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
        if (!_cache.Value.TryGetValue(type, out var model))
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

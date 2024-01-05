// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.Datasync;

public static class AspNetCoreExtensions
{
    /// <summary>
    /// Adds the services necessary to provide OData functionality for the Datasync service, building
    /// the <see cref="IEdmModel"/> from the entities configured via <see cref="TableController{TEntity}"/>.
    /// </summary>
    /// <param name="services">The current service collection.</param>
    /// <param name="model">The (optional) <see cref="IEdmModel"/> to use for configuring OData.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddDatasyncServices(this IServiceCollection services, IEdmModel? model = null)
    {
        services.AddSingleton<IDatasyncServiceOptions, DatasyncServiceOptions>();
        if (model != null)
        {
            services.AddSingleton<IEdmModel>(model);
        }
        return services;
    }
}

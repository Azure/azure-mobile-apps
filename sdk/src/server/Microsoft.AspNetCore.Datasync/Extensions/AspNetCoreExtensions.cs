// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync;

public static class AspNetCoreExtensions
{
    /// <summary>
    /// Adds the services necessary to provide OData functionality for the Datasync service, building
    /// the <see cref="IEdmModel"/> from the entities configured via <see cref="TableController{TEntity}"/>.
    /// </summary>
    /// <param name="services">The current service collection.</param>
    /// <returns>The modified service collection.</returns>
    [SuppressMessage("Roslynator", "RCS1021:Convert lambda expression body to expression body", Justification = "Readability")]
    public static IServiceCollection AddDatasyncControllers(this IServiceCollection services)
    {
        services.AddSingleton<IDatasyncServiceOptions, DatasyncServiceOptions>();
        services.AddControllers().AddOData(options =>
        {
            options.Count().Filter().OrderBy().Select().SetMaxTop(null);
        });
        return services;
    }

    /// <summary>
    /// Adds the services necessary to provide OData functionality for the Datasync service, using the
    /// provided <see cref="IEdmModel"/> as a basis for the OData configuration.
    /// </summary>
    /// <param name="services">The current service collection.</param>
    /// <param name="model">The <see cref="IEdmModel"/> to use for configuring OData.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddDatasyncControllers(this IServiceCollection services, IEdmModel model)
    {
        services.AddSingleton(model);
        return services.AddDatasyncControllers();
    }
}

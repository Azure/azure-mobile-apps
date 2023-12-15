// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.Datasync;

public static class AspNetCoreExtensions
{
    /// <summary>
    /// Adds the services necessary to provide OData functionality for the Datasync service, building
    /// the <see cref="IEdmModel"/> from the entities configured via <see cref="TableController{TEntity}"/>.
    /// </summary>
    /// <param name="services">The current service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddDatasyncControllers(this IServiceCollection services)
    {
        ODataConventionModelBuilder modelBuilder = new();
        // TODO: Build the EdmModel via reflection.
        return services.AddDatasyncControllers(modelBuilder.GetEdmModel());
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
        services.AddControllers().AddOData(options =>
        {
            options.EnableQueryFeatures();
            options.AddRouteComponents("tables", model);
            options.EnableAttributeRouting = true;
        });
        return services;
    }
}

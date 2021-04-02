// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;

namespace Microsoft.AzureMobile.Server
{
    /// <summary>
    /// The set of extension methods to introduce Azure Mobile into the service pipeline
    /// for ASP.NET Core
    /// </summary>
    public static class AspNetCoreExtensions
    {
        /// <summary>
        /// Adds the services necessary to provide OData functionality for Azure Mobile Apps.
        /// </summary>
        /// <param name="services">The current service collection</param>
        /// <returns>The resulting service collection</returns>
        public static IServiceCollection AddAzureMobile(this IServiceCollection services)
        {
            services.AddOData();

            services
                .AddTransient<ODataUriResolver>()
                .AddTransient<ODataQueryValidator>()
                .AddTransient<TopQueryValidator>()
                .AddTransient<CountQueryValidator>()
                .AddTransient<FilterQueryValidator>()
                .AddTransient<OrderByQueryValidator>()
                .AddTransient<SelectExpandQueryValidator>()
                .AddTransient<SkipQueryValidator>();

            return services;
        }

        /// <summary>
        /// Enables the OData functionality within individual routes for ASP.NET Core MVC.
        /// </summary>
        /// <param name="routeBuilder"></param>
        public static void EnableAzureMobile(this IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.Count().Expand().Filter().MaxTop(null).OrderBy().Select().SkipToken();
            routeBuilder.EnableDependencyInjection();
        }
    }
}

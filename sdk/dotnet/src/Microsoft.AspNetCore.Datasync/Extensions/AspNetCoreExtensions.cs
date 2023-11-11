// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Datasync.Converters;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;

namespace Microsoft.AspNetCore.Datasync
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
        public static IServiceCollection AddDatasyncControllers(this IServiceCollection services)
        {
            services
                .AddControllers()
                .AddOData(options => options.EnableQueryFeatures());

            return services;
        }

        /// <summary>
        /// Enables App Service Authentication to be supported within the application.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure">An option setting mechanisms</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddAzureAppServiceAuthentication(this AuthenticationBuilder builder, Action<AzureAppServiceAuthenticationOptions> configure)
            => builder.AddScheme<AzureAppServiceAuthenticationOptions, AzureAppServiceAuthenticationHandler>(AzureAppServiceAuthentication.AuthenticationScheme, AzureAppServiceAuthentication.DisplayName, configure);
    }
}

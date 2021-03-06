﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;

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
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

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
        public static void EnableTableControllers(this IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.Count().Expand().Filter().MaxTop(null).OrderBy().Select().SkipToken();
            routeBuilder.EnableDependencyInjection();
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

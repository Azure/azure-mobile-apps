// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Zumo.Server
{
    /// <summary>
    /// Extension methods for setting up Azure Mobile Apps in an <see cref="IServiceCollection"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class IServiceCollectionExtensions
    {

        /// <summary>
        /// Add Azure Mobile Apps services to the specified <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> to use for additiona.</param>
        /// <returns>An <see cref="IServiceCollection"/> that can be used to further configure the services.</returns>
        public static IServiceCollection AddAzureMobileApps(this IServiceCollection collection)
        {
            // Add the OData Functionality
            collection.AddOData();

            // Add transients for OData parsing.
            collection
                .AddTransient<ODataUriResolver>()
                .AddTransient<ODataQueryValidator>()
                .AddTransient<TopQueryValidator>()
                .AddTransient<FilterQueryValidator>()
                .AddTransient<SkipQueryValidator>()
                .AddTransient<OrderByQueryValidator>()
                .AddTransient<SkipTokenQueryValidator>();

            return collection;
        }
    }
}

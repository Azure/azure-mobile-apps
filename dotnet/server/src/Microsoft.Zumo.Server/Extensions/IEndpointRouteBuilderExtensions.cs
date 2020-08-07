// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Zumo.Server
{
    /// <summary>
    /// Injects dependency injection into the pipeline so that we can add OData functionality
    /// to a common controller.
    /// </summary>
    /// <remarks>
    /// There is very little up-side to checking code coverage of this method, so it's excluded 
    /// from testing.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public static class IEndpointRouteBuilderExtensions
    {
        public static void EnableAzureMobileApps(this IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.EnableDependencyInjection();
        }
    }
}

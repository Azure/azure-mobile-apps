// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;

namespace Swashbuckle.Application
{
    public static class SwaggerUiConfigExtensions
    {
        [CLSCompliant(false)]
        public static SwaggerUiConfig MobileAppUi(this SwaggerUiConfig config, HttpConfiguration httpConfig)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (httpConfig == null)
            {
                throw new ArgumentNullException("httpConfig");
            }

            httpConfig.MessageHandlers.Add(new SwaggerUiSecurityFilter(httpConfig));

            Assembly thisAssembly = typeof(SwaggerUiConfigExtensions).Assembly;
            config.CustomAsset("o2c-html", thisAssembly, "Microsoft.Azure.Mobile.Server.Swagger.o2c.html");
            config.CustomAsset("swagger-ui-min-js", thisAssembly, "Microsoft.Azure.Mobile.Server.Swagger.swagger-ui.min.js");
            config.CustomAsset("index", thisAssembly, "Microsoft.Azure.Mobile.Server.Swagger.index.html");

            return config;
        }
    }
}
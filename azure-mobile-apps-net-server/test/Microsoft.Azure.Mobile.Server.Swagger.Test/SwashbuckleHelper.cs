// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Owin.Testing;
using Owin;
using Swashbuckle.Application;

namespace Microsoft.Azure.Mobile.Server.Swagger.Test
{
    public static class SwashbuckleHelper
    {
        public static TestServer CreateSwaggerServer(HttpConfiguration config, Action<SwaggerDocsConfig> docsAction, Action<SwaggerUiConfig> uiAction)
        {
            return TestServer.Create(app =>
            {
                if (docsAction == null)
                {
                    docsAction = c =>
                    {
                        return;
                    };
                }

                if (uiAction == null)
                {
                    uiAction = c =>
                    {
                        return;
                    };
                }

                config.Services.Replace(typeof(IApiExplorer), new MobileAppApiExplorer(config));

                new MobileAppConfiguration()
                    .UseDefaultConfiguration()
                    .ApplyTo(config);

                config.EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "testService");
                    docsAction(c);
                })
                .EnableSwaggerUi(c =>
                {
                    uiAction(c);
                });

                app.UseWebApi(config);
            });
        }
    }
}
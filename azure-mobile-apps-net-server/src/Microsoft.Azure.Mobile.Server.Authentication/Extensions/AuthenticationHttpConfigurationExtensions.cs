// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Azure.Mobile.Server.Authentication;

namespace System.Web.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class AuthenticationHttpConfigurationExtensions
    {
        private const string ServiceTokenHandlerKey = "MS_ServiceTokenHandler";

        public static IAppServiceTokenHandler GetAppServiceTokenHandler(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            IAppServiceTokenHandler handler;
            if (!config.Properties.TryGetValue(ServiceTokenHandlerKey, out handler))
            {
                handler = new AppServiceTokenHandler(config);
                config.Properties[ServiceTokenHandlerKey] = handler;
            }

            return handler;
        }

        public static void SetAppServiceTokenHandler(this HttpConfiguration config, IAppServiceTokenHandler handler)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            config.Properties[ServiceTokenHandlerKey] = handler;
        }
    }
}
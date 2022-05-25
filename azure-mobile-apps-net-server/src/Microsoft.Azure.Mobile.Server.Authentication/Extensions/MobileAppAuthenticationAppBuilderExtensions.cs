// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Owin.Security;

namespace Owin
{
    /// <summary>
    /// Extension methods for <see cref="IAppBuilder"/>.
    /// </summary>
    public static class MobileAppAuthenticationAppBuilderExtensions
    {
        public static IAppBuilder UseAppServiceAuthentication(this IAppBuilder appBuilder, AppServiceAuthenticationOptions options)
        {
            if (appBuilder == null)
            {
                throw new ArgumentNullException("appBuilder");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            appBuilder.Use(typeof(AppServiceAuthenticationMiddleware), new object[]
            {
                appBuilder,
                options
            });

            return appBuilder;
        }
    }
}
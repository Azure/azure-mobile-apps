// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.Azure.Mobile.Server.Swagger;

namespace Swashbuckle.Application
{
    public static class SwaggerDocsConfigExtensions
    {
        [CLSCompliant(false)]
        public static OAuth2SchemeBuilder AppServiceAuthentication(this SwaggerDocsConfig config, string siteUri, string authProvider)
        {
            if (siteUri == null)
            {
                throw new ArgumentNullException("siteUri");
            }

            Uri uri = new Uri(siteUri);
            return AppServiceAuthentication(config, uri, authProvider);
        }

        [CLSCompliant(false)]
        public static OAuth2SchemeBuilder AppServiceAuthentication(this SwaggerDocsConfig config, Uri siteUri, string authProvider)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (siteUri == null)
            {
                throw new ArgumentNullException("siteUri");
            }

            if (authProvider == null)
            {
                throw new ArgumentNullException("authProvider");
            }

            config.OperationFilter(() => new MobileAppAuthenticationFilter(authProvider));

            Uri loginUri = new Uri(siteUri, new Uri(".auth/login/" + authProvider, UriKind.Relative));
            return config.OAuth2(authProvider)
                .Description("OAuth2 Implicit Grant")
                .Flow("implicit")
                .AuthorizationUrl(loginUri.ToString())
                .Scopes(scopes =>
                {
                    scopes.Add(authProvider, string.Empty);
                });
        }
    }
}
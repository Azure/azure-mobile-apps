// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Azure.Mobile.Server.Authentication
{
    /// <summary>
    /// The <see cref="AppServiceAuthenticationMiddleware"/> provides the OWIN middleware for authenticating a caller who has already authenticated using the Login controller.
    /// </summary>
    public class AppServiceAuthenticationMiddleware : AuthenticationMiddleware<AppServiceAuthenticationOptions>
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServiceAuthenticationMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next <see cref="OwinMiddleware"/>.</param>
        /// <param name="appBuilder">The <see cref="IAppBuilder"/> to configure.</param>
        /// <param name="options">The options for this middleware.</param>
        public AppServiceAuthenticationMiddleware(OwinMiddleware next, IAppBuilder appBuilder, AppServiceAuthenticationOptions options)
            : base(next, options)
        {
            if (appBuilder == null)
            {
                throw new ArgumentNullException("appBuilder");
            }

            this.logger = appBuilder.CreateLogger<AppServiceAuthenticationMiddleware>();
        }

        /// <inheritdoc />
        protected override AuthenticationHandler<AppServiceAuthenticationOptions> CreateHandler()
        {
            return new AppServiceAuthenticationHandler(this.logger);
        }
    }
}
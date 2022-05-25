// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile.Server.Authentication.AppService;
using Microsoft.Azure.Mobile.Server.Properties;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Azure.Mobile.Server.Authentication
{
    /// <summary>
    /// The <see cref="AppServiceAuthenticationHandler"/> authenticates a caller who has already authenticated using the Login controller.
    /// </summary>
    public class AppServiceAuthenticationHandler : AuthenticationHandler<AppServiceAuthenticationOptions>
    {
        public const string AuthenticationHeaderName = "x-zumo-auth";

        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServiceAuthenticationHandler"/> class with the given <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to use for logging.</param>
        public AppServiceAuthenticationHandler(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
        }

        protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            return Task.FromResult(this.Authenticate(this.Request, this.Options));
        }

        protected virtual AuthenticationTicket Authenticate(IOwinRequest request, AppServiceAuthenticationOptions options)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            ClaimsIdentity authenticatedIdentity = null;
            try
            {
                bool signingKeyExists = !string.IsNullOrEmpty(options.SigningKey);
                if (signingKeyExists)
                {
                    // Validate the token.
                    authenticatedIdentity = this.ValidateIdentity(request, options);
                }
                else
                {
                    // We can't validate without the signing key.
                    throw new InvalidOperationException(CommonResources.Property_CannotBeNull.FormatInvariant("SigningKey"));
                }
            }
            catch (Exception ex)
            {
                // An exception occurred. Ensure, we do not return an authenticated identity.
                this.logger.WriteError(RResources.Authentication_Error.FormatForUser(ex.Message), ex);
            }

            return this.CreateAuthenticationTicket(authenticatedIdentity);
        }

        protected virtual AuthenticationTicket CreateAuthenticationTicket(ClaimsIdentity identity)
        {
            if (identity == null)
            {
                // If we don't return a new ClaimsIdentity, it will cause request.User to be null.
                identity = new ClaimsIdentity();
            }

            return new AuthenticationTicket(identity, null);
        }

        /// <summary>
        /// Authenticates the login token from the <see cref="IOwinRequest"/> header, if it exists, and
        /// returns a <see cref="ClaimsIdentity"/> if authentication succeeded, or false if
        /// authentication failed. If token parsing failed, returns null.
        /// </summary>
        /// <param name="request">The <see cref="IOwinRequest"/> to authenticate.</param>
        /// <param name="options">Authentication options.</param>
        /// <returns>Returns the <see cref="ClaimsIdentity"/> if token validation succeeded.
        /// Returns null if token parsing failed for any reason.</returns>
        protected virtual ClaimsIdentity ValidateIdentity(IOwinRequest request, AppServiceAuthenticationOptions options)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            string[] tokenFromHeaderCollection;            
            if (!request.Headers.TryGetValue(AuthenticationHeaderName, out tokenFromHeaderCollection))
            {
                return null;
            }
            string tokenFromHeader = tokenFromHeaderCollection[0];            

            // Attempt to parse and validate the token from header
            ClaimsPrincipal claimsPrincipalFromToken;
            bool claimsAreValid = options.TokenHandler.TryValidateLoginToken(tokenFromHeader, options.SigningKey, options.ValidAudiences, options.ValidIssuers, out claimsPrincipalFromToken);
            if (claimsAreValid)
            {
                return claimsPrincipalFromToken.Identity as ClaimsIdentity;
            }
            else
            {
                this.logger.WriteInformation(RResources.Authentication_InvalidToken);
                return null;
            }
        }
    }
}
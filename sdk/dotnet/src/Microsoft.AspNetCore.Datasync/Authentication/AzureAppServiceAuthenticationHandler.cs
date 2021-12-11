// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// An <see cref="AuthenticationHandler{TOptions}"/> that implements the Azure App Service
    /// Authentication and Authorization protocol.
    /// </summary>
    public class AzureAppServiceAuthenticationHandler : AuthenticationHandler<AzureAppServiceAuthenticationOptions>
    {
        private const string AppServicePrincipalToken = "X-MS-CLIENT-PRINCIPAL";
        private const string AppServicePrincipalIdP = "X-MS-CLIENT-PRINCIPAL-IDP";
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAppServiceAuthenticationHandler"/>.
        /// </summary>
        /// <param name="options">The monitor for the options instance.</param>
        /// <param name="loggerFactory">An <see cref="ILoggerFactory"/></param>
        /// <param name="encoder">A <see cref="UrlEncoder"/></param>
        /// <param name="clock">The system clock</param>
        public AzureAppServiceAuthenticationHandler(
            IOptionsMonitor<AzureAppServiceAuthenticationOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, loggerFactory, encoder, clock)
        {
            _logger = loggerFactory.CreateLogger<AzureAppServiceAuthenticationHandler>();
        }

        /// <summary>
        /// Handle an authentication challenge.
        /// </summary>
        /// <returns>An authentication result</returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!IsEnabled)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            try
            {
                if (!Context.Request.Headers.TryGetValue(AppServicePrincipalToken, out StringValues encodedToken))
                {
                    _logger.LogDebug("No authentication header found - skipping authentication check");
                    return Task.FromResult(AuthenticateResult.NoResult());
                }

                _logger.LogDebug("Azure App Service Token = {encodedToken}", encodedToken);
                var token = AzureAppServiceToken.FromString(encodedToken);
                var claims = token.Claims.Select(claim => new Claim(claim.Type, claim.Value));

                if (!Context.Request.Headers.TryGetValue(AppServicePrincipalIdP, out StringValues idp))
                {
                    throw new TokenValidationException($"{AppServicePrincipalIdP} was expected, but not found");
                }
                if (token.Provider != idp[0])
                {
                    throw new TokenValidationException($"{AppServicePrincipalIdP} header does not match the token value");
                }
                if (!claims.Any(c => c.Type.Equals(token.NameType, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new TokenValidationException($"{AppServicePrincipalToken} token does not contain any names");
                }

                Context.User = new ClaimsPrincipal();
                Context.User.AddIdentity(new ClaimsIdentity(claims, token.Provider, token.NameType, token.RoleType));

                _logger.LogDebug("Azure App Service Authentication for user {user} succeeded", claims.Single(claims => claims.Type == token.NameType).Value);
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(Context.User, token.Provider)));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to handle Azure App Service authentication: {Message}", ex.Message);
                return Task.FromResult(AuthenticateResult.Fail(ex));
            }
        }

        /// <summary>
        /// Shows if the Azure App Service authentication scheme is enabled.
        /// </summary>
        internal bool IsEnabled { get => AzureAppServiceAuthentication.IsEnabled() || Options.ForceEnable; }
    }
}

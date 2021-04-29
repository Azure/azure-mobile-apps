// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Microsoft.AzureMobile.Server.Authentication
{
    public class AppServiceAuthenticationHandler : AuthenticationHandler<AppServiceAuthenticationOptions>
    {
        private const string AppServicePrincipalToken = "X-MS-CLIENT-PRINCIPAL";
        private readonly bool _isEnabled;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServiceAuthenticationHandler"/>.
        /// </summary>
        /// <param name="options">The monitor for the options instance</param>
        /// <param name="logger">An <see cref="ILoggerFactory"/></param>
        /// <param name="encoder">A <see cref="UrlEncoder"/></param>
        /// <param name="clock">The system clock</param>
        public AppServiceAuthenticationHandler(
            IOptionsMonitor<AppServiceAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock
        ) : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<AppServiceAuthenticationHandler>();
            _isEnabled = AppServiceAuthentication.IsEnabled();
            if (!_isEnabled)
            {
                _logger.LogInformation("App Service Authentication is enabled");
            }
        }

        /// <summary>
        /// Handle an authentication challenge.
        /// </summary>
        /// <returns>An authentication result.</returns>
        [SuppressMessage("Usage", "RCS1229:Use async/await when necessary.", Justification = "No async required, but part of interface")]
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!_isEnabled)
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

                // Decode the JWT from App Service Authentication
                _logger.LogDebug($"Token = {encodedToken}");
                var token = AppServiceToken.FromString(encodedToken);
                var claims = token.Claims.Select(claim => new Claim(claim.Type, claim.Value));

                // Transfer to a ClaimsPrincipal
                var principal = new ClaimsPrincipal();
                principal.AddIdentity(new ClaimsIdentity(claims, token.Provider, token.NameType, token.RoleType));

                // Add to context and return a ticket
                Context.User = principal;
                _logger.LogDebug($"AppService Authentication success for user {claims.Single(c => c.Type == token.NameType).Value}");
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, token.Provider)));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to handle authentication: {ex.Message}");
                return Task.FromResult(AuthenticateResult.Fail(ex));
            }
        }

        /// <summary>
        /// Gets a header value, with a default value.
        /// </summary>
        /// <param name="headerName">The header name</param>
        /// <returns></returns>
        private string GetDefaultHeader(string headerName)
            => Context.Request.Headers[headerName].FirstOrDefault() ?? "unknown";
    }
}

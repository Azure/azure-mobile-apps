// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Authentication;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A generic authentication provider that gets a JWT token from the specified
    /// action and puts it in the Authorization header.  The JWT is kept around until
    /// about 2-3 minutes before it expires, at which point a new one is requested.
    /// </summary>
    public class GenericAuthenticationProvider : AuthenticationProvider
    {
        /// <summary>
        /// Creates a new <see cref="GenericAuthenticationProvider"/> by specifying a function for the token requester.
        /// </summary>
        /// <param name="asyncTokenRequestor">The token requester</param>
        /// <param name="headerName">The name of the header</param>
        /// <param name="authenticationType">The authentication type (if specified)</param>
        public GenericAuthenticationProvider(Func<Task<AuthenticationToken>> asyncTokenRequestor, string headerName = "Authorization", string authenticationType = null)
        {
            TokenRequestorAsync = asyncTokenRequestor ?? throw new ArgumentNullException(nameof(asyncTokenRequestor));
            Arguments.IsNotNullOrWhitespace(headerName, nameof(headerName));

            if (headerName.Equals("authorization", StringComparison.InvariantCultureIgnoreCase))
            {
                authenticationType ??= "Bearer";
            }

            if (authenticationType != null && string.IsNullOrWhiteSpace(authenticationType))
            {
                throw new ArgumentException($"{nameof(authenticationType)} must be specified (or null if not Authorization header)", nameof(authenticationType));
            }

            HeaderName = headerName;
            AuthenticationType = authenticationType;
        }

        /// <summary>
        /// The function used to request the token.
        /// </summary>
        internal Func<Task<AuthenticationToken>> TokenRequestorAsync { get; set; }

        /// <summary>
        /// The header name to use for authentication
        /// </summary>
        internal string HeaderName { get; }

        /// <summary>
        /// The authentication type (normally Bearer)
        /// </summary>
        internal string AuthenticationType { get; }

        /// <summary>
        /// The current authentication token
        /// </summary>
        internal AuthenticationToken? Current { get; set; }

        /// <summary>
        /// The amount of time prior to expiry that we refresh the token
        /// </summary>
        private TimeSpan _bufferPeriod = new(0, 2, 0); // Two minutes;
        public TimeSpan RefreshBufferTimeSpan
        {
            get => _bufferPeriod;
            set
            {
                if (value.TotalMilliseconds < 1000)
                {
                    throw new ArgumentException("Refresh buffer is too short", nameof(RefreshBufferTimeSpan));
                }
                _bufferPeriod = value;
            }
        }

        /// <summary>
        /// Initiate a login request out of band of the pipeline.  This can be used to initiate the login process via a button.
        /// </summary>
        /// <returns>An async task that resolves when the login is complete</returns>
        public override Task LoginAsync() => GetTokenAsync(true);

        /// <summary>
        /// Gets a valid authentication token
        /// </summary>
        /// <param name="force">If true, forces re-acquisition of the authentication token</param>
        /// <returns>The authentication token (asynchronously)</returns>
        public async Task<string> GetTokenAsync(bool force = false)
        {
            if (force || IsExpired(Current))
            {
                Current = await TokenRequestorAsync.Invoke().ConfigureAwait(false);
                IsLoggedIn = !IsExpired(Current);
                UserId = !IsLoggedIn ? null : Current.Value.UserId;
                DisplayName = !IsLoggedIn ? null : Current.Value.DisplayName;
            }
            if (IsExpired(Current))
            {
                System.Diagnostics.Debug.WriteLine($"GenericAuthenticationProvider:: Current is expired - token = {Current}");
            }

            return IsExpired(Current) ? null : Current.Value.Token;
        }

        /// <summary>
        /// Determines if the token is valid and unexpired.
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>true if the token is valid.</returns>
        internal bool IsExpired(AuthenticationToken? token)
        {
            try
            {
                if (!token.HasValue) return true;
                return DateTimeOffset.Now >= token.Value.ExpiresOn.Subtract(RefreshBufferTimeSpan);
            }
            catch
            {
                // If any errors occurred, treat as if the token is expired.
                return true;
            }
        }

        /// <summary>
        /// The delegating handler for this request - injects the authorization header into the request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The response (asynchronously)</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (request.Headers.Contains(HeaderName))
            {
                request.Headers.Remove(HeaderName);
            }

            var token = await GetTokenAsync().ConfigureAwait(false);
            if (token == null)
            {
                System.Diagnostics.Debug.WriteLine("GenericAuthenticationProvider::: token is null");
            }
            if (token != null)
            {
                var headerValue = AuthenticationType != null ? $"{AuthenticationType} {token}" : token;

                request.Headers.Add(HeaderName, headerValue);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}

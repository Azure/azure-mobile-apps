// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Microsoft.Azure.Mobile.Server.Authentication
{
    /// <summary>
    /// Provides an abstraction for handling security tokens. This abstraction can be used for validating security
    /// tokens and creating <see cref="ClaimsPrincipal"/> instances.
    /// </summary>
    public interface IAppServiceTokenHandler
    {
        /// <summary>
        /// Validates a string representation of a mobile service authentication token used to authenticate a user request.
        /// </summary>
        /// <param name="token">A <see cref="string"/> representation of the authentication token to validate.</param>
        /// <param name="signingKey">The secret key with which the token has been signed.</param>
        /// <param name="validAudiences">The valid audiences to accept in token validation.</param>
        /// <param name="validIssuers">The valid issuers to accept in token validation.</param>
        /// <param name="claimsPrincipal">The resulting <see cref="ClaimsPrincipal"/> if the token is valid; null otherwise.</param>
        /// <returns><c>true</c> if <paramref name="token"/> is valid; otherwise <c>false</c>/</returns>
        bool TryValidateLoginToken(string token, string signingKey, IEnumerable<string> validAudiences, IEnumerable<string> validIssuers, out ClaimsPrincipal claimsPrincipal);

        /// <summary>
        /// Creates a user id value contained within a <see cref="ProviderCredentials"/>. The user id is of the form
        /// <c>ProviderName:ProviderId</c> where the <c>ProviderName</c> is the unique identifier for the login provider
        /// and the <c>ProviderId</c> is the provider specific id for a given user.
        /// </summary>
        /// <param name="providerName">The login provider name.</param>
        /// <param name="providerUserId">The provider specific user id.</param>
        /// <returns>A formatted <see cref="string"/> representing the resulting value.</returns>
        string CreateUserId(string providerName, string providerUserId);

        /// <summary>
        /// Parses a user id into its two components: a <c>ProviderName</c> which uniquely identifies the login provider
        /// and the <c>ProviderId</c> which identifies the provider specific id for a given user.
        /// </summary>
        /// <param name="userId">The input value to parse.</param>
        /// <param name="providerName">The login provider name; or <c>null</c> if the <paramref name="userId"/> is not valid.</param>
        /// <param name="providerUserId">The provider specific user id; or <c>null</c> is the <paramref name="userId"/> is not valid.</param>
        /// <returns><c>true</c> if <paramref name="userId"/> is valid; otherwise <c>false</c>/</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "This is not unreasonable for this API.")]
        bool TryParseUserId(string userId, out string providerName, out string providerUserId);
    }
}
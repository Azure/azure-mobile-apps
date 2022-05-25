//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Mobile.Server.Authentication.AppService
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;

    /// <summary>
    /// Common data contract for storing provider-specific security tokens.
    /// </summary>
    /// <remarks>
    /// This is a strongly-typed token container class. Not all token types will
    /// apply to all providers. Null-valued properties will be explicitly excluded
    /// from the serialized JSON (<see cref="DataMemberAttribute.EmitDefaultValue"/>).
    /// </remarks>
    [DataContract]
    internal sealed class TokenEntry 
    {
        public TokenEntry(string providerName)
        {
            this.ProviderName = providerName;
            this.UserClaims = new List<ClaimSlim>();
        }

        /// <summary>
        /// The well-known alias of the provider that issued the token (e.g. "aad").
        /// </summary>
        [DataMember(Name = "provider_name")]
        internal string ProviderName { get; set; }

        /// <summary>
        /// Gets or sets a friendly, provider-specific User ID for the current entry (e.g. "name@contoso.com").
        /// </summary>
        [DataMember(Name = "user_id")]
        internal string UserId { get; set; }

        /// <summary>
        /// Gets or sets an OAuth 1.0a/2.0 access token for the current entry.
        /// </summary>
        [DataMember(Name = "access_token", EmitDefaultValue = false)]
        internal string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets an OAuth 1.0a access token secret for the current entry.
        /// </summary>
        [DataMember(Name = "access_token_secret", EmitDefaultValue = false)]
        internal string AccessTokenSecret { get; set; }

        /// <summary>
        /// Gets or sets the date/time on which the OAuth 2.0 access token expires.
        /// </summary>
        [DataMember(Name = "expires_on", EmitDefaultValue = false)]
        internal DateTime? ExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets an OAuth 2.0 refresh token for the current entry.
        /// </summary>
        [DataMember(Name = "refresh_token", EmitDefaultValue = false)]
        internal string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets an OpenID Connection ID token for the current entry.
        /// </summary>
        [DataMember(Name = "id_token", EmitDefaultValue = false)]
        internal string IdToken { get; set; }

        /// <summary>
        /// Gets or sets an authentication token for the current entry.
        /// </summary>
        [DataMember(Name = "authentication_token", EmitDefaultValue = false)]
        internal string AuthenticationToken { get; set; }

        /// <summary>
        /// Gets or sets provider-specific user claims for this current entry.
        /// </summary>
        [DataMember(Name = "user_claims", EmitDefaultValue = false)]
        internal List<ClaimSlim> UserClaims { get; set; }
    }
}

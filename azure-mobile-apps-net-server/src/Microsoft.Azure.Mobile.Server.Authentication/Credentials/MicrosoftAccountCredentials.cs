// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;

namespace Microsoft.Azure.Mobile.Server.Authentication
{
    /// <summary>
    /// A <see cref="ProviderCredentials"/> implementation containing provider specific credentials for Microsoft Account authentication.
    /// </summary>
    public class MicrosoftAccountCredentials : ProviderCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftAccountCredentials"/> class.
        /// </summary>
        public MicrosoftAccountCredentials()
            : base("MicrosoftAccount")
        {
        }

        /// <summary>
        /// Gets or sets the access token for the current user.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the refresh token for the current user.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the expiration time for the access token.
        /// </summary>
        public DateTimeOffset? AccessTokenExpiration { get; set; }
    }
}

// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Server.Authentication
{
    /// <summary>
    /// A <see cref="ProviderCredentials"/> implementation containing provider specific credentials for Facebook authentication.
    /// </summary>
    public class FacebookCredentials : ProviderCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookCredentials"/> class.
        /// </summary>
        public FacebookCredentials()
            : base("Facebook")
        {
        }

        /// <summary>
        /// Gets or sets the access token secret for the current user.
        /// </summary>
        public string AccessToken { get; set; }
    }
}

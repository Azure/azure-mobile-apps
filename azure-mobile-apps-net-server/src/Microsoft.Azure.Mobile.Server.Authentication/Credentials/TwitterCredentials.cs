// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Server.Authentication
{
    /// <summary>
    /// A <see cref="ProviderCredentials"/> implementation containing provider specific credentials for Twitter authentication.
    /// </summary>
    public class TwitterCredentials : ProviderCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterCredentials"/> class.
        /// </summary>
        public TwitterCredentials()
            : base("Twitter")
        {
        }

        /// <summary>
        /// Gets or sets the access token for the current user.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the access token secret for the current user.
        /// </summary>
        public string AccessTokenSecret { get; set; }
    }
}

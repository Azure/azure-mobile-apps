// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Server.Authentication
{
    /// <summary>
    /// A <see cref="ProviderCredentials"/> implementation containing provider specific credentials for Azure Active directory authentication.
    /// </summary>
    public class AzureActiveDirectoryCredentials : ProviderCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureActiveDirectoryCredentials"/> class.
        /// </summary>
        public AzureActiveDirectoryCredentials()
            : base("Aad")
        {
        }

        /// <summary>
        /// Gets or sets the tenant ID for the current user.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the object ID for the current user.
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the access token for the current user.
        /// </summary>
        public string AccessToken { get; set; }
    }
}

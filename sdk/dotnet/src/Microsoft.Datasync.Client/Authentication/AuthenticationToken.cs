// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Definition of an authentication token response.
    /// </summary>
    public struct AuthenticationToken
    {
        /// <summary>
        /// The display name for this user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The expiry date of the JWT Token
        /// </summary>
        public DateTimeOffset ExpiresOn { get; set; }
        /// <summary>
        /// The actual JWT Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The User Id for this user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Return a visual representation of the authentication token for logging purposes.
        /// </summary>
        /// <returns>The string representation of the authentication token</returns>
        public override string ToString()
        {
            return $"AuthenticationToken(DisplayName=\"{DisplayName}\",ExpiresOn=\"{ExpiresOn}\",Token=\"{Token}\",UserId=\"{UserId}\")";
        }
    }
}

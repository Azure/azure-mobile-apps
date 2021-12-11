// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Authentication
{
    /// <summary>
    /// Definition of an authentication provider, which is a specific type of
    /// delegating handler that handles authentication updates.
    /// </summary>
    public abstract class AuthenticationProvider : DelegatingHandler
    {
        /// <summary>
        /// The display name for the currently logged in user.  This may be null.
        /// </summary>
        public string DisplayName { get; protected set; }

        /// <summary>
        /// If true, the user is logged in (and the UserId is available)
        /// </summary>
        public bool IsLoggedIn { get; protected set; }

        /// <summary>
        /// The user ID for this user.
        /// </summary>
        public string UserId { get; protected set; }

        /// <summary>
        /// Initiate a login request out of band of the pipeline.  This can be used
        /// to initiate the login process via a button.
        /// </summary>
        /// <returns>An async task that resolves when the login is complete</returns>
        public abstract Task LoginAsync();
    }
}

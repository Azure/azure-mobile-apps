// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Security
{
    /// <summary>
    /// Used to specify the required authorization level for service resources
    /// </summary>
    public enum AuthorizationLevel
    {
        /// <summary>
        /// Allow access to anonymous requests.
        /// </summary>
        Anonymous = 0,

        /// <summary>
        /// Allow access to requests that include the application key as a header.
        /// </summary>
        Application,

        /// <summary>
        /// Allow access to requests that include a valid authentication token as a header.
        /// </summary>
        User,

        /// <summary>
        /// Allow access to requests that include the master key as a header.
        /// </summary>
        Admin
    }
}

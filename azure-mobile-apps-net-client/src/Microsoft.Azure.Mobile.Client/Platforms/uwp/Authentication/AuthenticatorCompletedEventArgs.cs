// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class AuthenticatorCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Whether the authentication succeeded and there is a valid authorization code.
        /// </summary>
        public bool IsAuthenticated { get { return AuthorizationCode != null; } }

        /// <summary>
        /// Gets the authorization code that represents this authentication.
        /// </summary>
        /// <value>
        /// The authorization code.
        /// </value>
        public string AuthorizationCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AuthenticatorCompletedEventArgs class.
        /// </summary>
        /// <param name='authorizationCode'>
        /// The authorization code received or null if authentication failed or was canceled.
        /// </param>
        public AuthenticatorCompletedEventArgs(string authorizationCode)
        {
            AuthorizationCode = authorizationCode;
        }
    }
}
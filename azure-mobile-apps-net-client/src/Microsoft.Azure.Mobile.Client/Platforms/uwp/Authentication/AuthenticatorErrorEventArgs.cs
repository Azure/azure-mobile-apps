// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class AuthenticatorErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a message describing the error.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AuthenticatorErrorEventArgs class
        /// with a message.
        /// </summary>
        /// <param name='message'>
        /// A message describing the error.
        /// </param>
        public AuthenticatorErrorEventArgs(string message)
        {
            Message = message;
        }
    }
}
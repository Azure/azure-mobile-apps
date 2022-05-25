// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// List of client options, for creating mobile clients.
    /// </summary>
    public interface IMobileServiceClientOptions
    {
        /// <summary>
        /// The backend Uri of the Azure Mobile Apps service
        /// </summary>
        Uri MobileAppUri { get; }

        /// <summary>
        /// The location of the web authenticator backend (normally null)
        /// </summary>
        Uri AlternateLoginHost { get; }

        /// <summary>
        /// The Login prefix (normally /.auth)
        /// </summary>
        string LoginUriPrefix { get; }

        /// <summary>
        /// How to get the default message handlers.
        /// </summary>
        /// <param name="client">The client reference</param>
        /// <returns></returns>
        IEnumerable<HttpMessageHandler> GetDefaultMessageHandlers(IMobileServiceClient client);
    }
}

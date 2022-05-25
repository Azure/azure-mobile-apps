// ---------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Web.Http;

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// Provides a way to initialize the <see cref="HttpConfiguration"/> during Mobile App configuration.
    /// </summary>
    public interface IMobileAppExtensionConfigProvider
    {
        /// <summary>
        /// Initializes the extension.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration"/>.</param>
        void Initialize(HttpConfiguration config);
    }
}
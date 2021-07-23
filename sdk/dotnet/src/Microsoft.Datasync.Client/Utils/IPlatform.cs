// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// Definition of the platform-specific API.
    /// </summary>
    internal interface IPlatform
    {
        /// <summary>
        /// The handler for the key-value storage for the application.
        /// </summary>
        IApplicationStorage ApplicationStorage { get; }

        /// <summary>
        /// The platform-specific information.
        /// </summary>
        IPlatformInformation PlatformInformation { get; }
    }
}

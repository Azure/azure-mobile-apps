// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// Information about the operating system
    /// </summary>
    internal interface IOSInformation
    {
        /// <summary>
        /// Name of the operating system
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The version of the operating system
        /// </summary>
        string Version { get; }

        /// <summary>
        /// The architecture of the operating system
        /// </summary>
        string Architecture { get; }
    }

    /// <summary>
    /// Information about the platform we are running on.
    /// </summary>
    internal interface IPlatformInformation
    {
        /// <summary>
        /// Information about the operating system
        /// </summary>
        IOSInformation OS { get; }

        /// <summary>
        /// The details section of the user-agent string.
        /// </summary>
        string UserAgentDetails { get; }
    }
}

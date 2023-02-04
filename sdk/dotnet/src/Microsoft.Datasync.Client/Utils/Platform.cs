// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Platforms;
using System;

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// Provides access to the platform-specific API.
    /// </summary>
    internal static class Platform
    {
        /// <summary>
        /// Internal lazy initializer for the platform-specific API.
        /// </summary>
        private static readonly Lazy<IPlatform> _currentPlatform = new(() => new CurrentPlatform());

        /// <summary>
        /// Reference to the current platform.
        /// </summary>
        internal static IPlatform Instance
        {
            get => _currentPlatform.Value;
        }

        /// <summary>
        /// Obtains the assembly version of the current assembly.
        /// </summary>
        internal static string AssemblyVersion => Instance.GetType().Assembly.GetName().Version.ToString();

        /// <summary>
        /// The details section of the User-Agent header.
        /// </summary>
        internal static string UserAgentDetails
        {
            get => Instance.PlatformInformation.UserAgentDetails;
        }
    }
}

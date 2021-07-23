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
        /// The key for the installation ID within the application storage handler.
        /// </summary>
        private const string InstallationIdKey = "installationId";

        /// <summary>
        /// Reference to the current platform.
        /// </summary>
        internal static IPlatform Instance
        {
            get => _currentPlatform.Value;
        }

        /// <summary>
        /// Gets the application storage handler for the current platform.
        /// </summary>
        internal static IApplicationStorage ApplicationStorage
        {
            get => Instance.ApplicationStorage;
        }

        /// <summary>
        /// Obtains the assembly version of the current assembly.
        /// </summary>
        internal static string AssemblyVersion => Instance.GetType().Assembly.GetName().Version.ToString();

        /// <summary>
        /// Returns the internal static application installation ID.
        /// </summary>
        internal static string InstallationId
        {
            get
            {
                IApplicationStorage storage = Instance.ApplicationStorage;
                if (storage.TryGetValue(InstallationIdKey, out string installationId))
                {
                    return installationId;
                }

                string newId = Guid.NewGuid().ToString();
                storage.SetValue(InstallationIdKey, newId);
                return newId;
            }
        }

        /// <summary>
        /// Gets the platform information for the current platform.
        /// </summary>
        internal static IPlatformInformation PlatformInformation
        {
            get => Instance.PlatformInformation;
        }

        /// <summary>
        /// The details section of the User-Agent header.
        /// </summary>
        internal static string UserAgentDetails
        {
            get => PlatformInformation.UserAgentDetails;
        }
    }
}

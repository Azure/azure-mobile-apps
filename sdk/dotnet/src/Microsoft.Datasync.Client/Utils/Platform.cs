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
        /// Internal lazy initializer for the applications Installation Id
        /// </summary>
        private static readonly Lazy<string> _installationId = new(() => GetApplicationInstallationId());

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
        /// Obtains the assembly version of the current assembly.
        /// </summary>
        internal static string AssemblyVersion => Instance.GetType().Assembly.GetName().Version.ToString();

        /// <summary>
        /// The internal static application installation ID.
        /// </summary>
        internal static string InstallationId { get => _installationId.Value; }

        /// <summary>
        /// Retrieve the application installation ID, creating one if it doesn't exist.
        /// </summary>
        internal static string GetApplicationInstallationId()
        {
            if (Instance.ApplicationStorage.TryGetValue(InstallationIdKey, out string installationId))
            {
                return installationId;
            }

            string newId = Guid.NewGuid().ToString();
            Instance.ApplicationStorage.SetValue(InstallationIdKey, newId);
            return newId;
        }

        /// <summary>
        /// The details section of the User-Agent header.
        /// </summary>
        internal static string UserAgentDetails
        {
            get => Instance.PlatformInformation.UserAgentDetails;
        }
    }
}

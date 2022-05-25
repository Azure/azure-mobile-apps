// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides access to platform-specific framework API's.
    /// </summary>
    internal static class Platform
    {
        /// <summary>
        /// The string value to use for the operating system name, arch, or version if
        /// the value is unknown.
        /// </summary>
        public const string UnknownValueString = "--";

        private static IPlatform current;

        /// <summary>
        /// Gets the current platform. If none is loaded yet, accessing this property triggers platform resolution.
        /// </summary>
        public static IPlatform Instance
        {
            get
            {
                // create if not yet created
                if (current == null)
                {
                    current = new CurrentPlatform();
                }

                return current;
            }

            // keep this public so we can set a Platform for unit testing.
            set
            {
                current = value;
            }
        }
    }
}

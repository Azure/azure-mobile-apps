// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Implements the <see cref="IPlatform"/> interface for the Windows
    /// Phone platform.
    /// </summary>
    internal class PlatformInformation : IPlatformInformation
    {
        /// <summary>
        /// A singleton instance of the <see cref="ApplicationStorage"/>.
        /// </summary>
        public static IPlatformInformation Instance { get; } = new PlatformInformation();

        /// <summary>
        /// The architecture of the platform.
        /// </summary>
        public string OperatingSystemArchitecture => RuntimeInformation.OSArchitecture.ToString();

        /// <summary>
        /// The name of the operating system of the platform.
        /// </summary>
        public string OperatingSystemName => RuntimeInformation.OSDescription;

        /// <summary>
        /// The version of the operating system of the platform.
        /// </summary>
        public string OperatingSystemVersion => Platform.UnknownValueString;

        /// <summary>
        /// Indicated whether the device is an emulator or not
        /// </summary>
        public bool IsEmulator => false;

        public string Version => this.GetVersionFromAssemblyFileVersion();
    }
}

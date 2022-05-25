// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using UIKit;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class PlatformInformation : IPlatformInformation
    {
        /// <summary>
        /// A singleton instance of the <see cref="PlatformInformation"/>.
        /// </summary>
        public static IPlatformInformation Instance { get; } = new PlatformInformation();

        public string OperatingSystemArchitecture => PlatformID.MacOSX.ToString();

        public string OperatingSystemName => "iOS";

        public string OperatingSystemVersion => UIDevice.CurrentDevice.SystemVersion;

        public bool IsEmulator => (UIDevice.CurrentDevice.Model.ToLower().Contains("simulator"));

        public string Version => this.GetVersionFromAssemblyFileVersion();
    }
}

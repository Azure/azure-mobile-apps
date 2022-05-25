// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Android.OS;
using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    class PlatformInformation : IPlatformInformation
    {
        public static IPlatformInformation Instance { get; } = new PlatformInformation();

        public string OperatingSystemArchitecture => System.Environment.OSVersion.Platform.ToString();

        public string OperatingSystemName => "Android";

        public string OperatingSystemVersion => Build.VERSION.Release;

        public bool IsEmulator => Build.Brand.Equals("generic", StringComparison.OrdinalIgnoreCase);

        public string Version => this.GetVersionFromAssemblyFileVersion();
    }
}

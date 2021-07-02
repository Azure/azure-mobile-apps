// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Windows.ApplicationModel;

namespace Microsoft.Datasync.Client.Platforms
{
    /// <summary>
    /// UWP-specific implementation of the <see cref="IPlatformInformation"/> interface.
    /// </summary>
    internal class PlatformInformation : IPlatformInformation
    {
        /// <summary>
        /// Information about the OS.
        /// </summary>
        public IOSInformation OS => new OSInformation
        {
            Architecture = Package.Current.Id.Architecture.ToString(),
            Name = "UAP",
            Version = System.Environment.OSVersion.VersionString
        };

        /// <summary>
        /// True if this is running on an emulated device
        /// </summary>
        public bool IsEmulator => false;

        /// <summary>
        /// Determines if the debugger is attached.
        /// </summary>
        /// <remarks>
        /// Excluded from code coverage because the string.Empty version will never be returned in a test situation.
        /// </remarks>
        private static string UnderTest => Debugger.IsAttached ? ";test" : "";

        /// <summary>
        /// The details section of the <c>User-Agent</c> header.
        /// </summary>
        public string UserAgentDetails
        {
            get => $"lang=Managed;os={OS.Name}/{OS.Version};arch={OS.Architecture};version={Platform.AssemblyVersion}{UnderTest}";
        }
    }
}



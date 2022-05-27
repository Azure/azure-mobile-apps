// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using UIKit;

namespace Microsoft.Datasync.Client.Platforms
{
    /// <summary>
    /// iOS-specific implementation of the <see cref="IPlatformInformation"/> interface.
    /// </summary>
    internal class PlatformInformation : IPlatformInformation
    {
        /// <summary>
        /// Information about the OS.
        /// </summary>
        public IOSInformation OS => new OSInformation
        {
            Architecture = nameof(PlatformID.MacOSX),
            Name = "iOS",
            Version = UIDevice.CurrentDevice.SystemVersion
        };

        /// <summary>
        /// Converts the IsEmulator into a string.
        /// </summary>
        /// <remarks>
        /// Excluded from code coverage because the string.Empty version will never be returned in a test situation.
        /// </remarks>
        private string Emulator => UIDevice.CurrentDevice.Model.Contains("simulator", StringComparison.InvariantCultureIgnoreCase) ? ";simulator" : "";

        /// <summary>
        /// The details section of the <c>User-Agent</c> header.
        /// </summary>
        public string UserAgentDetails
        {
            get => $"lang=dotnet6;os={OS.Name}/{OS.Version};arch={OS.Architecture};version={Platform.AssemblyVersion}{Emulator}";
        }
    }
}


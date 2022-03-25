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
        /// The details section of the <c>User-Agent</c> header.
        /// </summary>
        public string UserAgentDetails
        {
            get => $"lang=Managed;os={OS.Name}/{OS.Version};arch={OS.Architecture};version={Platform.AssemblyVersion}";
        }
    }
}



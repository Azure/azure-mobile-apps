// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Runtime.InteropServices;

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
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            Name = "Windows",
            Version = Environment.OSVersion.VersionString
        };

        /// <summary>
        /// The details section of the <c>User-Agent</c> header.
        /// </summary>
        public string UserAgentDetails
        {
            get => $"lang=dotnet6;os={OS.Name}/{OS.Version};arch={OS.Architecture};version={Platform.AssemblyVersion}";
        }
    }
}



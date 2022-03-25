// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// A structure for recording the OS information.
    /// </summary>
    public struct OSInformation : IOSInformation
    {
        /// <summary>
        /// The name of the operating system.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The version of the operating system.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The architecture of the operating system
        /// </summary>
        public string Architecture { get; set; }
    }
}
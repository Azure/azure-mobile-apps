// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// The system properties that we can expect on an instance.
    /// </summary>
    internal static class SystemProperties
    {
        /// <summary>
        /// The name of the "id" property in a JSON document.
        /// </summary>
        internal const string JsonIdProperty = "id";

        /// <summary>
        /// The name of the "version" property in a JSON document.
        /// </summary>
        internal const string JsonVersionProperty = "version";

        /// <summary>
        /// The name of the "updatedAt" property in a JSON document.
        /// </summary>
        internal const string JsonUpdatedAtProperty = "updatedAt";
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Attributes
{
    /// <summary>
    /// An interface for attributes applied to recognize system properties.
    /// </summary>
    internal interface ISystemPropertyAttribute
    {
        /// <summary>
        /// The name of the system property.
        /// </summary>
        string PropertyName { get; }
    }
}

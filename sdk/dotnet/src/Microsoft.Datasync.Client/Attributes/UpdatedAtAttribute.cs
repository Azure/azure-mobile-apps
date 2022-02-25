// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Attributes;
using Microsoft.Datasync.Client.Serialization;
using System;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Attribute applied to a member of a type to specify that it represents
    /// the version system property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UpdatedAtAttribute : Attribute, ISystemPropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the VersionAttribute class.
        /// </summary>
        public UpdatedAtAttribute()
        {
        }

        /// <summary>
        /// Gets the system property the attribute represents.
        /// </summary>
        public string PropertyName { get; } = SystemProperties.JsonUpdatedAtProperty;
    }
}
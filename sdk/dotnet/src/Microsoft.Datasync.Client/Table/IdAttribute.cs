// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// An attribute that identifies the ID field within a DTO.
    /// </summary>
    /// <remarks>
    /// Excluding from code coverage because there is no code in this attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    [ExcludeFromCodeCoverage]
    public class IdAttribute : Attribute
    {
        /// <summary>
        /// Creates a new <see cref="IdAttribute"/>.
        /// </summary>
        public IdAttribute()
        {
        }
    }
}

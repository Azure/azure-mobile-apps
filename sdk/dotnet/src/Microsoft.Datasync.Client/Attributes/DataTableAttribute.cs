// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Attribute applied to a type to specify the Datasync table it represents.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataTableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the DataTableAttribute class.
        /// </summary>
        /// <param name="name">The name of the table the class represents.</param>
        public DataTableAttribute(string name)
        {
            Arguments.IsValidTableName(name, nameof(name));
            Name = name;
        }

        /// <summary>
        /// Gets the name of the table the class represents.
        /// </summary>
        public string Name { get; }
    }
}

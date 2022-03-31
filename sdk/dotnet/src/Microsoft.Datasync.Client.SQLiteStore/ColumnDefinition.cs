// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Datasync.Client.SQLiteStore
{
    /// <summary>
    /// A class that represents the type of column on local store
    /// </summary>
    public class ColumnDefinition : IEquatable<ColumnDefinition>
    {
        /// <summary>
        /// Creates a new column definition.
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="jsonType">The column type for JSON modeling</param>
        /// <param name="storeType">The stored type</param>
        public ColumnDefinition(string name, JTokenType jsonType, string storeType)
        {
            Name = name;
            JsonType = jsonType;
            StoreType = storeType;
        }

        /// <summary>
        /// The name of the column
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the column
        /// </summary>
        public JTokenType JsonType { get; }

        /// <summary>
        /// The stored type.
        /// </summary>
        public string StoreType { get; }

        #region IEquatable<ColumnDefinition>
        /// <summary>
        /// Provides a default implementation of the equality function.
        /// </summary>
        /// <param name="other">The comparison object</param>
        /// <returns><c>true</c> if equal and <c>false</c> otherwise.</returns>
        public bool Equals(ColumnDefinition other)
            => Name.Equals(other.Name) && JsonType.Equals(other.JsonType) && StoreType.Equals(other.StoreType);
        #endregion

        #region IEquatable
        /// <summary>
        /// Provides a default implementation of the hash function.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode() => Tuple.Create(Name, JsonType, StoreType).GetHashCode();

        /// <summary>
        /// Provides a default implementation of the equality function.
        /// </summary>
        /// <param name="obj">The comparison object</param>
        /// <returns>True if equal</returns>
        public override bool Equals(object obj) => obj is ColumnDefinition other && Equals(other);
        #endregion

        /// <summary>
        /// Provides a default implementation of the string conversion.
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString() => $"ColumnDefinition({Name},{JsonType},{StoreType})";
    }
}

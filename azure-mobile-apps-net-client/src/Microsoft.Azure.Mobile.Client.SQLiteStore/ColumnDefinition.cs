// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    /// <summary>
    /// A class that represents the type of column on local store
    /// </summary>
    public class ColumnDefinition
    {
        /// <summary>
        /// The name of the column
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The type of the column
        /// </summary>
        public JTokenType JsonType { get; private set; }

        /// <summary>
        /// The stored type.
        /// </summary>
        public string StoreType { get; private set; }

        /// <summary>
        /// Creates a new column definition.
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="jsonType">The JSON type</param>
        /// <param name="storeType">The stored type</param>
        public ColumnDefinition(string name, JTokenType jsonType, string storeType)
        {
            this.Name = name;
            this.JsonType = jsonType;
            this.StoreType = storeType;
        }

        /// <summary>
        /// Provides a default implementation of the hash function.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return Tuple.Create(this.Name, this.JsonType, this.StoreType).GetHashCode();
        }

        /// <summary>
        /// Provides a default implementation of the equality function.
        /// </summary>
        /// <param name="obj">The comparison object</param>
        /// <returns>True if equal</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ColumnDefinition other))
            {
                return base.Equals(obj);
            }

            return this.Name.Equals(other.Name) &&
                   this.JsonType.Equals(other.JsonType) &&
                   this.StoreType.Equals(other.StoreType);
        }

        /// <summary>
        /// Provides a default implementation of the string conversion.
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            return String.Format("{0}, {1}, {1}", this.Name, this.JsonType, this.StoreType);
        }
    }
}

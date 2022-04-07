// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Datasync.Client.SQLiteStore
{
    /// <summary>
    /// A class that represents the type of column on local store
    /// </summary>
    public class ColumnDefinition : IEquatable<ColumnDefinition>
    {
        private static readonly DateTime epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
        /// <c>true</c> if this is the ID column.
        /// </summary>
        public bool IsIdColumn { get => Name.Equals(SystemProperties.JsonIdProperty); }

        /// <summary>
        /// The stored type.
        /// </summary>
        public string StoreType { get; }

        /// <summary>
        /// Deserializes a value of this column type back to the JSON form.
        /// </summary>
        /// <param name="value">The value to deserialize.</param>
        /// <returns>The <see cref="JToken"/> for the value.</returns>
        public JToken DeserializeValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (SqlColumnType.IsTextType(StoreType))
            {
                string strValue = value as string;
                return JsonType switch
                {
                    JTokenType.Guid => Guid.Parse(strValue),
                    JTokenType.Bytes => Convert.FromBase64String(strValue),
                    JTokenType.TimeSpan => TimeSpan.Parse(strValue),
                    JTokenType.Uri => new Uri(strValue, UriKind.RelativeOrAbsolute),
                    JTokenType.Array => JToken.Parse(strValue),
                    JTokenType.Object => JToken.Parse(strValue),
                    _ => strValue
                };
            }

            if (SqlColumnType.IsFloatType(StoreType))
            {
                return Convert.ToDouble(value);
            }

            if (SqlColumnType.IsNumberType(StoreType))
            {
                long longValue = Convert.ToInt64(value);
                return JsonType switch
                {
                    JTokenType.Date => epoch.AddMilliseconds(Convert.ToDouble(value)),
                    JTokenType.Boolean => longValue == 1,
                    _ => longValue
                };
            }

            return null;
        }

        /// <summary>
        /// Serializes a value for storage into SQLite.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The serialized value.</returns>
        public object SerializeValue(JToken value)
            => SqlColumnType.SerializeValue(value, StoreType, JsonType);

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

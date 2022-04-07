// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.SQLiteStore
{
    /// <summary>
    /// The list of the SQL column types.
    /// </summary>
    internal static class SqlColumnType
    {
        private static readonly DateTime epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public const string Integer = "INTEGER";
        public const string Text = "TEXT";
        public const string None = "NONE";
        public const string Real = "REAL";
        public const string Numeric = "NUMERIC";
        public const string Boolean = "BOOLEAN";
        public const string SqlDateTime = "DATETIME";
        public const string Float = "FLOAT";
        public const string Blob = "BLOB";
        public const string Guid = "GUID";
        public const string Json = "JSON";
        public const string Uri = "URI";
        public const string TimeSpan = "TIMESPAN";

        /// <summary>
        /// A mapping from the <see cref="JTokenType"/> to the equivalent SQL column type for
        /// supported mappings.
        /// </summary>
        private static readonly Dictionary<JTokenType, string> columnMapping = new()
        {
            { JTokenType.Boolean, Boolean },
            { JTokenType.Integer, Integer },
            { JTokenType.Date, SqlDateTime },
            { JTokenType.Float, Float },
            { JTokenType.String, Text },
            { JTokenType.Guid, Guid },
            { JTokenType.Array, Json },
            { JTokenType.Object, Json },
            { JTokenType.Bytes, Blob },
            { JTokenType.Uri, Uri },
            { JTokenType.TimeSpan, TimeSpan }
        };

        /// <summary>
        /// Converts the incoming <see cref="JTokenType"/> type into the equivalent SQL column type.
        /// </summary>
        /// <param name="type">The incoming <see cref="JTokenType"/> type.</param>
        /// <param name="allowNull">If <c>true</c>, supports <see cref="JTokenType.Null"/>.</param>
        /// <returns>The store type.</returns>
        /// <exception cref="NotSupportedException">If the provided type is not supported by this store.</exception>
        public static string Get(JTokenType type, bool allowNull)
        {
            if (columnMapping.ContainsKey(type))
            {
                return columnMapping[type];
            }
            else if (type == JTokenType.Null && allowNull)
            {
                return null;
            }
            else
            {
                throw new NotSupportedException($"Property of type '{type}' is not supported.");
            }
        }

        /// <summary>
        /// Gets the store type from a .NET type.
        /// </summary>
        /// <param name="type">The .NET type.</param>
        /// <returns>The store type</returns>
        /// <exception cref="NotSupportedException">If the .NET type cannot be supported.</exception>
        public static string GetStoreCastType(Type type)
        {
            if (type == typeof(bool) || type == typeof(DateTime) || type == typeof(decimal))
            {
                return Numeric;
            }
            else if (type == typeof(int) || type == typeof(uint) ||
                     type == typeof(long) || type == typeof(ulong) ||
                     type == typeof(short) || type == typeof(ushort) ||
                     type == typeof(byte) || type == typeof(sbyte))
            {
                return Integer;
            }
            else if (type == typeof(float) || type == typeof(double))
            {
                return Real;
            }
            else if (type == typeof(string) || type == typeof(Guid) || type == typeof(byte[]) || type == typeof(Uri) || type == typeof(TimeSpan))
            {
                return Text;
            }

            throw new NotSupportedException($"Type '{type.FullName}' not supported in SQLite.");
        }

        /// <summary>
        /// Returns true if the provided type is stored as an integer.
        /// </summary>
        /// <param name="type">The <see cref="SqlColumnType"/> to check.</param>
        /// <returns><c>true</c> if stored as an integer, <c>false</c> otherwise.</returns>
        public static bool IsNumberType(string type)
            => type == Integer || type == Numeric || type == Boolean || type == SqlDateTime;

        /// <summary>
        /// Returns true if the provided type is stored as a floating point number.
        /// </summary>
        /// <param name="type">The <see cref="SqlColumnType"/> to check.</param>
        /// <returns><c>true</c> if stored as a floating point number, <c>false</c> otherwise.</returns>
        public static bool IsFloatType(string type)
            => type == Real || type == Float;

        /// <summary>
        /// Returns true if the provided type is stored as a text object.
        /// </summary>
        /// <param name="type">The <see cref="SqlColumnType"/> to check.</param>
        /// <returns><c>true</c> if stored as a text object, <c>false</c> otherwise.</returns>
        public static bool IsTextType(string type)
            => type == Text || type == Blob || type == Guid || type == Json || type == Uri || type == TimeSpan;

        /// <summary>
        /// Serializes a value for storage into SQLite.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="allowNull">If <c>true</c>, allow null values.</param>
        /// <returns>The serialized value.</returns>
        public static object SerializeValue(JValue value, bool allowNull)
        {
            string storeType = Get(value.Type, allowNull);
            return SerializeValue(value, storeType, value.Type);
        }

        /// <summary>
        /// Serializes a value for storage into SQLite.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="storeType">The SQLite store type.</param>
        /// <param name="columnType">The JSON token type.</param>
        /// <returns></returns>
        public static object SerializeValue(JToken value, string storeType, JTokenType columnType)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                return null;
            }

            if (IsTextType(storeType))
            {
                if (columnType == JTokenType.Bytes && value.Type == JTokenType.Bytes)
                {
                    return Convert.ToBase64String(value.Value<byte[]>());
                }
                return value.ToString();
            }

            if (IsFloatType(storeType))
            {
                return value.Value<double>();
            }

            if (IsNumberType(storeType))
            {
                if (columnType == JTokenType.Date)
                {
                    var date = value.ToObject<DateTime>();
                    if (date.Kind == DateTimeKind.Unspecified)
                    {
                        date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                    }
                    return Math.Round((date.ToUniversalTime() - epoch).TotalMilliseconds, 3);
                }
                return value.Value<long>();
            }

            return value.ToString();
        }
    }
}

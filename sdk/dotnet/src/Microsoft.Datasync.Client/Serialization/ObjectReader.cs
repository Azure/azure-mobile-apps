// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Reflection;
using System.Text.Json.Serialization;

#nullable enable

namespace Microsoft.Datasync.Client.Serialization
{
    public static class ObjectReader
    {
        private const string idPropertyName = "Id";
        private const string versionPropertyName = "Version";
        private const string updatedAtPropertyName = "UpdatedAt";

        public static void GetSystemProperties<T>(T item, out DatasyncClientData systemProperties)
        {
            Validate.IsNotNull(item, nameof(item));
            systemProperties = new SystemProperties();

            // We look for things in reverse order, so the most significant type is last in the list.
            // Least Priority: Specific names of the properties.
            foreach (var property in item!.GetType().GetProperties())
            {
                if (property.IsMatchingProperty<string>(idPropertyName))
                {
                    systemProperties.Id = (string)property.GetValue(item);
                }

                if (property.IsMatchingProperty<string>(versionPropertyName))
                {
                    systemProperties.Version = (string)property.GetValue(item);
                }

                if (property.IsMatchingProperty<DateTimeOffset>(updatedAtPropertyName))
                {
                    systemProperties.UpdatedAt = (DateTimeOffset)property.GetValue(item);
                }
            }

            // Most Priority: JsonPropertyName with the correct value.
            foreach (var property in item!.GetType().GetProperties())
            {
                // Note: only inline the first out - the others use the same variable.
                if (property.HasJsonPropertyName<string>(out string name) && name.Equals(idPropertyName.CamelCase()))
                {
                    systemProperties.Id = (string)property.GetValue(item);
                }

                if (property.HasJsonPropertyName<string>(out name) && name.Equals(versionPropertyName.CamelCase()))
                {
                    systemProperties.Version = (string)property.GetValue(item);
                }

                if (property.HasJsonPropertyName<string>(out name) && name.Equals(updatedAtPropertyName.CamelCase()))
                {
                    systemProperties.UpdatedAt = (DateTimeOffset)property.GetValue(item);
                }
            }

            // systemProperties is now set up properly and we can return.
        }

        /// <summary>
        /// Returns the camel-cased version of a string.
        /// </summary>
        /// <param name="value">The value to camel-case</param>
        /// <returns>The camel-cased version</returns>
        internal static string CamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return char.ToLower(value[0]) + value.Substring(1);
        }

        /// <summary>
        /// Determine if the property has a <see cref="JsonPropertyNameAttribute"/>; if it does, return
        /// true and set the name to the value of the attribute.
        /// </summary>
        /// <typeparam name="T">The expected type of the property.</typeparam>
        /// <param name="property">The property</param>
        /// <param name="name">On return, the value of the <see cref="JsonPropertyNameAttribute"/>.</param>
        /// <returns><c>true</c> if the <see cref="JsonPropertyNameAttribute"/> is set and the type is right.</returns>
        internal static bool HasJsonPropertyName<T>(this PropertyInfo property, out string name)
        {
            name = string.Empty;

            if (property.PropertyType != typeof(T) && property.PropertyType != typeof(T?))
            {
                return false;
            }
            var attr = property.GetCustomAttribute<JsonPropertyNameAttribute>(true);
            if (attr != null)
            {
                name = attr.Name;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if the property provided is a matching property for the given name and type.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="property">The property to check.</param>
        /// <param name="propertyName">The expected name.</param>
        /// <returns>True if the name and type match.</returns>
        internal static bool IsMatchingProperty<T>(this PropertyInfo property, string propertyName)
            => property.Name.Equals(propertyName) && (property.PropertyType == typeof(T) || property.PropertyType == typeof(T?));

        /// <summary>
        /// An internal copy of the DatasyncClientData that only has the system properties in it.
        /// </summary>
        internal class SystemProperties : DatasyncClientData
        {
        }
    }
}

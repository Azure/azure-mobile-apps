// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// Provides serialization and deserialization for a <see cref="DatasyncClient"/>.
    /// </summary>
    internal class ServiceSerializer
    {
        /// <summary>
        /// The JSON serializer settings to use with this serializer.
        /// </summary>
        public DatasyncSerializerSettings SerializerSettings { get; set; } = new();

        /// <summary>
        /// Gets the value of the ID property from a <see cref="JObject"/> instance, ensuring
        /// that the value is valid.
        /// </summary>
        /// <param name="instance">The subject instance.</param>
        /// <param name="ignoreCase">If <c>true</c>, find any variant spelling of the ID property.</param>
        /// <param name="allowDefault">If <c>true</c>, the default value for the ID is valid.</param>
        /// <returns>The ID property value.</returns>
        /// <exception cref="ArgumentException">If the value is invalid.</exception>
        public static string GetId(JObject instance, bool ignoreCase = false, bool allowDefault = false)
        {
            Arguments.IsNotNull(instance, nameof(instance));

            bool hasId = TryGetId(instance, ignoreCase, out string id);
            if (!hasId && !ignoreCase && TryGetId(instance, true, out id))
            {
                throw new ArgumentException($"The casing of the '{SystemProperties.JsonIdProperty}' property is invalid.", nameof(instance));
            }
            if (hasId)
            {
                Arguments.IsValidId(id, nameof(instance));
            }
            else if (!allowDefault)
            {
                throw new ArgumentException($"Required '{SystemProperties.JsonIdProperty}' property is not found.", nameof(instance));
            }
            return id;
        }

        /// <summary>
        /// Returns the version of the instance.  If one is not present, returns null.
        /// </summary>
        /// <param name="instance">The subject instance.</param>
        /// <returns>The version of the instance, or <c>null</c>.</returns>
        public static string GetVersionOrDefault(JObject instance)
        {
            foreach (JProperty property in instance.Properties())
            {
                if (string.Equals(property.Name, SystemProperties.JsonVersionProperty, StringComparison.OrdinalIgnoreCase))
                {
                    return instance.Value<string>(property.Name);
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to get the value of the ID property from an instance.
        /// </summary>
        /// <param name="instance">The subject instance.</param>
        /// <param name="ignoreCase">If <c>true</c>, find any variant spelling of the ID property.</param>
        /// <param name="id">On return, the value of the ID property or <c>null</c>.</param>
        /// <returns><c>true</c> if the property has an ID property, <c>false</c> otherwise.</returns>
        public static bool TryGetId(JObject instance, bool ignoreCase, out string id)
        {
            StringComparison comparator = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            bool hasId = instance.TryGetValue(SystemProperties.JsonIdProperty, comparator, out JToken idToken);
            id = (hasId && idToken is JValue idValue && idValue.Type == JTokenType.String) ? idValue.Value<string>() : null;
            return hasId && id != null;
        }
    }
}

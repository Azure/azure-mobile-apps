// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// A set of methods useful for handling objects used as data transfer objects (or models)
    /// </summary>
    internal static class ObjectReader
    {
        /// <summary>
        /// Gets the system properties for a given instance.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="instance"/> is a <see cref="JObject"/>, then the only
        /// acceptable values for the system properties are the serialized properties.
        ///
        /// If the <paramref name="instance"/> is any other type, then we use the serialization
        /// first, then the plain name of the property.  This allows us to detect all sorts
        /// of configurations.
        /// </remarks>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="systemProperties">When complete, the system properties.</param>
        internal static void GetSystemProperties<T>(T instance, out SystemProperties systemProperties)
        {
            systemProperties = new();

            if (instance is JObject json)
            {
                systemProperties.Id = GetJsonPropertyValue<string>(json, JTokenType.String, SystemProperties.JsonIdProperty);
                systemProperties.Version = GetJsonPropertyValue<string>(json, JTokenType.String, SystemProperties.JsonVersionProperty);
                var jsondate = GetJsonPropertyValue<string>(json, JTokenType.String, SystemProperties.JsonUpdatedAtProperty);
                if (!string.IsNullOrWhiteSpace(jsondate))
                {
                    systemProperties.UpdatedAt = DateTimeOffset.Parse(jsondate);
                }
                return;
            }

            // For normal models, the properties have a convoluted form:
            //  1) Check to see if the [JsonProperty(Name = "xxx")] is included - if so, this is property
            //  2) Check to see if a property with the name of the upper cased JsonProperty is included - if so, this is the property.
            //  3) The property must be the right type:
            //      ID must be a string.
            //      VERSION can be a string or byte array.
            //      UPDATEDAT can be a string that parses as a date or a DateTimeOffset, or a Nullable<DateTimeOffset>
            PropertyInfo idProperty = GetProperty(instance, SystemProperties.JsonIdProperty);
            if (idProperty != null)
            {
                if (idProperty.PropertyType == typeof(string))
                {
                    systemProperties.Id = (string)idProperty.GetValue(instance);
                }
                else
                {
                    throw new InvalidOperationException($"Id property '{idProperty.Name}' must be a string.");
                }
            }

            PropertyInfo versionProperty = GetProperty(instance, SystemProperties.JsonVersionProperty);
            if (versionProperty != null)
            {
                if (versionProperty.PropertyType == typeof(string))
                {
                    systemProperties.Version = (string)versionProperty.GetValue(instance);
                }
                else if (versionProperty.PropertyType == typeof(byte[]))
                {
                    systemProperties.Version = Encoding.UTF8.GetString((byte[])versionProperty.GetValue(instance));
                }
                else
                {
                    throw new InvalidOperationException($"Version property '{versionProperty.Name}' must be a string or byte[].");
                }
            }

            PropertyInfo updatedAtProperty = GetProperty(instance, SystemProperties.JsonUpdatedAtProperty);
            if (updatedAtProperty != null)
            {
                if (updatedAtProperty.PropertyType == typeof(DateTimeOffset))
                {
                    systemProperties.UpdatedAt = (DateTimeOffset)updatedAtProperty.GetValue(instance);
                }
                else if (updatedAtProperty.PropertyType == typeof(DateTimeOffset?))
                {
                    systemProperties.UpdatedAt = (DateTimeOffset?)updatedAtProperty.GetValue(instance);
                }
                else if (updatedAtProperty.PropertyType == typeof(string))
                {
                    systemProperties.UpdatedAt = DateTimeOffset.Parse((string)updatedAtProperty.GetValue(instance));
                }
                else
                {
                    throw new InvalidOperationException($"UpdatedAt property '{versionProperty.Name}' must be a string or DateTimeOffset.");
                }
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of a system property.  If the type does not match the required type, then
        /// throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <typeparam name="T">The type of the return value.</typeparam>
        /// <param name="json">The JSON object to parse.</param>
        /// <param name="tokenType">The expected token type of the property.</param>
        /// <param name="propName">The property name.</param>
        /// <returns>The value of the property.</returns>
        private static T GetJsonPropertyValue<T>(JObject json, JTokenType tokenType, string propName)
        {
            Arguments.IsNotNull(json, nameof(json));
            Arguments.IsNotNullOrWhitespace(propName, nameof(propName));

            if (!json.ContainsKey(propName))
            {
                return default;
            }
            if (json[propName].Type != tokenType)
            {
                throw new InvalidOperationException($"Property '{propName}' is not the correct JSON type.");
            }
            return json.Value<T>(propName);
        }

        /// <summary>
        /// Returns the <see cref="PropertyInfo"/> for the provided property name, according to the rules
        /// that govern the property naming.
        /// </summary>
        /// <param name="instance">The instance to process.</param>
        /// <param name="propName">The name of the JSON property.</param>
        /// <returns>The property information, or <c>null</c> if the property cannot be found.</returns>
        private static PropertyInfo GetProperty(object instance, string propName)
        {
            var props = instance.GetType().GetProperties();
            var jsonprop = Array.Find(props, prop => HasJsonPropertyAttribute(prop, propName));
            if (jsonprop != null)
            {
                return jsonprop;
            }

            var capPropName = char.ToUpper(propName[0]) + propName.Substring(1);
            return Array.Find(props, prop => prop.Name == capPropName);
        }

        /// <summary>
        /// Returns <c>true</c> the property has a <see cref="JsonPropertyAttribute"/> with a matching property name.
        /// </summary>
        /// <param name="prop">The <see cref="PropertyInfo"/> to use.</param>
        /// <param name="propName">The property name.</param>
        /// <returns><c>true</c> if the property has a <see cref="JsonPropertyAttribute"/> with the current property name; <c>false</c> otherwise.</returns>
        private static bool HasJsonPropertyAttribute(PropertyInfo prop, string propName)
        {
            var attr = prop.GetCustomAttribute<JsonPropertyAttribute>(true);
            return attr != null && attr.PropertyName == propName;
        }
    }
}

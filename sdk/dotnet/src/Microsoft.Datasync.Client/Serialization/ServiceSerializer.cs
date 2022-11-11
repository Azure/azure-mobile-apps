// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// Provides serialization and deserialization for a <see cref="DatasyncClient"/>.
    /// </summary>
    public class ServiceSerializer
    {
        /// <summary>
        /// The JSON serializer settings to use with this serializer.
        /// </summary>
        public DatasyncSerializerSettings SerializerSettings { get; set; } = new();

        /// <summary>
        /// Deserializes a JSON string into an instance.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize into.</typeparam>
        /// <param name="json">The parsed JSON string.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> to use.</param>
        /// <returns>The deserialized content.</returns>
        public T Deserialize<T>(JToken json, JsonSerializer serializer = null)
        {
            Arguments.IsNotNull(json, nameof(json));
            return json.ToObject<T>(serializer ?? SerializerSettings.GetSerializerFromSettings());
        }

        /// <summary>
        /// Deserializes a JSON string into an instance.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize into.</typeparam>
        /// <param name="json">The parsed JSON string.</param>
        /// <param name="instance">The instance to populate.</param>
        public void Deserialize<T>(JToken json, T instance)
        {
            Arguments.IsNotNull(json, nameof(json));
            Arguments.IsNotNull(instance, nameof(instance));
            JsonConvert.PopulateObject(json.ToString(), instance, SerializerSettings);
        }

        /// <summary>
        /// Given a JSON string, return it as a <see cref="JObject"/> or return <c>null</c> if not valid.
        /// </summary>
        /// <param name="json">The json string to parse.</param>
        /// <returns>The <see cref="JObject"/> value, or <c>null</c></returns>
        public JObject DeserializeObjectOrDefault(string json)
        {
            var item = string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<JToken>(json, SerializerSettings);
            return item is JObject obj && obj.Value<string>(SystemProperties.JsonIdProperty) != null ? (JObject)item : null;
        }

        /// <summary>
        /// Given a type, ensure that the type has an Id property and that it is a string.
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        public static void EnsureIdIsString<T>()
            => EnsureIdIsString(typeof(T));

        /// <summary>
        /// Given a type, ensure that the type has an Id property and that it is a string.
        /// </summary>
        /// <param name="type">The type to check.</param>
        public static void EnsureIdIsString(Type type)
        {
            // Special case - if the Type is a generic Dictionary or a JObject, then we need to basically say
            // "customer knows what they are doing" because they are constructing a JsonObject with the right
            // fields on each call.  We can't prevent this, so skip over the check and check it in operation.
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType.Name == "Dictionary`2")
                {
                    return;
                }
            }

            var idProperty = type.GetProperties().Where(prop => prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (idProperty == null)
            {
                throw new ArgumentException($"Type {type.Name} must contain an Id property", nameof(type));
            }
            if (idProperty.PropertyType != typeof(string))
            {
                throw new ArgumentException($"Property {type.Name}.Id must be a string", nameof(type));
            }
            // We should be OK here!
        }

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
        /// Gets the value of the ID property from a strongly typed object, ensuring
        /// that the value is valid.
        /// </summary>
        /// <param name="instance">The subject instance.</param>
        /// <param name="allowDefault">If <c>true</c>, the default value for the ID is valid.</param>
        /// <returns>The ID property value.</returns>
        /// <exception cref="ArgumentException">If the value is invalid.</exception>
        public string GetId<T>(T instance, bool allowDefault = false)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            JsonProperty idProperty = SerializerSettings.ContractResolver.ResolveIdProperty(instance.GetType());
            string id = idProperty.ValueProvider.GetValue(instance) as string;
            if (id != null)
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
        /// Gets the <c>updatedAt</c> field as a parsed DateTimeOffset, returns
        /// null if the <c>updatedAt</c> field is missing.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static DateTimeOffset? GetUpdatedAt(JObject item)
        {
            string updatedAt = item.Value<string>(SystemProperties.JsonUpdatedAtProperty);
            if (updatedAt == null)
            {
                return null;
            }
            // Throw an error if the service returned a bad date format.
            return DateTimeOffset.Parse(updatedAt, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the version of the instance.  If one is not present, returns null.
        /// </summary>
        /// <param name="instance">The subject instance.</param>
        /// <returns>The version of the instance, or <c>null</c>.</returns>
        public static string GetVersion(JObject instance)
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
        /// Returns true if the instance is deleted, and false otherwise.
        /// </summary>
        /// <param name="instance">The instance to check.</param>
        /// <returns><c>true</c> if the instance is marked as deleted.</returns>
        public static bool IsDeleted(JObject instance)
            => instance.Value<bool>(SystemProperties.JsonDeletedProperty);

        /// <summary>
        /// Removes all system properties from the instance.
        /// </summary>
        /// <param name="instance">The subject instance.</param>
        /// <param name="version">When complete, set to the version system property before it is removed.</param>
        /// <returns>The subject instance with the system properties removed.</returns>
        public static JObject RemoveSystemProperties(JObject instance, out string version)
        {
            version = null;
            bool haveCloned = false;
            foreach (JProperty prop in instance.Properties())
            {
                if (SystemProperties.AllSystemProperties.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                {
                    if (!haveCloned)
                    {
                        instance = instance.DeepClone() as JObject;
                        haveCloned = true;
                    }

                    if (string.Equals(prop.Name, SystemProperties.JsonVersionProperty, StringComparison.OrdinalIgnoreCase))
                    {
                        version = (string)instance[prop.Name];
                    }
                    instance.Remove(prop.Name);
                }
            }
            return instance;
        }

        /// <summary>
        /// Returns a table name for a type, accounting for table renaming via the <see cref="DataTableAttribute"/> and the <see cref="JsonContainerAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The type for which to return the table name.</typeparam>
        /// <returns>The table name.</returns>
        public string ResolveTableName<T>()
            => SerializerSettings.ContractResolver.ResolveTableName(typeof(T));

        /// <summary>
        /// Serializes an instance into a JSON string.
        /// </summary>
        /// <param name="instance">The instance to serialize.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> to use.</param>
        /// <returns>The serialized instance.</returns>
        public JToken Serialize(object instance, JsonSerializer serializer = null)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            return JToken.FromObject(instance, serializer ?? SerializerSettings.GetSerializerFromSettings());
        }

        /// <summary>
        /// Sets the instance's ID property value to the default ID value.
        /// </summary>
        /// <param name="instance">The instance on which to clear the ID property.</param>
        public void SetIdToDefault(object instance)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            if (instance is JObject json)
            {
                json[SystemProperties.JsonIdProperty] = null;
            }
            else
            {
                JsonProperty idProperty = SerializerSettings.ContractResolver.ResolveIdProperty(instance.GetType());
                idProperty.ValueProvider.SetValue(instance, null);
            }
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

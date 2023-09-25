// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Attributes;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// An <see cref="IContractResolver"/> implementation that is used with
    /// a <see cref="DatasyncClient"/>.
    /// </summary>
    public class DatasyncContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// A cache of the ID <see cref="JsonProperty"/> for each model type.
        /// </summary>
        private readonly Dictionary<Type, JsonProperty> idPropertyCache = new();

        /// <summary>
        /// A cache for the <see cref="JsonProperty"/> values created by <see cref="CreateProperty(MemberInfo, MemberSerialization)"/>.
        /// </summary>
        private readonly Dictionary<MemberInfo, JsonProperty> jsonPropertyCache = new();

        /// <summary>
        /// a cache for the translation of model types to the table name.
        /// </summary>
        private readonly Dictionary<Type, string> tableNameCache = new();

        /// <summary>
        /// A lock to synchronize concurrent access to <see cref="jsonPropertyCache"/>.
        /// </summary>
        private readonly ReaderWriterLockSlim jsonPropertyCacheLock = new();

        /// <summary>
        /// Locks for the <see cref="CreateProperties(Type, MemberSerialization)"/> method.
        /// </summary>
        private static readonly Dictionary<Type, object> createPropertiesForTypeLocks = new();

        /// <summary>
        /// Indicates if the property names should be camel-cased when serialized
        /// out of JSON.
        /// </summary>
        internal bool CamelCasePropertyNames { get; set; } = true;

        /// <summary>
        /// Creates the <see cref="IValueProvider"/> used by the serializer to get and set values from a member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The <see cref="IValueProvider"/> used by the serializer to get and set values from a member.</returns>
        protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
            => new ReflectionValueProvider(member);

        /// <summary>
        /// Creates a <see cref="JsonProperty"/> for a given <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <remarks>
        /// This method is overridden to set specialized <see cref="IValueProvider"/> implementations for
        /// certain property types - most specifically, the date/time formatters.
        /// </remarks>
        /// <param name="member">The <see cref="MemberInfo"/> to create the <see cref="JsonProperty"/>.</param>
        /// <param name="memberSerialization">Member serialization options for the member.</param>
        /// <returns>A <see cref="JsonProperty"/> for the <paramref name="member"/>.</returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            try
            {
                jsonPropertyCacheLock.EnterWriteLock();
                jsonPropertyCache[member] = property;
            }
            finally
            {
                jsonPropertyCacheLock.ExitWriteLock();
            }
            return property;
        }

        /// <summary>
        /// Creates a collection of <see cref="JsonProperty"/> instances for the members of a given type.
        /// </summary>
        /// <remarks>
        /// This method checks for and applies system property attributes.
        /// </remarks>
        /// <param name="type">The type used to create the collection of <see cref="JsonProperty"/> instances.</param>
        /// <param name="memberSerialization">Member serialization options for the type.</param>
        /// <returns>A list of <see cref="JsonProperty"/> instances for the type.</returns>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            lock (createPropertiesForTypeLocks)
            {
                if (!createPropertiesForTypeLocks.ContainsKey(type))
                {
                    createPropertiesForTypeLocks.Add(type, new object());
                }
            }

            lock (createPropertiesForTypeLocks[type])
            {
                var properties = base.CreateProperties(type, memberSerialization);

                // If this type is for a known table, ensure that it has an Id.
                TypeInfo typeInfo = type.GetTypeInfo();
                if (tableNameCache.ContainsKey(type) || tableNameCache.Keys.Any(t => t.GetTypeInfo().IsAssignableFrom(typeInfo)))
                {
                    properties = properties.Where(prop => prop.Writable).ToList();
                    _ = GetIdProperty(type, properties);

                    // Set any needed converters and look for system property attributes.
                    var relevantProperties = FilterJsonPropertyCacheByType(typeInfo);
                    foreach (var property in properties)
                    {
                        var memberInfo = relevantProperties.Single(x => x.Value == property).Key;
                        SetMemberConverters(property);
                        ApplySystemPropertyAttributes(property, memberInfo);
                    }
                }

                return properties;
            }
        }

        /// <summary>
        /// Returns the ID <see cref="JsonProperty"/> for the given type.
        /// </summary>
        /// <param name="type">The subject type.</param>
        /// <param name="throwIfNotFound">If <c>true</c>, throw an exception if the ID property cannot be found.</param>
        /// <returns>The ID <see cref="JsonProperty"/>.</returns>
        /// <exception cref="InvalidOperationException">If the ID property is not found and <paramref name="throwIfNotFound"/> is set to <c>true</c>.</exception>
        public virtual JsonProperty ResolveIdProperty(Type type, bool throwIfNotFound = true)
        {
            if (!idPropertyCache.TryGetValue(type, out JsonProperty property))
            {
                ResolveContract(type);
                idPropertyCache.TryGetValue(type, out property);
            }

            if (property == null && throwIfNotFound)
            {
                throw new InvalidOperationException($"No '{SystemProperties.JsonIdProperty}' member found on type '{type.FullName}'.");
            }

            return property;
        }

        /// <summary>
        /// Returns the <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/> instance. The <see cref="JsonProperty"/>
        /// can be used to get information about how the <see cref="MemberInfo"/> should be serialized.
        /// </summary>
        /// <param name="member">The <see cref="MemberInfo"/> for which to get the <see cref="JsonProperty"/>.</param>
        /// <returns>The <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/> instance.</returns>
        public virtual JsonProperty ResolveProperty(MemberInfo member)
        {
            try
            {
                jsonPropertyCacheLock.EnterUpgradeableReadLock();
                if (!jsonPropertyCache.TryGetValue(member, out JsonProperty property))
                {
                    ResolveContract(member.DeclaringType);
                    jsonPropertyCache.TryGetValue(member, out property);
                }
                return property;
            }
            finally
            {
                jsonPropertyCacheLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Returns the serialized property name.
        /// </summary>
        /// <param name="propertyName">The property name to serialize.</param>
        /// <returns>The property name in JSON form.</returns>
        protected override string ResolvePropertyName(string propertyName)
        {
            if (CamelCasePropertyNames && !string.IsNullOrWhiteSpace(propertyName))
            {
                return propertyName.Length > 1 ? char.ToLower(propertyName[0]) + propertyName.Substring(1) : propertyName.ToLower();
            }
            return propertyName;
        }

        /// <summary>
        /// Returns a table name for a type, accounting for table renaming via the
        /// <see cref="DataTableAttribute"/> and the <see cref="JsonContainerAttribute"/>.
        /// </summary>
        /// <param name="type">The type for which to return the table name.</param>
        /// <returns>The table name.</returns>
        public string ResolveTableName(Type type)
        {
            Arguments.IsNotNull(type, nameof(type));

            string name = null;
            lock (tableNameCache)
            {
                if (!tableNameCache.TryGetValue(type, out name))
                {
                    // Default is the name of the type, but lower-case.
                    name = type.Name.ToLowerInvariant();

                    // Note that the attributes are considered here in reverse order, so that
                    // the highest-priority name is done last.
                    if (type.HasAttribute<JsonContainerAttribute>(out JsonContainerAttribute jsonattr))
                    {
                        if (!string.IsNullOrWhiteSpace(jsonattr.Id))
                        {
                            name = jsonattr.Id;
                        }
                        else if (!string.IsNullOrWhiteSpace(jsonattr.Title))
                        {
                            name = jsonattr.Title;
                        }
                    }

                    if (type.HasAttribute<DataTableAttribute>(out DataTableAttribute dtattr))
                    {
                        if (!string.IsNullOrWhiteSpace(dtattr.Name))
                        {
                            name = dtattr.Name;
                        }
                    }

                    tableNameCache[type] = name;
                    CreateContract(type);  // Build a JsonContract now to catch any contract errors early
                }
            }

            return name;
        }

        /// <summary>
        /// Applies the system property attribute to the property by renaming the property to the
        /// system property name.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="member">The <see cref="MemberInfo"/> that corresponds to the property.</param>
        private static void ApplySystemPropertyAttributes(JsonProperty property, MemberInfo member)
        {
            List<string> systemProperties = new();

            foreach (object attribute in member.GetCustomAttributes(true))
            {
                if (attribute is ISystemPropertyAttribute systemProperty)
                {
                    property.PropertyName = systemProperty.PropertyName;
                    if (systemProperties.Contains(property.PropertyName))
                    {
                        throw new InvalidOperationException("A type can only have one system property attribute for each system property.");
                    }
                    else
                    {
                        systemProperties.Add(property.PropertyName);
                    }
                }
            }
        }

        /// <summary>
        /// Acquire a list of all MemberInfos currently tracked by the contract resolver in a threadsafe manner.
        /// </summary>
        /// <returns>A collection of <see cref="MemberInfo"/> instances.</returns>
        private IList<KeyValuePair<MemberInfo, JsonProperty>> FilterJsonPropertyCacheByType(TypeInfo typeInfo)
        {
            try
            {
                jsonPropertyCacheLock.EnterReadLock();
                return jsonPropertyCache.Where(pair => pair.Key.DeclaringType.GetTypeInfo().IsAssignableFrom(typeInfo)).ToList();
            }
            finally
            {
                jsonPropertyCacheLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Determines which of the properties is the ID property for the type.
        /// </summary>
        /// <param name="type">The type to determine the ID property of.</param>
        /// <param name="properties">The property ame.</param>
        /// <returns></returns>
        private JsonProperty GetIdProperty(Type type, IEnumerable<JsonProperty> properties)
        {
            JsonProperty[] idProperties = properties.Where(p => p.PropertyName.Equals(SystemProperties.JsonIdProperty, StringComparison.OrdinalIgnoreCase) && !p.Ignored).ToArray();
            if (idProperties.Length > 1)
            {
                throw new InvalidOperationException($"Only one member of the type '{type.FullName}' may be an ID.");
            }
            if (idProperties.Length < 1)
            {
                throw new InvalidOperationException($"No '{SystemProperties.JsonIdProperty}' properties found on type '{type.FullName}'");
            }

            JsonProperty idProperty = idProperties[0];
            idProperty.PropertyName = SystemProperties.JsonIdProperty;
            idProperty.NullValueHandling = NullValueHandling.Ignore;
            idProperty.DefaultValueHandling = DefaultValueHandling.Ignore;
            lock (idPropertyCache)
            {
                idPropertyCache[type] = idProperty;
            }
            return idProperty;
        }

        /// <summary>
        /// Sets the member converters on the property if the property is a value type.
        /// </summary>
        /// <param name="property">The property to add the member converters to.</param>
        private static void SetMemberConverters(JsonProperty property)
        {
            if (property.PropertyType.GetTypeInfo().IsValueType)
            {
                // The NullHandlingConverter will ensure that nulls get treated as the default value for value types.
                property.Converter = property.Converter == null ? NullHandlingConverter.Instance : new NullHandlingConverter(property.Converter);
            }
        }
    }
}
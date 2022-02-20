// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Runtime.Serialization;

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

            throw new NotImplementedException();
        }
    }
}

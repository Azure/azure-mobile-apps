// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// A set of extensions to the standard library methods.
    /// </summary>
    internal static class StdLibExtensions
    {
        /// <summary>
        /// Determines if the type has an attribute on it.  If it does, then return
        /// the first attribute.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to check for.</typeparam>
        /// <param name="type">The subject type.</param>
        /// <param name="attr">On return, the first attribute that is set on the subject type.</param>
        /// <returns><c>true</c> if the subject type has an attribute of type <typeparamref name="T"/>, <c>false</c> otherwise.</returns>
        internal static bool HasAttribute<T>(this Type type, out T attr)
        {
            if (type.GetTypeInfo().GetCustomAttributes(typeof(T), true).FirstOrDefault() is T requiredAttr)
            {
                attr = requiredAttr;
                return true;
            }
            attr = default;
            return false;
        }

        /// <summary>
        /// Determines if the type Nullable{Enum}.
        /// </summary>
        /// <param name="type">The subject type.</param>
        /// <param name="enumPropertyType">On return, the first generic attribute which is contains the Enum type itself.</param>
        /// <returns><c>true</c> if the subject type enum of nullable, <c>false</c> otherwise.</returns>
        internal static bool IsNullableEnum(this Type type, out Type enumPropertyType)
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                type.GetGenericArguments()[0].IsEnum)
            {
                enumPropertyType = type.GetGenericArguments()[0];
                return true;
            }

            enumPropertyType = default;
            return false;
        }

        /// <summary>
        /// Normalize an endpoint by removing any query and fragment, then ensuring that the
        /// path has a trailing slash.
        /// </summary>
        /// <param name="endpoint">The endpoint to normalize.</param>
        /// <returns>The normalized endpoint.</returns>
        internal static Uri NormalizeEndpoint(this Uri endpoint)
        {
            Arguments.IsValidEndpoint(endpoint, nameof(endpoint));

            var builder = new UriBuilder(endpoint) { Query = string.Empty, Fragment = string.Empty };
            builder.Path = builder.Path.TrimEnd('/') + "/";
            return builder.Uri;
        }
    }
}

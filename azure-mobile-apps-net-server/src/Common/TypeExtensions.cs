// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.ComponentModel;

namespace System
{
    /// <summary>
    /// Utility class for manipulations of <see cref="Type"/> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class TypeExtensions
    {
        /// <summary>
        /// Gets a short name for a given <paramref name="type"/> excluding any special 
        /// characters such as '`' used in generic types.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to create a short name for.</param>
        /// <returns>The generated short name.</returns>
        public static string GetShortName(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type.IsGenericType || type.IsGenericTypeDefinition)
            {
                int length = type.Name.IndexOf('`');
                if (length > -1)
                {
                    return type.Name.Substring(0, length);
                }
            }

            return type.Name;
        }
    }
}

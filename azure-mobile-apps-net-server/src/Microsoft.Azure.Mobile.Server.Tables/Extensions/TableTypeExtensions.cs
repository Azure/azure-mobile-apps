// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace System
{
    /// <summary>
    /// Utility class for manipulations of <see cref="Type"/> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class TableTypeExtensions
    {
        public static Type GetEnumerableElementType(this Type type)
        {
            if (type == null)
            {
                return null;
            }

            Type genType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
            if (genType == typeof(Task<>))
            {
                type = type.GetGenericArguments()[0];
                genType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
            }

            if (genType == typeof(SingleResult<>))
            {
                return type.GetGenericArguments()[0];
            }

            // We special case IE<T> and IQ<T>
            if (type.IsInterface && (genType == typeof(IEnumerable<>) || genType == typeof(IQueryable<>)))
            {
                return GetInnerGenericType(type);
            }

            // For the rest of the interfaces
            foreach (Type interfaceType in type.GetInterfaces())
            {
                genType = interfaceType.IsGenericType ? interfaceType.GetGenericTypeDefinition() : null;
                if (genType == typeof(IEnumerable<>) || genType == typeof(IQueryable<>))
                {
                    // Special case the IEnumerable<T>
                    return GetInnerGenericType(interfaceType);
                }
            }

            return null;
        }

        private static Type GetInnerGenericType(Type interfaceType)
        {
            // Getting the type T definition if the returning type implements IEnumerable<T>
            Type[] parameterTypes = interfaceType.GetGenericArguments();

            if (parameterTypes.Length == 1 && interfaceType.FullName != null)
            {
                return parameterTypes[0];
            }

            return null;
        }
    }
}

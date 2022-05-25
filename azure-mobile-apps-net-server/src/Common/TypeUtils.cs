// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Azure.Mobile
{
    internal static class TypeUtils
    {
        /// <summary>
        /// Checks whether <paramref name="type"/> is of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to test against</typeparam>
        /// <param name="type">The type to test.</param>
        /// <returns><c>true</c> if the type is of type <typeparamref name="T"/>.</returns>
        public static bool IsType<T>(Type type)
        {
            return
                type != null &&
                type.IsClass &&
                type.IsVisible &&
                !type.IsAbstract &&
                typeof(T).IsAssignableFrom(type);
        }

        /// <summary>
        /// Finds types matching the <paramref name="predicate"/> in a given set of <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">The assemblies to look through.</param>
        /// <param name="predicate">The predicate to apply to the search.</param>
        /// <returns>An <see cref="ICollection{T}"/> of types found.</returns>
        public static ICollection<Type> GetTypes(IEnumerable<_Assembly> assemblies, Predicate<Type> predicate)
        {
            List<Type> result = new List<Type>();
            if (assemblies == null)
            {
                return result;
            }

            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            // Go through all assemblies and search for types matching a predicate
            foreach (_Assembly assembly in assemblies)
            {
                Type[] exportedTypes = null;
                if (assembly == null)
                {
                    continue;
                }

                Assembly asm = assembly as Assembly;
                if (asm != null && asm.IsDynamic)
                {
                    // can't call GetExportedTypes on a dynamic assembly
                    continue;
                }

                try
                {
                    exportedTypes = assembly.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    exportedTypes = ex.Types;
                }
                catch
                {
                    // We deliberately ignore all other exceptions. 
                    continue;
                }

                if (exportedTypes != null)
                {
                    result.AddRange(exportedTypes.Where(x => predicate(x)));
                }
            }

            return result;
        }

        public static ICollection<T> GetAssemblyAttributes<T>(IEnumerable<_Assembly> assemblies)
            where T : Attribute
        {
            List<T> result = new List<T>();
            foreach (_Assembly assembly in assemblies)
            {
                object[] attributes = assembly.GetCustomAttributes(typeof(T), true);
                if (attributes != null)
                {
                    result.AddRange(attributes.Cast<T>());
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a Linq expression for invoking a method with a given <typeparamref name="TReturn"/> return type but 
        /// no parameters on a given type.
        /// </summary>
        /// <param name="instanceType">The instance type containing the method to invoke.</param>
        /// <param name="method">The <see cref="MethodInfo"/> which is to be invoked.</param>
        /// <returns>A delegate which can be used to invoke the method given a type instance.</returns>
        public static Func<object, TReturn> CreateInvoker<TReturn>(Type instanceType, MethodInfo method)
        {
            ParameterExpression objectInstanceParameter = Expression.Parameter(typeof(object), "instance");
            UnaryExpression typedInstanceParameter = Expression.Convert(objectInstanceParameter, instanceType);
            MethodCallExpression methodInvocation = Expression.Call(typedInstanceParameter, method);
            Expression<Func<object, TReturn>> expression = Expression.Lambda<Func<object, TReturn>>(methodInvocation, objectInstanceParameter);
            return expression.Compile();
        }
    }
}

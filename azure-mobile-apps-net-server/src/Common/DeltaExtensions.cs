// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace System.Web.Http.OData
{
    /// <summary>
    /// Various extension methods for the <see cref="Delta{T}"/> class.
    /// </summary>
    internal static class DeltaExtensions
    {
        /// <summary>
        /// Gets the value of a given property or <c>default</c> if property is either not present or not of type <typeparamref name="TProperty"/>.
        /// </summary>
        /// <typeparam name="TData">The type of <see cref="Delta{T}"/>.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="patch">The <see cref="Delta{T}"/> instance to probe for the property.</param>
        /// <param name="propertyName">The property name to look up.</param>
        /// <returns>The value of the property or <c>default</c> if either not present or of the wrong type.</returns>
        public static TProperty GetPropertyValueOrDefault<TData, TProperty>(this Delta<TData> patch, string propertyName)
            where TData : class
        {
            if (patch == null)
            {
                throw new ArgumentNullException("patch");
            }

            object objValue;
            if (patch.TryGetPropertyValue(propertyName, out objValue))
            {
                if (objValue is TProperty)
                {
                    return (TProperty)objValue;
                }
            }

            return default(TProperty);
        }
    }
}

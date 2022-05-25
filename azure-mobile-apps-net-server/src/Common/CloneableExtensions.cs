// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.ComponentModel;

namespace System
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class CloneableExtensions
    {
        /// <summary>
        /// Cloning an object that implement <see cref="ICloneable"/> using an explicit interface implementation.
        /// </summary>
        /// <typeparam name="T">The type of the object to clone.</typeparam>
        /// <param name="value">The object to clone.</param>
        /// <returns>A clone of <paramref name="value"/>.</returns>
        public static T Clone<T>(this T value) where T : ICloneable
        {
            return (T)value.Clone();
        }
    }
}

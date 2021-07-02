// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Reflection;

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// Attribute to associate a string value to an enum value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal class EnumValueAttribute : Attribute
    {
        /// <summary>
        /// Creates a new <see cref="EnumValueAttribute"/>
        /// </summary>
        /// <param name="value">The string value to be associated with the enum value</param>
        public EnumValueAttribute(string value)
        {
            Value = value;
        }

        /// <summary>
        /// The string associated with the value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Returns the associated string for a specific enum value.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum</typeparam>
        /// <param name="value">The enum value</param>
        /// <returns>The string value associated with the enum value, or null.</returns>
        internal static string GetValue<TEnum>(TEnum value)
            => typeof(TEnum).GetTypeInfo().GetDeclaredField(value.ToString())?.GetCustomAttribute<EnumValueAttribute>()?.Value;
    }
}

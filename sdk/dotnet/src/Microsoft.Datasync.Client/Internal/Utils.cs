// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Microsoft.Datasync.Client.Internal
{
    /// <summary>
    /// A set of utility methods.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// The name of the default Id property.
        /// </summary>
        private const string idPropertyName = "Id";

        /// <summary>
        /// The name of the default Version property.
        /// </summary>
        private const string versionPropertyName = "Version";

        /// <summary>
        /// Finds the value of the id field via reflection.  If there is a field marked with <see cref="KeyAttribute"/>,
        /// then that is used.  If there is a field called <see cref="Id"/> then that is used.  If neither are available,
        /// then <see cref="MemberAccessException"/> is thrown.
        /// </summary>
        /// <param name="item">The item to process</param>
        /// <returns>The id of the item</returns>
        internal static string GetIdFromItem(object item)
        {
            Validate.IsNotNull(item, nameof(item));
            var idProperty = Array.Find(item.GetType().GetProperties(), prop => prop.IsDefined(typeof(KeyAttribute)))
                ?? item.GetType().GetProperty(idPropertyName);
            if (idProperty == null)
            {
                throw new MissingMemberException($"{idPropertyName} not found, and no property has the [Key] attribute.");
            }
            object idValue = idProperty.GetValue(item);
            if (idValue == null)
            {
                throw new ArgumentNullException($"{idProperty.Name} is null", nameof(item));
            }
            if (idValue is string id)
            {
                return id;
            }
            throw new MemberAccessException($"{idProperty.Name} property is not a string");
        }

        /// <summary>
        /// Finds the value of the version field via reflection.  If there is a field marked with <see cref="ConcurrencyCheckAttribute"/>,
        /// then that is used.  If there is a field called <see cref="Version"/> then that is used.  If neither are available,
        /// then <see cref="MemberAccessException"/> is thrown.
        /// </summary>
        /// <param name="item">The item to process</param>
        /// <returns>The version of the item</returns>
        internal static string GetVersionFromItem(object item)
        {
            Validate.IsNotNull(item, nameof(item));
            var versionProperty = Array.Find(item.GetType().GetProperties(), prop => prop.IsDefined(typeof(ConcurrencyCheckAttribute)))
                ?? item.GetType().GetProperty(versionPropertyName);
            if (versionProperty == null)
            {
                throw new MissingMemberException($"{versionPropertyName} not found, and no property has the [ConcurrencyCheck] attribute.");
            }
            object versionValue = versionProperty.GetValue(item);
            if (versionValue == null)
            {
                throw new ArgumentNullException($"{versionProperty.Name} is null", nameof(item));
            }
            if (versionValue is string version)
            {
                return version;
            }
            throw new MemberAccessException($"{versionProperty.Name} property is not a string");
        }
    }
}

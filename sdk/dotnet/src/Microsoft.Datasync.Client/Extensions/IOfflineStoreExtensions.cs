// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Serialization;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A set of extension methods for the <see cref="IOfflineStore"/> class.
    /// </summary>
    public static class IOfflineStoreExtensions
    {
        /// <summary>
        /// Defines a table for use with offline sync.
        /// </summary>
        /// <typeparam name="T">The type of entity stored in the table.</typeparam>
        /// <param name="store">The offline store.</param>
        public static void DefineTable<T>(this IOfflineStore store)
            => DefineTable<T>(store, new DatasyncSerializerSettings());

        /// <summary>
        /// Defines a table for use with offline sync.
        /// </summary>
        /// <typeparam name="T">The type of entity stored in the table.</typeparam>
        /// <param name="store">The offline store.</param>
        /// <param name="tableName">The name of the table.</param>
        public static void DefineTable<T>(this IOfflineStore store, string tableName)
            => store.DefineTable<T>(tableName, new DatasyncSerializerSettings());

        /// <summary>
        /// Defines a table for use with offline sync.
        /// </summary>
        /// <typeparam name="T">The type of entity stored in the table.</typeparam>
        /// <param name="store">The offline store.</param>
        /// <param name="settings">The serializer settings.</param>
        public static void DefineTable<T>(this IOfflineStore store, DatasyncSerializerSettings settings)
            => store.DefineTable<T>(settings.ContractResolver.ResolveTableName(typeof(T)), settings);
    }
}

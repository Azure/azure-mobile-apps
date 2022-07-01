// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A set of extension methods for the client library.
    /// </summary>
    public static class LibraryExtensions
    {
        /// <summary>
        /// Defines a table for use with offline sync.
        /// </summary>
        /// <typeparam name="T">The type of entity stored in the table.</typeparam>
        /// <param name="store">The offline store.</param>
        public static void DefineTable<T>(this AbstractOfflineStore store)
            => DefineTable<T>(store, new DatasyncSerializerSettings());

        /// <summary>
        /// Defines a table for use with offline sync.
        /// </summary>
        /// <typeparam name="T">The type of entity stored in the table.</typeparam>
        /// <param name="store">The offline store.</param>
        /// <param name="tableName">The name of the table.</param>
        public static void DefineTable<T>(this AbstractOfflineStore store, string tableName)
            => DefineTable<T>(store, tableName, new DatasyncSerializerSettings());

        /// <summary>
        /// Defines a table for use with offline sync.
        /// </summary>
        /// <typeparam name="T">The type of entity stored in the table.</typeparam>
        /// <param name="store">The offline store.</param>
        /// <param name="settings">The serializer settings.</param>
        public static void DefineTable<T>(this AbstractOfflineStore store, DatasyncSerializerSettings settings)
            => DefineTable<T>(store, settings.ContractResolver.ResolveTableName(typeof(T)), settings);

        /// <summary>
        /// Defines a table for use with offline sync.
        /// </summary>
        /// <typeparam name="T">The type of entity stored in the table.</typeparam>
        /// <param name="store">The offline store.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="settings">The serializer settings.</param>
        public static void DefineTable<T>(this AbstractOfflineStore store, string tableName, DatasyncSerializerSettings settings)
        {
            if (settings.ContractResolver.ResolveContract(typeof(T)) is not JsonObjectContract contract)
            {
                throw new ArgumentException($"The generic type '{typeof(T).Name}' is not an object");
            }
            if (contract.DefaultCreator == null)
            {
                throw new ArgumentException($"The generic type '{typeof(T).Name}' does not have a parameterless constructor.");
            }

            object definition = contract.DefaultCreator();

            // Set enum values to their default.
            foreach (JsonProperty contractProperty in contract.Properties)
            {
                if (contractProperty.PropertyType.GetTypeInfo().IsEnum)
                {
                    object firstValue = Enum.GetValues(contractProperty.PropertyType).Cast<object>().FirstOrDefault();
                    if (firstValue != null)
                    {
                        contractProperty.ValueProvider.SetValue(definition, firstValue);
                    }
                }
            }

            // Create a JObject out of the definition.
            string json = JsonConvert.SerializeObject(definition, settings);
            JObject jsonDefinition = JsonConvert.DeserializeObject<JObject>(json, settings);

            // Ensure we have an ID field.
            jsonDefinition[SystemProperties.JsonIdProperty] = string.Empty;

            // Set anything with a null in it to the appropriate type.
            foreach (JProperty prop in jsonDefinition.Properties().Where(i => i.Value.Type == JTokenType.Null))
            {
                JsonProperty cprop = contract.Properties[prop.Name];
                if (cprop.PropertyType == typeof(string) || cprop.PropertyType == typeof(Uri))
                {
                    jsonDefinition[prop.Name] = string.Empty;
                }
                else if (cprop.PropertyType == typeof(byte[]))
                {
                    jsonDefinition[prop.Name] = Array.Empty<byte>();
                }
                else if (cprop.PropertyType.GetTypeInfo().IsGenericType && cprop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    jsonDefinition[prop.Name] = new JValue(Activator.CreateInstance(cprop.PropertyType.GenericTypeArguments[0]));
                }
                else
                {
                    jsonDefinition[prop.Name] = new JObject();
                }
            }

            store.DefineTable(tableName, jsonDefinition);
        }

        /// <summary>
        /// Pull all items from the remote table to the offline table.
        /// </summary>
        /// <param name="table">The table reference.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation is complete.</returns>
        public static Task PullItemsAsync(this IOfflineTable table, CancellationToken cancellationToken = default)
           => table.PullItemsAsync(string.Empty, new PullOptions(), cancellationToken);

        /// <summary>
        /// Pull all items from the remote table to the offline table.
        /// </summary>
        /// <param name="table">The table reference.</param>
        /// <param name="options">The pull options to use.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation is complete.</returns>
        public static Task PullItemsAsync(this IOfflineTable table, PullOptions options, CancellationToken cancellationToken = default)
            => table.PullItemsAsync(string.Empty, options, cancellationToken);

        /// <summary>
        /// Pull all items matching the OData query string from the remote table to the offline table.
        /// </summary>
        /// <param name="table">The table reference.</param>
        /// <param name="query">The OData query string.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation is complete.</returns>
        public static Task PullItemsAsync(this IOfflineTable table, string query, CancellationToken cancellationToken = default)
            => table.PullItemsAsync(query, new PullOptions(), cancellationToken);

        /// <summary>
        /// Pull all items matching the LINQ query from the remote table to the offline table.
        /// </summary>
        /// <param name="table">The table reference.</param>
        /// <param name="query">The LINQ query.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation is complete.</returns>
        public static Task PullItemsAsync<T, U>(this IOfflineTable<T> table, ITableQuery<U> query, CancellationToken cancellationToken = default)
            => table.PullItemsAsync(query, new PullOptions(), cancellationToken);
    }
}

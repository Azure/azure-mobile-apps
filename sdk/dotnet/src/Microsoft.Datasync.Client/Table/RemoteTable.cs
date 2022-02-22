// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Table
{
    /// <summary>
    /// Provides the operations that can be done against a remote table 
    /// with untyped (JSON) object.
    /// </summary>
    internal class RemoteTable : IRemoteTable
    {
        /// <summary>
        /// Creates a new <see cref="RemoteTable"/> instance to perform
        /// untyped (JSON) requests to a remote table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="serviceClient">The service client that created this table.</param>
        internal RemoteTable(string tableName, DatasyncClient serviceClient)
        {
            Arguments.IsValidTableName(tableName, nameof(tableName));
            Arguments.IsNotNull(serviceClient, nameof(serviceClient));

            ServiceClient = serviceClient;
            TableName = tableName;
        }

        #region IRemoteTable
        /// <summary>
        /// The service client being used for communication.
        /// </summary>
        public DatasyncClient ServiceClient { get; }

        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Deletes an item from the remote table.
        /// </summary>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the response when complete.</returns>
        public async Task<JToken> DeleteItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            ObjectReader.GetSystemProperties(instance, out SystemProperties systemProperties);
            Arguments.IsValidId(systemProperties.Id, nameof(instance));

            // systemProperties.Id is the ID to be deleted.
            // systemProperties.Version (if set) is the version of the item to be deleted (send in If-Match header)

            throw new NotImplementedException();
        }

        /// <summary>
        /// Execute a query against a remote table.
        /// </summary>
        /// <param name="query">An OData query to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the results when the query finishes.</returns>
        public IAsyncEnumerable<JToken> GetAsyncItems(string query)
            => new FuncAsyncPageable<JToken>(nextLink => GetNextPageAsync(query, nextLink));

        /// <summary>
        /// Retrieve an item from the remote table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public Task<JToken> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidId(id, nameof(id));

            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to insert into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the inserted data when complete.</returns>
        public Task<JToken> InsertItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            ObjectReader.GetSystemProperties(instance, out SystemProperties systemProperties);
            if (systemProperties.Id != null)
            {
                // If the Id is set, then it must be valid.
                Arguments.IsValidId(systemProperties.Id, nameof(instance));
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Replaces an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to replace into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the replaced data when complete.</returns>
        public Task<JToken> ReplaceItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            ObjectReader.GetSystemProperties(instance, out SystemProperties systemProperties);
            Arguments.IsValidId(systemProperties.Id, nameof(instance));

            // systemProperties.Id is the ID to be replaced.
            // systemProperties.Version (if set) is the version of the item to be deleted (send in If-Match header)

            throw new NotImplementedException();
        }

        /// <summary>
        /// Undeletes an item in the remote table.
        /// </summary>
        /// <remarks>
        /// This requires that the table supports soft-delete.
        /// </remarks>
        /// <param name="instance">The instance to undelete in the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the response when complete.</returns>
        public Task<JToken> UndeleteItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            ObjectReader.GetSystemProperties(instance, out SystemProperties systemProperties);
            Arguments.IsValidId(systemProperties.Id, nameof(instance));

            // systemProperties.Id is the ID to be undeleted.
            // systemProperties.Version (if set) is the version of the item to be deleted (send in If-Match header)

            throw new NotImplementedException();
        }
        #endregion

        protected Task<Page<JToken>> GetNextPageAsync(string query, string nextLink)
        {
            throw new NotImplementedException();
        }
    }
}

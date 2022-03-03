// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// Reads and writes settings in the __config system table.
    /// </summary>
    internal class SyncSettingsManager : IDisposable
    {
        /// <summary>
        /// The definition of the configuration table in the persistent store.
        /// </summary>
        public static readonly JObject TableDefinition = new()
        {
            { SystemProperties.JsonIdProperty, string.Empty },
            { "value", string.Empty }
        };

        /// <summary>
        /// The minimum delta-token value (set as the epoch).
        /// </summary>
        private const string DefaultDeltaToken = "1970-01-01T00:00:00.0000000+00:00";

        /// <summary>
        /// A set of locks to coordinate access to the cache.
        /// </summary>
        private readonly AsyncLockDictionary cacheLock = new();

        /// <summary>
        /// The cache of the configuration.
        /// </summary>
        private readonly Dictionary<string, string> cache = new();

        /// <summary>
        /// Constructor for unit-testing.
        /// </summary>
        protected SyncSettingsManager()
        {
        }

        /// <summary>
        /// Creates a new settings manager.
        /// </summary>
        /// <param name="store">The store to persistently store the configuration elements.</param>
        public SyncSettingsManager(IOfflineStore store)
        {
            OfflineStore = store;
        }

        /// <summary>
        /// The offline store we are using to persistently store the configuration elements.
        /// </summary>
        internal IOfflineStore OfflineStore { get; }

        #region Delta Token Management
        /// <summary>
        /// Obtains the current delta token for a table/queryId from persistent store.
        /// </summary>
        /// <remarks>
        /// Delta tokens are stored for each query (identified by a user-specified queryId) to determine
        /// the minimum value for updatedAt that must be queried to get incremental changes to the table.
        /// </remarks>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="queryId">The query ID of the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to acquire.</param>
        /// <returns>A task that returns the delta token when complete.</returns>
        public virtual async Task<DateTimeOffset> GetDeltaTokenAsync(string tableName, string queryId, CancellationToken cancellationToken = default)
        {
            string value = await GetFromStoreAsync(GetDeltaTokenKey(tableName, queryId), DefaultDeltaToken, cancellationToken).ConfigureAwait(false);
            return DateTimeOffset.Parse(value);
        }

        /// <summary>
        /// Resets the delta token for a table/queryId from persistent store.
        /// </summary>
        /// <remarks>
        /// Delta tokens are stored for each query (identified by a user-specified queryId) to determine
        /// the minimum value for updatedAt that must be queried to get incremental changes to the table.
        /// </remarks>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="queryId">The query ID of the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to acquire.</param>
        /// <returns>A task that completes when the delta token has been reset.</returns>
        public virtual async Task ResetDeltaTokenAsync(string tableName, string queryId, CancellationToken cancellationToken = default)
        {
            string key = GetDeltaTokenKey(tableName, queryId);
            using (await cacheLock.Acquire(key, cancellationToken).ConfigureAwait(false))
            {
                cache.Remove(key);
                await OfflineStore.DeleteAsync(OfflineSystemTables.Configuration, new[] { key }, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sets the delta token for a table/queryId from persistent store.
        /// </summary>
        /// <remarks>
        /// Delta tokens are stored for each query (identified by a user-specified queryId) to determine
        /// the minimum value for updatedAt that must be queried to get incremental changes to the table.
        /// </remarks>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="queryId">The query ID of the table.</param>
        /// <param name="deltaToken">The value of the delta token.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to acquire.</param>
        /// <returns>A task that completes when the delta token has been set in the persistent store.</returns>
        public virtual Task SetDeltaTokenAsync(string tableName, string queryId, DateTimeOffset deltaToken, CancellationToken cancellationToken = default)
            => SetInStoreAsync(GetDeltaTokenKey(tableName, queryId), deltaToken.ToString("o"), cancellationToken);

        /// <summary>
        /// Returns the key of a delta token.
        /// </summary>
        /// <remarks>
        /// Delta tokens are stored for each query (identified by a user-specified queryId) to determine
        /// the minimum value for updatedAt that must be queried to get incremental changes to the table.
        /// </remarks>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="queryId">The query ID of the table.</param>
        /// <returns>The configuration key for the delta token.</returns>
        private static string GetDeltaTokenKey(string tableName, string queryId) => $"dt|{tableName}|{queryId}";
        #endregion

        /// <summary>
        /// Gets a setting from the persistent store.
        /// </summary>
        /// <param name="key">The key for the setting.</param>
        /// <param name="defaultValue">The default value for the setting (if the key is not found).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the value of the setting when complete.</returns>
        private async Task<string> GetFromStoreAsync(string key, string defaultValue, CancellationToken cancellationToken = default)
        {
            using (await cacheLock.Acquire(key, cancellationToken).ConfigureAwait(false))
            {
                if (!cache.TryGetValue(key, out string value))
                {
                    JObject setting = await OfflineStore.GetItemAsync(OfflineSystemTables.Configuration, key, cancellationToken).ConfigureAwait(false);
                    value = setting?.Value<string>("value") ?? defaultValue;
                    cache[key] = value;
                }
                return value;
            }
        }

        /// <summary>
        /// Stores a setting in the persistent store.
        /// </summary>
        /// <param name="key">The key for the setting.</param>
        /// <param name="value">The value for the setting.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        private async Task SetInStoreAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            using (await cacheLock.Acquire(key, cancellationToken).ConfigureAwait(false))
            {
                cache[key] = value;

                var obj = new JObject()
                {
                    { SystemProperties.JsonIdProperty, key },
                    { "value", value }
                };
                await OfflineStore.UpsertAsync(OfflineSystemTables.Configuration, new[] { obj }, false, cancellationToken).ConfigureAwait(false);
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // DO NOT DO ANYTHING HERE JUST YET
            }
        }
        #endregion
    }
}

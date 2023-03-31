// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.SQLiteStore.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.SQLiteStore
{
    internal class SQLiteDeltaTokenStore : IDeltaTokenStore
    {
        /// <summary>
        /// The regular expression to match for a valid table name.
        /// </summary>
        /// <remarks>
        /// This has to meet multiple standards.  The first is that it must be valid
        /// as a path segment according to RFC 2396.  It must also be a valid table
        /// name for any table in the offline store.  Since these vary wildly, we use
        /// a case of "least common denominator".
        /// </remarks>
        private static readonly Regex validTableNameRegex = new("^[a-z][a-z0-9_]{0,63}$");

        /// <summary>
        /// Definition for the __config system table in the persistent store.
        /// </summary>
        public static readonly JObject TableDefinition = new()
        {
            { SystemProperties.JsonIdProperty, string.Empty },
            { "value", 0L }
        };

        /// <summary>
        /// A set of locks to coordinate access to the cache.
        /// </summary>
        private readonly AsyncLock cacheLock = new();

        /// <summary>
        /// A cache of the delta-tokens.
        /// </summary>
        private readonly Dictionary<string, DateTimeOffset> cache = new();

        /// <summary>
        /// Creates a new <see cref="DefaultDeltaTokenStore"/> instance.
        /// </summary>
        /// <param name="store">The offline store used to persistently store the delta tokens</param>
        public SQLiteDeltaTokenStore(IOfflineStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            OfflineStore = store;
        }

        /// <summary>
        /// The offline store used to persistently store the delta tokens.
        /// </summary>
        internal IOfflineStore OfflineStore { get; }

        /// <summary>
        /// Obtains the current delta token for a table/queryId from persistent store.
        /// </summary>
        /// <remarks>
        /// Delta tokens are stored for each query (identified by a user-specified queryId) to determine
        /// the minimum value for updatedAt that must be queried to get incremental changes to the table.
        /// </remarks>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="queryId">The query ID of the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the delta token when complete.</returns>
        public virtual async Task<DateTimeOffset> GetDeltaTokenAsync(string tableName, string queryId, CancellationToken cancellationToken = default)
        {
            string key = GetKey(tableName, queryId);
            using (await cacheLock.AcquireAsync(cancellationToken).ConfigureAwait(false))
            {
                if (cache.TryGetValue(key, out DateTimeOffset cacheValue))
                {
                    return cacheValue;
                }

                JObject setting = await OfflineStore.GetItemAsync(SystemTables.Configuration, key, cancellationToken).ConfigureAwait(false);
                long unixms = setting?.Value<long>("value") ?? 0L;
                cache[key] = DateTimeOffset.FromUnixTimeMilliseconds(unixms);
                return cache[key];
            }
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
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the delta token has been reset.</returns>
        public virtual async Task ResetDeltaTokenAsync(string tableName, string queryId, CancellationToken cancellationToken = default)
        {
            string key = GetKey(tableName, queryId);
            using (await cacheLock.AcquireAsync(cancellationToken).ConfigureAwait(false))
            {
                cache.Remove(key);
                await OfflineStore.DeleteAsync(SystemTables.Configuration, new string[] { key }, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// For testing purposes, this invalidates the cache entry without removing the key from the DB.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="queryId">The query ID.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the delta token cache key is removed.</returns>
        internal async Task InvalidateCacheAsync(string tableName, string queryId, CancellationToken cancellationToken = default)
        {
            string key = GetKey(tableName, queryId);
            using (await cacheLock.AcquireAsync(cancellationToken).ConfigureAwait(false))
            {
                cache.Remove(key);
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
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abserve.</param>
        /// <returns>A task that completes when the delta token has been set in the persistent store.</returns>
        public virtual async Task SetDeltaTokenAsync(string tableName, string queryId, DateTimeOffset deltaToken, CancellationToken cancellationToken = default)
        {
            var key = GetKey(tableName, queryId);
            using (await cacheLock.AcquireAsync(cancellationToken).ConfigureAwait(false))
            {
                long unixms = deltaToken.ToUnixTimeMilliseconds();
                var obj = new JObject()
                {
                    { SystemProperties.JsonIdProperty, key },
                    { "value", unixms }
                };
                await OfflineStore.UpsertAsync(SystemTables.Configuration, new[] { obj }, false, cancellationToken).ConfigureAwait(false);
                cache[key] = DateTimeOffset.FromUnixTimeMilliseconds(unixms);
            }
        }

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
        private static string GetKey(string tableName, string queryId)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            if (queryId == null) throw new ArgumentNullException(nameof(queryId));
            if (!validTableNameRegex.IsMatch(tableName)) throw new ArgumentException($"'{tableName}' is an invalid table name", nameof(tableName));
            if (string.IsNullOrWhiteSpace(queryId)) throw new ArgumentException("Query ID cannot be white space", nameof(queryId));

            return $"dt.{tableName}.{queryId}";
        }
    }
}

// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class MobileServiceSyncTable<T> : MobileServiceSyncTable, IMobileServiceSyncTable<T>
    {
        private readonly MobileServiceTableQueryProvider queryProvider;
        private readonly IMobileServiceTable<T> remoteTable;

        public MobileServiceSyncTable(string tableName, MobileServiceTableKind kind, MobileServiceClient client)
            : base(tableName, kind, client)
        {
            this.remoteTable = client.GetTable<T>();
            this.queryProvider = new MobileServiceTableQueryProvider(this);
        }

        public Task<IEnumerable<T>> ReadAsync()
        {
            return ReadAsync(CreateQuery());
        }

        public Task<IEnumerable<U>> ReadAsync<U>(IMobileServiceTableQuery<U> query)
        {
            Arguments.IsNotNull(query, nameof(query));

            return this.queryProvider.Execute(query);
        }

        public Task PullAsync<U>(string queryId, IMobileServiceTableQuery<U> query, bool pushOtherTables, CancellationToken cancellationToken, PullOptions pullOptions)
        {
            Arguments.IsNotNull(query, nameof(query));

            string queryString = this.queryProvider.ToODataString(query);

            return this.PullAsync(queryId, queryString, query.Parameters, pushOtherTables, cancellationToken, pullOptions);
        }

        public Task PurgeAsync<U>(string queryId, IMobileServiceTableQuery<U> query, CancellationToken cancellationToken)
        {
            return this.PurgeAsync<U>(queryId, query, false, cancellationToken);
        }

        public Task PurgeAsync<U>(string queryId, IMobileServiceTableQuery<U> query, bool force, CancellationToken cancellationToken)
        {
            Arguments.IsNotNull(query, nameof(query));

            return this.PurgeAsync(queryId, queryProvider.ToODataString(query), force, cancellationToken);
        }

        public async Task RefreshAsync(T instance)
        {
            Arguments.IsNotNull(instance, nameof(instance));

            MobileServiceSerializer serializer = this.MobileServiceClient.Serializer;
            object objId = serializer.GetId(instance, ignoreCase: false, allowDefault: true);
            if (objId == null)
            {
                return; // refresh is not supposed to throw if your object does not have an id for some reason
            }

            string id = EnsureIdIsString(objId);

            // Get the latest version of this element
            JObject refreshed = await base.LookupAsync(id);

            if (refreshed == null)
            {
                throw new InvalidOperationException("Item not found in local store.");
            }

            // Deserialize that value back into the current instance
            serializer.Deserialize<T>(refreshed, instance);
        }

        public async Task InsertAsync(T instance)
        {
            Arguments.IsNotNull(instance, nameof(instance));

            MobileServiceSerializer serializer = this.MobileServiceClient.Serializer;
            var value = serializer.Serialize(instance) as JObject;
            // remove system properties since the jtoken insert overload doesn't remove them
            value = RemoveSystemPropertiesKeepVersion(value);
            JObject inserted = await base.InsertAsync(value);
            serializer.Deserialize(inserted, instance);
        }

        public Task UpdateAsync(T instance)
        {
            Arguments.IsNotNull(instance, nameof(instance));

            MobileServiceSerializer serializer = this.MobileServiceClient.Serializer;
            var value = serializer.Serialize(instance) as JObject;
            return base.UpdateAsync(value);
        }

        public Task DeleteAsync(T instance)
        {
            Arguments.IsNotNull(instance, nameof(instance));

            MobileServiceSerializer serializer = this.MobileServiceClient.Serializer;
            var value = serializer.Serialize(instance) as JObject;
            return base.DeleteAsync(value);
        }

        public async new Task<T> LookupAsync(string id)
        {
            JToken value = await base.LookupAsync(id);
            return value == null ? default : MobileServiceClient.Serializer.Deserialize<T>(value);
        }

        public IMobileServiceTableQuery<T> CreateQuery()
        {
            return this.queryProvider.Create(this.remoteTable, new T[0].AsQueryable(), new Dictionary<string, string>(), includeTotalCount: false);
        }

        public IMobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            return CreateQuery().Where(predicate);
        }

        public IMobileServiceTableQuery<U> Select<U>(Expression<Func<T, U>> selector)
        {
            return CreateQuery().Select(selector);
        }

        public IMobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return CreateQuery().OrderBy(keySelector);
        }

        public IMobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return CreateQuery().OrderByDescending(keySelector);
        }

        public IMobileServiceTableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return CreateQuery().ThenBy(keySelector);
        }

        public IMobileServiceTableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return CreateQuery().ThenByDescending(keySelector);
        }

        public IMobileServiceTableQuery<T> Skip(int count)
        {
            return CreateQuery().Skip(count);
        }

        public IMobileServiceTableQuery<T> Take(int count)
        {
            return CreateQuery().Take(count);
        }

        public IMobileServiceTableQuery<T> IncludeTotalCount()
        {
            return this.CreateQuery().IncludeTotalCount();
        }

        public Task<IEnumerable<T>> ToEnumerableAsync()
        {
            return this.ReadAsync();
        }

        public async Task<List<T>> ToListAsync()
        {
            return new QueryResultList<T>(await this.ReadAsync());
        }
    }
}

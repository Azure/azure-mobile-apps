// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Query
{
    /// <summary>
    /// Represents a query that can be evaluated against a Mobile Services
    /// table. MobileServiceTableQuery instances can be obtained via
    /// MobileServiceClient.Query(of T)().
    /// </summary>
    /// <remarks>
    /// Rather than implenting IQueryable directly, we've implemented the
    /// portion of the LINQ query pattern we support on MobileServiceTableQuery
    /// objects.  MobileServiceTableQuery instances are used to build up
    /// IQueryables from LINQ query operations.
    /// </remarks>
    internal class MobileServiceTableQuery<T> : IMobileServiceTableQuery<T>
    {
        /// <summary>
        /// Initializes a new instance of the MobileServiceTableQuery class.
        /// </summary>
        /// <param name="table">
        /// The table being queried.
        /// </param>
        /// <param name="queryProvider">
        /// The <see cref="MobileServiceTableQueryProvider"/> associated with this 
        /// <see cref="T:MobileServiceTableQuery`1{T}"/>
        /// </param>
        /// <param name="query">
        /// The encapsulated <see cref="IQueryable"/>.
        /// </param>
        /// <param name="parameters">
        /// The optional user-defined query string parameters to include with the query.
        /// </param>
        /// <param name="includeTotalCount">
        /// A value that if set will determine whether the query will request
        /// the total count for all the records that would have been returned
        /// ignoring any take paging/limit clause specified by client or
        /// server.
        /// </param>
        internal MobileServiceTableQuery(IMobileServiceTable<T> table,
                                         MobileServiceTableQueryProvider queryProvider,
                                         IQueryable<T> query,
                                         IDictionary<string, string> parameters,
                                         bool includeTotalCount)
        {
            Arguments.IsNotNull(table, nameof(table));

            this.Table = table;
            this.RequestTotalCount = includeTotalCount;
            this.Parameters = parameters;
            this.Query = query;
            this.QueryProvider = queryProvider;
        }

        /// <summary>
        /// Gets the MobileServiceTable being queried.
        /// </summary>
        public IMobileServiceTable<T> Table { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the query will request the total
        /// count for all the records that would have been returned ignoring
        /// any take paging/limit clause specified by client or server.
        /// </summary>
        public bool RequestTotalCount { get; private set; }

        /// <summary>
        /// The user-defined query string parameters to include with the query.
        /// </summary>
        public IDictionary<string, string> Parameters { get; private set; }

        /// <summary>
        /// Gets the underlying IQueryable associated with this query.
        /// </summary>
        public IQueryable<T> Query { get; set; }

        /// <summary>
        /// Gets the associated Query Provider capable of executing a <see cref="T:MobileServiceTableQuery`1{T}"/>.
        /// </summary>
        public MobileServiceTableQueryProvider QueryProvider { get; set; }

        /// <summary>
        /// Applies the specified filter predicate to the source query.
        /// </summary>
        /// <param name="predicate">
        /// The filter predicate.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            Arguments.IsNotNull(predicate, nameof(predicate));
            return QueryProvider.Create(Table, Queryable.Where(Query, predicate), Parameters, RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified selection to the source query.
        /// </summary>
        /// <typeparam name="U">
        /// Type representing the projected result of the query.
        /// </typeparam>
        /// <param name="selector">
        /// The selector function.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<U> Select<U>(Expression<Func<T, U>> selector)
        {
            Arguments.IsNotNull(selector, nameof(selector));

            // Create a new table with the same name/client but
            // pretending to be of a different type since the query needs to
            // have the same type as the table.  This won't cause any issues
            // since we're only going to use it to evaluate the query and it'll
            // never leak to users.
            MobileServiceTable<U> table = new MobileServiceTable<U>(Table.TableName, Table.MobileServiceClient);
            return QueryProvider.Create(table, Queryable.Select(this.Query, selector), Parameters, RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Arguments.IsNotNull(keySelector, nameof(keySelector));
            return QueryProvider.Create(Table, Queryable.OrderBy(Query, keySelector), Parameters, RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Arguments.IsNotNull(keySelector, nameof(keySelector));
            return QueryProvider.Create(Table, Queryable.OrderByDescending(Query, keySelector), Parameters, RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Arguments.IsNotNull(keySelector, nameof(keySelector));
            return QueryProvider.Create(Table, Queryable.ThenBy((IOrderedQueryable<T>)Query, keySelector), Parameters, RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of the member being ordered by.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression selecting the member to order by.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Arguments.IsNotNull(keySelector, nameof(keySelector));
            return QueryProvider.Create(Table, Queryable.ThenByDescending((IOrderedQueryable<T>)Query, keySelector), Parameters, RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified skip clause to the source query.
        /// </summary>
        /// <param name="count">
        /// The number to skip.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<T> Skip(int count)
        {
            return QueryProvider.Create(Table, Queryable.Skip(Query, count), Parameters, RequestTotalCount);
        }

        /// <summary>
        /// Applies the specified take clause to the source query.
        /// </summary>
        /// <param name="count">
        /// The number to take.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<T> Take(int count)
        {
            return QueryProvider.Create(Table, Queryable.Take(Query, count), Parameters, RequestTotalCount);
        }

        /// <summary>
        /// Applies to the source query the specified string key-value 
        /// pairs to be used as user-defined parameters with the request URI 
        /// query string.
        /// </summary>
        /// <param name="parameters">
        /// The parameters to apply.
        /// </param>
        /// <returns>
        /// The composed query.
        /// </returns>
        public IMobileServiceTableQuery<T> WithParameters(IDictionary<string, string> parameters)
        {
            if (parameters != null)
            {
                // Make sure to replace any existing value for the key
                foreach (KeyValuePair<string, string> pair in parameters)
                {
                    this.Parameters[pair.Key] = pair.Value;
                }
            }

            return QueryProvider.Create(Table, Query, Parameters, RequestTotalCount);
        }

        /// <summary>
        /// Ensure the query will get the total count for all the records that
        /// would have been returned ignoring any take paging/limit clause
        /// specified by client or server.
        /// </summary>
        /// <returns>
        /// The query object.
        /// </returns>
        public IMobileServiceTableQuery<T> IncludeTotalCount()
        {
            return QueryProvider.Create(Table, Query, Parameters, includeTotalCount: true);
        }

        /// <summary>
        /// Ensure the query will get the deleted records. This requires the soft delete feature to be enabled on the Mobile Service. Visit <see href="http://go.microsoft.com/fwlink/?LinkId=507647">the link</see> for details.
        /// </summary>
        /// <returns>
        /// The query object.
        /// </returns>
        public IMobileServiceTableQuery<T> IncludeDeleted()
        {
            return QueryProvider.Create(Table, Query, MobileServiceTable.IncludeDeleted(Parameters), includeTotalCount: true);
        }

        /// <summary>
        /// Evalute the query asynchronously and return the results.
        /// </summary>
        /// <returns>
        /// The evaluated query results.
        /// </returns>
        public Task<IEnumerable<T>> ToEnumerableAsync()
        {
            return QueryProvider.Execute(this);
        }

        /// <summary>
        /// Evalute the query asynchronously and return the results in a new
        /// List.
        /// </summary>
        /// <returns>
        /// The evaluated query results as a List.
        /// </returns>
        public async Task<List<T>> ToListAsync()
        {
            IEnumerable<T> items = await QueryProvider.Execute(this);
            return new QueryResultList<T>(items);
        }
    }
}

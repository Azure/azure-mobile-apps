// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Linq;
using System.Web.Http.OData.Query;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    /// <summary>
    /// Provides various utilities and helper methods for table related features.
    /// </summary>
    public static class TableUtils
    {
        private const string Version = "Version";
        private const string Deleted = "Deleted";

        private const int MinPageSize = 1;
        private const int DefaultPageSize = 50;

        private static int pageSize = DefaultPageSize;

        /// <summary>
        /// Gets the <c>Version</c> property name.
        /// </summary>
        public static string VersionPropertyName
        {
            get
            {
                return Version;
            }
        }

        /// <summary>
        /// Gets the <c>Deleted</c> property name.
        /// </summary>
        public static string DeletedPropertyName
        {
            get
            {
                return Deleted;
            }
        }

        /// <summary>
        /// Gets the max number of records that will get returned in a query result.
        /// </summary>
        public static int PageSize
        {
            get
            {
                return pageSize;
            }

            set
            {
                if (value < MinPageSize)
                {
                    throw new ArgumentOutOfRangeException("value", value, CommonResources.ArgMustBeGreaterThanOrEqualTo.FormatForUser(MinPageSize));
                }

                pageSize = value;
            }
        }

        /// <summary>
        /// Applies the filter on deleted records if <paramref name="includeDeleted"/> is true.
        /// </summary>
        /// <typeparam name="TData">The type of data</typeparam>
        /// <param name="query">The query to filter</param>
        /// <param name="includeDeleted">Whether filer is set</param>
        /// <returns></returns>
        public static IQueryable<TData> ApplyDeletedFilter<TData>(IQueryable<TData> query, bool includeDeleted) where TData : class, ITableData
        {
            if (!includeDeleted)
            {
                query = query.Where(item => !item.Deleted);
            }

            return query;
        }

        /// <summary>
        /// Gets the effective query result size meaning the max number of elements to include in a single query result
        /// based on the page size and top parameters in the <paramref name="query"/> <paramref name="settings"/>.
        /// </summary>
        /// <returns>The effective page size for the given query.</returns>
        public static int GetResultSize(ODataQueryOptions query, ODataQuerySettings settings)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            int defaultResultSize = settings.PageSize.HasValue ? settings.PageSize.Value : PageSize;
            return query.Top != null ? Math.Min(query.Top.Value, defaultResultSize) : defaultResultSize;
        }

        /// <summary>
        /// Gets a <see cref="NotImplementedException"/> indicating that a given <see cref="IDomainManager{TData}"/> only supports
        /// <see cref="IQueryable{T}"/> based querying.
        /// </summary>
        /// <param name="domainManagerType">The type of <see cref="IDomainManager{TData}"/>.</param>
        /// <param name="method">The method name which is not supports.</param>
        /// <returns>A new <see cref="NotImplementedException"/>.</returns>
        public static Exception GetQueryableOnlyQueryException(Type domainManagerType, string method)
        {
            string msg = TResources.DomainManager_NoQueryableQuery.FormatForUser(domainManagerType.GetShortName(), typeof(IQueryable<>).GetShortName(), method);
            return new NotImplementedException(msg);
        }

        /// <summary>
        /// Gets a <see cref="NotImplementedException"/> indicating that a given <see cref="IDomainManager{TData}"/> does not support
        /// <see cref="IQueryable{T}"/> based querying.
        /// </summary>
        /// <param name="domainManagerType">The type of <see cref="IDomainManager{TData}"/>.</param>
        /// <param name="method">The method name which is not supports.</param>
        /// <returns>A new <see cref="NotImplementedException"/>.</returns>
        public static Exception GetNoQueryableQueryException(Type domainManagerType, string method)
        {
            string msg = TResources.DomainManager_QueryableOnlyQuery.FormatForUser(domainManagerType.GetShortName(), typeof(IQueryable<>).GetShortName(), method);
            return new NotImplementedException(msg);
        }

        /// <summary>
        /// Gets a <see cref="NotImplementedException"/> indicating that a given <see cref="IDomainManager{TData}"/> only supports
        /// <see cref="IQueryable{T}"/> based lookup operations.
        /// </summary>
        /// <param name="domainManagerType">The type of <see cref="IDomainManager{TData}"/>.</param>
        /// <param name="method">The method name which is not supports.</param>
        /// <returns>A new <see cref="NotImplementedException"/>.</returns>
        public static Exception GetQueryableOnlyLookupException(Type domainManagerType, string method)
        {
            string msg = TResources.DomainManager_NoQueryableLookup.FormatForUser(domainManagerType.GetShortName(), typeof(IQueryable<>).GetShortName(), method);
            return new NotImplementedException(msg);
        }

        /// <summary>
        /// Gets a <see cref="NotImplementedException"/> indicating that a given <see cref="IDomainManager{TData}"/> does not support
        /// <see cref="IQueryable{T}"/> based lookup operations.
        /// </summary>
        /// <param name="domainManagerType">The type of <see cref="IDomainManager{TData}"/>.</param>
        /// <param name="method">The method name which is not supports.</param>
        /// <returns>A new <see cref="NotImplementedException"/>.</returns>
        public static Exception GetNoQueryableLookupException(Type domainManagerType, string method)
        {
            string msg = TResources.DomainManager_QueryableOnlyLookup.FormatForUser(domainManagerType.GetShortName(), typeof(IQueryable<>).GetShortName(), method);
            return new NotImplementedException(msg);
        }
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Community.OData.Linq;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.OData;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Datasync.Client.InMemoryStore
{
    /// <summary>
    /// A representation of an OData query
    /// </summary>
    internal class ODataQuery
    {
        // The page size for paging of results.
        private const int pageSize = 10;

        /// <summary>
        /// The list of additional parameters to send.
        /// </summary>
        public Dictionary<string, string> AdditionalParameters { get; set; } = new();

        /// <summary>
        /// The value of the $count parameters
        /// </summary>
        public bool Count { get; set; }

        /// <summary>
        /// The value of the $filter parameter.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// The value of the $orderBy parameter.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// The value of the $select parameter.
        /// </summary>
        public string Select { get; set; }

        /// <summary>
        /// The value of the $skip parameter.
        /// </summary>
        public string Skip { get; set; }

        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The value of the $top parameter.
        /// </summary>
        public string Top { get; set; }

        /// <summary>
        /// Counts the number of items to be returned without considering paging.
        /// </summary>
        /// <param name="items">An <see cref="IQueryable{T}"/> for the unfiltered items.</param>
        /// <returns>The filtered items as an <see cref="IQueryable{T}"/>.</returns>
        public long CountWithoutPaging(IQueryable<JObject> items)
        {
            var queryable = items.OData(s => s.QuerySettings.PageSize = pageSize);
            if (Filter != null)
            {
                queryable = queryable.Filter(Filter);
            }
            if (OrderBy != null)
            {
                queryable = queryable.OrderBy(OrderBy);
            }
            return queryable.ToOriginalQuery().LongCount();
        }

        /// <summary>
        /// Returns the filtered items (not including select).
        /// </summary>
        /// <param name="items">An <see cref="IQueryable{T}"/> for the unfiltered items.</param>
        /// <returns>The filtered items as an <see cref="IQueryable{T}"/>.</returns>
        public IQueryable<JObject> Invoke(IQueryable<JObject> items)
        {
            var queryable = items.OData(s => s.QuerySettings.PageSize = 10);
            if (Filter != null)
            {
                queryable = queryable.Filter(Filter);
            }
            if (OrderBy != null)
            {
                queryable = queryable.OrderBy(OrderBy);
            }
            if (Skip != null || Top != null)
            {
                queryable = queryable.TopSkip(topText: Top, skipText: Skip);
            }
            return queryable.ToOriginalQuery();
        }

        /// <summary>
        /// Constructs a "nextLink" value for the next query.
        /// </summary>
        /// <param name="skipValue">The number of items to skip.</param>
        /// <param name="topValue">The number of items to take.</param>
        /// <returns>A new URI for the skip value and top value.</returns>
        public Uri NextQuery(long skipValue, long topValue)
        {
            Dictionary<string, string> parameters = new();

            if (Count)
            {
                parameters.Add("$count", "true");
            }
            if (Filter != null)
            {
                parameters.Add("$filter", Filter);
            }
            if (OrderBy != null)
            {
                parameters.Add("$orderby", OrderBy);
            }
            if (Select != null)
            {
                parameters.Add("$select", Select);
            }
            if (skipValue > 0)
            {
                parameters.Add("$skip", $"{skipValue}");
            }
            if (topValue > 0)
            {
                parameters.Add("$top", $"{Math.Min(topValue, pageSize)}");
            }
            foreach (var kv in AdditionalParameters)
            {
                parameters.Add(kv.Key, kv.Value);
            }
            string querystring = string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}").ToList());
            return new Uri($"https://inmemorystore.local/tables/{TableName}?{querystring}");
        }

        /// <summary>
        /// Converts a query string to an <see cref="ODataQuery"/> object.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <returns>The <see cref="ODataQuery"/> equivalent.</returns>
        public static ODataQuery Parse(string tableName, string queryString)
        {
            var result = new ODataQuery() { TableName = string.IsNullOrWhiteSpace(tableName) ? "notlisted" : tableName };
            var parameters = HttpUtility.ParseQueryString(queryString);
            foreach (var parameter in parameters.AllKeys)
            {
                var paramValue = parameters.Get(parameter);
                switch (parameter.ToLowerInvariant())
                {
                    case "$count":
                        result.Count = paramValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;
                    case "$filter":
                        result.Filter = paramValue;
                        break;
                    case "$orderby":
                        result.OrderBy = paramValue;
                        break;
                    case "$select":
                        result.Select = paramValue;
                        break;
                    case "$skip":
                        result.Skip = paramValue;
                        break;
                    case "$top":
                        result.Top = paramValue;
                        break;
                    default:
                        result.AdditionalParameters.Add(parameter, paramValue);
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Converts a <see cref="QueryDescription"/> to an <see cref="ODataQuery"/> object.
        /// </summary>
        /// <param name="queryDescription">The <see cref="QueryDescription"/></param>
        /// <returns>The <see cref="ODataQuery"/> equivalent.</returns>
        public static ODataQuery Parse(QueryDescription queryDescription)
            => Parse(queryDescription.TableName, queryDescription.ToODataString());
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Datasync.Client.Query.OData
{
    /// <summary>
    /// A set of extension methods to enable OData translation for a <see cref="QueryDescription"/>.
    /// </summary>
    public static class QueryDescriptionExtensions
    {
        /// <summary>
        /// Converts the query structure into a standard OData URI protocol for queries.
        /// </summary>
        /// <param name="value">The <see cref="QueryDescription"/> to convert to OData.</param>
        /// <param name="parameters">A list of optional parameters to include in the query string.</param>
        /// <returns>A URI query string representing the query.</returns>
        public static string ToODataString(this QueryDescription value, IDictionary<string, string> parameters = null)
        {
            List<string> queryFragments = new();

            if (value.Filter != null)
            {
                string filterStr = ODataExpressionVisitor.ToODataString(value.Filter);
                queryFragments.Add($"{ODataOptions.Filter}={Uri.EscapeDataString(filterStr)}");
            }

            if (value.Ordering.Count > 0)
            {
                var orderings = value.Ordering.Select(o => o.ToODataString());
                queryFragments.Add($"{ODataOptions.OrderBy}={string.Join(",", orderings)}");
            }

            if (value.Skip.HasValue && value.Skip >= 0)
            {
                queryFragments.Add($"{ODataOptions.Skip}={value.Skip}");
            }

            if (value.Top.HasValue && value.Top >= 0)
            {
                queryFragments.Add($"{ODataOptions.Top}={value.Top}");
            }

            if (value.Selection.Count > 0)
            {
                queryFragments.Add($"{ODataOptions.Select}={string.Join(",", value.Selection.OrderBy(field => field).Select(Uri.EscapeDataString))}");
            }

            if (value.IncludeTotalCount)
            {
                queryFragments.Add($"{ODataOptions.InlineCount}=true");
            }

            if (parameters?.Count > 0)
            {
                foreach (var kv in parameters)
                {
                    queryFragments.Add($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}");
                }
            }

            return string.Join("&", queryFragments);
        }

        /// <summary>
        /// Convert this ordering node into an OData string representation.
        /// </summary>
        /// <returns>The OData string</returns>
        public static string ToODataString(this OrderByNode node)
        {
            string field = ODataExpressionVisitor.ToODataString(node.Expression);
            string direction = node.Direction == OrderByDirection.Ascending ? "" : " desc";
            return $"{field}{direction}";
        }
    }
}

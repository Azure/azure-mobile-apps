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
    internal static class QueryDescriptionExtensions
    {
        /// <summary>
        /// Converts the query structure into a standard OData URI protocol for queries.
        /// </summary>
        /// <returns>A URI query string representing the query.</returns>
        public static string ToODataString(this QueryDescription value)
        {
            List<string> queryFragments = new();

            if (value.Filter != null)
            {
                string filterStr = ODataExpressionVisitor.ToODataString(value.Filter);
                queryFragments.Add($"{ODataOptions.Filter}={filterStr}");
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
                queryFragments.Add($"{ODataOptions.Select}={string.Join(",", value.Selection.Select(Uri.EscapeDataString))}");
            }

            if (value.IncludeTotalCount)
            {
                queryFragments.Add($"{ODataOptions.InlineCount}=true");
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

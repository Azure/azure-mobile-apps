// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table.Query.Nodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Datasync.Client.Table.Query
{
    /// <summary>
    /// Represents the structural elements of an OData query over the
    /// subset of OData that the library uses.
    /// </summary>
    internal class QueryDescription
    {
        private const string FilterQueryParameter = "$filter";
        private const string OrderByQueryParameter = "$orderby";
        private const string SelectQueryParameter = "$select";
        private const string SkipQueryParameter = "$skip";
        private const string TopQueryParameter = "$top";

        /// <summary>
        /// The top of the parsed Filter query.
        /// </summary>
        internal QueryNode Filter { get; set; }

        /// <summary>
        /// The set of order-by descriptors.
        /// </summary>
        internal List<OrderByNode> Ordering { get; } = new();

        /// <summary>
        /// A list of additional parameters to send with the query.
        /// </summary>
        internal Dictionary<string, string> Parameters { get; set; } = new();

        /// <summary>
        /// The list of projections that should be applied to each element of the query.
        /// </summary>
        internal List<Delegate> Projections { get; } = new();

        /// <summary>
        /// The type of the argument to the projection (i.e. the type that should be deserialized)
        /// </summary>
        internal Type ProjectionArgumentType { get; set; }

        /// <summary>
        /// The set of fields to return from the query.
        /// </summary>
        internal List<string> Selection { get; } = new();

        /// <summary>
        /// The number of items to skip within the query
        /// </summary>
        internal int Skip { get; set; }

        /// <summary>
        /// The number of items to return.
        /// </summary>
        internal int Top { get; set; }

        /// <summary>
        /// Converts the current query to an OData string.
        /// </summary>
        /// <returns>The query string</returns>
        internal string ToODataString()
        {
            List<string> queryParams = new();

            if (Filter != null)
            {
                queryParams.Add($"{FilterQueryParameter}={Uri.EscapeUriString(ODataExpressionVisitor.ToODataString(Filter))}");
            }

            if (Ordering.Count > 0)
            {
                queryParams.Add($"{OrderByQueryParameter}={string.Join(",", Ordering.Select(o => o.ToODataString()))}");
            }

            if (Selection.Count > 0)
            {
                queryParams.Add($"{SelectQueryParameter}={string.Join(",", Selection.Select(Uri.EscapeUriString))}");
            }

            if (Skip > 0)
            {
                queryParams.Add($"{SkipQueryParameter}={Skip}");
            }

            if (Top > 0)
            {
                queryParams.Add($"{TopQueryParameter}={Top}");
            }

            if (Parameters.Count > 0)
            {
                Parameters.ToList().ForEach(p => queryParams.Add($"{Uri.EscapeUriString(p.Key)}={Uri.EscapeUriString(p.Value)}"));
            }

            queryParams.Sort(StringComparer.InvariantCulture); // Deterministic output order for testing
            return string.Join("&", queryParams);
        }
    }
}

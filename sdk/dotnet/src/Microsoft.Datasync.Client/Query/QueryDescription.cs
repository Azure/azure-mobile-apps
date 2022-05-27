// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Query.OData;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Datasync.Client.Query
{
    /// <summary>
    /// The structural elements of a compiled LINQ query that can be
    /// represented by the subset of OData that a Datasync service uses.
    /// </summary>
    public sealed class QueryDescription
    {
        public QueryDescription(string tableName)
        {
            Arguments.IsValidTableName(tableName, true, nameof(tableName));
            TableName = tableName;
        }

        /// <summary>
        /// The <see cref="QueryNode"/> for the query filter expression.
        /// </summary>
        public QueryNode Filter { get; set; }

        /// <summary>
        /// If <c>true</c>, include the total count of items that will be returned with this query
        /// (without considering paging).
        /// </summary>
        public bool IncludeTotalCount { get; set; }

        /// <summary>
        /// The list of expressions that specify the ordering constraints requested by the query.
        /// </summary>
        public IList<OrderByNode> Ordering { get; private set; } = new List<OrderByNode>();

        /// <summary>
        /// The type of the argument to the projection (i.e. the type that should be deserialized).
        /// </summary>
        internal Type ProjectionArgumentType { get; set; }

        /// <summary>
        /// The collection of projections that should be applied to each element of the query.
        /// </summary>
        internal List<Delegate> Projections { get; private set; } = new();

        /// <summary>
        /// The list of fields that should be selected from the items in the table.
        /// </summary>
        public IList<string> Selection { get; private set; } = new List<string>();

        /// <summary>
        /// The number of elements to skip
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// The name of the table being queried.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// The number of elements to take.
        /// </summary>
        public int? Top { get; set; }

        /// <summary>
        /// Creates a copy of a <see cref="QueryDescription"/>.
        /// </summary>
        /// <returns>The cloned copy.</returns>
        public QueryDescription Clone() => new(TableName)
        {
            Filter = Filter,
            IncludeTotalCount = IncludeTotalCount,
            Ordering = Ordering.ToList(),
            Projections = Projections.ToList(),
            ProjectionArgumentType = ProjectionArgumentType,
            Selection = Selection.ToList(),
            Skip = Skip,
            Top = Top
        };

        /// <summary>
        /// Parses an OData query and creates a <see cref="QueryDescription"/> instance.
        /// </summary>
        /// <param name="tableName">The name of the table for the query.</param>
        /// <param name="query">The OData query string.</param>
        /// <returns>The <see cref="QueryDescription"/> for the query.</returns>
        public static QueryDescription Parse(string tableName, string query = null)
        {
            bool includeTotalCount = false;
            int? top = null, skip = null;
            string[] selection = null;
            QueryNode filter = null;
            IList<OrderByNode> orderings = null;

            query ??= string.Empty;
            IDictionary<string, string> parameters = query.Split('&')
                .Select(part => part.Split(new[] { '=' }, 2))
                .ToDictionary(x => Uri.UnescapeDataString(x[0]), x => x.Length > 1 ? Uri.UnescapeDataString(x[1]) : string.Empty);
            foreach (var parameter in parameters)
            {
                if (string.IsNullOrEmpty(parameter.Key))
                {
                    continue;
                }

                switch (parameter.Key)
                {
                    case ODataOptions.Filter:
                        filter = ODataExpressionParser.ParseFilter(parameter.Value);
                        break;
                    case ODataOptions.OrderBy:
                        orderings = ODataExpressionParser.ParseOrderBy(parameter.Value);
                        break;
                    case ODataOptions.Skip:
                        skip = Int32.Parse(parameter.Value);
                        break;
                    case ODataOptions.Top:
                        top = Int32.Parse(parameter.Value);
                        break;
                    case ODataOptions.Select:
                        selection = parameter.Value.Split(',');
                        break;
                    case ODataOptions.InlineCount:
                        includeTotalCount = parameter.Value.Equals("true");
                        break;
                    default:
                        // Skip this argument - it isn't important.
                        break;
                }
            }

            return new QueryDescription(tableName)
            {
                Filter = filter,
                IncludeTotalCount = includeTotalCount,
                Ordering = new List<OrderByNode>(orderings ?? Array.Empty<OrderByNode>()),
                Projections = new(),
                Selection = new List<string>(selection ?? Array.Empty<string>()),
                Skip = skip,
                Top = top,
            };
        }
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
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
            Arguments.IsValidTableName(tableName, nameof(tableName));
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
        /// Creates a copy of a <see cref="QueryDescription."/>
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
    }
}

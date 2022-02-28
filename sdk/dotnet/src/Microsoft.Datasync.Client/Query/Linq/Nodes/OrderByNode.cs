// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.OData;

namespace Microsoft.Datasync.Client.Query.Linq.Nodes
{
    /// <summary>
    /// The ordering direction possibilities.
    /// </summary>
    public enum OrderByDirection
    {
        Ascending,
        Descending
    }

    /// <summary>
    /// Represents an $orderBy query option
    /// </summary>
    public class OrderByNode
    {
        public OrderByNode(QueryNode expression, OrderByDirection direction)
        {
            Expression = expression;
            Direction = direction;
        }

        /// <summary>
        /// Helper constructor to set ascending/descending property on boolean
        /// </summary>
        /// <param name="expression">The OData expression</param>
        /// <param name="ascending">True if ordering is ascending</param>
        public OrderByNode(QueryNode expression, bool ascending)
            : this(expression, ascending ? OrderByDirection.Ascending : OrderByDirection.Descending)
        {
        }

        public QueryNode Expression { get; }

        public OrderByDirection Direction { get; }
    }
}

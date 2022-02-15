// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Visitor;

namespace Microsoft.Datasync.Client.Query.Nodes
{
    /// <summary>
    /// The ordering direction possibilities.
    /// </summary>
    internal enum OrderByDirection
    {
        Ascending,
        Descending
    }

    /// <summary>
    /// Represents an $orderBy query option
    /// </summary>
    internal class OrderByNode
    {
        internal OrderByNode(QueryNode expression, OrderByDirection direction)
        {
            Expression = expression;
            Direction = direction;
        }

        /// <summary>
        /// Helper constructor to set ascending/descending property on boolean
        /// </summary>
        /// <param name="expression">The OData expression</param>
        /// <param name="ascending">True if ordering is ascending</param>
        internal OrderByNode(QueryNode expression, bool ascending)
            : this(expression, ascending ? OrderByDirection.Ascending : OrderByDirection.Descending)
        {
        }

        /// <summary>
        /// Convert this ordering node into an OData string representation.
        /// </summary>
        /// <returns>The OData string</returns>
        internal string ToODataString()
        {
            string field = ODataExpressionVisitor.ToODataString(Expression);
            string direction = Direction == OrderByDirection.Ascending ? "" : " desc";
            return $"{field}{direction}";
        }

        public QueryNode Expression { get; }

        public OrderByDirection Direction { get; }
    }
}

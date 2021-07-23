// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Table.Query.Nodes
{
    /// <summary>
    /// Enumerator of the supported binary operators.
    /// </summary>
    internal enum BinaryOperatorKind
    {
        Or = 0,
        And,
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo
    }

    /// <summary>
    /// A <see cref="QueryNode"/> representing a binary operator
    /// </summary>
    internal sealed class BinaryOperatorNode : QueryNode
    {
        internal BinaryOperatorNode(BinaryOperatorKind kind, QueryNode left = null, QueryNode right = null)
        {
            OperatorKind = kind;
            LeftOperand = left;
            RightOperand = right;
        }

        /// <inheritdoc/>
        internal override QueryNodeKind Kind => QueryNodeKind.BinaryOperator;

        /// <summary>
        /// The type of binary operator.
        /// </summary>
        internal BinaryOperatorKind OperatorKind { get; }

        /// <summary>
        /// The left side of the operation
        /// </summary>
        internal QueryNode LeftOperand { get; set; }

        /// <summary>
        /// The right side of the operation
        /// </summary>
        internal QueryNode RightOperand { get; set; }

        /// <inheritdoc />
        internal override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);

        /// <inheritdoc />
        internal override void SetChildren(IList<QueryNode> children)
        {
            Validate.IsNotNull(children, nameof(children));
            if (children.Count != 2)
            {
                throw new ArgumentException($"{nameof(children)} must have exactly 2 elements", nameof(children));
            }
            LeftOperand = children[0];
            RightOperand = children[1];
        }
    }
}

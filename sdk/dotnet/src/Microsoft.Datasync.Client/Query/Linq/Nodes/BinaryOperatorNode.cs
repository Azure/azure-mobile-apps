// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Query.Linq.Nodes
{
    /// <summary>
    /// Enumerator of the supported binary operators.
    /// </summary>
    public enum BinaryOperatorKind
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
    public sealed class BinaryOperatorNode : QueryNode
    {
        public BinaryOperatorNode(BinaryOperatorKind kind, QueryNode left = null, QueryNode right = null)
        {
            OperatorKind = kind;
            LeftOperand = left;
            RightOperand = right;
        }

        /// <inheritdoc/>
        public override QueryNodeKind Kind => QueryNodeKind.BinaryOperator;

        /// <summary>
        /// The type of binary operator.
        /// </summary>
        public BinaryOperatorKind OperatorKind { get; }

        /// <summary>
        /// The left side of the operation
        /// </summary>
        public QueryNode LeftOperand { get; set; }

        /// <summary>
        /// The right side of the operation
        /// </summary>
        public QueryNode RightOperand { get; set; }

        /// <inheritdoc />
        public override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);

        /// <inheritdoc />
        internal override void SetChildren(IList<QueryNode> children)
        {
            Arguments.IsNotNull(children, nameof(children));
            if (children.Count != 2)
            {
                throw new ArgumentException($"{nameof(children)} must have exactly 2 elements", nameof(children));
            }
            LeftOperand = children[0];
            RightOperand = children[1];
        }
    }
}

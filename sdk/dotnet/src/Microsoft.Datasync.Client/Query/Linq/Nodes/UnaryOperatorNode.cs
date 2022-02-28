// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Query.Linq.Nodes
{
    /// <summary>
    /// Enumeration of the supported unary operators.
    /// </summary>
    public enum UnaryOperatorKind
    {
        Negate,
        Not
    }

    /// <summary>
    /// A <see cref="QueryNode"/> representing a unary operator.
    /// </summary>
    public sealed class UnaryOperatorNode : QueryNode
    {
        public UnaryOperatorNode(UnaryOperatorKind kind, QueryNode operand)
        {
            OperatorKind = kind;
            Operand = operand;
        }

        /// <inheritdoc/>
        public override QueryNodeKind Kind => QueryNodeKind.UnaryOperator;

        /// <summary>
        /// The operand of the unary operator.
        /// </summary>
        public QueryNode Operand { get; set; }

        /// <summary>
        /// The operator represented by this node.
        /// </summary>
        public UnaryOperatorKind OperatorKind { get; set; }

        /// <inheritdoc/>
        public override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);

        /// <inheritdoc/>
        internal override void SetChildren(IList<QueryNode> children)
        {
            Arguments.IsNotNull(children, nameof(children));
            if (children.Count != 1)
            {
                throw new ArgumentException($"{nameof(children)} must contain exactly one element", nameof(children));
            }
            Operand = children[0];
        }
    }
}

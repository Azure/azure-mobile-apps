// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Datasync.Client.Table.Query.Nodes
{
    /// <summary>
    /// Enumeration of the supported unary operators.
    /// </summary>
    internal enum UnaryOperatorKind
    {
        Negate,
        Not
    }

    /// <summary>
    /// A <see cref="QueryNode"/> representing a unary operator.
    /// </summary>
    internal sealed class UnaryOperatorNode : QueryNode
    {
        internal UnaryOperatorNode(UnaryOperatorKind kind, QueryNode operand)
        {
            OperatorKind = kind;
            Operand = operand;
        }

        /// <inheritdoc/>
        internal override QueryNodeKind Kind => QueryNodeKind.UnaryOperator;

        /// <summary>
        /// The operand of the unary operator.
        /// </summary>
        internal QueryNode Operand { get; set; }

        /// <summary>
        /// The operator represented by this node.
        /// </summary>
        internal UnaryOperatorKind OperatorKind { get; set; }

        /// <inheritdoc/>
        internal override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);

        /// <inheritdoc/>
        internal override void SetChildren(IList<QueryNode> children)
        {
            Validate.IsNotNull(children, nameof(children));
            if (children.Count != 1)
            {
                throw new ArgumentException($"{nameof(children)} must contain exactly one element", nameof(children));
            }
            Operand = children[0];
        }
    }
}

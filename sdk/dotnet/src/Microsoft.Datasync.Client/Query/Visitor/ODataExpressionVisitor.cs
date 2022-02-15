// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Nodes;
using System;
using System.Text;

namespace Microsoft.Datasync.Client.Query.Visitor
{
    /// <summary>
    /// Walks the expression tree provided, generating an OData filter.
    /// </summary>
    internal sealed class ODataExpressionVisitor : QueryNodeVisitor<QueryNode>
    {
        /// <summary>
        /// The accumulator for the new representation of the expression tree.
        /// </summary>
        private StringBuilder Expression { get; } = new StringBuilder();

        /// <summary>
        /// You cannot instantiate this - access the visitor through one of the static methods.
        /// </summary>
        private ODataExpressionVisitor()
        {
        }

        /// <summary>
        /// Convert the provided <see cref="QueryNode"/> into an OData string.
        /// </summary>
        /// <param name="filter">The <see cref="QueryNode"/> to convert</param>
        /// <returns>The OData string representation of the <see cref="QueryNode"/></returns>
        internal static string ToODataString(QueryNode filter)
        {
            if (filter == null)
            {
                return string.Empty;
            }

            var visitor = new ODataExpressionVisitor();
            filter.Accept(visitor);
            return visitor.Expression.ToString();
        }

        /// <summary>
        /// Accept a visitor to a node, with error checking
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <param name="node">The node to visit</param>
        private void Accept(QueryNode parent, QueryNode node)
        {
            if (node == null)
            {
                throw new ArgumentException($"Parent {parent.Kind} is not complete.", nameof(node));
            }
            else
            {
                node.Accept(this);
            }
        }

        /// <summary>
        /// Visit a <see cref="BinaryOperatorNode"/>
        /// </summary>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node</returns>
        internal override QueryNode Visit(BinaryOperatorNode node)
        {
            Expression.Append('(');
            Accept(node, node.LeftOperand);
            Expression.Append(' ').Append(node.OperatorKind.ToODataString()).Append(' ');
            Accept(node, node.RightOperand);
            Expression.Append(')');
            return node;
        }

        /// <summary>
        /// Visit a <see cref="ConstantNode"/>
        /// </summary>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node</returns>
        internal override QueryNode Visit(ConstantNode node)
        {
            Expression.Append(node.ToODataString());
            return node;
        }

        /// <summary>
        /// Visit a <see cref="ConvertNode"/>
        /// </summary>
        /// <remarks>
        /// This should never happen, but it's added for compatibility with the interface
        /// </remarks>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node</returns>
        internal override QueryNode Visit(ConvertNode node)
        {
            throw new NotSupportedException("ConvertNode is not supported on the ODataExpressionVisitor");
        }

        /// <summary>
        /// Visit a <see cref="FunctionCallNode"/>
        /// </summary>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node</returns>
        internal override QueryNode Visit(FunctionCallNode node)
        {
            bool appendSeparator = false;
            Expression.Append(node.Name).Append('(');
            foreach (QueryNode arg in node.Arguments)
            {
                if (appendSeparator) Expression.Append(',');
                Accept(node, arg);
                appendSeparator = true;
            }
            Expression.Append(')');
            return node;
        }

        /// <summary>
        /// Visit a <see cref="MemberAccessNode"/>
        /// </summary>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node</returns>
        internal override QueryNode Visit(MemberAccessNode node)
        {
            Expression.Append(node.MemberName);
            return node;
        }

        /// <summary>
        /// Visit a <see cref="UnaryOperatorNode"/>
        /// </summary>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node</returns>
        internal override QueryNode Visit(UnaryOperatorNode node)
        {
            switch (node.OperatorKind)
            {
                case UnaryOperatorKind.Not:
                    Expression.Append("not(");
                    Accept(node, node.Operand);
                    Expression.Append(')');
                    break;
                default:
                    throw new NotSupportedException($"'{node.OperatorKind}' is not supported in a table query");
            }
            return node;
        }
    }
}

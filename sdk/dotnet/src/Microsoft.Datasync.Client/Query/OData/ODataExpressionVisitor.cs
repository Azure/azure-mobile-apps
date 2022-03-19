// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
using System;
using System.Text;

namespace Microsoft.Datasync.Client.Query.OData
{
    /// <summary>
    /// Translates an expression tree into an OData expression.
    /// </summary>
    internal class ODataExpressionVisitor : QueryNodeVisitor<QueryNode>
    {
        /// <summary>
        /// Translates an expression tree of <see cref="QueryNode"/> elements
        /// to an OData expression.
        /// </summary>
        /// <param name="filter">The top <see cref="QueryNode"/> representing the entire expression.</param>
        /// <returns>An OData string.</returns>
        public static string ToODataString(QueryNode filter)
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
        /// You cannot instantiate this - access the visitor through the static methods.
        /// </summary>
        protected ODataExpressionVisitor()
        {
        }

        /// <summary>
        /// The OData expression.
        /// </summary>
        public StringBuilder Expression { get; } = new();

        #region QueryNodeVisitor<QueryNode>
        /// <summary>
        /// Visit a <see cref="BinaryOperatorNode"/>
        /// </summary>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node</returns>
        public override QueryNode Visit(BinaryOperatorNode node)
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
        public override QueryNode Visit(ConstantNode node)
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
        public override QueryNode Visit(ConvertNode node)
        {
            throw new NotSupportedException("ConvertNode is not supported on the ODataExpressionVisitor");
        }

        /// <summary>
        /// Visit a <see cref="FunctionCallNode"/>
        /// </summary>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node</returns>
        public override QueryNode Visit(FunctionCallNode node)
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
        public override QueryNode Visit(MemberAccessNode node)
        {
            Expression.Append(node.MemberName);
            return node;
        }

        /// <summary>
        /// Visit a <see cref="UnaryOperatorNode"/>
        /// </summary>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node</returns>
        public override QueryNode Visit(UnaryOperatorNode node)
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
        #endregion

        /// <summary>
        /// Accept a visitor to a node, with error checking
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <param name="node">The node to visit</param>
        protected void Accept(QueryNode parent, QueryNode node)
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
    }
}

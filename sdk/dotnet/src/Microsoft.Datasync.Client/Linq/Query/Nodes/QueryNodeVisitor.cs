// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Linq.Query.Nodes
{
    /// <summary>
    /// Visitor interface for walking the QueryNode tree.
    /// </summary>
    /// <typeparam name="T">Type produced by the visitor.</typeparam>
    internal abstract class QueryNodeVisitor<T>
    {
        /// <summary>
        /// Visit a <see cref="BinaryOperatorNode"/>
        /// </summary>
        internal virtual T Visit(BinaryOperatorNode node)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Visit a <see cref="ConstantNode"/>
        /// </summary>
        internal virtual T Visit(ConstantNode node)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Visit a <see cref="ConvertNode"/>
        /// </summary>
        internal virtual T Visit(ConvertNode node)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Visit a <see cref="FunctionCallNode"/>
        /// </summary>
        internal virtual T Visit(FunctionCallNode node)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Visit a <see cref="MemberAccessNode"/>
        /// </summary>
        internal virtual T Visit(MemberAccessNode node)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Visit a <see cref="UnaryOperatorNode"/>
        /// </summary>
        internal virtual T Visit(UnaryOperatorNode node)
        {
            throw new NotSupportedException();
        }
    }
}

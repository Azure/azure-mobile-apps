
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Query.Linq.Nodes
{
    /// <summary>
    /// Visitor interface for walking the <see cref="QueryNode"/> tree.
    /// </summary>
    /// <typeparam name="T">Type produced by the visitor.</typeparam>
    public abstract class QueryNodeVisitor<T>
    {
        /// <summary>
        /// Visit a <see cref="BinaryOperatorNode"/>
        /// </summary>
        public abstract T Visit(BinaryOperatorNode node);

        /// <summary>
        /// Visit a <see cref="ConstantNode"/>
        /// </summary>
        public abstract T Visit(ConstantNode node);

        /// <summary>
        /// Visit a <see cref="ConvertNode"/>
        /// </summary>
        public abstract T Visit(ConvertNode node);

        /// <summary>
        /// Visit a <see cref="FunctionCallNode"/>
        /// </summary>
        public abstract T Visit(FunctionCallNode node);

        /// <summary>
        /// Visit a <see cref="MemberAccessNode"/>
        /// </summary>
        public abstract T Visit(MemberAccessNode node);

        /// <summary>
        /// Visit a <see cref="UnaryOperatorNode"/>
        /// </summary>
        public abstract T Visit(UnaryOperatorNode node);
    }
}

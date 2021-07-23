// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Table.Query.Nodes
{
    /// <summary>
    /// A <see cref="QueryNode"/> representing a primitive constant value.
    /// </summary>
    internal sealed class ConstantNode : QueryNode
    {
        internal ConstantNode(object value)
        {
            Value = value;
        }

        /// <inheritdoc />
        internal override QueryNodeKind Kind => QueryNodeKind.Constant;

        /// <summary>
        /// The value of the primitive constant.
        /// </summary>
        internal object Value { get; set; }

        /// <inheritdoc />
        internal override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}

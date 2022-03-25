// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Query.Linq.Nodes
{
    /// <summary>
    /// A <see cref="QueryNode"/> representing a primitive constant value.
    /// </summary>
    public sealed class ConstantNode : QueryNode
    {
        public ConstantNode(object value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public override QueryNodeKind Kind => QueryNodeKind.Constant;

        /// <summary>
        /// The value of the primitive constant.
        /// </summary>
        public object Value { get; set; }

        /// <inheritdoc />
        public override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Datasync.Client.Table.Query.Nodes
{
    /// <summary>
    /// A <see cref="QueryNode"/> representing a function call
    /// </summary>
    internal sealed class FunctionCallNode : QueryNode
    {
        internal FunctionCallNode(string name, IList<QueryNode> arguments = null)
        {
            Name = name;
            Arguments = arguments ?? new List<QueryNode>();
        }

        /// <inheritdoc />
        internal override QueryNodeKind Kind => QueryNodeKind.FunctionCall;

        /// <summary>
        /// The name of the function to call.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// The list of arguments to this function call.
        /// </summary>
        internal IList<QueryNode> Arguments { get; set; }

        /// <inheritdoc/>
        internal override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);

        /// <inheritdoc/>
        internal override void SetChildren(IList<QueryNode> children)
        {
            Arguments = children.ToList();
        }
    }
}

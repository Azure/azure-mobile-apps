// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Datasync.Client.Query.Linq.Nodes
{
    /// <summary>
    /// A <see cref="QueryNode"/> representing a function call
    /// </summary>
    public sealed class FunctionCallNode : QueryNode
    {
        public FunctionCallNode(string name, IList<QueryNode> arguments = null)
        {
            Name = name;
            Arguments = arguments ?? new List<QueryNode>();
        }

        /// <inheritdoc />
        public override QueryNodeKind Kind => QueryNodeKind.FunctionCall;

        /// <summary>
        /// The name of the function to call.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The list of arguments to this function call.
        /// </summary>
        public IList<QueryNode> Arguments { get; set; }

        /// <inheritdoc/>
        public override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);

        /// <inheritdoc/>
        internal override void SetChildren(IList<QueryNode> children)
        {
            Arguments = children.ToList();
        }
    }
}

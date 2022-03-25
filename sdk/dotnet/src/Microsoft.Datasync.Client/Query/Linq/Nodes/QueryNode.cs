// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Query.Linq.Nodes
{
    /// <summary>
    /// The base class for all types of QueryNode.
    /// </summary>
    public abstract class QueryNode
    {
        /// <summary>
        /// Accept a QueryNodeVisitor that walks a tree of type QueryNode.
        /// </summary>
        /// <typeparam name="T">The type that the visitor will return after visiting this token.</typeparam>
        /// <param name="visitor">The visitor.</param>
        public abstract T Accept<T>(QueryNodeVisitor<T> visitor);

        /// <summary>
        /// The type of the QueryNode
        /// </summary>
        public abstract QueryNodeKind Kind { get; }

        /// <summary>
        /// Sets the children for this QueryNode.  Note that not all query nodes support
        /// children, so this is not always called.  If it is called when unexpected, a
        /// <see cref="NotSupportedException"/> will be thrown.
        /// </summary>
        /// <param name="children">The list of children to set.</param>
        internal virtual void SetChildren(IList<QueryNode> children)
        {
            throw new NotSupportedException();
        }
    }
}

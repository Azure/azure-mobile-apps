// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Query.Linq.Nodes
{
    /// <summary>
    /// A <see cref="QueryNode"/> representing a conversion from one type to another.
    /// </summary>
    public sealed class ConvertNode : QueryNode
    {
        public ConvertNode(QueryNode source, Type targetType)
        {
            Source = source;
            TargetType = targetType;
        }

        /// <inheritdoc />
        public override QueryNodeKind Kind => QueryNodeKind.Convert;

        /// <summary>
        /// The source value to convert
        /// </summary>
        public QueryNode Source { get; set; }

        /// <summary>
        /// The type that we're converting to
        /// </summary>
        public Type TargetType { get; set; }

        /// <inheritdoc/>
        /// <remarks>
        /// This Accept method is never called.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        public override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);

        /// <inheritdoc/>
        internal override void SetChildren(IList<QueryNode> children)
        {
            if (children.Count == 0)
            {
                throw new ArgumentException("children is empty", nameof(children));
            }
            Source = children[0];
        }
    }
}

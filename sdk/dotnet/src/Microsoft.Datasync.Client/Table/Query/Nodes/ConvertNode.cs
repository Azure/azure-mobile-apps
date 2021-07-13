// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Table.Query.Nodes
{
    /// <summary>
    /// A <see cref="QueryNode"/> representing a conversion from one type to another.
    /// </summary>
    internal sealed class ConvertNode : QueryNode
    {
        internal ConvertNode(QueryNode source, Type targetType)
        {
            Source = source;
            TargetType = targetType;
        }

        /// <inheritdoc />
        internal override QueryNodeKind Kind => QueryNodeKind.Convert;

        /// <summary>
        /// The source value to convert
        /// </summary>
        internal QueryNode Source { get; set; }

        /// <summary>
        /// The type that we're converting to
        /// </summary>
        internal Type TargetType { get; set; }

        /// <inheritdoc/>
        /// <remarks>
        /// This Accept method is never called.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        internal override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}

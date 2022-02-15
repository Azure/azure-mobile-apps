// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Query.Nodes
{
    /// <summary>
    /// A <see cref="QueryNode"/> representing access to a member value
    /// </summary>
    internal sealed class MemberAccessNode : QueryNode
    {
        internal MemberAccessNode(QueryNode instance, string memberName)
        {
            Instance = instance;
            MemberName = memberName;
        }

        /// <summary>
        /// The object instance we are accessing
        /// </summary>
        internal QueryNode Instance { get; set; }

        /// <inheritdoc />
        internal override QueryNodeKind Kind => QueryNodeKind.MemberAccess;

        /// <summary>
        /// The name of the member (property, field, etc.) we are acccessing
        /// </summary>
        internal string MemberName { get; set; }

        /// <inheritdoc/>
        internal override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}

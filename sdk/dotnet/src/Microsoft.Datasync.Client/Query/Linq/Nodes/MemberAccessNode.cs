// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Query.Linq.Nodes
{
    /// <summary>
    /// A <see cref="QueryNode"/> representing access to a member value
    /// </summary>
    public sealed class MemberAccessNode : QueryNode
    {
        public MemberAccessNode(QueryNode instance, string memberName)
        {
            Instance = instance;
            MemberName = memberName;
        }

        /// <summary>
        /// The object instance we are accessing
        /// </summary>
        public QueryNode Instance { get; set; }

        /// <inheritdoc />
        public override QueryNodeKind Kind => QueryNodeKind.MemberAccess;

        /// <summary>
        /// The name of the member (property, field, etc.) we are acccessing
        /// </summary>
        public string MemberName { get; set; }

        /// <inheritdoc/>
        public override T Accept<T>(QueryNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}

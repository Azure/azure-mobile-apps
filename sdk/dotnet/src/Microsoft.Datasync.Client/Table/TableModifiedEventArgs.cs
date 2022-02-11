// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client
{
    public struct TableModifiedEventArgs
    {
        /// <summary>
        /// The list of potential operation types.
        /// </summary>
        public enum TableOperation
        {
            Create,
            Delete,
            Replace
        }

        /// <summary>
        /// If the request is for the remote service, the <see cref="Uri"/> for the table endpoint.
        /// </summary>
        /// <remarks>
        /// If for an offline table, TableEndpoint will be <c>null</c>.
        /// </remarks>
        public Uri TableEndpoint { get; internal set; }

        /// <summary>
        /// The name of the offline table.  If the request is against a remote service, then the 
        /// <see cref="TableName"/> will be <c>null</c>, and <see cref="TableEndpoint"/> will be
        /// set instead.
        /// </summary>
        public string TableName { get; internal set; }

        /// <summary>
        /// The ID of the entity that was operated on.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// The operation that was performed.
        /// </summary>
        public TableOperation Operation { get; internal set; }

        /// <summary>
        /// The entity, if available, that was acted on.
        /// </summary>
        public object Entity { get; internal set; }
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// The list of potential operation types.
    /// </summary>
    public enum TableOperation
    {
        Create,
        Delete,
        Update
    }

    /// <summary>
    /// When the <see cref="IDatasyncTable{T}.TableModified"/> event is fired,
    /// this is the arguments that are passed to the event handler.
    /// </summary>
    public struct DatasyncTableEventArgs
    {
        /// <summary>
        /// The <see cref="Uri"/> for the table endpoint.
        /// </summary>
        public Uri TableEndpoint { get; internal set; }

        /// <summary>
        /// The ID of the entity that was operated on.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// The operation that was performed.
        /// </summary>
        public TableOperation Operation { get; internal set; }

        /// <summary>
        /// The created or updated entity.  If the <see cref="Operation"/> is <c>Delete</c>,
        /// then the entity will be null.
        /// </summary>
        /// <remarks>
        /// The object will be the same entity type as the table generating the event.
        /// </remarks>
        public object Entity { get; internal set; }
    }
}

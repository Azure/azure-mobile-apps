// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync;

/// <summary>
/// The valid operations passed to an <see cref="IAccessControlProvider{TEntity}"/> method to
/// show what the client is requesting.
/// </summary>
public enum TableOperation
{
    /// <summary>
    /// An entity is being created.
    /// </summary>
    Create,

    /// <summary>
    /// An entity is being deleted.
    /// </summary>
    Delete,

    /// <summary>
    /// The data store is being queried.
    /// </summary>
    Query,

    /// <summary>
    /// An entity is being read.
    /// </summary>
    Read,

    /// <summary>
    /// An entity is being updated.
    /// </summary>
    Update
}

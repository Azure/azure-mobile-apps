// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.Datasync.Client.Http;

/// <summary>
/// The base class for client data objects.  Note that this is *NOT* the same as the
/// server side <see cref="ITableData"/> object because the Version is a string when
/// transmitted over the wire, and is considered opaque.
/// </summary>
public abstract class ClientTableData
{
    /// <summary>
    /// The globally unique ID of the entity.
    /// </summary>
    [Key]
    public string Id { get; internal set; } = string.Empty;

    /// <summary>
    /// The version of the entity.  This is an opaque string that is used to detect
    /// server side conflicts.  DO NOT USE <see cref="RowVersionAttribute"/> on this
    /// property.
    /// </summary>
    public string Version { get; internal set; } = string.Empty;

    /// <summary>
    /// The last date/time that the entity was updated on the server.  Do not use a
    /// client-side trigger on this as it needs to be set by the server.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; internal set; } = DateTimeOffset.MinValue;

    /// <summary>
    /// If set to true, the entity is considered deleted and should not be shown to the
    /// user.  Do not use a client-side trigger on this as it needs to be set by the server.
    /// </summary>
    public bool Deleted { get; internal set; }
}

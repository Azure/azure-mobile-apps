// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.EFCore;

public class EntityTableRepositoryOptions
{
    /// <summary>
    /// If <c>true</c>, then the database automatically updates the <c>UpdatedAt</c> property
    /// on the entity.  If <c>false</c>, the repository will do it instead.
    /// </summary>
    public bool DatabaseUpdatesTimestamp { get; set; } = true;

    /// <summary>
    /// If <c>true</c>, then the database automatically updates the <c>Version</c> property
    /// on the entity.  If <c>false</c>, the repository will do it instead.
    /// </summary>
    public bool DatabaseUpdatesVersion { get; set; } = true;
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Datasync;

internal static class ITableDataExtensions
{
    /// <summary>
    /// Determines if the entity has a valid version.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns><c>true</c> if the entity version is valid; <c>false</c> otherwise.</returns>
    internal static bool HasValidVersion(this ITableData entity) => entity.Version.Length > 0;

    /// <summary>
    /// Returns the <see cref="EntityTagHeaderValue"/> for this entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The <see cref="EntityTagHeaderValue"/> or <c>null</c> if the version is not valid.</returns>
    internal static EntityTagHeaderValue? ToEntityTagHeaderValue(this ITableData entity)
        => HasValidVersion(entity) ? new EntityTagHeaderValue($"\"{Convert.ToBase64String(entity.Version)}\"") : null;
}

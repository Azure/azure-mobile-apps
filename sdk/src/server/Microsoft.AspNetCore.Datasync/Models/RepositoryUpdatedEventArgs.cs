// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync;

/// <summary>
/// The event arguments for the <see cref="IRepository{TEntity}.RepositoryUpdated"/> event.
/// </summary>
public readonly struct RepositoryUpdatedEventArgs
{
    /// <summary>
    /// Creaets a new <see cref="RepositoryUpdatedEventArgs"/> object.
    /// </summary>
    /// <param name="operation">The operation being performed on an entity.</param>
    /// <param name="entityName">The name of the entity.</param>
    /// <param name="entity">The entity value.</param>
    public RepositoryUpdatedEventArgs(TableOperation operation, string entityName, object entity)
    {
        Operation = operation;
        EntityName = entityName;
        Entity = entity;
    }

    /// <summary>
    /// The operation that was performed on the entity.
    /// </summary>
    public TableOperation Operation { get; }

    /// <summary>
    /// The name of the entity.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// The updated entity value.  If the entity was deleted, the original entity value is returned.
    /// </summary>
    public object Entity { get; }

    /// <summary>
    /// The time the repository event was raised.
    /// </summary>
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync.Models;

/// <summary>
/// An internal version of the repository that throws an exception if it is used.  This is used as
/// the default value of the repository within the table controller.
/// </summary>
/// <typeparam name="TEntity">The type of entity used in the repository.</typeparam>
[ExcludeFromCodeCoverage]
internal class Repository<TEntity> : IRepository<TEntity> where TEntity : ITableData
{
    public ValueTask<IQueryable<TEntity>> AsQueryableAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("The repository must be set within the table controller.");
    }

    public ValueTask CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("The repository must be set within the table controller.");
    }

    public ValueTask DeleteAsync(string id, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("The repository must be set within the table controller.");
    }

    public ValueTask<TEntity> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("The repository must be set within the table controller.");
    }

    public ValueTask ReplaceAsync(TEntity entity, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("The repository must be set within the table controller.");
    }
}

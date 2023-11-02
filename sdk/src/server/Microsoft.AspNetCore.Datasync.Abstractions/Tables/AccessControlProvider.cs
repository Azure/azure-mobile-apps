// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.


using Microsoft.AspNetCore.Datasync.Abstractions;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Datasync;

/// <summary>
/// The default implementation of the <see cref="IAccessControlProvider{TEntity}"/> interface that
/// allows the connecting client to do anything, but doesn't do anything extra.
/// </summary>
/// <typeparam name="TEntity">The type of entity being processed.</typeparam>
public class AccessControlProvider<TEntity> : IAccessControlProvider<TEntity> where TEntity : ITableData
{
    /// <inheritdoc />
    public virtual Expression<Func<TEntity, bool>>? GetDataView()
        => null;

    /// <inheritdoc />
    public virtual ValueTask<bool> IsAuthorizedAsync(TableOperation operation, TEntity entity, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(true);

    /// <inheritdoc />
    public virtual ValueTask PostCommitHookAsync(TableOperation operation, TEntity entity, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <inheritdoc />
    public virtual ValueTask PreCommitHookAsync(TableOperation operation, TEntity entity, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// <para>
    /// An <see cref="IAccessControlProvider{TEntity}"/> object defines the access permissions
    /// for a client to access one or more entities within the table.  It is comprised of three
    /// elements - a view of the data, a boolean check for authorization, and a pre-commit hook.
    /// </para>
    /// <para>
    /// Between the three methods, you can decide what items can be operated on, what operations
    /// are allowed, and how to store any additional information to provide for authorization.</para>
    /// </summary>
    /// <typeparam name="TEntity">The type of entity within the table.</typeparam>
    public interface IAccessControlProvider<TEntity> where TEntity : ITableData
    {
        /// <summary>
        /// Returns a LINQ <see cref="Where{T}"/> predicate to limit the data that the client can
        /// see.  Return null if you wish the client to see all data.
        /// </summary>
        /// <returns>A LINQ predicate.</returns>
        Expression<Func<TEntity, bool>> GetDataView();

        /// <summary>
        /// Determines if the client is allowed to perform the <see cref="TableOperation"/> on
        /// the provided entity.
        /// </summary>
        /// <param name="operation">The <see cref="TableOperation"/> being requested.</param>
        /// <param name="entity">The entity being used.</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>True if the operation is authorized.</returns>
        Task<bool> IsAuthorizedAsync(TableOperation operation, TEntity entity, CancellationToken token = default);

        /// <summary>
        /// Updates the entity immediately prior to write operations with the data store to
        /// support the chosen access control rules.
        /// </summary>
        /// <param name="operation">The <see cref="TableOperation"/> being requested.</param>
        /// <param name="entity">The entity being used.</param>
        /// <param name="token">A cancellation token</param>
        Task PreCommitHookAsync(TableOperation operation, TEntity entity, CancellationToken token = default);

        /// <summary>
        /// Called by the <see cref="TableController{TEntity}"/> after the entity has been updated. 
        /// </summary>
        /// <remarks>
        /// Do not change the entity in the <see cref="PostCommitHookAsync(TableOperation, TEntity, CancellationToken)"/>.
        /// It will be sent to the client, but not recorded in the repository.
        /// </remarks>
        /// <param name="operation">The <see cref="TableOperation"/> being requested.</param>
        /// <param name="entity">The entity being used.</param>
        /// <param name="token">A cancellation token</param>
        Task PostCommitHookAsync(TableOperation operation, TEntity entity, CancellationToken token = default)
            => Task.CompletedTask;
    }
}

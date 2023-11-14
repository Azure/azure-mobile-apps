// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;

namespace Microsoft.AspNetCore.Datasync.Extensions;

internal static class IAccessControlProviderExtensions
{
    /// <summary>
    /// Determines if the provided entity is in the view of the current user.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="provider">The <see cref="IAccessControlProvider{TEntity}"/> that controls access to entities.</param>
    /// <param name="entity">The entity being checked.</param>
    /// <returns><c>true</c> if the entity is in view; <c>false</c> otherwise.</returns>
    internal static bool EntityIsInView<TEntity>(this IAccessControlProvider<TEntity> provider, TEntity entity) where TEntity : ITableData
        => provider.GetDataView()?.Compile().Invoke(entity) != false;
}

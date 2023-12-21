// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Datasync.Extensions;

internal static class IQueryableExtensions
{
    /// <summary>
    /// Applies an optional filter to the queryable.
    /// </summary>
    /// <typeparam name="T">The type of entity being queried.</typeparam>
    /// <param name="query">The current <see cref="IQueryable{T}"/> representing the query.</param>
    /// <param name="predicate">The optional predicate to add.</param>
    /// <returns>An updated <see cref="IQueryable{T}"/> queryable.</returns>
    internal static IQueryable<T> ApplyDataView<T>(this IQueryable<T> query, Expression<Func<T, bool>>? predicate) where T : ITableData
        => predicate != null ? query.Where(predicate) : query;

    /// <summary>
    /// Applies the deleted filter if soft delete is enabled and the request does not include a request to include soft-deleted items.
    /// </summary>
    /// <typeparam name="T">The type of entity being queried.</typeparam>
    /// <param name="query">The current <see cref="IQueryable{T}"/> representing the query.</param>
    /// <param name="request">The current <see cref="HttpRequest"/></param>
    /// <param name="enableSoftDelete">If <c>true</c>, soft-delete is enabled.</param>
    /// <returns></returns>
    internal static IQueryable<T> ApplyDeletedView<T>(this IQueryable<T> query, HttpRequest request, bool enableSoftDelete) where T : ITableData
        => !enableSoftDelete || request.ShouldIncludeDeletedItems() ? query : query.Where(x => !x.Deleted);
}

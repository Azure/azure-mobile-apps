// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AzureMobile.Server.Extensions
{
    internal static class IQueryableExtensions
    {
        /// <summary>
        /// Applies an optional data view to the query.
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="query">The current query</param>
        /// <param name="predicate"></param>
        /// <returns>The modified query</returns>
        internal static IQueryable<T> ApplyDataView<T>(this IQueryable<T> query, Func<T, bool> predicate) where T : ITableData
            => predicate != null ? query.Where(e => predicate.Invoke(e)) : query;
    }
}

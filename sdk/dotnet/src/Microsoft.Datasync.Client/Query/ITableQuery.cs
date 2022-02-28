// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Datasync.Client.Query
{
    /// <summary>
    /// Represents a query that can be evaluated against an OData table.
    /// </summary>
    /// <remarks>
    /// Rather than implenting <see cref="IQueryable{T}"/> directly, we've implemented the
    /// portion of the LINQ query pattern we support on a datasync service.  You can use
    /// the <see cref="ITableQuery{T}"/> instance to build up a query using normal LINQ
    /// patterns.
    /// </remarks>
    public interface ITableQuery<T> : ILinqMethods<T>
    {
        /// <summary>
        /// The table being queried.
        /// </summary>
        IRemoteTable<T> RemoteTable { get; }

        /// <summary>
        /// The user-defined query string parameters to include with the query when
        /// sent to the remote service.
        /// </summary>
        IDictionary<string, string> Parameters { get; }

        /// <summary>
        /// The underlying <see cref="IQueryable{T}"/> associated with this query.
        /// </summary>
        IQueryable<T> Query { get; set; }

        /// <summary>
        /// If <c>true</c>, include the total count of items that will be returned with this query
        /// (without considering paging).
        /// </summary>
        bool RequestTotalCount { get; }
    }
}

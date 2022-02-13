// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Datasync.Client.Query
{
    /// <summary>
    /// A simplified LINQ-like query language for the Datasync service.  Since the datasync
    /// service does not support the extensive capabilities of LINQ, we only support the
    /// subset that OData v4 supports.
    /// </summary>
    /// <typeparam name="T">The type of entity being queried</typeparam>
    public interface ITableQuery<T> : ILinqMethods<T>
    {
        /// <summary>
        /// The <see cref="IQueryable{T}"/> representing the underlying LINQ query.
        /// </summary>
        IQueryable<T> Query { get; }

        /// <summary>
        /// The additional query parameters to be sent.
        /// </summary>
        IDictionary<string, string> QueryParameters { get; }

        /// <summary>
        /// The $skip component of the query.
        /// </summary>
        int SkipCount { get; }

        /// <summary>
        /// The table that is associated with this query.
        /// </summary>
        IRemoteTable<T> Table { get; }

        /// <summary>
        /// The $top component of the query.
        /// </summary>
        int TakeCount { get; }
    }
}

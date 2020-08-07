// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Zumo.Server
{
    /// <summary>
    /// Options to modify the bahavior of the table controller.
    /// </summary>
    /// <typeparam name="TEntity">The model type for entities served by the table controller</typeparam>
    public class TableControllerOptions<T> where T : class, ITableData
    {
        private const int DEFAULT_MAXTOP = 1000;
        private const int DEFAULT_PAGESIZE = 50;

        /// <summary>
        /// The predicate (LINQ Where modifier) used to limit what the requesting user can see of the dataset.
        /// </summary>
        public Func<T, bool> DataView { get; set; } = entity => true;

        /// <summary>
        /// When a record is deleted with soft-delete enabled, the Deleted flag is set - other operations will
        /// not operate on a deleted record.  This allows mobile clients to clean up deleted records.
        /// </summary>
        public bool SoftDeleteEnabled { get; set; } = false;

        /// <summary>
        /// The maximum value of $top that a user can submit in a query parameter
        /// </summary>
        public int MaxTop { get; set; } = DEFAULT_MAXTOP;

        /// <summary>
        /// The default page-size if the user doesn't specify $top.
        /// </summary>
        public int PageSize { get; set; } = DEFAULT_PAGESIZE;
    }
}

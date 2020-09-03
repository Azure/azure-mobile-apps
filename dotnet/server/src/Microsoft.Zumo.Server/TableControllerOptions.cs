// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Zumo.Server
{
    /// <summary>
    /// Options to modify the bahavior of the table controller.
    /// </summary>
    /// <typeparam name="TEntity">The model type for entities served by the table controller</typeparam>
    public class TableControllerOptions
    {
        /// <summary>
        /// When a record is deleted with soft-delete enabled, the Deleted flag is set - other operations will
        /// not operate on a deleted record.  This allows mobile clients to clean up deleted records.
        /// </summary>
        public bool SoftDeleteEnabled { get; set; } = false;
    }
}

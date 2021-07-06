// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// An exception that indicates a conflict.
    /// </summary>
    /// <typeparam name="T">The type of the entity causing the conflict</typeparam>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Specialized Exception")]
    public class DatasyncConflictException<T> : DatasyncOperationException
    {
        /// <summary>
        /// Creates a new <see cref="DatasyncConflictException{T}"/> using the provided information.
        /// </summary>
        /// <param name="ex">The <see cref="DatasyncOperationException"/> causing the error</param>
        /// <param name="entity">The entity causing the error</param>
        public DatasyncConflictException(DatasyncOperationException ex, T entity) : base(ex.Request, ex.Response)
        {
            ServerItem = entity;
        }

        public T ServerItem { get; }
    }
}

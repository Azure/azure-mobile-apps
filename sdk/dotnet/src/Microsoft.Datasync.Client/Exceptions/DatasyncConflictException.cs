// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Provides details of a HTTP <c>Conflict</c> or <c>Precondition Failed</c>
    /// response.
    /// </summary>
    [SuppressMessage("Roslynator", "RCS1194:Implement exception constructors.", Justification = "Specialty exception.")]
    public class DatasyncConflictException : DatasyncInvalidOperationException
    {
        public DatasyncConflictException(DatasyncInvalidOperationException source, JObject value)
            : base(source.Message, source.Request, source.Response, value)
        {
        }
    }

    /// <summary>
    /// Provides details of a HTTP <c>Conflict</c> or <c>Precondition Failed</c>
    /// response.
    /// </summary>
    [SuppressMessage("Roslynator", "RCS1194:Implement exception constructors.", Justification = "Specialty exception.")]
    public class DatasyncConflictException<T> : DatasyncConflictException
    {
        public DatasyncConflictException(DatasyncInvalidOperationException source, T item) : base(source, source.Value)
        {
            Item = item;
        }

        /// <summary>
        /// The current instance from the server that the precondition failed for.
        /// </summary>
        public T Item { get; }
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Net.Http;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// An exception thrown when the entity being retrieved has not been modified since it was
    /// last read.
    /// </summary>
    public class EntityNotModifiedException : DatasyncOperationException
    {
        /// <inheritdoc />
        internal EntityNotModifiedException(HttpRequestMessage request, HttpResponseMessage response)
            : base(request, response)
        {
        }
    }
}

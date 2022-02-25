// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Provides details of a HTTP <c>Conflict</c> or <c>Precondition Failed</c>
    /// response.
    /// </summary>
    public class DatasyncConflictException : DatasyncInvalidOperationException
    {
        public DatasyncConflictException(DatasyncInvalidOperationException source, JObject value)
            : base(source.Message, source.Request, source.Response, value)
        {
        }
    }
}

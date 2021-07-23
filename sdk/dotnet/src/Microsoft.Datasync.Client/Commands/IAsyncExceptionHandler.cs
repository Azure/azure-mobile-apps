// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Commands
{
    /// <summary>
    /// Definition of the user-provided exception handler for async commands.
    /// </summary>
    public interface IAsyncExceptionHandler
    {
        void OnAsyncException(Exception ex);
    }
}

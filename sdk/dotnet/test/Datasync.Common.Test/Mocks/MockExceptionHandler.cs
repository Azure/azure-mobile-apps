// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Datasync.Common.Test.Mocks
{
    /// <summary>
    /// A test exception handler that just stores the exceptions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MockExceptionHandler : IAsyncExceptionHandler
    {
        public List<Exception> Received = new();

        public void OnAsyncException(Exception ex)
        {
            Received.Add(ex);
        }
    }
}

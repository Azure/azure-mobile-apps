// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Commands;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Test.Commands
{
    /// <summary>
    /// A wrapped version of <see cref="AsyncCommand"/> that provides access
    /// to the underlying (normally protected) methods.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class WrappedAsyncCommand : AsyncCommand
    {
        public WrappedAsyncCommand(Func<Task> execute, Func<bool> canExecute = null, IAsyncExceptionHandler handler = null)
            : base(execute, canExecute, handler)
        {
        }

        public Func<Task> WrappedExecuteFunc { get => base.ExecuteFunc; }
        public Func<bool> WrappedCanExecuteFunc { get => base.CanExecuteFunc; }
        public IAsyncExceptionHandler WrappedExceptionHandler { get => base.ExceptionHandler; }
    }
}

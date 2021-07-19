// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Datasync.Client.Commands
{
    /// <summary>
    /// An async version of <see cref="ICommand"/>.
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        /// <summary>
        /// Execute the command
        /// </summary>
        Task ExecuteAsync();

        /// <summary>
        /// True if the command can be executed.
        /// </summary>
        bool CanExecute();
    }
}

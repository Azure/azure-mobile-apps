// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Datasync.Client.Commands
{
    /// <summary>
    /// An implementation of the <see cref="IAsyncCommand"/> interface, used for
    /// commands when doing lazy paged loading.
    /// </summary>
    public class AsyncCommand : IAsyncCommand
    {
        /// <summary>
        /// An event handler that is triggered when <see cref="CanExecute"/> is changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        private bool _isExecuting;
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private IAsyncExceptionHandler _handler;

        /// <summary>
        /// Creates a new <see cref="AsyncCommand"/>
        /// </summary>
        /// <param name="execute">The function to execute asynchronously</param>
        /// <param name="canExecute">A boolean indicating whether it is ok to execute</param>
        public AsyncCommand(Func<Task> execute, Func<bool> canExecute = null, IAsyncExceptionHandler handler = null)
        {
            Validate.IsNotNull(execute, nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
            _handler = handler;
        }

        /// <summary>
        /// Determines if the execution can happen.
        /// </summary>
        /// <returns>true if the execution can happen</returns>
        public bool CanExecute()
            => !_isExecuting && (_canExecute?.Invoke() ?? true);

        /// <summary>
        /// Executes the task.
        /// </summary>
        public async Task ExecuteAsync()
        {
            if (CanExecute())
            {
                try
                {
                    _isExecuting = true;
                    await _execute();
                }
                finally
                {
                    _isExecuting = false;
                }
            }

            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region ICommand
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        bool ICommand.CanExecute(object parameter) => CanExecute();

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter"></param>
        void ICommand.Execute(object parameter) => ExecuteAsync().FireAndForgetSafeAsync(_handler);
        #endregion
    }
}

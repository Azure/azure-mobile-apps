// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
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
        private bool _isExecuting;

        /// <summary>
        /// An event handler that is triggered when <see cref="CanExecute"/> is changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// The Executor Function
        /// </summary>
        protected Func<Task> ExecuteFunc { get; }

        /// <summary>
        /// The CanExecute Function
        /// </summary>
        protected Func<bool> CanExecuteFunc { get; }

        /// <summary>
        /// The error handler.
        /// </summary>
        protected IAsyncExceptionHandler ExceptionHandler { get; }

        /// <summary>
        /// Creates a new <see cref="AsyncCommand"/>
        /// </summary>
        /// <param name="execute">The function to execute asynchronously</param>
        /// <param name="canExecute">A boolean indicating whether it is OK to execute</param>
        /// <param name="handler">The exception handler</param>
        public AsyncCommand(Func<Task> execute, Func<bool> canExecute = null, IAsyncExceptionHandler handler = null)
        {
            Validate.IsNotNull(execute, nameof(execute));

            ExecuteFunc = execute;
            CanExecuteFunc = canExecute;
            ExceptionHandler = handler;
        }

        /// <summary>
        /// Determines if the execution can happen.
        /// </summary>
        /// <returns>true if the execution can happen</returns>
        public bool CanExecute()
            => !_isExecuting && (CanExecuteFunc?.Invoke() ?? true);

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
                    await ExecuteFunc();
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
        void ICommand.Execute(object parameter) => ExecuteAsync().FireAndForgetSafeAsync(ExceptionHandler);
        #endregion
    }
}

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using System.Windows;
using TodoApp.Data;
using TodoApp.Data.MVVM;
using TodoApp.Data.Services;

namespace TodoApp.WPF
{
    /// <summary>
    /// Entry point for the application.
    /// </summary>
    public partial class App : Application, IMVVMHelper
    {
        public static ITodoService TodoService { get; } = new RemoteTodoService();

        #region IMVVMHelper
        /// <summary>
        /// Runs the associated code on the UI thread so that the UI can
        /// be updated correctly.
        /// </summary>
        /// <param name="func">The code to be run</param>
        /// <returns>A task the completes when the code is completed.</returns>
        public Task RunOnUiThreadAsync(Action func)
        {
            Dispatcher.Invoke(func);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Displays a pop-up alert showing an error condition.
        /// </summary>
        /// <param name="title">The title of the alert.</param>
        /// <param name="message">The content of the alert.</param>
        /// <returns>A task that completes when the user acknowledges the error.</returns>
        public Task DisplayErrorAlertAsync(string title, string message)
        {
            Dispatcher.Invoke(() => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error));
            return Task.CompletedTask;
        }
        #endregion
    }
}

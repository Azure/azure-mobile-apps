// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace TodoApp.Data.MVVM
{
    /// <summary>
    /// To properly do cross-platform MVVM, we sometimes need to indicate that code
    /// is run on the UI thread.  This interface allows platform-specific implementations
    /// of that.
    /// </summary>
    public interface IMVVMHelper
    {
        /// <summary>
        /// Runs the associated code on the UI thread so that the UI can
        /// be updated correctly.
        /// </summary>
        /// <param name="func">The code to be run</param>
        /// <returns>A task the completes when the code is completed.</returns>
        Task RunOnUiThreadAsync(Action func);

        /// <summary>
        /// Displays a pop-up alert showing an error condition.
        /// </summary>
        /// <param name="title">The title of the alert.</param>
        /// <param name="message">The content of the alert.</param>
        /// <returns>A task that completes when the user acknowledges the error.</returns>
        Task DisplayErrorAlertAsync(string title, string message);
    }
}

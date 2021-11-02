// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Windows;

namespace TodoApp.WPF
{
    /// <summary>
    /// Entry point for the application.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Runs the specified code on the UI thread, which is needed
        /// when you want to update the UI.
        /// </summary>
        /// <param name="func">The code to run.</param>
        internal static void RunOnUiThread(Action func)
            => App.Current.Dispatcher.Invoke(func);
    }
}

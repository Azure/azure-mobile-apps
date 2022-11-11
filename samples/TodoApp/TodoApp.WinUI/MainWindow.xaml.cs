// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Data.MVVM;
using TodoApp.Data.Services;
using Windows.Graphics;
using Windows.System;

namespace TodoApp.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainWindow : Window, IMVVMHelper
    {
        private readonly TodoListViewModel _viewModel;
        private readonly ITodoService _service;

        public MainWindow()
        {
            this.InitializeComponent();
            ResizeWindow(this, 480, 800);

            _service = new RemoteTodoService();
            _viewModel = new TodoListViewModel(this, _service);
            mainContainer.DataContext = _viewModel;
        }

        private static void ResizeWindow(Window window, int width, int height)
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            var size = new SizeInt32(width, height);
            appWindow.Resize(size);
        }

        #region IMVVMHelper
        public Task RunOnUiThreadAsync(Action func)
        {
            DispatcherQueue.TryEnqueue(() => func());
            return Task.CompletedTask;
        }

        public async Task DisplayErrorAlertAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };
            dialog.XamlRoot = Content.XamlRoot;
            await dialog.ShowAsync();
        }
        #endregion

        #region Event Handlers

        protected void GridLoadedEventHandler(object sender, RoutedEventArgs e) 
        {
            _viewModel.OnActivated();
        }
        
        protected void TextboxKeyDownHandler(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                AddItemToList();
            }
        }

        /// <summary>
        /// Event handler that is triggered when the Add Item button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void AddItemClickHandler(object sender, RoutedEventArgs e)
        {
            AddItemToList();
        }

        private async void AddItemToList()
        {
            await _viewModel.AddItemAsync(textboxControl.Text.Trim());
            await RunOnUiThreadAsync(() => textboxControl.Text = String.Empty);
        }

        /// <summary>
        /// Event handler that is triggered when the check box next to an item is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void CheckboxClickHandler(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb)
            {
                await _viewModel.UpdateItemAsync(cb.Tag as string, cb.IsChecked ?? false);
            }
        }

        /// <summary>
        /// Event handler that is triggered when the Refresh Items button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void RefreshItemsClickHandler(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshItemsAsync();
        }
        #endregion
    }
}

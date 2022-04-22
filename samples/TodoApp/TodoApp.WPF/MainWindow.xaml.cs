// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TodoApp.Data.MVVM;

namespace TodoApp.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TodoListViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new TodoListViewModel(App.Current as IMVVMHelper, App.TodoService);
            DataContext = _viewModel;
        }

        /// <summary>
        /// Event handler called when the window is activated.  We need to
        /// pass this to the view model for data refresh.
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _viewModel.OnActivated();
        }

        /// <summary>
        /// Event handler that is called when you press a key in the input box.  It is used
        /// for adding an item when you press enter.
        /// </summary>
        protected async void TextboxKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                await _viewModel.AddItemAsync(textboxControl.Text.Trim()).ConfigureAwait(false);
                ClearTextBox();
            }
        }

        /// <summary>
        /// Event handler called when the user clicks on the + sign to add an item.
        /// </summary>
        protected async void AddItemClickHandler(object sender, RoutedEventArgs e)
        {
            await _viewModel.AddItemAsync(textboxControl.Text.Trim()).ConfigureAwait(false);
            ClearTextBox();
        }

        /// <summary>
        /// Event handler called when the user checks or unchecks an item.
        /// </summary>
        protected async void CheckboxClickHandler(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.Tag is string itemId)
            {
                bool isComplete = checkbox.IsChecked ?? false;
                await _viewModel.UpdateItemAsync(itemId, isComplete).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Event handler called when the user clicks the refresh items button.
        /// </summary>
        protected async void RefreshItemsClickHandler(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshItemsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Clears the content from the text box.
        /// </summary>
        private void ClearTextBox() => App.Current.Dispatcher.Invoke(() => textboxControl.Text = String.Empty);
    }
}

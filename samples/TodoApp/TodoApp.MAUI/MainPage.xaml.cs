// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using TodoApp.Data;
using TodoApp.Data.Models;
using TodoApp.Data.MVVM;
using TodoApp.Data.Services;

namespace TodoApp.MAUI
{
    public partial class MainPage : ContentPage, IMVVMHelper
    {
        private readonly MainViewModel viewModel;

        public ITodoService TodoService { get; }

        public MainPage()
        {
            InitializeComponent();
            TodoService = new RemoteTodoService();
            viewModel = new MainViewModel(this, TodoService);
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.OnActivated();
        }

        public void OnListItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is TodoItem item)
            {
                Debug.WriteLine($"[UI] >>> Item clicked: {item.Id}");
                viewModel.SelectItemCommand.Execute(item);
            }
            if (sender is ListView itemList)
            {
                itemList.SelectedItem = null;
            }
        }

        #region IMVVMHelper
        public Task RunOnUiThreadAsync(Action func)
            => MainThread.InvokeOnMainThreadAsync(func);

        public Task DisplayErrorAlertAsync(string title, string message)
            => RunOnUiThreadAsync(async () => await DisplayAlert(title, message, "OK"));
        #endregion
    }
}
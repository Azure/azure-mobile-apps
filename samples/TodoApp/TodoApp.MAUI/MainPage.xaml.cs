// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using TodoApp.Data;
using TodoApp.Data.MVVM;
using TodoApp.Data.Services;

namespace TodoApp.MAUI
{
    public partial class MainPage : ContentPage, IMVVMHelper
    {
        private readonly MainViewModel model;
        private readonly ITodoService todoService;

        public MainPage()
        {
            InitializeComponent();
            todoService = new RemoteTodoService();
            model = new MainViewModel(this, todoService);
            BindingContext = model;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            model.OnActivated();
        }

        public void OnListItemTapped(object sender, ItemTappedEventArgs e)
        {
            model.SelectItemCommand.Execute(e.Item);
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
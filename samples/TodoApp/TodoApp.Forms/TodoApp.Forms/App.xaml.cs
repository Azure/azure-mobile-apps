// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Data.MVVM;
using TodoApp.Data.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TodoApp.Forms
{
    public partial class App : Application, IMVVMHelper
    {
        public ITodoService TodoService { get; }

        public App()
        {
            InitializeComponent();
            TodoService = new RemoteTodoService();
            MainPage = new NavigationPage(new MainPage(this, TodoService));
        }

        #region IMVVMHelper
        public Task RunOnUiThreadAsync(Action func)
            => MainThread.InvokeOnMainThreadAsync(func);

        public Task DisplayErrorAlertAsync(string title, string message)
            => RunOnUiThreadAsync(async () => await Current.MainPage.DisplayAlert(title, message, "OK"));
        #endregion

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}

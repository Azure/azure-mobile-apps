// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using TodoApp.Data;
using TodoApp.Data.MVVM;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TodoApp.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel model;

        public MainPage(IMVVMHelper helper, ITodoService service)
        {
            InitializeComponent();
            model = new MainViewModel(helper, service);
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
    }
}
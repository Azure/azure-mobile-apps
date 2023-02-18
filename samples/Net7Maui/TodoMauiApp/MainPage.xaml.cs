using System.Diagnostics;
using TodoMauiApp.Models;
using TodoMauiApp.MVVM;
using TodoMauiApp.Services;
using TodoMauiApp.ViewModels;

namespace TodoMauiApp;

public partial class MainPage : ContentPage, IMVVMHelper
{
    private readonly MainViewModel viewModel;

    public MainPage()
    {
        InitializeComponent();

        Uri serviceUri = new("https://todoappservicenet620230203152451.azurewebsites.net");
        TodoService = new RemoteTodoService(serviceUri)
        {
            OfflineDb = FileSystem.CacheDirectory + "/offline.db"
        };
        viewModel = new MainViewModel(this, TodoService);
        BindingContext = viewModel;
    }

    internal ITodoService TodoService { get; }

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
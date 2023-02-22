using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Threading.Tasks;
using TodoApp.Data.Models;
using TodoApp.Data.MVVM;
using TodoApp.Data.Services;
using TodoApp.Data;
using System.Windows.Input;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TodoApp.Uno;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page, IMVVMHelper
{
    private readonly MainViewModel viewModel;

    public ITodoService TodoService { get; }

    public MainPage()
    {
		this.InitializeComponent();
        TodoService = new RemoteTodoService();
        viewModel = new MainViewModel(this, TodoService);
        DataContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        viewModel.OnActivated();
    }

    //public void OnListItemTapped(object sender, ItemTappedEventArgs e)
    //{
    //    if (e.Item is TodoItem item)
    //    {
    //        Debug.WriteLine($"[UI] >>> Item clicked: {item.Id}");
    //        viewModel.SelectItemCommand.Execute(item);
    //    }
    //    if (sender is ListView itemList)
    //    {
    //        itemList.SelectedItem = null;
    //    }
    //}

    #region IMVVMHelper
    public async Task RunOnUiThreadAsync(Action func)
        => this.DispatcherQueue.TryEnqueue(()=>func());

    public Task DisplayErrorAlertAsync(string title, string message)
        => RunOnUiThreadAsync(async () =>
        {
            var dialog = new MessageDialog(title, message);
            await dialog.ShowAsync();
        });
    #endregion
}

public class MainViewModel : TodoListViewModel
{
    public MainViewModel(IMVVMHelper helper, ITodoService service) : base(helper, service)
    {
    }

    //public ICommand AddItemCommand
    //    => new Command<Entry>(async (Entry entry) => await AddItemAsync(entry.Text));

    //public ICommand RefreshItemsCommand
    //    => new Command(async () => await RefreshItemsAsync());

    //public ICommand SelectItemCommand
    //    => new Command<TodoItem>(async (TodoItem item) => await UpdateItemAsync(item.Id, !item.IsComplete));
}
using Microsoft.Datasync.Client;
using System.Windows.Input;
using TodoMauiApp.Models;
using TodoMauiApp.MVVM;
using TodoMauiApp.Services;

namespace TodoMauiApp.ViewModels;

internal class MainViewModel : ViewModel
{
    private readonly IMVVMHelper _helper;
    private readonly ITodoService _service;
    private bool _isRefreshing = false;

    
    public MainViewModel(IMVVMHelper helper, ITodoService service)
    {
        _helper = helper;
        _service = service;
    }

    public ICommand AddItemCommand
        => new Command<Entry>(async (Entry entry) => await AddItemAsync(entry.Text));

    public ICommand RefreshItemsCommand
        => new Command(async () => await RefreshItemsAsync());

    public ICommand SelectItemCommand
        => new Command<TodoItem>(async (TodoItem item) => await UpdateItemAsync(item.Id, !item.IsComplete));

    public ConcurrentObservableCollection<TodoItem> Items { get; } = new();

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value, nameof(IsRefreshing));
    }

    public async void OnActivated()
    {
        await RefreshItemsAsync();
        _service.TodoItemsUpdated += OnTodoItemsUpdated;
    }

    public async Task RefreshItemsAsync()
    {
        if (IsRefreshing) return;
        await SetRefreshing(true);

        try
        {
            await _service.RefreshItemsAsync();
            var items = await _service.GetItemsAsync();
            await _helper.RunOnUiThreadAsync(() =>
            {
                Items.ReplaceAll(items);
            });
        }
        catch (Exception ex)
        {
            await _helper.DisplayErrorAlertAsync("RefreshItems", ex.Message);
        }
        finally
        {
            await SetRefreshing(false);
        }
    }

    public async Task UpdateItemAsync(string itemId, bool isComplete)
    {
        try
        {
            var item = Items.Single(m => m.Id == itemId);
            item.IsComplete = isComplete;
            await _service.SaveItemAsync(item);
        }
        catch (Exception ex)
        {
            await _helper.DisplayErrorAlertAsync("UpdateItem", ex.Message);
        }
    }

    public async Task AddItemAsync(string text)
    {
        try
        {
            var item = new TodoItem { Title = text };
            await _service.SaveItemAsync(item);
        }
        catch (Exception ex)
        {
            await _helper.DisplayErrorAlertAsync("AddItem", ex.Message);
        }
    }

    private async void OnTodoItemsUpdated(object sender, TodoServiceEventArgs e)
    {
        await _helper.RunOnUiThreadAsync(() =>
        {
            switch (e.Action)
            {
                case TodoServiceEventArgs.ListAction.Add:
                    Items.AddIfMissing(m => m.Id == e.Item.Id, e.Item);
                    break;
                case TodoServiceEventArgs.ListAction.Delete:
                    Items.RemoveIf(m => m.Id == e.Item.Id);
                    break;
                case TodoServiceEventArgs.ListAction.Update:
                    Items.ReplaceIf(m => m.Id == e.Item.Id, e.Item);
                    break;
            }
        });
    }

    private Task SetRefreshing(bool value)
        => _helper.RunOnUiThreadAsync(() => IsRefreshing = value);
}

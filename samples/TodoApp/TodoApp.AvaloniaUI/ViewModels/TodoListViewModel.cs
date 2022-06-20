using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Data.Extensions;
using TodoApp.Data.Models;

namespace TodoApp.AvaloniaUI.ViewModels
{
    public class TodoListViewModel : ViewModelBase
    {
        private readonly ITodoService _service;
        private bool _isRefreshing;
        private string _addItemTitle = "";

        public TodoListViewModel(ITodoService service)
        {
            _service = service;

            Debug.WriteLine("CTOR[TodoListViewModel]");
            Task.Run(async () => await OnActivated());
        }

        /// <summary>
        /// The list of items.
        /// </summary>
        public ObservableCollection<TodoItem> Items { get; } = new ObservableCollection<TodoItem>();

        /// <summary>
        /// True if the service is refreshing the data.
        /// </summary>
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => this.RaiseAndSetIfChanged(ref _isRefreshing, value);
        }

        /// <summary>
        /// The text in the Add Item box.
        /// </summary>
        public string AddItemTitle
        {
            get => _addItemTitle;
            set => this.RaiseAndSetIfChanged(ref _addItemTitle, value);
        }

        /// <summary>
        /// External event handler for when the page is first displayed.
        /// </summary>
        public async Task OnActivated()
        {
            Debug.WriteLine("In OnActivated");
            await RefreshItemsAsync();
            _service.TodoItemsUpdated += OnTodoItemsUpdated;
        }

        /// <summary>
        /// Command for refreshing the items.  This will synchronize the backend.
        /// </summary>
        /// <returns>A task that completes when the sync is done.</returns>
        public async Task RefreshItemsAsync()
        {
            Debug.WriteLine("In RefreshItemsAsync");
            IsRefreshing = true;
            try
            {
                // Do any database service refreshing needed
                await _service.RefreshItemsAsync();

                // Get the current list of items.
                var items = await _service.GetItemsAsync();

                Items.Clear();
                foreach (var item in items)
                {
                    Items.Add(item);
                    Debug.WriteLine($"Adding item {item.Id}:{item.Title}");
                }
            }
            catch (Exception ex)
            {
                await DisplayErrorAlertAsync("RefreshItems", ex.Message);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        /// <summary>
        /// Command for updating an item.
        /// </summary>
        /// <param name="itemId">The Id of the item to update</param>
        /// <param name="isComplete">The new value of the completion flag.</param>
        /// <returns>A task that completes when the update is done.</returns>
        public virtual async Task UpdateItemAsync(string itemId, bool isComplete)
        {
            try
            {
                var item = Items.Single(m => m.Id == itemId);
                item.IsComplete = isComplete;
                await _service.SaveItemAsync(item);
            }
            catch (Exception ex)
            {
                await DisplayErrorAlertAsync("UpdateItem", ex.Message);
            }
        }

        /// <summary>
        /// Command for adding an item.
        /// </summary>
        /// <param name="text">The value of the text box.</param>
        /// <returns>A task that completes when the addition is done.</returns>
        public virtual async Task AddItemAsync(string text)
        {
            try
            {
                var item = new TodoItem { Title = text };
                await _service.SaveItemAsync(item);
            }
            catch (Exception ex)
            {
                await DisplayErrorAlertAsync("UpdateItem", ex.Message);
            }
        }

        /// <summary>
        /// Event handler that maintains the <see cref="ObservableCollection{TodoItem}"/> so that
        /// it is in sync with the store.
        /// </summary>
        /// <param name="sender">The service that sent the event</param>
        /// <param name="e">The event arguments</param>
        private void OnTodoItemsUpdated(object? sender, TodoServiceEventArgs e)
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
        }

        public async Task DisplayErrorAlertAsync(string title, string message)
        {
            Debug.WriteLine($"Error: {title} {message}");
            throw new NotImplementedException();
        }
    }
}

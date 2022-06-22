using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Data.Extensions;
using TodoApp.Data.Models;

namespace TodoApp.AvaloniaUI.ViewModels
{
    public class TodoListViewModel : ViewModelBase, IActivatableViewModel
    {
        private readonly ITodoService _service;
        private bool _isRefreshing, _isShowingDialog;
        private string _addItemTitle = "", _dialogTitle = "", _dialogMessage = "";

        public TodoListViewModel(ITodoService service)
        {
            _service = service;

            Debug.WriteLine("CTOR[TodoListViewModel]");
            Activator = new ViewModelActivator();
            this.WhenActivated((CompositeDisposable disposables) =>
            {
                Task.Run(async () => await OnActivated());
                Disposable.Create(() => { /* handle deactivation */ }).DisposeWith(disposables);
            });

            AddItemCommand = ReactiveCommand.CreateFromTask(() => AddItemAsync());
            RefreshItemsCommand = ReactiveCommand.CreateFromTask(() => RefreshItemsAsync());
            UpdateItemCommand = ReactiveCommand.CreateFromTask((string id) => UpdateItemAsync(id));
        }

        /// <summary>
        /// The handler for the IActivatableViewModel
        /// </summary>
        public ViewModelActivator Activator { get; }


        /// <summary>
        /// The list of items.
        /// </summary>
        public ObservableCollection<TodoItem> Items { get; } = new ObservableCollection<TodoItem>();

        /// <summary>
        /// Command for the Add Item button.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddItemCommand { get; } 

        /// <summary>
        /// Command for the Refresh Items button.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RefreshItemsCommand { get; }

        /// <summary>
        /// Command for updating an item.
        /// </summary>
        public ReactiveCommand<string, Unit> UpdateItemCommand { get; }

        /// <summary>
        /// True if the service is refreshing the data.
        /// </summary>
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => this.RaiseAndSetIfChanged(ref _isRefreshing, value);
        }

        /// <summary>
        /// True if the UI is showing a dialog.
        /// </summary>
        public bool IsShowingDialog
        {
            get => _isShowingDialog;
            set => this.RaiseAndSetIfChanged(ref _isShowingDialog, value);
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
        /// The dialog title
        /// </summary>
        public string DialogTitle
        {
            get => _dialogTitle;
            set => this.RaiseAndSetIfChanged(ref _dialogTitle, value);
        }

        /// <summary>
        /// The dialog message
        /// </summary>
        public string DialogMessage
        {
            get => _dialogMessage;
            set => this.RaiseAndSetIfChanged(ref _dialogMessage, value);
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
            Debug.WriteLine("RefreshItemsAsync");
            IsRefreshing = true;
            try
            {
                // Do any database service refreshing needed
                await _service.RefreshItemsAsync();

                // Get the current list of items.
                var items = await _service.GetItemsAsync();

                Items.Clear();
                Items.AddRange(items);
            }
            catch (Exception ex)
            {
                DisplayErrorAlert("RefreshItems", ex.Message);
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
        public virtual async Task UpdateItemAsync(string itemId)
        {
            try
            {
                var item = Items.Single(m => m.Id == itemId);
                Debug.WriteLine($"Item = {item.Id} / title = \"{item.Title}\" iscomplete = {item.IsComplete}");
                await _service.SaveItemAsync(item);
            }
            catch (Exception ex)
            {
                DisplayErrorAlert("UpdateItem", ex.Message);
            }
        }

        /// <summary>
        /// Command for adding an item.
        /// </summary>
        /// <returns>A task that completes when the addition is done.</returns>
        public virtual async Task AddItemAsync()
        {
            string text = AddItemTitle;
            Debug.WriteLine($"Calling AddItemAsync with text = {text}");
            try
            {
                var item = new TodoItem { Title = text };
                await _service.SaveItemAsync(item);
            }
            catch (Exception ex)
            {
                DisplayErrorAlert("UpdateItem", ex.Message);
            }
            finally
            {
                AddItemTitle = "";
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

        public void DisplayErrorAlert(string title, string message)
        {
            Debug.WriteLine($"Error: {title} {message}");
            DialogTitle = title;
            DialogMessage = message;
            IsShowingDialog = true;
        }

        public void RemoveDialog()
        {
            Debug.WriteLine($"Removing error dialog");
            IsShowingDialog = false;
        }
    }
}

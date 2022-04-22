// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Data.Extensions;
using TodoApp.Data.Models;

namespace TodoApp.Data.MVVM
{
    public class TodoListViewModel : ViewModel
    {
        /// <summary>
        /// The <see cref="ITodoService"/> singleton that is injected.
        /// </summary>
        private readonly ITodoService _service;

        /// <summary>
        /// The <see cref="IMVVMHelper"/> we are using.  This allows us to update
        /// the UI correctly and display alerts.
        /// </summary>
        private readonly IMVVMHelper _helper;

        /// <summary>
        /// The backing store for the <see cref="IsRefreshing"/> property.
        /// </summary>
        private bool _isRefreshing = false;

        /// <summary>
        /// Create a new <see cref="MainWindowViewModel"/>.
        /// </summary>
        /// <param name="service">The injected <see cref="ITodoService"/> singleton.</param>
        public TodoListViewModel(IMVVMHelper mvvmHelper, ITodoService service)
        {
            _service = service;
            _helper = mvvmHelper;
        }

        /// <summary>
        /// External event handler for when the page is first displayed.
        /// </summary>
        public async void OnActivated()
        {
            await RefreshItemsAsync().ConfigureAwait(false);
            _service.TodoItemsUpdated += OnTodoItemsUpdated;
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
            set => SetProperty(ref _isRefreshing, value, nameof(IsRefreshing));
        }

        /// <summary>
        /// Command for refreshing the items.  This will synchronize the backend.
        /// </summary>
        /// <returns>A task that completes when the sync is done.</returns>
        public virtual async Task RefreshItemsAsync()
        {
            await SetRefreshing(true).ConfigureAwait(false);
            try
            {
                // Do any database service refreshing needed
                await _service.RefreshItemsAsync().ConfigureAwait(false);

                // Get the current list of items.
                var items = await _service.GetItemsAsync().ConfigureAwait(false);

                // Clear the list and then add each item from the service in turn.
                // This has to be done via the UI thread.
                await _helper.RunOnUiThreadAsync(() =>
                {
                    Items.Clear();
                    foreach (var item in items)
                    {
                        Items.Add(item);
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _helper.DisplayErrorAlertAsync("RefreshItems", ex.Message).ConfigureAwait(false);
            }
            finally
            {
                await SetRefreshing(false).ConfigureAwait(false);
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
                await _service.SaveItemAsync(item).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _helper.DisplayErrorAlertAsync("UpdateItem", ex.Message).ConfigureAwait(false);
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
                await _service.SaveItemAsync(item).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _helper.DisplayErrorAlertAsync("UpdateItem", ex.Message).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Event handler that maintains the <see cref="ObservableCollection{TodoItem}"/> so that
        /// it is in sync with the store.
        /// </summary>
        /// <param name="sender">The service that sent the event</param>
        /// <param name="e">The event arguments</param>
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
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// The IsRefreshing needs to be set on the UI thread in some cases (e.g. UWP).
        /// </summary>
        /// <param name="value">The new value</param>
        private Task SetRefreshing(bool value)
            => _helper.RunOnUiThreadAsync(() => IsRefreshing = value);
    }
}

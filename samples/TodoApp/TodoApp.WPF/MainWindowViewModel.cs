// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TodoApp.Data;
using TodoApp.Data.Extensions;
using TodoApp.Data.Models;
using TodoApp.Data.MVVM;

namespace TodoApp.WPF
{
    /// <summary>
    /// The viewmodel for the <see cref="MainWindow"/> view.
    /// </summary>
    public class MainWindowViewModel : ViewModel
    {
        /// <summary>
        /// The <see cref="ITodoService"/> singleton that is injected.
        /// </summary>
        private readonly ITodoService _service;

        /// <summary>
        /// The backing store for the <see cref="IsRefreshing"/> property.
        /// </summary>
        private bool _isRefreshing = false;

        /// <summary>
        /// Create a new <see cref="MainWindowViewModel"/>.
        /// </summary>
        /// <param name="service">The injected <see cref="ITodoService"/> singleton.</param>
        public MainWindowViewModel(ITodoService service)
        {
            _service = service;
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
        /// The list of items.  The service exposes this as an <see cref="ObservableCollection{TodoItem}"/>
        /// already, and we never change it.
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
        public async Task RefreshItemsAsync()
        {
            IsRefreshing = true;
            try
            {
                await _service.RefreshItemsAsync().ConfigureAwait(false);
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
        public async Task UpdateItemAsync(string itemId, bool isComplete)
        {
            try
            {
                var item = Items.Single(m => m.Id == itemId);
                item.IsComplete = isComplete;
                await _service.SaveItemAsync(item).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                DisplayErrorAlert("UpdateItem", ex.Message);
            }
        }

        /// <summary>
        /// Command for adding an item.
        /// </summary>
        /// <param name="text">The value of the text box.</param>
        /// <returns>A task that completes when the addition is done.</returns>
        public async Task AddItemAsync(string text)
        {
            try
            {
                var item = new TodoItem { Title = text };
                await _service.SaveItemAsync(item).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                DisplayErrorAlert("UpdateItem", ex.Message);
            }
        }

        /// <summary>
        /// Displays a pop-up alert for the user.
        /// </summary>
        /// <param name="title">The title of the message box</param>
        /// <param name="message">The message</param>
        private static void DisplayErrorAlert(string title, string message)
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);

        /// <summary>
        /// Event handler that maintains the <see cref="ObservableCollection{TodoItem}"/> so that
        /// it is in sync with the store.
        /// </summary>
        /// <param name="sender">The service that sent the event</param>
        /// <param name="e">The event arguments</param>
        private void OnTodoItemsUpdated(object? sender, TodoServiceEventArgs e)
        {
            App.RunOnUiThread(() =>
            {
                switch (e.Action)
                {
                    case TodoServiceEventArgs.ListAction.Add:
                        Items.Add(e.Item);
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
    }
}

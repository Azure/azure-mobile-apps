// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Data.Models;

namespace TodoApp.Data.Services
{
    /// <summary>
    /// An implementation of the <see cref="ITodoService"/> interface that implements an in-memory
    /// service.
    /// </summary>
    public class InMemoryTodoService : ITodoService
    {
        private readonly List<TodoItem> _items = new List<TodoItem>();

        /// <summary>
        /// An event handler that is triggered when the list of items changes.
        /// </summary>
        public event EventHandler<TodoServiceEventArgs> TodoItemsUpdated;

        /// <summary>
        /// Get all the items in the list.
        /// </summary>
        /// <returns>The list of items (asynchronously)</returns>
        public Task<IEnumerable<TodoItem>> GetItemsAsync() => Task.FromResult((IEnumerable<TodoItem>)_items);

        /// <summary>
        /// Refreshes the TodoItems list manually.
        /// </summary>
        /// <returns>A task that completes when the refresh is done.</returns>
        public Task RefreshItemsAsync() => Task.CompletedTask;

        /// <summary>
        /// Removes an item in the list, if it exists.
        /// </summary>
        /// <param name="item">The item to be removed</param>
        /// <returns>A task that completes when the item is removed.</returns>
        public Task RemoveItemAsync(TodoItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.Id != null)
            {
                var itemToRemove = _items.SingleOrDefault(m => m.Id == item.Id);
                if (itemToRemove != null)
                {
                    _items.Remove(itemToRemove);
                    TodoItemsUpdated?.Invoke(this, new TodoServiceEventArgs(TodoServiceEventArgs.ListAction.Delete, itemToRemove));
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves an item to the list.  If the item does not have an Id, then the item
        /// is considered new and will be added to the end of the list.  Otherwise, the
        /// item is considered existing and is replaced.
        /// </summary>
        /// <param name="item">The new item</param>
        /// <returns>A task that completes when the item is saved.</returns>
        public Task SaveItemAsync(TodoItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.Version = Guid.NewGuid().ToString();
            item.UpdatedAt = DateTimeOffset.UtcNow;

            if (item.Id == null)
            {
                item.Id = Guid.NewGuid().ToString();
                _items.Add(item);
                TodoItemsUpdated?.Invoke(this, new TodoServiceEventArgs(TodoServiceEventArgs.ListAction.Add, item));
            }
            else
            {
                var itemToReplace = _items.SingleOrDefault(m => m.Id == item.Id);
                if (itemToReplace != null)
                {
                    var idx = _items.IndexOf(itemToReplace);
                    _items[idx] = item;
                    TodoItemsUpdated?.Invoke(this, new TodoServiceEventArgs(TodoServiceEventArgs.ListAction.Update, item));
                }
            }

            return Task.CompletedTask;
        }
    }
}

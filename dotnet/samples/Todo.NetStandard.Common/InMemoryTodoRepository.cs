using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Todo.NetStandard.Common
{
    public class InMemoryTodoRepository : ITodoRepository
    {
        /// <summary>
        /// The internal list of items.
        /// </summary>
        private List<TodoItem> _items = new List<TodoItem>();

        /// <summary>
        /// Adds a new item to the list of items.
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>The item that was added</returns>
        public Task AddTodoItemAsync(TodoItem item)
        {
            if (item.Id == null)
            {
                item.Id = Guid.NewGuid().ToString("N");
            }
            UpdateVersion(item);
            if (_items.Any(t => t.Id == item.Id))
            {
                throw new InvalidOperationException("Item already exists");
            }
            return Task.Run(() => _items.Add(item));
        }

        /// <summary>
        /// Removes an item from the list.
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns></returns>
        public Task DeleteTodoItemAsync(TodoItem item)
        {
            if (item.Id == null)
            {
                throw new InvalidOperationException("Item does not have an Id");
            }
            if (!_items.Any(t => t.Id == item.Id))
            {
                throw new InvalidOperationException("Item does not exist");
            }
            return Task.Run(() => _items.Remove(item));
        }

        /// <summary>
        /// Retrieves an item from the list
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <returns>The Item</returns>
        public Task<TodoItem> GetTodoItemAsync(string id)
        {
            if (!_items.Any(t => t.Id == id))
            {
                throw new InvalidOperationException("Item does not exist");
            }
            return Task.FromResult(_items.Find(t => t.Id == id));
        }

        /// <summary>
        /// Retrieves the list of items
        /// </summary>
        /// <returns>The list of items.</returns>
        public Task<List<TodoItem>> GetTodoItemsAsync()
        {
            return Task.FromResult(_items);
        }

        /// <summary>
        /// Replaces an item with a new item
        /// </summary>
        /// <param name="item">THe item to replace, with updated fields</param>
        /// <returns>The new item</returns>
        public Task ReplaceTodoItemAsync(TodoItem item)
        {
            if (item.Id == null)
            {
                throw new InvalidOperationException("Item does not have an Id");
            }
            if (!_items.Any(t => t.Id == item.Id))
            {
                throw new InvalidOperationException("Item does not exist");
            }

            return Task.Run(() =>
            {
                var idx = _items.FindIndex(t => t.Id == item.Id);
                UpdateVersion(item);
                _items[idx] = item;
            });
        }

        /// <summary>
        /// Updates the version in the provided item.
        /// </summary>
        /// <param name="item">The item to update</param>
        private void UpdateVersion(TodoItem item)
        {
            item.UpdatedAt = DateTimeOffset.UtcNow;
            item.Version = Guid.NewGuid().ToString("N");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

/*
 * This repository implementation is synchronous in nature, so act that way!
 */
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Todo.NetStandard.Common
{
    /// <summary>
    /// An in-memory version of the TodoRepository, for offline testing purposes.
    /// </summary>
    public class InMemoryTodoRepository : ITodoRepository
    {
        private Dictionary<string, TodoItem> _items;

        public InMemoryTodoRepository()
        {
            _items = new Dictionary<string, TodoItem>();
        }

        #region ITodoRepository
        /// <summary>
        /// An event handler that is called whenever the repository is updated.
        /// </summary>
        public event EventHandler<RepositoryEventArgs> RepositoryUpdated;

        /// <summary>
        /// Adds an item to the service table.  Updates the item in-situ, and calls the
        /// <see cref="RepositoryUpdated"/> event handler on a successful operation.
        /// </summary>
        /// <param name="item">The item to add to the record</param>
        /// <exception cref="RepositoryException">when the record cannot be added</exception>
        public async Task AddTodoItemAsync(TodoItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            if (item.Id == null)
            {
                item.Id = Guid.NewGuid().ToString("N");
            }
            if (_items.ContainsKey(item.Id))
            {
                throw new RepositoryException("Item exists");
            }
            UpdateVersionInfo(item);
            _items.Add(item.Id, item);
            OnRepositoryChanged(RepositoryAction.Add, item);
        }

        /// <summary>
        /// Deletes an item in the service table.  Calls the <see cref="RepositoryUpdated"/> event 
        /// handler on a successful operation.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <exception cref="RepositoryException">when the record cannot be deleted</exception>
        public async Task DeleteTodoItemAsync(TodoItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            if (item.Id == null)
            {
                throw new RepositoryException("Item has not been added yet");
            }
            if (!_items.ContainsKey(item.Id))
            {
                throw new RepositoryException("Item does not exist");
            }
            var deletedItem = _items[item.Id];
            _items.Remove(item.Id);
            OnRepositoryChanged(RepositoryAction.Delete, deletedItem);
        }

        /// <summary>
        /// Retrieves a list of all items in the table.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{TodoItem}"/> containing all the items</returns>
        public Task<IEnumerable<TodoItem>> GetTodoItemsAsync()
        {
            IEnumerable<TodoItem> result = _items.Values.AsEnumerable();
            return Task.FromResult(result);
        }

        /// <summary>
        /// Saves the item, either adding or updating as appropiate.  An item is considered new
        /// if the item does not have an Id.  The <see cref="RepositoryUpdated"/> event handler
        /// is called with the appropriate action on success.
        /// </summary>
        /// <param name="item">The item to save</param>
        /// <exception cref="RepositoryException">If the operation fails</exception>
        public Task SaveTodoItemAsync(TodoItem item)
            => (item.Id == null) ? AddTodoItemAsync(item) : UpdateTodoItemAsync(item);

        /// <summary>
        /// Updates the item in the service table, updating the passed value with any updated
        /// values as well.  The <see cref="RepositoryUpdated"/> event handler is called with
        /// the appropriate action on success.
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <exception cref="RepositoryException">If the operation fails</exception>
        public async Task UpdateTodoItemAsync(TodoItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            if (item.Id == null || !_items.ContainsKey(item.Id))
            {
                throw new RepositoryException("Item does not exist");
            }
            UpdateVersionInfo(item);
            _items[item.Id] = item;
            OnRepositoryChanged(RepositoryAction.Update, item);
        }
        #endregion

        private void OnRepositoryChanged(RepositoryAction action, TodoItem item)
        {
            var eventArgs = new RepositoryEventArgs(action, item);
            RepositoryUpdated?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// The service will update the version information on success - this method
        /// simulates that activity.
        /// </summary>
        /// <param name="item">The item to be updated.</param>
        private void UpdateVersionInfo(TodoItem item)
        {
            item.Version = Guid.NewGuid().ToString("N");
            item.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}

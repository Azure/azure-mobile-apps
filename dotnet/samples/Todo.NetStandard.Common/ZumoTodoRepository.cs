using Azure;
using Microsoft.Zumo.MobileData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Todo.NetStandard.Common
{
    public class ZumoTodoRepository : ITodoRepository
    {
        private MobileTableClient _client;
        private MobileTable<TodoItem> _table;

        public ZumoTodoRepository()
        {
            _client = new MobileTableClient(Configuration.BackendService);
            _table = _client.GetTable<TodoItem>();
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
            try
            {
                TodoItem response = await _table.InsertItemAsync(item).ConfigureAwait(false);
                OnRepositoryChanged(RepositoryAction.Add, response);
            }
            catch (RequestFailedException ex)
            {
                Debug.WriteLine($"Insert Item Failed: {ex.Message}");
                throw new RepositoryException(ex.Message, ex);
            }
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
            try
            {
                await _table.DeleteItemAsync(item).ConfigureAwait(false);
                OnRepositoryChanged(RepositoryAction.Delete, item);
            }
            catch (RequestFailedException ex)
            {
                Debug.WriteLine($"Delete Item Failed: {ex.Message}");
                throw new RepositoryException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Retrieves a list of all items in the table.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{TodoItem}"/> containing all the items</returns>
        public Task<IEnumerable<TodoItem>> GetTodoItemsAsync()
        {
            var items = _table.GetItems().ToList();
            return Task.FromResult(items.AsEnumerable());
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
        /// If you have a backing store, then this signifies that you should synchronize the store.
        /// </summary>
        /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task SynchronizeAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // No backing store.
            return;
        }

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
            if (item.Id == null)
            {
                throw new RepositoryException("Item does not exist");
            }
            try
            {
                TodoItem response = await _table.ReplaceItemAsync(item).ConfigureAwait(false);
                OnRepositoryChanged(RepositoryAction.Update, response);
            }
            catch (RequestFailedException ex)
            {
                throw new RepositoryException(ex.Message, ex);
            }
        }
        #endregion

        private void OnRepositoryChanged(RepositoryAction action, TodoItem item)
        {
            var eventArgs = new RepositoryEventArgs(action, item);
            RepositoryUpdated?.Invoke(this, eventArgs);
        }
    }
}

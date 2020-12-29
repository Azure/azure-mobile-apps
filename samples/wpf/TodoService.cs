using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ZumoQuickstart
{
    /// <summary>
    /// The possible actions for a list change.
    /// </summary>
    public enum TodoListAction { Add, Delete, Update }

    /// <summary>
    /// Definition of the change event for a list change.
    /// </summary>
    public class TodoListEventArgs : EventArgs
    {
        public TodoListEventArgs(TodoListAction action, TodoItem item)
        {
            Action = action;
            Item = item;
        }

        public TodoListAction Action { get; }
        public TodoItem Item { get; }
    }

    /// <summary>
    /// The service interactions for an Azure Mobile Apps todo list service.
    /// </summary>
    public class TodoService
    {
        private bool isInitialized = false;
        private readonly AsyncLock initializationLock = new AsyncLock();

        private MobileServiceClient mClient;
        private IMobileServiceTable<TodoItem> mTable;
        // private IMobileServiceSyncTable<TodoItem> mTable;
        private readonly IAppContext mContext;

        public TodoService(IAppContext context)
        {
            mContext = context;
        }

        private async Task InitializeAsync()
        {
            using (await initializationLock.LockAsync())
            {
                if (!isInitialized)
                {
                    mClient = new MobileServiceClient(Constants.BackendUrl, new LoggingHandler());
                    mTable = mClient.GetTable<TodoItem>();
                    // mTable = mClient.GetSyncTable<TodoItem>();
                    isInitialized = true;
                }
            }
        }

        private void OnTodoListChanged(TodoListAction action, TodoItem item)
            => TodoListUpdated?.Invoke(this, new TodoListEventArgs(action, item));

        /// <summary>
        /// An event handler that is called whenever the repository is updated
        /// </summary>
        public event EventHandler<TodoListEventArgs> TodoListUpdated;

        /// <summary>
        /// Adds an item to the todo list.
        /// </summary>
        /// <param name="item">The item to add</param>
        public async Task AddTodoItemAsync(TodoItem item)
        {
            EnsureNotNull(item, nameof(item));
            await InitializeAsync().ConfigureAwait(false);

            await mTable.InsertAsync(item).ConfigureAwait(false);
            OnTodoListChanged(TodoListAction.Add, item);
        }

        /// <summary>
        /// Deletes and item from the todo list
        /// </summary>
        /// <param name="item">The item to delete</param>
        public async Task DeleteTodoItemAsync(TodoItem item)
        {
            EnsureNotNull(item, nameof(item));
            await InitializeAsync().ConfigureAwait(false);

            if (item.Id == null)
            {
                throw new ArgumentException("Item has not yet been added", nameof(item));
            }

            await mTable.DeleteAsync(item).ConfigureAwait(false);
            OnTodoListChanged(TodoListAction.Delete, item);
        }

        /// <summary>
        /// Retrieve a list of all items in the table.
        /// </summary>
        public async Task<IEnumerable<TodoItem>> GetTodoItemsAsync()
        {
            await InitializeAsync().ConfigureAwait(false);
            return await mTable.ToEnumerableAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Saves an item to the table, calling either update or add, depending on if it exists.
        /// </summary>
        /// <param name="item">The item to be saved</param>
        public Task SaveTodoItemAsync(TodoItem item)
            => (item.Id == null) ? AddTodoItemAsync(item) : UpdateTodoItemAsync(item);

        /// <summary>
        /// Synchronizes the local store with the remote store, if necessary.
        /// </summary>
        public async Task SynchronizeAsync()
        {
            await InitializeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an item that is already in the table.
        /// </summary>
        /// <param name="item">The item to update.</param>
        public async Task UpdateTodoItemAsync(TodoItem item)
        {
            EnsureNotNull(item, nameof(item));
            await InitializeAsync().ConfigureAwait(false);
            if (item.Id == null)
            {
                throw new ArgumentException("Item has not been added", nameof(item));
            }

            await mTable.UpdateAsync(item).ConfigureAwait(false);
            OnTodoListChanged(TodoListAction.Update, item);
        }

        /// <summary>
        /// Helper method to throw ArgumentNullException if the arg is null.
        /// </summary>
        /// <param name="o">The arg to test</param>
        /// <param name="name">The name of the arg</param>
        private void EnsureNotNull(object o, string name)
        {
            if (o == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Delegating Handler for the HTTP client to log all requests and responses.
        /// </summary>
        private class LoggingHandler : DelegatingHandler
        {
            public LoggingHandler() : base() { }
            public LoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
            {
                Debug.WriteLine($"[HTTP] >>> {request}");
                if (request.Content != null)
                {
                    Debug.WriteLine($"[HTTP] >>> {await request.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                }

                HttpResponseMessage response = await base.SendAsync(request, token).ConfigureAwait(false);

                Debug.WriteLine($"[HTTP] <<< {response}");
                if (response.Content != null)
                {
                    Debug.WriteLine($"[HTTP] <<< {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                }

                return response;
            }
        }
    }
}
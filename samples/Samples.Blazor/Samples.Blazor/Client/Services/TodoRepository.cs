// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client;
using Microsoft.JSInterop;
using Samples.Blazor.Client.Models;

namespace Samples.Blazor.Client.Services
{
    /// <summary>
    /// Implmentation of the <see cref="ITodoRepository"/> that works with Azure Mobile Apps.
    /// </summary>
    public class TodoRepository : ITodoRepository
    {
        /// <summary>
        /// When set to true, the client and table are both initialized.
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// Used for locking the initialization block to ensure only one initialization happens.
        /// </summary>
        private readonly SemaphoreSlim _asyncLock = new(1, 1);

        /// <summary>
        /// Reference to the client used for datasync operations.
        /// </summary>
        private DatasyncClient? _client = null;

        /// <summary>
        /// Reference to the table used for datasync operations.
        /// </summary>
        private IRemoteTable<TodoItemDTO>? _table = null;

        /// <summary>
        /// The runtime for JSInterop.
        /// </summary>
        private IJSRuntime _jsRuntime;

        /// <summary>
        /// Construct a new <see cref="TodoRepository"/> based on the default HttpClient.
        /// </summary>
        /// <param name="httpClient">The HttpClient.</param>
        /// <exception cref="ArgumentException">If the HttpClient does not have a BaseAddress defined.</exception>
        public TodoRepository(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            BackendUri = httpClient.BaseAddress ?? throw new ArgumentException("HttpClient must have a BaseAddress defined");
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// The URI of the backend service.
        /// </summary>
        public Uri BackendUri { get; }

        /// <summary>
        /// An event handler that is triggered when the list of items changes.
        /// </summary>
        public event EventHandler<TodoRepositoryEventArgs> RepositoryUpdated = delegate { };

        /// <summary>
        /// Initializes the datasync client.
        /// </summary>
        /// <returns>A task that completes when initialization is complete.</returns>
        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            try
            {
                await _asyncLock.WaitAsync();
                if (_initialized)
                    return;

                // Blazor support - must specify Installation Id.
                var options = new DatasyncClientOptions
                {
                    HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() },
                    InstallationId = BlazorSupport.GetInstallationId(_jsRuntime)
                };

                _client = new DatasyncClient(BackendUri, options);
                _table = _client.GetRemoteTable<TodoItemDTO>("todoitem");

                _initialized = true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _asyncLock.Release();
            }

        }

        /// <summary>
        /// Part of <see cref="ITodoRepository"/> - adds a new item to the repository.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>A task that completes when the operation is complete.</returns>
        public async Task AddNewItemAsync(TodoItemDTO item)
        {
            if (item == null) 
                throw new ArgumentNullException(nameof(item));

            await InitializeAsync();
            await _table!.InsertItemAsync(item);
            RepositoryUpdated?.Invoke(this, new TodoRepositoryEventArgs(RepositoryOperation.Add, item));
        }

        /// <summary>
        /// Deletes an existing item from the repository.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <returns>A task that finalizes when the operation is complete.</returns>
        public async Task DeleteItemAsync(TodoItemDTO item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.Id == null)
                return;

            await InitializeAsync();
            await _table!.DeleteItemAsync(item);
            RepositoryUpdated?.Invoke(this, new TodoRepositoryEventArgs(RepositoryOperation.Delete, item));
        }

        /// <summary>
        /// Gets a list of all items in the repository.
        /// </summary>
        /// <returns>A task that returns the list of items in the repository when complete.</returns>
        public async Task<IEnumerable<TodoItemDTO>> GetItemsAsync()
        {
            await InitializeAsync();
            return await _table!.GetAsyncItems().ToListAsync();
        }

        /// <summary>
        /// Updates an item in the repository.
        /// </summary>
        /// <param name="item">The updated item.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        public async Task UpdateItemAsync(TodoItemDTO item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.Id == null)
                throw new ArgumentException("Cannot update item without an ID", nameof(item));

            await InitializeAsync();
            await _table!.ReplaceItemAsync(item);
            RepositoryUpdated?.Invoke(this, new TodoRepositoryEventArgs(RepositoryOperation.Update, item));
        }
    }
}

using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.SQLiteStore;
using TodoMauiApp.Models;

namespace TodoMauiApp.Services;

internal class RemoteTodoService : ITodoService
{
    /// <summary>
    /// Reference to the client used for datasync operations.
    /// </summary>
    private DatasyncClient _client = null;

    /// <summary>
    /// Reference to the table used for datasync operations.
    /// </summary>
    private IOfflineTable<TodoItem> _table = null;

    /// <summary>
    /// The path to the offline database location.
    /// </summary>
    public string OfflineDb { get; set; }

    /// <summary>
    /// When set to true, the client and table and both initialized.
    /// </summary>
    private bool _initialized = false;

    /// <summary>
    /// Used for locking the initialization block to ensure only one initialization happens.
    /// </summary>
    private readonly SemaphoreSlim _asyncLock = new(1, 1);

    /// <summary>
    /// An event handler that is triggered when the list of items changes.
    /// </summary>
    public event EventHandler<TodoServiceEventArgs> TodoItemsUpdated;

    /// <summary>
    /// When using authentication, the token requestor to use.
    /// </summary>
    public Func<Task<AuthenticationToken>> TokenRequestor;

    public Uri ServiceUri;

    /// <summary>
    /// Creates a new <see cref="RemoteTodoService"/> with no authentication.
    /// </summary>
    public RemoteTodoService(Uri serviceUri)
    {
        ServiceUri = serviceUri;
        TokenRequestor = null; // no authentication
    }

    /// <summary>
    /// Creates a new <see cref="RemoteTodoService"/> with authentication.
    /// </summary>
    public RemoteTodoService(Uri serviceUri, Func<Task<AuthenticationToken>> tokenRequestor)
    {
        ServiceUri = serviceUri;
        TokenRequestor = tokenRequestor;
    }

    /// <summary>
    /// Initialize the connection to the remote table.
    /// </summary>
    /// <returns></returns>
    private async Task InitializeAsync()
    {
        // Short circuit, in case we are already initialized.
        if (_initialized)
        {
            return;
        }

        try
        {
            // Wait to get the async initialization lock
            await _asyncLock.WaitAsync();
            if (_initialized)
            {
                // This will also execute the async lock.
                return;
            }

            var connectionString = new UriBuilder
            {
                Scheme = "file",
                Path = OfflineDb,
                Query = "?mode=rwc"
            }.Uri.ToString();
            var store = new OfflineSQLiteStore(connectionString);
            store.DefineTable<TodoItem>();
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() },
                OfflineStore = store
            };

            // Initialize the client.
            _client = TokenRequestor == null
                ? new DatasyncClient(ServiceUri, options)
                : new DatasyncClient(ServiceUri, new GenericAuthenticationProvider(TokenRequestor), options);
            await _client.InitializeOfflineStoreAsync();

            // Get a reference to the offline table.
            _table = _client.GetOfflineTable<TodoItem>();

            // Set _initialied to true to prevent duplication of locking.
            _initialized = true;
        }
        catch (Exception)
        {
            // Re-throw the exception.
            throw;
        }
        finally
        {
            _asyncLock.Release();
        }
    }

    /// <summary>
    /// Get all the items in the list.
    /// </summary>
    /// <returns>The list of items (asynchronously)</returns>
    public async Task<IEnumerable<TodoItem>> GetItemsAsync()
    {
        await InitializeAsync();
        return await _table.ToListAsync();
    }

    /// <summary>
    /// Refreshes the TodoItems list manually.
    /// </summary>
    /// <returns>A task that completes when the refresh is done.</returns>
    public async Task RefreshItemsAsync()
    {
        await InitializeAsync();
        await _table.PushItemsAsync();
        await _table.PullItemsAsync();
        return;
    }

    /// <summary>
    /// Removes an item in the list, if it exists.
    /// </summary>
    /// <param name="item">The item to be removed.</param>
    /// <returns>A task that completes when the item is removed.</returns>
    public async Task RemoveItemAsync(TodoItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (item.Id == null)
        {
            // Short circuit for when the item has not been saved yet.
            return;
        }
        await InitializeAsync();
        await _table.DeleteItemAsync(item);
        TodoItemsUpdated?.Invoke(this, new TodoServiceEventArgs(TodoServiceEventArgs.ListAction.Delete, item));
    }

    /// <summary>
    /// Saves an item to the list.  If the item does not have an Id, then the item
    /// is considered new and will be added to the end of the list.  Otherwise, the
    /// item is considered existing and is replaced.
    /// </summary>
    /// <param name="item">The new item</param>
    /// <returns>A task that completes when the item is saved.</returns>
    public async Task SaveItemAsync(TodoItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        await InitializeAsync();

        TodoServiceEventArgs.ListAction action = (item.Id == null) ? TodoServiceEventArgs.ListAction.Add : TodoServiceEventArgs.ListAction.Update;
        if (item.Id == null)
        {
            await _table.InsertItemAsync(item);
        }
        else
        {
            await _table.ReplaceItemAsync(item);
        }
        TodoItemsUpdated?.Invoke(this, new TodoServiceEventArgs(action, item));
    }
}

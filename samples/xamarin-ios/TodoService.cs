using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZumoQuickstart
{
    public class TodoService
    {
        /// <summary>
        /// Singleton reference.
        /// </summary>
        public static TodoService DefaultService { get; } = new TodoService();

        private readonly MobileServiceClient client;
        private readonly IMobileServiceTable<TodoItem> todoTable;
        // private readonly IMobileServiceSyncTable<TodoItem> todoTable;

        private TodoService()
        {
            CurrentPlatform.Init();

            // Initialize the Azure Mobile Apps SDK
            client = new MobileServiceClient(Constants.BackendUrl);
            InitializeStoreAsync().Wait();

            // Get a reference to the table
            todoTable = client.GetTable<TodoItem>();
        }

        public List<TodoItem> Items { get; private set; }

        /// <summary>
        /// Initialize the offline store (if required)
        /// </summary>
        /// <returns></returns>
        public async Task InitializeStoreAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Synchronize the offline store with the online store
        /// </summary>
        /// <param name="pullData"></param>
        /// <returns></returns>
        public async Task SyncAsync(bool pullData = false)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Refresh the data within the app.
        /// </summary>
        /// <returns></returns>
        public async Task<List<TodoItem>> RefreshDataAsync()
        {
            try
            {
                await SyncAsync(pullData: true);
                Items = await todoTable.Where(item => !item.Complete).ToListAsync();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                Console.Error.WriteLine($"Refresh Error: {e.Message}");
                return null;
            }
            return Items;
        }

        /// <summary>
        /// Insert a new item - note that this does not push it to the server - you need to
        /// refresh to do that.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task InsertTodoItemAsync(TodoItem item)
        {
            try
            {
                await todoTable.InsertAsync(item);
                Items.Add(item);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                Console.Error.WriteLine($"Insert Error: {e.Message}");
            }
        }

        /// <summary>
        /// Mark an item complete - note that this does not push the change to the server - you need
        /// to refresh to do that.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task CompleteItemAsync(TodoItem item)
        {
            try
            {
                item.Complete = true;
                await todoTable.UpdateAsync(item);
                Items.Remove(item);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                Console.Error.WriteLine($"Update Error: {e.Message}");
            }
        }
    }
}
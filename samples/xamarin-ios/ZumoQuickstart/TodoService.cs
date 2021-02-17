using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Nito.AsyncEx;
using System.Threading;

namespace ZumoQuickstart
{
    public class TodoService
    {
        public static TodoService DefaultService { get; } = new TodoService();

        private bool isInitialized = false;
        private readonly AsyncLock initializationLock = new AsyncLock();
        private MobileServiceClient mClient;
        private IMobileServiceTable<TodoItem> mTable;
        // private readonly IMobileServiceSyncTable<TodoItem> mTable;

        public List<TodoItem> Items { get; private set; }

        private TodoService()
        {
            // Initialize Azure Mobile Apps, platform-specific code
            CurrentPlatform.Init();
        }

        /// <summary>
        /// TODO: Initialize the offline sync store.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeOfflineStoreAsync()
        {
            using (await initializationLock.LockAsync())
            {
                if (!isInitialized)
                {
                    mClient = new MobileServiceClient(Constants.BackendUrl, new LoggingHandler());
                    mTable = mClient.GetTable<TodoItem>();
                    isInitialized = true;
                }
            }
                await Task.CompletedTask;
        }

        /// <summary>
        /// TODO: Synchronize the offline store with the backend
        /// </summary>
        /// <param name="pullData"></param>
        /// <returns></returns>
        public async Task SyncAsync()
        {
            await InitializeOfflineStoreAsync();

            await Task.CompletedTask;
        }

        /// <summary>
        /// Refresh the item list from the backing store.
        /// </summary>
        /// <returns></returns>
        public async Task<List<TodoItem>> RefreshDataAsync()
        {
            try
            {
                await InitializeOfflineStoreAsync();
                await SyncAsync();
                Items = await mTable.Where(item => !item.Complete).ToListAsync();
                return Items;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Refresh ERROR: {ex.Message}");
                return null;
            }
        }

        public async Task InsertItemAsync(TodoItem item)
        {
            try
            {
                await InitializeOfflineStoreAsync();
                await mTable.InsertAsync(item);
                Items.Add(item);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Insert ERROR: {ex.Message}");
            }
        }

        public async Task CompleteItemAsync(TodoItem item)
        {
            try
            {
                await InitializeOfflineStoreAsync();
                item.Complete = true;
                await mTable.InsertAsync(item);
                Items.Remove(item);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Update ERROR: {ex.Message}");
            }
        }

        private class LoggingHandler : DelegatingHandler
        {
            public LoggingHandler() : base() { }
            public LoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
            {
                Console.Out.WriteLine($"[HTTP] >>> {request}");
                if (request.Content != null)
                {
                    Console.Out.WriteLine($"[HTTP] >>> {await request.Content.ReadAsStringAsync()}");
                }

                HttpResponseMessage response = await base.SendAsync(request, token);

                Console.Out.WriteLine($"[HTTP] <<< {response}");
                if (response.Content != null)
                {
                    Console.Out.WriteLine($"[HTTP] <<< {await response.Content.ReadAsStringAsync()}");
                }

                return response;
            }
        }
    }
}

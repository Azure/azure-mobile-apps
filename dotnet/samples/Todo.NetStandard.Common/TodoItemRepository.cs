using Azure.Mobile.Client;
using SQLite;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Todo.NetStandard.Common
{
    public class TodoItemRepository
    {
        /// <summary>
        /// Event handler definition - called when an item is inserted
        /// into the table.
        /// </summary>
        public event EventHandler<TodoItem> OnItemAdded;

        /// <summary>
        /// Event handler definition - called when an item is updated
        /// within the table.
        /// </summary>
        public event EventHandler<TodoItem> OnItemUpdated;

        /// <summary>
        /// Get the complete list of items to process.
        /// </summary>
        /// <returns>A <see cref="List{TodoItem}"/> for the list of items</returns>
        public async Task<List<TodoItem>> GetItemsAsync()
        {
            var client = await GetClientAsync();
            return await client.Table<TodoItem>().ToListAsync();
        }

        /// <summary>
        /// Add an item to the list.  If the addition is successful, then <see cref="OnItemAdded"/>
        /// event handler is triggered with the added item.
        /// </summary>
        /// <param name="item">The item to be added</param>
        public async Task AddItemAsync(TodoItem item)
        {
            var client = await GetClientAsync();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an item to the list.  If the update is successful, then <see cref="OnItemUpdated"/>
        /// event handler is triggered with the updated item.
        /// </summary>
        /// <param name="item">The item to be updated</param>
        public async Task UpdateItemAsync(TodoItem item)
        {
            var client = await GetClientAsync();
            throw new NotImplementedException();
        }

        public Task AddOrUpdateItemAsync(TodoItem item)
            => item.Id == null ? AddItemAsync(item) : UpdateItemAsync(item);

        #region Get Client
        private SQLiteAsyncConnection connection;

        /// <summary>
        /// Returns the current client.  If there is no client yet, then it will
        /// be created.
        /// </summary>
        /// <returns>The current client</returns>
        private async Task<SQLiteAsyncConnection> GetClientAsync()
        {
            if (connection != null)
            {
                return connection;
            }

            // Initialize the Sqlite database
            var documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var databasePath = Path.Combine(documentPath, "TodoItems.db");

            connection = new SQLiteAsyncConnection(databasePath);
            await connection.CreateTableAsync<TodoItem>();

            if (await connection.Table<TodoItem>().CountAsync() == 0)
            {
                await connection.InsertAsync(new TodoItem()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    UpdatedAt = DateTimeOffset.UtcNow,
                    Version = Guid.NewGuid().ToString("N"),
                    Title = "Welcome to Azure TodoList"
                });
            }

            return connection;
        }
        #endregion
    }
}

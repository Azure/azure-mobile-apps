// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Samples.Blazor.Client.Models;

namespace Samples.Blazor.Client.Services
{
    /// <summary>
    /// The interface to the repository for holding <see cref="TodoItemDTO"/> values.
    /// </summary>
    public interface ITodoRepository
    {
        /// <summary>
        /// An event handler that is triggered when the list of items changes.
        /// </summary>
        event EventHandler<TodoRepositoryEventArgs> RepositoryUpdated;

        /// <summary>
        /// Initializes the datasync client.
        /// </summary>
        /// <returns>A task that completes when initialization is complete.</returns>
        Task InitializeAsync();

        /// <summary>
        /// Adds a new item to the repository.
        /// </summary>
        /// <param name="item">The item to store.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        Task AddNewItemAsync(TodoItemDTO item);

        /// <summary>
        /// Deletes an existing item from the repository.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <returns>A task that finalizes when the operation is complete.</returns>
        Task DeleteItemAsync(TodoItemDTO item);

        /// <summary>
        /// Gets a list of all items in the repository.
        /// </summary>
        /// <returns>A task that returns the list of items in the repository when complete.</returns>
        Task<IEnumerable<TodoItemDTO>> GetItemsAsync();

        /// <summary>
        /// Updates an item in the repository.
        /// </summary>
        /// <param name="item">The updated item.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        Task UpdateItemAsync(TodoItemDTO item);
    }
}

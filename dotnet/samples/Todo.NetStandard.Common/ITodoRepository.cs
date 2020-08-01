using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Todo.NetStandard.Common
{
    public interface ITodoRepository
    {
        /// <summary>
        /// An event handler that is called whenever the repository is updated.
        /// </summary>
        event EventHandler<RepositoryEventArgs> RepositoryUpdated;

        /// <summary>
        /// Retrieve a list of all items in the table.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TodoItem>> GetTodoItemsAsync();

        /// <summary>
        /// Adds an item to the service table.  Updates the item in-situ, and calls the
        /// <see cref="RepositoryUpdated"/> event handler on a successful operation.
        /// </summary>
        /// <param name="item">The item to add to the record</param>
        /// <exception cref="RepositoryException">when the record cannot be added</exception>
        Task AddTodoItemAsync(TodoItem item);

        /// <summary>
        /// Deletes an item in the service table.  Calls the <see cref="RepositoryUpdated"/> event 
        /// handler on a successful operation.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <exception cref="RepositoryException">when the record cannot be deleted</exception>
        Task DeleteTodoItemAsync(TodoItem item);

        /// <summary>
        /// Saves an item that is already in the table, either adding or updating the
        /// item record from the service.  Also sends an event out on success with the 
        /// new item.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <exception cref="RepositoryException">when the record cannot be updated</exception>
        Task SaveTodoItemAsync(TodoItem item);

        /// <summary>
        /// Updates an item that is already in the table, updating the
        /// item record from the service.  Also sends an event out on
        /// success with the new item.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <exception cref="RepositoryException">when the record cannot be updated</exception>
        Task UpdateTodoItemAsync(TodoItem item);

    }
}

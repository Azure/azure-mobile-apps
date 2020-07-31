using System.Collections.Generic;
using System.Threading.Tasks;

namespace Todo.NetStandard.Common
{
    /// <summary>
    /// Definition of the repository of TodoItems
    /// </summary>
    public interface ITodoRepository
    {
        /// <summary>
        /// Get the list of all Todo Items.
        /// </summary>
        /// <returns></returns>
        Task<List<TodoItem>> GetTodoItemsAsync();

        /// <summary>
        /// Get a single Todo Item by ID.
        /// </summary>
        /// <param name="id">The ID of the TodoItem to retrieve</param>
        /// <returns>The TodoItem</returns>
        Task<TodoItem> GetTodoItemAsync(string id);

        /// <summary>
        /// Add a TodoItem to the repository.
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>The actual item that was added</returns>
        Task AddTodoItemAsync(TodoItem item);

        /// <summary>
        /// Replace a TodoItem in the repository.
        /// </summary>
        /// <param name="item">The item to replace</param>
        /// <returns>The item after replacement</returns>
        Task ReplaceTodoItemAsync(TodoItem item);

        /// <summary>
        /// Deletes a TodoItem from the repository.
        /// </summary>
        /// <param name="item">The item to delete</param>
        Task DeleteTodoItemAsync(TodoItem item);
    }
}

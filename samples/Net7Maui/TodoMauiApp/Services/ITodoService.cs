using TodoMauiApp.Models;

namespace TodoMauiApp.Services;

internal interface ITodoService
{
    event EventHandler<TodoServiceEventArgs> TodoItemsUpdated;

    Task<IEnumerable<TodoItem>> GetItemsAsync();
    Task RefreshItemsAsync();
    Task RemoveItemAsync(TodoItem item);
    Task SaveItemAsync(TodoItem item);
}

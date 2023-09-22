using Microsoft.Datasync.Client;

namespace TodoMauiApp.Models;

public class TodoItem : DatasyncClientData, IEquatable<TodoItem>
{
    public string Title { get; set; }
    public bool IsComplete { get; set; }

    public bool Equals(TodoItem other)
        => other != null && other.Id == Id && other.Title == Title && other.IsComplete == IsComplete;
}

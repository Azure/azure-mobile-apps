namespace TodoMauiApp.Models;

internal class TodoServiceEventArgs : EventArgs
{
    public TodoServiceEventArgs(ListAction action, TodoItem item)
    {
        Action = action;
        Item = item;
    }

    public ListAction Action { get; }
    public TodoItem Item { get; }

    public enum ListAction { Add, Delete, Update };
}

using TodoApp.Data;
using TodoApp.Data.Services;

namespace TodoApp.AvaloniaUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public MainWindowViewModel()
        {
            TodoService = new RemoteTodoService();
            TodoList = new TodoListViewModel(TodoService);
        }

        public ITodoService TodoService { get; }

        public TodoListViewModel TodoList { get; }
    }
}

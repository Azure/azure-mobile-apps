using TodoApp.Data;

namespace TodoApp.AvaloniaUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(ITodoService service)
        {
            TodoList = new TodoListViewModel(service);
        }

        public TodoListViewModel TodoList { get; }
    }
}

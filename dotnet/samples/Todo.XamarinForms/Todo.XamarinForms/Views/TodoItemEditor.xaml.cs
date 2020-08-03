using Todo.NetStandard.Common;
using Todo.XamarinForms.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Todo.XamarinForms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TodoItemEditor : ContentPage
    {
        public TodoItemEditor(ITodoRepository repository, TodoItem item)
        {
            InitializeComponent();
            BindingContext = new TodoItemViewModel(Navigation, repository, item);
        }
    }
}
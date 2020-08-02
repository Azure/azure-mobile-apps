using Todo.NetStandard.Common;
using Todo.XamarinForms.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Todo.XamarinForms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TodoListPage : ContentPage
    {
        public TodoListPage(ITodoRepository repository)
        {
            InitializeComponent();
            BindingContext = new TodoListViewModel(Navigation, repository);
        }
    }
}
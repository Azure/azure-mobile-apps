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

        public void OnListItemTapped(object sender, ItemTappedEventArgs e)
        {
            (BindingContext as TodoListViewModel).SelectItemCommand.Execute(e.Item);
            if (sender is ListView itemlist)
            {
                itemlist.SelectedItem = null; // De-select the item in the list.
            }
        }
    }
}
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ZumoQuickstart
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TodoListPage : ContentPage
    {
        public TodoListPage(TodoService service)
        {
            InitializeComponent();
            BindingContext = new TodoListViewModel(Navigation, service);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as TodoListViewModel)?.OnAppearing();
        }

        public void OnListItemTapped(object sender, ItemTappedEventArgs e)
        {
            (BindingContext as TodoListViewModel)?.SelectItemCommand.Execute(e.Item);
            if (sender is ListView itemlist)
            {
                itemlist.SelectedItem = null;
            }
        }
    }
}
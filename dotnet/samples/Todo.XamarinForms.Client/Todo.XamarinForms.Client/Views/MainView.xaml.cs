using Todo.XamarinForms.Client.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Todo.XamarinForms.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainView : ContentPage
    {
        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

            ItemsListView.ItemSelected += (s, e) => ItemsListView.SelectedItem = null;
        }
    }
}
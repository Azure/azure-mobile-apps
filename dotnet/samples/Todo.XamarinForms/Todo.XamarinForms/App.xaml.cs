using Todo.NetStandard.Common;
using Todo.XamarinForms.Views;
using Xamarin.Forms;

namespace Todo.XamarinForms
{
    public partial class App : Application
    {
        public ITodoRepository repository = new InMemoryTodoRepository();

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new TodoListPage(repository));
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}

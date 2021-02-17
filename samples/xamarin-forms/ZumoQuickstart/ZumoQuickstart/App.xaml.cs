using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ZumoQuickstart
{
    public partial class App : Application
    {
        public IAppContext AppContext { get; }

        public TodoService TodoService { get; }

        public App(IAppContext context)
        {
            // Stores the app context
            AppContext = context;
            TodoService = new TodoService(context);

            InitializeComponent();
            MainPage = new NavigationPage(new TodoListPage(TodoService));
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

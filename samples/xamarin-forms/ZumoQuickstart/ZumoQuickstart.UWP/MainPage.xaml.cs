namespace ZumoQuickstart.UWP
{
    public sealed partial class MainPage : IAppContext
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new ZumoQuickstart.App(this));
        }
    }
}

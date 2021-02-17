using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Xamarin.Forms;

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

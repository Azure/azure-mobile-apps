// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Foundation;
using TodoApp.iOS.ViewControllers;
using UIKit;

namespace TodoApp.iOS
{
    public static class Application
    {
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }

    [Register("AppDelegate")]
    public class AppDelegate : UIResponder, IUIApplicationDelegate
    {
        [Export("window")]
        public UIWindow Window { get; set; }

        [Export("application:didFinishLaunchingWithOptions:")]
        public bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            var controller = new HomeViewController();

            var nav = new UINavigationController(controller);
            
            Window.RootViewController = nav;
            Window.MakeKeyAndVisible();

            return true;
        }
    }
}


// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace TodoApp.MAUI
{
    public partial class App : Application
    {
        const int WindowWidth = 400;
        const int WindowHeight = 800;

        public App()
        {
            InitializeComponent();

            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
#if WINDOWS
                handler.PlatformView.Activate();

                IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(handler.PlatformView);
                AppWindow appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(windowHandle));

                appWindow.Resize(new SizeInt32(WindowWidth, WindowHeight));
#endif
            });

            MainPage = new NavigationPage(new MainPage());
        }
    }
}
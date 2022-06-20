using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TodoApp.AvaloniaUI.ViewModels;
using TodoApp.AvaloniaUI.Views;
using TodoApp.Data.Services;

namespace TodoApp.AvaloniaUI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var service = new RemoteTodoService();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(service),
                };
            }
        }
    }
}

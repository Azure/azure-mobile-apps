using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TodoApp.AvaloniaUI.Views
{
    public partial class TodoListView : UserControl
    {
        public TodoListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

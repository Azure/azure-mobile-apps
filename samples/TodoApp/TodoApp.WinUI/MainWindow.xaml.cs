using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TodoApp.Data.MVVM;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TodoApp.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, IMVVMHelper
    {
        private readonly TodoListViewModel _viewModel;

        public MainWindow()
        {
            this.InitializeComponent();
            _viewModel = new TodoListViewModel(this as IMVVMHelper, App.TodoService);
            mainContainer.DataContext = _viewModel;
        }

        private void mainContainer_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.OnActivated();
        }

        private async void TextboxKeyDownHandler(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                await _viewModel.AddItemAsync(textboxControl.Text.Trim()).ConfigureAwait(false);
                ClearTextBox();
            }
        }

        /// <summary>
        /// Event handler called when the user clicks on the + sign to add an item.
        /// </summary>
        private async void AddItemClickHandler(object sender, RoutedEventArgs e)
        {
            await _viewModel.AddItemAsync(textboxControl.Text.Trim()).ConfigureAwait(false);
            ClearTextBox();
        }

        /// <summary>
        /// Event handler called when the user checks or unchecks an item.
        /// </summary>
        private async void CheckboxClickHandler(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.Tag is string itemId)
            {
                bool isComplete = checkbox.IsChecked ?? false;
                await _viewModel.UpdateItemAsync(itemId, isComplete).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Event handler called when the user clicks the refresh items button.
        /// </summary>
        private async void RefreshItemsClickHandler(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshItemsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Clears the content from the text box.
        /// </summary>
        private void ClearTextBox() => RunOnUiThreadAsync(() => textboxControl.Text = String.Empty);

        public Task RunOnUiThreadAsync(Action func)
        {
            this.DispatcherQueue.TryEnqueue(() => func());
            return Task.CompletedTask;
        }

        public Task DisplayErrorAlertAsync(string title, string message)
        {
            throw new NotImplementedException();
        }
    }
}

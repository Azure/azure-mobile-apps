using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZumoQuickstart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IAppContext
    {
        private readonly MainWindowViewModel _viewModel;
        private static readonly TodoService _service = new TodoService();

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel(_service);
            DataContext = _viewModel;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _viewModel.OnActivated();
        }

        /// <summary>
        /// Event handler that handles pressing Enter in the input box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected async void TextboxKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                await _viewModel.AddItemAsync(textboxControl.Text.Trim()).ConfigureAwait(false);
                App.RunOnUiThread(() => textboxControl.Text = string.Empty);
            }
        }

        /// <summary>
        /// Event handler that handles clicking on the + sign to add an item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected async void AddItemClickHandler(object sender, RoutedEventArgs e)
        {
            await _viewModel.AddItemAsync(textboxControl.Text.Trim()).ConfigureAwait(false);
            App.RunOnUiThread(() => textboxControl.Text = string.Empty);
        }

        /// <summary>
        /// Event handler that handles checking or unchecking an item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected async void CheckboxClickHandler(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            string itemId = checkbox.Tag as string;
            bool isComplete = checkbox.IsChecked ?? false;
            await _viewModel.UpdateItemAsync(itemId, isComplete).ConfigureAwait(false);
        }

        /// <summary>
        /// Event handler that handles the refresh click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected async void RefreshItemsClickHandler(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshItemsAsync(true).ConfigureAwait(false);
        }
    }
}

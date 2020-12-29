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

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel(this);
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
        protected async Task textboxKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                await _viewModel.AddItemAsync(textboxControl.Text.Trim());
                textboxControl.Text = string.Empty;
            }
        }

        /// <summary>
        /// Event handler that handles clicking on the + sign to add an item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected async Task addItemClickHandler(object sender, RoutedEventArgs e)
        {
            await _viewModel.AddItemAsync(textboxControl.Text.Trim());
            textboxControl.Text = string.Empty;
        }

        /// <summary>
        /// Event handler that handles selecting an item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected async Task itemListSelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            await _viewModel.SelectItemAsync(itemlistControl.SelectedItem as TodoItem);
            itemlistControl.SelectedItem = null;
        }
    }
}

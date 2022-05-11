// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TodoApp.Data.MVVM;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TodoApp.UWP
{
    /// <summary>
    /// Backing code for the main page.
    /// </summary>
    public partial class MainPage : Page, IMVVMHelper
    {
        private readonly TodoListViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(450, 800));

            ApplicationView.PreferredLaunchViewSize = new Size(450, 800);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            _viewModel = new TodoListViewModel(this, (Application.Current as App)?.TodoService);
            DataContext = _viewModel;
        }

        #region IMVVMHelper
        /// <summary>
        /// Runs the associated code on the UI thread so that the UI can
        /// be updated correctly.
        /// </summary>
        /// <param name="func">The code to be run</param>
        /// <returns>A task the completes when the code is completed.</returns>
        public async Task RunOnUiThreadAsync(Action func)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => func.Invoke());
        }

        /// <summary>
        /// Displays a pop-up alert showing an error condition.
        /// </summary>
        /// <param name="title">The title of the alert.</param>
        /// <param name="message">The content of the alert.</param>
        /// <returns>A task that completes when the user acknowledges the error.</returns>
        public async Task DisplayErrorAlertAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
        }
        #endregion

        /// <summary>
        /// Event handler that is triggered when the page is first displayed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _viewModel.OnActivated();
        }

        /// <summary>
        /// Event handler that is triggered when the Add Item button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "Event handler")]
        protected async void AddItemClickHandler(object sender, RoutedEventArgs e)
        {
            await _viewModel.AddItemAsync(textboxControl.Text.Trim()).ConfigureAwait(false);
            await RunOnUiThreadAsync(() => textboxControl.Text = String.Empty).ConfigureAwait(false);
        }

        /// <summary>
        /// Event handler that is triggered when the check box next to an item is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "Event handler")]
        protected async void CheckboxClickHandler(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb)
            {
                await _viewModel.UpdateItemAsync(cb.Tag as string, cb.IsChecked ?? false).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Event handler that is triggered when the Refresh Items button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "Event handler")]
        protected async void RefreshItemsClickHandler(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshItemsAsync().ConfigureAwait(false);
        }
    }
}

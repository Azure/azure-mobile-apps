using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Data.MVVM;
using TodoApp.Data.Services;
using Windows.System;


namespace TodoApp.Uno;

public sealed partial class MainPage : Page, IMVVMHelper {
    private readonly TodoListViewModel _viewModel;
    private readonly ITodoService _service;

    public MainPage() {
        this.InitializeComponent();

        _service = new RemoteTodoService();
        _viewModel = new TodoListViewModel(this, _service);
        mainContainer.DataContext = _viewModel;
    }


    #region IMVVMHelper
    public Task RunOnUiThreadAsync(Action func) {
        DispatcherQueue.TryEnqueue(() => func());
        return Task.CompletedTask;
    }

    public async Task DisplayErrorAlertAsync(string title, string message) {
        var dialog = new ContentDialog {
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };
#if WINDOWS
        dialog.XamlRoot = Content.XamlRoot;
#endif
        await dialog.ShowAsync();
    }
    #endregion

    #region Event Handlers

    private void GridLoadedEventHandler(object sender, RoutedEventArgs e) {
        _viewModel.OnActivated();
    }

    private void TextboxKeyDownHandler(object sender, KeyRoutedEventArgs e) {
        if (e.Key == VirtualKey.Enter) {
            AddItemToList();
        }
    }

    /// <summary>
    /// Event handler that is triggered when the Add Item button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddItemClickHandler(object sender, RoutedEventArgs e) {
        AddItemToList();
    }

    private async void AddItemToList() {
        await _viewModel.AddItemAsync(textboxControl.Text.Trim());
        await RunOnUiThreadAsync(() => textboxControl.Text = String.Empty);
    }

    /// <summary>
    /// Event handler that is triggered when the check box next to an item is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CheckboxClickHandler(object sender, RoutedEventArgs e) {
        if (sender is Microsoft.UI.Xaml.Controls.CheckBox cb) {
            await _viewModel.UpdateItemAsync(cb.Tag as string, cb.IsChecked ?? false);
        }
    }

    /// <summary>
    /// Event handler that is triggered when the Refresh Items button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RefreshItemsClickHandler(object sender, RoutedEventArgs e) {
        await _viewModel.RefreshItemsAsync();
    }
    #endregion


}

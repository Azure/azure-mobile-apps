using Microsoft.WindowsAzure.MobileServices;
// for Offline Sync
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ZumoQuickstart.DataModel;

// Disable unused parameters warning - since we only have one button, the sender is not relevant.
// This pragma disables the intellisense warning about it.
#pragma warning disable RCS1163 // Unused parameter.

// Disable ConfigureAwait warnings.  Normally, you would need to make a judgement on where to retrieve
// the response to the async operation - the same thread or a different thread.
#pragma warning disable RCS1090 // Add call to 'ConfigureAwait' (or vice versa).

namespace ZumoQuickstart
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MobileServiceCollection<TodoItem, TodoItem> items;

        private readonly IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();
        // private readonly IMobileServiceSyncTable<TodoItem> todoTable = App.MobileService.GetSyncTable<TodoItem>();

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// When this page is brought up, automatically initialize the offline store (if being used) and
        /// refresh the data.
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("OnNavigatedTo");
            await InitializeOfflineStoreAsync();
            ButtonRefresh_Click(this, null);
        }

        /// <summary>
        /// Insert a TodoItem into the store.
        /// </summary>
        /// <param name="todoItem"></param>
        /// <returns></returns>
        private async Task InsertTodoItemAsync(TodoItem todoItem)
        {
            Debug.WriteLine("InsertTodoItemAsync");
            await todoTable.InsertAsync(todoItem);
            items.Add(todoItem);
        }

        /// <summary>
        /// Refresh the list of items from the store.
        /// </summary>
        /// <returns></returns>
        private async Task RefreshTodoItemsAsync()
        {
            Debug.WriteLine("RefreshTodoItemsAsync");
            try
            {
                items = await todoTable.Where(todoItem => !todoItem.Complete).ToCollectionAsync();
                Debug.WriteLine("Got items");
                ListItems.ItemsSource = items;
                Debug.WriteLine("Assigned to list");
                ButtonSave.IsEnabled = true;
                Debug.WriteLine("Save Button is Enabled");
            }
            catch (MobileServiceInvalidOperationException e)
            {
                await new MessageDialog(e.Message, "Error Loading Items").ShowAsync();
            }
        }

        /// <summary>
        /// Update the state of the TodoItem to mark/clear the completed flag.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task UpdateCheckedTodoItemAsync(TodoItem item)
        {
            Debug.WriteLine("UpdateCheckedTodoItemAsync");
            await todoTable.UpdateAsync(item);
            items.Remove(item);
            ListItems.Focus(FocusState.Unfocused);
        }

        /// <summary>
        /// Event handler called when the Refresh button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ButtonRefresh_Click");
            ButtonRefresh.IsEnabled = false;
            await SyncAsync();          // Refresh the offline store
            await RefreshTodoItemsAsync();   // Update the list from the current store.
            ButtonRefresh.IsEnabled = true;
        }

        /// <summary>
        /// Event handler called when the Save button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ButtonSave_Click");
            var todoItem = new TodoItem { Text = TextInput.Text };
            await InsertTodoItemAsync(todoItem);
        }

        /// <summary>
        /// Event handler called when the check-box is set or cleared
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("CheckBoxComplete_Checked");
            CheckBox cb = (CheckBox)sender;
            TodoItem item = cb.DataContext as TodoItem;
            await UpdateCheckedTodoItemAsync(item);
        }

        /// <summary>
        /// Initialize the offline-store.
        /// </summary>
        /// <returns></returns>
        private async Task InitializeOfflineStoreAsync()
        {
            Debug.WriteLine("InitializeOfflineStoreAsync");
            await SyncAsync();
        }

        /// <summary>
        /// Synchronize the offline store with the online store.
        /// </summary>
        /// <returns></returns>
        private async Task SyncAsync()
        {
            Debug.WriteLine("SyncAsync");
            await Task.CompletedTask;
        }
    }
}

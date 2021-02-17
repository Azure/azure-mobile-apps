using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ZumoQuickstart.Utils;

#pragma warning disable IDE0060 // Unused parameter.
#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable RCS1090 // Add call to 'ConfigureAwait' (or vice versa).

namespace ZumoQuickstart
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        private static readonly TodoService _service = new TodoService();
        private ObservableCollection<TodoItem> _items = new ObservableCollection<TodoItem>();
        private bool _isRefreshing = false;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(450, 800);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            DataContext = this;
        }

        /// <summary>
        /// When this page is brought up, automatically initialize the offline store (if being used) and
        /// refresh the data.
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await RefreshItemsAsync(true);
            _service.TodoListUpdated += OnServiceUpdated;
        }

        #region Bindable Properties
        /// <summary>
        /// Displays a pop-up alert for the user to interact with.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        private async Task DisplayAlertAsync(string title, string message)
        {
            var dialog = new ContentDialog()
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
        }

        #region Bindable Properties
        public ObservableCollection<TodoItem> Items
        {
            get => _items;
            set {
                _items = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRefreshing)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefreshVisibility)));
            }
        }
        #endregion

        public Visibility RefreshVisibility
        {
            get => _isRefreshing ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        /// <summary>
        /// Event handler called when the service list is updated.  This allows the app
        /// to modify the displayed list by modifying the backend database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnServiceUpdated(object sender, TodoListEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (e.Action)
                {
                    case TodoListAction.Add:
                        Items.Add(e.Item);
                        break;
                    case TodoListAction.Delete:
                        Items.RemoveIf(m => m.Id == e.Item.Id);
                        break;
                    case TodoListAction.Update:
                        Items.ReplaceIf(m => m.Id == e.Item.Id, e.Item);
                        break;
                }
            });
        }

        private async Task ClearTextboxAsync()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                textboxControl.Text = string.Empty;
            });
        }

        /// <summary>
        /// Event handler called when the user clicks on the Add Item button next
        /// to the text box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void AddItemClickHandler(object sender, RoutedEventArgs e)
        {
            await AddItemAsync(textboxControl.Text.Trim());
            await ClearTextboxAsync();
        }

        /// <summary>
        /// Event handler called when the user clicks on a checkbox in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void CheckboxClickHandler(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            string itemId = checkbox.Tag as string;
            bool isComplete = checkbox.IsChecked ?? false;
            await UpdateItemAsync(itemId, isComplete);
        }

        /// <summary>
        /// Event handler called when the user clicks on the Refresh Items button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void RefreshItemsClickHandler(object sender, RoutedEventArgs e)
        {
            await RefreshItemsAsync(true);
        }

        /// <summary>
        /// Refresh the list of items from the store
        /// </summary>
        /// <param name="syncItems">If true, synchronize with the backend store prior to fetch</param>
        /// <returns></returns>
        public async Task RefreshItemsAsync(bool syncItems = false)
        {
            IsRefreshing = true;
            try
            {
                if (syncItems)
                {
                    await _service.SynchronizeAsync();
                }
                var enumerable = await _service.GetTodoItemsAsync();
                Items = new ObservableCollection<TodoItem>(enumerable);
            }
            catch (Exception error)
            {
                await DisplayAlertAsync("Refresh Error", error.Message);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        /// <summary>
        /// Adds an item to the list of items in the store.
        /// </summary>
        /// <param name="text">The text of the new item</param>
        /// <returns></returns>
        public async Task AddItemAsync(string text)
        {
            try
            {
                var item = new TodoItem { Text = text };
                await _service.AddTodoItemAsync(item);
            }
            catch (Exception error)
            {
                await DisplayAlertAsync("Add Item Error", error.Message);
            }
        }

        /// <summary>
        /// Called when an item is to be updated in the list.
        /// </summary>
        /// <param name="itemId">The ID of the item</param>
        /// <param name="isComplete">The state of the complete flag</param>
        /// <returns></returns>
        public async Task UpdateItemAsync(string itemId, bool isComplete)
        {
            try
            {
                var item = Items.Single(m => m.Id == itemId);
                item.Complete = isComplete;
                await _service.SaveTodoItemAsync(item);
            }
            catch (Exception error)
            {
                await DisplayAlertAsync("Error", error.Message);
            }
        }
    }
}

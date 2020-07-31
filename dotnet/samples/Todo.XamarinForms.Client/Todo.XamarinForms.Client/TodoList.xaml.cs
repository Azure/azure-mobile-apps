using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Todo.NetStandard.Common;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Todo.XamarinForms.Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TodoList : ContentPage
    {
        private ObservableCollection<TodoItem> _todoItems;
        private bool _isRefreshing;

        public TodoList()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshItemsAsync();
        }

        /// <summary>
        /// Bindable property for the items in the list.
        /// </summary>
        public ObservableCollection<TodoItem> TodoItems
        {
            get => _todoItems;
            set { _todoItems = value; OnPropertyChanged(nameof(TodoItems)); }
        }

        /// <summary>
        /// Bindable property to indicate the service is refreshing
        /// </summary>
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(nameof(IsRefreshing)); }
        }

        /// <summary>
        /// Event handler for selecting an item in the repository
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as TodoItem;
            if (Device.RuntimePlatform != Device.iOS && item != null)
            {
                // Not iOS - the swipe-to-delete is discoverable there
                await DisplayAlert(item.Text, "Press-and-hold to complete task", "OK");
            }

            // Prevents background getting highlighted
            todoList.SelectedItem = null;
        }

        /// <summary>
        /// Event handler for completing an item in the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public async void OnComplete(object sender, EventArgs e)
        {
            var menuItem = ((MenuItem)sender);
            var item = menuItem.CommandParameter as TodoItem;

            try
            {
                item.Complete = true;
                await TodoManager.Repository.ReplaceTodoItemAsync(item);
            }
            catch (Exception error)
            {
                await DisplayAlert("Add Error", $"Error Message: {error.Message}", "OK");
            }

            TodoItems = await GetItemsAsync();
        }

        public ICommand RefreshCommand
        {
            get => new Command(async () => await RefreshItemsAsync());
        }

        private async Task RefreshItemsAsync()
        {
            IsRefreshing = true;
            TodoItems = await GetItemsAsync(true);
            IsRefreshing = false;
        }

        #region Add Command
        public ICommand AddCommand
        {
            get => new Command(async () => await AddItemAsync());
        }

        private async Task AddItemAsync()
        {
            try
            {
                var newItem = new TodoItem { Text = newItemName.Text };
                await TodoManager.Repository.AddTodoItemAsync(newItem);
            }
            catch (Exception e)
            {
                await DisplayAlert("Add Error", $"Error Message: {e.Message}", "OK");
            }

            TodoItems = await GetItemsAsync();

            newItemName.Text = string.Empty;
            newItemName.Unfocus();
        }
        #endregion

        private async Task<ObservableCollection<TodoItem>> GetItemsAsync(bool syncItems = false)
        {
            try
            {
                if (syncItems)
                {
                    await TodoManager.SynchronizeAsync();
                }

                var items = (await TodoManager.Repository.GetTodoItemsAsync()).Where(todoItem => !todoItem.Complete);
                return new ObservableCollection<TodoItem>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error: {e.Message}");
            }
            return null;
        }
    }
}
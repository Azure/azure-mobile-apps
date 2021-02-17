using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ZumoQuickstart
{
    public class TodoListViewModel : BindableObject
    {
        private string mTitle;
        private bool mIsRefreshing = false;
        private ObservableCollection<TodoItem> mItems;

        public TodoListViewModel(INavigation navigation, TodoService service)
        {
            Navigation = navigation;
            TodoService = service;
            Title = "QuickStart Todo List";
        }

        public void OnAppearing()
        {
            RefreshItemsCommand.Execute(null);
            TodoService.TodoListUpdated += OnServiceUpdated;
        }

        private void OnServiceUpdated(object sender, TodoListEventArgs e)
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
        }

        /// <summary>
        /// The current navigation context
        /// </summary>
        public INavigation Navigation { get; }

        /// <summary>
        /// The current data repository
        /// </summary>
        public TodoService TodoService { get; }

        /// <summary>
        /// Displays a pop-up alert message.
        /// </summary>
        private void DisplayAlert(string title, string message, string button)
        {
            Device.BeginInvokeOnMainThread(new Action(async () =>
                await App.Current.MainPage.DisplayAlert(title, message, button).ConfigureAwait(false)
            ));
        }

        #region Bindable Properties
        public string Title
        {
            get => mTitle;
            set { mTitle = value; OnPropertyChanged(nameof(Title)); }
        }

        public bool IsRefreshing
        {
            get => mIsRefreshing;
            set { mIsRefreshing = value; OnPropertyChanged(nameof(IsRefreshing)); }
        }

        public ObservableCollection<TodoItem> Items
        {
            get => mItems;
            set { mItems = value; OnPropertyChanged(nameof(Items)); }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Command interface called when the user wishes to add a new item.
        /// </summary>
        public ICommand AddItemCommand
            => new Command<Entry>(async (Entry entry) => await AddItemAsync(entry).ConfigureAwait(false));

        /// <summary>
        /// Command interface called when the list needs to be refreshed
        /// </summary>
        public ICommand RefreshItemsCommand
            => new Command(async () => await RefreshItemsAsync(true).ConfigureAwait(false));

        /// <summary>
        /// Command interface called when an item in the list has been selected
        /// </summary>
        public ICommand SelectItemCommand
            => new Command<TodoItem>(async (TodoItem item) => await SelectItemAsync(item).ConfigureAwait(false));
        #endregion

        /// <summary>
        /// Add a new item (with the specified text) to the list of items
        /// </summary>
        /// <param name="control">The Entry control.</param>
        private async Task AddItemAsync(Entry control)
        {
            try
            {
                var item = new TodoItem { Text = control.Text };
                await TodoService.AddTodoItemAsync(item).ConfigureAwait(false);
                Device.BeginInvokeOnMainThread(new Action(() => {
                    control.Text = string.Empty;
                    control.Unfocus();
                }));
            }
            catch (Exception error)
            {
                DisplayAlert("Error", error.Message, "OK");
            }
        }

        /// <summary>
        /// Refresh the list of items from the store.
        /// </summary>
        /// <param name="syncItems">If true, synchronize with the backend store.</param>
        /// <returns></returns>
        private async Task RefreshItemsAsync(bool syncItems = false)
        {
            IsRefreshing = true;
            try
            {
                if (syncItems)
                {
                    await TodoService.SynchronizeAsync().ConfigureAwait(false);
                }

                var enumerable = await TodoService.GetTodoItemsAsync().ConfigureAwait(false);
                Items = new ObservableCollection<TodoItem>(enumerable);
            }
            catch (Exception error)
            {
                DisplayAlert("Error", error.Message, "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task SelectItemAsync(TodoItem item)
        {
            try
            {
                item.Complete = !item.Complete;
                await TodoService.SaveTodoItemAsync(item).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                DisplayAlert("Error", error.Message, "OK");
            }
        }
    }
}

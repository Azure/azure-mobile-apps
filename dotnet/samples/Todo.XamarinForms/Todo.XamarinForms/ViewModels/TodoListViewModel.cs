using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Todo.NetStandard.Common;
using Todo.XamarinForms.Extensions;
using Todo.XamarinForms.Views;
using Xamarin.Forms;

namespace Todo.XamarinForms.ViewModels
{
    /// <summary>
    /// View Model for the <see cref="TodoListPage"/>.
    /// </summary>
    public class TodoListViewModel : ViewModel
    {
        // Backing store for the IsRefreshing property
        private bool isRefreshing = false;

        // Backing store for the Items property
        private ObservableCollection<TodoItem> items;

        /// <summary>
        /// Creates a new <see cref="TodoListViewModel"/> instance
        /// </summary>
        /// <param name="navigation">The navigation context</param>
        /// <param name="repository">The data repository</param>
        public TodoListViewModel(INavigation navigation, ITodoRepository repository) 
            : base(navigation, repository)
        {
            Title = "Azure Todo List";

            // Get a copy of the list from the local store
            RefreshItemsCommand.Execute(null);

            // Respond to events from the store.
            Repository.RepositoryUpdated += OnRepositoryUpdated;
        }

        /// <summary>
        /// Event handler that allows this page to respond to changes within the repository.
        /// </summary>
        /// <param name="sender">The repository</param>
        /// <param name="e">The event causing the call</param>
        private void OnRepositoryUpdated(object sender, RepositoryEventArgs e)
        {
            switch (e.Action)
            {
                case RepositoryAction.Add:
                    Items.Add(e.Item);
                    break;
                case RepositoryAction.Delete:
                    Items.RemoveIf(m => m.Id == e.Item.Id);
                    break;
                case RepositoryAction.Update:
                    Items.ReplaceIf(m => m.Id == e.Item.Id, e.Item);
                    break;
            }
        }

        #region Bindable Properties
        /// <summary>
        /// Shows or hides the Pull-to-Refresh indicator
        /// </summary>
        public bool IsRefreshing
        {
            get => isRefreshing;
            set { isRefreshing = value; OnPropertyChanged(nameof(IsRefreshing)); }
        }

        /// <summary>
        /// The list of items to show in the ListView.
        /// </summary>
        public ObservableCollection<TodoItem> Items
        {
            get => items;
            set { items = value; OnPropertyChanged(nameof(Items)); }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Command interface called when the user wishes to add a new item.
        /// </summary>
        public ICommand AddItemCommand => new Command<Entry>(async (Entry entry) => await AddItemAsync(entry));

        /// <summary>
        /// Command interface called when the list needs to be refreshed
        /// </summary>
        public ICommand RefreshItemsCommand => new Command(async () => await RefreshItemsAsync(true));

        /// <summary>
        /// Command interface called when an item in the list has been selected
        /// </summary>
        public ICommand SelectItemCommand => new Command<TodoItem>(async (TodoItem item) => await SelectItemAsync(item));
        #endregion

        /// <summary>
        /// Add a new item (with the specified text) to the list of items
        /// </summary>
        /// <param name="text">The title of the new item</param>
        /// <returns></returns>
        private async Task AddItemAsync(Entry control)
        {
            var item = new TodoItem { Title = control.Text };
            await Repository.AddTodoItemAsync(item);

            // Cleans the Add Item EntryBox
            control.Text = string.Empty;
            control.Unfocus();
        }

        /// <summary>
        /// Refresh the list of items from the store.
        /// </summary>
        /// <param name="syncItems">If true, synchronize with the backend store.</param>
        /// <returns></returns>
        private async Task RefreshItemsAsync(bool syncItems = false)
        {
            IsRefreshing = true;
            var enumerable = await Repository.GetTodoItemsAsync();
            var newlist = new ObservableCollection<TodoItem>(enumerable);
            Items = newlist;
            IsRefreshing = false;
        }

        private async Task SelectItemAsync(TodoItem item)
        {
            await Navigation.PushModalAsync(new TodoItemEditor(Repository, item));
        }
    }
}

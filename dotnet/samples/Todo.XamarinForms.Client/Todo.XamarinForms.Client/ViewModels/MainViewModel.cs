using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Todo.NetStandard.Common;
using Todo.XamarinForms.Client.Views;
using Xamarin.Forms;

namespace Todo.XamarinForms.Client.ViewModels
{
    public class MainViewModel : ViewModel
    {
        public MainViewModel(TodoItemRepository repository)
        {
            // Listen for items being added and update the list accordingly.
            repository.OnItemAdded += (sender, item) => Items.Add(CreateTodoItemViewModel(item));

            // Listen for items being updatd and update the list accordingly.
            repository.OnItemUpdated += (sender, item) => Task.Run(async () => await LoadDataAsync());

            this.repository = repository;

            // Load the initial data
            Task.Run(async () => await LoadDataAsync());
        }

        /// <summary>
        /// The repository for the TodoItem table.
        /// </summary>
        private readonly TodoItemRepository repository;

        /// <summary>
        /// The list of Todo Items
        /// </summary>
        public ObservableCollection<TodoItemViewModel> Items { get; set; }

        /// <summary>
        /// Property that indicates if filtering is being used
        /// </summary>
        public bool ShowAll { get; set; }

        /// <summary>
        /// The text to display for the filtering state
        /// </summary>
        public string FilterText => ShowAll ? "All" : "Active";

        /// <summary>
        /// An <see cref="ICommand"/> for toggling the filtering state.
        /// </summary>
        public ICommand ToggleFilter => new Command(async () => { ShowAll = !ShowAll; await LoadDataAsync(); });

        /// <summary>
        /// An <see cref="ICommand"/> used for triggering the Add Item.
        /// </summary>
        public ICommand AddItem => new Command(async () => { var view = Resolver.Resolve<ItemView>(); await Navigation.PushAsync(view); });

        /// <summary>
        /// Property that is changed through data binding when a user selected an item.
        /// </summary>
        public TodoItemViewModel SelectedItem
        {
            get { return null; }
            set
            {
                Device.BeginInvokeOnMainThread(async () => await NavigateToItem(value));
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }

        /// <summary>
        /// Loads the data from the repository
        /// </summary>
        /// <returns></returns>
        private async Task LoadDataAsync()
        {
            var items = await repository.GetItemsAsync();
            if (!ShowAll)
            {
                items = items.Where(item => item.Completed == false).ToList();
            }
            var itemViewModels = items.Select(i => CreateTodoItemViewModel(i));
            Items = new ObservableCollection<TodoItemViewModel>(itemViewModels);
        }

        /// <summary>
        /// Creates a new view-model for the item, wiring up the ItemStatusChanged
        /// event feed at the same time.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private TodoItemViewModel CreateTodoItemViewModel(TodoItem item)
        {
            var vm = new TodoItemViewModel(item);
            vm.ItemStatusChanged += ItemStatusChanged;
            return vm;
        }

        /// <summary>
        /// Event handler that is called when an item is updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemStatusChanged(object sender, EventArgs e)
        {
            if (sender is TodoItemViewModel item)
            {
                if (!ShowAll && item.Item.Completed)
                {
                    Items.Remove(item);
                }
                Task.Run(async () => await repository.UpdateItemAsync(item.Item));
            }
        }

        /// <summary>
        /// Navigate to the details page for a specific item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task NavigateToItem(TodoItemViewModel item)
        {
            if (item == null)
            {
                return;
            }

            var view = Resolver.Resolve<ItemView>();
            var vm = view.BindingContext as ItemViewModel;
            vm.Item = item.Item;
            await Navigation.PushAsync(view);
        }

    }
}

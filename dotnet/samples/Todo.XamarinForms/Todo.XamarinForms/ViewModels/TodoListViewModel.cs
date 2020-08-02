using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.NetStandard.Common;
using Xamarin.Forms;

namespace Todo.XamarinForms.ViewModels
{
    public class TodoListViewModel : ViewModel
    {
        public TodoListViewModel(INavigation navigation, ITodoRepository repository) : base(navigation, repository)
        {
            // Blocking call - refreshes the items.  Should really be a fire-and-forget call maybe?
            RefreshItemsAsync().Wait();
        }

        #region Items Property
        private List<TodoItem> _items;

        /// <summary>
        /// The Bindable Property for the List of TodoItems
        /// </summary>
        public List<TodoItem> Items
        {
            get { return _items;  }
            set { _items = value; OnPropertyChanged(nameof(Items)); }
        }
        #endregion

        #region SelectedItem Property
        private TodoItem _selectedItem;
        
        /// <summary>
        /// The Bindable Property for the Item selected in the list
        /// </summary>
        public TodoItem SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(nameof(SelectedItem)); }
        }
        #endregion

        #region IsRefreshing Property
        private bool _isRefreshing = false;

        /// <summary>
        /// The Bindable Property that shows if the interface is refreshing or not.
        /// Used within Pull-to-Refresh or activity indicator situations.
        /// </summary>
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set { _isRefreshing = value; OnPropertyChanged(nameof(IsRefreshing)); }
        }
        #endregion

        /// <summary>
        /// Refreshes the list of items.
        /// </summary>
        /// <param name="syncItems">If true, also synchronize the backing store.</param>
        /// <returns></returns>
        private async Task RefreshItemsAsync(bool syncItems = false)
        {
            IsRefreshing = true;
            if (syncItems)
            {
                await Repository?.SynchronizeAsync();
            }
            var itemsEnumerable = await Repository?.GetTodoItemsAsync();
            Items = itemsEnumerable.ToList();
            IsRefreshing = false;
        }
    }
}

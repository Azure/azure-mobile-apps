using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using ZumoQuickstart.Models;
using ZumoQuickstart.Utils;

namespace ZumoQuickstart.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Called when the page is fully initialized.
        /// </summary>
        /// <returns></returns>
        public async Task OnActivatedAsync()
        {
            Debug.WriteLine("START ViewModel.OnActivatedAsync");
            await RefreshItemsAsync(true);
            Debug.WriteLine("BACK FROM RefreshItemsAsync");
            TodoService.Instance.TodoListUpdated += OnServiceUpdated;
            Debug.WriteLine("END ViewModel.OnActivatedAsync");
        }

        /// <summary>
        /// Event handler called when the service list is updated.  This allows the app
        /// to modify the displayed list by modifying the backend database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServiceUpdated(object sender, TodoListEventArgs e)
        {
            Debug.WriteLine("START OnServiceUpdated");
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
            Debug.WriteLine("END OnServiceUpdated");
        }

        #region Bindable Properties
        private ObservableCollection<TodoItem> _items = new ObservableCollection<TodoItem>();
        public ObservableCollection<TodoItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private bool _isRefreshing = true;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                SetProperty(ref _isRefreshing, value);
                // Also raise a PropertyChanged event for the IsNotRefreshing property
                RaisePropertyChanged(nameof(IsNotRefreshing));
            }
        }

        public bool IsNotRefreshing
        {
            get => !_isRefreshing;
        }

        private string _itemText = "";
        public string ItemText
        {
            get => _itemText;
            set => SetProperty(ref _itemText, value);
        }
        #endregion

        public async Task RefreshItemsAsync(bool syncItems = false)
        {
            Debug.WriteLine("START RefreshItemsAsync");
            IsRefreshing = true;
            Debug.WriteLine("1...");
            try
            {
                Debug.WriteLine("2...");
                if (syncItems)
                {
                    Debug.WriteLine("3...");
                    await TodoService.Instance.SynchronizeAsync();
                    Debug.WriteLine("4...");
                }
                Debug.WriteLine("5...");
                var enumerable = await TodoService.Instance.GetTodoItemsAsync();
                Debug.WriteLine("6...");
                Items = new ObservableCollection<TodoItem>(enumerable);
            }
            catch (Exception error)
            {
                Debug.WriteLine("7...");
                await DisplayAlertAsync("Refresh", error.Message);
                Debug.WriteLine("8...");
            }
            finally
            {
                Debug.WriteLine("9...");
                IsRefreshing = false;
                Debug.WriteLine("10...");
            }
            Debug.WriteLine("11...");
        }
    }
}

﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace ZumoQuickstart
{
    public class MainWindowViewModel : INotifyPropertyChanged, IAppContext
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<TodoItem> _items;
        private readonly TodoService _service;
        private bool _isRefreshing = false;

        public MainWindowViewModel()
        {
            _items = new ObservableCollection<TodoItem>();
            _service = new TodoService(this);
        }

        public async void OnActivated()
        {
            await RefreshItemsAsync().ConfigureAwait(false);
            _service.TodoListUpdated += OnServiceUpdated;
        }

        /// <summary>
        /// Part of the INotifyPropertyChanged system, this method will notify watchers that
        /// a bound property has been updated
        /// </summary>
        private void SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            storage = value;
            if (propertyName != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Displays a pop-up alert for the user to interact with.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="button"></param>
        private void DisplayAlert(string title, string message)
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);

        /// <summary>
        /// Event handler called when the service list is updated.  This allows the app
        /// to modify the displayed list by modifying the backend database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #region Bindable Properties
        public ObservableCollection<TodoItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value, nameof(Items));
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value, nameof(IsRefreshing));
        }
        #endregion

        /// <summary>
        /// Refresh the list of items from the store
        /// </summary>
        /// <param name="syncItems">If true, synchronize with the backend store prior to fetch</param>
        /// <returns></returns>
        private async Task RefreshItemsAsync(bool syncItems = false)
        {
            IsRefreshing = true;
            try
            {
                if (syncItems)
                {
                    await _service.SynchronizeAsync().ConfigureAwait(false);
                }
                var enumerable = await _service.GetTodoItemsAsync().ConfigureAwait(false);
                Items = new ObservableCollection<TodoItem>(enumerable);
            }
            catch (Exception error)
            {
                DisplayAlert("Refresh Error", error.Message);
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}

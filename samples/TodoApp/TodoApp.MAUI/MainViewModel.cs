// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Windows.Input;
using TodoApp.Data;
using TodoApp.Data.Models;
using TodoApp.Data.MVVM;

namespace TodoApp.MAUI
{
    public class MainViewModel : TodoListViewModel
    {
        public MainViewModel(IMVVMHelper helper, ITodoService service) : base(helper, service)
        {
        }

        public ICommand AddItemCommand
            => new Command<Entry>(async (Entry entry) => await AddItemAsync(entry.Text));

        public ICommand RefreshItemsCommand
            => new Command(async () => await RefreshItemsAsync());

        public ICommand SelectItemCommand
            => new Command<TodoItem>(async (TodoItem item) => await UpdateItemAsync(item.Id, !item.IsComplete));
    }
}

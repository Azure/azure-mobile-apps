// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using CoreGraphics;
using System;
using System.Threading.Tasks;
using Foundation;
using TodoApp.Data;
using TodoApp.Data.Models;
using TodoApp.iOS.DataSources;
using UIKit;
using TodoApp.Data.Services;

namespace TodoApp.iOS.ViewControllers
{
    public class HomeViewController : UIViewController
    {
        protected UIBarButtonItem addItemButton;
        protected UITableView tableView;
        protected TodoItemsTableSource tableSource;

        public ITodoService TodoService { get; }

        public HomeViewController()
        {
            Title = "Todo Items";
            TodoService = new RemoteTodoService();
            TodoService.TodoItemsUpdated += OnTodoItemsUpdated;
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Set background color
            View.BackgroundColor = UIColor.White;

            // Add Item button in nav bar
            addItemButton = new UIBarButtonItem()
            {
                Image = UIImage.GetSystemImage("plus")
            };
            addItemButton.Clicked += async (sender, e) => await OnAddItemClicked();
            NavigationItem.RightBarButtonItem = addItemButton;

            // Add the table of items
            tableSource = new TodoItemsTableSource(this);
            tableView = new UITableView
            {
                Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height),
                RefreshControl = new UIRefreshControl(),
                Source = tableSource
            };
            tableView.RefreshControl.ValueChanged += async (_,_) => await RefreshListAsync();
            Add(tableView);

            // Initialize the list
            await RefreshListAsync();
        }

        /// <summary>
        /// Refreshes the content of the list.
        /// </summary>
        public async Task RefreshListAsync()
        {
            tableView.RefreshControl?.BeginRefreshing();
            await TodoService.RefreshItemsAsync();
            tableSource.ReplaceAllItems(await TodoService.GetItemsAsync());
            tableView.RefreshControl?.EndRefreshing();
            tableView.ReloadData();
        }

        public void OnTodoItemsUpdated(object sender, TodoServiceEventArgs e)
        {
            NSIndexPath index;

            switch (e.Action)
            {
                case TodoServiceEventArgs.ListAction.Add:
                    index = tableSource.InsertItem(e.Item);
                    tableView.InsertRows(new[] { index }, UITableViewRowAnimation.None);
                    break;
                case TodoServiceEventArgs.ListAction.Delete:
                    index = tableSource.DeleteItem(e.Item);
                    tableView.DeleteRows(new[] { index }, UITableViewRowAnimation.None);
                    break;
                case TodoServiceEventArgs.ListAction.Update:
                    index = tableSource.ReplaceItem(e.Item);
                    tableView.ReloadRows(new[] { index }, UITableViewRowAnimation.None);
                    break;
            }
        }

        /// <summary>
        /// Handles when a user selects a row.
        /// </summary>
        /// <param name="item">The item that was selected</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task OnRowSelectedAsync(TodoItem item)
        {
            item.IsComplete = !item.IsComplete;
            await TodoService.SaveItemAsync(item);
        }

        /// <summary>
        /// Event handler called when the Add Item button is clicked.
        /// </summary>
        protected async Task OnAddItemClicked()
        {
            var newItemTitle = await ShowAddItemDialogAsync();
            var newItem = new TodoItem { Title = newItemTitle };
            await TodoService.SaveItemAsync(newItem);
        }

        /// <summary>
        /// Displays the Add new item? dialog.
        /// </summary>
        /// <returns>A task that returns the added item text when complete.</returns>
        protected static Task<string> ShowAddItemDialogAsync()
        {
            var tcs = new TaskCompletionSource<string>();
            var alert = new UIAlertView
            {
                Title = "Add new item?",
                AlertViewStyle = UIAlertViewStyle.PlainTextInput
            };
            nint okButtonIndex = alert.AddButton("OK");
            nint cancelButtonIndex = alert.AddButton("Cancel");
            alert.CancelButtonIndex = cancelButtonIndex;
            alert.Clicked += (_, e) =>
            {
                if (e.ButtonIndex == okButtonIndex)
                {
                    tcs.TrySetResult(alert.GetTextField(0).Text);
                }
            };
            alert.Show();
            return tcs.Task;
        }
    }
}

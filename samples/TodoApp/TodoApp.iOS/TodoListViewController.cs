// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Data.Models;
using UIKit;

namespace TodoApp.iOS
{
    [Register("TodoListViewController")]
    public class TodoListViewController : UITableViewController
    {
        private readonly ITodoService _service = TodoService.Value;
        private readonly List<TodoItem> _items = new List<TodoItem>();
        private const string CellIdentifier = "Cell";

        [Outlet]
        UITextField itemText { get; set; }

        public TodoListViewController(IntPtr handle) : base(handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();
            RefreshControl.ValueChanged += async (sender, e) => await RefreshAsync();
            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            RefreshControl.BeginRefreshing();
            await _service.RefreshItemsAsync();
            var newItems = await _service.GetItemsAsync();
            _items.Clear();
            _items.AddRange(newItems);
            RefreshControl.EndRefreshing();
            TableView.ReloadData();
        }

        #region UITableView
        public override nint RowsInSection(UITableView tableView, nint section)
            => _items.Count;

        public override nint NumberOfSections(UITableView tableView)
            => 1;

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(CellIdentifier) ?? new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);
            var label = (UILabel)cell.ViewWithTag(1);

            var item = _items[indexPath.Row];
            label.TextColor = item.IsComplete ? UIColor.DarkGray : UIColor.Black;
            label.Text = item.Title;

            return cell;
        }

        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
            => _items[indexPath.Row].IsComplete ? "Mark InProgress" : "Mark Complete";

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            => _items[indexPath.Row].IsComplete ? UITableViewCellEditingStyle.None : UITableViewCellEditingStyle.Delete;

        public async override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            var item = _items[indexPath.Row];
            var label = (UILabel)TableView.CellAt(indexPath).ViewWithTag(1);

            // Turn the text color purple to indicate saving, then do the save
            label.TextColor = UIColor.Purple;
            item.IsComplete = !item.IsComplete;
            await _service.SaveItemAsync(item);
            _items[indexPath.Row] = item;

            // Then change the color to what it's meant to be.
            label.TextColor = item.IsComplete ? UIColor.DarkGray : UIColor.Black;
        }
        #endregion

        #region UI Actions
        [Action("OnAdd:")]
        async void OnAdd(UIButton sender)
        {
            if (string.IsNullOrWhiteSpace(itemText.Text))
            {
                return;
            }

            var newItem = new TodoItem { Title = itemText.Text.Trim(), IsComplete = false };
            await _service.SaveItemAsync(newItem);
            _items.Add(newItem);

            var index = _items.FindIndex(x => x.Id == newItem.Id);
            TableView.InsertRows(new[] { NSIndexPath.FromItemSection(index, 0) }, UITableViewRowAnimation.Automatic);
            itemText.Text = "";
        }
        #endregion

        [Export("textFieldShouldReturn:")]
        public virtual bool ShouldReturn(UITextField textField)
        {
            textField.ResignFirstResponder();
            return true;
        }
    }
}
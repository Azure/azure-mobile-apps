// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using TodoApp.Data;
using TodoApp.Data.Extensions;
using TodoApp.Data.Models;
using TodoApp.iOS.ViewControllers;
using UIKit;

namespace TodoApp.iOS.DataSources
{
    public class TodoItemsTableSource : UITableViewSource
    {
        private const string cellId = "todoitem";

        public TodoItemsTableSource(UIViewController parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// The parent view controller.
        /// </summary>
        protected UIViewController Parent { get; }

        /// <summary>
        /// The list of items being presented
        /// </summary>
        protected List<TodoItem> Items { get; } = new();

        /// <summary>
        /// Refresh the items in the list.
        /// </summary>
        /// <returns>A task that completes when the refresh is complete.</returns>
        public void ReplaceAllItems(IEnumerable<TodoItem> items)
        {
            Items.Clear();
            Items.AddRange(items);
        }

        /// <summary>
        /// Inserts an item into the list.
        /// </summary>
        /// <param name="item">The item being inserted.</param>
        /// <returns>The indexpath of the item inserted.</returns>
        public NSIndexPath InsertItem(TodoItem item)
        {
            Items.Add(item);
            var index = Items.FindIndex(x => x.Id == item.Id);
            return NSIndexPath.FromItemSection(index, 0);
        }

        /// <summary>
        /// Deletes an item into the list.
        /// </summary>
        /// <param name="item">The item being deleted.</param>
        /// <returns>The indexpath of the item deleted.</returns>
        public NSIndexPath DeleteItem(TodoItem item)
        {
            var index = Items.FindIndex(x => x.Id == item.Id);
            Items.RemoveAt(index);
            return NSIndexPath.FromItemSection(index, 0);
        }

        /// <summary>
        /// Updates an item into the list.
        /// </summary>
        /// <param name="item">The item being updated.</param>
        /// <returns>The indexpath of the item updated.</returns>
        public NSIndexPath ReplaceItem(TodoItem item)
        {
            var index = Items.FindIndex(x => x.Id == item.Id);
            Items[index] = item;
            return NSIndexPath.FromItemSection(index, 0);
        }

        #region UITableViewSource
        /// <summary>
        /// Called by the <see cref="UITableView"/> to get the cell for a
        /// specific index.
        /// </summary>
        /// <param name="tableView">The calling table view.</param>
        /// <param name="indexPath">The index path of the item needed.</param>
        /// <returns>The table view cell</returns>
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (TodoItemTableViewCell)tableView.DequeueReusableCell(cellId) ?? new TodoItemTableViewCell(cellId);
            cell.UpdateCell(Items[indexPath.Row]);
            return cell;
        }

        /// <summary>
        /// Called by the <see cref="UITableView"/> when a row is selected.
        /// </summary>
        /// <param name="tableView"></param>
        /// <param name="indexPath"></param>
        public override async void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (Parent is HomeViewController home)
            {
                await home.OnRowSelectedAsync(Items[indexPath.Row]);
                tableView.DeselectRow(indexPath, true);
            }
        }

        /// <summary>
        /// Called by the <see cref="UITableView"/> to get the number of
        /// sections.
        /// </summary>
        /// <param name="tableView">The calling table view.</param>
        /// <returns>The number of sections.</returns>
        public override nint NumberOfSections(UITableView tableView)
            => 1;

        /// <summary>
        /// Called by the <see cref="UITableView"/> to get the number of
        /// items in a specific section.
        /// </summary>
        /// <param name="tableview">The calling table view.</param>
        /// <param name="section">The section being queried.</param>
        /// <returns>The number of items in the section.</returns>
        public override nint RowsInSection(UITableView tableview, nint section)
            => Items.Count;
        #endregion
    }
}

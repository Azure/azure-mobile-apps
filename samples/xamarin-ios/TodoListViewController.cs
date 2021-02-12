using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;

#pragma warning disable IDE0060 // Unused parameter.
#pragma warning disable RCS1163 // Unused parameter.

namespace ZumoQuickstart
{
    [Register("TodoListViewController")]
    public class TodoListViewController : UITableViewController
    {
        private TodoService todoService;

        [Outlet]
        UITextField itemText { get; }

        public TodoListViewController(IntPtr handle) : base(handle) { }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            todoService = TodoService.DefaultService;
            await todoService.InitializeStoreAsync();

            RefreshControl.ValueChanged += async (sender, e) => await RefreshAsync();

            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            RefreshControl.BeginRefreshing();
            await todoService.RefreshDataAsync();
            RefreshControl.EndRefreshing();
            TableView.ReloadData();
        }

        #region UITableView methods
        public override nint RowsInSection(UITableView tableView, nint section)
            => todoService?.Items?.Count ?? 0;

        public override nint NumberOfSections(UITableView tableView)
            => 1;

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            const string CellIdentifier = "Cell";
            var cell = tableView.DequeueReusableCell(CellIdentifier)
                ?? new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);

            var label = (UILabel)cell.ViewWithTag(1);
            label.TextColor = UIColor.Black;
            label.Text = todoService.Items[indexPath.Row].Text;
            return cell;
        }

        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
            => "complete";

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            => todoService.Items[indexPath.Row].Complete ? UITableViewCellEditingStyle.None : UITableViewCellEditingStyle.Delete;

        public override async void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            var item = todoService.Items[indexPath.Row];
            var label = (UILabel)TableView.CellAt(indexPath).ViewWithTag(1);
            label.TextColor = UIColor.Black;
            await todoService.CompleteItemAsync(item);
            tableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Top);
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

            var newItem = new TodoItem { Text = itemText.Text, Complete = false };
            await todoService.InsertTodoItemAsync(newItem);
            if ((todoService?.Items?.Count ?? 0) == 0)
            {
                return;
            }

            var index = todoService.Items.FindIndex(i => i.Id == newItem.Id);
            TableView.InsertRows(new[] { NSIndexPath.FromItemSection(index, 0) }, UITableViewRowAnimation.Top);
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
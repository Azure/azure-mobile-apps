using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace ZumoQuickstart
{
    public partial class TodoListViewController : UITableViewController
    {
        private TodoService _service;
        private const string CellIdentifier = "Cell";

        public TodoListViewController(IntPtr handle) : base(handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            _service = TodoService.DefaultService;

            RefreshControl.ValueChanged += async (sender, e) => await RefreshAsync();
            await RefreshAsync();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        private async Task RefreshAsync()
        {
            RefreshControl.BeginRefreshing();
            await _service.RefreshDataAsync();
            RefreshControl.EndRefreshing();
            TableView.ReloadData();
        }

        #region UITableView Method
        public override nint RowsInSection(UITableView tableView, nint section)
            => _service?.Items?.Count ?? 0;

        public override nint NumberOfSections(UITableView tableView)
            => 1;

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(CellIdentifier) ?? new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);
            var label = (UILabel)cell.ViewWithTag(1);
            label.TextColor = UIColor.Black;
            label.Text = _service.Items[indexPath.Row].Text;
            return cell;
        }

        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
            => "complete";

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            =>_service.Items[indexPath.Row].Complete ? UITableViewCellEditingStyle.None : UITableViewCellEditingStyle.Delete;

        public async override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            var item = _service.Items[indexPath.Row];

            // Change color until we actually delete it
            var label = (UILabel)TableView.CellAt(indexPath).ViewWithTag(1);
            label.TextColor = UIColor.Gray;

            await _service.CompleteItemAsync(item);
            tableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
        }
        #endregion

        #region UI Actions
        async partial void OnAdd(UIButton sender)
        {
            if (string.IsNullOrWhiteSpace(itemText.Text))
            {
                return;
            }

            var newItem = new TodoItem { Text = itemText.Text.Trim(), Complete = false };
            await _service.InsertItemAsync(newItem);
            if (_service.Items == null || _service.Items.Count == 0)
            {
                return;
            }

            var index = _service.Items.FindIndex(item => item.Id == newItem.Id);
            TableView.InsertRows(new[] { NSIndexPath.FromItemSection(index, 0) }, UITableViewRowAnimation.Automatic);
            itemText.Text = "";
        }
        #endregion

        [Export ("textFieldShouldReturn:")]
        public virtual bool ShouldReturn(UITextField textField)
        {
            textField.ResignFirstResponder();
            return true;
        }

    }
}


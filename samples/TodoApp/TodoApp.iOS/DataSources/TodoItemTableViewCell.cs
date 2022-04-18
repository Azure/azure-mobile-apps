using TodoApp.Data.Models;
using UIKit;

namespace TodoApp.iOS.DataSources
{
    public class TodoItemTableViewCell : UITableViewCell
    {
        protected UILabel titleLabel;
        protected UIImageView iconView;

        public TodoItemTableViewCell(string cellId) : base(UITableViewCellStyle.Default, cellId)
        {
            titleLabel = new UILabel
            {
                Font = UIFont.PreferredHeadline,
                TextColor = UIColor.Black
            };

            iconView = new UIImageView();

            ContentView.AddSubviews(new UIView[] { titleLabel, iconView });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            var width = ContentView.Bounds.Width;

            titleLabel.Frame = new CoreGraphics.CGRect(12, 9, width - 64, 28);
            iconView.Frame = new CoreGraphics.CGRect(width - 40, 9, 24, 24);
        }

        public void UpdateCell(TodoItem item)
        {
            titleLabel.Text = item.Title;
            iconView.Image = UIImage.GetSystemImage(item.IsComplete ? "checkmark.circle" : "circle");
            iconView.TintColor = item.IsComplete ? UIColor.SystemGreen : UIColor.LightGray;
        }
    }
}

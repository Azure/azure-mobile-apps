using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Threading.Tasks;

using AlertDialog = Android.App.AlertDialog;
using ProgressBar = Android.Widget.ProgressBar;
using EditText = Android.Widget.EditText;

namespace ZumoQuickstart
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ITodoAdapterCallback
    {
        private RecyclerView itemList;
        private FloatingActionButton addItemButton;
        private ProgressBar isBusyIndicator;
        private TodoAdapter todoAdapter;
        private TodoService todoService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));

            // Azure Mobile Apps
            CurrentPlatform.Init();
            todoService = new TodoService();

            // Xamarin Essentials
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Find the elements in the UI
            itemList = FindViewById<RecyclerView>(Resource.Id.item_list);
            addItemButton = FindViewById<FloatingActionButton>(Resource.Id.add_item_button);
            isBusyIndicator = FindViewById<ProgressBar>(Resource.Id.busy_indicator);

            // Set up the List Adapter
            todoAdapter = new TodoAdapter(this);
            itemList.SetAdapter(todoAdapter);
            itemList.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Vertical, false));

            // Set up the Floating Action Button click-handler
            addItemButton.Click += OnAddItemClicked;
        }

        protected async override void OnResume()
        {
            base.OnResume();
            Xamarin.Essentials.Platform.OnResume();
            await RefreshItemsAsync().ConfigureAwait(false);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_refresh:
                    if (isBusyIndicator.Visibility == ViewStates.Gone)
                    {
                        RefreshItemsAsync().ConfigureAwait(false);
                    }
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        /// <summary>
        /// Refresh the item list from the service.
        /// </summary>
        /// <returns></returns>
        private async Task RefreshItemsAsync()
        {
            SetIsBusy(true);

            try
            {
                await todoService.SynchronizeAsync();
                var items = await todoService.GetTodoItemsAsync();
                RunOnUiThread(() => todoAdapter.RefreshItems(items));
            }
            catch (Exception error)
            {
                ShowError(error);
            }
            finally
            {
                SetIsBusy(false);
            }
        }

        /// <summary>
        /// Event handler called when the add item button has been pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnAddItemClicked(object sender, EventArgs eventArgs)
        {
            var builder = new AlertDialog.Builder(this);
            var dialogLayout = LayoutInflater.Inflate(Resource.Layout.dialog_new_item, null);
            var control = dialogLayout.FindViewById<EditText>(Resource.Id.new_item_text);

            builder.SetTitle(GetString(Resource.String.new_item_title));
            builder.SetView(dialogLayout);
            builder.SetPositiveButton("OK", (sender, args) =>
            {
                if (control.Text != null)
                {
                    CreateItemFromDialogAsync(control.Text).ConfigureAwait(false);
                }
            });
            var dialog = builder.Create();
            dialog.Show();
        }

        private async Task CreateItemFromDialogAsync(string text)
        {
            var newItem = new TodoItem { Text = text, Complete = false };
            try
            {
                await todoService.AddTodoItemAsync(newItem);
                RunOnUiThread(() => todoAdapter.AddItem(newItem));
            }
            catch (Exception error)
            {
                ShowError(error);
            }
        }

        /// <summary>
        /// If there has been an error, show an error dialog.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private void ShowError(Exception error)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle(GetString(Resource.String.error_title));
            builder.SetMessage(error.Message);
            builder.SetPositiveButton("OK", (sender, args) =>
            {
                (sender as Dialog).Dismiss();
            });
            var dialog = builder.Create();
            dialog.Show();
        }

        /// <summary>
        /// Shows or hides the progress bar based on whether we are able to handle operations.
        /// </summary>
        /// <param name="isBusy"></param>
        private void SetIsBusy(bool isBusy)
        {
            RunOnUiThread(() =>
            {
                isBusyIndicator.Enabled = isBusy;
                isBusyIndicator.Visibility = isBusy ? ViewStates.Visible : ViewStates.Gone;
                addItemButton.Enabled = !isBusy;
            });
        }

        /// <summary>
        /// Callback from the TodoAdapter that is called when the user checks or clears
        /// the checkbox.
        /// </summary>
        /// <param name="item">The item that was modified</param>
        /// <param name="isChecked">The modification</param>
        /// <returns></returns>
        async Task ITodoAdapterCallback.UpdateItemFromListAsync(TodoItem item, bool isChecked)
        {
            item.Complete = isChecked;
            try
            {
                await todoService.UpdateTodoItemAsync(item);
                RunOnUiThread(() => todoAdapter.ReplaceItem(item));
            }
            catch (Exception error)
            {
                ShowError(error);
            }
        }
    }
}

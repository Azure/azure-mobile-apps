// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using TodoApp.Data;
using TodoApp.Data.Models;

using AlertDialog = Android.App.AlertDialog;
using ProgressBar = Android.Widget.ProgressBar;
using EditText = Android.Widget.EditText;
using FloatingActionButton = Google.Android.Material.FloatingActionButton.FloatingActionButton;
using TodoApp.Data.Services;

namespace TodoApp.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ITodoAdapterCallback
    {
        private RecyclerView itemList;
        private FloatingActionButton addItemButton;
        private ProgressBar isBusyIndicator;
        private TodoAdapter todoAdapter;

        public static ITodoService TodoService { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            itemList = FindViewById<RecyclerView>(Resource.Id.item_list);
            addItemButton = FindViewById<FloatingActionButton>(Resource.Id.add_item_button);
            isBusyIndicator = FindViewById<ProgressBar>(Resource.Id.busy_indicator);

            // Set up the TodoService
            TodoService = new RemoteTodoService();
            
            // Set up the List Adapter
            todoAdapter = new TodoAdapter(this);
            itemList.SetAdapter(todoAdapter);
            itemList.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Vertical, false));

            // Set up the FAB click-handler
            addItemButton.Click += OnAddItemClicked;
        }

        protected async override void OnResume()
        {
            base.OnResume();
            Xamarin.Essentials.Platform.OnResume();
            await RefreshItemsAsync();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
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
                        _ = Task.Run(async () => await RefreshItemsAsync());
                    }
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private async Task RefreshItemsAsync()
        {
            SetIsBusy(true);
            try
            {
                await TodoService.RefreshItemsAsync();
                var items = await TodoService.GetItemsAsync();
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

        private void OnAddItemClicked(object sender, EventArgs eventArgs)
        {
            var builder = new AlertDialog.Builder(this);
            var dialogLayout = LayoutInflater.Inflate(Resource.Layout.dialog_new_item, null);
            var control = dialogLayout.FindViewById<EditText>(Resource.Id.new_item_text);

            builder.SetTitle(GetString(Resource.String.new_item_title));
            builder.SetView(dialogLayout);
            builder.SetPositiveButton("OK", async (sender, eventArgs) => await CreateItemFromDialogAsync(control));
            var dialog = builder.Create();
            dialog.Show();
        }

        private async Task CreateItemFromDialogAsync(EditText control)
        {
            if (control.Text != null)
            {
                var item = new TodoItem { Title = control.Text, IsComplete = false };
                try
                {
                    await TodoService.SaveItemAsync(item);
                    RunOnUiThread(() => todoAdapter.AddItem(item));
                }
                catch (Exception error)
                {
                    ShowError(error);
                }
            }
        }

        private void ShowError(Exception error)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle(GetString(Resource.String.error_title));
            builder.SetMessage(error.Message);
            builder.SetPositiveButton("OK", (sender, args) => (sender as Dialog)?.Dismiss());
            var dialog = builder.Create();
            dialog.Show();
        }

        private void SetIsBusy(bool isBusy)
        {
            RunOnUiThread(() =>
            {
                isBusyIndicator.Enabled = isBusy;
                isBusyIndicator.Visibility = isBusy ? ViewStates.Visible : ViewStates.Gone;
                addItemButton.Enabled = !isBusy;
            });
        }

        async Task ITodoAdapterCallback.UpdateItemFromListAsync(TodoItem item, bool isChecked)
        {
            item.IsComplete = isChecked;
            try
            {
                await TodoService.SaveItemAsync(item);
                RunOnUiThread(() => todoAdapter.ReplaceItem(item));
            }
            catch (Exception error)
            {
                ShowError(error);
            }
        }
    }
}
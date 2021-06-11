/*
 * To add Offline Sync Support:
 *  1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 *  2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see: http://go.microsoft.com/fwlink/?LinkId=717898
 */
//#define OFFLINE_SYNC_ENABLED

using System;
using System.IO;
using System.Threading.Tasks;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using Java.Interop;
using Java.Net;

using Microsoft.WindowsAzure.MobileServices;
using ZUMOAPPNAME;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
#endif

namespace ZUMOAPPNAME
{
    [Activity(MainLauncher = true,
               Icon = "@drawable/ic_launcher", Label = "@string/app_name",
               Theme = "@style/AppTheme")]
    public class ToDoActivity : Activity
    {
        // Client reference.
        MobileServiceClient client;

#if OFFLINE_SYNC_ENABLED
        IMobileServiceSyncTable<ToDoItem> todoTable;

        const string localDbFilename = "localstore.db";
#else
        private IMobileServiceTable<ToDoItem> todoTable;
#endif

        // Adapter to map the items list to the view
        ToDoItemAdapter adapter;

        // EditText containing the "New ToDo" text
        EditText textNewToDo;

		// URL of the mobile app backend.
        const string applicationURL = @"ZUMOAPPURL";

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Activity_To_Do);

            CurrentPlatform.Init();

            // Create the client instance, using the mobile app backend URL.
            client = new MobileServiceClient(applicationURL);
#if OFFLINE_SYNC_ENABLED
            await InitLocalStoreAsync();

            // Get the sync table instance to use to store TodoItem rows.
            todoTable = client.GetSyncTable<ToDoItem>();
#else
            todoTable = client.GetTable<ToDoItem>();
#endif

            textNewToDo = FindViewById<EditText>(Resource.Id.textNewToDo);

            // Create an adapter to bind the items with the view
            adapter = new ToDoItemAdapter(this, Resource.Layout.Row_List_To_Do);
            var listViewToDo = FindViewById<ListView>(Resource.Id.listViewToDo);
            listViewToDo.Adapter = adapter;

            // Load the items from the mobile app backend.
            OnRefreshItemsSelected();
        }

#if OFFLINE_SYNC_ENABLED
        async Task InitLocalStoreAsync()
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var store = new MobileServiceSQLiteStore(Path.Combine(dir, localDbFilename));
            store.DefineTable<ToDoItem>();

            // Uses the default conflict handler, which fails on conflict
            // To use a different conflict handler, pass a parameter to InitializeAsync.
            // For more details, see http://go.microsoft.com/fwlink/?LinkId=521416
            await client.SyncContext.InitializeAsync(store);
        }

        async Task SyncAsync(bool pullData = false)
        {
            try
        {
                await client.SyncContext.PushAsync();

                if (pullData)
                {
                    await todoTable.PullAsync("allTodoItems", todoTable.CreateQuery()); // query ID is used for incremental sync
                }
            }
            catch (MalformedURLException)
            {
                CreateAndShowDialog(new Exception("There was an error creating the Mobile Service. Verify the URL"), "Error");
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }
#endif

        //Initializes the activity menu
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.activity_main, menu);
            return true;
        }

        //Select an option from the menu
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_refresh)
            {
                item.SetEnabled(false);

                OnRefreshItemsSelected();

                item.SetEnabled(true);
            }

            return true;
        }

        // Called when the refresh menu option is selected.
        async void OnRefreshItemsSelected()
        {
#if OFFLINE_SYNC_ENABLED
			// Get changes from the mobile app backend.
            await SyncAsync(true);
#endif
			// refresh view using local store.
            await RefreshItemsFromTableAsync();
        }

        //Refresh the list with the items in the local store.
        async Task RefreshItemsFromTableAsync()
        {
            try
        {
                // Get the items that weren't marked as completed and add them in the adapter
                var list = await todoTable.Where(item => item.Complete == false).ToListAsync();

                adapter.Clear();

                foreach (var current in list)
                {
                    adapter.Add(current);
            }
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        public async Task CheckItem(ToDoItem item)
        {
            if (client == null)
            {
                return;
            }

            // Set the item as completed and update it in the table
            item.Complete = true;
            try
            {
				// Update the new item in the local store.
                await todoTable.UpdateAsync(item);
#if OFFLINE_SYNC_ENABLED
                // Send changes to the mobile app backend.
				await SyncAsync();
#endif

                if (item.Complete)
                {
                    adapter.Remove(item);
            }
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        [Export]
        public async void AddItem(View view)
        {
            if (client == null || string.IsNullOrWhiteSpace(textNewToDo.Text))
            {
                return;
            }

            // Create a new item
            var item = new ToDoItem
                       {
                Text = textNewToDo.Text,
                Complete = false
            };

            try
            {
				// Insert the new item into the local store.
                await todoTable.InsertAsync(item);
#if OFFLINE_SYNC_ENABLED
                // Send changes to the mobile app backend.
				await SyncAsync();
#endif

                if (!item.Complete)
                {
                    adapter.Add(item);
                }
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }

            textNewToDo.Text = "";
        }

        void CreateAndShowDialog(Exception exception, string title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        void CreateAndShowDialog(string message, string title)
        {
            var builder = new AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }
    }
}

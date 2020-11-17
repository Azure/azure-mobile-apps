# Enable Offline Sync

This tutorial covers the offline sync feature of Azure Mobile Apps for Android. Offline sync allows end users to interact with a mobile app&mdash;viewing, adding, or modifying data&mdash;even when there is no network connection. Changes are stored in a local database. Once the device is back online, these changes are synced with the remote backend.

Prior to starting this tutorial, you should have completed the [Android Quickstart Tutorial](./index.md), which includes creating a suitable backend service.

To learn more about the offline sync feature, see the topic [Offline Data Sync in Azure Mobile Apps](../../howto/datasync.md).

## Update the app to support offline sync

In online operation, you read to and write from a `MobileServiceTable`.  When using offline sync, you read to and write from a `MobileServiceSyncTable` instead.  The `MobileServiceSyncTable` is backed by an on-device SQLite database, and synchronized with the backend database.

In `TodoActivity.java`:

1. Update the definition of the `mTable` variable (approx. line 56).  Comment out the current definition, and uncomment the offline sync version.

    ``` java linenums="56"
    private MobileServiceSyncTable<TodoItem> mTable;
    ```

2. When initializing the `mTable` variable in the `onCreate()` method (approx. line 102), change the call to use `getSyncTable()` instead.   Comment out the current definition, and uncomment the offline sync version.

    ``` java linenums="102"
    // Get a reference to the Remote table.
    // mTable = mClient.getTable(TodoItem.class);
    // OFFLINE SYNC:
    mTable = mClient.getSyncTable(TodoItem.class);
    ```

3. In the `RefreshItemsAsync` class, replace the search using the online table to a synchronization and then searching the offline table.  Comment out the current code, and uncomment the offline sync version.

    ``` java linenums="224"
    // List<TodoItem> results = mTable.where(query).execute().get();
    // OFFLINE SYNC:
    sync().get();
    List<TodoItem> results = mTable.read(query).get();
    ```

4. In the `SyncItemsAsync` class, uncomment the code that pulls changes from the remote server.

    ``` java linenums="262"
    // OFFLINE SYNC:
    mTable.pull(null).get();
    ```

## Test the app

In this section, test the behavior with WiFi on, and then turn off WiFi to create an offline scenario.

When you add data items, they are held in the local SQLite store, but not synced to the mobile service until you press the **Refresh** button. Other apps may have different requirements regarding when data needs to be synchronized, but for demo purposes this tutorial has the user explicitly request it.

When you press the **Refresh** button, a new background task starts. It first pushes all changes made to the local store using synchronization context, then pulls all changed data from Azure to the local table.

### Offline testing

1. Place the device or simulator in *Airplane Mode*. This creates an offline scenario.
2. Add some Todo items, or mark some items as complete. Quit the device or simulator (or forcibly close the app) and restart the app. Verify that your changes have been persisted on the device because they are held in the local SQLite store.
3. View the contents of the Azure *TodoItem* table either with a SQL tool such as *SQL Server Management Studio*, or a REST client such as *Fiddler* or *Postman*. Verify that the new items have *not* been synced to the server
4. Turn on WiFi in the device or simulator. Next, press the **Refresh** button.
5. View the TodoItem data again in the Azure portal. The new and changed TodoItems should now appear.

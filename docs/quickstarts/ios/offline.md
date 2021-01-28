# Enable Offline Sync

This tutorial covers the offline sync feature of Azure Mobile Apps for iOS. Offline sync allows end users to interact with a mobile app&mdash;viewing, adding, or modifying data&mdash;even when there is no network connection. Changes are stored in a local database. Once the device is back online, these changes are synced with the remote backend.

Prior to starting this tutorial, you should have completed the [iOS Quickstart Tutorial](./index.md), which includes creating a suitable backend service.

To learn more about the offline sync feature, see the topic [Offline Data Sync in Azure Mobile Apps](../../howto/datasync.md).

## Update the app to support offline sync

In online operation, you read to and write from a `MSTable`.  When using offline sync, you read to and write from a `MSSyncTable` instead.  The `MSSyncTable` is backed by an on-device Core Data database, and synchronized with the backend database.

Review the `ZumoQuickstart.xcdatamodel` contents.  This contains the definition of the tables required to support Azure Mobile Apps.  Specifically, there are three required tables:

* `MS_TableConfig` holds information about each table, including the last sync time.
* `MS_TableOperationsErrors` holds information about operational errors.
* `MS_TableOperations` holds data changes prior to sending them to the service.

You will also need a local table that matches each of the tables in use within your application.  In the case of the quick start project, we are using one table called `TodoItem`.

In the `TodoService.swift` class:

1. Update the definition of the `table` variable.  Comment out the current definition, and uncomment the offline sync version.

    ``` swift linenums="16"
        // private var table: MSTable?
        private var table: MSSyncTable?
    ```

2. Update the `initialize()` method to initialize the table:

    ``` swift linenums="24"
    private func initialize(completion: (Error?) -> Void) {
        let context = (UIApplication.shared.delegate as! AppDelegate).managedObjectContext!
        self.store = MSCoreDataStore(managedObjectContext: context)
        self.client.syncContext = MSSyncContext(delegate: nil, dataSource: self.store, callback: nil)
        self.table = client.syncTable(withName: "TodoItem")
        completion(nil)
    }
    ```

3. Replace `syncItems()` method that will synchronize the data in the offline store with the online store:

    > TODO: Add syncItems() to the main project and call it at appropriate points.
    > TODO: Fix the syncItems() code within the docs.

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

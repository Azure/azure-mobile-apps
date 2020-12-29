# Offline Data Sync

Offline data sync is a client and server SDK feature of Azure Mobile Apps that makes it easy for developers to create apps that are functional without a network connection.

When your app is in offline mode, you can still create and modify data, which are saved to a local store. When the app is back online, it can synchronize local changes with your Azure Mobile App backend. The feature also includes support for detecting conflicts when the same record is changed on both the client and the backend. Conflicts can then be handled either on the server or the client.

Offline sync has several benefits:

* Improve app responsiveness by caching server data locally on the device
* Create robust apps that remain useful when there are network issues
* Allow end users to create and modify data even when there is no network access, supporting scenarios with little or no connectivity
* Sync data across multiple devices and detect conflicts when the same record is modified by two devices
* Limit network use on high-latency or metered networks

The following tutorials show how to add offline sync to your mobile clients using Azure Mobile Apps:

* [Android: Enable offline sync](../quickstarts/android/offline.md)
* [Apache Cordova: Enable offline sync](../quickstarts/cordova/offline.md)
* [iOS: Enable offline sync](../quickstarts/ios/offline.md)
* [Windows (UWP): Enable offline sync](../quickstarts/uwp/offline.md)
* [Windows (WPF): Enable offline sync](../quickstarts/wpf/offline.md)
* [Xamarin.Android: Enable offline sync](../quickstarts/xamarin-android/offline.md)
* [Xamarin.Forms: Enable offline sync](../quickstarts/xamarin-forms/offline.md)
* [Xamarin.iOS: Enable offline sync](../quickstarts/xamarin-ios/offline.md)

## What is a sync table?

To access the "/tables" endpoint, the Azure Mobile client SDKs provide interfaces such as `IMobileServiceTable` (.NET client SDK) or `MSTable` (iOS client). These APIs connect directly to the Azure Mobile App backend and fail if the client device does not have a network connection.

To support offline use, your app should instead use the *sync table* APIs, such as `IMobileServiceSyncTable` (.NET client SDK) or `MSSyncTable` (iOS client). All the same CRUD operations (Create, Read, Update, Delete) work against sync table APIs, except now they read from or write to a *local store*. Before any sync table operations can be performed, the local store must be initialized.

## What is a local store?

A local store is the data persistence layer on the client device. The Azure Mobile Apps client SDKs provide a default local store implementation. On Windows, Xamarin and Android, it is based on SQLite. On iOS, it is based on Core Data.

To use the SQLite-based implementation on Windows Phone or Microsoft Store, you need to install a SQLite extension. For more information, see [Windows (UWP): Enable offline sync](../quickstarts/uwp/offline.md). Android and iOS ship with a version of SQLite in the device operating system itself, so it is not necessary to reference your own version of SQLite.

Developers can also implement their own local store. For instance, if you wish to store data in an encrypted format on the mobile client, you can define a local store that uses SQLCipher for encryption.

## What is a sync context?

A *sync context* is associated with a mobile client object (such as `IMobileServiceClient` or `MSClient`) and tracks changes that are made with sync tables. The sync context maintains an *operation queue*, which keeps an ordered list of CUD operations (Create, Update, Delete) that is later sent to the server.

A local store is associated with the sync context using an initialize method such as `IMobileServicesSyncContext.InitializeAsync(localstore)` in the .NET client SDK.

## How offline sync works

When using sync tables, your client code controls when local changes are synchronized with an Azure Mobile App backend. Nothing is sent to the backend until there is a call to *push* local changes. Similarly, the local store is populated with new data only when there is a call to *pull* data.

### Push

Push is an operation on the sync context and sends all CUD changes since the last push. Note that it is not possible to send only an individual table's changes, because otherwise operations could be sent out of order. Push executes a series of REST calls to your Azure Mobile App backend, which in turn modifies your server database.

### Pull

Pull is performed on a per-table basis and can be customized with a query to retrieve only a subset of the server data. The Azure Mobile client SDKs then insert the resulting data into the local store.

### Implicit Push

If a pull is executed against a table that has pending local updates, the pull first executes a `push()` on the sync context. This push helps minimize conflicts between changes that are already queued and new data from the server.

### Incremental Sync

The first parameter to the pull operation is a *query name* that is used only on the client. If you use a non-null query name, the Azure Mobile SDK performs an *incremental sync*. Each time a pull operation returns a set of results, the latest `updatedAt` timestamp from that result set is stored in the SDK local system tables. Subsequent pull operations retrieve only records after that timestamp.

To use incremental sync, your server must return meaningful `updatedAt` values and must also support sorting by this field. However, since the SDK adds its own sort on the updatedAt field, you cannot use a pull query that has its own `orderBy` clause.

The query name can be any string you choose, but it must be unique for each logical query in your app. Otherwise, different pull operations could overwrite the same incremental sync timestamp and your queries can return incorrect results.

If the query has a parameter, one way to create a unique query name is to incorporate the parameter value. For instance, if you are filtering on userid, your query name could be as follows (in C#):

``` csharp
await todoTable.PullAsync("todoItems" + userid, 
    syncTable.Where(u => u.UserId == userid));
```

If you want to opt out of incremental sync, pass `null` as the query ID. In this case, all records are retrieved on every call to `PullAsync`, which is potentially inefficient.

### Purging

You can clear the contents of the local store using `IMobileServiceSyncTable.PurgeAsync`. Purging may be necessary if you have stale data in the client database, or if you wish to discard all pending changes.

A purge clears a table from the local store. If there are operations awaiting synchronization with the server database, the purge throws an exception unless the *force purge* parameter is set.

As an example of stale data on the client, suppose in the "todo list" example, Device1 only pulls items that are not completed. A todoitem "Buy milk" is marked completed on the server by another device. However, Device1 still has the "Buy milk" todoitem in local store because it is only pulling items that are not marked complete. A purge clears this stale item.

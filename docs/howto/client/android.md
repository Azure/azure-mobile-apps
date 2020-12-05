# How to use the Azure Mobile Apps SDK for Android

This guide shows you how to use the Android client SDK for Mobile Apps to implement common scenarios, such as:

* Querying for data (inserting, updating, and deleting).
* Authentication.
* Handling errors.
* Customizing the client.

Additionally, you can find the [Javadocs API reference][12] for the Android client library on GitHub.

## Supported Platforms

The Azure Mobile Apps SDK for Android supports API level 19+ for phone and tablet form factors.  Authentication, in particular, utilizes a common web framework approach to gather credentials.  Server-flow authentication does not work with small form factor devices such as watches.

## Configuring your project.

To configure your Android project, you must add the dependencies to your `build.gradle` files, and enable the `INTERNET` permission in your Android manifest.

### <a name="gradle-build"></a>Update the Gradle build file

Change both **build.gradle** files:

1. Add this code to the *Project* level **build.gradle** file:

    ``` gradle
    buildscript {
        repositories {
            jcenter()
            google()
        }
    }

    allprojects {
        repositories {
            jcenter()
            google()
        }
    }
    ```

2. Add this code to the *Module app* level **build.gradle** file inside the *dependencies* tag:

    ``` gradle
    // Required for Azure Mobile Apps
    implementation 'com.google.code.gson:gson:2.8.6'
    implementation 'com.google.guava:guava:24.1-jre'
    implementation 'com.squareup.okhttp3:okhttp:3.12.4'
    implementation 'com.microsoft.azure:azure-mobile-android:3.5.1@aar'

    // Optional, for logging requests.
    implementation 'com.squareup.okhttp3:logging-interceptor:3.12.12'

    // Optional, for supporting authentication
    implementation 'com.android.support:customtabs:28.0.0'
    ```

    Currently the latest version is 3.5.1. The supported versions are listed [on bintray][14].

### <a name="enable-internet"></a>Enable internet permission

To access Azure, your app must have the `INTERNET` permission enabled. If it's not already enabled, add the following line of code to your **AndroidManifest.xml** file:

``` xml
<uses-permission android:name="android.permission.INTERNET" />
```

## Create a Client Connection

Azure Mobile Apps provides four functions to your mobile application:

* Authentication with Azure App Service Authentication and Authorization.
* Data Access and Offline Synchronization with an Azure Mobile Apps Service.
* Call Custom APIs written with the Azure Mobile Apps Server SDK.
* Push Notification Registration with Notification Hubs.

Each of these functions first requires that you create a `MobileServiceClient` object.  Only one `MobileServiceClient` object should be created within your mobile client (that is, it should be a Singleton pattern).  To create a `MobileServiceClient` object:

=== "Java"

    ``` java
    MobileServiceClient mClient = new MobileServiceClient(
        "<MobileAppUrl>",       // Replace with the Site URL
        this);                  // Your application Context
    ```

=== "Kotlin"

    ``` kotlin
    val mClient = MobileServiceClient(
        "<MobileAppUrl>",       // Replace with the Site URL
        this)                   // Your application Context
    )
    ```

The `<MobileAppUrl>` is either a string or a URL object that points to your mobile backend.  If you are using Azure App Service to host your mobile backend, then ensure you use the secure `https://` version of the URL.

The client also requires access to the Activity or Context - the `this` parameter in the example.  The MobileServiceClient construction should happen within the `onCreate()` method of the Activity referenced in the `AndroidManifest.xml` file.

As a best practice, you should abstract server communication into its own (singleton-pattern) class.  In this case, you should pass the Activity within the constructor to appropriately configure the service.  For example:

=== "Java"

    ``` java
    public class AzureServiceAdapter {
        private String mMobileBackendUrl = "https://myappname.azurewebsites.net";
        private Context mContext;
        private MobileServiceClient mClient;
        private static AzureServiceAdapter mInstance = null;

        private AzureServiceAdapter(Context context) {
            mContext = context;
            mClient = new MobileServiceClient(mMobileBackendUrl, mContext);
        }

        public static void initialize(Context context) {
            if (mInstance == null) {
                mInstance = new AzureServiceAdapter(context);
            } else {
                throw new IllegalStateException("AzureServiceAdapter is already initialized");
            }
        }

        public static AzureServiceAdapter getInstance() {
            if (mInstance == null) {
                throw new IllegalStateException("AzureServiceAdapter is not initialized");
            }
            return mInstance;
        }

        // Place any public methods that operate on mClient here.
    }
    ```

=== "Kotlin"

    ``` kotlin
    class AzureServiceAdapter private constructor() {
        private object Singleton {
            val INSTANCE = TodoService()
        }

        companion object {
            val instance: TodoService by lazy { Singleton.INSTANCE }
        }

        private lateinit var mClient: MobileServiceClient

        fun initialize(context: Context) {
            mClient = MobileServiceClient(Constants.BackendUrl, context)
        }

        // Place any public methods that operate on mClient here.
    }
    ```


You can now call the `initialize()` method within the `onCreate()` method of your main activity:

=== "Java"

    ``` java
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        AzureServiceAdapter.getInstance().initialize(this);
    }
    ```

=== "Kotlin"

    ``` kotlin
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        // Rest of UI Setup

        AzureServiceAdapter.instance.initialize(this)
    }
    ```

In larger applications, you will want to use a dependency injection package to properly initialize the client for online operations.

### Implementing logging

The Azure Mobile Apps client defers all HTTP client creation to a client factory that creates an [OkHttp] client when required.  You can override the default client factory to adjust the [OkHttp] client settings.  For example, to add connection logging:

=== "Java"

    ``` java
    HttpLoggingInterceptor loggingInterceptor = new HttpLoggingInterceptor();
    loggingInterceptor.level = HttpLoggingInterceptor.Level.BODY;

    mClient.setAndroidHttpClientFactory(new OkHttpClientFactory() {
        @Override
        public OkHttpClient createOkHttpClient() {
            OkHttpClient client = new OkHttpClient.Builder()
                    .addInterceptor(loggingInterceptor)
                    .build();

            return client;
        }
    });
    ```

=== "Kotlin"

    ``` kotlin
    mClient.setAndroidHttpClientFactory {
        OkHttpClient.Builder()
            .addInterceptor(HttpLoggingInterceptor().apply { 
                level = HttpLoggingInterceptor.Level.BODY 
            })
            .build()
    }
    ```
### Adjusting the HTTP connection timeouts

In a similar way, you can adjust the connect and read timeout if you expect your app will operate on slow links.  Create an OkHttpClientFactory and use the callback to create the appropriate client reference.

=== "Java"

    ``` java
    mClient.setAndroidHttpClientFactory(new OkHttpClientFactory() {
        @Override
        public OkHttpClient createOkHttpClient() {
            OkHttpClient client = new OkHttpClient.Builder()
                    .connectTimeout(20, TimeUnit.SECONDS)
                    .readTimeout(20, TimeUnit.SECONDS)
                    .build();

            return client;
        }
    });
    ```

=== "Kotlin"

    ``` kotlin
    mClient.setAndroidHttpClientFactory {
        OkHttpClient.Builder()
            .connectTimeout(20, TimeUnit.SECONDS)
            .readTimeout(20, TimeUnit.SECONDS)
            .build()
    }
    ```

You can use this method to add additional headers.  This provides a mechanism for adding additional authentication mechanisms that use the `Authorization` header, as an example.

## Implement a Progress Filter

You can implement an intercept of every request by implementing a `ServiceFilter`.  For example, the following updates a pre-created progress bar:

=== "Java"

    ``` java
    private class ProgressFilter implements ServiceFilter {
        @Override
        public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback next) {
            final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    if (mProgressBar != null) mProgressBar.setVisibility(ProgressBar.VISIBLE);
                }
            });

            ListenableFuture<ServiceFilterResponse> future = next.onNext(request);
            Futures.addCallback(future, new FutureCallback<ServiceFilterResponse>() {
                @Override
                public void onFailure(Throwable e) {
                    resultFuture.setException(e);
                }
                @Override
                public void onSuccess(ServiceFilterResponse response) {
                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            if (mProgressBar != null)
                                mProgressBar.setVisibility(ProgressBar.GONE);
                        }
                    });
                    resultFuture.set(response);
                }
            });
            return resultFuture;
        }
    }
    ```

=== "Kotlin"

    ``` kotlin
    private class ProgressFilter : ServiceFilter {
        override fun handleRequest(
            request: ServiceFilterRequest?, 
            next: NextServiceFilterCallback
        ): ListenableFuture<ServiceFilterResponse> {
            val resultFuture = SettableFuture.create()
            runOnUiThread  { mProgressBar?.setVisibility(ProgressBar.VISIBLE) }

            val future = next.onNext(request)
            future.addListener({
                try {
                    val response = future.get()
                    resultFuture.set(response)
                } catch (ex: Exception) {
                    resultFuture.setException(ex)
                }
            }, executor)

            return resultFuture
        }
    }
    ```

You can attach this filter to the client as follows:

=== "Java"

    ``` java
    mClient = new MobileServiceClient(applicationUrl, this).withFilter(new ProgressFilter());
    ```

=== "Kotlin"

    ``` java
    mClient = MobileServiceClient(applicationUrl, this).withFilter(ProgressFilter());
    ```
## Data Operations

The core of the Azure Mobile Apps SDK is to provide access to data stored within SQL Azure on the Mobile App backend.  You can access this data using strongly typed classes (preferred) or untyped queries (not recommended).  The bulk of this section deals with using strongly typed classes.

### Define client data classes

To access data from SQL Azure tables, define client data classes that correspond to the tables in the Mobile App backend. Examples in this topic assume a table named **MyDataTable**, which has the following columns:

* `id`
* `text`
* `complete`

The corresponding typed client-side object resides in a "POCO" class:

=== "Java"

    ``` java
    public class MyDataTable {
        private String mId;
        private String mText;
        private Boolean mComplete;

        public String getId() {
            return mId;
        }

        public void setId(String id) {
            mId = id;
        }

        public String getText() {
            return mText;
        }

        public void setText(String text) {
            mText = text;
        }

        public Boolean isComplete() {
            return mComplete;
        }

        public void setComplete(Boolean complete) {
            mComplete = complete;
        }
    }
    ```

=== "Kotlin"

    ``` kotlin
    data class MyDataTable(
        var id: String?, 
        var text: String, 
        var complete: Boolean = false
    )
    ```

The `id` field is typically set by the server, so you should not set this yourself.  In Kotlin, define the `id` field as a nullable string.

An Azure Mobile Apps backend table defines five special fields, four of which are available to clients:

* `String id`: The globally unique ID for the record.  As a best practice, make the id the String representation of a [UUID][17] object.
* `DateTimeOffset updatedAt`: The date/time of the last update.  The updatedAt field is set by the server and should never be set by your client code.
* `DateTimeOffset createdAt`: The date/time that the object was created.  The createdAt field is set by the server and should never be set by your client code.
* `byte[] version`: Normally represented as a string, the version is also set by the server.
* `boolean deleted`: Indicates that the record has been deleted but not purged yet.  Do not use `deleted` as a property in your class.

The `id` field is required.  The `updatedAt` field and `version` field are used for offline synchronization (for incremental sync and conflict resolution respectively).  The `createdAt` field is a reference field and is not used by the client.  The names are "across-the-wire" names of the properties and are not adjustable.  However, you can create a mapping between your object and the "across-the-wire" names using the [gson][3] library.  For example:

=== "Java"

    ```java
    package com.example.zumoappname;

    import com.google.gson.annotations.SerializedName;
    import com.microsoft.windowsazure.mobileservices.table.DateTimeOffset;

    public class MyDataTable
    {
        @SerializedName("id")
        private String mId;
        public String getId() { return mId; }
        public final void setId(String id) { mId = id; }

        @SerializedName("complete")
        private boolean mComplete;
        public boolean isComplete() { return mComplete; }
        public void setComplete(boolean complete) { mComplete = complete; }

        @SerializedName("text")
        private String mText;
        public String getText() { return mText; }
        public final void setText(String text) { mText = text; }

        @SerializedName("createdAt")
        private DateTimeOffset mCreatedAt;
        public DateTimeOffset getCreatedAt() { return mCreatedAt; }
        protected void setCreatedAt(DateTimeOffset createdAt) { mCreatedAt = createdAt; }

        @SerializedName("updatedAt")
        private DateTimeOffset mUpdatedAt;
        public DateTimeOffset getUpdatedAt() { return mUpdatedAt; }
        protected void setUpdatedAt(DateTimeOffset updatedAt) { mUpdatedAt = updatedAt; }

        @SerializedName("version")
        private String mVersion;
        public String getVersion() { return mVersion; }
        public final void setVersion(String version) { mVersion = version; }

        public TodoItem() { }

        public TodoItem(String id, String text) {
            this.setId(id);
            this.setText(text);
        }

        @Override
        public boolean equals(Object o) {
            return o instanceof TodoItem && ((TodoItem) o).mId == mId;
        }

        @Override
        public String toString() {
            return getText();
        }
    }
    ```

=== "Kotlin"

    ``` kotlin
    package com.example.zumoappname;

    import com.google.gson.annotations.SerializedName;
    import com.microsoft.windowsazure.mobileservices.table.DateTimeOffset;

    data class MyDataTable(
        @SerializedName("id") var id: String?,
        @SerializedName("text") var text: String,
        @SerializedName("complete") var complete: Boolean = false,
        @SerializedName("createdAt") var createdAt: DateTimeOffset? = null,
        @SerializedName("updatedAt") var updatedAt: DateTimeOffset? = null,
        @SerializedName("version") var version: String? = null
    )
    ```

### Create a Table Reference

To access a table, first create a [MobileServiceTable][8] object by calling the **getTable** method on the [MobileServiceClient][9].  This method has two overloads:

``` java
public class MobileServiceClient {
    public <E> MobileServiceTable<E> getTable(Class<E> clazz);
    public <E> MobileServiceTable<E> getTable(String name, Class<E> clazz);
}
```

In the following code, **mClient** is a reference to your MobileServiceClient object.  The first overload is used where the class name and the table name are the same, and is the one used in the Quickstart:

=== "Java"

    ``` java
    MobileServiceTable<TodoItem> mTable = mClient.getTable(MyDataTable.class);
    ```

=== "Kotlin"

    ``` kotlin
    val mTable = mClient.getTable(MyDataTable::class.java)
    ```

The second overload is used when the table name is different from the class, The first parameter is the table name according to the server (and exposed as a REST endpoint under `/tables`).

=== "Java"

    ``` java
    MobileServiceTable<TodoItem> mTable = mClient.getTable("MyTable", MyDataTable.class);
    ```

=== "Kotlin"

    ``` kotlin
    val mTable = mClient.getTable("MyTable", MyDataTable::class.java)
    ```

To avoid confusion, you should almost always match the class name to the table name on the server (i.e. use the one parameter version of the `getTable()` method).

## <a name="query"></a>Query a Backend Table

First, obtain a table reference.  Then execute a query on the table reference.  A query is any combination of:

* A `.where()` [filter clause](#filtering).
* An `.orderBy()` [ordering clause](#sorting).
* A `.select()` [field selection clause](#selection).
* A `.skip()` and `.top()` for [paged results](#paging).

The clauses must be presented in the preceding order.

### <a name="filter"></a> Filtering Results

The general form of a query is:

=== "Java"

    ``` java
    List<MyDataTable> results = mTable
        // More filters here
        .execute()          // Returns a ListenableFuture<E>
        .get()              // Converts the async into a sync result
    ```

=== "Kotlin"

    ``` kotlin
    val results = mTable
        // More filters here
        .execute()          // Returns a ListenableFuture<E>
        .get()              // Converts the async into a sync result
    ```

Azure Mobile Apps uses [Guava](https://github.com/google/guava/wiki/ListenableFutureExplained) to report results from asynchronous operations back to your application.

The preceding example returns all results (up to the maximum page size set by the server).  The `.execute()` method executes the query on the backend.  The query is converted to an [OData v3][19] query before transmission to the Mobile Apps backend.  On receipt, the Mobile Apps backend converts the query into an SQL statement before executing it on the SQL Azure instance.  Since network activity takes some time, The `.execute()` method returns a [`ListenableFuture<E>`][18].

### <a name="filtering"></a>Filter returned data

The following query execution returns all items from the **TodoItem** table where **complete** equals **false**.

=== "Java"

    ``` java
    List<TodoItem> results = mTable
        .where()
        .field("complete").eq(false)
        .execute()
        .get();
    ```

=== "Kotlin"

    ``` kotlin
    val results = mTable
        .where()
        .field("complete").eq(false)
        .execute()
        .get();
    ```

**mTable** is the reference to the mobile service table that we created previously.

Define a filter using the **where** method call on the table reference. The **where** method is followed by a **field** method followed by a method that specifies the logical predicate. Possible predicate methods include **eq** (equals), **ne** (not equal), **gt** (greater than), **ge** (greater than or equal to), **lt** (less than), **le** (less than or equal to). These methods let you compare number and string fields to specific values.

You can filter on dates. The following methods let you compare the entire date field or parts of the date: **year**, **month**, **day**, **hour**, **minute**, and **second**. The following example adds a filter for items whose *due date* equals 2013.

=== "Java"

    ``` java
    List<TodoItem> results = mTable
        .where()
        .year("due").eq(2013)
        .execute()
        .get();
    ```

=== "Kotlin"

    ``` kotlin
    val results = mTable
        .where()
        .year("due").eq(2013)
        .execute()
        .get();
    ```

The following methods support complex filters on string fields: **startsWith**, **endsWith**, **concat**, **subString**, **indexOf**, **replace**, **toLower**, **toUpper**, **trim**, and **length**. The following example filters for table rows where the *text* column starts with "PRI0."

=== "Java"

    ``` java
    List<TodoItem> results = mTable
        .where()
        .startsWith("text", "PRI0")
        .execute()
        .get();
    ```

=== "Kotlin"

    ``` kotlin
    val results = mTable
        .where()
        .startsWith("text", "PRI0")
        .execute()
        .get();
    ```

The following operator methods are supported on number fields: **add**, **sub**, **mul**, **div**, **mod**, **floor**, **ceiling**, and **round**. The following example filters for table rows where the **duration** is an even number.

=== "Java"

    ``` java
    List<TodoItem> results = mTable
        .where()
        .field("duration").mod(2).eq(0)
        .execute()
        .get();
    ```

=== "Kotlin"

    ``` kotlin
    val results = mTable
        .where()
        .field("duration").mod(2).eq(0)
        .execute()
        .get();
    ```

You can combine predicates with these logical methods: **and**, **or** and **not**. The following example combines two of the preceding examples.

=== "Java"

    ``` java
    List<TodoItem> results = mTable
        .where()
        .year("due").eq(2013).and().startsWith("text", "PRI0")
        .execute()
        .get();
    ```

=== "Kotlin"

    ``` kotlin
    val results = mTable
        .where()
        .year("due").eq(2013).and().startsWith("text", "PRI0")
        .execute()
        .get();
    ```

Group and nest logical operators:

=== "Java"

    ``` java
    List<TodoItem> results = mTable
        .where()
        .year("due").eq(2013)
        .and(
            startsWith("text", "PRI0")
            .or()
            .field("duration").gt(10)
        )
        .execute().get();
    ```

=== "Kotlin"

    ``` java
    val results = mTable
        .where()
        .year("due").eq(2013)
        .and(
            startsWith("text", "PRI0")
            .or()
            .field("duration").gt(10)
        )
        .execute().get();
    ```

### <a name="sorting"></a>Sort returned data

The following code returns all items from a table of **Items** sorted ascending by the *text* field. *mTable* is the reference to the backend table that you created previously:

=== "Java"

    ``` java
    List<Item> results = mTable
        .orderBy("text", QueryOrder.Ascending)
        .execute()
        .get();
    ```

=== "Kotlin"

    ``` kotlin
    val results = mTable
        .orderBy("text", QueryOrder.Ascending)
        .execute()
        .get();
    ```

The first parameter of the **orderBy** method is a string equal to the name of the field on which to sort. The second parameter uses the **QueryOrder** enumeration to specify whether to sort ascending or descending.  If you are filtering using the ***where*** method, the ***where*** method must be invoked before the ***orderBy*** method.

### <a name="selection"></a>Select specific columns

The following code illustrates how to return all items from a table of **Items**, but only displays the **complete** and **text** fields. **mTable** is the reference to the backend table that we created previously.

=== "Java"

    ``` java
    List<ItemNarrow> results = mTable
        .select("complete", "text")
        .execute()
        .get();
    ```

=== "Kotlin"

    ``` kotlin
    val results = mTable
        .select("complete", "text")
        .execute()
        .get();
    ```

The parameters to the select function are the string names of the table's columns that you want to return.  The **select** method needs to follow methods like **where** and **orderBy**. It can be followed by paging methods like **skip** and **top**.

### <a name="paging"></a>Return data in pages

Data is **ALWAYS** returned in pages.  The maximum number of records returned is set by the server.  If the client requests more records, then the server returns the maximum number of records.  By default, the maximum page size on the server is 50 records.

The first example shows how to select the top five items from a table. The query returns the items from a table of **TodoItems**. **mTable** is the reference to the backend table that you created previously:

=== "Java"

    ``` java
    List<TodoItem> results = mTable.top(5).execute().get();
    ```

=== "Kotlin"

    ``` kotlin
    val results = mTable.top(5).execute().get();
    ```

Here's a query that skips the first five items, and then returns the next five:

=== "Java"

    ``` java
    List<TodoItem> results = mTable
        .skip(5).top(5)
        .execute()
        .get();
    ```

=== "Kotlin"

    ``` kotlin
    val results = mTable
        .skip(5).top(5)
        .execute()
        .get();
    ```

If you wish to get all records in a table, implement code to iterate over all pages:

=== "Java"

    ``` java
    List<Item> results = new ArrayList<>();
    int nResults;
    do {
        int currentCount = results.size();
        List<Item> page = mTable.skip(currentCount).top(500).execute().get();
        nResults = page.size();
        if (nResults > 0) {
            results.addAll(pagedResults);
        }
    } while (nResults > 0);
    ```

=== "Kotlin"

    ``` kotlin
    val results = mutableListOf<Item>()
    do {
        val page = mTable.skip(results.size).top(500).execute().get()
        if (page.isNotEmpty()) {
            results.addAll(page)
        }
    } while (page.isNotEmpty())
    ```

A request for all records using this method creates a minimum of two requests to the Mobile Apps backend.

> [!TIP]
> Choosing the right page size is a balance between memory usage while the request is happening, bandwidth usage and delay in receiving the data completely.  The default (50 records) is suitable for all devices.  If you exclusively operate on larger memory devices, increase up to 500.  We have found that increasing the page size beyond 500 records results in unacceptable delays and large memory issues.

### <a name="chaining"></a>How to: Concatenate query methods

The methods used in querying backend tables can be concatenated. Chaining query methods allows you to select specific columns of filtered rows that are sorted and paged. You can create complex logical filters.  Each query method returns a Query object. To end the series of methods and actually run the query, call the **execute** method. For example:

=== "Java"

    ``` java
    List<TodoItem> results = mTable
            .where()
            .year("due").eq(2013)
            .and(
                startsWith("text", "PRI0").or().field("duration").gt(10)
            )
            .orderBy(duration, QueryOrder.Ascending)
            .select("id", "complete", "text", "duration")
            .skip(200).top(100)
            .execute()
            .get();
    ```

=== "Kotlin"

    ``` kotlin
    val results = mTable
        .where()
        .year("due").eq(2013)
        .and(
            startsWith("text", "PRI0").or().field("duration").gt(10)
        )
        .orderBy(duration, QueryOrder.Ascending)
        .select("id", "complete", "text", "duration")
        .skip(200).top(100)
        .execute()
        .get();
    ```

The chained query methods must be ordered as follows:

1. Filtering (**where**) methods.
2. Sorting (**orderBy**) methods.
3. Selection (**select**) methods.
4. paging (**skip** and **top**) methods.

## <a name="binding"></a>Bind data to the user interface

Data binding involves three components:

* The data source
* The screen layout
* The adapter that ties the two together.

In our sample code, we return the data from the Mobile Apps SQL Azure table **TodoItem** into an array. This activity is a common pattern for data applications.  Database queries often return a collection of rows that the client gets in a list or array. In this sample, the array is the data source.  The code specifies a screen layout that defines the view of the data that appears on the device.  The two are bound together with an adapter, which in this code is an extension of the **ArrayAdapter&lt;TodoItem&gt;** class.

#### <a name="layout"></a>Define the Layout

The layout is defined by several snippets of XML code. Given an existing layout, the following code represents the **ListView** we want to populate with our server data.

```xml
<ListView
    android:id="@+id/listViewToDo"
    android:layout_width="match_parent"
    android:layout_height="wrap_content"
    tools:listitem="@layout/row_list_to_do" >
</ListView>
```

In the preceding code, the *listitem* attribute specifies the id of the layout for an individual row in the list. This code specifies a check box and its associated text and gets instantiated once for each item in the list. This layout does not display the **id** field, and a more complex layout would specify additional fields in the display. This code is in the **row_list_to_do.xml** file.

```xml
<?xml version="1.0" encoding="utf-8"?>
<LinearLayout xmlns:android="http://schemas.android.com/apk/res/android"
    android:layout_width="match_parent"
    android:layout_height="match_parent"
    android:orientation="horizontal">
    <CheckBox
        android:id="@+id/checkTodoItem"
        android:layout_width="wrap_content"
        android:layout_height="wrap_content"
        android:text="@string/checkbox_text" />
</LinearLayout>
```

#### <a name="adapter"></a>Define the adapter

Since the data source of our view is an array of **TodoItem**, we subclass our adapter from an **ArrayAdapter&lt;TodoItem&gt;** class. This subclass produces a View for every **TodoItem** using the **list_item** layout.  In our code, we define the following class that is an extension of the **ArrayAdapter&lt;E&gt;** class:

=== "Java"

    ``` java
    public class TodoItemAdapter extends ArrayAdapter<TodoItem> {
        // Implementation
    }
    ```

=== "Kotlin"

    ``` kotlin
    class TodoItemAdapter : ArrayAdapter<TodoItem> {
        // Implementation
    }

Override the adapters **getView** method. For example:

=== "Java"

    ```java
    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        View row = convertView;

        final TodoItem currentItem = getItem(position);

        if (row == null) {
            LayoutInflater inflater = ((Activity) mContext).getLayoutInflater();
            row = inflater.inflate(R.layout.list_item, parent, false);
        }
        row.setTag(currentItem);

        final CheckBox checkBox = (CheckBox) row.findViewById(R.id.list_item_checkbox);
        checkBox.setText(currentItem.getText());
        checkBox.setChecked(false);
        checkBox.setEnabled(true);

        checkBox.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View arg0) {
                if (checkBox.isChecked()) {
                    checkBox.setEnabled(false);
                    ToDoActivity activity = (ToDoActivity) mContext;
                    activity.checkItem(currentItem);
                }
            }
        });
        return row;
    }
    ```

=== "Kotlin"

    ``` kotlin
    override fun getView(position: Int, convertView: View, parent: ViewGroup): View {
        val item = getItem(position)
        val inflater = (mContext as Activity).getLayoutInflater()
        val row = convertView ?? inflater.inflate(R.layout.list_item, parent, false)
        row.tag = item

        val chkbox = row.findViewById<CheckBox>(R.id.list_item_checkbox)
        chkbox.apply {
            text = item.text
            isChecked = false
            isEnabled = true
        }
        chkbox.setOnClickListener(object : View.OnClickListener {
            override fun onClick(view: View) {
                if (chkbox.isChecked) {
                    chkbox.isEnabled = false
                    (mContext as TodoActivity).checkItem(item)
                }
            }
        })

        return row
    }
    ```

We create an instance of this class in our Activity as follows:

=== "Java"

    ``` java
    TodoItemAdapter mAdapter;

    // In onCreate()
    mAdapter = new TodoItemAdapter(this, R.layout.list_item);
    ```

=== "Kotlin"

    ``` kotlin
    private lateinit var mAdapter: TodoItemAdapter
    
    // In onCreate()
    mAdapter = TodoItemAdapter(this, R.layout.list_item)
    ```
    
The second parameter to the TodoItemAdapter constructor is a reference to the layout. We can now instantiate the **ListView** and assign the adapter to the **ListView**.

=== "Java"

    ``` java
    ListView listViewToDo = (ListView) findViewById(R.id.listViewToDo);
    listViewToDo.setAdapter(mAdapter);
    ```

=== "Kotlin"

    ``` kotlin
    val listView = findViewById<ListView>(R.id.listViewToDo)
    listView.adapter = mAdapter
    ```
#### <a name="use-adapter"></a>Use the Adapter to Bind to the UI

You are now ready to use data binding. The following code shows how to get items in the table and fills the local adapter with the returned items.

=== "Java"

    ``` java
    public void showAll(View view) {
        AsyncTask<Void, Void, Void> task = new AsyncTask<Void, Void, Void>(){
            @Override
            protected Void doInBackground(Void... params) {
                try {
                    final List<TodoItem> results = mToDoTable.execute().get();
                    runOnUiThread(new Runnable() {

                        @Override
                        public void run() {
                            mAdapter.clear();
                            for (TodoItem item : results) {
                                mAdapter.add(item);
                            }
                        }
                    });
                } catch (Exception exception) {
                    createAndShowDialog(exception, "Error");
                }
                return null;
            }
        };
        runAsyncTask(task);
    }
    ```

=== "Kotlin"

    ``` kotlin
    fun showAll(view: View) {
        val task = object : AsyncTask<Void, Void, Void>() {
            override fun doInBackground(params: Void...) {
                try {
                    val results = mTable.execute().get()
                    runOnUiThread {
                        mAdapter.clear()
                        mAdapter.addAll(results)
                    }
                } catch (exception: Exception) {
                    createAndShowDialog(exception, "Error")
                }
            }
        }
    }
    ```

Call the adapter any time you modify the **TodoItem** table. Since modifications are done on a record by record basis, you handle a single row instead of a collection. When you insert an item, call the **add** method on the adapter; when deleting, call the **remove** method.

You can find a complete example in the [Android Quickstart Project][21].

## <a name="inserting"></a>Insert data into the backend

Instantiate an instance of the *TodoItem* class and set its properties, just as you normally would.  Then use **insert()** to insert an object.

=== "Java"

    ``` java
    TodoItem item = new TodoItem("Text Content", false);
    TodoItem entity = mTable.insert(item).get();
    ```

=== "Kotlin"

    ``` kotlin
    val item = TodoItem("Text Content", false)
    val entity = mTable.insert(item).get()
    ```

The returned entity matches the data inserted into the backend table, included the ID and any other values (such as the `createdAt`, `updatedAt`, and `version` fields) set on the backend.

Mobile Apps tables require a primary key column named **id**. This column must be a string. The default value of the ID column is a GUID.  You can provide other unique values, such as email addresses or usernames. When a string ID value is not provided for an inserted record, the backend generates a new GUID.

GUID ID values provide the following advantages:

* IDs can be generated without making a round trip to the database.
* Records are easier to merge from different tables or databases.
* ID values integrate better with an application's logic.

String ID values are **REQUIRED** for offline sync support.  You cannot change an id once it is stored in the backend database.

## <a name="updating"></a>Update data in a mobile app

To update data in a table, pass the new object to the **update()** method.

=== "Java"

    ``` java
    mTable.update(entity).get();
    ```

=== "Kotlin"

    ``` kotlin
    mTable.update(entity).get()
    ```

In this example, *entity* is a reference to a row in the *TodoItem* table (such as an entity returned from a previous insert operation or query), which has had some changes made to it  The row with the same **id** is updated.  `.update(entity)` returns a `ListenableFuture<Entity>`, and `.get()` converts the listenable future into a synchronous operation.

## <a name="deleting"></a>Delete data in a mobile app

The following code shows how to delete data from a table by specifying the data object.

=== "Java"

    ``` java
    mTable.delete(item).get();
    ```

=== "Kotlin"

    ``` kotlin
    mTable.delete(item).get()
    ```

You can also delete an item by specifying the **id** field of the row to delete.

=== "Java"

    ``` java
    String myRowId = "2FA404AB-E458-44CD-BC1B-3BC847EF0902";
    mTable.delete(myRowId);
    ```

=== "Kotlin"

    ``` kotlin
    val myRowId = "2FA404AB-E458-44CD-BC1B-3BC847EF0902"
    mTable.delete(myRowId)
    ```

## <a name="lookup"></a>Look up a specific item by Id

Look up an item with a specific **id** field with the **lookUp()** method:

=== "Java"

    ```java
    TodoItem result = mTable.lookUp(myRowId).get()
    ```

=== "Kotlin"

    ``` kotlin
    val result = mTable.lookUp(myRowId).get()
    ```
## <a name="untyped"></a>How to: Work with untyped data

The untyped programming model gives you exact control over JSON serialization.  There are some common scenarios where you may wish to use an untyped programming model. For example, if your backend table contains many columns and you only need to reference a subset of the columns.  The typed model requires you to define all the columns defined in the Mobile Apps backend in your data class.  Most of the API calls for accessing data are similar to the typed programming calls. The main difference is that in the untyped model you invoke methods on the **MobileServiceJsonTable** object, instead of the **MobileServiceTable** object.

### <a name="json_instance"></a>Create an instance of an untyped table

Similar to the typed model, you start by getting a table reference, but in this case it's a **MobileServicesJsonTable** object. Obtain the reference by calling the **getTable** method on an instance of the client:

=== "Java"

    ``` java
    private MobileServiceJsonTable mJsonTable;

    // ... later ...
    mJsonTable = mClient.getTable("TodoItem");
    ```

=== "Kotlin"

    ``` kotlin
    private lateinit var mJsonTable: MobileServiceJsonTable

    // ... later ...
    mJsonTable = mClient.getTable("TodoItem")
    ```

Once you have created an instance of the **MobileServiceJsonTable**, it has virtually the same API available as with the typed programming model. In some cases, the methods take an untyped parameter instead of a typed parameter.

### <a name="json_insert"></a>Insert into an untyped table

The following code shows how to do an insert. The first step is to create a [JsonObject][1], which is part of the [gson][3] library.

=== "Java"

    ``` java
    JsonObject jsonItem = new JsonObject();
    jsonItem.addProperty("text", "Wake up");
    jsonItem.addProperty("complete", false);
    ```

=== "Kotlin"

    ``` kotlin
    val jsonItem= JsonObject().apply {
        addProperty("text", "Wake up")
        addProperty("complete", false)
    }
    ```

Then, Use **insert()** to insert the untyped object into the table.

=== "Java"

    ``` java
    JsonObject entity = mJsonTable.insert(jsonItem).get();
    ```

=== "Kotlin"

    ``` kotlin
    val entity = mJsonTable.insert(jsonItem).get()
    ```

If you need to get the ID of the inserted object, use the **getAsJsonPrimitive()** method.

=== "Java"

    ``` java
    String id = entity.getAsJsonPrimitive("id").getAsString();
    ```

=== "Kotlin"

    ``` kotlin
    val id = entity.getAsJsonPrimitive("id").getAsString()
    ```

### <a name="json_delete"></a>Delete from an untyped table

The following code shows how to delete an instance, in this case, the same instance of a **JsonObject** that was created in the prior *insert* example. The code is the same as with the typed case, but the method has a different signature since it references an **JsonObject**.

=== "Java"

    ```java
    mJsonTable.delete(entity);
    ```

=== "Kotlin"

    ``` kotlin
    mJsonTable.delete(entity)
    ```

You can also delete an instance directly by using its ID:

=== "Java"

    ``` java
    mJsonTable.delete(id);
    ```

=== "Kotlin"

    ``` kotlin
    mJsonTable.delete(id)
    ```

### <a name="json_get"></a>Return all rows from an untyped table

The following code shows how to retrieve an entire table. Since you are using a JSON Table, you can selectively retrieve only some of the table's columns.

=== "Java"

    ``` java
    public AsyncTask<Void, Void, Void> showAllUntyped(View view) {
        return new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... params) {
                try {
                    final JsonElement result = mJsonTable.execute().get();
                    final JsonArray results = result.getAsJsonArray();
                    runOnUiThread(new Runnable() {
                        @Override public void run() {
                            mAdapter.clear();
                            for (JsonElement item : results) {
                                JsonObject entity = item.getAsJsonObject()
                                String ID = entity.getAsJsonPrimitive("id").getAsString();
                                String mText = entity.getAsJsonPrimitive("text").getAsString();
                                Boolean mComplete = entity.getAsJsonPrimitive("complete").getAsBoolean();
                                TodoItem TodoItem = new TodoItem();
                                TodoItem.setId(ID);
                                TodoItem.setText(mText);
                                TodoItem.setComplete(mComplete);
                                mAdapter.add(TodoItem);
                            }
                        }
                    });
                } catch (Exception exception) {
                    createAndShowDialog(exception, "Error");
                }
                return null;
            }
        };
    }
    ```

=== "Kotlin"

    ``` kotlin
    fun showAllUntyped(view: View): AsyncTask<Void, Void, Void> => 
        object : AsyncTask<Void, Void, Void> {
            override fun doInBackground(params: Void...): Void {
                try {
                    val results = mJsonTable.execute().get().asJsonArray()
                    runOnUiThread {
                        mAdapter.clear()
                        for (item in results) {
                            val entity = item.getAsJsonObject()
                            val TodoItem = TodoItem.apply {
                                id = item.getAsJsonPrimitive("id").getAsString()
                                text = item.getAsJsonPrimitive("text").getAsString()
                                complete = item.getAsJsonPrimitive("complete").getAsBoolean()
                            }
                            mAdapter.add(TodoItem)
                        }
                    }
                }
            }
        }
    ```

The same set of filtering, filtering and paging methods that are available for the typed model are available for the untyped model.

## <a name="offline-sync"></a>Implement Offline Sync

The Azure Mobile Apps Client SDK also implements offline synchronization of data by using a SQLite database to store a copy of the server data locally.  Operations performed on an offline table do not require mobile connectivity to work.  Offline sync aids in resilience and performance at the expense of more complex logic for conflict resolution.  The Azure Mobile Apps Client SDK implements the following features:

* Incremental Sync: Only updated and new records are downloaded, saving bandwidth and memory consumption.
* Optimistic Concurrency: Operations are assumed to succeed.  Conflict Resolution is deferred until updates are performed on the server.
* Conflict Resolution: The SDK detects when a conflicting change has been made at the server and provides hooks to alert the user.
* Soft Delete: Deleted records are marked deleted, allowing other devices to update their offline cache.

### Initialize Offline Sync

Each offline table must be defined in the offline cache before use.  Normally, table definition is done immediately after the creation of the client, but it must be done asynchronously.

=== "Java"

    ``` java
    private AsyncTask<Void, Void, Void>  initializeStore(MobileServiceClient mClient)
    {
        return new AsyncTask<Void, Void, Void>() {
            @Override
            protected void doInBackground(Void... params) {
                try {
                    MobileServiceSyncContext syncContext = mClient.getSyncContext();
                    if (syncContext.isInitialized()) {
                        return null;
                    }

                    // Create the offline database
                    SQLiteLocalStore localStore = new SQLiteLocalStore(
                        mClient.getContext(),       // Application Context
                        "offlinestore",             // database name
                        null,                       // cursor factory
                        1                           // version
                    );

                    // Define the offline database schema
                    Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
                    tableDefinition.put("id", ColumnDataType.String);
                    tableDefinition.put("complete", ColumnDataType.Boolean);
                    tableDefinition.put("text", ColumnDataType.String);
                    tableDefinition.put("version", ColumnDataType.String);
                    tableDefinition.put("updatedAt", ColumnDataType.DateTimeOffset);
                    localStore.defineTable("TodoItem", tableDefinition);

                    // Specify a sync handler, for conflict resolution
                    SimpleSyncHandler syncHandler = new SimpleSyncHandler();

                    // Initialize the client sync context
                    syncContext.initialize(localStore, syncHandler) // ListenableFuture<T>
                        .get();                                     // convert to sync
                } catch (Exception err) {
                    // Handle error
                }
                return null;
            }
        };
    }
    ```

=== "Kotlin"

    ``` kotlin
    private fun initializeStore(mClient: MobileServiceClient): AsyncTask<Void, Void, Void> =>
        object : AsyncTask<Void, Void, Void>() {
            override fun doInBackground(params: Void...) {
                if (mClient.syncContext.isInitialized) {
                    return null
                }
                
                // Create the database
                val localStore = SQLiteLocalStore(
                    mClient.context,            // Application context
                    "OfflineStore",             // Database filename
                    null,                       // Cursor factory
                    1)                          // Version 
                
                // Define the SQLite offline database schema
                val tableDefinition = hashMapOf<String, ColumnDataType>(
                    "id" to ColumnDataType.String,
                    "text" to ColumnDataType.String,
                    "complete" to ColumnDataType.Boolean
                )
                localStore.defineTable("TodoItem", tableDefinition)

                // Configure the sync context with a sync conflict handler
                mClient.syncContext.initialize(localStore, SimpleSyncHandler())
                    .get()          // sync version from a listenableFuture
            }
        }
    ```

### Obtain a reference to the Offline Cache Table

For an online table, you use `.getTable()`.  For an offline table, use `.getSyncTable()`:

=== "Java"

    ``` java
    MobileServiceSyncTable<TodoItem> mSyncTable = mClient.getSyncTable("TodoItem", TodoItem.class);
    ```

=== "Kotlin"

    ``` kotlin
    val mSyncTable = mClient.getSyncTable("TodoItem", TodoItem::class.java)
    ```

All the methods that are available for online tables (including filtering, sorting, paging, inserting data, updating data, and deleting data) work equally well on online and offline tables.

### Synchronize the Local Offline Cache

Synchronization is within the control of your app.  Here is an example synchronization method:

=== "Java"

    ``` java
    public void syncItems() {
        mClient.getSyncContext().push().get();
        mSyncTable.pull(null, "tableIncrementalSync").get();
    }
    ```

=== "Kotlin"

    ``` kotlin
    private func syncItems() {
        mClient.syncContext.push().get()
        mSyncTable.pull(null, "tableIncrementalSync").get()
    }
    ```

If a query name is provided to the `.pull(query, queryname)` method, then incremental sync is used to return only records that have been created or changed since the last successfully completed pull.  If no query name is provided, then all records are pulled.  

Although this method is shown as a syncrhonous method, you should execute synchronization asynchronously.  Android requires that network requests execute on an async thread.

### Handle Conflicts during Offline Synchronization

If a conflict happens during a `.push()` operation, a `MobileServiceConflictException` is thrown.   The server-issued item is embedded in the exception and can be retrieved by `.getItem()` on the exception.  Adjust the push by calling the following items on the MobileServiceSyncContext object:

*  `.cancelAndDiscardItem()`
*  `.cancelAndUpdateItem()`
*  `.updateOperationAndItem()`

Once all conflicts are marked as you wish, call `.push()` again to resolve all the conflicts.

## <a name="custom-api"></a>Call a custom API

A custom API enables you to define custom endpoints that expose server functionality that does not map to an insert, update, delete, or read operation. By using a custom API, you can have more control over messaging, including reading and setting HTTP message headers and defining a message body format other than JSON.

From an Android client, you call the **invokeApi** method to call the custom API endpoint. The following example shows how to call an API endpoint named **completeAll**, which returns a collection class named **MarkAllResult**.

=== "Java"

    ``` java
    public void completeItem(View view) {
        ListenableFuture<MarkAllResult> result = mClient.invokeApi("completeAll", MarkAllResult.class);
        Futures.addCallback(result, new FutureCallback<MarkAllResult>() {
            @Override
            public void onFailure(Throwable exc) {
                // Handle error
            }

            @Override
            public void onSuccess(MarkAllResult result) {
                // Handle result
            }
        });
    }
    ```

=== "Kotlin"

    ``` kotlin
    fun completeItem(view: View) {
        val task = mClient.invokeApi("completeAll", MarkAllResult::class.java);
        task.addListener({
            try {
                val result: MarkAllResult = task.get()
                // Handle result
            } catch (err: Exception) {
                // Handle error
            }
        }, executor)
    }
    ```


The **invokeApi** method is called on the client, which sends a POST request to the new custom API. The result returned by the custom API is displayed in a message dialog, as are any errors. Other versions of **invokeApi** let you optionally send an object in the request body, specify the HTTP method, and send query parameters with the request. Untyped versions of **invokeApi** are provided as well.

## <a name="authentication"></a>Add authentication to your app

Tutorials already describe in detail how to add these features.

App Service supports [authenticating app users](https://docs.microsoft.com/azure/app-service/app-service-authentication-how-to) using various external identity providers: Facebook, Google, Microsoft Account, Twitter, and Azure Active Directory. You can set permissions on tables to restrict access for specific operations to only authenticated users. You can also use the identity of authenticated users to implement authorization rules in your backend.

Two authentication flows are supported: a **server** flow and a **client** flow. The server flow provides the simplest authentication experience, as it relies on the identity providers web interface.  No additional SDKs are required to implement server flow authentication. Server flow authentication does not provide a deep integration into the mobile device and is only recommended for proof of concept scenarios.

The client flow allows for deeper integration with device-specific capabilities such as single sign-on as it relies on SDKs provided by the identity provider.  For example, you can integrate the Facebook SDK into your mobile application.  The mobile client swaps into the Facebook app and confirms your sign-on before swapping back to your mobile app.

Four steps are required to enable authentication in your app:

* Register your app for authentication with an identity provider.
* Configure your App Service backend.
* Restrict table permissions to authenticated users only on the App Service backend.
* Add authentication code to your app.

You can set permissions on tables to restrict access for specific operations to only authenticated users. You can also use the SID of an authenticated user to modify requests.  

### <a name="caching"></a>Authentication: Server Flow

To use the server flow login process, you must:

* Define a callback to handle the response from the server.
* Initiate the login process.

> **NOTE**: Microsoft Account is now done via Azure Active Directory.  Do not use the in-built Microsoft Account support for Azure App Service Authentication and Authorization.

The process is the same, irrespective of the provider used.  To define the callback, add the following to your entry activity (such as `MainActivity`):

=== "Java"

    ``` java
    public static final int LOGIN_REQUEST_CODE = 1;    // Pick a unique request code

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == LOGIN_REQUEST_CODE && resultCode == RESULT_OK) {
            MobileServiceActivityResult result = mClient.onActivityResult(data);
            if (result.isLoggedIn()) {
                // Handle a successful login
            } else {
                // Handle a failed login
            }
        }
    }
    ```

=== "Kotlin"

    ``` kotlin
    companion object {
        const val LOGIN_REQUEST_CODE = 1
    }

    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent) {
        if (requestCode == LOGIN_REQUEST_CODE && resultCode == RESULT_OK) {
            val result = mClient.onActivityResult(data)
            if (result.isLoggedIn) {
                // Handle a successful login
            } else {
                // Handle a failed login
            }
        }
    }
    ```

You should not perform a service operation (such as downloading the latest data or synchronizing data) until you have performed a successful login.

To initiate a login process, use the following:

=== "Java"

    ``` java
    MobileServiceUser user = mClient.login(
        "aad",                  // Auth provider 
        "zumoauth",             // URL scheme for the response
        LOGIN_REQUEST_CODE);    // The request code
    ```

=== "Kotlin"

    ``` kotlin
    val user = mClient.login(
        "aad",                  // Auth provider 
        "zumoauth",             // URL scheme for the response
        LOGIN_REQUEST_CODE)     // The request code
    ```

The auth provider can either be a string or a member of the `MobileServiceAuthenticationProvider` enum.  If using custom authentication, use a string.

You also need to configure the project for customtabs.  First specify a redirect-URL.  Add the following snippet to `AndroidManifest.xml`:

``` xml
<activity android:name="com.microsoft.windowsazure.mobileservices.authentication.RedirectUrlActivity">
    <intent-filter>
        <action android:name="android.intent.action.VIEW" />
        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />
        <data android:scheme="zumoauth" android:host="easyauth.callback"/>
    </intent-filter>
</activity>
```

Ensure that the `android:scheme` matches the URL scheme you use within the `.login()` call and within the Authentication configuration for Azure App Service.  Then, add the **redirectUriScheme** to the `build.gradle` file for your application:

```gradle
android {
    buildTypes {
        release {
            // … …
            manifestPlaceholders = ['redirectUriScheme': 'zumoauth://easyauth.callback']
        }
        debug {
            // … …
            manifestPlaceholders = ['redirectUriScheme': 'zumoauth://easyauth.callback']
        }
    }
}
```

Again, the `redirectUriScheme` must match the scheme in `AndroidManifest.xml` and that is configured within your Azure App Service Authentication configuration.

Finally, add `com.android.support:customtabs:28.0.0` to the dependencies list in the `build.gradle` file:

```gradle
dependencies {
    // Other dependencies
    implementation 'com.android.support:customtabs:28.0.0'
}
```

Obtain the ID of the logged-in user from a **MobileServiceUser** using the **getUserId** method. 

### <a name="caching"></a>Cache authentication tokens

Caching authentication tokens requires you to store the User ID and authentication token locally on the device. The next time the app starts, you check the cache, and if these values are present, you can skip the log in procedure and rehydrate the client with this data. However this data is sensitive, and it should be stored encrypted for safety in case the phone gets stolen.

When you try to use an expired token, you receive a *401 unauthorized* response. You can handle authentication errors using filters.  Filters intercept requests to the App Service backend. The filter code tests the response for a 401, triggers the sign-in process, and then resumes the request that generated the 401.

### <a name="refresh"></a>Use Refresh Tokens

The token returned by Azure App Service Authentication and Authorization has a defined life time of one hour.  After this period, you must reauthenticate the user.  If you are using a long-lived token that you have received via client-flow authentication, then you can reauthenticate with Azure App Service Authentication and Authorization using the same token.  Another Azure App Service token is generated with a new lifetime.

You can also register the provider to use Refresh Tokens.  A Refresh Token is not always available.  Additional configuration is required:

* For **Azure Active Directory**, configure a client secret for the Azure Active Directory App.  Specify the client secret in the Azure App Service when configuring Azure Active Directory Authentication.  When calling `.login()`, pass `response_type=code id_token` as a parameter:

    === "Java"

        ``` java
        HashMap<String, String> parameters = new HashMap<String, String>();
        parameters.put("response_type", "code id_token");
        MobileServiceUser user = mClient.login
            MobileServiceAuthenticationProvider.AzureActiveDirectory,
            "{url_scheme_of_your_app}",
            AAD_LOGIN_REQUEST_CODE,
            parameters);
        ```

    === "Kotlin"

        ``` kotlin
        val parameters = hashMapOf<String, String>("response_type", "code id_token")
        val user = mClient.login("aad", "zumoauth", LOGIN_REQUEST_CODE, parameters)
        ```

* For **Google**, pass the `access_type=offline` as a parameter:

    === "Java"

        ``` java
        HashMap<String, String> parameters = new HashMap<String, String>();
        parameters.put("access_type", "offline");
        MobileServiceUser user = mClient.login
            MobileServiceAuthenticationProvider.Google,
            "{url_scheme_of_your_app}",
            GOOGLE_LOGIN_REQUEST_CODE,
            parameters);
        ```

    === "Kotlin"

        ``` kotlin
        val parameters = hashMapOf<String, String>("access_type", "offline")
        val user = mClient.login("aad", "zumoauth", LOGIN_REQUEST_CODE, parameters)
        ```

As a best practice, create a filter that detects a 401 response from the server and tries to refresh the user token.

## Log in with Client-flow Authentication

The general process for logging in with client-flow authentication is as follows:

* Configure Azure App Service Authentication and Authorization as you would server-flow authentication.
* Integrate the authentication provider SDK for authentication to produce an access token.
* Call the `.login()` method as follows (`result` should be an `AuthenticationResult`):

=== "Java"

    ``` java
    JSONObject payload = new JSONObject();
    payload.put("access_token", result.getAccessToken());
    ListenableFuture<MobileServiceUser> mLogin = mClient.login("{provider}", payload.toString());
    Futures.addCallback(mLogin, new FutureCallback<MobileServiceUser>() {
        @Override
        public void onFailure(Throwable exc) {
            exc.printStackTrace();
        }
        @Override
        public void onSuccess(MobileServiceUser user) {
            Log.d(TAG, "Login Complete");
        }
    });
    ```

=== "Kotlin"

    ``` kotlin
    val payload = JSONObject().apply { put("access_token", result.accessToken) }
    val loginTask = mClient.login("aad", payload.toString())
    loginTask.addListener({
        try {
            val user = loginTask.get()
            // Handle success
        } catch (ex: Exception) {
            // Handle error
        }
    }, executor)
    ```


### Customize Request Headers

Use the following `ServiceFilter` and attach the filter in the same way as the `ProgressFilter`:

```java
private class CustomHeaderFilter implements ServiceFilter {
    @Override
    public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback next) {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                request.addHeader("X-APIM-Router", "mobileBackend");
            }
        });
        SettableFuture<ServiceFilterResponse> result = SettableFuture.create();
        try {
            ServiceFilterResponse response = next.onNext(request).get();
            result.set(response);
        } catch (Exception exc) {
            result.setException(exc);
        }
    }
}
```

### <a name="conversions"></a>Configure Automatic Serialization

You can specify a conversion strategy that applies to every column by using the [gson][3] API. The Android client library uses [gson][3] behind the scenes to serialize Java objects to JSON data before the data is sent to Azure App Service.  The following code uses the **setFieldNamingStrategy()** method to set the strategy. This example will delete the initial character (an "m"), and then lower-case the next character, for every field name. For example, it would turn "mId" into "id."  Implement a conversion strategy to reduce the need for `SerializedName()` annotations on most fields.

=== "Java"

    ```java
    FieldNamingStrategy namingStrategy = new FieldNamingStrategy() {
        public String translateName(File field) {
            String name = field.getName();
            return Character.toLowerCase(name.charAt(1)) + name.substring(2);
        }
    }

    mClient.setGsonBuilder(
        MobileServiceClient
            .createMobileServiceGsonBuilder()
            .setFieldNamingStrategy(namingStrategy)
    );
    ```

=== "Kotlin"

    ``` kotlin
    val namingStrategy = object : FieldNamingStrategy() {
        fun translateName(field: File): String =>
            Character.toLowerCase(field.name.charAt(1)) + name.substring(2)
    }
    mClient.setGsonBuilder(
        MobileServiceClient.createMobileServiceGsonBuilder()
            .setFieldNamingStrategy(namingStrategy)
    )
    ```

This code must be executed before creating a mobile client reference using the **MobileServiceClient**.

<!-- URLs. -->
[1]: https://static.javadoc.io/com.google.code.gson/gson/2.8.5/com/google/gson/JsonObject.html
[3]: https://www.javadoc.io/doc/com.google.code.gson/gson/2.8.5
[8]: https://azure.github.io/azure-mobile-apps-android-client/com/microsoft/windowsazure/mobileservices/table/MobileServiceTable.html
[9]: https://azure.github.io/azure-mobile-apps-android-client/com/microsoft/windowsazure/mobileservices/MobileServiceClient.html
[12]: https://azure.github.io/azure-mobile-apps-android-client/
[14]: https://go.microsoft.com/fwlink/p/?LinkID=717034
[17]: https://developer.android.com/reference/java/util/UUID.html
[18]: https://github.com/google/guava/wiki/ListenableFutureExplained
[19]: https://www.odata.org/documentation/odata-version-3-0/
[21]: https://github.com/Azure-Samples/azure-mobile-apps-android-quickstart
[Future]: https://developer.android.com/reference/java/util/concurrent/Future.html
[AsyncTask]: https://developer.android.com/reference/android/os/AsyncTask.html
[OkHttp]: https://square.github.io/okhttp/

package com.azure.mobile.zumoquickstart

import android.content.Context
import com.google.common.util.concurrent.MoreExecutors
import com.microsoft.windowsazure.mobileservices.MobileServiceClient
import com.microsoft.windowsazure.mobileservices.MobileServiceList
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncTable
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.SQLiteLocalStore
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.SimpleSyncHandler
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import java.util.concurrent.Executors
import java.util.concurrent.TimeUnit

/**
 * Implements an asynchronous service call to the Azure Mobile Apps backend.
 */
class TodoService private constructor() {
    private object Singleton {
        val INSTANCE = TodoService()
    }

    companion object {
        val instance: TodoService by lazy { Singleton.INSTANCE }
    }

    private lateinit var mClient: MobileServiceClient
    private val executor = MoreExecutors.listeningDecorator(Executors.newFixedThreadPool(4))
    private var initialized = false

    private lateinit var mTable: MobileServiceTable<TodoItem>
    //private lateinit var mTable: MobileServiceSyncTable<TodoItem>

    /**
     * Call initialize() before using mClient or mTable - it needs a reference to an
     * application context.
     */
    fun initialize(context: Context, callback: (Throwable?) -> Unit) {
        if (initialized) {
            callback.invoke(null)
            return
        }

        mClient = MobileServiceClient(Constants.BackendUrl, context)
        mClient.setAndroidHttpClientFactory {
            OkHttpClient.Builder()
                .readTimeout(30, TimeUnit.SECONDS)
                .connectTimeout(30, TimeUnit.SECONDS)
                .addInterceptor(HttpLoggingInterceptor().apply { level = HttpLoggingInterceptor.Level.BODY })
                .build()
        }

        // Get a reference to the table to use.
        mTable = mClient.getTable(TodoItem::class.java)
        //mTable = mClient.getSyncTable(TodoItem::class.java)

        // We are finished if:
        //  a) We are not using offline sync
        //  b) We are using offline sync, but the database is already created
        if (mTable !is MobileServiceSyncTable<*> || mClient.syncContext.isInitialized) {
            callback.invoke(null)
            initialized = true
            return
        }

        // Define the SQLite offline database schema
        val localStore = SQLiteLocalStore(mClient.context, "OfflineStore", null, 1)
        val tableDefinition = hashMapOf<String, ColumnDataType>(
            "id" to ColumnDataType.String,
            "text" to ColumnDataType.String,
            "complete" to ColumnDataType.Boolean
        )
        localStore.defineTable("TodoItem", tableDefinition)

        // Create the SQLite offline database using the provided definition
        val future = mClient.syncContext.initialize(localStore, SimpleSyncHandler())
        future.addListener({
           try {
               future.get()
               initialized = true
               callback.invoke(null)
           } catch (e: Exception) {
               callback.invoke(e)
           }
        }, executor)
    }

    /**
     * Synchronize items with the server.  First, any changes are pushed to the server,
     * then new or changed items are pulled from the server
     */
    fun syncItems(callback: (Throwable?) -> Unit) {
        // This will be replaced when updating for offline sync
        callback.invoke(null)
    }

    /**
     * Get all the items in the table.
     *
     * @param callback called when the operation completes.
     */
    fun getTodoItems(callback: (MobileServiceList<TodoItem>?, Throwable?) -> Unit) {
        val query = QueryOperations.tableName("TodoItem").orderBy("createdAt", QueryOrder.Ascending)
        val future = mTable.where(query).execute()
        //val future = mTable.read(query)

        /*
         * The return type from .execute() is a ListenableFuture<MobileServiceList<TodoItem>>, which
         * completes asynchronously.  We have to add a listener to handle the completion.
         */
        future.addListener({
            try {
                val items = future.get()
                callback.invoke(items, null)
            } catch (e: Exception) {
                callback.invoke(null, e)
            }
        }, executor)
    }

    /**
     * Creates a new item in the table.
     *
     * @param item the item to insert into the table.
     * @param callback called when the operation completes.
     */
    fun createTodoItem(item: TodoItem, callback: (TodoItem?, Throwable?) -> Unit) {
        val future = mTable.insert(item)

        /*
         * The return type from .insert(item) is a ListenableFuture<TodoItem>, which
         * completes asynchronously.  We have to add a listener to handle the completion.
         */
        future.addListener({
           try {
               val newItem = future.get()
               callback.invoke(newItem, null)
           } catch (e: Exception) {
               callback.invoke(null, e)
           }
        }, executor)
    }

    /**
     * Updates an existing item in the table.
     *
     * @param item the item to update in the table.
     * @param callback called when the operation completes.
     */
    fun updateTodoItem(item: TodoItem, callback: (TodoItem?, Throwable?) -> Unit) {
        val future = mTable.update(item)

        /*
         * The return type from .update(item) is a ListenableFuture<TodoItem>, which
         * completes asynchronously.  We have to add a listener to handle the completion.
         */
        future.addListener({
            try {
                future.get()
                callback.invoke(item, null)
            } catch (e: Exception) {
                callback.invoke(null, e)
            }
        }, executor)
    }
}
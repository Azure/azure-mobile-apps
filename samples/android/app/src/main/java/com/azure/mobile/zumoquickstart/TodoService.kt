package com.azure.mobile.zumoquickstart

import android.content.Context
import com.google.common.util.concurrent.MoreExecutors
import com.microsoft.windowsazure.mobileservices.MobileServiceClient
import com.microsoft.windowsazure.mobileservices.MobileServiceList
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable
import java.util.concurrent.Executors

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
    private lateinit var mTable: MobileServiceTable<TodoItem>
    private val executor = MoreExecutors.listeningDecorator(Executors.newFixedThreadPool(4))
    private var initialized = false

    /**
     * Call initialize() before using mClient or mTable - it needs a reference to an
     * application context.
     */
    fun initialize(context: Context) {
        if (!initialized) {
            mClient = MobileServiceClient(Constants.BackendUrl, context)
            mTable = mClient.getTable(TodoItem::class.java)
            initialized = true
        }
    }

    /**
     * Get all the items in the table.
     *
     * @param callback called when the operation completes.
     */
    fun getTodoItems(callback: (MobileServiceList<TodoItem>?, Throwable?) -> Unit) {
        val future = mTable.where().execute()

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
                val newItem = future.get()
                callback.invoke(newItem, null)
            } catch (e: Exception) {
                callback.invoke(null, e)
            }
        }, executor)
    }
}
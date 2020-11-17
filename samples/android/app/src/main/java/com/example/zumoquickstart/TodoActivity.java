package com.example.zumoquickstart;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.AlertDialog;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.ProgressBar;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.MoreExecutors;
import com.google.common.util.concurrent.SettableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncContext;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncTable;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStoreException;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.SQLiteLocalStore;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.SimpleSyncHandler;

import org.checkerframework.checker.nullness.compatqual.NullableDecl;

import java.time.Duration;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ExecutionException;

import okhttp3.OkHttpClient;

import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.val;

public class TodoActivity extends Activity {
    /**
     * Client reference
     */
    private MobileServiceClient mClient;

    /**
     * Table used to access data from the mobile app backend
     */
    private MobileServiceTable<TodoItem> mTable;
    // OFFLINE SYNC
    // private MobileServiceSyncTable<TodoItem> mTable;

    /**
     * Adapter to sync the items list with the view
     */
    private TodoItemAdapter mAdapter;

    /**
     * EditText containing the "New Todo Item" text
     */
    private EditText mTextNewTodo;

    /**
     * Progress spinner used for table operations
     */
    private ProgressBar mProgressBar;

    /**
     * Initialize the activity
     */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_to_do);

        // Initialize the UI
        mProgressBar = findViewById(R.id.loadingProgressBar);
        mProgressBar.setVisibility(ProgressBar.GONE);
        mTextNewTodo = findViewById(R.id.textNewToDo);
        mAdapter = new TodoItemAdapter(this, R.layout.row_list_to_do);
        ListView mListView = findViewById(R.id.listViewToDo);
        mListView.setAdapter(mAdapter);

        // Connect to the Azure Mobile Apps Backend
        try {
            // Create the client instance, using the Configuring
            mClient = new MobileServiceClient(Configuration.BackendUrl, this).withFilter(new ProgressFilter());

            // Use a custom HTTP Client with a 30 second timeout for connections
            // This is required on "free" App Services to prevent timeouts as the service spins up
            Duration timeout = Duration.ofSeconds(30);
            mClient.setAndroidHttpClientFactory(() -> new OkHttpClient.Builder().connectTimeout(timeout).readTimeout(timeout).build());

            // AUTHENTICATE USERS
            // authenticate();
            createTable();

        } catch (Exception e) {
            createAndShowDialog(e, e.getClass().getName());
        }
    }

    /**
     * Initializes the activity menu
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.activity_main, menu);
        return true;
    }

    /**
     * Select an option from the menu
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        if (item.getItemId() == R.id.menu_refresh) {
            refreshItemsFromTable();
        }
        return true;
    }

    private void createTable() {
        // Get a reference to the Remote table.
        mTable = mClient.getTable(TodoItem.class);
        // OFFLINE SYNC:
        // mTable = mClient.getSyncTable(TodoItem.class);

        // Initialize the local store.
        initLocalStore().get();

        // Load items from the backend
        refreshItemsFromTable();
    }

    /**
     * Mark an item as completed.
     */
    public void checkItem(final TodoItem item) {
        if (mClient == null)
            return;
        item.setComplete(true);
        StoreItemsTask task = new StoreItemsTask();
        task.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR, item);
    }

    private class StoreItemsTask extends AsyncTask<TodoItem, Void, Exception> {
        @Override
        protected Exception doInBackground(TodoItem... items) {
            try {
                for (final TodoItem item : items) {
                    mTable.update(item).get();
                    runOnUiThread(() -> {
                        if (item.isComplete()) mAdapter.remove(item);
                    });
                }
                return null;
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        protected void onPostExecute(Exception e) {
            if (e != null) {
                createAndShowDialog(e, e.getClass().getName());
            }
        }
    }

    /**
     * Add a new item
     */
    public void addItem(View view) {
        if (mClient == null) {
            return;
        }

        final TodoItem item = new TodoItem();
        item.setText(mTextNewTodo.getText().toString());
        item.setComplete(false);
        AddItemsTask task = new AddItemsTask();
        task.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR, item);
        mTextNewTodo.setText("");
    }

    private class AddItemsTask extends AsyncTask<TodoItem, Void, Exception> {
        @Override
        protected Exception doInBackground(TodoItem... items) {
            try {
                for (final TodoItem item : items) {
                    TodoItem entity = mTable.insert(item).get();
                    runOnUiThread(() -> {
                        if (!entity.isComplete()) mAdapter.add(item);
                    });
                }
                return null;
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        protected void onPostExecute(Exception e) {
            if (e != null) {
                createAndShowDialog(e, e.getClass().getName());
            }
        }
    }

    /**
     * Refresh the list with the items in the table.
     */
    private void refreshItemsFromTable() {
        new RefreshItemsAsync().executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);
    }

    private class RefreshItemsAsync extends AsyncTask<Void, Void, Object> {
        @Override
        protected Object doInBackground(Void... items) {
            try {
                Query query = QueryOperations.field("complete").eq(val(false));

                List<TodoItem> results = mTable.where(query).execute().get();
                // OFFLINE SYNC:
                // sync().get();
                // List<TodoItem> results = mTable.read(query).get();

                return results;
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        protected void onPostExecute(Object result) {
            if (result instanceof Exception) {
                createAndShowDialog((Exception) result, "Error");
            } else {
                mAdapter.clear();
                mAdapter.addAll((List<TodoItem>) result);
            }
        }
    }

    /**
     * Sync the current context to the backend
     */
    private AsyncTask<Void, Void, Exception> sync() {
        return new SyncItemsTask().executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);
    }

    private class SyncItemsTask extends AsyncTask<Void, Void, Exception> {
        @Override
        protected Exception doInBackground(Void... items) {
            try {
                MobileServiceSyncContext syncContext = mClient.getSyncContext();
                if (syncContext.isInitialized()) {
                    syncContext.push().get();
                }

                // OFFLINE SYNC:
                //mTable.pull(null).get();

                return null;
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        protected void onPostExecute(Exception e) {
            if (e != null) {
                createAndShowDialog(e, e.getClass().getName());
            }
        }
    }

    /**
     * Initialize the local store, if required
     */
    private AsyncTask<Void, Void, Exception> initLocalStore() {
        return new InitStoreTask().executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);
    }

    /**
     * Async Task for initializing the store.
     */
    private class InitStoreTask extends AsyncTask<Void, Void, Exception> {
        @Override
        protected Exception doInBackground(Void... items) {
            try {
                MobileServiceSyncContext syncContext = mClient.getSyncContext();
                if (syncContext.isInitialized())
                    return null;
                SQLiteLocalStore localStore = new SQLiteLocalStore(mClient.getContext(), "OfflineStore", null, 1);
                Map<String, ColumnDataType> tableDefinition = new HashMap<>();
                tableDefinition.put("id", ColumnDataType.String);
                tableDefinition.put("text", ColumnDataType.String);
                tableDefinition.put("complete", ColumnDataType.Boolean);
                localStore.defineTable("TodoItem", tableDefinition);
                SimpleSyncHandler syncHandler = new SimpleSyncHandler();
                syncContext.initialize(localStore, syncHandler).get();
                return null;
            } catch (Exception e) {
                return e;
            }
        }

        @Override
        protected void onPostExecute(Exception result) {
            if (result != null) {
                createAndShowDialog(result, result.getClass().getName());
            }
        }
    }

    /**
     * Creates a dialog and shows it
     */
    private void createAndShowDialog(Exception e, String title) {
        Throwable ex = e;
        if (e.getCause() != null) {
            ex = e.getCause();
        }
        createAndShowDialog(ex.getMessage(), title);
    }

    /**
     * Creates a dialog and shows it
     */
    private void createAndShowDialog(String message, String title) {
        AlertDialog dialog = new AlertDialog.Builder(this)
                .setMessage(message)
                .setTitle(title)
                .create();
        dialog.show();
    }

    /**
     * MobileServiceClient service filter that shows or hides the progress bar based on
     * if a request is being handled or not.
     */
    private class ProgressFilter implements ServiceFilter {
        @Override
        public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback next) {
            SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    if (mProgressBar != null) mProgressBar.setVisibility(ProgressBar.VISIBLE);
                }
            });

            ListenableFuture<ServiceFilterResponse> future = next.onNext(request);
            FutureCallback<ServiceFilterResponse> callback = new FutureCallback<ServiceFilterResponse>() {
                @Override
                public void onSuccess(@NullableDecl ServiceFilterResponse result) {
                    runOnUiThread(() -> {
                        if (mProgressBar != null) mProgressBar.setVisibility(ProgressBar.GONE);
                    });
                }

                @Override
                public void onFailure(Throwable t) {
                    resultFuture.setException(t);
                }
            };

            Futures.addCallback(future, callback, MoreExecutors.directExecutor());
            return resultFuture;
        }
    }
}

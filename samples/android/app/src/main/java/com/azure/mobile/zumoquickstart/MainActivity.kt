package com.azure.mobile.zumoquickstart

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import android.view.Menu
import android.view.MenuItem
import android.widget.Toast
import androidx.recyclerview.widget.RecyclerView
import com.google.android.material.floatingactionbutton.FloatingActionButton
import com.microsoft.windowsazure.mobileservices.MobileServiceClient
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable

class MainActivity : AppCompatActivity() {
    private lateinit var itemList: RecyclerView
    private lateinit var addItemButton: FloatingActionButton
    private lateinit var adapter: TodoItemAdapter

    // Azure Mobile Apps
    private lateinit var mClient: MobileServiceClient
    private lateinit var mTable: MobileServiceTable<TodoItem>

    /**
     * Lifecycle event handler called when the activity is starting.  This is where most
     * initialization happens, including the UI.
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
        setSupportActionBar(findViewById(R.id.toolbar))

        // Identify the components
        itemList = findViewById(R.id.item_list)
        addItemButton = findViewById(R.id.add_item_button)

        // Initialize the RecyclerView for the List of Items
        adapter = TodoItemAdapter { item, isChecked -> updateItemFromList(item, isChecked) }
        itemList.adapter = adapter

        // Wire up an event handler to handle the click event on the Add Item button
        addItemButton.setOnClickListener { onAddItemClicked() }
    }

    /**
     * Lifecycle event handler called when the activity becomes active and ready to receive
     * inputs.  It is on top of the activity stack and visible to the user.
     */
    override fun onResume() {
        super.onResume()

        // Connect to the Azure Mobile Apps backend
        mClient = MobileServiceClient(Constants.BackendUrl, this)

        // Get a reference to the online table
        mTable = mClient.getTable(TodoItem::class.java)

        // Automatically refresh the items when the view starts
        onRefreshItemsClicked()
    }

    /**
     * Lifecycle event handler called when the top menu is created.
     */
    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        menuInflater.inflate(R.menu.menu_main, menu)
        return true
    }

    /**
     * Lifecycle event handler called when a menu item is selected.
     */
    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        return when (item.itemId) {
            R.id.action_refresh -> {
                onRefreshItemsClicked()
                true
            }
            else -> super.onOptionsItemSelected(item)
        }
    }

    /**
     * Event handler: called when the items list needs to be refreshed.
     */
    private fun onRefreshItemsClicked() {
        Toast.makeText(this, "Refresh Items", Toast.LENGTH_LONG).show()
    }

    /**
     * Event handler: called when the Add Item floating action button is clicked.
     */
    private fun onAddItemClicked() {
        Toast.makeText(this, "Add Item", Toast.LENGTH_LONG).show()
    }

    /**
     * Event handler: called when the user presses an item to complete it.
     */
    private fun updateItemFromList(item: TodoItem, isChecked: Boolean) {
        Toast.makeText(this, "Update Item", Toast.LENGTH_LONG).show()
    }
}
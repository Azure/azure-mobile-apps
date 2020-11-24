package com.azure.mobile.zumoquickstart

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import android.view.Menu
import android.view.MenuItem
import android.view.View
import android.widget.EditText
import android.widget.ProgressBar
import androidx.appcompat.app.AlertDialog
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.google.android.material.floatingactionbutton.FloatingActionButton

class MainActivity : AppCompatActivity() {
    private lateinit var itemList: RecyclerView
    private lateinit var addItemButton: FloatingActionButton
    private lateinit var isBusyIndicator: ProgressBar
    private lateinit var adapter: TodoItemAdapter

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
        isBusyIndicator = findViewById(R.id.busy_indicator)

        // Initialize the ZumoService
        TodoService.instance.initialize(this)

        // Initialize the RecyclerView for the List of Items
        adapter = TodoItemAdapter { item, isChecked -> updateItemFromList(item, isChecked) }
        itemList.adapter = adapter
        itemList.layoutManager = LinearLayoutManager(this, LinearLayoutManager.VERTICAL, false)

        // Wire up an event handler to handle the click event on the Add Item button
        addItemButton.setOnClickListener { onAddItemClicked() }
    }

    /**
     * Lifecycle event handler called when the activity becomes active and ready to receive
     * inputs.  It is on top of the activity stack and visible to the user.
     */
    override fun onResume() {
        super.onResume()

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
                if (isBusyIndicator.visibility == View.GONE)
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
        setIsBusy(true)
        TodoService.instance.getTodoItems { items, error ->
            if (error != null)
                showError(error)
            else runOnUiThread {
                val serverItems = items!!.toMutableList()
                adapter.refreshItems(serverItems)
            }
            setIsBusy(false)
        }
    }

    /**
     * Event handler: called when the Add Item floating action button is clicked.
     */
    private fun onAddItemClicked() {
        val builder = AlertDialog.Builder(this)
        val dialogLayout: View = layoutInflater.inflate(R.layout.dialog_new_item, null)
        val newItemControl = dialogLayout.findViewById<EditText>(R.id.new_item_text)

        with (builder) {
            setTitle(R.string.new_item_title)
            setView(dialogLayout)
            setPositiveButton(android.R.string.ok) { _, _ ->
                createItemFromDialog(newItemControl.text.toString())
            }
            show()
        }
    }

    /**
     * Event handler: called when the user adds an item from a dialog
     */
    private fun createItemFromDialog(text: String) {
        val item = TodoItem(id=null, text=text, complete=false)
        TodoService.instance.createTodoItem(item) { newItem, error ->
            if (error != null)
                showError(error)
            else runOnUiThread {
                adapter.addItem(newItem!!)
            }
        }
    }
    /**
     * Event handler: called when the user presses an item to complete it.
     */
    private fun updateItemFromList(item: TodoItem, isChecked: Boolean) {
        item.complete = isChecked
        TodoService.instance.updateTodoItem(item) { newItem, error ->
            if (error != null)
                showError(error)
            else runOnUiThread {
                adapter.replaceItem(newItem!!)
            }
        }
    }

    /**
     * Shows an error as a dialog box
     * @param error the exception that was thrown
     */
    private fun showError(error: Throwable) = runOnUiThread {
        val builder = AlertDialog.Builder(this)
        with (builder) {
            setTitle(R.string.error_title)
            setMessage(error.localizedMessage)
            setPositiveButton(android.R.string.ok) { dialog, _ -> dialog.dismiss() }
            show()
        }
    }

    private fun setIsBusy(isBusy: Boolean) = runOnUiThread {
        if (isBusy) {
            isBusyIndicator.isEnabled = true
            isBusyIndicator.visibility = View.VISIBLE
            addItemButton.isEnabled = false
        } else {
            isBusyIndicator.isEnabled = false
            isBusyIndicator.visibility = View.GONE
            addItemButton.isEnabled = true
        }

    }
}
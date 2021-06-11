/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
(function() {
    "use strict";

    var client, // Connection to the Azure Mobile App backend
        store,  // Sqlite store to use for offline data sync
        syncContext, // Offline data sync context
        tableName = 'todoitem',
        todoItemTable; // Reference to a table endpoint on backend

    // Set useOfflineSync to true to use tables from local store.
    // Set useOfflineSync to false to use tables on the server.
    var useOfflineSync = false;

    // Add an event listener to call our initialization routine when the host is ready
    document.addEventListener('deviceready', onDeviceReady, false);

    /**
     * Event Handler, called when the host is ready
     *
     * @event
     */
    function onDeviceReady() {
        // Create a connection reference to our Azure Mobile Apps backend 
        client = new WindowsAzure.MobileServiceClient('ZUMOAPPURL');

        if (useOfflineSync) {
            initializeStore().then(setup);
        } else {
            setup();
        }
    }

    /**
     * Set up and initialize the local store.
     */
    function initializeStore() {
        // Create the sqlite store
        store = new WindowsAzure.MobileServiceSqliteStore();

        // Define the table schema
        return store.defineTable({
            name: tableName,
            columnDefinitions: {
                id: 'string',
                deleted: 'boolean',
                text: 'string',
                complete: 'boolean',
                version: 'string'
            }
        }).then(function() {
            // Initialize the sync context
            syncContext = client.getSyncContext();

            // Define an overly simplified push handler that discards
            // local changes whenever there is an error or conflict.
            // Note that a real world push handler will have to take action according
            // to the nature of conflict.
            syncContext.pushHandler = {
                onConflict: function (pushError) {
                    return pushError.cancelAndDiscard();
                },
                onError: function (pushError) {
                    return pushError.cancelAndDiscard();
                }
            };

            return syncContext.initialize(store);
        });
    }
    
    /**
     * Set up the tables, event handlers and load data from the server 
     */
    function setup() {

        // Create a table reference
        if (useOfflineSync) {
            todoItemTable = client.getSyncTable(tableName);
        } else {
            todoItemTable = client.getTable(tableName);
        }

        // Refresh the todoItems
        refreshDisplay();

        // Wire up the UI Event Handler for the Add Item
        $('#add-item').submit(addItemHandler);
        $('#refresh').on('click', refreshDisplay);
    }

    /**
     * Refresh the display with items from the table.
     * If offline sync is enabled, the local table will be synchronized
     * with the server table before displaying the todo items.
     */
    function refreshDisplay() {
        updateSummaryMessage('Loading Data from Azure');

        if (useOfflineSync) {
            syncLocalTable().then(displayItems);
        } else {
            displayItems();
        }    
    }

    /**
     * Synchronize local table with the table on the server.
     * We do this by pushing local changes to the server and then
     * pulling the latest changes from the server.
     */
    function syncLocalTable() {
        return syncContext
                    .push()
                    .then(function() {
                        return syncContext.pull(new WindowsAzure.Query(tableName));
                    });
    }
    
    /**
     * Displays the todo items
     */
    function displayItems() {
        // Execute a query for uncompleted items and process
        todoItemTable
            .where({ complete: false })     // Set up the query
            .read()                         // Read the results
            .then(createTodoItemList, handleError);
    }
    
    /**
     * Updates the Summary Message
     * @param {string} msg the message to use
     * @returns {void}
     */
    function updateSummaryMessage(msg) {
        $('#summary').html(msg);
    }

    /**
     * Create the DOM for a single todo item
     * @param {Object} item the Todo Item
     * @param {string} item.id the ID of the item
     * @param {bool} item.complete true if the item is completed
     * @param {string} item.text the text value
     * @returns {jQuery} jQuery DOM object
     */
    function createTodoItem(item) {
        return $('<li>')
            .attr('data-todoitem-id', item.id)
            .append($('<button class="item-delete">Delete</button>'))
            .append($('<input type="checkbox" class="item-complete">').prop('checked', item.complete))
            .append($('<div>').append($('<input class="item-text">').val(item.text)));
    }

    /**
     * Create a list of Todo Items
     * @param {TodoItem[]} items an array of todoitem objects
     * @returns {void}
     */
    function createTodoItemList(items) {
        // Cycle through each item received from Azure and add items to the item list
        var listItems = $.map(items, createTodoItem);
        $('#todo-items').empty().append(listItems).toggle(listItems.length > 0);
        $('#summary').html('<strong>' + items.length + '</strong> item(s)');

        // Wire up the event handlers for each item in the list
        $('.item-delete').on('click', deleteItemHandler);
        $('.item-text').on('change', updateItemTextHandler);
        $('.item-complete').on('change', updateItemCompleteHandler);
    }

    /**
     * Handle error conditions
     * @param {Error} error the error that needs handling
     * @returns {void}
     */
    function handleError(error) {
        var text = error + (error.request ? ' - ' + error.request.status : '');
        console.error(text);
        $('#errorlog').append($('<li>').text(text));
    }

    /**
     * Given a sub-element of an LI, find the TodoItem ID associated with the list member
     *
     * @param {DOMElement} el the form element
     * @returns {string} the ID of the TodoItem
     */
    function getTodoItemId(el) {
        return $(el).closest('li').attr('data-todoitem-id');
    }

    /**
     * Event handler for when the user enters some text and clicks on Add
     * @param {Event} event the event that caused the request
     * @returns {void}
     */
    function addItemHandler(event) {
        var textbox = $('#new-item-text'),
            itemText = textbox.val();

        updateSummaryMessage('Adding New Item');
        if (itemText !== '') {
            todoItemTable.insert({
                text: itemText,
                complete: false
            }).then(displayItems, handleError);
        }

        textbox.val('').focus();
        event.preventDefault();
    }

    /**
     * Event handler for when the user clicks on Delete next to a todo item
     * @param {Event} event the event that caused the request
     * @returns {void}
     */
    function deleteItemHandler(event) {
        var itemId = getTodoItemId(event.currentTarget);

        updateSummaryMessage('Deleting Item in Azure');
        todoItemTable
            .del({ id: itemId })   // Async send the deletion to backend
            .then(displayItems, handleError); // Update the UI
        event.preventDefault();
    }

    /**
     * Event handler for when the user updates the text of a todo item
     * @param {Event} event the event that caused the request
     * @returns {void}
     */
    function updateItemTextHandler(event) {
        var itemId = getTodoItemId(event.currentTarget),
            newText = $(event.currentTarget).val();

        updateSummaryMessage('Updating Item in Azure');
        todoItemTable
            .update({ id: itemId, text: newText })  // Async send the update to backend
            .then(displayItems, handleError); // Update the UI
        event.preventDefault();
    }

    /**
     * Event handler for when the user updates the completed checkbox of a todo item
     * @param {Event} event the event that caused the request
     * @returns {void}
     */
    function updateItemCompleteHandler(event) {
        var itemId = getTodoItemId(event.currentTarget),
            isComplete = $(event.currentTarget).prop('checked');

        updateSummaryMessage('Updating Item in Azure');
        todoItemTable
            .update({ id: itemId, complete: isComplete })  // Async send the update to backend
            .then(displayItems, handleError);        // Update the UI
    }
})();
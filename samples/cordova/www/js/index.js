"use strict";

(function() {
    var BackendUrl = "https://ZUMOAPPNAME.azurewebsites.net";

    var client, store, syncContext, todoTable;
    var addItemEl, refreshButtonEl, summaryEl, errorLogEl, textbox, itemListEl;

    document.addEventListener('deviceready', onDeviceReady, false);

    function onDeviceReady() {
        addItemEl = document.getElementById('add-item');
        refreshButtonEl = document.getElementById('refresh');
        summaryEl = document.getElementById('summary');
        errorLogEl = document.getElementById('errorlog');
        textbox = document.getElementById('new-item-text');
        itemListEl = document.getElementById('todo-items');

        client = new WindowsAzure.MobileServiceClient(BackendUrl);
        initializeStore().then(setup);
    }

    function initializeStore() {
        return new Promise((resolve) => resolve());
    }

    function syncLocalTable() {
        return new Promise((resolve) => resolve());
    }

    function setup() {
        todoTable = client.getTable('todoitem');
        refreshDisplay();
        addItemEl.addEventListener('submit', addItemHandler);
        refreshButtonEl.addEventListener('click', refreshDisplay);
    }

    function refreshDisplay() {
        updateSummaryMessage('Loading data from Azure Mobile Apps');
        syncLocalTable().then(displayItems);
    }

    function displayItems() {
        todoTable
            .where({ complete: false })
            .read()
            .then(createTodoItemList, handleError);
    }

    function updateSummaryMessage(msg) {
        summaryEl.innerHTML = msg;
    }

    function createTodoItem(item) {
        const button='<button class="item-delete">Delete</button>';
        const checked = item.complete ? ' checked' : '';
        const checkbox=`<input type="checkbox" class="item-complete"${checked}/>`;
        const text = `<div><input class="item-text">${item.text}</input></div>`;
        return `<li data-todoitem-id="${item.id}">${button}${checkbox}${text}</li>`;
    }

    function createTodoItemList(items) {
        itemListEl.innerHTML = "";
        items.map((item) => itemListEl.append(createTodoItem(item)));
        updateSummaryMessage(`${items.length} item(s)`);

        // Wire up the event handlers
        var deleteEls = document.querySelectorAll('.item-delete');
        deleteEls.forEach((el) => el.addEventListener('click', deleteItemHandler));

        var textEls = document.querySelectorAll('.item-text');
        textEls.forEach((el) => el.addEventListener('change', updateItemTextHandler));

        var checkEls = document.querySelectorAll('.item-complete');
        checkEls.forEach((el) => el.addEventListener('change', updateItemCompleteHandler));
    }

    function handleError(error) {
        var text = error + (error.request ? ' - ' + error.request.status : '');
        console.error(text);
        errorLogEl.append(`<li>${text}</li>`);
    }

    function getTodoItemId(el) {
        return el.parentElement.dataset['todoitemId'];
    }

    function addItemHandler(event) {
        var itemText = textbox.value;
        if (itemText !== '') {
            updateSummaryMessage('Adding new item');
            todoTable.insert({ text: itemText, complete: false })
                .then(displayItems, handleError);
        }
        textbox.value = '';
        textbox.focus();
        event.preventDefault();
    }

    function deleteItemHandler(event) {
        var itemId = getTodoItemId(event.currentTarget);

        updateSummaryMessage('Deleting item in Azure');
        todoTable.del({ id: itemId })
            .then(displayItems, handleError);
        event.preventDefault();
    }

    function updateItemTextHandler(event) {
        var itemId = getTodoItemId(event.currentTarget),
            newText = event.currentTarget.value;

        updateSummaryMessage('Updating item in Azure');
        todoTable.update({ id: itemId, text: newText })
            .then(displayItems, handleError);
        event.preventDefault();
    }

    function updateItemCompleteHandler(event) {
        var itemId = getTodoItemId(event.currentTarget),
            isComplete = event.currentTarget.checked;

        updateSummaryMessage('Updating item in Azure');
        todoTable.update({ id: itemId, complete: isComplete })
            .then(displayItems, handleError);
    }
})();


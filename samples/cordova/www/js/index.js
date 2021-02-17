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
        console.log(item);
        const checkbox=`<input type="checkbox" data-todoitem-id="${item.id}" class="item-complete"${item.Complete ? ' checked' : ''}/>`;
        const text = `<div><input data-todoitem-id="${item.id}" class="item-text" value="${item.Text}"/></div>`;
        const el = document.createElement('li');
        el.innerHTML = `${checkbox}${text}`;
        return el;
    }

    function createTodoItemList(items) {
        itemListEl.innerHTML = "";
        items.map((item) => itemListEl.appendChild(createTodoItem(item)));
        updateSummaryMessage(`${items.length} item(s)`);

        // Wire up the event handlers
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

    function updateItemTextHandler(event) {
        var itemId = event.currentTarget.dataset['todoitemId'],
            newText = event.currentTarget.value;

        updateSummaryMessage('Updating item in Azure');
        todoTable.update({ id: itemId, Text: newText })
            .then(displayItems, handleError);
        event.preventDefault();
    }

    function updateItemCompleteHandler(event) {
        var itemId = event.currentTarget.dataset['todoitemId'],
            isComplete = event.currentTarget.checked;

        updateSummaryMessage('Updating item in Azure');
        todoTable.update({ id: itemId, Complete: isComplete })
            .then(displayItems, handleError);
    }
})();

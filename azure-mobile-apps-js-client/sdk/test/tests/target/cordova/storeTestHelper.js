// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file Helper file for local store unit tests
 */

var Platform = require('../../../../src/Platform'),
    Query = require('azure-query-js').Query,
    operations = require('../../../../src/sync/operations'),
    MobileServiceSqliteStore = require('../../../../src/Platform/cordova/MobileServiceSqliteStore'),
    operationTableName = require('../../../../src/constants').table.operationTableName,
    pulltimeTableName = require('../../../../src/constants').table.pulltimeTableName,
    testTableName = 'todoitem',
    testDbFile = 'somedbfile.db',
    store;

// Method need not be async, but defining it async to be consistent with createEmptyStore    
function createStore() {
    return Platform.async(function(callback) {
        callback();
    })().then(function() {
        if (!store) {
            store = new MobileServiceSqliteStore(testDbFile);
        }

        return store.init(); 
    }).then(function() {
        return Object.create(store);
    });
}

function resetStore(store) {
    return Platform.async(function(callback) {
        store._db.sqlBatch([
            'DROP TABLE IF EXISTS ' + operationTableName,
            'DROP TABLE IF EXISTS ' + pulltimeTableName,
            'DROP TABLE IF EXISTS ' + testTableName
        ], function() {
            callback(null, store);
        }, function(error) {
            callback(error);
        });
    })();
}

function createEmptyStore() {
    return createStore().then(function(store) {
        return resetStore(store);
    });
}

module.exports = {
    createStore: createStore,
    createEmptyStore: createEmptyStore,
    resetStore: resetStore,
    
    testTableName: testTableName
};

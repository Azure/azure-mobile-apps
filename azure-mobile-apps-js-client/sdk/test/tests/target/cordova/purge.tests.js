// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file unit tests for the 'purge' module
 * 
 */

var Platform = require('../../../../src/Platform'),
    Query = require('azure-query-js').Query,
    createPurgeManager = require('../../../../src/sync/purge').createPurgeManager,
    tableConstants = require('../../../../src/constants').table,
    MobileServiceClient = require('../../../../src/MobileServiceClient'),
    storeTestHelper = require('./storeTestHelper'),
    testHelper = require('../../shared/testHelper'),
    runner = require('../../../../src/Utilities/taskRunner'),
    createOperationTableManager = require('../../../../src/sync/operations').createOperationTableManager,
    MobileServiceSqliteStore = require('../../../../src/Platform/cordova/MobileServiceSqliteStore'),
    store,
    client,
    syncContext,
    tableName = storeTestHelper.testTableName,
    purgeManager;
    
$testGroup('purge tests')

    // Clear the store before running each test.
    .beforeEachAsync(function() {
        return storeTestHelper.createEmptyStore().then(function(emptyStore) {
            store = emptyStore;
            purgeManager = createPurgeManager(store, runner());
            client = new MobileServiceClient('http://someurl');
            syncContext = client.getSyncContext();
            return store.defineTable({
                name: tableName,
                columnDefinitions: {
                    id: 'string',
                    text: 'text'
                }
            }).then(function() {
                return client.getSyncContext().initialize(store);
            });
        });
    }).tests(

    $test('Vanilla purge - purge query matching entire table')
    .checkAsync(function () {
        var record1 = {id: '1', text: 'a'},
            record2 = {id: '2', text: 'b'},
            records = [record1, record2],
            tableQuery = new Query(tableName),
            purgeQuery = tableQuery;

        var actions = [
            // Add record and incremental state
            [store, store.upsert, tableName, records],
            addIncrementalSyncState,
            // Perform purge
            [purgeManager, purgeManager.purge, purgeQuery],
            // Verify purge
            verifyIncrementalSyncStateIsRemoved,
            [store, store.read, tableQuery],
            function(result) {
                $assert.areEqual(result, []);
            }
        ];

        return testHelper.runActions(actions);
    }),

    $test('Vanilla purge - purge query matching entire table where table has too many records')
    .checkAsync(function () {
        var records = [],
            tableQuery = new Query(tableName),
            purgeQuery = tableQuery;

        for (var i = 0; i < 3000; i++) {
            records.push({id: 'id'+i, text: 'sometext'});
        }

        var actions = [
            // Add record and incremental state
            [store, store.upsert, tableName, records],
            addIncrementalSyncState,
            // Perform purge
            [purgeManager, purgeManager.purge, purgeQuery],
            // Verify purge
            verifyIncrementalSyncStateIsRemoved,
            [store, store.read, tableQuery],
            function(result) {
                $assert.areEqual(result, []);
            }
        ];

        return testHelper.runActions(actions);
    }),

    $test('Vanilla purge - purge query not matching all records')
    .checkAsync(function () {
        var record1 = {id: '1', text: 'a'},
            record2 = {id: '2', text: 'b'},
            records = [record1, record2],
            tableQuery = new Query(tableName),
            purgeQuery = new Query(tableName).where(function() {
                return this.id === '1';
            });

        var actions = [
            // Add record and incremental state
            [store, store.upsert, tableName, records],
            addIncrementalSyncState,
            // Perform purge
            [purgeManager, purgeManager.purge, purgeQuery],
            // Verify purge
            verifyIncrementalSyncStateIsRemoved,
            [store, store.read, tableQuery],
            function(result) {
                $assert.areEqual(result, [record2]);
            }
        ];

        return testHelper.runActions(actions);
    }),

    $test('Vanilla purge - purge query matching no record')
    .checkAsync(function () {
        var record1 = {id: '1', text: 'a'},
            record2 = {id: '2', text: 'b'},
            records = [record1, record2],
            tableQuery = new Query(tableName),
            purgeQuery = new Query(tableName).where(function() {
                return this.id === 'non existent id';
            });

        var actions = [
            // Add record and incremental state
            [store, store.upsert, tableName, records],
            addIncrementalSyncState,
            // Perform purge
            [purgeManager, purgeManager.purge, purgeQuery],
            // Verify purge
            verifyIncrementalSyncStateIsRemoved,
            [store, store.read, tableQuery],
            function(result) {
                $assert.areEqual(result, records);
            }
        ];

        return testHelper.runActions(actions);
    }),

    $test('Vanilla purge - pending operations in the operation table')
    .checkAsync(function () {
        var record = {id: '1', text: 'a'},
            tableQuery = new Query(tableName),
            purgeQuery = tableQuery;

        var actions = [
            // Add record, pending operation and incremental state
            [syncContext, syncContext.insert, tableName, record],
            addIncrementalSyncState,
            // Perform purge
            [purgeManager, purgeManager.purge, purgeQuery],
            // Verify purge
            {
                fail: function(error) {
                    // failure expected - pending operations in the queue
                }
            },
            // purge shouldn't have remove the table data
            [store, store.read, tableQuery],
            function(result) {
                $assert.areEqual(result, [record]);
            },
            // purge shouldn't have removed the pending operation
            [store, store.read, new Query(tableConstants.operationTableName)],
            function(result) {
                $assert.areEqual(result.length, 1);
            },
            // purge shouldn't have removed the incremental sync state
            [store, store.read, new Query(tableConstants.pulltimeTableName)],
            function(result) {
                var incrementalSyncReset = true;
                result.forEach(function(record) {
                    if (record.tableName === tableName) {
                        incrementalSyncReset = false;
                    }
                });
                $assert.isFalse(incrementalSyncReset);
            }
        ];

        return testHelper.runActions(actions);
    }),

    $test('Force purge - pending operations in the operation table')
    .checkAsync(function () {
        var record = {id: '1', text: 'a'},
            tableQuery = new Query(tableName),
            purgeQuery = tableQuery;

        var actions = [
            // Add record, pending operation and incremental state
            [syncContext, syncContext.insert, tableName, record],
            addIncrementalSyncState,
            // Perform purge
            [purgeManager, purgeManager.purge, purgeQuery, true /* force purge */],
            // Verify purge
            verifyIncrementalSyncStateIsRemoved,
            verifyPendingOperationsAreRemoved,
            [store, store.read, tableQuery],
            function(result) {
                $assert.areEqual(result, []);
            }
        ];

        return testHelper.runActions(actions);
    })
);

function addIncrementalSyncState() {
    return store.upsert(tableConstants.pulltimeTableName, [
        {
            id: '1',
            tableName: tableName,
            value: new Date()
        },
        {
            id: '2',
            tableName: 'someothertablename',
            value: new Date()
        }
    ]).then(function() {
        // no action needed
    }, function(error) {
        $assert.fail(error);
    });
}

function verifyIncrementalSyncStateIsRemoved() {
    return store.read(new Query(tableConstants.pulltimeTableName)).then(function(result) {
        result.forEach(function(record) {
            if (record.tableName === tableName) {
                $assert.fail('incremental sync state not reset');
            }
        });
    }, function(error) {
        $assert.fail(error);
    });
}

function verifyPendingOperationsAreRemoved() {
    var query = new Query(tableConstants.operationTableName).where(function(tableName) {
        return this.tableName === tableName;
    }, tableName);

    return store.read(query).then(function(result) {
        $assert.areEqual(result, []);
    }, function(error) {
        $assert.fail(error);
    });
}

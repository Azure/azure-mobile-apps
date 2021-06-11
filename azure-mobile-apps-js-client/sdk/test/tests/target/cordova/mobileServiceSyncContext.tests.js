// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file MobileServiceSyncContext unit tests
 */

var Platform = require('../../../../src/Platform'),
    Query = require('azure-query-js').Query,
    operations = require('../../../../src/sync/operations'),
    MobileServiceClient = require('../../../../src/MobileServiceClient'),
    tableConstants = require('../../../../src/constants').table,
    MobileServiceSyncContext = require('../../../../src/sync/MobileServiceSyncContext'),
    storeTestHelper = require('./storeTestHelper'),
    MobileServiceSqliteStore = require('../../../../src/Platform/cordova/MobileServiceSqliteStore');
    
var store,
    testOperationId = 100000,
    testId = 'someid',
    testName = 'somename',
    testRecord = {id: testId, name: testName},
    filterRecord = {id: testId, name: 'filter name'}, // record that will be returned from the store layer's filter function
    operationTableName = tableConstants.operationTableName;

$testGroup('MobileServiceSyncContext tests')

    // Clear the local store before running each test.
    .beforeEachAsync(function() {
        return storeTestHelper.createEmptyStore().then(function(emptyStore) {
            store = emptyStore;
        });
    }).tests(
        
    $test('Lookup test')
    .description('test verifies that lookup calls store.lookup and returns its result')
    .checkAsync(function () {
        
        store.lookup = function(tableName, id) {
            $assert.areEqual(tableName, storeTestHelper.testTableName);
            $assert.areEqual(id, filterRecord.id);
            return filterRecord;
        };
        
        return performActionWithCustomLogging(filterRecord.id, 'lookup').then(function(result) {
            $assert.areEqual(result, filterRecord);
        }, function(error) {
            $assert.fail(error);
        });
    }),

    $test('Read test')
    .description('test verifies that read calls store.read and returns its result')
    .checkAsync(function () {
        
        var records = [ {id: 1} ];
        var read = store.read;
        store.read = function(query) {
            if (query.getComponents().table === storeTestHelper.testTableName) {
                return records;
            }
                
            return read.apply(store, arguments);
        };
        
        return performActionWithCustomLogging(filterRecord.id, 'read').then(function(result) {
            $assert.areEqual(result, records);
        }, function(error) {
            $assert.fail(error);
        });
    }),

    $test('Insert should record operations in the operation table')
    .description('test verifies that insert executes operations specified by the operation table manager in a single batch')
    .checkAsync(function () {
        
        store.executeBatch = function(operationBatch) {
            $assert.areEqual(operationBatch, [
                {
                    action: 'upsert',
                    tableName: storeTestHelper.testTableName,
                    data: {
                        id: testId,
                        name: testName
                    }
                },
                {
                    action: 'upsert',
                    tableName: operationTableName,
                    data: {
                        id: testOperationId,
                        action: 'insert_override',
                        tableName: storeTestHelper.testTableName,
                        itemId: testId
                    }
                }
            ]);
            return filterRecord;
        };
        
        return performActionWithCustomLogging(testId, 'insert').then(function(result) {
            $assert.areEqual(result, testRecord);
        }, function(error) {
            $assert.fail(error);
        });
    }),

    $test('Update should record operations in the operation table')
    .description('test verifies that update executes the operations specified by the operation table manager in a single batch')
    .checkAsync(function () {
        
        store.executeBatch = function(operationBatch) {
            $assert.areEqual(operationBatch, [
                {
                    action: 'upsert',
                    tableName: storeTestHelper.testTableName,
                    data: {
                        id: testId,
                        name: testName
                    }
                },
                {
                    action: 'upsert',
                    tableName: operationTableName,
                    data: {
                        id: testOperationId,
                        action: 'update_override',
                        tableName: storeTestHelper.testTableName,
                        itemId: testId
                    }
                }
            ]);
            return filterRecord;
        };
        
        // insert a record in the table in advance, so that we can update it subsequently
        return defineTestTable().then(function() {
            return store.upsert(storeTestHelper.testTableName, {id: testId});
        }).then(function() {
            return performActionWithCustomLogging(testId, 'update');
        }).then(function(result) {
            $assert.areEqual(result, testRecord);
        }, function(error) {
            $assert.fail(error);
        });
    }),

    $test('Del should record operations in the operation table')
    .description('test verifies that del executes the operations specified by the operation table manager in a single batch')
    .checkAsync(function () {
        
        store.executeBatch = function(operationBatch) {
            $assert.areEqual(operationBatch, [
                {
                    action: 'delete',
                    tableName: storeTestHelper.testTableName,
                    id: testId
                },
                {
                    action: 'upsert',
                    tableName: operationTableName,
                    data: {
                        id: testOperationId,
                        action: 'delete_override',
                        tableName: storeTestHelper.testTableName,
                        itemId: testId
                    }
                }
            ]);
        };
        
        return performActionWithCustomLogging(testId, 'del').then(function(result) {
            $assert.isNull(result);
        }, function(error) {
            $assert.fail(error);
        });
    }),

    $test('Insert should fail iff object with same id already exists')
    .checkAsync(function () {

        var syncContext;
        return getSyncContext().then(function(context) {
            syncContext = context;
            return syncContext.insert(storeTestHelper.testTableName, {id: testId});
        }).then(function() {
            // NOP - success expected
        }, function(error) {
            $assert.fail(error);
        }).then(function(context) {
            return syncContext.insert(storeTestHelper.testTableName, {id: testId});
        }).then(function() {
            $assert.fail('should have failed to insert as a record with same id already exists');
        }, function(error) {
            // NOP - failure expected
        });
    }),

    $test('Update should work even if record with same id already exists')
    .checkAsync(function () {

        var syncContext;
        return getSyncContext().then(function(context) {
            syncContext = context;
            return syncContext.update(storeTestHelper.testTableName, {id: testId});
        }).then(function() {
            // NOP - success expected
        }, function(error) {
            $assert.fail(error);
        }).then(function(context) {
            return syncContext.update(storeTestHelper.testTableName, {id: testId});
        }).then(function() {
            // NOP - success expected
        }, function(error) {
            $assert.fail(error);
        });
    }),

    $test('Inserting object without ID should auto-generate ID')
    .checkAsync(function () {

        store.executeBatch = function(operationBatch) {
            $assert.isNotNull(operationBatch);
            $assert.areEqual(operationBatch.length, 2);
            $assert.isNotNull(operationBatch[0].data.id);
            $assert.isNotNull(operationBatch[1].data.id);
            return filterRecord;
        };
        
        return performActionWithCustomLogging(undefined, 'insert').then(function(result) {
            $assert.isNotNull(result);
            $assert.isNotNull(result.id);
            $assert.areEqual(result.name, testName);
        }, function(error) {
            $assert.fail(error);
        });
    }),

    $test('Updating object without ID should fail')
    .checkAsync(function () {
        
        store.executeBatch = function(operationBatch) {
            $assert.fail('We should have failed long before this getting called!');
        };
        
        // insert a record in the table in advance, so that we can update it subsequently
        return defineTestTable().then(function() {
            return store.upsert(storeTestHelper.testTableName, {id: testId});
        }).then(function() {
            return performActionWithCustomLogging(undefined /* do not set ID */, 'update');
        }).then(function(result) {
            $assert.fail('failure expected');
        }, function(error) {
            // failure expected
        });
    }),

    $test('Deleting object without ID should fail')
    .checkAsync(function () {
        
        store.executeBatch = function(operationBatch) {
            $assert.fail('We should have failed long before this getting called!');
        };
        
        // insert a record in the table in advance, so that we can update it subsequently
        return defineTestTable().then(function() {
            return store.upsert(storeTestHelper.testTableName, {id: testId});
        }).then(function() {
            return performActionWithCustomLogging(undefined /* do not set ID */, 'del');
        }).then(function(result) {
            $assert.fail('failure expected');
        }, function(error) {
            // failure expected
        });
    }),

    $test('Verify insert, update, delete do not run concurrently')
    .checkAsync(function () {
        var syncContext,
            actionInProgress,
            operationCount = 0,
            repetitionCount = 1,
            completion;
            
        function notifyWhenDone() {
            if (operationCount === repetitionCount * 3 /* insert, update and delete */) {
                if (completion) {
                    completion();
                    completion = undefined;
                }
            } else if (operationCount > repetitionCount * 3 - 1) {
                $assert.fail('completed operation count cannot exceed the number of operations that were started!');
            }
        }
        
        function updateOperationCount() {
            ++operationCount;
            notifyWhenDone();
        }
            
        return createSyncContextWithLongExecutionTime(function(action) { // callback1
            $assert.isNull(actionInProgress); // verify no other operation is in progress
            actionInProgress = action;
        }, function() { // callback2
            actionInProgress = undefined;
            notifyWhenDone();
        }).then(function(context) {
            syncContext = context;
            var id = testId;
            // Perform insert, update and delete for a few times to verify they don't overlap
            for (var i = 0; i < repetitionCount; i++) {
                id = id + '!';
                syncContext.insert(storeTestHelper.testTableName, {id: testId}).then(updateOperationCount);
                
                syncContext.update(storeTestHelper.testTableName, {id: testId}).then(updateOperationCount);
                
                syncContext.del(storeTestHelper.testTableName, {id: testId}).then(updateOperationCount);
                
                id = id + id; // generate next id. will work for numbers as well as strings.
            }
        }).then(function() {
            return Platform.async(function(callback) {
                completion = callback;
                notifyWhenDone();
            })();
        }, function(error) {
            $assert.fail(error);
        });
    }),

    $test('purge')
    .description('Tests that the purge API simply calls pullManager.purge() and returns whatever it returns')
    .checkAsync(function () {
        var syncContext;
        return getSyncContext().then(function(result) {
            syncContext = result;
            syncContext._getPurgeManager().purge = function (query, forcePurge) {
                $assert.areEqual(query, {dummykey: 'dummyvalue'});
                $assert.areEqual(forcePurge, true);
                return 'result';
            };
        }).then(function() {
            return syncContext.purge({dummykey: 'dummyvalue'}, true);
        }).then(function(result) {
            $assert.areEqual(result, 'result');
        });
    })
);

function getSyncContext() {
    var syncContext = new MobileServiceSyncContext(new MobileServiceClient('someurl'));
    return syncContext.initialize(store).then(function() {
        return defineTestTable();
    }).then(function() {
        return syncContext;
    });
}

function performActionWithCustomLogging(id, action) {
    var syncContext;
    return getSyncContext().then(function(context) {
        syncContext = context;
        syncContext._getOperationTableManager().getLoggingOperation = function(tableName, action, item) {
            return Platform.async(function(callback) {
                callback();
            })().then(function() {
                return {
                    tableName: operationTableName,
                    action: 'upsert',
                    data: {
                        id: testOperationId,
                        tableName: tableName,
                        action: action + '_override',
                        itemId: item.id
                    }
                };
            });
        };
    }).then(function() {
        if (action === 'lookup') {
            return syncContext.lookup(storeTestHelper.testTableName, id);
        } else if (action === 'read') {
            return syncContext.read(new Query(storeTestHelper.testTableName));
        } else if (action === 'insert') {
            return syncContext.insert(storeTestHelper.testTableName, {id: id, name: testName});
        } else if (action === 'update') {
            return syncContext.update(storeTestHelper.testTableName, {id: id, name: testName});
        } else if (action === 'del') {
            return syncContext.del(storeTestHelper.testTableName, {id: id});
        } else {
            throw new Error('unsupported action. fix the test.');
        }
    });
}

// callback1 and callback2 will be invoked at two arbitrary points of time during the execution of insert/update/delete.
// Though they won't be invoked at the very beginning and end of insert/update/delete operations, a custom delay introduced
// while getting the log operation gives us a good enough way to detect concurrent executions of these operations
function createSyncContextWithLongExecutionTime(callback1, callback2) {
    var syncContext = new MobileServiceSyncContext(new MobileServiceClient('someurl'));

    return syncContext.initialize(store).then(function() {
        return defineTestTable();
    }).then(function() {
        // Override getLoggingOperation() and simulate long execution time
        
        var getLoggingOperation = syncContext._getOperationTableManager().getLoggingOperation;
        syncContext._getOperationTableManager().getLoggingOperation = function(tableName, action, item) {
            var args = arguments;
            callback1();
            return Platform.async(function(callback) {
                setTimeout(function() {
                    callback2();
                    callback();
                }, 500);
                callback();
            })().then(function() {
                return getLoggingOperation.apply(syncContext._getOperationTableManager(), args);
            });
        };
        
        return syncContext;
    });
}

function defineTestTable() {
    return store.defineTable({
        name: storeTestHelper.testTableName,
        columnDefinitions: {
            id: MobileServiceSqliteStore.ColumnType.Text,
            name: MobileServiceSqliteStore.ColumnType.String
        }
    });
}

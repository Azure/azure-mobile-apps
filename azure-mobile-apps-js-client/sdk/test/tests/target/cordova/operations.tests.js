// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file unit tests for the 'operations' module
 */

var Platform = require('../../../../src/Platform'),
    Query = require('azure-query-js').Query,
    operations = require('../../../../src/sync/operations'),
    tableConstants = require('../../../../src/constants').table,
    MobileServiceSqliteStore = require('../../../../src/Platform/cordova/MobileServiceSqliteStore'),
    storeTestHelper = require('./storeTestHelper');
    
var createOperationTableManager = operations.createOperationTableManager,
    operationTableName = tableConstants.operationTableName,
    store,
    testId = 'abc',
    testVersion = 'testversion',
    testItem = {id: testId},
    testMetadata = {version: 'someversion'};

$testGroup('operations tests')

    // Clear the store before running each test.
    .beforeEachAsync(function() {
        return storeTestHelper.createEmptyStore().then(function(emptyStore) {
            store = emptyStore;
        }).then(function() {
            return store.defineTable({
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.String,
                    version: MobileServiceSqliteStore.ColumnType.String
                }
            });
        });
    }).tests(

    $test('verify initialization')
    .checkAsync(function () {
        var operationTableManager = createOperationTableManager(store);
        
        return operationTableManager.initialize().then(function() {
            return store.read(new Query(operationTableName));
        }).then(function(result) {
            $assert.areEqual(result, []);
        }, function(error) {
            $assert.fail(error);
        });
    }),
    
    $test('basic logging')
    .checkAsync(function () {
        var operationTableManager = createOperationTableManager(store);
        
        return operationTableManager.initialize().then(function() {
            return operationTableManager.getLoggingOperation(storeTestHelper.testTableName, 'insert', testItem);
        }).then(function(op) {
            return store.executeBatch([op]);
        }).then(function() {
            return operationTableManager.readPendingOperations(storeTestHelper.testTableName, testItem.id);
        }).then(function(result) {
            $assert.areEqual(result, [
                {
                    action: 'insert',
                    id: 1,
                    itemId: testId,
                    tableName: storeTestHelper.testTableName,
                    metadata: {}
                }
            ]);
        }, function(error) {
            $assert.fail(error);
        });
    }),
    
    $test('operation ID generation')
    .checkAsync(function () {
        var operationTableManager = createOperationTableManager(store),
            item1 = {id: 'abc'},
            item2 = {id: 'def'};
        
        return operationTableManager.initialize().then(function() {
            return operationTableManager.getLoggingOperation(storeTestHelper.testTableName, 'insert', item1); // insert item1
        }).then(function(op) {
            return store.executeBatch([op]);
        }).then(function() {
            return operationTableManager.readPendingOperations(storeTestHelper.testTableName, item1.id);
        }).then(function(result) {
            $assert.areEqual(result, [
                {
                    action: 'insert',
                    id: 1,
                    itemId: item1.id,
                    tableName: storeTestHelper.testTableName,
                    metadata: {}
                }
            ]);
        }).then(function() {
            return operationTableManager.getLoggingOperation(storeTestHelper.testTableName, 'delete', item1); // delete item1
        }).then(function(op) {
            return store.executeBatch([op]);
        }).then(function() {
            return operationTableManager.getLoggingOperation(storeTestHelper.testTableName, 'insert', item1); // insert item1 again
        }).then(function(op) {
            return store.executeBatch([op]);
        }).then(function() {
            return operationTableManager.readPendingOperations(storeTestHelper.testTableName, item1.id);
        }).then(function(result) {
            $assert.areEqual(result, [
                {
                    action: 'insert',
                    id: 2,
                    itemId: item1.id,
                    tableName: storeTestHelper.testTableName,
                    metadata: {}
                }
            ]);
        }).then(function() {
            operationTableManager = createOperationTableManager(store); // create new instance of operation table manager
            return operationTableManager.initialize();
        }).then(function() {
            return operationTableManager.getLoggingOperation(storeTestHelper.testTableName, 'insert', item2); // insert item2
        }).then(function(op) {
            return store.executeBatch([op]);
        }).then(function() {
            return operationTableManager.readPendingOperations(storeTestHelper.testTableName, item2.id);
        }).then(function(result) {
            $assert.areEqual(result, [
                {
                    action: 'insert',
                    id: 3,
                    itemId: item2.id,
                    tableName: storeTestHelper.testTableName,
                    metadata: {}
                }
            ]);
        }, function(error) {
            $assert.fail(error);
        });
    }),
    
    // Successful operation sequences starting with insert..
     
    $test('getLoggingOperation insert')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['insert'], [
            {id: 1, action: 'insert'}
        ]);
    }),
    
    $test('getLoggingOperation insert, update')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['insert', 'update'], [
            {id: 1, action: 'insert'}
        ]);
    }),
    
    $test('getLoggingOperation insert, delete')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['insert', 'delete'], []);
    }),
    
    $test('getLoggingOperation insert, lock, update')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['insert', 'lock', 'update'], [
            {id: 1, action: 'insert'},
            {id: 2, action: 'update'},
        ]);
    }),
    
    $test('getLoggingOperation insert, lock, delete')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['insert', 'lock', 'delete'], [
            {id: 1, action: 'insert'},
            {id: 2, action: 'delete'},
        ]);
    }),
    
    // Successful operation sequences starting with update..
    
    $test('getLoggingOperation update')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['update'], [
            {id: 1, action: 'update'}
        ]);
    }),
    
    $test('getLoggingOperation update, update')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['update', 'update'], [
            {id: 1, action: 'update'}
        ]);
    }),
    
    $test('getLoggingOperation update, delete')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['update', 'delete'], [
            {id: 1, action: 'delete'}
        ]);
    }),
    
    $test('getLoggingOperation update, lock, update')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['update', 'lock', 'update'], [
            {id: 1, action: 'update'},
            {id: 2, action: 'update'},
        ]);
    }),
    
    $test('getLoggingOperation update, lock, delete')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['update', 'lock', 'delete'], [
            {id: 1, action: 'update'},
            {id: 2, action: 'delete'},
        ]);
    }),
    
    // Successful operation sequences starting with delete..
    
    $test('getLoggingOperation delete')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['delete'], [
            {id: 1, action: 'delete'}
        ]);
    }),
    
    $test('getLoggingOperation delete, delete')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['delete', 'delete'], [
            {id: 1, action: 'delete'}
        ]);
    }),
    
    $test('getLoggingOperation delete, lock, delete')
    .checkAsync(function () {
        return performActionsAndVerifySuccess(['delete', 'lock', 'delete'], [
            {id: 1, action: 'delete'},
            {id: 2, action: 'delete'},
        ]);
    }),
    
    // Failure sequences starting with insert...
    
    $test('getLoggingOperation insert, insert')
    .checkAsync(function () {
        return performActionsAndVerifyError(['insert'], 'insert');
    }),
    
    $test('getLoggingOperation insert, lock, insert')
    .checkAsync(function () {
        return performActionsAndVerifyError(['insert', 'lock'], 'insert');
    }),
    
    // Failure sequences starting with update...
    
    $test('getLoggingOperation update, insert')
    .checkAsync(function () {
        return performActionsAndVerifyError(['update'], 'insert');
    }),
    
    $test('getLoggingOperation update, lock, insert')
    .checkAsync(function () {
        return performActionsAndVerifyError(['update', 'lock'], 'insert');
    }),
    
    // Failure sequences starting with delete...
    
    $test('getLoggingOperation delete, insert')
    .checkAsync(function () {
        return performActionsAndVerifyError(['delete'], 'insert');
    }),
    
    $test('getLoggingOperation delete, update')
    .checkAsync(function () {
        return performActionsAndVerifyError(['delete'], 'update');
    }),
    
    $test('getLoggingOperation delete, lock, insert')
    .checkAsync(function () {
        return performActionsAndVerifyError(['delete', 'lock'], 'insert');
    }),
    
    $test('getLoggingOperation delete, lock, update')
    .checkAsync(function () {
        return performActionsAndVerifyError(['delete', 'lock'], 'update');
    }),
    
    $test('readFirstPendingOperationWithData reads - insert log operation')
    .checkAsync(function () {
        var operationTableManager = createOperationTableManager(store);
        var logRecord1 = { id: 1001, action: 'update', tableName: storeTestHelper.testTableName, itemId: 'a', metadata: testMetadata },
            logRecord2 = { id: 1,    action: 'insert', tableName: storeTestHelper.testTableName, itemId: 'b', metadata: testMetadata },
            logRecord3 = { id: 1002,    action: 'insert', tableName: storeTestHelper.testTableName, itemId: 'c', metadata: testMetadata },
            logRecord4 = { id: 2001, action: 'delete', tableName: storeTestHelper.testTableName, itemId: 'dd', metadata: testMetadata },
            data1 = { id: 'a', version: testVersion },
            data2 = { id: 'b', version: testVersion },
            data3 = { id: 'c', version: testVersion };            
            
        return operationTableManager.initialize().then(function() {
            return store.executeBatch([
                { tableName: operationTableName, action: 'upsert', data: logRecord1 },
                { tableName: operationTableName, action: 'upsert', data: logRecord2 },
                { tableName: operationTableName, action: 'upsert', data: logRecord3 },
                { tableName: operationTableName, action: 'upsert', data: logRecord4 },
                { tableName: storeTestHelper.testTableName, action: 'upsert', data: data1 },
                { tableName: storeTestHelper.testTableName, action: 'upsert', data: data2 },
                { tableName: storeTestHelper.testTableName, action: 'upsert', data: data3 }
            ]);
        }).then(function() {
            return operationTableManager.readFirstPendingOperationWithData(-1);
        }).then(function(result) {
            $assert.areEqual(result, {
                logRecord: logRecord2,
                data: data2
            });
        }, function(error) {
            $assert.fail(error);
        }).then(function() {
            return operationTableManager.readFirstPendingOperationWithData(1001);
        }).then(function(result) {
            $assert.areEqual(result, {
                logRecord: logRecord3,
                data: data3
            });
        }, function(error) {
            $assert.fail(error);
        });
    }),
    
    $test('readFirstPendingOperationWithData - update log operation')
    .checkAsync(function () {
        var operationTableManager = createOperationTableManager(store);
        var logRecord1 = { id: 1001, action: 'insert', tableName: storeTestHelper.testTableName, itemId: 'a', metadata: testMetadata },
            logRecord2 = { id: 1,    action: 'update', tableName: storeTestHelper.testTableName, itemId: 'b', metadata: testMetadata },
            logRecord3 = { id: 500,    action: 'update', tableName: storeTestHelper.testTableName, itemId: 'c', metadata: testMetadata },
            logRecord4 = { id: 2001, action: 'delete', tableName: storeTestHelper.testTableName, itemId: 'dd', metadata: testMetadata },
            data1 = { id: 'a', version: testVersion },
            data2 = { id: 'b', version: testVersion },
            data3 = { id: 'c', version: testVersion };
            
        return operationTableManager.initialize().then(function() {
            return store.executeBatch([
                { tableName: operationTableName, action: 'upsert', data: logRecord1 },
                { tableName: operationTableName, action: 'upsert', data: logRecord2 },
                { tableName: operationTableName, action: 'upsert', data: logRecord3 },
                { tableName: operationTableName, action: 'upsert', data: logRecord4 },
                { tableName: storeTestHelper.testTableName, action: 'upsert', data: data1 },
                { tableName: storeTestHelper.testTableName, action: 'upsert', data: data2 },
                { tableName: storeTestHelper.testTableName, action: 'upsert', data: data3 }
            ]);
        }).then(function() {
            return operationTableManager.readFirstPendingOperationWithData(-1);
        }).then(function(result) {
            $assert.areEqual(result, {
                logRecord: logRecord2,
                data: data2
            });
        }, function(error) {
            $assert.fail(error);
        }).then(function() {
            return operationTableManager.readFirstPendingOperationWithData(1);
        }).then(function(result) {
            $assert.areEqual(result, {
                logRecord: logRecord3,
                data: data3
            });
        }, function(error) {
            $assert.fail(error);
        });
    }),
    
    $test('readFirstPendingOperationWithData - delete log operation')
    .checkAsync(function () {
        var operationTableManager = createOperationTableManager(store);
        var logRecord1 = { id: 1001, action: 'insert', tableName: storeTestHelper.testTableName, itemId: 'a', metadata: testMetadata },
            logRecord2 = { id: 1,    action: 'delete', tableName: storeTestHelper.testTableName, itemId: 'dd1', metadata: testMetadata },
            logRecord3 = { id: 5000,    action: 'delete', tableName: storeTestHelper.testTableName, itemId: 'dd2', metadata: testMetadata },
            logRecord4 = { id: 2001, action: 'update', tableName: storeTestHelper.testTableName, itemId: 'b', metadata: testMetadata },
            data1 = { id: 'a', version: testVersion },
            data2 = { id: 'b', version: testVersion };            
            
        return operationTableManager.initialize().then(function() {
            return store.executeBatch([
                { tableName: operationTableName, action: 'upsert', data: logRecord1 },
                { tableName: operationTableName, action: 'upsert', data: logRecord2 },
                { tableName: operationTableName, action: 'upsert', data: logRecord3 },
                { tableName: operationTableName, action: 'upsert', data: logRecord4 },
                { tableName: storeTestHelper.testTableName, action: 'upsert', data: data1 },
                { tableName: storeTestHelper.testTableName, action: 'upsert', data: data2 }
            ]);
        }).then(function() {
            return operationTableManager.readFirstPendingOperationWithData(-1);
        }).then(function(result) {
            $assert.areEqual(result, {
                logRecord: logRecord2
            });
        }, function(error) {
            $assert.fail(error);
        }).then(function() {
            return operationTableManager.readFirstPendingOperationWithData(3000);
        }).then(function(result) {
            $assert.areEqual(result, {
                logRecord: logRecord3
            });
        }, function(error) {
            $assert.fail(error);
        });
    }),
    
    $test('readFirstPendingOperationWithData - first log record without data record, next log record has data record')
    .checkAsync(function () {
        var operationTableManager = createOperationTableManager(store);
        var logRecord1 = { id: 1001, action: 'udpate', tableName: storeTestHelper.testTableName, itemId: 'a', metadata: testMetadata },
            logRecord2 = { id: 1,    action: 'insert', tableName: storeTestHelper.testTableName, itemId: 'b', metadata: testMetadata },
            logRecord3 = { id: 2001, action: 'delete', tableName: storeTestHelper.testTableName, itemId: 'dd', metadata: testMetadata },
            data1 = { id: 'a', version: testVersion },
            data2 = { id: 'c', version: testVersion };            
            
        return operationTableManager.initialize().then(function() {
            return store.executeBatch([
                { tableName: operationTableName, action: 'upsert', data: logRecord1 },
                { tableName: operationTableName, action: 'upsert', data: logRecord2 },
                { tableName: operationTableName, action: 'upsert', data: logRecord3 },
                { tableName: storeTestHelper.testTableName, action: 'upsert', data: data1 },
                { tableName: storeTestHelper.testTableName, action: 'upsert', data: data2 }
            ]);
        }).then(function() {
            return operationTableManager.readFirstPendingOperationWithData(-1);
        }).then(function(result) {
            $assert.areEqual(result, {
                logRecord: logRecord1,
                data: data1
            });
            return store.read(new Query(operationTableName).orderBy('id'));
        }).then(function(result) {
            $assert.areEqual(result, [logRecord1, logRecord3]);
        }, function(error) {
            $assert.fail(error);
        });
    }),
    
    $test('readFirstPendingOperationWithData - first log record without data record, next log record does not exist')
    .checkAsync(function () {
        var operationTableManager = createOperationTableManager(store);
        var logRecord1 = { id: 1,    action: 'insert', tableName: storeTestHelper.testTableName, itemId: 'b', metadata: testMetadata };            
            
        return operationTableManager.initialize().then(function() {
            return store.executeBatch([
                { tableName: operationTableName, action: 'upsert', data: logRecord1 }
            ]);
        }).then(function() {
            return operationTableManager.readFirstPendingOperationWithData(-1);
        }).then(function(result) {
            $assert.isNull(result);
            return store.read(new Query(operationTableName));
        }).then(function(result) {
            $assert.areEqual(result, []);
        }, function(error) {
            $assert.fail(error);
        });
    }),
    
    $test('readFirstPendingOperationWithData - log record does not exist')
    .checkAsync(function () {
        var operationTableManager = createOperationTableManager(store);
            
        return operationTableManager.initialize().then(function() {
            return operationTableManager.readFirstPendingOperationWithData(-1);
        }).then(function(result) {
            $assert.isNull(result);
        }, function(error) {
            $assert.fail(error);
        });
    }),
    
    $test('removeLockedOperation')
    .checkAsync(function () {
        var operationTableManager = createOperationTableManager(store);
        var logRecord1 = { id: 1, action: 'udpate', tableName: storeTestHelper.testTableName, itemId: 'a', metadata: testMetadata },
            logRecord2 = { id: 101,    action: 'insert', tableName: storeTestHelper.testTableName, itemId: 'b', metadata: testMetadata },
            logRecord3 = { id: 2001, action: 'delete', tableName: storeTestHelper.testTableName, itemId: 'dd', metadata: testMetadata };
            
        return operationTableManager.initialize().then(function() {
            return store.executeBatch([
                { tableName: operationTableName, action: 'upsert', data: logRecord1 },
                { tableName: operationTableName, action: 'upsert', data: logRecord2 },
                { tableName: operationTableName, action: 'upsert', data: logRecord3 }
            ]);
        }).then(function() {
            return operationTableManager.lockOperation(logRecord2.id);
        }).then(function(result) {
            return operationTableManager.removeLockedOperation();
        }).then(function(result) {
            return store.read(new Query(operationTableName).orderBy('id'));
        }).then(function(result) {
            $assert.areEqual(result, [logRecord1, logRecord3]);
        }, function(error) {
            $assert.fail(error);
        });
    }),

    // getMetadata tests - action: insert

    $test('getMetadata - action is insert, new record has version')
    .checkAsync(function () {
        return verify_getMetadata('insert', undefined, {version: testVersion}, {version: testVersion});
    }),
    
    $test('getMetadata - action is insert, new record has null version')
    .checkAsync(function () {
        return verify_getMetadata('insert', undefined, {version: null}, {version: null});
    }),
    
    $test('getMetadata - action is insert, new record has no version')
    .checkAsync(function () {
        return verify_getMetadata('insert', undefined, {}, {version: undefined});
    }),
    
    // getMetadata tests - action: upsert

    $test('getMetadata - action is upsert, new record has version')
    .checkAsync(function () {
        return verify_getMetadata('upsert', undefined, {version: testVersion}, {version: testVersion});
    }),
    
    $test('getMetadata - action is upsert, new record has null version')
    .checkAsync(function () {
        return verify_getMetadata('upsert', undefined, {version: null}, {version: null});
    }),
    
    $test('getMetadata - action is upsert, new record has no version')
    .checkAsync(function () {
        return verify_getMetadata('upsert', undefined, {}, {version: undefined});
    }),
    
    // getMetadata tests - action: update

    $test('getMetadata - action is update, store has no such record, new record has version')
    .checkAsync(function () {
        return verify_getMetadata('update', undefined, {version: testVersion}, {version: testVersion});
    }),
    
    $test('getMetadata - action is update, store has no such record, new record has null version')
    .checkAsync(function () {
        return verify_getMetadata('update', undefined, {version: null}, {version: null});
    }),
    
    $test('getMetadata - action is update, store has no such record, new record has no version')
    .checkAsync(function () {
        return verify_getMetadata('update', undefined, {}, {});
    }),
    
    $test('getMetadata - action is update, store has record without version, new record has version')
    .checkAsync(function () {
        return verify_getMetadata('update', {version: undefined}, {version: testVersion}, {version: testVersion});
    }),
    
    $test('getMetadata - action is update, store has record without version, new record has null version')
    .checkAsync(function () {
        return verify_getMetadata('update', {version: undefined}, {version: null}, {version: null});
    }),
    
    $test('getMetadata - action is update, store has record without version, new record has no version')
    .checkAsync(function () {
        return verify_getMetadata('update', {version: undefined}, {}, {version: null});
    }),
    
    $test('getMetadata - action is update, store has record with version, new record has new version')
    .checkAsync(function () {
        return verify_getMetadata('update', {version: 'oldversion'}, {version: testVersion}, {version: testVersion});
    }),

    $test('getMetadata - action is update, store has record with version, new record has same version')
    .checkAsync(function () {
        return verify_getMetadata('update', {version: testVersion}, {version: testVersion}, {version: testVersion});
    }),

    $test('getMetadata - action is update, store has record with version, new record has null version')
    .checkAsync(function () {
        return verify_getMetadata('update', {version: testVersion}, {version: undefined}, {version: undefined});
    }),

    $test('getMetadata - action is update, store has record with version, new record has no version')
    .checkAsync(function () {
        return verify_getMetadata('update', {version: testVersion}, {}, {version: testVersion});
    }),

    // getMetadata tests - action: delete

    $test('getMetadata - action is delete, store has no such record')
    .checkAsync(function () {
        return verify_getMetadata('delete', undefined, {}, {});
    }),

    $test('getMetadata - action is delete, store has record with version')
    .checkAsync(function () {
        return verify_getMetadata('delete', {version: testVersion}, {}, {version: testVersion});
    }),

    $test('getMetadata - action is delete, store has record with null version')
    .checkAsync(function () {
        return verify_getMetadata('delete', {version: null}, {}, {version: null});
    }),

    $test('getMetadata - action is delete, store has record with no version')
    .checkAsync(function () {
        return verify_getMetadata('delete', {}, {}, {version: null});
    }),

    $test('getMetadata - action is delete, store has record with no version, specified record has version')
    .description('verify that version passed to getMetadata is not used in case of a delete operation')
    .checkAsync(function () {
        return verify_getMetadata('delete', {}, {version: testVersion}, {version: null});
    }),

    $test('getOperationForInsertingLog')
    .description('verify that getOperationForInsertingLog uses whatever metadata is returned by getMetadata()')
    .checkAsync(function () {
        return verify_getOperation(function(operationTableManager) {
            return operationTableManager._getOperationForInsertingLog(storeTestHelper.testTableName, 'someaction', {id: testId});
        }, function(operation) {
            $assert.areEqual(operation, {
                action: 'upsert',
                tableName: operationTableName,
                data: {
                    id: 1,
                    tableName: storeTestHelper.testTableName,
                    action: 'someaction',
                    itemId: testId,
                    metadata: 'some-metadata'
                }
            });
        });
    }),

    $test('getOperationForUpdatingLog')
    .description('verify that getOperationForInsertingLog uses whatever metadata is returned by getMetadata()')
    .checkAsync(function () {
        return verify_getOperation(function(operationTableManager) {
            return operationTableManager._getOperationForUpdatingLog(1, storeTestHelper.testTableName, 'someaction', {id: testId});
        }, function(operation) {
            $assert.areEqual(operation, {
                action: 'upsert',
                tableName: operationTableName,
                data: {
                    id: 1,
                    action: 'someaction',
                    metadata: 'some-metadata'
                }
            });
        });
    })
);

function verify_getOperation(getOperation, verifyOperation) {
    var operationTableManager = createOperationTableManager(store);
    operationTableManager.getMetadata = function() {
        return Platform.async(function(callback) {
            callback();
        })().then(function() {
            return 'some-metadata';
        });
    };

    return operationTableManager.initialize().then(function() {
        return getOperation(operationTableManager);
    }).then(function(result) {
        verifyOperation(result);
    }, function(error) {
        $assert.fail(error);
    });
}

function verify_getMetadata(action, existingRecord, record, expectedMetadata) {
    var operationTableManager = createOperationTableManager(store);
    
    return operationTableManager.initialize().then(function() {
        // Setup the local table
        if (existingRecord) {
            existingRecord.id = testId;
            return store.upsert(storeTestHelper.testTableName, existingRecord);
        }
    }).then(function(result) {
        // call getMetadata
        record.id = testId; 
        return operationTableManager.getMetadata(storeTestHelper.testTableName, action, record);
    }).then(function(result) {
        // verify result of getMetadata
        expectedMetadata = expectedMetadata;
        $assert.areEqual(result, expectedMetadata);
    }, function(error) {
        $assert.fail(error);
    });
}

// Perform the specified actions and verify that the operation table has the expected operations
function performActionsAndVerifySuccess(actions, expectedOperations) {
    var operationTableManager = createOperationTableManager(store);

    return performActions(operationTableManager, testItem, actions).then(function() {
        $assert.isNotNull(expectedOperations);
        return verifyOperations(operationTableManager, testItem.id, expectedOperations);
    }, function(error) {
        $assert.isNull(expectedOperations);
    });
}

// Perform the specified setupActions and then verify that errorAction fails
function performActionsAndVerifyError(setupActions, errorAction) {
    var operationTableManager = createOperationTableManager(store);

    return performActions(operationTableManager, testItem, setupActions).then(function() {
        return performActions(operationTableManager, testItem.id, [errorAction]);
    }, function(error) {
        $assert.fail(error);
    }).then(function() {
        $assert.fail('failure expected');
    }, function(error) {
        // Failure Expected.
    });
}

// Perform actions specified by the actions array. Valid values for the actions
// array are 'insert', 'update', 'delete', 'lock' and 'unlock'.
function performActions(operationTableManager, item, actions) {
    var asyncChain = operationTableManager.initialize();
    actions = actions || [];
    for (var i = 0; i < actions.length; i++) {
        asyncChain = performAction(asyncChain, operationTableManager, item, actions[i]);
    }
    return asyncChain;
}

function performAction(asyncChain, operationTableManager, item, action) {
    return asyncChain.then(function() {
        if (action === 'insert' || action === 'update' || action === 'delete') {
            return operationTableManager.getLoggingOperation(storeTestHelper.testTableName, action, item).then(function(operation) {
                return store.executeBatch([operation]);
            });
        } else if (action === 'lock') {
            return operationTableManager.lockOperation(1 /* For this test the first operation will always have ID = 1 */);
        } else if (action === 'unlock') {
            return operationTableManager.unlockOperation();
        } else {
            throw new Error('something is wrong');
        }
    });
}

// Verify that the pending operations in the operation table are as expected
function verifyOperations(operationTableManager, itemId, expectedOperations) {
    return operationTableManager.readPendingOperations(storeTestHelper.testTableName, itemId).then(function(operations) {
        
        expectedOperations = expectedOperations || [];
        for (var i = 0; i < expectedOperations.length; i++) {
            expectedOperations[i].tableName = storeTestHelper.testTableName;
            expectedOperations[i].itemId = itemId;
            expectedOperations[i].metadata = expectedOperations[i].metadata || {};
        }
        
        $assert.areEqual(operations, expectedOperations);
    }, function(error) {
        $assert.fail(error);
    });
}

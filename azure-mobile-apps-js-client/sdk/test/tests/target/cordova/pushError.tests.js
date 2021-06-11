// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file unit tests for the 'pushError' module
 */

var Platform = require('../../../../src/Platform'),
    Query = require('azure-query-js').Query,
    tableConstants = require('../../../../src/constants').table,
    storeTestHelper = require('./storeTestHelper'),
    runner = require('../../../../src/Utilities/taskRunner'),
    createOperationTableManager = require('../../../../src/sync/operations').createOperationTableManager,
    createPushError = require('../../../../src/sync/pushError').createPushError,
    MobileServiceSqliteStore = require('../../../../src/Platform/cordova/MobileServiceSqliteStore');

var operationTableName = tableConstants.operationTableName,
    store,
    testVersion = 'someversion',
    testError = {
        request: {
            status: 400
        }
    },
    testId = 'someid';
    
$testGroup('pushError tests')

    // Clear the store before running each test.
    .beforeEachAsync(function() {
        return storeTestHelper.createEmptyStore().then(function(emptyStore) {
            store = emptyStore;
        });
    }).tests(

    $test('pushError.getError()')
    .description("verify that modifying values returned by pushError's member methods does not affect its future behavior")
    .check(function () {
        var testId = 'someid',
            operationTableManager = createOperationTableManager(store),
            pushOperation = {
                logRecord: {
                    id: 101,
                    tableName: storeTestHelper.testTableName,
                    action: 'someaction',
                    itemId: testId,
                    metadata: {}
                },
                data: {
                    clientKey: 'clientVal'
                }
            },
            operationError = {
                request: {
                    status: 409
                },
                serverInstance: {
                    serverKey: 'serverVal'
                }
            },
            pushError = createPushError(store, operationTableManager, runner(), pushOperation, operationError);

            var error = pushError.getError(),
                action = pushError.getAction(),
                serverRecord = pushError.getServerRecord(),
                clientRecord = pushError.getClientRecord(),
                tableName = pushError.getTableName(),
                isConflict = pushError.isConflict();

            var actionCopy = makeCopy(action),
                serverRecordCopy = makeCopy(serverRecord),
                clientRecordCopy = makeCopy(clientRecord),
                tableNameCopy = makeCopy(tableName),
                isConflictCopy = makeCopy(isConflict);

            // Modify the properties of interest
            error.request.status.newprop1 = 'somevalue';
            error.serverInstance.newprop2 = 'somevalue';
            action.newprop3 = 'somevalue';
            serverRecord.newprop4 = 'somevalue';
            clientRecord.newprop5 = 'somevalue';
            tableName.newprop6 = 'somevalue';
            isConflict.newprop7 = 'somevalue';
            error.request.status = -1;
            error.serverInstance = {somekey: 'somevalue'};

            // Verify that pushError methods still behave the same
            $assert.areEqual(pushError.getAction(), actionCopy);
            $assert.areEqual(pushError.getServerRecord(), serverRecordCopy);
            $assert.areEqual(pushError.getClientRecord(), clientRecordCopy);
            $assert.areEqual(pushError.getTableName(), tableNameCopy);
            $assert.areEqual(pushError.isConflict(), isConflictCopy);
    }),
    
    $test('pushError.update()')
    .description('verify update() uses whatever metadata is returned by operationTableManager.getMetadata()')
    .checkAsync(function () {
        var testId = 'someid',
            operationTableManager = createOperationTableManager(store),
            pushOperation = {
                logRecord: {
                    id: 101,
                    tableName: storeTestHelper.testTableName,
                    action: 'someaction',
                    itemId: testId,
                    metadata: {}
                },
                data: {
                    id: testId
                }
            },
            pushError = createPushError(store, operationTableManager, runner(), pushOperation, testError);

        var batchExecuted;
        store.executeBatch = function(batch) {
            return Platform.async(function(callback) {
                $assert.areEqual(batch[0].data.metadata, 'some-metadata');
                batchExecuted = true;
                callback();
            })();
        };

        operationTableManager.getMetadata = function() {
            return Platform.async(function(callback) {
                return callback(null, 'some-metadata');
            })();
        };

        return pushError.update({ id: testId }).then(function() {
            $assert.isTrue(batchExecuted);
        });
    }),

    // changeAction(insert) tests
    $test('pushError.changeAction() - new action is insert, new record value specifies version')
    .checkAsync(function () {
        return verifyChangeAction('update', 'insert', testId, testId, undefined, testVersion, true, testVersion);
    }),

    $test('pushError.changeAction() - new action is insert, new record value specifies different ID')
    .checkAsync(function () {
        return verifyChangeAction('update', 'insert', testId, 'changed id', testVersion, testVersion, false);
    }),

    $test('pushError.changeAction() - new action is insert, new record value not specified, old action is update')
    .checkAsync(function () {
        return verifyChangeAction('update', 'insert', testId, undefined, testVersion, undefined, true, testVersion);
    }),

    $test('pushError.changeAction() - new action is insert, new record value not specified, old action is delete')
    .checkAsync(function () {
        return verifyChangeAction('delete', 'insert', testId, undefined, testVersion, undefined, false);
    }),

    // changeAction(update) tests
    $test('pushError.changeAction() - new action is update, new record value specifies version')
    .checkAsync(function () {
        return verifyChangeAction('insert', 'update', testId, testId, undefined, testVersion, true, testVersion);
    }),

    $test('pushError.changeAction() - new action is update, new record value specifies different ID')
    .checkAsync(function () {
        return verifyChangeAction('update', 'update', testId, 'changed id', testVersion, testVersion, false);
    }),

    $test('pushError.changeAction() - new action is update, new record value not specified, old action is insert')
    .checkAsync(function () {
        return verifyChangeAction('insert', 'update', testId, undefined, testVersion, undefined, true, testVersion);
    }),

    $test('pushError.changeAction() - new action is update, new record value not specified, old action is delete')
    .checkAsync(function () {
        return verifyChangeAction('delete', 'update', testId, undefined, testVersion, undefined, false);
    }),

    // changeAction(delete) tests
    $test('pushError.changeAction() - new action is delete, new record value specifies version')
    .checkAsync(function () {
        return verifyChangeAction('insert', 'delete', testId, testId, undefined, testVersion, true, testVersion);
    }),

    $test('pushError.changeAction() - new action is delete, new record value specifies different ID')
    .checkAsync(function () {
        return verifyChangeAction('update', 'delete', testId, 'changed id', testVersion, testVersion, false);
    }),

    $test('pushError.changeAction() - new action is delete, new record value not specified, old action is insert')
    .checkAsync(function () {
        return verifyChangeAction('insert', 'delete', testId, undefined, testVersion, undefined, true, testVersion);
    }),

    $test('pushError.changeAction() - new action is update, new record value not specified, old action is delete')
    .checkAsync(function () {
        return verifyChangeAction('delete', 'update', testId, undefined, testVersion, undefined, false);
    })
);

function verifyChangeAction(oldAction, newAction, oldItemId, newItemId, oldVersion, newVersion, isSuccessExpected, expectedVersion) {
    var testId = 'someid',
        operationTableManager = createOperationTableManager(store),
        pushOperation = {
            logRecord: {
                id: 101,
                tableName: storeTestHelper.testTableName,
                action: oldAction,
                itemId: oldItemId,
                metadata: { version: oldVersion }
            },
            data: {
                id: oldItemId
                // version not set intentionally. we want to make sure the test succeeds by using the version value in the new record.
            }
        },
        pushError = createPushError(store, operationTableManager, runner(), pushOperation, testError);

    var batchExecuted;
    store.executeBatch = function(batch) {
        return Platform.async(function(callback) {
            batchExecuted = batch;
            callback();
        })();
    };

    var newRecord;
    if (newItemId) {
        newRecord = {id: newItemId, version: newVersion};
    }
    return pushError.changeAction(newAction, newRecord).then(function() {
        $assert.isTrue(isSuccessExpected);
        // 0th operation in the batch modifies the log record
        $assert.areEqual(batchExecuted[0].data.metadata.version, expectedVersion);
        $assert.areEqual(batchExecuted[0].data.action, newAction);

        // 1st operation in the batch modifies the data in the local table
        if (newAction === 'delete') {
            $assert.areEqual(batchExecuted.length, 2);
            $assert.areEqual(batchExecuted[1].action, 'delete');
        }
    }, function(error) {
        $assert.isNull(batchExecuted);
        $assert.isFalse(isSuccessExpected);
    });
}

function makeCopy(obj) {
    if (obj === undefined || obj === null) {
        return obj;
    }
    return JSON.parse( JSON.stringify(obj) );
}
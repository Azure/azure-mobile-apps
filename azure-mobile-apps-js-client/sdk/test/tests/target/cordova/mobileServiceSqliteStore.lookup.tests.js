// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file MobileServiceSqliteStore.lookup(..) unit tests
 */

var Platform = require('../../../../src/Platform'),
    Query = require('azure-query-js').Query,
    storeTestHelper = require('./storeTestHelper'),
    MobileServiceSqliteStore = require('../../../../src/Platform/cordova/MobileServiceSqliteStore'),
    store;

$testGroup('SQLiteStore - lookup tests')

    // Clear the test table before running each test.
    .beforeEachAsync(function() {
        return storeTestHelper.createEmptyStore().then(function(emptyStore) {
            store = emptyStore;
        });
    }).tests(

    $test('table not defined')
    .checkAsync(function () {
        return store.lookup(storeTestHelper.testTableName, 'one').then(function (result) {
            $assert.fail('failure expected');
        }, function (err) {
        });
    }),

    $test('Id of type string')
    .checkAsync(function () {
        var row = { id: 'someid', price: 51.5 };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                price: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, row);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, row.id);
        }).then(function (result) {
            $assert.areEqual(result, row);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('Id of type integer')
    .checkAsync(function () {
        var row = { id: 51, price: 51.5 };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                price: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, row);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, '51');
        }).then(function (result) {
            $assert.areEqual(result, row);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('Id of type real')
    .checkAsync(function () {
        var row = { id: 21.11, price: 51.5 };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Real,
                price: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, row);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, '21.11');
        }).then(function (result) {
            $assert.areEqual(result, row);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('verify id case insensitivity')
    .checkAsync(function () {
        var row = { id: 'ABC', description: 'something' };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, row);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, 'abc');
        }).then(function (result) {
            $assert.areEqual(result, row);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('read columns that are missing in table definition')
    .checkAsync(function () {
        var row = { id: 'ABC', column1: 1, column2: 2 },
            tableDefinition = {
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.Text,
                    column1: MobileServiceSqliteStore.ColumnType.Integer,
                    column2: MobileServiceSqliteStore.ColumnType.Integer
                }
            };

        return store.defineTable(tableDefinition).then(function () { 
            return store.upsert(storeTestHelper.testTableName, row);
        }).then(function () {
            // Redefine the table without column2
            delete tableDefinition.columnDefinitions.column2;
            return store.defineTable(tableDefinition);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, 'abc');
        }).then(function (result) {
            $assert.areEqual(result, row);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('record not found - suppressRecordNotFoundError === false')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                price: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, 'someid');
        }).then(function (result) {
            $assert.fail('lookup should have failed');
        }, function (error) {
            // NOP - failure expected
        });
    }),

    $test('record not found - suppressRecordNotFoundError === true')
    .description('Check that promise returned by lookup is either resolved or rejected even when invoked with extra parameters')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                price: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, 'some id that does not exist', true /* suppressRecordNotFoundError */);
        }).then(function (result) {
            $assert.isNull(result);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('invoked with extra parameters')
    .description('Check that promise returned by lookup is either resolved or rejected even when invoked with extra parameters')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                price: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, 'some id', false /* suppressRecordNotFoundError */, 'extra param');
        }).then(function (result) {
            $assert.fail('should have failed');
        }, function (error) {
            // NOP - failure expected
        });
    }),

    $test('null id')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                price: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, null);
        }).then(function (result) {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('id defined as undefined')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                price: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, undefined);
        }).then(function (result) {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('id property not defined')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                price: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, undefined);
        }).then(function (result) {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('invalid id')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                price: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, {invalid: 'invalid'});
        }).then(function (result) {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('null table name')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.lookup(null, [{ id: 'something', description: 'something' }]);
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('undefined table name')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.lookup(undefined, [{ id: 'something', description: 'something' }]);
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('invalid table name')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.lookup('*', [{ id: 'something', description: 'something' }]);
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('invoked without any parameter')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.lookup();
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('verify deserialization error is handled properly')
    .checkAsync(function() {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                prop: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function() {
            return store.upsert(storeTestHelper.testTableName, { id: '1', prop: 1.5 });
        }).then(function() {
            // Change table definition to introduce deserialization error;
            return store.defineTable({
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.String,
                    prop: MobileServiceSqliteStore.ColumnType.Date
                }
            });
        }).then(function() {
            return store.lookup(storeTestHelper.testTableName, '1');
        }).then(function(result) {
            $assert.fail('lookup should have failed');
        }, function(error) {
        });
    })
);

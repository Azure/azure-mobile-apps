// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file MobileServiceSqliteStore.upsert(..) unit tests
 */

var Platform = require('../../../../src/Platform'),
    Query = require('azure-query-js').Query,
    storeTestHelper = require('./storeTestHelper'),
    MobileServiceSqliteStore = require('../../../../src/Platform/cordova/MobileServiceSqliteStore');

$testGroup('SQLiteStore - upsert tests')

    // Clear the test table before running each test.
    .beforeEachAsync(function() {
        return storeTestHelper.createEmptyStore().then(function(emptyStore) {
            store = emptyStore;
        });
    }).tests(

    $test('table is not defined')
    .checkAsync(function () {
        var row = { id: 101, description: 'some description' };

        return store.upsert(storeTestHelper.testTableName, row).then(function (result) {
            $assert.fail('failure expected');
        }, function (err) {
        });
    }),

    $test('insert new record and then update it')
    .checkAsync(function () {
        var row = { id: 'some id', price: 100 };

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
            // Change property value and upsert again
            row.price = 5000.1;
            return store.upsert(storeTestHelper.testTableName, row);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, row.id);
        }).then(function (result) {
            $assert.areEqual(result, row);
        }).then(function () {
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('array of records, all having the same id')
    .checkAsync(function () {
        var rows = [{ id: 't1', description: 'description1', price: 5 }, { id: 't1', description: 'description2' }];

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String,
                price: MobileServiceSqliteStore.ColumnType.Integer
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, rows);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName));
        }).then(function (result) {
            $assert.areEqual(result, [{ id: 't1', description: 'description2', price: 5 }]);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('update property of an existing record')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                prop1: MobileServiceSqliteStore.ColumnType.Real,
                prop2: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, { id: 'some id', prop1: 100, prop2: 200 });
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, 'some id');
        }).then(function (result) {
            $assert.areEqual(result, { id: 'some id', prop1: 100, prop2: 200 });
            // Update property of an existing record
            return store.upsert(storeTestHelper.testTableName, { id: 'some id', prop2: -99999 });
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, 'some id');
        }).then(function (result) {
            $assert.areEqual(result, { id: 'some id', prop1: 100, prop2: -99999 });
        }).then(function () {
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('verify id case sensitivity')
    .checkAsync(function () {
        var row1 = { id: 'ABC', description: 'old' },
            row2 = { id: 'abc', description: 'new' };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            // record with an upper cased id
            return store.upsert(storeTestHelper.testTableName, row1);
        }).then(function () {
            // update record using a lower cased id
            return store.upsert(storeTestHelper.testTableName, row2);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName));
        }).then(function (result) {
            $assert.areEqual(result, [row1, row2]);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('insert array of records containing a null object')
    .checkAsync(function () {
        var row = { id: 'ABC', description: 'something' };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, [null, row]);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, row.id);
        }).then(function (result) {
            $assert.areEqual(result, row);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('insert array of records containing an undefined object')
    .checkAsync(function () {
        var row = { id: 'ABC', description: 'something' };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, [undefined, row]);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, row.id);
        }).then(function (result) {
            $assert.areEqual(result, row);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('empty table name')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.upsert('', [{ id: 'something', description: 'something' }]);
        }).then(function () {
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
            return store.upsert(null, [{ id: 'something', description: 'something' }]);
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
            return store.upsert(undefined, [{ id: 'something', description: 'something' }]);
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
            return store.upsert('*', [{ id: 'something', description: 'something' }]);
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('empty array')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, []);
        }).then(function () {
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('record is null')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, null);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName));
        }).then(function (result) {
            $assert.areEqual(result, []);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('record is undefined')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, undefined);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName));
        }).then(function (result) {
            $assert.areEqual(result, []);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('adding record with columns that are not defined should succeed')
    .checkAsync(function () {
        var row = { id: 101, flag: 51, undefinedColumn: 1 },
            tableDefinition = {
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.Integer,
                    flag: MobileServiceSqliteStore.ColumnType.Integer
                }
            };

        return store.defineTable(tableDefinition).then(function () {
            return store.upsert(storeTestHelper.testTableName, row);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, row.id);
        }).then(function (result) {
            $assert.areEqual(result, { id: row.id, flag: row.flag });
        }).then(function (result) {
            // Success expected
        }, function (err) {
            $assert.fail(err);
        });
    }),

    $test('adding record with incorrect column type should fail')
    .checkAsync(function () {
        var row = { id: 101, flag: [1, 2] },
            tableDefinition = {
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.Integer,
                    flag: MobileServiceSqliteStore.ColumnType.Integer
                }
            };

        return store.defineTable(tableDefinition).then(function () {
            return store.upsert(storeTestHelper.testTableName, row);
        }).then(function (result) {
            $assert.fail('test should have failed');
        }, function (err) {
        });
    }),

    $test('record is not an object')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                prop1: MobileServiceSqliteStore.ColumnType.Real,
                prop2: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, 1000);
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('record does not have an id property')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                prop1: MobileServiceSqliteStore.ColumnType.Real,
                prop2: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, { prop1: 100, prop2: 200 });
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('record id is null')
    .description('Check that promise returned by upsert is either resolved or rejected even when id is null')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                prop1: MobileServiceSqliteStore.ColumnType.Real,
                prop2: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, { id: null, prop1: 100, prop2: 200 });
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('record id is defined as undefined')
    .description('Check that promise returned by upsert is either resolved or rejected even when id is undefined')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                prop1: MobileServiceSqliteStore.ColumnType.Real,
                prop2: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, { id: undefined, prop1: 100, prop2: 200 });
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('record does not have an id')
    .description('Check that promise returned by upsert is either resolved or rejected even when id is missing')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                prop1: MobileServiceSqliteStore.ColumnType.Real,
                prop2: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, { prop1: 100, prop2: 200 });
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('ID property not the first property that gets enumerated')
    .checkAsync(function () {
        var record = { // Defined property names such that id will not be the first property to be enumerated
            a_prop1: 100,
            id: 'someid',
            z_prop2: 200
        };
        
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                a_prop1: MobileServiceSqliteStore.ColumnType.Real,
                z_prop2: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, record);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, record.id);
        }).then(function (result) {
            return $assert.areEqual(result, record);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('invoked with extra arguments')
    .description('Check that promise returned by upsert is either resolved or rejected even when invoked with extra parameters')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                prop1: MobileServiceSqliteStore.ColumnType.Real,
                prop2: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, { id: 'someid', prop1: 100, prop2: 200 }, 'extra param');
        }).then(function () {
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('invoked without any arguments')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.upsert();
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),
    
    $test('verify serialization error is handled properly')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                prop: MobileServiceSqliteStore.ColumnType.Integer
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, {id: '1', prop: 1.5});
        }).then(function (result) {
            $assert.fail('test should have failed');
        }, function (error) {
        });
    })
);

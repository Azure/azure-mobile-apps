// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file Miscellaneous MobileServiceSqliteStore unit tests
 */

var Platform = require('../../../../src/Platform'),
    Query = require('azure-query-js').Query,
    storeTestHelper = require('./storeTestHelper'),
    testHelper = require('../../shared/testHelper'),
    MobileServiceSqliteStore = require('../../../../src/Platform/cordova/MobileServiceSqliteStore'),
    store;

$testGroup('SQLiteStore - miscellaneous tests')

    // Clear the test table before running each test.
    .beforeEachAsync(function() {
        return storeTestHelper.createEmptyStore().then(function(emptyStore) {
            store = emptyStore;
        });
    }).tests(

    $test('Roundtrip non-null property values')
    .checkAsync(function () {
        var row = {
                id: 'someid',
                object: {
                    int: 1,
                    string: 'str1'
                },
                array: [
                    2,
                    'str2',
                    {
                        int: 3,
                        array: [4, 5, 6]
                    }
                ],
                integer: 7,
                int: 8,
                float: 8.5,
                real: 9.5,
                string: 'str3',
                text: 'str4',
                boolean: true,
                bool: false,
                date: new Date(2015, 11, 11, 23, 5, 59)
            };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                object: MobileServiceSqliteStore.ColumnType.Object,
                array: MobileServiceSqliteStore.ColumnType.Array,
                integer: MobileServiceSqliteStore.ColumnType.Integer,
                int: MobileServiceSqliteStore.ColumnType.Int,
                float: MobileServiceSqliteStore.ColumnType.Float,
                real: MobileServiceSqliteStore.ColumnType.Real,
                string: MobileServiceSqliteStore.ColumnType.String,
                text: MobileServiceSqliteStore.ColumnType.Text,
                boolean: MobileServiceSqliteStore.ColumnType.Boolean,
                bool: MobileServiceSqliteStore.ColumnType.Bool,
                date: MobileServiceSqliteStore.ColumnType.Date
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

    $test('Roundtrip null property values')
    .checkAsync(function () {
        var row = {
                id: '1',
                object: null,
                array: null,
                integer: null,
                int: null,
                float: null,
                real: null,
                string: null,
                text: null,
                boolean: null,
                bool: null,
                date: null,
            };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                object: MobileServiceSqliteStore.ColumnType.Object,
                array: MobileServiceSqliteStore.ColumnType.Array,
                integer: MobileServiceSqliteStore.ColumnType.Integer,
                int: MobileServiceSqliteStore.ColumnType.Int,
                float: MobileServiceSqliteStore.ColumnType.Float,
                real: MobileServiceSqliteStore.ColumnType.Real,
                string: MobileServiceSqliteStore.ColumnType.String,
                text: MobileServiceSqliteStore.ColumnType.Text,
                boolean: MobileServiceSqliteStore.ColumnType.Boolean,
                bool: MobileServiceSqliteStore.ColumnType.Bool,
                date: MobileServiceSqliteStore.ColumnType.Date
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

    $test('Read table with columns missing from table definition')
    .checkAsync(function () {
        var row = { id: 101, flag: 51, object: { 'a': 21 } },
            tableDefinition = {
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.Integer,
                    flag: MobileServiceSqliteStore.ColumnType.Integer,
                    object: MobileServiceSqliteStore.ColumnType.Object
                }
            };

        return store.defineTable(tableDefinition).then(function () {
            return store.upsert(storeTestHelper.testTableName, row);
        }).then(function () {
            // Now change column definition to only contain id column
            delete tableDefinition.columnDefinitions.flag;
            return store.defineTable(tableDefinition);
        }).then(function () {
            // Now read data inserted before changing column definition
            return store.lookup(storeTestHelper.testTableName, row.id);
        }).then(function (result) {
            // Check that the original data is read irrespective of whether the properties are defined by defineColumn
            $assert.areEqual(result, row);
        }, function (err) {
            $assert.fail(err);
        });
    }),

    $test('ID column case insensitivity')
    .checkAsync(function () {
        var tableDefinition = {
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    Id: MobileServiceSqliteStore.ColumnType.Integer,
                    somecolumn: MobileServiceSqliteStore.ColumnType.Integer
                }
            };

        return testHelper.runActions([
            [store, store.defineTable, tableDefinition],
            [store, store.upsert, storeTestHelper.testTableName, {ID: 101, somecolumn: 51}],
            [store, store.lookup, storeTestHelper.testTableName, 101],
            function(result) {
                $assert.areEqual(result, {Id: 101, somecolumn: 51});
            },
            [store, store.upsert, storeTestHelper.testTableName, {iD: 101, somecolumn: 71}],
            [store, store.read, new Query(storeTestHelper.testTableName).where(function() { return this.iD === 101; })],
            function(result) {
                $assert.areEqual(result, [ {Id: 101, somecolumn: 71} ]);
            },
            [store, store.del, storeTestHelper.testTableName, 101],
            [store, store.read, new Query(storeTestHelper.testTableName)],
            function(result) {
                $assert.areEqual(result, []);
            }
        ]);
    }),

    $test('Non-ID column case insensitivity')
    .checkAsync(function () {
        var tableDefinition = {
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.Integer,
                    someCOLUMN: MobileServiceSqliteStore.ColumnType.Integer
                }
            };

        return testHelper.runActions([
            [store, store.defineTable, tableDefinition],
            [store, store.upsert, storeTestHelper.testTableName, {id: 101, SOMEcolumn: 51}],
            [store, store.lookup, storeTestHelper.testTableName, 101],
            function(result) {
                $assert.areEqual(result, {id: 101, someCOLUMN: 51});
            },
            [store, store.upsert, storeTestHelper.testTableName, {id: 101, SoMeCoLuMn: 71}],
            [store, store.read, new Query(storeTestHelper.testTableName).where(function() { return this.somECOlumN === 71; })],
            function(result) {
                $assert.areEqual(result, [ {id: 101, someCOLUMN: 71} ]);
            }
        ]);
    }),

    $test('Verify MobileServiceSqliteStore initialization works as expected with the new operator')
    .check(function () {
        var store = new MobileServiceSqliteStore('somedbname');
        $assert.isNotNull(store.upsert);
    }),
    
    $test('Verify MobileServiceSqliteStore initialization works as expected without the new operator')
    .check(function () {
        var store = MobileServiceSqliteStore('somedbname');
        $assert.isNotNull(store, 'somedbname');
        $assert.isNotNull(store.upsert);
    }),
    
    $test('MobileServiceSqliteStore constructor without db name')
    .check(function () {
        var localStore = MobileServiceSqliteStore('somedbname');
        $assert.isNotNull(store.upsert);
    }),
    
    $test('MobileServiceSqliteStore constructor with a null db name')
    .check(function () {
        var localStore = MobileServiceSqliteStore('somedbname');
        $assert.isNotNull(store.upsert);
    })
);

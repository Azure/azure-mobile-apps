// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file MobileServiceSqliteStore.read(..) unit tests
 */

var Platform = require('../../../../src/Platform'),
    Query = require('azure-query-js').Query,
    storeTestHelper = require('./storeTestHelper'),
    MobileServiceSqliteStore = require('../../../../src/Platform/cordova/MobileServiceSqliteStore'),
    store;

$testGroup('SQLiteStore - read tests')

    // Clear the test table before running each test.
    .beforeEachAsync(function() {
        return storeTestHelper.createEmptyStore().then(function(emptyStore) {
            store = emptyStore;
        });
    }).tests(

    $test('table not defined')
    .checkAsync(function () {
        return store.read(new Query(storeTestHelper.testTableName)).then(function (result) {
            $assert.fail('failure expected');
        }, function (err) {
        });
    }),

    $test('Read entire table')
    .checkAsync(function () {
        var rows = [{ id: 1, int: 101, str: 'text1' }, { id: 2, int: 102, str: 'text2' }];

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Int,
                int: MobileServiceSqliteStore.ColumnType.Int,
                str: MobileServiceSqliteStore.ColumnType.Text
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, rows);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName));
        }).then(function (results) {
            $assert.areEqual(results, rows);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('Read - query on a bool property')
    .checkAsync(function () {
        var record1 = { id: 1, flag: false };
        var record2 = { id: 2, flag: true  };
        var record3 = { id: 3, flag: null  };

        var records = [ record1, record2, record3 ];

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Int,
                flag: MobileServiceSqliteStore.ColumnType.Boolean
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, records);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName).where(function() {
                return this.flag === false;
            }));
        }).then(function (results) {
            $assert.areEqual(results, [record1]);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName).where(function(v) {
                return this.flag === v;
            }, true));
        }).then(function (results) {
            $assert.areEqual(results, [record2]);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName).where(function() {
                return this.flag === null;
            }));
        }).then(function (results) {
            $assert.areEqual(results, [record3]);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('Read - query on a string property')
    .checkAsync(function () {
        var record1 = { id: '1', name: 'a' };
        var record2 = { id: '2', name: ''  };
        var record3 = { id: '3', name: null  };

        var records = [ record1, record2, record3 ];

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                name: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, records);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName).where(function() {
                return this.name === 'a';
            }));
        }).then(function (results) {
            $assert.areEqual(results, [record1]);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName).where(function(v) {
                return this.name === v;
            }, ''));
        }).then(function (results) {
            $assert.areEqual(results, [record2]);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName).where(function() {
                return this.name === null;
            }));
        }).then(function (results) {
            $assert.areEqual(results, [record3]);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('Read - query on a date property')
    .checkAsync(function () {
        var record1 = { id: '1', time: new Date(2011, 10, 11, 12, 13, 14) };
        var record2 = { id: '2', time: null  };

        var records = [ record1, record2];

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                time: MobileServiceSqliteStore.ColumnType.Date
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, records);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName).where(function(v) {
                return this.time === v;
            }, record1.time));
        }).then(function (results) {
            $assert.areEqual(results, [record1]);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName).where(function() {
                return this.time === null;
            }));
        }).then(function (results) {
            $assert.areEqual(results, [record2]);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('Read - query on an integer property')
    .checkAsync(function () {
        var record1 = { id: '1', val: 1 };
        var record2 = { id: '2', val: 2  };

        var records = [ record1, record2];

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                val: MobileServiceSqliteStore.ColumnType.Integer
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, records);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName).where(function(v) {
                return this.val === 1;
            }, record1.time));
        }).then(function (results) {
            $assert.areEqual(results, [record1]);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('Read - query on a float property')
    .checkAsync(function () {
        var record1 = { id: '1', val: 1.1 };
        var record2 = { id: '2', val: 1.0 };
        var record3 = { id: '3', val: 2.1  };

        var records = [ record1, record2, record3];

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                val: MobileServiceSqliteStore.ColumnType.Float
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, records);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName).where(function(v) {
                return this.val === 1.1;
            }, record1.time));
        }).then(function (results) {
            $assert.areEqual(results, [record1]);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('simple select')
    .checkAsync(function () {
        var rows = [{ id: 1, int: 101, str: 'text1' }];

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Int,
                int: MobileServiceSqliteStore.ColumnType.Int,
                str: MobileServiceSqliteStore.ColumnType.Text
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, rows);
        }).then(function () {
            var query = new Query(storeTestHelper.testTableName);
            return store.read(query.select('str', 'int'));
        }).then(function (results) {
            $assert.areEqual(results, rows.map(function (obj) {
                return {
                    str: obj.str,
                    int: obj.int
                };
            }));
        }, function (error) {
        });
    }),

    $test('select invalid columns')
    .checkAsync(function () {
        var row = { id: 1, int: 101, str: 'text1' };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Int,
                int: MobileServiceSqliteStore.ColumnType.Int,
                str: MobileServiceSqliteStore.ColumnType.Text
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, row);
        }).then(function () {
            var query = new Query(storeTestHelper.testTableName);
            return store.read(query.select('invalid column'));
        }).then(function (results) {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('select same columns more than once')
    .checkAsync(function () {
        var row1 = { id: 1, int: 101, str: 'text1' },
            row2 = { id: 2, int: 102, str: 'text2' },
            row3 = { id: 3, int: 103, str: 'text3' },
            rows = [row1, row2, row3];

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Int,
                int: MobileServiceSqliteStore.ColumnType.Int,
                str: MobileServiceSqliteStore.ColumnType.Text
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, rows);
        }).then(function () {
            var query = new Query(storeTestHelper.testTableName);
            return store.read(query.select('id', 'id', 'str', 'str'));
        }).then(function (results) {
            $assert.areEqual(results, rows.map(function (obj) {
                return {
                    id: obj.id,
                    str: obj.str
                };
            }));
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('query a non-existent table')
    .checkAsync(function () {
        return store.read(new Query('nonexistenttable')).then(function (results) {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('query an invalid table name')
    .checkAsync(function () {
        return store.read(new Query('*')).then(function (results) {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('read performed without any arguments')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Text,
                description: MobileServiceSqliteStore.ColumnType.String
            }
        }).then(function () {
            return store.read();
        }).then(function () {
            $assert.fail('failure expected');
        }, function (error) {
        });
    }),

    $test('read invoked with extra parameters')
    .description('Check that promise returned by read is either resolved or rejected even when invoked with extra parameters')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                prop1: MobileServiceSqliteStore.ColumnType.Real,
                prop2: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName), 'extra param');
        }).then(function (result) {
            $assert.areEqual(result, []);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('verify deserialization error is handled properly')
    .checkAsync(function () {
        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.String,
                prop: MobileServiceSqliteStore.ColumnType.Real
            }
        }).then(function () {
            return store.upsert(storeTestHelper.testTableName, {id: 'not-an-integer', prop: 1.5});
        }).then(function () {
            // Change table definition to introduce deserialization error;
            return store.defineTable({
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.Integer,
                    prop: MobileServiceSqliteStore.ColumnType.Real
                }
            });
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName));
        }).then(function (result) {
            $assert.fail('test should have failed');
        }, function (error) {
        });
    })
);

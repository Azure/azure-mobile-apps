// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file MobileServiceSqliteStore.defineTable(..) unit tests
 */

var Platform = require('../../../../src/Platform'),
    Query = require('azure-query-js').Query,
    MobileServiceSqliteStore = require('../../../../src/Platform/cordova/MobileServiceSqliteStore'),
    storeTestHelper = require('./storeTestHelper'),
    testHelper = require('../../shared/testHelper');
    
var store;

$testGroup('SQLiteStore - defineTable tests')

    // Clear the test table before running each test.
    .beforeEachAsync(function() {
        return storeTestHelper.createEmptyStore().then(function(emptyStore) {
            store = emptyStore;
        });
    }).tests(

    $test('basic table definition')
    .checkAsync(function () {
        var row = { id: 101, price: 51.5 };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
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

    $test('table name case insensitivity')
    .checkAsync(function () {
        var row = { id: 101, price: 51.5 };

        $assert.isTrue(storeTestHelper.testTableName.length > 2);
        var originalTableName = '';
        for (var i = 0; i < storeTestHelper.testTableName.length; i++) {
            if (i % 2 === 0) {
                originalTableName += storeTestHelper.testTableName[i].toLowerCase();
            } else {
                originalTableName += storeTestHelper.testTableName[i].toUpperCase();
            }
        }

        return testHelper.runActions([
            [null, runCrudCaseTest, originalTableName, originalTableName],
            [null, runCrudCaseTest, originalTableName, originalTableName.toLowerCase()],
            [null, runCrudCaseTest, originalTableName, originalTableName.toUpperCase()]
        ]);

        function runCrudCaseTest (creationTableName, crudTableName) {
            return testHelper.runActions(
                [
                    [
                        store, store.defineTable, {
                            name: creationTableName,
                            columnDefinitions: {
                                id: MobileServiceSqliteStore.ColumnType.Integer,
                                price: MobileServiceSqliteStore.ColumnType.Real
                            }
                        }
                    ],
                    [ store, store.upsert, crudTableName, row ],
                    [ store, store.lookup, crudTableName, row.id ],
                    function(result) {
                        $assert.areEqual(result, row);
                        return store.read(new Query(crudTableName));
                    },
                    function (result) {
                        $assert.areEqual(result, [row]);
                        return store.del(crudTableName, row.id);
                    },
                    function (result) {
                        return store.read(new Query(crudTableName));
                    },
                    {
                        success: function (result) {
                            $assert.areEqual(result, []);
                        },
                        fail: function (error) {
                            $assert.fail(error);
                        }
                    }
                ]
            );
        }
    }),
    
    $test('table definition containing a single column')
    .checkAsync(function () {
        var row = { id: 101 };

        return store.defineTable({
            name: storeTestHelper.testTableName,
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer
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

    $test('add new columns to existing table')
    .checkAsync(function () {
        var row = { id: 101, price: 51.5 },
            tableDefinition = {
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.Integer,
                    price: MobileServiceSqliteStore.ColumnType.Real
                }
            };

        return store.defineTable(tableDefinition).then(function () {
            return store.upsert(storeTestHelper.testTableName, row);
        }).then(function () {
            tableDefinition.columnDefinitions.newColumn = MobileServiceSqliteStore.ColumnType.Integer;
            return store.defineTable(tableDefinition);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, row.id);
        }).then(function (result) {
            // Expect a null value for the newly added column
            row.newColumn = null;
            $assert.areEqual(result, row);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('Redefining table with changed column types should read table data accordingly')
    .description('Sqlite does not let you alter datatype of columns. This test only checks that deserializing is performed as per latest table definition')
    .checkAsync(function () {
        var row = { id: 101, flag: 51 },
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
            // Change type of the flag column from Integer to Boolean
            tableDefinition.columnDefinitions.flag = MobileServiceSqliteStore.ColumnType.Boolean;
            return store.defineTable(tableDefinition);
        }).then(function () {
            return store.lookup(storeTestHelper.testTableName, row.id);
        }).then(function (result) {
            row.flag = true;
            $assert.areEqual(result, row);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('Redefining table with changed table and column name case should work as expected')
    .checkAsync(function () {
        var row = { id: 101, flag: 51 },
            expectedResult = {iD: 101, fLaG: 51},
            oldDefinition = {
                name: storeTestHelper.testTableName.toLowerCase(),
                columnDefinitions: {
                    iD: 'integer',
                    fLaG: 'integer'
                }
            },
            newDefinition = {
                name: storeTestHelper.testTableName.toUpperCase(),
                columnDefinitions: {
                    iD: 'integer',
                    flAG: 'integer'
                }
            };

        return testHelper.runActions([
            [store, store.defineTable, oldDefinition], 
            [store, store.upsert, oldDefinition.name, row],
            [store, store.lookup, oldDefinition.name, row.id],
            function(result) {
                $assert.areEqual(result, {iD: 101, fLaG: 51});
            },
            // Redefinition with different case
            [store, store.defineTable, newDefinition], 
            [store, store.upsert, newDefinition.name, row],
            [store, store.lookup, newDefinition.name, row.id],
            function(result) {
                $assert.areEqual(result, {iD: 101, flAG: 51});
            }
        ]);
    }),

    $test('table definition without table name')
    .checkAsync(function () {
        var tableDefinition = {
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                flag: MobileServiceSqliteStore.ColumnType.Integer
            }
        };

        return store.defineTable(tableDefinition).then(function () {
            $assert.fail('test should fail');
        }, function (error) {
        });
    }),

    $test('table definition with an invalid table name')
    .checkAsync(function () {
        var tableDefinition = {
            tableName: '*',
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                flag: MobileServiceSqliteStore.ColumnType.Integer
            }
        };

        return store.defineTable(tableDefinition).then(function () {
            $assert.fail('test should fail');
        }, function (error) {
        });
    }),

    $test('table definition with column definitions missing')
    .checkAsync(function () {
        var tableDefinition = {
            name: storeTestHelper.testTableName
        };

        return store.defineTable(tableDefinition).then(function () {
            $assert.fail('test should fail');
        }, function (error) {
        });
    }),

    $test('table definition with an invalid column name')
    .checkAsync(function () {
        var tableDefinition = {
            tableName: '*',
            columnDefinitions: {}
        };

        tableDefinition.columnDefinitions.id = MobileServiceSqliteStore.ColumnType.Integer;
        tableDefinition.columnDefinitions['*'] = MobileServiceSqliteStore.ColumnType.Integer;

        return store.defineTable(tableDefinition).then(function () {
            $assert.fail('test should fail');
        }, function (error) {
        });
    }),

    $test('table definition with primary key of type int')
    .checkAsync(function () {
        var row1 = { id: 1, str: 'str1'},
            row2 = { id: 1, str: 'str2' },
            tableDefinition = {
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.Integer,
                    str: MobileServiceSqliteStore.ColumnType.Text
                }
            };

        return store.defineTable(tableDefinition).then(function() {
            return store.upsert(storeTestHelper.testTableName, [row1, row2]);
        }).then(function() {
            return store.read(new Query(storeTestHelper.testTableName));
        }).then(function(result) {
            $assert.areEqual(result, [row2]);
        }, function(error) {
            $assert.fail(error);
        });
    }),

    $test('table definition with primary key of type real')
    .checkAsync(function () {
        var row1 = { id: 1.1, str: 'str1' },
            row2 = { id: 1.1, str: 'str2' },
            tableDefinition = {
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.Real,
                    str: MobileServiceSqliteStore.ColumnType.Text
                }
            };

        return store.defineTable(tableDefinition).then(function () {
            return store.upsert(storeTestHelper.testTableName, [row1, row2]);
        }).then(function () {
            return store.read(new Query(storeTestHelper.testTableName));
        }).then(function (result) {
            $assert.areEqual(result, [row2]);
        }, function (error) {
            $assert.fail(error);
        });
    }),

    $test('table definition with primary key of type string')
    .checkAsync(function () {
        var row1 = { id: '1', str: 'str1'},
            row2 = { id: '1', str: 'str2' },
            tableDefinition = {
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.String,
                    str: MobileServiceSqliteStore.ColumnType.Text
                }
            };

        return store.defineTable(tableDefinition).then(function() {
            return store.upsert(storeTestHelper.testTableName, [row1, row2]);
        }).then(function() {
            return store.read(new Query(storeTestHelper.testTableName));
        }).then(function(result) {
            $assert.areEqual(result, [row2]);
        }, function(error) {
            $assert.fail(error);
        });
    }),

    $test('invalid column definition')
    .checkAsync(function () {
        var tableDefinition = {
            columnDefinitions: [
                MobileServiceSqliteStore.ColumnType.Integer,
                MobileServiceSqliteStore.ColumnType.Integer
            ]
        };

        return store.defineTable(tableDefinition).then(function () {
            $assert.fail('test should fail');
        }, function (error) {
        });
    }),

    $test('id column missing from column definitions')
    .checkAsync(function () {
        var tableDefinition = {
            columnDefinitions: {
                flag: MobileServiceSqliteStore.ColumnType.Integer
            }
        };

        return store.defineTable(tableDefinition).then(function () {
            $assert.fail('test should fail');
        }, function (error) {
        });
    }),

    $test('unsupported column type in table definition')
    .checkAsync(function () {
        var tableDefinition = {
            columnDefinitions: {
                id: 'unsupportedtype',
                flag: MobileServiceSqliteStore.ColumnType.Integer
            }
        };

        return store.defineTable(tableDefinition).then(function () {
            $assert.fail('test should fail');
        }, function (error) {
        });
    }),

    $test('undefined column type in table definition')
    .checkAsync(function () {
        var tableDefinition = {
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                flag: undefined
            }
        };

        return store.defineTable(tableDefinition).then(function () {
            $assert.fail('test should fail');
        }, function (error) {
        });
    }),

    $test('null column type in table definition')
    .checkAsync(function () {
        var tableDefinition = {
            columnDefinitions: {
                id: MobileServiceSqliteStore.ColumnType.Integer,
                flag: null
            }
        };

        return store.defineTable(tableDefinition).then(function () {
            $assert.fail('test should fail');
        }, function (error) {
        });
    }),

    $test('method invoked with extra parameters')
    .checkAsync(function () {
        return store.defineTable({
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: MobileServiceSqliteStore.ColumnType.Integer,
                    flag: MobileServiceSqliteStore.ColumnType.Integer,
                    object: MobileServiceSqliteStore.ColumnType.Object
                }
            },
            'extra parameter'
        ).then(function() {
        }, function(error) {
            $assert.fail(error);
        });
    }),

    $test('invoked with no parameter')
    .checkAsync(function () {
        return store.defineTable().then(function () {
            $assert.fail('failure expected');
        }, function(error) {
        });
    })
);

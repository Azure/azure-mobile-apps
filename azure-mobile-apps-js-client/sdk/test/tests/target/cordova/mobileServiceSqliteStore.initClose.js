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
    runActions = require('../../shared/testHelper').runActions;
    
var store;

$testGroup('SQLiteStore - init / close tests').tests(

    $test('basic init')
    .checkAsync(function () {
        var store = MobileServiceSqliteStore();

        $assert.isNull(store._db);
        return runActions([
            [store, store.init],
            function() {
                $assert.isNotNull(store._db);
            }
        ]);
    }),

    $test('double init')
    .checkAsync(function () {
        var store = MobileServiceSqliteStore(),
            db;

        $assert.isNull(store._db);

        return runActions([
            [store, store.init],
            function() {
                db = store._db;
                $assert.isNotNull(store._db);
            },
            [store, store.init],
            function() {
                $assert.areEqual(store._db, db);
            }
        ]);
    }),

    $test('init error - invalid db name')
    .checkAsync(function () {
        var store = MobileServiceSqliteStore('.'), // invalid db name to force an error
            db;

        $assert.isNull(store._db);

        return runActions([
            [store, store.init],
            {
                fail: function(error) {
                    // no action needed - error expected
                }
            }
        ]);
    }),

    $test('init error - invalid `this` value')
    .checkAsync(function () {
        var store = MobileServiceSqliteStore(),
            db;

        $assert.isNull(store._db);

        return runActions([
            [undefined /* undefined this value */, store.init],
            {
                fail: function(error) {
                    // no action needed - error expected
                }
            }
        ]);
    }),

    $test('basic close')
    .checkAsync(function () {
        var store = MobileServiceSqliteStore();

        $assert.isNull(store._db);
        return runActions([
            [store, store.init],
            function() {
                $assert.isNotNull(store._db);
            },
            [store, store.close],
            function() {
                $assert.isNull(store._db);
            }
        ]);
    }),

    $test('double close')
    .checkAsync(function () {
        var store = MobileServiceSqliteStore();

        $assert.isNull(store._db);
        return runActions([
            [store, store.init],
            function() {
                $assert.isNotNull(store._db);
            },
            [store, store.close],
            function() {
                $assert.isNull(store._db);
            },
            [store, store.close],
            function() {
                $assert.isNull(store._db);
            }
        ]);
    }),

    $test('close error - invalid `this` value')
    .checkAsync(function () {
        var store = MobileServiceSqliteStore();

        $assert.isNull(store._db);
        return runActions([
            [undefined /* undefined this value */, store.close],
            {
                fail: function(error) {
                    // no action needed - failure expected
                }
            }
        ]);
    })
);

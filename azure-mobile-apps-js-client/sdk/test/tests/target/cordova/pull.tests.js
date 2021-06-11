// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file unit tests for the 'pull' module
 * 
 * The pull module has minimal unit tests and relies more on functional tests for validation
 */

var Platform = require('../../../../src/Platform'),
    Query = require('azure-query-js').Query,
    createPullManager = require('../../../../src/sync/pull').createPullManager,
    tableConstants = require('../../../../src/constants').table,
    MobileServiceClient = require('../../../../src/MobileServiceClient'),
    storeTestHelper = require('./storeTestHelper'),
    runner = require('../../../../src/Utilities/taskRunner'),
    createOperationTableManager = require('../../../../src/sync/operations').createOperationTableManager,
    MobileServiceSqliteStore = require('../../../../src/Platform/cordova/MobileServiceSqliteStore'),
    defaultPageSize = 50,
    store,
    client;
    
$testGroup('pull tests')

    // Clear the store before running each test.
    .beforeEachAsync(function() {
        return storeTestHelper.createEmptyStore().then(function(emptyStore) {
            store = emptyStore;
            client = new MobileServiceClient('http://someurl');
            return client.getSyncContext().initialize(store);
        });
    }).tests(

    $test('Valid pull settings, valid custom page size')
    .checkAsync(function () {
        return pullAndValidateSettings({pageSize: 4321}, 4321);
    }),

    $test('Valid pull settings, undefined custom page size')
    .checkAsync(function () {
        return pullAndValidateSettings({}, defaultPageSize);
    }),

    $test('Valid pull settings, null custom page size')
    .checkAsync(function () {
        return pullAndValidateSettings({pageSize: null}, defaultPageSize);
    }),

    $test('Valid pull settings, Invalid custom page size - Zero')
    .checkAsync(function () {
        return pullAndValidateSettings({pageSize: 0}, 'error');
    }),

    $test('Valid pull settings, Invalid custom page size - Negative integer')
    .checkAsync(function () {
        return pullAndValidateSettings({pageSize: -1}, 'error');
    }),

    $test('Valid pull settings, Invalid custom page size - Float')
    .checkAsync(function () {
        return pullAndValidateSettings({pageSize: 1.2}, 'error');
    }),

    $test('Valid pull settings, Invalid custom page size - String')
    .checkAsync(function () {
        return pullAndValidateSettings({pageSize: '1'}, 'error');
    }),

    $test('Valid pull settings, Invalid custom page size - Object')
    .checkAsync(function () {
        return pullAndValidateSettings({pageSize: {}}, 'error');
    }),

    $test('Invalid pull settings - string')
    .checkAsync(function () {
        return pullAndValidateSettings('abc', 'error');
    }),

    $test('Invalid pull settings - number')
    .checkAsync(function () {
        return pullAndValidateSettings(2, 'error');
    }),

    $test('Invalid pull settings - undefined')
    .checkAsync(function () {
        return pullAndValidateSettings(undefined, defaultPageSize);
    }),

    $test('Invalid pull settings - null')
    .checkAsync(function () {
        return pullAndValidateSettings(null, defaultPageSize);
    }),

    $test('Vanilla pull - verify X-ZUMO-FEATURES')
    .checkAsync(function () {
        return pullAndValidateFeatures(false /* incremental sync */);
    }),

    $test('Incremental pull - verify X-ZUMO-FEATURES')
    .checkAsync(function () {
        return pullAndValidateFeatures(true /* incremental sync */);
    })
);

function pullAndValidateSettings(settings, expectedPageSize) {
    client = client.withFilter( function(req, next, callback) {
        $assert.areEqual(req.url, "http://someurl/tables/todoitem?$filter=(updatedAt ge datetimeoffset'1969-12-31T08:00:00.000Z')&$orderby=updatedAt&$top=" + 
                        expectedPageSize + "&__includeDeleted=true");
        callback('someerror', 'response');
    });

    var pullManager = createPullManager(client, store, runner(), createOperationTableManager(store));
    return pullManager.pull(new Query(storeTestHelper.testTableName), 'queryId', settings).then(function() {
        $assert.fail('failure expected');
    }, function(error) {
        if (expectedPageSize === 'error') {
            // NOP
        } else {
            $assert.areEqual(error.message, 'someerror');
        }
    });
}

function pullAndValidateFeatures(incrementalSync) {
    var filterInvoked = false,
        offlineSyncFeatureAdded = false,
        incrementalSyncFeatureAdded = false;

    client = client.withFilter( function(req, next, callback) {
        var features = req.headers['X-ZUMO-FEATURES'];
        offlineSyncFeatureAdded = features && features.indexOf('OL') > -1;
        incrementalSyncFeatureAdded = features && features.indexOf('IP') > -1;
        filterInvoked = true; 
        callback('someerror');
    });

    var pullManager = createPullManager(client, store, runner(), createOperationTableManager(store));
    var queryId = incrementalSync ? 'queryId' : null;
    return pullManager.pull(new Query(storeTestHelper.testTableName), queryId).then(function() {
        $assert.fail('failure expected');
    }, function(error) {
        $assert.isTrue(filterInvoked);
         
        // expect the relevant offline features to be added to the X-ZUMO-FEATURES header
        $assert.isTrue(offlineSyncFeatureAdded);
        $assert.isTrue(incrementalSyncFeatureAdded === incrementalSync);
    }).then(function() {
        return client.getTable(storeTestHelper.testTableName).insert({id: '1'});
    }).then(function() {
        $assert.fail('failure expected');
    }, function(error) {
        $assert.isTrue(filterInvoked);

        // expect no offline features to be added to the X-ZUMO-FEATURES header
        $assert.isFalse(offlineSyncFeatureAdded);
        $assert.isFalse(incrementalSyncFeatureAdded);
    });
}

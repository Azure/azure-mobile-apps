// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file MobileServiceSyncTable unit tests
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
    table,
    client;

$testGroup('MobileServiceSyncTable tests')

    // Clear the local store before running each test.
    .beforeEachAsync(function() {
        return storeTestHelper.createEmptyStore().then(function(emptyStore) {
            store = emptyStore;
        }).then(function() {
            return store.defineTable({
                name: storeTestHelper.testTableName,
                columnDefinitions: {
                    id: 'string',
                    text: 'string'
                }
            });
        }).then(function() {
            client = new MobileServiceClient('someurl');
            return client.getSyncContext().initialize(store);
        }).then(function() {
            table = client.getSyncTable(storeTestHelper.testTableName);
        });
    }).tests(
        
    $test('table.where operator')
    .checkAsync(function () {
        var record1 = { id: '1', text: 'abc' },
            record2 = { id: '2', text: 'def' },
            record3 = { id: '3', text: 'def' },
            record4 = { id: '4', text: 'abc' };
            
        return store.upsert(storeTestHelper.testTableName, [record1, record2, record3, record4]).then(function() {
            return table
                    .where({text: 'abc'})
                    .orderByDescending('id')
                    .read();
        }).then(function(result) {
            $assert.isNotNull(result);
            $assert.areEqual(result.length, 2);
            $assert.areEqual(result[0], record4);
            $assert.areEqual(result[1], record1);
        });
    }),
    
    $test('insert')
    .description('Verifies that insert acts as a passthrough function')
    .check(function () {
        
        var testRecord = {somekey: 'some value'},
            testResult = 'operation result';
        
        client.getSyncContext().insert = function(tableName, record) {
            $assert.areEqual(tableName, table.getTableName());
            $assert.areEqual(record, testRecord);
            $assert.areEqual(arguments.length, 2);
            return testResult;
        };

        $assert.areEqual(table.insert(testRecord, 'extra param'), testResult);
    }),
    
    $test('lookup')
    .description('Verifies that lookup acts as a passthrough function')
    .check(function () {
        
        var testId = 4321,
            testResult = 'operation result',
            testSuppressRecordNotFoundError = 'suppressRecordNotFoundError';
        
        client.getSyncContext().lookup = function(tableName, id, suppressRecordNotFoundError) {
            $assert.areEqual(tableName, table.getTableName());
            $assert.areEqual(id, testId);
            $assert.areEqual(suppressRecordNotFoundError, testSuppressRecordNotFoundError);
            $assert.areEqual(arguments.length, 3);
            return testResult;
        };

        $assert.areEqual(table.lookup(testId, testSuppressRecordNotFoundError), testResult);
    }),
    
    $test('read')
    .description('Verifies that read acts as a passthrough function')
    .check(function () {
        
        var testQuery = {somekey: 'somevalue'},
            testResult = 'operation result';
        
        client.getSyncContext().read = function(query) {
            $assert.areEqual(query, testQuery);
            $assert.areEqual(arguments.length, 1);
            return testResult;
        };

        $assert.areEqual(table.read(testQuery), testResult);
    }),
    
    $test('update')
    .description('Verifies that update acts as a passthrough function')
    .check(function () {
        
        var testRecord = {somekey: 'some value'},
            testResult = 'operation result';
        
        client.getSyncContext().update = function(tableName, record) {
            $assert.areEqual(tableName, table.getTableName());
            $assert.areEqual(record, testRecord);
            $assert.areEqual(arguments.length, 2);
            return testResult;
        };

        $assert.areEqual(table.update(testRecord), testResult);
    }),
    
    $test('del')
    .description('Verifies that del acts as a passthrough function')
    .check(function () {
        
        var testRecord = {somekey: 'some value'},
            testResult = 'operation result';
        
        client.getSyncContext().del = function(tableName, record) {
            $assert.areEqual(tableName, table.getTableName());
            $assert.areEqual(record, testRecord);
            $assert.areEqual(arguments.length, 2);
            return testResult;
        };

        $assert.areEqual(table.del(testRecord), testResult);
    }),
    
    $test('pull - query specified')
    .description('Tests that the pull API simply calls MobileServiceSyncContext.pull() and returns whatever it returns')
    .check(function () {

        // The pull params defined below have invalid values, but that does not matter
        // as all we want to test is that pull acts as a passthrough function.
        var query = 'query',
            queryId = 'queryId',
            settings = 'settings',
            result = 'result';

        client.getSyncContext().pull = function(queryParam, queryIdParam, settingsParam) {
            $assert.areEqual(queryParam, query);
            $assert.areEqual(queryIdParam, queryId);
            $assert.areEqual(settingsParam, settings);
            $assert.areEqual(arguments.length, 3);
            return result;
        };

        $assert.areEqual(table.pull(query, queryId, settings, 'fourth_param_just_in_case_pull_starts_taking_more_params_in_the_future'), result);
    }),

    $test('pull - query not specified')
    .description('Tests that the pull API simply calls MobileServiceSyncContext.pull() and returns whatever it returns')
    .check(function () {

        // The pull params defined below have invalid values, but that does not matter
        // as all we want to test is that pull acts as a passthrough function.
        var queryId = 'queryId',
            settings = 'settings',
            result = 'result';

        client.getSyncContext().pull = function(queryParam, queryIdParam, settingsParam) {
            $assert.isNotNull(queryParam);
            $assert.areEqual(queryParam.getComponents().table, storeTestHelper.testTableName);
            $assert.areEqual(queryIdParam, queryId);
            $assert.areEqual(settingsParam, settings);
            $assert.areEqual(arguments.length, 3);
            return result;
        };

        $assert.areEqual(table.pull(null, queryId, settings, 'fourth_param_just_in_case_pull_starts_taking_more_params_in_the_future'), result);
    }),

    $test('purge - query specified')
    .description('Tests that the purge API simply calls MobileServiceSyncContext.purge() and returns whatever it returns')
    .check(function () {

        client.getSyncContext().purge = function (query, forcePurge) {
            $assert.areEqual(query, {dummykey: 'dummyvalue'});
            $assert.areEqual(forcePurge, true);
            return 'result';
        };

        // The purge params have invalid values, but that does not matter
        // as all we want to test is that purge acts as a passthrough function.
        $assert.areEqual(table.purge({dummykey: 'dummyvalue'}, true), 'result');
    }),

    $test('purge - query specified')
    .description('Tests that the purge API simply calls MobileServiceSyncContext.purge() and returns whatever it returns')
    .check(function () {

        client.getSyncContext().purge = function (query, forcePurge) {
            $assert.isNotNull(query);
            $assert.areEqual(query.getComponents().table, storeTestHelper.testTableName);
            $assert.areEqual(forcePurge, true);
            return 'result';
        };

        $assert.areEqual(table.purge(null, true), 'result');
    })
);

function getArgs() {
    return [1, 2, 3, 4];
}
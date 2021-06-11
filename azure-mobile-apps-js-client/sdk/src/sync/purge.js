// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file Table purge logic implementation
 * @private
 */

var Validate = require('../Utilities/Validate'),
    Query = require('azure-query-js').Query,
    Platform = require('../Platform'),
    taskRunner = require('../Utilities/taskRunner'),
    MobileServiceTable = require('../MobileServiceTable'),
    tableConstants = require('../constants').table,
    _ = require('../Utilities/Extensions');
    
function createPurgeManager(store, storeTaskRunner) {

    return {
        purge: purge
    };

    /**
     * Purges data, pending operations and incremental sync state associated with a local table
     * A regular purge, would fail if there are any pending operations for the table being purged.
     * A forced purge will proceed even if pending operations for the table being purged exist in the operation table. In addition,
     * it will also delete the table's pending operations.
     * 
     * @param query Query object that specifies what records are to be purged
     * @param [forcePurge] An optional boolean, which if set to true, will perform a forced purge.
     * 
     * @returns A promise that is fulfilled when purge is complete OR is rejected if it fails.  
     */
    function purge(query, forcePurge) {
        return storeTaskRunner.run(function() {
            Validate.isObject(query, 'query');
            Validate.notNull(query, 'query');
            if (!_.isNull(forcePurge)) {
                Validate.isBool(forcePurge, 'forcePurge');
            }

            // Query for pending operations associated with this table
            var operationQuery = new Query(tableConstants.operationTableName)
                .where(function(tableName) {
                    return this.tableName === tableName; 
                }, query.getComponents().table);
            
            // Query to search for the incremental sync state associated with this table
            var incrementalSyncStateQuery = new Query(tableConstants.pulltimeTableName)
                .where(function(tableName) {
                    return this.tableName === tableName;
                }, query.getComponents().table);

            // 1. In case of force purge, simply remove operation table entries for the table being purged
            //    Else, ensure no records exists in the operation table for the table being purged.
            // 2. Delete pull state for all incremental queries associated with this table
            // 3. Delete the records from the table as specified by 'query'
            // 
            // TODO: All store operations performed while purging should be part of a single transaction
            // Note: An incremental pull after a purge should fetch purged records again. If we run 3 before 2,
            // we might end up in a state where 3 is complete but 2 has failed. In such a case subsequent incremental pull
            // will not re-fetch purged records. Hence, it is important to run 2 before 3.
            // There still exists a possibility of pending operations being deleted while force purging and the subsequent
            // operations failing which is tracked by the above TODO. 
            return Platform.async(function(callback) {
                callback();
            })().then(function() {
                if (forcePurge) {
                    return store.del(operationQuery);
                } else {
                    return store.read(operationQuery).then(function(operations) {
                        if (operations.length > 0) {
                            throw new Error('Cannot purge the table as it contains pending operations');
                        }
                    });
                }
            }).then(function() {
                return store.del(incrementalSyncStateQuery);
            }).then(function() {
                return store.del(query);
            });
        });
    }
}

exports.createPurgeManager = createPurgeManager;

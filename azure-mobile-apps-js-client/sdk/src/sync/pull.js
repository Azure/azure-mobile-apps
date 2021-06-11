// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file Table pull logic implementation
 * @private
 */

var Validate = require('../Utilities/Validate'),
    Query = require('azure-query-js').Query,
    Platform = require('../Platform'),
    taskRunner = require('../Utilities/taskRunner'),
    MobileServiceTable = require('../MobileServiceTable'),
    constants = require('../constants'),
    tableConstants = constants.table,
    _ = require('../Utilities/Extensions');
    
var defaultPageSize = 50,
    idPropertyName = tableConstants.idPropertyName,
    pulltimeTableName = tableConstants.pulltimeTableName,
    sysProps = tableConstants.sysProps;
    
function createPullManager(client, store, storeTaskRunner, operationTableManager) {
    // Task runner for running pull tasks. We want only one pull to run at a time. 
    var pullTaskRunner = taskRunner(),
        mobileServiceTable,
        pageSize,
        lastKnownUpdatedAt, // get the largest known value of the updatedAt column 
        tablePullQuery, // the query specified by the user for pulling the table 
        pagePullQuery, // query for fetching a single page
        pullQueryId; // the query ID. if this is a non-null string, the pull will be performed incrementally.
    
    return {
        initialize: initialize,
        pull: pull
    };

    /**
     * Creates and initializes the table used to store the state for performing incremental pull
     */
    function initialize () {
        return pullTaskRunner.run(function() {
            return store.defineTable({
                name: pulltimeTableName,
                columnDefinitions: {
                    id: 'string', // column for storing queryId
                    tableName: 'string', // column for storing table name 
                    value: 'date' // column for storing lastKnownUpdatedAt
                }
            });
        });
    }
    
    /**
     * Pulls changes from the server tables into the local store.
     * 
     * @param query Query specifying which records to pull
     * @param pullQueryId A unique string ID for an incremental pull query OR null for a vanilla pull query.
     * @param [settings] An object that defines the various pull settings - currently only pageSize
     * 
     * @returns A promise that is fulfilled when all records are pulled OR is rejected if the pull fails or is cancelled.  
     */
    function pull(query, queryId, settings) {
        //TODO: support pullQueryId
        //TODO: page size should be configurable
        
        return pullTaskRunner.run(function() {
            validateQuery(query, 'query');
            Validate.isString(queryId, 'queryId'); // non-null string or null - both are valid
            Validate.isObject(settings, 'settings');

            settings = settings || {};
            if (_.isNull(settings.pageSize)) {
                pageSize = defaultPageSize;
            } else if (_.isInteger(settings.pageSize) && settings.pageSize > 0) {
                pageSize = settings.pageSize;
            } else {
                throw new Error('Page size must be a positive integer. Page size ' + settings.pageSize + ' is invalid.');
            }

            // Make a copy of the query as we will be modifying it
            tablePullQuery = copyQuery(query);            

            mobileServiceTable = client.getTable(tablePullQuery.getComponents().table);
            mobileServiceTable._features = queryId ? [constants.features.OfflineSync, constants.features.IncrementalPull] : [constants.features.OfflineSync];
            pullQueryId = queryId;

            // Set up the query for initiating a pull and then pull all pages          
            return setupQuery().then(function() {
                return pullAllPages();
            });
        });
    }

    // Setup the query to get started with pull
    function setupQuery() {
        return getLastKnownUpdatedAt().then(function(updatedAt) {
            buildQueryFromLastKnownUpdateAt(updatedAt);
        });
    }

    // Pulls all pages from the server table, one page at a time.
    function pullAllPages() {
        // 1. Pull one page
        // 2. Check if Pull is complete
        // 3. If it is complete, go to 5. If not, update the query to fetch the next page.
        // 4. Go to 1
        // 5. DONE
        return pullPage().then(function(pulledRecords) {
            if (!isPullComplete(pulledRecords)) {
                // update query and continue pulling the remaining pages
                return updateQueryForNextPage(pulledRecords).then(function() {
                    return pullAllPages();
                });
            }
        });
    }
    
    // Check if the pull is complete or if there are more records left to be pulled
    function isPullComplete(pulledRecords) {
         // Pull is NOT complete when the number of fetched records is less than page size as the server's page size
         // can cause the result set to be smaller than the requested page size.
         // We consider the pull to be complete only when the result contains 0 records.
        return pulledRecords.length === 0;
    }
    
    // Pull the page as described by the query
    function pullPage() {

        // Define appropriate parameter to enable fetching of deleted records from the server.
        // Assumption is that soft delete is enabled on the server table.
        var params = {};
        params[tableConstants.includeDeletedFlag] = true;

        var pulledRecords;
        
        // azure-query-js does not support datatimeoffset
        // As a temporary workaround, convert the query to an odata string and replace datetime' with datetimeoffset'. 
        var queryString = pagePullQuery.toOData();
        var tableName = pagePullQuery.getComponents().table;
        queryString = queryString.replace(new RegExp('^/' + tableName), '').replace("datetime'", "datetimeoffset'");

        return mobileServiceTable.read(queryString, params).then(function(result) {
            pulledRecords = result || [];

            var chain = Platform.async(function(callback) {
                callback();
            })();
            
            // Process all records in the page
            for (var i = 0; i < pulledRecords.length; i++) {
                chain = processPulledRecord(chain, tableName, pulledRecords[i]); 
            }

            return chain;
        }).then(function(pulled) {
            return onPagePulled();
        }).then(function() {
            return pulledRecords;
        });
    }

    // Processes the pulled record by taking an appropriate action, which can be one of:
    // inserting, updating, deleting in the local store or no action at all.
    function processPulledRecord(chain, tableName, pulledRecord) {
        return chain.then(function() {

            // Update the store as per the pulled record 
            return storeTaskRunner.run(function() {
                if (Validate.isValidId(pulledRecord[idPropertyName])) {
                    throw new Error('Pulled record does not have a valid ID');
                }
                
                return operationTableManager.readPendingOperations(tableName, pulledRecord[idPropertyName]).then(function(pendingOperations) {
                    // If there are pending operations for the record we just pulled, we ignore it.
                    if (pendingOperations.length > 0) {
                        return;
                    }

                    if (pulledRecord[sysProps.deletedColumnName] === true) {
                        return store.del(tableName, pulledRecord.id);
                    } else if (pulledRecord[sysProps.deletedColumnName] === false) {
                        return store.upsert(tableName, pulledRecord);
                    } else {
                        throw new Error("'" + sysProps.deletedColumnName + "' system property is missing. Pull cannot work without it.'");
                    }
                });
            });
        });
    }

    // Gets the last known updatedAt timestamp.
    // For incremental pull, we check if we have any information about it in the store.
    // If not we simply use 1970 to start the sync operation, just like a non-incremental / vanilla pull.
    function getLastKnownUpdatedAt() {
        
        return Platform.async(function(callback) {
            callback();
        })().then(function() {
            
            if (pullQueryId) { // read lastKnownUpdatedAt from the store
                return store.lookup(pulltimeTableName, pullQueryId, true /* suppressRecordNotFoundError */);
            }

        }).then(function(result) {

            if (result) {
                return result.value;
            }

            return new Date (1970, 0, 0);
        });
    }

    // update the query to pull the next page
    function updateQueryForNextPage(pulledRecords) {
        return Platform.async(function(callback) {
            callback();
        })().then(function() {

            if (!pulledRecords) {
                throw new Error('Something is wrong. pulledRecords cannot be null at this point');
            }

            var lastRecord = pulledRecords[ pulledRecords.length - 1];

            if (!lastRecord) {
                throw new Error('Something is wrong. Possibly invalid response from the server. lastRecord cannot be null!');
            }

            var lastRecordTime = lastRecord[tableConstants.sysProps.updatedAtColumnName];

            if (!_.isDate(lastRecordTime)) {
                throw new Error('Property ' + tableConstants.sysProps.updatedAtColumnName + ' of the last record should be a valid date');
            }

            if (lastRecordTime.getTime() === lastKnownUpdatedAt.getTime()) {
                pagePullQuery.skip(pagePullQuery.getComponents().skip + pulledRecords.length);
            } else {
                buildQueryFromLastKnownUpdateAt(lastRecordTime);
            }
        });
    }

    // Builds a query to fetch one page
    // Records with updatedAt values >= updatedAt will be fetched 
    function buildQueryFromLastKnownUpdateAt(updatedAt) {

        lastKnownUpdatedAt = updatedAt;

        // Make a copy of the table query and tweak it to fetch the next first page
        pagePullQuery = copyQuery(tablePullQuery);
        pagePullQuery = pagePullQuery.where(function(lastKnownUpdatedAt) {
            // Ideally we would have liked to set this[tableConstants.sysProps.updatedAtColumnName]
            // but this isn't supported
            return this.updatedAt >= lastKnownUpdatedAt;
        }, lastKnownUpdatedAt);

        pagePullQuery.orderBy(tableConstants.sysProps.updatedAtColumnName);
        pagePullQuery.take(pageSize);
    }

    // Called after a page is pulled and processed
    function onPagePulled() {

        // For incremental pull, make a note of the lastKnownUpdatedAt in the store
        if (pullQueryId) {
            return store.upsert(pulltimeTableName, {
                id: pullQueryId,
                tableName: pagePullQuery.getComponents().table,
                value: lastKnownUpdatedAt
            });
        }
    }

    // Not all query operations are allowed while pulling.
    // This function validates that the query does not perform unsupported operations.
    function validateQuery(query) {
        Validate.isObject(query);
        Validate.notNull(query);
        
        var components = query.getComponents();
        
        for (var i in components.ordering) {
            throw new Error('orderBy and orderByDescending clauses are not supported in the pull query');
        }
        
        if (components.skip) {
            throw new Error('skip is not supported in the pull query');
        }

        if (components.take) {
            throw new Error('take is not supported in the pull query');
        }

        if (components.selections && components.selections.length !== 0) {
            throw new Error('select is not supported in the pull query');
        }

        if (components.includeTotalCount) {
            throw new Error('includeTotalCount is not supported in the pull query');
        }
    }

    // Makes a copy of the QueryJS object
    function copyQuery(query) {
        var components = query.getComponents();
        var queryCopy = new Query(components.table);
        queryCopy.setComponents(components);

        return queryCopy;
    }
}

exports.createPullManager = createPullManager;

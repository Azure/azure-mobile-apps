// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file Implements operation table management functions like defining the operation table,
 * adding log operations to the operation table, condensing operations, etc
 * @private
 */

var Validate = require('../Utilities/Validate'),
    Platform = require('../Platform'),
    ColumnType = require('./ColumnType'),
    taskRunner = require('../Utilities/taskRunner'),
    tableConstants = require('../constants').table,
    _ = require('../Utilities/Extensions'),
    Query = require('azure-query-js').Query;

var idPropertyName = tableConstants.idPropertyName,
    versionColumnName = tableConstants.sysProps.versionColumnName,
    operationTableName = tableConstants.operationTableName;
    
function createOperationTableManager(store) {

    Validate.isObject(store);
    Validate.notNull(store);

    var runner = taskRunner(),
        isInitialized,
        maxOperationId = 0,
        lockedOperationId,
        maxId;

    var api = {
        initialize: initialize,
        lockOperation: lockOperation,
        unlockOperation: unlockOperation,
        readPendingOperations: readPendingOperations,
        readFirstPendingOperationWithData: readFirstPendingOperationWithData,
        removeLockedOperation: removeLockedOperation,
        getLoggingOperation: getLoggingOperation,
        getMetadata: getMetadata
    };

    // Exports for testing purposes only
    api._getOperationForInsertingLog = getOperationForInsertingLog;
    api._getOperationForUpdatingLog = getOperationForUpdatingLog;

    return api;
    
    /**
     * Defines the operation table in the local store.
     * Schema of the operation table is: [ INT id | TEXT tableName | TEXT action | TEXT itemId ]
     * If the table already exists, it will have no effect.
     * @param localStore The local store to create the operation table in.
     * @returns A promise that is resolved when initialization is complete and rejected if it fails.
     */
    function initialize () {
        return store.defineTable({
            name: operationTableName,
            columnDefinitions: {
                id: ColumnType.Integer,
                tableName: ColumnType.String,
                action: ColumnType.String,
                itemId: ColumnType.String,
                metadata: ColumnType.Object 
            }
        }).then(function() {
            return getMaxOperationId();
        }).then(function(id) {
            maxId = id;
            isInitialized = true;
        });
    }
    
    /**
     * Locks the operation with the specified id.
     * 
     * TODO: Lock state and the value of the locked operation should be persisted.
     * That way we can handle the following scenario: insert -> initiate push -> connection failure after item inserted in server table
     * -> client crashes or cancels push -> client app starts again -> delete -> condense. 
     * In the above scenario if we condense insert and delete into nothing, we end up not deleting the item we sent to server.
     * And if we do not condense, insert will have no corresponding data in the table to send to the server while pushing as 
     * the record would have been deleted.
     */
    function lockOperation(id) {
        return runner.run(function() {
            // Locking a locked operation should have no effect
            if (lockedOperationId === id) {
                return;
            }
            
            if (!lockedOperationId) {
                lockedOperationId = id;
                return;
            }

            throw new Error('Only one operation can be locked at a time');
        });
    }
    
    /**
     * Unlock the locked operation
     */
    function unlockOperation() {
        return runner.run(function() {
            lockedOperationId = undefined;
        });
    }
    
    /**
     * Given an operation that will be performed on the store, this method returns a corresponding operation for recording it in the operation table.
     * The logging operation can add a new record, edit an earlier record or remove an earlier record from the operation table.
     * 
     * @param tableName Name of the table on which the action is performed
     * @param action Action performed on the table. Valid actions are 'insert', 'update' or 'delete'
     * @param item Record that is being inserted, updated or deleted. In case of 'delete', all properties other than id will be ignored.
     * 
     * @returns Promise that is resolved with the logging operation. In case of a failure the promise is rejected.
     */
    function getLoggingOperation(tableName, action, item) {
        
        // Run as a single task to avoid task interleaving.
        return runner.run(function() {
            Validate.notNull(tableName);
            Validate.isString(tableName);
            
            Validate.notNull(action);
            Validate.isString(action);
            
            Validate.notNull(item);
            Validate.isObject(item);
            Validate.isValidId(item[idPropertyName]);

            if (!isInitialized) {
                throw new Error('Operation table manager is not initialized');
            }
            
            return readPendingOperations(tableName, item[idPropertyName]).then(function(pendingOperations) {
                
                // Multiple operations can be pending for <tableName, itemId> due to an opertion being locked in the past.
                // Get the last pending operation
                var pendingOperation = pendingOperations.pop(),
                    condenseAction;
                
                // If the operation table has a pending operation, we attempt to condense the new action into the pending operation.
                // If not, we simply add a new operation.
                if (pendingOperation) {
                    condenseAction = getCondenseAction(pendingOperation, action);
                } else {
                    condenseAction = 'add';
                }

                if (condenseAction === 'add') { // Add a new operation
                    return getOperationForInsertingLog(tableName, action, item);
                } else if (condenseAction === 'modify') { // Edit the pending operation's action to be the new action.
                    return getOperationForUpdatingLog(pendingOperation.id, tableName, action /* new action */, item);
                } else if (condenseAction === 'remove') { // Remove the earlier log from the operation table
                    return getOperationForDeletingLog(pendingOperation.id);
                } else if (condenseAction === 'nop') { // NO OP. Nothing to be logged
                    return; 
                } else  { // Error
                    throw new Error('Unknown condenseAction: ' + condenseAction);
                }
            });
        });
    }
    
    /**
     * Reads the pending operations for the specified table and item / record ID from the operation table.
     * @param tableName Name of the table whose operations we are looking for
     * @param itemId ID of the record whose operations we are looking for 
     */
    function readPendingOperations(tableName, itemId) {
        return Platform.async(function(callback) {
            callback();
        })().then(function() {
            var query = new Query(operationTableName);
            return store.read(query.where(function (tableName, itemId) {
                return this.tableName === tableName && this.itemId === itemId;
            }, tableName, itemId).orderBy('id'));
        });
    }
    
    /**
     * Gets the first / oldest pending operation, i.e. the one with smallest id value
     * 
     * @returns Object containing logRecord (record from the operation table) and an optional data record (i.e. record associated with logRecord).
     * The data record will be present only for insert and update operations.
     */
    function readFirstPendingOperationWithData(lastProcessedOperationId) {
        return runner.run(function() {
            return readFirstPendingOperationWithDataInternal(lastProcessedOperationId);
        });
    }

    /**
     * Removes the operation that is currently locked
     * 
     * @returns A promise that is fulfilled when the locked operation is unlocked.
     * If no operation is currently locked, the promise is rejected.
     */
    function removeLockedOperation() {
        return removePendingOperation(lockedOperationId).then(function() {
            return unlockOperation();
        });
    }
    

    // Checks if the specified operation is locked
    function isLocked(operation) {
        return operation && operation.id === lockedOperationId;
    }

    function readFirstPendingOperationWithDataInternal(lastProcessedOperationId) {
        var logRecord, // the record logged in the operation table
            query = new Query(operationTableName).where(function(lastProcessedOperationId) {
                        return this.id > lastProcessedOperationId;
                    }, lastProcessedOperationId).orderBy('id').take(1);
        
        // Read record from operation table with the smallest ID
        return store.read(query).then(function(result) {
            if (result.length === 1) {
                logRecord = result[0];
            } else if (result.length === 0) { // no pending records
                return;
            } else {
                throw new Error('Something is wrong!');
            }
        }).then(function() {
            if (!logRecord) { // no pending records
                return;
            }
            
            if (logRecord.action === 'delete') {
                return {
                    logRecord: logRecord
                };
            }
            
            // Find the data record associated with the log record. 
            return store.lookup(logRecord.tableName, logRecord.itemId, true /* suppressRecordNotFoundError */).then(function(data) {
                if (data) { // Return the log record and the data record.
                    return {
                        logRecord: logRecord,
                        data: data
                    };
                }
                
                // It is possible that a log record corresponding to an insert / update operation has no corresponding
                // data record. 
                // 
                // This can happen in the following scenario:
                // insert -> push / lock operation begins -> delete -> push fails
                //  
                // In such a case, we remove the log operation from the operation table and proceed to the next log operation.
                return removePendingOperationInternal(logRecord.id).then(function() {
                    lastProcessedOperationId = logRecord.id;
                    return readFirstPendingOperationWithDataInternal(lastProcessedOperationId);
                });
            });
        });
    }
    
    function removePendingOperation(id) {
        return runner.run(function() {
            return removePendingOperationInternal(id);
        });
    }

    function removePendingOperationInternal(id) {
        return Platform.async(function(callback) {
            callback();
        })().then(function() {
            if (!id) {
                throw new Error('Invalid operation id');
            }
            return store.del(operationTableName, id);
        });
    }

    /**
     * Determines how to condense the new action into the pending operation
     * @returns 'nop' - if no action is needed
     *          'remove' - if the pending operation should be removed
     *          'modify' - if the pending action should be modified to be the new action
     *          'add' - if a new operation should be added
     */
    function getCondenseAction(pendingOperation, newAction) {
        
        var pendingAction = pendingOperation.action,
            condenseAction;
        if (pendingAction === 'insert' && newAction === 'update') {
            condenseAction = 'nop';
        } else if (pendingAction === 'insert' && newAction === 'delete') {
            condenseAction = 'remove';
        } else if (pendingAction === 'update' && newAction === 'update') {
            condenseAction = 'nop';
        } else if (pendingAction === 'update' && newAction === 'delete') {
            condenseAction = 'modify';
        } else if (pendingAction === 'delete' && newAction === 'delete') {
            condenseAction = 'nop';
        } else if (pendingAction === 'delete') {
            throw new Error('Operation ' + newAction + ' not supported as a DELETE operation is pending'); //TODO: Limitation on all client SDKs
        } else {
            throw new Error('Condense not supported when pending action is ' + pendingAction + ' and new action is ' + newAction);
        }
        
        if (isLocked(pendingOperation)) {
            condenseAction = 'add';
        }
        
        return condenseAction;
    }
    
    /**
     * Gets the operation that will insert a new record in the operation table.
     */
    function getOperationForInsertingLog(tableName, action, item) {
        return api.getMetadata(tableName, action, item).then(function(metadata) {
            return {
                tableName: operationTableName,
                action: 'upsert',
                data: {
                    id: ++maxId,
                    tableName: tableName,
                    action: action,
                    itemId: item[idPropertyName],
                    metadata: metadata
                }
            };
        });
    }
    
    /**
     * Gets the operation that will update an existing record in the operation table.
     */
    function getOperationForUpdatingLog(operationId, tableName, action, item) {
        return api.getMetadata(tableName, action, item).then(function(metadata) {
            return {
                tableName: operationTableName,
                action: 'upsert',
                data: {
                    id: operationId,
                    action: action,
                    metadata: metadata
                }
            };
        });
    }
    
    /**
     * Gets an operation that will delete a record from the operation table.
     */
    function getOperationForDeletingLog(operationId) {
        return {
            tableName: operationTableName,
            action: 'delete',
            id: operationId
        };
    }

    /**
     * Gets the metadata to associate with a log record in the operation table
     * 
     * @param action 'insert', 'update' and 'delete' correspond to the insert, update and delete operations.
     *               'upsert' is a special action that is used only in the context of conflict handling.
     */
    function getMetadata(tableName, action, item) {
        
        return Platform.async(function(callback) {
            callback();
        })().then(function() {
            var metadata = {};

            // If action is update and item defines version property OR if action is insert / update,
            // define metadata.version to be the item's version property
            if (action === 'upsert' || 
                action === 'insert' ||
                (action === 'update' && item.hasOwnProperty(versionColumnName))) {
                metadata[versionColumnName] = item[versionColumnName];
                return metadata;
            } else if (action == 'update' || action === 'delete') { // Read item's version property from the table
                return store.lookup(tableName, item[idPropertyName], true /* suppressRecordNotFoundError */).then(function(result) {
                    if (result) {
                        metadata[versionColumnName] = result[versionColumnName];
                    }
                    return metadata;
                });
            } else {
                throw new Error('Invalid action ' + action);
            }
        });
        
    }

    /**
     * Gets the largest operation ID from the operation table
     * If there are no records in the operation table, returns 0.
     */
    function getMaxOperationId() {
        var query = new Query(operationTableName);
        return store.read(query.orderByDescending('id').take(1)).then(function(result) {
            Validate.isArray(result);
            
            if (result.length === 0) {
                return 0;
            } else if (result.length === 1) {
                return result[0].id;
            } else {
                throw new Error('something is wrong!');
            }
        });
    }
}

module.exports = {
    createOperationTableManager: createOperationTableManager
};

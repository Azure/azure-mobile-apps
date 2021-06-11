// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file Table push error handling implementation. Defines various methods for resolving conflicts
 * @private
 */

var Platform = require('../Platform'),
    _ = require('../Utilities/Extensions'),
    tableConstants = require('../constants').table;
    
var operationTableName = tableConstants.operationTableName,
    deletedColumnName = tableConstants.sysProps.deletedColumnName;

/**
 * Creates a pushError object that wraps the low level error encountered while pushing
 * and adds other useful methods for error handling.
 * @private
 */
function createPushError(store, operationTableManager, storeTaskRunner, pushOperation, operationError) {

    // Calling getError will return the operationError object to the caller without cloning it.
    // (As operationError can have loops, our simplistic makeCopy(..) method won't be able to clone it)
    //
    // To guard ourselves from possible modifications to the operationError object by the caller,
    // we make a copy of its members we may need later.
    var serverRecord = makeCopy(operationError.serverInstance),
        statusCode = makeCopy(operationError.request.status);
    
    /**
     * @class PushError
     * @classdesc A conflict / error encountered while pushing a change to the server. This wraps the underlying error
     * and provides additional helper methods for handling the conflict / error. 
     * @protected
     */
    return {
        /**
         * @member {boolean} isHandled When set to true, the push logic will consider the conflict / error to be handled.
         *                             In such a case, an attempt would be made to push the change to the server again.
         *                             By default, `isHandled` is set to false.
         * @instance
         * @memberof PushError
         */
        isHandled: false,

        getError: getError,
        
        // Helper methods
        isConflict: isConflict,
        
        // Data query methods
        getTableName: getTableName,
        getAction: getAction,
        getServerRecord: getServerRecord,
        getClientRecord: getClientRecord,

        // Error handling methods
        cancelAndUpdate: cancelAndUpdate,
        cancelAndDiscard: cancelAndDiscard,
        cancel: cancel,
        update: update,
        changeAction: changeAction
    };

    /**
     * Gets the name of the table for which conflict / error occurred.
     * 
     * @function
     * @instance
     * @memberof PushError
     * 
     * @returns {string} The name of the table for which conflict / error occurred. 
     */
    function getTableName() {
        return makeCopy(pushOperation.logRecord.tableName);
    }
    
    /**
     * Gets the action for which conflict / error occurred.
     * 
     * @function
     * @instance
     * @memberof PushError 
     * 
     * @returns {string} The action for which conflict / error occurred. Valid action values
     *                   are _'insert'_, _'update'_ or _'delete'_.
     */
    function getAction() {
        return makeCopy(pushOperation.logRecord.action);
    }
    
    /**
     * Gets the value of the record on the server, if available, when the conflict / error occurred.
     * This is useful while handling conflicts. However, **note** that in the event of
     * a conflict / error, the server may not always respond with the server record's value.
     * Example: If the push failed even before the client value reaches the server, we won't have the server value.
     * Also, there are some scenarios where the server does not respond with the server value.
     * 
     * @function
     * @instance
     * @memberof PushError
     * 
     * @returns {object} Server record value.
     */
    function getServerRecord() {
        return makeCopy(serverRecord);
    }
    
    /**
     * Gets the value of the record that was pushed to the server when the conflict /error occurred.
     * Note that this may not be the latest value as local tables could have changed after we
     * started the push operation. 
     * 
     * @function
     * @instance
     * @memberof PushError
     * 
     * @returns {object} Client record value.
     */
    function getClientRecord() {
        return makeCopy(pushOperation.data);
    }
    
    /**
     * Gets the underlying error encountered while performing the push operation. This contains
     * grannular details of the failure like server response, error code, etc.
     * 
     * Note: Modifying value returned by this method will have a side effect of permanently
     * changing the underlying error object
     * 
     * @function
     * @instance
     * @memberof PushError
     * @returns {Error} The underlying error object.
     */
    function getError() {
        // As operationError can have loops, our simplistic makeCopy(..) method won't be able to clone it. 
        // Return without cloning.
        return operationError;
    }
    
    /**
     * Checks if the error is a conflict.
     * 
     * @function
     * @instance
     * @memberof PushError
     * 
     * @returns {boolean} true if the error is a conflict. False, otherwise.
     */
    function isConflict() {
        return statusCode === 409 || statusCode === 412;
    }
    
    /**
     * Cancels the push operation for the current record and updates the record in the local store.
     * This will also set {@link PushError#isHandled} to true.
     * 
     * @function
     * @instance
     * @memberof PushError
     * 
     * @param {object} newValue New value of the client record that will be updated in the local store.
     * 
     * @returns {Promise} A promise that is fulfilled when the operation is cancelled and the client record is updated.
     *                    The promise is rejected with the error if `cancelAndUpdate` fails.
     */
    function cancelAndUpdate(newValue) {
        var self = this;
        return storeTaskRunner.run(function() {

            if (pushOperation.logRecord.action === 'delete') {
                throw new Error('Cannot update a deleted record');
            }
            
            if (_.isNull(newValue)) {
                throw new Error('Need a valid object to update the record');
            }
            
            if (!_.isValidId(newValue.id)) {
                throw new Error('Invalid ID: ' + newValue.id);
            }
            
            if (newValue.id !== pushOperation.data.id) {
                throw new Error('Only updating the record being pushed is allowed');
            }
            
            // Operation to update the data record
            var dataUpdateOperation = {
                tableName: pushOperation.logRecord.tableName,
                action: 'upsert',
                data: newValue
            };
            
            // Operation to delete the log record
            var logDeleteOperation = {
                tableName: operationTableName,
                action: 'delete',
                id: pushOperation.logRecord.id
            };
            
            // Execute the log and data operations
            var operations = [dataUpdateOperation, logDeleteOperation];
            return store.executeBatch(operations).then(function() {
                self.isHandled = true;
            });
        });
    }
    
    /**
     * Cancels the push operation for the current record and discards the record from the local store.
     * This will also set {@link PushError#isHandled} to true.
     * 
     * @function
     * @instance
     * @memberof PushError
     * 
     * @returns {Promise} A promise that is fulfilled when the operation is cancelled and the client record is discarded
     *                    and rejected with the error if `cancelAndDiscard` fails.
     */
    function cancelAndDiscard() {
        var self = this;
        return storeTaskRunner.run(function() {
            
            // Operation to delete the data record
            var dataDeleteOperation = {
                tableName: pushOperation.logRecord.tableName,
                action: 'delete',
                id: pushOperation.logRecord.itemId
            };
            
            // Operation to delete the log record
            var logDeleteOperation = {
                tableName: operationTableName,
                action: 'delete',
                id: pushOperation.logRecord.id
            };
            
            // Execute the log and data operations
            var operations = [dataDeleteOperation, logDeleteOperation];
            return store.executeBatch(operations).then(function() {
                self.isHandled = true;
            });
        });
    }
    
    /**
     * Updates the client data record associated with the current operation.
     * If required, the metadata in the log record will also be associated.
     * This will also set {@link PushError#isHandled} to true.
     *
     * @function
     * @instance
     * @memberof PushError
     * 
     * @param {object} newValue New value of the data record. 
     * 
     * @returns {Promise} A promise that is fulfilled when the data record is updated in the localstore.
     */
    function update(newValue) {
        var self = this;
        return storeTaskRunner.run(function() {
            if (pushOperation.logRecord.action === 'delete') {
                throw new Error('Cannot update a deleted record');
            }
            
            if (_.isNull(newValue)) {
                throw new Error('Need a valid object to update the record');
            }
            
            if (!_.isValidId(newValue.id)) {
                throw new Error('Invalid ID: ' + newValue.id);
            }
            
            if (newValue.id !== pushOperation.data.id) {
                throw new Error('Only updating the record being pushed is allowed');
            }

            //TODO: Do we need to disallow updating record if the record has been deleted after
            //we attempted push?

            return operationTableManager.getMetadata(pushOperation.logRecord.tableName, 'upsert', newValue).then(function(metadata) {
                pushOperation.logRecord.metadata = metadata;
                return store.executeBatch([
                    { // Update the log record
                        tableName: operationTableName,
                        action: 'upsert',
                        data: pushOperation.logRecord
                    },
                    { // Update the record in the local table
                        tableName: pushOperation.logRecord.tableName,
                        action: 'upsert',
                        data: newValue
                    }
                ]).then(function() {
                    self.isHandled = this;
                });
            });

        });
    }
    
    /**
     * Changes the type of operation that will be pushed to the server.
     * This is useful for handling conflicts where you might need to change the type of the 
     * operation to be able to push the changes to the server.
     * This will also set {@link PushError#isHandled} to true.
     *
     * Example: You might need to change _'insert'_ to _'update'_ to be able to push a record that 
     * was already inserted on the server.
     * 
     * Note: Changing the action to _'delete'_' will automatically remove the associated record from the 
     * data table in the local store.
     * 
     * @function
     * @instance
     * @memberof PushError
     * 
     * @param {string} newAction New type of the operation. Valid values are _'insert'_, _'update'_ and _'delete'_.
     * @param {object} [newClientRecord] New value of the client record. 
     *                          The `id` property of the new record should match the `id` property of the original record.
     *                          If `newAction` is _'delete'_, only the `id` and `version` properties will be read from `newClientRecord`.
     *                          Reading the `version` property while deleting is useful if
     *                          the conflict handler changes an _'insert'_  /_'update'_ action to _'delete'_' and also updated the version
     *                          to reflect the server version.
     * 
     * @returns {Promise} A promise that is fulfilled when the action is changed and, optionally, the data record is updated / deleted.
     *                    If this fails, the promsie is rejected with the error.
     */
    function changeAction(newAction, newClientRecord) {
        var self = this;
        return storeTaskRunner.run(function() {
            var dataOperation, // operation to edit the data record
                logOperation = { // operation to edit the log record 
                    tableName: operationTableName,
                    action: 'upsert',
                    data: makeCopy(pushOperation.logRecord)
                };

            // If a new value for the record is specified, use the version property to update the metadata
            // If not, there is nothing that needs to be changed in the metadata. Just use the metadata we already have.
            if (newClientRecord) {
                if (!newClientRecord.id) {
                    throw new Error('New client record value must specify the record ID');
                }
                    
                if (newClientRecord.id !== pushOperation.logRecord.itemId) {
                    throw new Error('New client record value cannot change the record ID. Original ID: ' +
                                    pushOperation.logRecord.id + ' New ID: ' + newClientRecord.id);
                }

                // FYI: logOperation.data and pushOperation.data are not the same thing!
                logOperation.data.metadata = logOperation.data.metadata || {};
                logOperation.data.metadata[tableConstants.sysProps.versionColumnName] = newClientRecord[tableConstants.sysProps.versionColumnName];
            }

            if (newAction === 'insert' || newAction === 'update') {
                
                // Change the action as specified
                var oldAction = logOperation.data.action;
                logOperation.data.action = newAction;

                // Update the client record, if a new value is specified
                if (newClientRecord) {
                    
                    dataOperation = {
                        tableName: pushOperation.logRecord.tableName,
                        action: 'upsert',
                        data: newClientRecord
                    };
                    
                } else if (oldAction !== 'insert' && oldAction !== 'update') {

                    // If we are here, it means we are changing the action from delete to insert / update. 
                    // In such a case we expect newClientRecord to be non-null as we won't otherwise know what to insert / update.
                    // Example: changing delete to insert without specifying a newClientRecord is meaningless.
                    throw new Error('Changing action from ' + oldAction + ' to ' + newAction +
                                    ' without specifying a value for the associated record is not allowed!');
                }
                
            } else if (newAction === 'delete' || newAction === 'del') {

                // Change the action to 'delete'
                logOperation.data.action = 'delete';

                // Delete the client record as the new action is 'delete'
                dataOperation = {
                    tableName: pushOperation.logRecord.tableName,
                    action: 'delete',
                    id: pushOperation.logRecord.id
                };

            } else {
                throw new Error('Action ' + newAction + ' not supported.');
            }
            
            // Execute the log and data operations
            return store.executeBatch([logOperation, dataOperation]).then(function() {
                self.isHandled = true;
            });
        });
    }
    
    /**
     * Cancels pushing the current operation to the server permanently.
     * This will also set {@link PushError#isHandled} to true.
     * 
     * This method simply removes the pending operation from the operation table, thereby 
     * permanently skipping the associated change from being pushed to the server. A future change 
     * done to the same record will not be affected and such changes will continue to be pushed.
     *  
     * @function
     * @instance
     * @memberof PushError
     *
     * @returns {Promise} A promise that is fulfilled when the operation is cancelled OR rejected
     *                    with the error if it fails to cancel it.
     */
    function cancel() {
        var self = this;
        return storeTaskRunner.run(function() {
            return store.del(operationTableName, pushOperation.logRecord.id).then(function() {
                self.isHandled = true;
            });
        });
    }
}

function makeCopy(value) {
    const dateFormat = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{3}Z$/;
    if (!_.isNull(value)) {
        value = JSON.parse(JSON.stringify(value), function(key, value) {
            if (typeof value === "string" && dateFormat.test(value)) {
                return new Date(value);
            }
            return value;
        });
    }
    return value;
}

/**
 * Attempts error handling by delegating it to the user, if a push handler is provided
 * @private
 */
function handlePushError(pushError, pushHandler) {
    return Platform.async(function(callback) {
        callback();
    })().then(function() {
        
        if (pushError.isConflict()) {
            if (pushHandler && pushHandler.onConflict) {
                // NOTE: value of server record will not be available in case of 409.
                return pushHandler.onConflict(pushError);
            }
        } else if (pushHandler && pushHandler.onError) {
            return pushHandler.onError(pushError);
        }

    }).then(undefined, function(error) {
        // Set isHandled to false even if the user has set it to handled if the onConflict / onError failed 
        pushError.isHandled = false;
    });
}

exports.createPushError = createPushError;
exports.handlePushError = handlePushError;

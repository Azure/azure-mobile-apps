// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var Validate = require('../Utilities/Validate'),
    Platform = require('../Platform'),
    createOperationTableManager = require('./operations').createOperationTableManager,
    taskRunner = require('../Utilities/taskRunner'),
    createPullManager = require('./pull').createPullManager,
    createPushManager = require('./push').createPushManager,
    createPurgeManager = require('./purge').createPurgeManager,
    uuid = require('uuid'),
    _ = require('../Utilities/Extensions');


// NOTE: The store can be a custom store provided by the user code.
// So we do parameter validation ourselves without delegating it to the
// store, even where it is possible.

/**
 * Settings for configuring the pull behavior
 * @typedef MobileServiceSyncContext.PullSettings
 * @property {number} pageSize Specifies the number of records to request as part of a page pulled from the server tables.
 */

/**
 * Callback to delegate conflict handling to the user while pushing changes to the server. The conflict handler should
 * update the isHandled property of the pushConflict parameter appropriately on completion. If a conflict is marked as handled,
 * the push logic will attempt to push the change again. If not, the push logic will note the conflict, **skip** the change and
 * proceed to push the next change.
 * @callback MobileServiceSyncContext.ConflictHandler
 * @param {PushError} pushConflict Push conflict. 
 * @returns {Promise | undefined} This method can either return synchronously or can return a promise that is resolved 
 *                           or rejected when conflict handling completes / fails.
 */

/**
 * Callback to delegate error handling to the user while pushing changes to the server. Note that an error is a failure other
 * than a conflict. The error handler should update the isHandled property of the pushError parameter appropriately on completion.
 * If a conflict is marked as handled, the push logic will attempt to push the change again.
 * If not, the push logic will **abort** the push operation without attempting to push the remaining changes.
 * @callback MobileServiceSyncContext.ErrorHandler
 * @param {PushError} pushError Push Error.
 * @returns {Promise | undefined} This method can either return synchronously or can return a promise that is resolved 
 *                           or rejected when conflict handling completes / fails.
 */

/**
 * Defines callbacks for performing conflict and error handling.
 * @typedef {object} MobileServiceSyncContext.PushHandler
 * @property {MobileServiceSyncContext.ConflictHandler} onConflict Callback for delegating conflict handling to the user.
 * @property {MobileServiceSyncContext.ErrorHandler} onError Callback for delegating error handling to the user.
 */

/**
 * @class
 * @classdesc Context for local store operations.
 * @protected
 * 
 * @param {MobileServiceClient} client The {@link MobileServiceClient} instance to be used to make 
 *                                     requests to the backend (server).
 */
function MobileServiceSyncContext(client) {

    Validate.notNull(client, 'client');
    
    var store,
        operationTableManager,
        pullManager,
        pushManager,
        purgeManager,
        isInitialized = false,
        syncTaskRunner = taskRunner(), // Used to run push / pull tasks
        storeTaskRunner = taskRunner(); // Used to run insert / update / delete tasks on the store

    /**
     * Initializes the {@link MobileServiceSyncContext} instance. Initailizing an initialized instance of 
     * {@link MobileServiceSyncContext} will have no effect.
     * 
     * @param {MobileServiceStore} localStore An intitialized instance of the {@link MobileServiceStore local store} to be associated 
     *                                        with the {@link MobileServiceSyncContext} instance.
     * 
     * @returns {Promise} A promise that is resolved when the initialization is completed successfully.
      *                    If initialization fails, the promise is rejected with the error.
     */
    this.initialize = function (localStore) {
        
        return Platform.async(function(callback) {
            Validate.isObject(localStore);
            Validate.notNull(localStore);
            
            callback(null, createOperationTableManager(localStore));
        })().then(function(opManager) {
            operationTableManager = opManager;
            return operationTableManager.initialize(localStore);
        }).then(function() {
            store = localStore;
            pullManager = createPullManager(client, store, storeTaskRunner, operationTableManager);
            pushManager = createPushManager(client, store, storeTaskRunner, operationTableManager);
            purgeManager = createPurgeManager(store, storeTaskRunner);
        }).then(function() {
            return pullManager.initialize();
        }).then(function() {
            isInitialized = true;
        });
        
    };

    /**
     * Inserts a new object / record into the specified local table.
     * If the inserted object does not specify an id, a GUID will be used as the id.
     * 
     * @param {string} tableName Name of the local table in which the object / record is to be inserted.
     * @param {object} instance The object / record to be inserted into the local table.
     * @param {string} instance.id The id of the record. If this is null / undefined, a GUID string
     *                             will be used as the id.
     * 
     * @returns {Promise} A promise that is resolved with the inserted object when the insert operation is completed successfully.
     *                    If the operation fails, the promise is rejected with the error.
     */
    this.insert = function (tableName, instance) { //TODO: add an insert method to the store
        return storeTaskRunner.run(function() {
            validateInitialization();
            
            // Generate an id if it is not set already 
            if (_.isNull(instance.id)) {
                instance.id = uuid.v4();
            }

            // Delegate parameter validation to upsertWithLogging
            return upsertWithLogging(tableName, instance, 'insert');
        });
    };

    /**
     * Update an object / record in the specified local table.
     * The id of the object / record identifies the record that will be updated in the table.
     * 
     * @param {string} tableName Name of the local table in which the object / record is to be updated.
     * @param {object} instance New value of the object / record to be updated.
     * @param {string} instance.id The id of the object / record identifies the record that will be updated in the table.
     * 
     * @returns {Promise} A promise that is resolved when the operation is completed successfully. 
     *                    If the operation fails, the promise is rejected.
     */
    this.update = function (tableName, instance) { //TODO: add an update method to the store
        return storeTaskRunner.run(function() {
            validateInitialization();
            
            // Delegate parameter validation to upsertWithLogging
            return upsertWithLogging(tableName, instance, 'update', true /* shouldOverwrite */);
        });
    };

    /**
     * Looks up an object / record from the specified local table using the object id.
     * 
     * @param {string} tableName Name of the local table in which to look up the object / record.
     * @param {string} id id of the object to be looked up in the local table.
     * @param {boolean} [suppressRecordNotFoundError] If set to true, lookup will return an undefined object
     *                                                if the record is not found. Otherwise, lookup will fail.
     *                                                This flag is useful to distinguish between a lookup
     *                                                failure due to the record not being present in the table
     *                                                versus a genuine failure in performing the lookup operation.
     * 
     * @returns {Promise} A promise that is resolved with the looked up object when the lookup is completed successfully.
     *                    If the operation fails, the promise is rejected with the error.
     */
    this.lookup = function (tableName, id, suppressRecordNotFoundError) {
        
        return Platform.async(function(callback) {
            validateInitialization();
            
            Validate.isString(tableName, 'tableName');
            Validate.notNullOrEmpty(tableName, 'tableName');

            Validate.isValidId(id, 'id');

            if (!store) {
                throw new Error('MobileServiceSyncContext not initialized');
            }
            
            callback();
        })().then(function() {
            return store.lookup(tableName, id, suppressRecordNotFoundError);
        });
    };


    /**
     * Reads records from the specified local table.
     * 
     * @param {QueryJs} query A {@link QueryJs} object representing the query to use while
     *                        reading the local table
     * @returns {Promise} A promise that is resolved with an array of records read from the table, if the read is successful.
     *                    If read fails, the promise is rejected with the error.
     */
    this.read = function (query) {
        
        return Platform.async(function(callback) {
            callback();
        })().then(function() {
            validateInitialization();

            Validate.notNull(query, 'query');
            Validate.isObject(query, 'query');

            return store.read(query);
        });
    };


    /**
     * Deletes an object / record from the specified local table.
     * 
     * @param {string} tableName Name of the local table to delete the object from.
     * @param {object} instance The object to delete from the local table. 
     * @param {string} instance.id id of the record to be deleted.
     * 
     * @returns {Promise} A promise that is resolved when the delete operation completes successfully.
     *                    If the operation fails, the promise is rejected with the error.
     */
    this.del = function (tableName, instance) {
        
        return storeTaskRunner.run(function() {
            validateInitialization();
            
            Validate.isString(tableName, 'tableName');
            Validate.notNullOrEmpty(tableName, 'tableName');

            Validate.notNull(instance);
            Validate.isValidId(instance.id);

            if (!store) {
                throw new Error('MobileServiceSyncContext not initialized');
            }

            return operationTableManager.getLoggingOperation(tableName, 'delete', instance).then(function(loggingOperation) {
                return store.executeBatch([
                    {
                        action: 'delete',
                        tableName: tableName,
                        id: instance.id
                    },
                    loggingOperation
                ]);
            });
        });
    };
    
    /**
     * Pulls changes from server table into the local store.
     * 
     * @param {QueryJs} query Query specifying which records to pull.
     * @param {string} queryId A unique string id for an incremental pull query. A null / undefined queryId 
     *                         will perform a vanilla pull, i.e. will pull all the records specified by the table
     *                         from the server 
     * @param {MobileServiceSyncContext.PullSettings} [settings] An object that defines various pull settings. 
     * 
     * @returns {Promise} A promise that is fulfilled when all records are pulled OR is rejected if the pull fails or is cancelled.  
     */
    this.pull = function (query, queryId, settings) { 
        //TODO: Implement cancel
        //TODO: Perform push before pulling
        return syncTaskRunner.run(function() {
            validateInitialization();
            
            return pullManager.pull(query, queryId, settings);
        });
    };
    
    /**
     * Pushes local changes to the corresponding tables on the server.
     * 
     * Conflict and error handling are delegated to {@link MobileServiceSyncContext#pushHandler}.
     * 
     * @returns {Promise} A promise that is fulfilled with an array of encountered conflicts when all changes
     *                    are pushed to the server without errors. Note that a conflict is not treated as an error.
     *                    The returned promise is rejected if the push fails.  
     */
    this.push = function () { //TODO: Implement cancel
        return syncTaskRunner.run(function() {
            validateInitialization();

            return pushManager.push(this.pushHandler);
        }.bind(this));
    };
    
    /**
     * Purges data from the local table as well as pending operations and any incremental sync state 
     * associated with the table.
     * 
     * A _regular purge_, would fail if there are any pending operations for the table being purged.
     * 
     * A _forced purge_ will proceed even if pending operations for the table being purged exist in the operation table. In addition,
     * it will also delete the table's pending operations.
     * 
     * @param {QueryJs} query A {@link QueryJs} object representing the query that specifies what records are to be purged.
     * @param {boolean} forcePurge If set to true, the method will perform a forced purge.
     * 
     * @returns {Promise} A promise that is fulfilled when purge is complete OR is rejected if it fails.  
     */
    this.purge = function (query, forcePurge) {
        return syncTaskRunner.run(function() {
            Validate.isObject(query, 'query');
            Validate.notNull(query, 'query');
            if (!_.isNull(forcePurge)) {
                Validate.isBool(forcePurge, 'forcePurge');
            }

            validateInitialization();

            return purgeManager.purge(query, forcePurge);
        }.bind(this));
    };

    /**
     * @property {MobileServiceSyncContext.PushHandler} pushHandler Defines push handler.
     */
    this.pushHandler = undefined;

    // Unit test purposes only
    this._getOperationTableManager = function () {
        return operationTableManager;
    };
    this._getPurgeManager = function() {
        return purgeManager;
    };
    
    // Performs upsert and logs the action in the operation table
    // Validates parameters. Callers can skip validation
    function upsertWithLogging(tableName, instance, action, shouldOverwrite) {
        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');

        Validate.notNull(instance, 'instance');
        Validate.isValidId(instance.id, 'instance.id');
        
        if (!store) {
            throw new Error('MobileServiceSyncContext not initialized');
        }
        
        return store.lookup(tableName, instance.id, true /* suppressRecordNotFoundError */).then(function(existingRecord) {
            if (existingRecord && !shouldOverwrite) {
                throw new Error('Record with id ' + existingRecord.id + ' already exists in the table ' + tableName);
            }
        }).then(function() {
            return operationTableManager.getLoggingOperation(tableName, action, instance);
        }).then(function(loggingOperation) {
            return store.executeBatch([
                {
                    action: 'upsert',
                    tableName: tableName,
                    data: instance
                },
                loggingOperation
            ]);
        }).then(function() {
            return instance;
        });
    }

    // Throws an error if the sync context is not initialized
    function validateInitialization() {
        if (!isInitialized) {
            throw new Error ('MobileServiceSyncContext is being used before it is initialized');
        }
    }
}

module.exports = MobileServiceSyncContext;

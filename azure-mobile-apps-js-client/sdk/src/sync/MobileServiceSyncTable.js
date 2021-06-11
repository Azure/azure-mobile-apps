// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var Validate = require('../Utilities/Validate'),
    Query = require('azure-query-js').Query,
    _ = require('../Utilities/Extensions'),
    tableHelper = require('../tableHelper'),
    Platform = require('../Platform');

/**
 * @class
 * @classdesc Represents a table in the local store.
 * @protected
 * 
 * @param {string} tableName Name of the local table.
 * @param {MobileServiceClient} client The {@link MobileServiceClient} instance associated with this table.
 */
function MobileServiceSyncTable(tableName, client) {
    Validate.isString(tableName, 'tableName');
    Validate.notNullOrEmpty(tableName, 'tableName');

    Validate.notNull(client, 'client');

    /**
     * Gets the name of the local table.
     * 
     * @returns {string} The name of the table.
     */
    this.getTableName = function () {
        return tableName;
    };

    /**
     * Gets the {@link MobileServiceClient} instance associated with this table.
     * 
     * @returns {MobileServiceClient} The {@link MobileServiceClient} associated with this table.
     */
    this.getMobileServiceClient = function () {
        return client;
    };

    /**
     * Inserts a new object / record in the local table.
     * If the inserted object does not specify an id, a GUID will be used as the id.
     * 
     * @param {object} instance Object / record to be inserted in the local table.
     * @param {string} instance.id The id of the record. If this is null / undefined, a GUID string
     *                             will be used as the id.
     * @returns {Promise} A promise that is resolved with the inserted object when the insert operation is completed successfully.
     *                    If the operation fails, the promise is rejected with the error.
     */
    this.insert = function (instance) {
        return client.getSyncContext().insert(tableName, instance);
    };

    /**
     * Update an object / record in the local table.
     * 
     * @param {object} instance New value of the object / record.
     * @param {string} instance.id The id of the object / record identifies the record that will be updated in the table.
     * 
     * @returns {Promise} A promise that is resolved when the operation is completed successfully.
     *                    If the operation fails, the promise is rejected with the error.
     */
    this.update = function (instance) {
        return client.getSyncContext().update(tableName, instance);
    };

    /**
     * Looks up an object / record from the local table using the object id.
     * 
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
    this.lookup = function (id, suppressRecordNotFoundError) {
        return client.getSyncContext().lookup(tableName, id, suppressRecordNotFoundError);
    };

    /**
     * Reads records from the local table.
     * 
     * @param {QueryJs} query A {@link QueryJs} object representing the query to use while
     *                        reading the local table
     * @returns {Promise} A promise that is resolved with an array of records read from the table, if the read is successful.
     *                    If read fails, the promise is rejected with the error.
     */
    this.read = function (query) {
        if (_.isNull(query)) {
            query = new Query(tableName);
        }
        
        return client.getSyncContext().read(query);
    };

    /**
     * Deletes an object / record from the local table.
     * 
     * @param {object} instance The object to delete from the local table. 
     * @param {string} instance.id id of the record to be deleted.
     * 
     * @returns {Promise} A promise that is resolved when the delete operation completes successfully.
     *                    If the operation fails, the promise is rejected with the error.
     */
    this.del = function (instance) {
        return client.getSyncContext().del(tableName, instance);
    };

    /**
     * Pulls changes from server table into the local table.
     * 
     * @param {QueryJs} query Query specifying which records to pull.
     * @param {string} queryId A unique string id for an incremental pull query. A null / undefined queryId 
     *                         will perform a vanilla pull, i.e. will pull all the records specified by the table
     *                         from the server 
     * @param {MobileServiceSyncContext.PullSettings} [settings] An object that defines various pull settings. 
     * 
     * @returns {Promise} A promise that is fulfilled when all records are pulled OR is rejected  with the error if pull fails.  
     */
    this.pull = function (query, queryId, settings) {
        if (!query) {
            query = new Query(tableName);
        }
        
        return client.getSyncContext().pull(query, queryId, settings);
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
     * @returns {Promise} A promise that is fulfilled when purge is complete OR is rejected with the error if it fails.  
     */
    this.purge = function (query, forcePurge) {
        if (!query) {
            query = new Query(tableName);
        }

        return client.getSyncContext().purge(query, forcePurge);
    };
}

// Define query operators
tableHelper.defineQueryOperators(MobileServiceSyncTable);

exports.MobileServiceSyncTable = MobileServiceSyncTable;

// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @interface MobileServiceStore
 * @description Represents a local store.
 */

/**
 * Defines schema of a table in the local store.
 * If a table with the same name already exists, the newly defined columns in the table definition will be added to the table.
 * If no table with the same name exists, a table with the specified schema will be created.  
 *
 * @function defineTable
 * @instance
 * @memberof MobileServiceStore
 * 
 * @param {object} tableDefinition An object that defines the table schema, i.e. the table name and columns.
 * @param {string} tableDefinition.name Table name.
 * @param {object} tableDefinition.columnDefinitions Column definitions with column name as key and column type as value. Valid
 *                                                   column types are _'object'_, _'array'_, _'integer'_ OR _'int'_, _'float'_ OR _'real'_,
 *                                                   _'string'_ OR _'text'_, _'boolean'_ OR _'bool'_ and _'date'_.
 *
 * @returns {Promise} A promise that is resolved when the operation is completed successfully OR rejected with the error if it fails.
 *
 * @example
 * store.defineTable({
 *     name: "todoItemTable",
 *     columnDefinitions : {
 *         id : "string",
 *         metadata : MobileServiceSqliteStore.ColumnType.Object,
 *         description : "string",
 *         purchaseDate : "date",
 *         price : MobileServiceSqliteStore.ColumnType.Real
 *     }
 * })
 * .then(function() {
 *     // table definition successful.
 * }, function(error) {
 *     // table definition failed. handle error.
 * });
 */

/**
 * Updates or inserts one or more objects / records in the local table.
 * If a property does not have a corresponding definition in tableDefinition, it will not be upserted into the table.
 * 
 * @function upsert
 * @instance
 * @memberof MobileServiceStore
 * 
 * @param {string} tableName Name of the local table in which data is to be upserted.
 * @param {object | object[]} data A single object / record OR an array of objects / records to be inserted/updated in the table.
 * 
 * @returns A promise that is resolved when the operation is completed successfully OR rejected with the error if it fails.
 */


/**
 * Perform an object / record lookup in the local table.
 *
 * @function lookup
 * @instance
 * @memberof MobileServiceStore
 *  
 * @param {string} tableName Name of the local table in which lookup is to be performed.
 * @param {string} id id of the object to be looked up.
 * @param {boolean} [suppressRecordNotFoundError] If set to true, lookup will return an undefined object if the record is not found.
 *                                                Otherwise, lookup will fail. 
 *                                                This flag is useful to distinguish between a lookup failure due to the record not being present in the table
 *                                                versus a genuine failure in performing the lookup operation.
 * 
 * @returns {Promise} A promise that will be resolved with the looked up object when the operation completes successfully OR 
 *                    rejected with the error if it fails. 
 */

/**
 * Deletes one or more records from the local table
 * 
 * @function del
 * @instance
 * @memberof MobileServiceStore
 * 
 * @param {string | QueryJs} tableNameOrQuery Either the name of the local table in which delete is to be performed,
 *                           or a {@link QueryJs} object defining records to be deleted.
 * @param {string | string[]} ids A single ID or an array of IDs of records to be deleted.
 *                         This argument is expected only if tableNameOrQuery is table name and not a {@link QueryJs} object.
 * 
 * @returns {Promise} A promise that is resolved when the operation completes successfully or rejected with the error if it fails.
 */

/**
 * Read records from a local table.
 * 
 * @function read
 * @instance
 * @memberof MobileServiceStore
 * 
 * @param {QueryJs} query A QueryJs object representing the query to use while reading the table.
 * @returns {Promise} A promise that is resolved with an array of records read when the operation is completed successfully or rejected with
 *                    the error if it fails.
 */

/**
 * Executes the specified operations as part of a single transaction.
 * 
 * @function executeBatch
 * @instance
 * @memberof MobileServiceStore
 * @private
 * 
 * @param {object[]} operations Array of operations to be performed.
 * @param {string} operations[].action Action to be performed. It is either 'upsert' or 'delete'.
 * @param {string} operations[].tableName Name of the table.
 * @param {object} [operations[].data] Record / Object to be upserted. This property is needed only if action is 'upsert'.
 * @param {string} [operations[].id] id of the record to be deleted. This property is needed only if id is 'delete'.
 *   
 * @returns A promise that is resolved when the operations are completed successfully OR rejected with the error if they fail.
 * @example
 * {
 *      action: 'upsert',
 *      tableName: name of the table,
 *      data: record / object to be upserted
 * }
 * 
 * OR
 * 
 * {
 *      action: 'delete',
 *      tableName: name of the table,
 *      id: ID of the record to be deleted
 * }
 * 
 */

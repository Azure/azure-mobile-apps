// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file Helper functions for performing store related operations
 * @private
 */

var idPropertyName = require('../../constants').table.idPropertyName,
    Validate = require('../../Utilities/Validate');

/**
 * Validates the table definition
 * @private
 */
function validateTableDefinition(tableDefinition) {
    // Do basic table name validation and leave the rest to the store
    Validate.notNull(tableDefinition, 'tableDefinition');
    Validate.isObject(tableDefinition, 'tableDefinition');

    Validate.isString(tableDefinition.name, 'tableDefinition.name');
    Validate.notNullOrEmpty(tableDefinition.name, 'tableDefinition.name');

    // Validate the specified column types and check for duplicate columns
    var columnDefinitions = tableDefinition.columnDefinitions,
        definedColumns = {};
    
    Validate.isObject(columnDefinitions);
    Validate.notNull(columnDefinitions);

    for (var columnName in columnDefinitions) {

        Validate.isString(columnDefinitions[columnName], 'columnType');
        Validate.notNullOrEmpty(columnDefinitions[columnName], 'columnType');

        if (definedColumns[columnName.toLowerCase()]) {
            throw new Error('Duplicate definition for column ' + columnName + '" in table "' + tableDefinition.name + '"');
        }

        definedColumns[columnName.toLowerCase()] = true;
    }
}

/**
 * Adds a tableDefinition to the tableDefinitions object
 * @private
 */
function addTableDefinition(tableDefinitions, tableDefinition) {
    Validate.isObject(tableDefinitions);
    Validate.notNull(tableDefinitions);
    validateTableDefinition(tableDefinition);

    tableDefinitions[tableDefinition.name.toLowerCase()] = tableDefinition;
}

/**
 * Gets the table definition for the specified table name from the tableDefinitions object
 * @private
 */
function getTableDefinition(tableDefinitions, tableName) {
    Validate.isObject(tableDefinitions);
    Validate.notNull(tableDefinitions);
    Validate.isString(tableName);
    Validate.notNullOrEmpty(tableName);

    return tableDefinitions[tableName.toLowerCase()];
}

/**
 * Gets the type of the specified column
 * @private
 */
function getColumnType(columnDefinitions, columnName) {
    Validate.isObject(columnDefinitions);
    Validate.notNull(columnDefinitions);
    Validate.isString(columnName);
    Validate.notNullOrEmpty(columnName);

    for (var column in columnDefinitions) {
        if (column.toLowerCase() === columnName.toLowerCase()) {
            return columnDefinitions[column];
        }
    }
}

/**
 * Returns the column name in the column definitions that matches the specified property
 * @private
 */
function getColumnName(columnDefinitions, property) {
    Validate.isObject(columnDefinitions);
    Validate.notNull(columnDefinitions);
    Validate.isString(property);
    Validate.notNullOrEmpty(property);

    for (var column in columnDefinitions) {
        if (column.toLowerCase() === property.toLowerCase()) {
            return column;
        }
    }

    return property; // If no definition found for property, simply returns the column name as is
}

/**
 * Returns the Id property value OR undefined if none exists
 * @private
 */
function getId(record) {
    Validate.isObject(record);
    Validate.notNull(record);

    for (var property in record) {
        if (property.toLowerCase() === idPropertyName.toLowerCase()) {
            return record[property];
        }
    }
}

/**
 * Checks if property is an ID property.
 * @private
 */
function isId(property) {
    Validate.isString(property);
    Validate.notNullOrEmpty(property);

    return property.toLowerCase() === idPropertyName.toLowerCase();
}

module.exports = {
    addTableDefinition: addTableDefinition,
    getColumnName: getColumnName,
    getColumnType: getColumnType,
    getId: getId,
    getTableDefinition: getTableDefinition,
    isId: isId,
    validateTableDefinition: validateTableDefinition
};

// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file Defines functions for serializing a JS object into an object that can be used for storing in a SQLite table and
 *       for deserializing a row / object read from a SQLite table into a JS object. The target type of a serialization or
 *       a deserialization operation is determined by the specified column definition.
 * @private
 */

var Platform = require('.'),
    Validate = require('../../Utilities/Validate'),
    _ = require('../../Utilities/Extensions'),
    ColumnType = require('../../sync/ColumnType'),
    storeHelper = require('./storeHelper'),
    verror = require('verror'),
    typeConverter = require('./typeConverter');

/**
 * Gets the SQLite type that matches the specified ColumnType.
 * @private
 * @param columnType - The type of values that will be stored in the SQLite table
 * @throw Will throw an error if columnType is not supported 
 */
function getSqliteType (columnType) {
    var sqliteType;

    switch (columnType) {
        case ColumnType.Object:
        case ColumnType.Array:
        case ColumnType.String:
        case ColumnType.Text:
            sqliteType = "TEXT";
            break;
        case ColumnType.Integer:
        case ColumnType.Int:
        case ColumnType.Boolean:
        case ColumnType.Bool:
        case ColumnType.Date:
            sqliteType = "INTEGER";
            break;
        case ColumnType.Real:
        case ColumnType.Float:
            sqliteType = "REAL";
            break;
        default:
            throw new Error('Column type ' + columnType + ' is not supported');
    }

    return sqliteType;
}

/**
 * Checks if the value can be stored in a table column of the specified type.
 * Example: Float values can be stored in column of type ColumnType.Float but not ColumnType.Integer. 
 * @private
 */
function isJSValueCompatibleWithColumnType(value, columnType) {
    
    // Allow NULL values to be stored in columns of any type
    if (_.isNull(value)) {
        return true;
    }
    
    switch (columnType) {
        case ColumnType.Object:
            return _.isObject(value);
        case ColumnType.Array:
            return _.isArray(value);
        case ColumnType.String:
        case ColumnType.Text:
            return true; // Allow any value to be stored in a string column
        case ColumnType.Boolean:
        case ColumnType.Bool:
        case ColumnType.Integer:
        case ColumnType.Int:
            return _.isBool(value) || _.isInteger(value);
        case ColumnType.Date:
            return _.isDate(value);
        case ColumnType.Real:
        case ColumnType.Float:
            return _.isNumber(value);
        default:
            return false;
    }
}

/**
 * Checks if the SQLite value matches the specified ColumnType.
 * A value read from a SQLite table can be incompatible with the specified column type, if it was stored
 * in the table using a column type different from columnType.
 * Example: If a non-integer numeric value is stored in a column of type ColumnType.Float and 
 * then deserialized into a column of type ColumnType.Integer, that will be an error. 
 * @private
 */
function isSqliteValueCompatibleWithColumnType(value, columnType) {
    
    // Null is a valid value for any column type
    if (_.isNull(value)) {
        return true;
    }
    
    switch (columnType) {
        case ColumnType.Object:
            return _.isString(value);
        case ColumnType.Array:
            return _.isString(value);
        case ColumnType.String:
        case ColumnType.Text:
            return _.isString(value);
        case ColumnType.Boolean:
        case ColumnType.Bool:
            return _.isInteger(value);
        case ColumnType.Integer:
        case ColumnType.Int:
            return _.isInteger(value);
        case ColumnType.Date:
            return _.isInteger(value);
        case ColumnType.Real:
        case ColumnType.Float:
            return _.isNumber(value);
        default:
            return false;
    }
}

/**
 * Checks if type is a supported ColumnType
 * @private
 */
function isColumnTypeValid(type) {
    for (var key in ColumnType) {
        if (ColumnType[key] === type) {
            return true;
        }
    }
    return false;
}

/**
 * Serializes an object into an object that can be stored in a SQLite table, as defined by columnDefinitions.
 * @private
 */
function serialize (value, columnDefinitions) {

    var serializedValue = {};

    try {
        Validate.notNull(columnDefinitions, 'columnDefinitions');
        Validate.isObject(columnDefinitions);
        
        Validate.notNull(value);
        Validate.isObject(value);

        for (var property in value) {

            var columnType = storeHelper.getColumnType(columnDefinitions, property);
            // Skip properties that don't match any column in the table 
            if (!_.isNull(columnType)) {
                serializedValue[property] = serializeMember(value[property], columnType);
            }
        }
        
    } catch (error) {
        throw new verror.VError(error, 'Failed to serialize value ' + JSON.stringify(value) + '. Column definitions: ' + JSON.stringify(columnDefinitions));
    }

    return serializedValue;
}

/**
 * Deserializes a row read from a SQLite table into a Javascript object, as defined by columnDefinitions.
 * @private
 */
function deserialize (value, columnDefinitions) {

    var deserializedValue = {};
    
    try {
        Validate.notNull(columnDefinitions, 'columnDefinitions');
        Validate.isObject(columnDefinitions);

        Validate.notNull(value);
        Validate.isObject(value);

        for (var property in value) {
            var columnName = storeHelper.getColumnName(columnDefinitions, property); // this helps us deserialize with proper case for the property name
            deserializedValue[columnName] = deserializeMember(value[property], storeHelper.getColumnType(columnDefinitions, property));
        }
        
    } catch (error) {
        throw new verror.VError(error, 'Failed to deserialize value ' + JSON.stringify(value) + '. Column definitions: ' + JSON.stringify(columnDefinitions));
    }

    return deserializedValue;
}

/**
 * Serializes a property of an object into a value which can be stored in a SQLite column of type columnType. 
 * @private
 */
function serializeMember(value, columnType) {
    
    // Start by checking if the specified column type is valid
    if (!isColumnTypeValid(columnType)) {
        throw new Error('Column type ' + columnType + ' is not supported');
    }

    // Now check if the specified value can be stored in a column of type columnType
    if (!isJSValueCompatibleWithColumnType(value, columnType)) {
        throw new Error('Converting value ' + JSON.stringify(value) + ' of type ' + typeof value + ' to type ' + columnType + ' is not supported.');
    }

    // If we are here, it means we are good to proceed with serialization
    
    var sqliteType = getSqliteType(columnType),
        serializedValue;
    
    switch (sqliteType) {
        case "TEXT":
            serializedValue = typeConverter.convertToText(value);
            break;
        case "INTEGER":
            serializedValue = typeConverter.convertToInteger(value);
            break;
        case "REAL":
            serializedValue = typeConverter.convertToReal(value);
            break;
        default:
            throw new Error('Column type ' + columnType + ' is not supported');
    }
    
    return serializedValue;
}

// Deserializes a property of an object read from SQLite into a value of type columnType
function deserializeMember(value, columnType) {
    
    // Handle this special case first.
    // Simply return 'value' if a corresponding columnType is not defined.   
    if (!columnType) {
        return value;
    }

    // Start by checking if the specified column type is valid.
    if (!isColumnTypeValid(columnType)) {
            throw new Error('Column type ' + columnType + ' is not supported');
    }

    // Now check if the specified value can be stored in a column of type columnType.
    if (!isSqliteValueCompatibleWithColumnType(value, columnType)) {
        throw new Error('Converting value ' + JSON.stringify(value) + ' of type ' + typeof value +
                            ' to type ' + columnType + ' is not supported.');
    }

    // If we are here, it means we are good to proceed with deserialization
    
    var deserializedValue, error;

    switch (columnType) {
        case ColumnType.Object:
            deserializedValue = typeConverter.convertToObject(value);
            break;
        case ColumnType.Array:
            deserializedValue = typeConverter.convertToArray(value);
            break;
        case ColumnType.String:
        case ColumnType.Text:
            deserializedValue = typeConverter.convertToText(value);
            break;
        case ColumnType.Integer:
        case ColumnType.Int:
            deserializedValue = typeConverter.convertToInteger(value);
            break;
        case ColumnType.Boolean:
        case ColumnType.Bool:
            deserializedValue = typeConverter.convertToBoolean(value);
            break;
        case ColumnType.Date:
            deserializedValue = typeConverter.convertToDate(value);
            break;
        case ColumnType.Real:
        case ColumnType.Float:
            deserializedValue = typeConverter.convertToReal(value);
            break;
        default:
            throw new Error(_.format(Platform.getResourceString("sqliteSerializer_UnsupportedColumnType"), columnType));
    }

    return deserializedValue;
}

/**
 * Serializes a JS value to its equivalent that will be stored in the store.
 * This method is useful while querying to convert values to their store representations.
 * @private
 */
function serializeValue(value) {

    var type;
    if ( _.isNull(value) ) {
        type = ColumnType.Object;
    } else if ( _.isNumber(value) ) {
        type = ColumnType.Real;
    } else if ( _.isDate(value) ) {
        type = ColumnType.Date;
    } else if ( _.isBool(value) ) {
        type = ColumnType.Boolean;
    } else if ( _.isString(value) ) {
        type = ColumnType.String;
    } else if ( _.isArray(value) ) {
        type = ColumnType.Array;
    } else if ( _.isObject(value) ) {
        type = ColumnType.Object;
    } else {
        type = ColumnType.Object;
    }

    return serializeMember(value, type);
}

exports.serialize = serialize;
exports.serializeValue = serializeValue;
exports.deserialize = deserialize;
exports.getSqliteType = getSqliteType;

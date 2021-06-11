// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/***
 * @file Defines converters for various data types.
 */

var Validate = require('../../Utilities/Validate'),
    _ = require('../../Utilities/Extensions'),
    verror = require('verror');

exports.convertToText = function (value) {
    
    if (_.isNull(value)) // undefined/null value should be converted to null
        return null;

    if (_.isString(value)) {
        return value;
    }

    return JSON.stringify(value);
};

exports.convertToInteger = function (value) {

    if (_.isNull(value)) // undefined/null value should be converted to null
        return null;

    if (_.isInteger(value)) {
        return value;
    }

    if (_.isBool(value)) {
        return value ? 1 : 0;
    }
    
    if (_.isDate(value)) {
        return value.getTime();
    }

    throw new Error(_.format(Platform.getResourceString('sqliteSerializer_UnsupportedTypeConversion'), JSON.stringify(value), typeof value, 'integer'));
};

exports.convertToBoolean = function (value) {

    if (_.isNull(value)) // undefined/null value should be converted to null
        return null;

    if (_.isBool(value)) {
        return value;
    }

    if (_.isInteger(value)) {
        return value === 0 ? false : true;
    }
        
    throw new Error(_.format(Platform.getResourceString('sqliteSerializer_UnsupportedTypeConversion'), JSON.stringify(value), typeof value, 'Boolean'));
};

exports.convertToDate = function (value) {

    if (_.isNull(value)) // undefined/null value should be converted to null
        return null;

    if (_.isDate(value)) {
        return value;
    }

    if (_.isInteger(value)) {
        return new Date(value);
    } 

    throw new Error(_.format(Platform.getResourceString('sqliteSerializer_UnsupportedTypeConversion'), JSON.stringify(value), typeof value, 'Date'));
};

exports.convertToReal = function (value) {

    if (_.isNull(value)) // undefined/null value should be converted to null
        return null;

    if (_.isNumber(value)) {
        return value;
    }

    throw new Error(_.format(Platform.getResourceString('sqliteSerializer_UnsupportedTypeConversion'), JSON.stringify(value), typeof value, 'Real'));
};

exports.convertToObject = function (value) {

    if (_.isNull(value)) // undefined/null value should be converted to null
        return null;

    if (_.isObject(value)) {
        return value;
    }

    var error;
    try {
        if (_.isString(value)) {
            var result = JSON.parse(value);
            
            // Make sure the deserialized value is indeed an object, and not some other type
            if (_.isObject(result)) {
                return result;
            } 
        }
    } catch(err) {
        error = err; 
    }

    throw new verror.VError(error, Platform.getResourceString('sqliteSerializer_UnsupportedTypeConversion'), JSON.stringify(value), typeof value, 'Object');    
};

exports.convertToArray = function (value) {

    if (_.isNull(value)) // undefined/null value should be converted to null
        return null;

    if (_.isArray(value)) {
        return value;
    }

    var error;
    try {
        if (_.isString(value)) {
            var result = JSON.parse(value);
            
            // Make sure the deserialized value is indeed an object, and not some other type
            if (_.isArray(result)) {
                return result;
            } 
        }
    } catch(err) {
        error = err; 
    }

    throw new verror.VError(error, Platform.getResourceString('sqliteSerializer_UnsupportedTypeConversion'), JSON.stringify(value), typeof value, 'Array');    
};

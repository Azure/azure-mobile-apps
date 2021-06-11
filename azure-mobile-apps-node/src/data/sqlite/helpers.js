// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var types = require('../../utilities/types'),
    strings = require('../../utilities/strings');

module.exports = {
    mapParameterValue: function (value) {
        return types.isDate(value) ? value.toISOString() : value;
    },
    mapParameters: function (parameters) {
        return parameters.reduce(function (result, parameter) {
            result[parameter.name] = module.exports.mapParameterValue(parameter.value);
            return result;
        }, {});
    },

    // Performs the following validations on the specified identifier:
    // - first char is alphabetic or an underscore
    // - all other characters are alphanumeric or underscore
    // - the identifier is LTE 128 in length
    isValidIdentifier: function (identifier) {
        if (!identifier || !types.isString(identifier) || identifier.length > 128) {
            return false;
        }

        for (var i = 0; i < identifier.length; i++) {
            var char = identifier[i];
            if (i === 0) {
                if (!(strings.isLetter(char) || (char == '_'))) {
                    return false;
                }
            } else {
                if (!(strings.isLetter(char) || strings.isDigit(char) || (char == '_'))) {
                    return false;
                }
            }
        }

        return true;
    },

    validateIdentifier: function (identifier) {
        if (!this.isValidIdentifier(identifier)) {
            throw new Error(identifier + " is not a valid identifier. Identifiers must be under 128 characters in length, start with a letter or underscore, and can contain only alpha-numeric and underscore characters.");
        }
    },

    formatTableName: function (table) {
        var tableName = table.containerName || table.databaseTableName || table.name;
        this.validateIdentifier(tableName);
        return '[' + tableName + ']';
    },

    formatMember: function (memberName) {
        this.validateIdentifier(memberName);
        return '[' + memberName + ']';
    },

    getColumnTypeFromValue: function (value) {
        var type = value && value.constructor;
        if(!type || type === String || type === Number || type === Boolean || type === Date)
            return (value === undefined || value === null) ? 'unknown' : value.constructor.name.toLowerCase();
        throw new Error("Unsupported type: " + type.name);
    },

    getSqlTypeFromColumnType: function (value, primaryKey) {
        switch(value) {
            case 'string':
                return 'TEXT';
            case 'number':
                return primaryKey ? 'INTEGER' : 'REAL';
            case 'boolean':
            case 'bool':
                return 'INTEGER';
            case 'datetime':
            case 'date':
                return 'TEXT';
        }

        throw new Error('Unrecognised column type: ' + value);
    },

    getColumnTypeFromSqlType: function (value) {
        switch(value.toLowerCase()) {
            case 'text':
                return 'string';
            case 'real':
                return 'number';
            case 'integer':
                return 'boolean';
            default:
                return value.toLowerCase();
        }
    },

    getSystemPropertiesDDL: function (softDelete) {
        var columns = {
            version: "version TEXT NOT NULL DEFAULT 1",
            createdAt: "createdAt TEXT NOT NULL DEFAULT (STRFTIME('%Y-%m-%dT%H:%M:%fZ', 'now'))",
            updatedAt: "updatedAt TEXT NOT NULL DEFAULT (STRFTIME('%Y-%m-%dT%H:%M:%fZ', 'now'))"
        };
        if(softDelete) columns.deleted = "deleted INTEGER NOT NULL DEFAULT 0";
        return columns;
    },

    toBase64: function(value) {
        return (new Buffer(value)).toString("base64");
    },

    fromBase64: function(value) {
        return (new Buffer(value, "base64")).toString("ascii");
    }
};

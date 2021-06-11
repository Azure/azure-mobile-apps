// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var types = require('../../utilities/types'),
    strings = require('../../utilities/strings'),
    statementHelpers = require('./statements/helpers'),
    mssql = require('mssql');

var helpers = module.exports = {
    statements: statementHelpers,

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
        var schemaName = table.schema || 'dbo',
            tableName = table.containerName || table.databaseTableName || table.name;
            
        this.validateIdentifier(tableName);

        if (schemaName !== undefined) {
            schemaName = module.exports.formatSchemaName(schemaName);
            this.validateIdentifier(schemaName);
            return '[' + schemaName + '].[' + tableName + ']';
        }

        return '[' + tableName + ']';
    },

    formatSchemaName: function (appName) {
        // Hyphens are not supported in schema names
        return appName.replace(/-/g, '_');
    },

    formatMember: function (memberName) {
        this.validateIdentifier(memberName);
        return '[' + memberName + ']';
    },

    getSqlType: function (value, primaryKey) {
        if(value === undefined || value === null)
            throw new Error('Cannot create column for null or undefined value');

        switch (value.constructor) {
            case String:
                // 900 bytes is the maximum length for a primary key - http://stackoverflow.com/questions/10555642/varcharmax-column-not-allowed-to-be-a-primary-key-in-sql-server
                return primaryKey ? "NVARCHAR(255)" : "NVARCHAR(MAX)";
            case Number:
                return primaryKey ? "INT" : "FLOAT(53)";
            case Boolean:
                return "BIT";
            case Date:
                return "DATETIMEOFFSET(7)";
            default:
                throw new Error("Unable to map value " + value.toString() + " to a SQL type.");
        }
    },

    getMssqlType: function (value, primaryKey) {
        switch (value !== undefined && value !== null && value.constructor) {
            case String:
                return primaryKey ? mssql.NVarChar(255) : mssql.NVarChar();
            case Number:
                return primaryKey || isInteger(value) ? mssql.Int : mssql.Float;
            case Boolean:
                return mssql.Bit;
            case Date:
                return mssql.DateTimeOffset;
            case Buffer:
                return mssql.VarBinary;
        }

        function isInteger(value) {
            // integers larger than the maximum value get inserted as 1 - treat these as float parameters as a workaround
            return value.toFixed() === value.toString() && value < 2147483648 && value > -2147483648;
        }
    },

    getPredefinedColumnType: function (value) {
        switch(value) {
            case 'string':
                return 'NVARCHAR(MAX)';
            case 'number':
                return 'FLOAT(53)';
            case 'boolean':
            case 'bool':
                return 'BIT';
            case 'datetime':
            case 'date':
                return 'DATETIMEOFFSET(7)';
        }

        throw new Error('Unrecognised column type: ' + value);
    },

    getPredefinedType: function (value) {
        switch(value) {
            case 'nvarchar':
                return 'string';
            case 'float':
                return 'number';
            case 'bit':
                return 'boolean';
            case 'datetimeoffset':
                return 'datetime';
            case 'timestamp':
                return 'string';
            default:
                return value;
        }
    },

    getSystemPropertiesDDL: function (softDelete) {
        var columns = {
            version: 'version ROWVERSION NOT NULL',
            createdAt: 'createdAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0)',
            updatedAt: 'updatedAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0)'
        };
        if(softDelete) columns.deleted = 'deleted bit NOT NULL DEFAULT 0';
        return columns;
    },

    getSystemProperties: function () {
        return Object.keys(helpers.getSystemPropertiesDDL());
    },

    isSystemProperty: function (property) {
        return helpers.getSystemProperties().some(function (systemProperty) { return property === systemProperty; });
    },
};

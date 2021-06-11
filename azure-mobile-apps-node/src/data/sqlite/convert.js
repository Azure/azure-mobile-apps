// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('./helpers');

var convert = module.exports = {
    value: function (type, value) {
        if(value === undefined || value === null)
            return null;

        switch(type) {
            case 'boolean':
                return !!value;
            case 'date':
                return new Date(value);
            default:
                return value;
        }
    },
    item: function (columns, item) {
        return columns.reduce(function (result, column) {
            if(item.hasOwnProperty(column.name)) {
                if(column.name === 'version')
                    result[column.name] = helpers.toBase64(item[column.name]);
                else
                    result[column.name] = convert.value(column.type, item[column.name]);
            }
            return result;
        }, {});
    }
}

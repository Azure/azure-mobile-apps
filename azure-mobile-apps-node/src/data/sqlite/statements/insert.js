// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers'),
    transforms = require('./transforms'),
    util = require('util');

module.exports = function (table, item) {
    var tableName = helpers.formatTableName(table),
        columnNames = [],
        valueParams = [],
        parameters = {};

    Object.keys(item).forEach(function (property) {
        // ignore the property if it is an autoIncrement id
        if ((property !== 'id' || !table.autoIncrement) && item[property] !== undefined && item[property] !== null) {
            columnNames.push(helpers.formatMember(property));
            valueParams.push('@' + property);
            parameters[property] = helpers.mapParameterValue(item[property]);
        }
    });

    var sql = columnNames.length > 0
        ? util.format("INSERT INTO %s (%s) VALUES (%s);", tableName, columnNames.join(','), valueParams.join(','))
        : util.format("INSERT INTO %s DEFAULT VALUES;", tableName)

    return [{
        sql: sql,
        parameters: parameters
    }, {
        sql: util.format("SELECT * FROM %s WHERE [rowid] = last_insert_rowid();", tableName),
        transform: transforms.prepareItems(table)
    }];
}
